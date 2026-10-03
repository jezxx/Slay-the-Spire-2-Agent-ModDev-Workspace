using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.Core.Multiplayer.Messages.Game.Flavor;
using MegaCrit.Sts2.Core.Multiplayer.Messages.Game.Sync;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Multiplayer.Game;

public class RestSiteSynchronizer : IDisposable
{
	private class PlayerRestSite
	{
		public List<RestSiteOption> options;

		public uint? lastChosenOptionIndex;

		public uint? hoveredOptionIndex;

		public TaskCompletionSource completionTaskSource = new TaskCompletionSource();
	}

	public const int minHoverMessageMsec = 50;

	private readonly INetGameService _netService;

	private readonly RunLocationTargetedMessageBuffer _messageBuffer;

	private readonly IPlayerCollection _playerCollection;

	private readonly ulong _localPlayerId;

	private readonly RunLobby? _runLobby;

	private readonly List<PlayerRestSite> _restSites = new List<PlayerRestSite>();

	private readonly MegaCrit.Sts2.Core.Logging.Logger _logger = new MegaCrit.Sts2.Core.Logging.Logger("RestSiteSynchronizer", LogType.GameSync);

	private ulong _lastHoverMessageMsec;

	private RestSiteOptionHoveredMessage? _hoveredMessage;

	private Task? _hoverMessageTask;

	private Player LocalPlayer => _playerCollection.GetPlayer(_localPlayerId);

	public event Action<ulong>? PlayerHoverChanged;

	public event Action<RestSiteOption, ulong>? BeforePlayerOptionChosen;

	public event Action<RestSiteOption, bool, ulong>? AfterPlayerOptionChosen;

	public RestSiteSynchronizer(RunLocationTargetedMessageBuffer messageBuffer, INetGameService netService, IPlayerCollection playerCollection, ulong localPlayerId, RunLobby? runLobby)
	{
		_netService = netService;
		_messageBuffer = messageBuffer;
		_playerCollection = playerCollection;
		_localPlayerId = localPlayerId;
		_runLobby = runLobby;
		_messageBuffer.RegisterMessageHandler<OptionIndexChosenMessage>(HandleRestSiteOptionChosenMessage);
		_messageBuffer.RegisterMessageHandler<RestSiteOptionHoveredMessage>(HandleRestSiteOptionHoveredMessage);
		_messageBuffer.RegisterMessageHandler<RestSiteSkippedMessage>(HandleRestSiteSkippedMessage);
		if (_runLobby != null)
		{
			_runLobby.RemotePlayerDisconnected += OnPeerDisconnected;
		}
	}

	public void Dispose()
	{
		_messageBuffer.UnregisterMessageHandler<OptionIndexChosenMessage>(HandleRestSiteOptionChosenMessage);
		_messageBuffer.UnregisterMessageHandler<RestSiteOptionHoveredMessage>(HandleRestSiteOptionHoveredMessage);
		_messageBuffer.UnregisterMessageHandler<RestSiteSkippedMessage>(HandleRestSiteSkippedMessage);
		if (_runLobby != null)
		{
			_runLobby.RemotePlayerDisconnected -= OnPeerDisconnected;
		}
		_hoverMessageTask?.Dispose();
		_hoverMessageTask = null;
	}

	public void BeginRestSite()
	{
		_logger.Debug("Beginning rest site");
		_restSites.Clear();
		foreach (Player player in _playerCollection.Players)
		{
			List<RestSiteOption> list = RestSiteOption.Generate(player);
			_restSites.Add(new PlayerRestSite
			{
				options = list
			});
			_logger.VeryDebug($"Rest site began for player {player.NetId} with options: {string.Join(",", list)}");
		}
	}

	private void HandleRestSiteOptionChosenMessage(OptionIndexChosenMessage message, ulong senderId)
	{
		if (message.type == OptionIndexType.RestSite)
		{
			_logger.Debug($"Player {senderId} chose rest site option index {message.optionIndex}");
			Player player = _playerCollection.GetPlayer(senderId);
			if (player == null)
			{
				throw new InvalidOperationException($"Received EventOptionChosenMessage for player {senderId} that doesn't exist!");
			}
			TaskHelper.RunSafely(ChooseOption(player, (int)message.optionIndex));
		}
	}

	private void HandleRestSiteOptionHoveredMessage(RestSiteOptionHoveredMessage message, ulong senderId)
	{
		int playerSlotIndex = _playerCollection.GetPlayerSlotIndex(_playerCollection.GetPlayer(senderId));
		_restSites[playerSlotIndex].hoveredOptionIndex = message.optionIndex;
		this.PlayerHoverChanged?.Invoke(senderId);
	}

	private void HandleRestSiteSkippedMessage(RestSiteSkippedMessage message, ulong senderId)
	{
		_logger.Debug($"Player {senderId} skipped remaining options at rest site");
		int playerSlotIndex = _playerCollection.GetPlayerSlotIndex(_playerCollection.GetPlayer(senderId));
		PlayerRestSite playerRestSite = _restSites[playerSlotIndex];
		if (!playerRestSite.completionTaskSource.Task.IsCompleted)
		{
			_logger.Debug($"Skipping {playerRestSite.options.Count} options");
			playerRestSite.options.Clear();
			playerRestSite.completionTaskSource.TrySetResult();
		}
	}

	/// <summary>
	/// Called when a remote player disconnects. If they still had rest site options remaining, they will never send us a
	/// skip or final option-chosen message, so we complete their rest site here. Otherwise a room exit that is waiting
	/// on <see cref="M:MegaCrit.Sts2.Core.Multiplayer.Game.RestSiteSynchronizer.AfterAllRestSitesCompleted" /> would hang forever.
	/// </summary>
	private void OnPeerDisconnected(ulong peerId)
	{
		Player player = _playerCollection.GetPlayer(peerId);
		if (player == null)
		{
			return;
		}
		int playerSlotIndex = _playerCollection.GetPlayerSlotIndex(player);
		if (playerSlotIndex < _restSites.Count)
		{
			PlayerRestSite playerRestSite = _restSites[playerSlotIndex];
			if (!playerRestSite.completionTaskSource.Task.IsCompleted)
			{
				_logger.Debug($"Player {peerId} disconnected with {playerRestSite.options.Count} rest site options remaining; completing their rest site");
				playerRestSite.options.Clear();
				playerRestSite.completionTaskSource.SetResult();
			}
		}
	}

	public Task<bool> ChooseLocalOption(int index)
	{
		_logger.Debug($"Local player chose rest site option index {index}");
		OptionIndexChosenMessage message = default(OptionIndexChosenMessage);
		message.type = OptionIndexType.RestSite;
		message.optionIndex = (uint)index;
		message.location = _messageBuffer.CurrentLocation;
		_netService.SendMessage(message);
		return ChooseOption(LocalPlayer, index);
	}

	private async Task<bool> ChooseOption(Player player, int optionIndex)
	{
		int playerSlotIndex = _playerCollection.GetPlayerSlotIndex(player);
		PlayerRestSite restSite = _restSites[playerSlotIndex];
		if (restSite.completionTaskSource.Task.IsCompleted)
		{
			throw new InvalidOperationException($"Player {player.NetId} attempted to choose rest site option index {optionIndex}, but the rest site has already been completed!");
		}
		if (optionIndex >= restSite.options.Count)
		{
			throw new InvalidOperationException($"Player {player.NetId} attempted to choose rest site option index {optionIndex}, but there were only {restSite.options.Count} options available!");
		}
		RestSiteOption option = restSite.options[optionIndex];
		this.BeforePlayerOptionChosen?.Invoke(option, player.NetId);
		bool flag = await option.OnSelect();
		_logger.Debug($"Rest site option index {optionIndex} chosen for player {player.NetId} with success {flag}. Option: {option.OptionId}");
		restSite.lastChosenOptionIndex = (uint)optionIndex;
		this.AfterPlayerOptionChosen?.Invoke(option, flag, player.NetId);
		if (!flag)
		{
			return false;
		}
		player.RunState.CurrentMapPointHistoryEntry?.GetEntry(player.NetId).RestSiteChoices.Add(option.OptionId);
		if (restSite.completionTaskSource.Task.IsCompleted)
		{
			return true;
		}
		if (Hook.ShouldDisableRemainingRestSiteOptions(player.RunState, player))
		{
			_logger.Debug($"Clearing all remaining rest site options for player {player.NetId}");
			restSite.options.Clear();
		}
		else
		{
			_logger.Debug($"Leaving remaining rest site options enabled for player {player.NetId}");
			restSite.options.RemoveAt(optionIndex);
		}
		if (restSite.options.Count == 0)
		{
			_logger.Debug($"Completing rest site for player {player.NetId} because there are no options left");
			restSite.completionTaskSource.SetResult();
		}
		return true;
	}

	/// <summary>
	/// Called by <see cref="T:MegaCrit.Sts2.Core.Rooms.RestSiteRoom" /> when the room is exited.
	/// If there are still options remaining for the local player, those options are cleared and the room is completed.
	/// This completion is synced to other players.
	/// </summary>
	public void BeforeLocalRestSiteExited()
	{
		Player me = LocalContext.GetMe(_playerCollection);
		int playerSlotIndex = _playerCollection.GetPlayerSlotIndex(me);
		PlayerRestSite playerRestSite = _restSites[playerSlotIndex];
		if (playerRestSite.options.Count > 0)
		{
			_logger.Debug($"Skipping remaining options ({playerRestSite.options.Count}) in rest site for local player {me.NetId} because we're exiting the rest site");
			playerRestSite.options.Clear();
			playerRestSite.completionTaskSource.SetResult();
			_netService.SendMessage(new RestSiteSkippedMessage
			{
				Location = _messageBuffer.CurrentLocation
			});
		}
	}

	/// <summary>
	/// Waits for all players' rest sites to be completed.
	/// This must be awaited before the room-exit checksum is generated so that we know that we won't be receiving any
	/// more OptionIndexChosenMessages for this room.
	/// </summary>
	public async Task AfterAllRestSitesCompleted()
	{
		foreach (PlayerRestSite restSite in _restSites)
		{
			await restSite.completionTaskSource.Task;
		}
	}

	public void LocalOptionHovered(RestSiteOption? option)
	{
		int playerSlotIndex = _playerCollection.GetPlayerSlotIndex(LocalContext.GetMe(_playerCollection));
		PlayerRestSite playerRestSite = _restSites[playerSlotIndex];
		if (option != null)
		{
			playerRestSite.hoveredOptionIndex = (uint)playerRestSite.options.IndexOf(option);
		}
		else
		{
			playerRestSite.hoveredOptionIndex = null;
		}
		RestSiteOptionHoveredMessage valueOrDefault = _hoveredMessage.GetValueOrDefault();
		if (!_hoveredMessage.HasValue)
		{
			RestSiteOptionHoveredMessage restSiteOptionHoveredMessage = default(RestSiteOptionHoveredMessage);
			restSiteOptionHoveredMessage.Location = _messageBuffer.CurrentLocation;
			restSiteOptionHoveredMessage.optionIndex = playerRestSite.hoveredOptionIndex;
			valueOrDefault = restSiteOptionHoveredMessage;
			_hoveredMessage = valueOrDefault;
		}
		TrySendHoverMessage();
		this.PlayerHoverChanged?.Invoke(LocalContext.NetId.Value);
	}

	public int? GetHoveredOptionIndex(ulong playerId)
	{
		int playerSlotIndex = _playerCollection.GetPlayerSlotIndex(_playerCollection.GetPlayer(playerId));
		return (int?)_restSites[playerSlotIndex].hoveredOptionIndex;
	}

	public int? GetChosenOptionIndex(ulong playerId)
	{
		int playerSlotIndex = _playerCollection.GetPlayerSlotIndex(_playerCollection.GetPlayer(playerId));
		return (int?)_restSites[playerSlotIndex].lastChosenOptionIndex;
	}

	public IReadOnlyList<RestSiteOption> GetLocalOptions()
	{
		return GetOptionsForPlayer(LocalPlayer);
	}

	public IReadOnlyList<RestSiteOption> GetOptionsForPlayer(ulong playerId)
	{
		return GetOptionsForPlayer(_playerCollection.GetPlayer(playerId));
	}

	public IReadOnlyList<RestSiteOption> GetOptionsForPlayer(Player player)
	{
		int playerSlotIndex = _playerCollection.GetPlayerSlotIndex(player);
		return _restSites[playerSlotIndex].options;
	}

	/// <summary>
	/// Sends a hover message if enough time has passed since the last one, or buffers it to be sent otherwise.
	/// </summary>
	private void TrySendHoverMessage()
	{
		if (_hoverMessageTask == null)
		{
			int num = (int)(_lastHoverMessageMsec + 50 - Time.GetTicksMsec());
			if (num <= 0)
			{
				_hoverMessageTask = TaskHelper.RunSafely(SendHoverMessageAfterSmallDelay());
			}
			else
			{
				_hoverMessageTask = TaskHelper.RunSafely(QueueHoverMessage(num));
			}
		}
	}

	private async Task QueueHoverMessage(int delayMsec)
	{
		await Task.Delay(delayMsec);
		SendHoverMessage();
	}

	private async Task SendHoverMessageAfterSmallDelay()
	{
		await Task.Yield();
		SendHoverMessage();
	}

	private void SendHoverMessage()
	{
		if (_netService.IsConnected)
		{
			_netService.SendMessage(_hoveredMessage.Value);
			_lastHoverMessageMsec = Time.GetTicksMsec();
			_hoveredMessage = null;
			_hoverMessageTask = null;
		}
	}
}

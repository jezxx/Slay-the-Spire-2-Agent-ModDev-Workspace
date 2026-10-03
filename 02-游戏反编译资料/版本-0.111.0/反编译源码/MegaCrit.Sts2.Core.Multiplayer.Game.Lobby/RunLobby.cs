using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Messages.Game;
using MegaCrit.Sts2.Core.Multiplayer.Messages.Lobby;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;

/// <summary>
/// Class which handles disconnection and reconnection while a run is in progress.
/// <see cref="T:MegaCrit.Sts2.Core.Multiplayer.Game.Lobby.StartRunLobby" /> or <see cref="T:MegaCrit.Sts2.Core.Multiplayer.Game.Lobby.LoadRunLobby" /> are used to initialize the run. Once the run is in
/// progress, those instances go away and this class takes over their responsibilities.
/// Connections from players that are not currently in the run will be denied. Only players that existed in the run
/// may reconnect to the RunLobby.
/// </summary>
public class RunLobby
{
	private readonly Logger _logger;

	private readonly IPlayerCollection _playerCollection;

	private readonly IRunLobbyListener _lobbyListener;

	private readonly INetGameService _netService;

	/// <summary>
	/// <see cref="P:MegaCrit.Sts2.Core.Runs.RunState.Players" /> contains all players that have ever been connected to the session, i.e. it may
	/// contain disconnected players. This set contains only players who are currently connected to the session.
	/// </summary>
	public List<RunLobbyPlayer> Players { get; }

	public IEnumerable<ulong> PlayerIds => Players.Select((RunLobbyPlayer p) => p.id);

	/// <summary>
	/// Set to true if any player in the session was playing with mods.
	/// This cannot be derived from <see cref="F:MegaCrit.Sts2.Core.Entities.Multiplayer.RunLobbyPlayer.isModded" /> because errors that occur during
	/// disconnection are emitted after the player has already left, which means they've been removed from
	/// <see cref="P:MegaCrit.Sts2.Core.Multiplayer.Game.Lobby.RunLobby.Players" />.
	/// </summary>
	public bool AnyPlayerHadMods { get; }

	public GameMode GameMode { get; }

	public event Action<RunLobbyPlayer>? PlayerRejoined;

	public event Action<ulong>? RemotePlayerDisconnected;

	public event Action? LocalPlayerDisconnected;

	/// <summary>
	/// Creates the RunLobby.
	/// </summary>
	/// <param name="gameMode">The type of run that was started.</param>
	/// <param name="netService">Net service used to send/receive messages.</param>
	/// <param name="lobbyListener">The lobby listener which receives callbacks from this lobby.</param>
	/// <param name="playerCollection">Queryable collection of players.</param>
	/// <param name="players">The players in the game.</param>
	public RunLobby(GameMode gameMode, INetGameService netService, IRunLobbyListener lobbyListener, IPlayerCollection playerCollection, IEnumerable<RunLobbyPlayer> players)
	{
		GameMode = gameMode;
		_netService = netService;
		_lobbyListener = lobbyListener;
		_playerCollection = playerCollection;
		_logger = new Logger("RunLobby", LogType.Network);
		Players = players.ToList();
		AnyPlayerHadMods = Players.Any((RunLobbyPlayer p) => p.isModded);
		_netService.RegisterMessageHandler<ClientLobbyJoinRequestMessage>(HandleClientLobbyJoinRequestMessage);
		_netService.RegisterMessageHandler<ClientLoadJoinRequestMessage>(HandleClientLoadJoinRequestMessage);
		_netService.RegisterMessageHandler<ClientRejoinRequestMessage>(HandleClientRejoinRequestMessage);
		_netService.RegisterMessageHandler<PlayerRejoinedMessage>(HandlePlayerRejoinedMessage);
		_netService.RegisterMessageHandler<PlayerLeftMessage>(HandlePlayerLeftMessage);
		_netService.RegisterMessageHandler<RunAbandonedMessage>(HandleRunAbandonedMessage);
		_netService.Disconnected += OnDisconnected;
		if (_netService.Type == NetGameType.Host && netService is INetHostGameService netHostGameService)
		{
			netHostGameService.ClientConnected += OnConnectedToClientAsHost;
			netHostGameService.ClientDisconnected += OnDisconnectedFromClientAsHost;
		}
	}

	/// <summary>
	/// This should be called to cleanup the lobby before exiting the run.
	/// </summary>
	public void Dispose()
	{
		_netService.UnregisterMessageHandler<ClientLobbyJoinRequestMessage>(HandleClientLobbyJoinRequestMessage);
		_netService.UnregisterMessageHandler<ClientLoadJoinRequestMessage>(HandleClientLoadJoinRequestMessage);
		_netService.UnregisterMessageHandler<ClientRejoinRequestMessage>(HandleClientRejoinRequestMessage);
		_netService.UnregisterMessageHandler<PlayerRejoinedMessage>(HandlePlayerRejoinedMessage);
		_netService.UnregisterMessageHandler<PlayerLeftMessage>(HandlePlayerLeftMessage);
		_netService.UnregisterMessageHandler<RunAbandonedMessage>(HandleRunAbandonedMessage);
		_netService.Disconnected -= OnDisconnected;
		if (_netService.Type == NetGameType.Host && _netService is INetHostGameService netHostGameService)
		{
			netHostGameService.ClientConnected -= OnConnectedToClientAsHost;
			netHostGameService.ClientDisconnected -= OnDisconnectedFromClientAsHost;
		}
	}

	private void HandleClientRejoinRequestMessage(ClientRejoinRequestMessage message, ulong senderId)
	{
		if (_netService.Type != NetGameType.Host)
		{
			throw new InvalidOperationException("Received ClientRejoinRequestMessage as non-host!");
		}
		_logger.Info($"Received ClientRejoinRequestMessage for {senderId}");
		NetHostGameService netHostGameService = (NetHostGameService)_netService;
		if (_playerCollection.GetPlayer(senderId) == null)
		{
			netHostGameService.DisconnectClient(senderId, NetError.RunInProgress);
			return;
		}
		ClientRejoinResponseMessage rejoinMessage = _lobbyListener.GetRejoinMessage();
		netHostGameService.SendMessage(rejoinMessage, senderId);
		netHostGameService.SetPeerReadyForBroadcasting(senderId);
		RunLobbyPlayer runLobbyPlayer = default(RunLobbyPlayer);
		runLobbyPlayer.id = senderId;
		runLobbyPlayer.isModded = netHostGameService.GetVersionInfoForPeer(senderId).Value.IsModded();
		RunLobbyPlayer runLobbyPlayer2 = runLobbyPlayer;
		Players.Add(runLobbyPlayer2);
		PlayerRejoinedMessage playerRejoinedMessage = default(PlayerRejoinedMessage);
		playerRejoinedMessage.player = runLobbyPlayer2;
		PlayerRejoinedMessage message2 = playerRejoinedMessage;
		foreach (RunLobbyPlayer player in Players)
		{
			if (player.id != _netService.NetId && player.id != runLobbyPlayer2.id)
			{
				netHostGameService.SendMessage(message2, player.id);
			}
		}
		this.PlayerRejoined?.Invoke(runLobbyPlayer2);
	}

	private void HandleClientLobbyJoinRequestMessage(ClientLobbyJoinRequestMessage _, ulong senderId)
	{
		if (_netService.Type != NetGameType.Host)
		{
			throw new InvalidOperationException("Received ClientLobbyJoinRequestMessage as non-host!");
		}
		_logger.Info($"Received invalid ClientLobbyJoinRequestMessage for {senderId}");
		NetHostGameService netHostGameService = (NetHostGameService)_netService;
		netHostGameService.DisconnectClient(senderId, NetError.InvalidJoin);
	}

	private void HandleClientLoadJoinRequestMessage(ClientLoadJoinRequestMessage _, ulong senderId)
	{
		if (_netService.Type != NetGameType.Host)
		{
			throw new InvalidOperationException("Received ClientLoadJoinRequestMessage as non-host!");
		}
		_logger.Info($"Received invalid ClientLoadJoinRequestMessage for {senderId}");
		NetHostGameService netHostGameService = (NetHostGameService)_netService;
		netHostGameService.DisconnectClient(senderId, NetError.InvalidJoin);
	}

	private void HandlePlayerRejoinedMessage(PlayerRejoinedMessage message, ulong _)
	{
		_logger.Debug($"Received PlayerRejoinedMessage for {message.player.id}");
		Players.Add(message.player);
		this.PlayerRejoined?.Invoke(message.player);
	}

	private void HandlePlayerLeftMessage(PlayerLeftMessage message, ulong _)
	{
		_logger.Debug($"Received PlayerLeftMessage for {message.playerId}");
		Players.RemoveAll((RunLobbyPlayer p) => p.id == message.playerId);
		this.RemotePlayerDisconnected?.Invoke(message.playerId);
	}

	public void AbandonRun()
	{
		if (_netService.Type != NetGameType.Host)
		{
			throw new InvalidOperationException("Abandon run can only be called as host!");
		}
		_logger.Info("Abandoning run as host");
		NetHostGameService netHostGameService = (NetHostGameService)_netService;
		netHostGameService.SendMessage(default(RunAbandonedMessage));
		_lobbyListener.RunAbandoned();
		_netService.Disconnect(NetError.HostAbandoned);
	}

	private void HandleRunAbandonedMessage(RunAbandonedMessage message, ulong _)
	{
		_logger.Debug("Received RunAbandonedMessage");
		_lobbyListener.RunAbandoned();
		_netService.Disconnect(NetError.HostAbandoned);
	}

	private void OnConnectedToClientAsHost(ulong playerId)
	{
		_logger.Info($"Player {playerId} connected to host.");
		InitialGameInfoMessage message = default(InitialGameInfoMessage);
		message.sessionState = RunSessionState.Running;
		message.gameMode = GameMode;
		if (_playerCollection.GetPlayer(playerId) == null)
		{
			_logger.Warn($"Client {playerId} connected but they are not in the run!");
			message.connectionFailureReason = ConnectionFailureReason.RunInProgress;
			_netService.SendMessage(message, playerId);
			NetHostGameService netHostGameService = (NetHostGameService)_netService;
			netHostGameService.DisconnectClient(playerId, NetError.RunInProgress);
		}
		else
		{
			_netService.SendMessage(message, playerId);
		}
	}

	private void OnDisconnectedFromClientAsHost(ulong playerId, NetErrorInfo info)
	{
		int num = Players.FindIndex((RunLobbyPlayer p) => p.id == playerId);
		_logger.Info($"Player {playerId} disconnected from host. Is in connected players: {num >= 0}. Reason: {info.GetReason()}");
		if (num >= 0)
		{
			PlayerLeftMessage playerLeftMessage = default(PlayerLeftMessage);
			playerLeftMessage.playerId = playerId;
			PlayerLeftMessage message = playerLeftMessage;
			_netService.SendMessage(message);
			Players.RemoveAt(num);
			this.RemotePlayerDisconnected?.Invoke(playerId);
		}
	}

	private void OnDisconnected(NetErrorInfo info)
	{
		_logger.Info($"Disconnected. Reason: {info.GetReason()}");
		Players.Clear();
		_lobbyListener.LocalPlayerDisconnected(info);
		this.LocalPlayerDisconnected?.Invoke();
	}
}

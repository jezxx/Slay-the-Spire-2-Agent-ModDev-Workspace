using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Game.PeerInput;
using MegaCrit.Sts2.Core.Multiplayer.Messages.Lobby;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;

/// <summary>
/// Class which handles the connection flow for players resuming a run from a save file.
/// This exists before Run, giving players an opportunity to all join before the run is resumed.
/// Only connections from players in the loaded save file will be accepted; all other connections will be denied.
/// <see cref="T:MegaCrit.Sts2.Core.Multiplayer.Game.Lobby.RunLobby" /> handles player connection and disconnection after the run begins.
/// </summary>
public class LoadRunLobby
{
	private struct ConnectingPlayer : IEquatable<ConnectingPlayer>
	{
		public ulong id;

		public CancellationTokenSource timeoutCancelToken;

		public bool Equals(ConnectingPlayer other)
		{
			if (id == other.id)
			{
				return timeoutCancelToken.Equals(other.timeoutCancelToken);
			}
			return false;
		}

		public override bool Equals(object? obj)
		{
			if (obj is ConnectingPlayer other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(id, timeoutCancelToken);
		}
	}

	private readonly Logger _logger;

	private readonly List<ConnectingPlayer> _connectingPlayers = new List<ConnectingPlayer>();

	private bool _isBeginningRun;

	public List<LoadRunLobbyPlayer> Players { get; } = new List<LoadRunLobbyPlayer>();


	public int PlayerCount => Players.Count;

	public IEnumerable<ulong> PlayerIds => Players.Select((LoadRunLobbyPlayer p) => p.id);

	public INetGameService NetService { get; }

	public ILoadRunLobbyListener LobbyListener { get; }

	public PeerInputSynchronizer InputSynchronizer { get; }

	public SerializableRun Run { get; }

	public GameMode GameMode => Run.GameMode;

	/// <summary>
	/// If we are the host, this is the amount of time we give clients to send the initial message response in
	/// milliseconds. Public for tests.
	/// </summary>
	public int ClientResponseTimeout { get; set; } = 10000;


	/// <summary>
	/// Provides extended disconnection info to UI, but only when the local player is the host.
	/// </summary>
	public event Action<ulong, NetErrorInfo>? PlayerFailedToConnect;

	public LoadRunLobby(INetGameService netService, ILoadRunLobbyListener lobbyListener, SerializableRun runSave)
	{
		Run = runSave;
		NetService = netService;
		LobbyListener = lobbyListener;
		InputSynchronizer = new PeerInputSynchronizer(NetService);
		_logger = new Logger("LoadRunLobby", LogType.Network);
		NetService.RegisterMessageHandler<ClientLoadJoinRequestMessage>(HandleClientLoadJoinRequestMessage);
		NetService.RegisterMessageHandler<ClientLobbyJoinRequestMessage>(HandleClientLobbyJoinRequestMessage);
		NetService.RegisterMessageHandler<ClientRejoinRequestMessage>(HandleClientRejoinRequestMessage);
		NetService.RegisterMessageHandler<PlayerReconnectedMessage>(HandlePlayerReconnectedMessage);
		NetService.RegisterMessageHandler<PlayerLeftMessage>(HandlePlayerLeftMessage);
		NetService.RegisterMessageHandler<LobbyPlayerSetReadyMessage>(HandlePlayerReadyMessage);
		NetService.RegisterMessageHandler<LobbyBeginLoadedRunMessage>(HandleLobbyBeginRunMessage);
		NetService.Disconnected += OnDisconnected;
		if (NetService.Type == NetGameType.Host)
		{
			INetHostGameService netHostGameService = (INetHostGameService)netService;
			netHostGameService.ClientConnected += OnConnectedToClientAsHost;
			netHostGameService.ClientDisconnected += OnDisconnectedFromClientAsHost;
			netHostGameService.ClientConnectionFailed += OnClientConnectionFailed;
		}
	}

	public LoadRunLobby(INetGameService netService, ILoadRunLobbyListener lobbyListener, ClientLoadJoinResponseMessage message)
		: this(netService, lobbyListener, message.serializableRun)
	{
		Players = message.playersAlreadyConnected;
	}

	/// <summary>
	/// This should be called to cleanup the lobby before exiting the lobby screen.
	/// </summary>
	/// <param name="disconnectSession">
	/// If true, the net service will be disconnected. Pass true if the lobby is being closed rather than transitioning
	/// to a run.
	/// </param>
	/// <param name="error">If disconnectSession is true, this is the error that is sent to clients.</param>
	public void CleanUp(bool disconnectSession, NetError error = NetError.Quit)
	{
		NetService.UnregisterMessageHandler<ClientLoadJoinRequestMessage>(HandleClientLoadJoinRequestMessage);
		NetService.UnregisterMessageHandler<ClientLobbyJoinRequestMessage>(HandleClientLobbyJoinRequestMessage);
		NetService.UnregisterMessageHandler<ClientRejoinRequestMessage>(HandleClientRejoinRequestMessage);
		NetService.UnregisterMessageHandler<PlayerReconnectedMessage>(HandlePlayerReconnectedMessage);
		NetService.UnregisterMessageHandler<PlayerLeftMessage>(HandlePlayerLeftMessage);
		NetService.UnregisterMessageHandler<LobbyPlayerSetReadyMessage>(HandlePlayerReadyMessage);
		NetService.UnregisterMessageHandler<LobbyBeginLoadedRunMessage>(HandleLobbyBeginRunMessage);
		if (disconnectSession)
		{
			if (NetService.IsConnected)
			{
				NetService.Disconnect(error);
			}
			InputSynchronizer.Dispose();
		}
		NetService.Disconnected -= OnDisconnected;
		if (NetService.Type == NetGameType.Host)
		{
			INetHostGameService netHostGameService = (INetHostGameService)NetService;
			netHostGameService.ClientConnected -= OnConnectedToClientAsHost;
			netHostGameService.ClientDisconnected -= OnDisconnectedFromClientAsHost;
			netHostGameService.ClientConnectionFailed -= OnClientConnectionFailed;
		}
	}

	/// <summary>
	/// Should be called when the lobby opens on the host player's side to generate the host's lobby player.
	/// </summary>
	public void AddLocalHostPlayer()
	{
		if (NetService.Type == NetGameType.Client)
		{
			throw new InvalidOperationException("Tried to add local host player as client!");
		}
		_logger.Context = $"Lobby ({NetService.NetId})";
		LoadRunLobbyPlayer loadRunLobbyPlayer = default(LoadRunLobbyPlayer);
		loadRunLobbyPlayer.id = NetService.NetId;
		loadRunLobbyPlayer.isReady = false;
		loadRunLobbyPlayer.isModded = NetService.LocalVersion.IsModded();
		LoadRunLobbyPlayer loadRunLobbyPlayer2 = loadRunLobbyPlayer;
		Players.Add(loadRunLobbyPlayer2);
		LobbyListener.PlayerConnected(loadRunLobbyPlayer2);
	}

	private void HandleClientLoadJoinRequestMessage(ClientLoadJoinRequestMessage message, ulong senderId)
	{
		if (NetService.Type != NetGameType.Host)
		{
			throw new InvalidOperationException("Received ClientLoadJoinRequestMessage as non-host!");
		}
		INetHostGameService netHostGameService = (INetHostGameService)NetService;
		try
		{
			if (Run.Players.FindIndex((SerializablePlayer p) => p.NetId == senderId) < 0)
			{
				_logger.Warn($"Client {senderId} sent ClientLoadJoinRequestMessage but they are not in the loaded run!");
				netHostGameService.DisconnectClient(senderId, NetError.NotInSaveGame);
				return;
			}
			_logger.Info($"Received ClientLoadJoinRequestMessage for {senderId}");
			LoadRunLobbyPlayer loadRunLobbyPlayer = default(LoadRunLobbyPlayer);
			loadRunLobbyPlayer.id = senderId;
			loadRunLobbyPlayer.isReady = false;
			loadRunLobbyPlayer.isModded = netHostGameService.GetVersionInfoForPeer(senderId).Value.IsModded();
			LoadRunLobbyPlayer loadRunLobbyPlayer2 = loadRunLobbyPlayer;
			Players.Add(loadRunLobbyPlayer2);
			LobbyListener.PlayerConnected(loadRunLobbyPlayer2);
			ClientLoadJoinResponseMessage message2 = default(ClientLoadJoinResponseMessage);
			message2.serializableRun = Run;
			message2.playersAlreadyConnected = Players.ToList();
			_logger.Debug($"Sending ClientLoadJoinResponseMessage to {senderId}");
			netHostGameService.SendMessage(message2, senderId);
			netHostGameService.SetPeerReadyForBroadcasting(senderId);
			PlayerReconnectedMessage message3 = default(PlayerReconnectedMessage);
			message3.player = loadRunLobbyPlayer2;
			foreach (LoadRunLobbyPlayer player in Players)
			{
				if (player.id != senderId && player.id != NetService.NetId)
				{
					_logger.Debug($"Sending PlayerReconnectedMessage to {player.id}");
					netHostGameService.SendMessage(message3, player.id);
				}
			}
			RemoveConnectingPlayer(senderId);
		}
		catch
		{
			netHostGameService.DisconnectClient(senderId, NetError.InternalError);
			throw;
		}
	}

	private void HandleClientLobbyJoinRequestMessage(ClientLobbyJoinRequestMessage _, ulong senderId)
	{
		if (NetService.Type != NetGameType.Host)
		{
			throw new InvalidOperationException("Received ClientLobbyJoinRequestMessage as non-host!");
		}
		_logger.Info($"Received invalid ClientLobbyJoinRequestMessage for {senderId}");
		INetHostGameService netHostGameService = (INetHostGameService)NetService;
		netHostGameService.DisconnectClient(senderId, NetError.InvalidJoin);
	}

	private void HandleClientRejoinRequestMessage(ClientRejoinRequestMessage _, ulong senderId)
	{
		if (NetService.Type != NetGameType.Host)
		{
			throw new InvalidOperationException("Received ClientRejoinRequestMessage as non-host!");
		}
		_logger.Info($"Received invalid ClientRejoinRequestMessage for {senderId}");
		INetHostGameService netHostGameService = (INetHostGameService)NetService;
		netHostGameService.DisconnectClient(senderId, NetError.InvalidJoin);
	}

	private void HandlePlayerReconnectedMessage(PlayerReconnectedMessage message, ulong _)
	{
		_logger.Debug($"Received PlayerReconnectedMessage with player ID {message.player.id}");
		Players.Add(message.player);
		LobbyListener.PlayerConnected(message.player);
	}

	private void HandlePlayerLeftMessage(PlayerLeftMessage message, ulong senderId)
	{
		_logger.Debug($"Received PlayerLeftMessage for {message.playerId}");
		int num = Players.RemoveAll((LoadRunLobbyPlayer p) => p.id == message.playerId);
		if (num > 0)
		{
			InputSynchronizer.OnPlayerDisconnected(message.playerId);
			LobbyListener.RemotePlayerDisconnected(message.playerId);
		}
	}

	private void HandlePlayerReadyMessage(LobbyPlayerSetReadyMessage message, ulong senderId)
	{
		_logger.Debug($"Received {"LobbyPlayerSetReadyMessage"} for player {senderId} with value {message.ready}");
		int num = Players.FindIndex((LoadRunLobbyPlayer p) => p.id == senderId);
		if (num >= 0)
		{
			LoadRunLobbyPlayer value = Players[num];
			bool isReady = value.isReady;
			value.isReady = message.ready;
			Players[num] = value;
			if (isReady != message.ready)
			{
				LobbyListener.PlayerReadyChanged(senderId);
			}
			BeginRunForAllPlayersIfAllReady();
		}
	}

	private void HandleLobbyBeginRunMessage(LobbyBeginLoadedRunMessage message, ulong senderId)
	{
		_logger.Debug("Received LobbyBeginLoadedRunMessage");
		_isBeginningRun = true;
		BeginRunLocally();
	}

	private async Task TryBeginRunForAllPlayers()
	{
		if (NetService.Type == NetGameType.Client)
		{
			throw new InvalidOperationException("Can only begin run for all peers as host!");
		}
		if (_isBeginningRun)
		{
			_logger.Warn("Tried to begin run twice, ignoring second one!");
			return;
		}
		_isBeginningRun = true;
		if (!(await LobbyListener.ShouldAllowRunToBegin()))
		{
			SetReady(ready: false);
			_isBeginningRun = false;
			return;
		}
		NetService.SendMessage(default(LobbyBeginLoadedRunMessage));
		BeginRunLocally();
		if (NetService.Type == NetGameType.Host)
		{
			INetHostGameService netHostGameService = (INetHostGameService)NetService;
			netHostGameService.NetHost?.SetHostIsClosed(isClosed: true);
		}
	}

	private void BeginRunLocally()
	{
		NetService.SetBufferMessages(bufferMessages: true);
		LobbyListener.BeginRun();
	}

	public void SetReady(bool ready)
	{
		int num = Players.FindIndex((LoadRunLobbyPlayer p) => p.id == NetService.NetId);
		if (num >= 0)
		{
			LoadRunLobbyPlayer value = Players[num];
			value.isReady = ready;
			Players[num] = value;
			LobbyPlayerSetReadyMessage message = default(LobbyPlayerSetReadyMessage);
			message.ready = ready;
			NetService.SendMessage(message);
			LobbyListener.PlayerReadyChanged(NetService.NetId);
			_logger.Info($"Local player {NetService.NetId} is ready");
			BeginRunForAllPlayersIfAllReady();
		}
	}

	/// <summary>
	/// Whether the given player has readied up. Players is only the peers currently in the lobby, but callers ask
	/// about every player in the save file, including ones who have not connected yet or who have just left. Those
	/// players are not ready, so this must answer for any id rather than only for lobby members.
	/// </summary>
	public bool IsPlayerReady(ulong playerId)
	{
		return Players.Any((LoadRunLobbyPlayer p) => p.id == playerId && p.isReady);
	}

	public bool IsAboutToBeginGame()
	{
		if (_connectingPlayers.Count > 0)
		{
			return false;
		}
		if (NetService.Type.IsMultiplayer() && Players.Count == 1)
		{
			return false;
		}
		return Players.All((LoadRunLobbyPlayer p) => p.isReady);
	}

	private void BeginRunForAllPlayersIfAllReady()
	{
		if ((NetService.Type == NetGameType.Host || NetService.Type == NetGameType.Singleplayer) && IsAboutToBeginGame())
		{
			TaskHelper.RunSafely(TryBeginRunForAllPlayers());
		}
	}

	private void OnConnectedToClientAsHost(ulong playerId)
	{
		_logger.Info($"Client {playerId} connected. Sending initial game info message");
		InitialGameInfoMessage message = default(InitialGameInfoMessage);
		message.sessionState = RunSessionState.InLoadedLobby;
		message.gameMode = GameMode;
		if (_isBeginningRun)
		{
			message.connectionFailureReason = ConnectionFailureReason.RunInProgress;
			NetService.SendMessage(message, playerId);
			_logger.Warn($"Client {playerId} connected but we are already beginning the run!");
			((INetHostGameService)NetService).DisconnectClient(playerId, NetError.RunInProgress);
		}
		else if (Run.Players.FindIndex((SerializablePlayer p) => p.NetId == playerId) < 0)
		{
			message.connectionFailureReason = ConnectionFailureReason.NotInSaveGame;
			NetService.SendMessage(message, playerId);
			_logger.Warn($"Client {playerId} connected but they were not in the loaded game!");
			((INetHostGameService)NetService).DisconnectClient(playerId, NetError.NotInSaveGame);
		}
		else
		{
			ConnectingPlayer connectingPlayer = default(ConnectingPlayer);
			connectingPlayer.id = playerId;
			connectingPlayer.timeoutCancelToken = new CancellationTokenSource();
			ConnectingPlayer connectingPlayer2 = connectingPlayer;
			_connectingPlayers.Add(connectingPlayer2);
			NetService.SendMessage(message, playerId);
			TaskHelper.RunSafely(BeginClientResponseTimeout(connectingPlayer2));
		}
	}

	private async Task BeginClientResponseTimeout(ConnectingPlayer connectingPlayer)
	{
		await Task.Delay(ClientResponseTimeout, connectingPlayer.timeoutCancelToken.Token);
		if (!connectingPlayer.timeoutCancelToken.IsCancellationRequested)
		{
			int num = _connectingPlayers.IndexOf(connectingPlayer);
			if (num >= 0)
			{
				_logger.Info($"Disconnecting player {connectingPlayer.id} because they did not respond to the initial game join message within {ClientResponseTimeout}ms");
				INetHostGameService netHostGameService = (INetHostGameService)NetService;
				netHostGameService.DisconnectClient(connectingPlayer.id, NetError.LobbyJoinTimeout);
			}
		}
	}

	private void OnDisconnectedFromClientAsHost(ulong playerId, NetErrorInfo info)
	{
		int num = Players.FindIndex((LoadRunLobbyPlayer p) => p.id == playerId);
		if (num >= 0)
		{
			_logger.Info($"Client {playerId} disconnected, reason: {info.GetReason()}");
			PlayerLeftMessage playerLeftMessage = default(PlayerLeftMessage);
			playerLeftMessage.playerId = playerId;
			PlayerLeftMessage message = playerLeftMessage;
			NetService.SendMessage(message);
			Players.RemoveAt(num);
			RemoveConnectingPlayer(playerId);
			InputSynchronizer.OnPlayerDisconnected(message.playerId);
			LobbyListener.RemotePlayerDisconnected(playerId);
			BeginRunForAllPlayersIfAllReady();
		}
	}

	private void OnClientConnectionFailed(ulong playerId, NetErrorInfo info)
	{
		this.PlayerFailedToConnect?.Invoke(playerId, info);
	}

	private void RemoveConnectingPlayer(ulong playerId)
	{
		for (int i = 0; i < _connectingPlayers.Count; i++)
		{
			if (_connectingPlayers[i].id == playerId)
			{
				_connectingPlayers[i].timeoutCancelToken.Cancel();
				_connectingPlayers.RemoveAt(i);
				i--;
			}
		}
	}

	private void OnDisconnected(NetErrorInfo info)
	{
		_logger.Info($"Disconnected from host, reason: {info.GetReason()}");
		Players.Clear();
		LobbyListener.LocalPlayerDisconnected(info);
	}
}

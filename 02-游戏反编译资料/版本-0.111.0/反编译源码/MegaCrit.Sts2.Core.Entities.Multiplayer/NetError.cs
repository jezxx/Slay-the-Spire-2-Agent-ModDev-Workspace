namespace MegaCrit.Sts2.Core.Entities.Multiplayer;

/// <summary>
/// A list of reasons for which we may be disconnected from a remote host. Each of these is associated with a particular
/// error message that we can show to the user.
/// It's likely that this list will expand over time.
/// Categories advance by 100 so that we can add more errors as time goes on. The list should stay stable between
/// versions.
/// </summary>
public enum NetError
{
	/// <summary>
	/// No reason was passed.
	/// </summary>
	None = 0,
	/// <summary>
	/// Normal quit (Host save and quit or quit the application).
	/// </summary>
	Quit = 1,
	/// <summary>
	/// Normal quit at the end of the run. Signals to clients they should not also quit the run.
	/// </summary>
	QuitGameOver = 2,
	/// <summary>
	/// Host abandoned the game without saving it.
	/// </summary>
	HostAbandoned = 3,
	/// <summary>
	/// We were forcibly removed from the game.
	/// </summary>
	Kicked = 4,
	/// <summary>
	/// Tried to join a user that is not currently in a multiplayer game.
	/// </summary>
	InvalidJoin = 5,
	/// <summary>
	/// The user cancelled the join flow before it was completed.
	/// </summary>
	CancelledJoin = 6,
	/// <summary>The lobby we tried to connect to is full.</summary>
	LobbyFull = 100,
	/// <summary>
	/// The run is already in progress, and rejoining is not implemented.
	/// </summary>
	RunInProgress = 101,
	/// <summary>
	/// The run was loaded from a save file, and the player attempting to connect is not in the save.
	/// </summary>
	NotInSaveGame = 102,
	/// <summary>
	/// The host's version does not match the client's.
	/// </summary>
	VersionMismatch = 103,
	/// <summary>
	/// You are banned from the lobby, you have blocked someone in the lobby, or someone in the lobby blocked you.
	/// </summary>
	JoinBlockedByUser = 104,
	/// <summary>
	/// Our state, as a client, diverged from the host's during combat.
	/// </summary>
	StateDivergence = 105,
	/// <summary>
	/// The client did not send the net-service-level handshake response in time.
	/// Different from an internet timeout, as that is below the application layer.
	/// </summary>
	HandshakeTimeout = 106,
	/// <summary>
	/// Either the host had mods that we didn't have, or we had mods that the host didn't have.
	/// </summary>
	ModMismatch = 107,
	/// <summary>
	/// The client responded with something invalid during the version handshake.
	/// </summary>
	InvalidHandshake = 108,
	/// <summary>
	/// The client did not send the response to the lobby join message in time.
	/// Different from an internet timeout, as that is below the application layer.
	/// </summary>
	LobbyJoinTimeout = 109,
	/// <summary>
	/// Couldn't connect to the session, likely because of internet issues.
	/// </summary>
	NoInternet = 200,
	/// <summary>
	/// Connection timed out.
	/// </summary>
	Timeout = 201,
	/// <summary>
	/// Internal error, like an exception or a similar local bug.
	/// </summary>
	InternalError = 202,
	/// <summary>
	/// Network issue that we are not sure how to diagnose.
	/// </summary>
	UnknownNetworkError = 203,
	/// <summary>
	/// Too many attempts to do the same thing. Player should try again in a bit.
	/// </summary>
	RateLimited = 204,
	/// <summary>
	/// Generic transient issue. Player should try again later.
	/// </summary>
	TryAgainLater = 205,
	/// <summary>
	/// Hosting the game failed.
	/// </summary>
	FailedToHost = 206,
	/// <summary>
	/// Couldn't make secure connection (Steam BadCert and BadCrypt). Most common cause is out-of-sync clocks.
	/// </summary>
	SecureConnectionFailed = 207
}

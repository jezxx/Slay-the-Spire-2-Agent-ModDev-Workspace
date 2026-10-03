using MegaCrit.Sts2.Core.Entities.Multiplayer;

namespace MegaCrit.Sts2.Core.Multiplayer.Connection;

public struct HandshakeResult
{
	public HandshakeStatus status;

	public PeerVersionInfo? remoteVersionInfo;

	public ConnectionFailureExtraInfo? extraInfo;

	public HandshakeResult(HandshakeStatus status)
	{
		remoteVersionInfo = null;
		extraInfo = null;
		this.status = status;
	}

	public HandshakeResult(HandshakeStatus status, PeerVersionInfo versionInfo, ConnectionFailureExtraInfo extraInfo)
	{
		this.status = status;
		remoteVersionInfo = versionInfo;
		this.extraInfo = extraInfo;
	}
}

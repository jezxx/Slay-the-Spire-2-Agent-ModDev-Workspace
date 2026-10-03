using MegaCrit.Sts2.Core.Multiplayer;

namespace MegaCrit.Sts2.Core.Entities.Multiplayer;

public struct NetClientData
{
	public ulong peerId;

	public bool readyForBroadcasting;

	public PeerVersionInfo versionInfo;
}

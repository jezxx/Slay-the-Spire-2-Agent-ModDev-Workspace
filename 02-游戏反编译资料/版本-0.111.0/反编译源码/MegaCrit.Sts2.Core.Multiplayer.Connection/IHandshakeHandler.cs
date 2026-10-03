using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace MegaCrit.Sts2.Core.Multiplayer.Connection;

public interface IHandshakeHandler
{
	NetGameType Type { get; }

	void SendHandshakeMessage(ulong peerId, PacketWriter writer);

	void HandshakeFailed(ulong peerId, NetErrorInfo info);

	void HandshakeSucceeded(ulong peerId, PeerVersionInfo versionInfo);
}

using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;

namespace MegaCrit.Sts2.Core.Multiplayer.Messages.Lobby;

/// <summary>
/// Sent from host to all clients when a client has rejoined the game.
/// </summary>
public struct PlayerRejoinedMessage : INetMessage, IPacketSerializable
{
	public RunLobbyPlayer player;

	public bool ShouldBroadcast => false;

	public NetTransferMode Mode => NetTransferMode.Reliable;

	public LogLevel LogLevel => LogLevel.VeryDebug;

	public bool ShouldBuffer => true;

	public void Serialize(PacketWriter writer)
	{
		writer.Write(player);
	}

	public void Deserialize(PacketReader reader)
	{
		player = reader.Read<RunLobbyPlayer>();
	}
}

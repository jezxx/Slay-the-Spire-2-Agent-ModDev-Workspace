using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;

namespace MegaCrit.Sts2.Core.Multiplayer.Messages.Lobby;

/// <summary>
/// Sent from host to clients when a client has reconnected, either in <see cref="T:MegaCrit.Sts2.Core.Multiplayer.Game.Lobby.LoadRunLobby" /> or during a run.
/// The newly joined client receives ClientRejoinResponseMessage and not this message.
/// </summary>
public struct PlayerReconnectedMessage : INetMessage, IPacketSerializable
{
	public LoadRunLobbyPlayer player;

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
		player = reader.Read<LoadRunLobbyPlayer>();
	}
}

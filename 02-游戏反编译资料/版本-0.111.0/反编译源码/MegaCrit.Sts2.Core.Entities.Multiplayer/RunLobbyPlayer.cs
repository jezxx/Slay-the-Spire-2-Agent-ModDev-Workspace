using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace MegaCrit.Sts2.Core.Entities.Multiplayer;

public struct RunLobbyPlayer : IPacketSerializable
{
	public ulong id;

	public bool isModded;

	public void Serialize(PacketWriter writer)
	{
		writer.WriteULong(id);
		writer.WriteBool(isModded);
	}

	public void Deserialize(PacketReader reader)
	{
		id = reader.ReadULong();
		isModded = reader.ReadBool();
	}
}

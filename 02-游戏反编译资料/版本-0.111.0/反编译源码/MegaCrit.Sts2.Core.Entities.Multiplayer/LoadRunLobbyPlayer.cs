using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace MegaCrit.Sts2.Core.Entities.Multiplayer;

public struct LoadRunLobbyPlayer : IPacketSerializable
{
	public ulong id;

	public bool isModded;

	public bool isReady;

	public void Serialize(PacketWriter writer)
	{
		writer.WriteULong(id);
		writer.WriteBool(isModded);
		writer.WriteBool(isReady);
	}

	public void Deserialize(PacketReader reader)
	{
		id = reader.ReadULong();
		isModded = reader.ReadBool();
		isReady = reader.ReadBool();
	}

	public override string ToString()
	{
		return $"Player {id}";
	}
}

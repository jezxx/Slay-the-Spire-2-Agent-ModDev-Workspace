using System.Text.Json.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace MegaCrit.Sts2.Core.Saves;

public record SerializableRng : IPacketSerializable
{
	[JsonPropertyName("counter")]
	public int counter;

	[JsonPropertyName("s0")]
	public ulong state0;

	[JsonPropertyName("s1")]
	public ulong state1;

	[JsonPropertyName("s2")]
	public ulong state2;

	[JsonPropertyName("s3")]
	public ulong state3;

	public void Serialize(PacketWriter writer)
	{
		writer.WriteInt(counter);
		writer.WriteULong(state0);
		writer.WriteULong(state1);
		writer.WriteULong(state2);
		writer.WriteULong(state3);
	}

	public void Deserialize(PacketReader reader)
	{
		counter = reader.ReadInt();
		state0 = reader.ReadULong();
		state1 = reader.ReadULong();
		state2 = reader.ReadULong();
		state3 = reader.ReadULong();
	}

	public override string ToString()
	{
		return $"Counter: {counter} State: {state0} {state1} {state2} {state3}";
	}
}

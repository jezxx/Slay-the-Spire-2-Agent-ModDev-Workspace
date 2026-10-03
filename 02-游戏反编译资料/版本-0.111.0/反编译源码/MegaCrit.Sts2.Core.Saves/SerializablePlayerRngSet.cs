using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using MegaCrit.Sts2.Core.Entities.Rngs;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace MegaCrit.Sts2.Core.Saves;

public class SerializablePlayerRngSet : IPacketSerializable
{
	[JsonPropertyName("seed")]
	public ulong Seed { get; set; }

	[JsonPropertyName("rngs")]
	public Dictionary<PlayerRngType, SerializableRng> Rngs { get; set; } = new Dictionary<PlayerRngType, SerializableRng>();


	public void Serialize(PacketWriter writer)
	{
		writer.WriteULong(Seed);
		writer.WriteInt(Rngs.Count, 8);
		PlayerRngType[] values = Enum.GetValues<PlayerRngType>();
		foreach (PlayerRngType playerRngType in values)
		{
			if (Rngs.TryGetValue(playerRngType, out SerializableRng value))
			{
				writer.WriteEnum(playerRngType);
				writer.Write(value);
			}
		}
	}

	public void Deserialize(PacketReader reader)
	{
		Seed = reader.ReadULong();
		int num = reader.ReadInt(8);
		for (int i = 0; i < num; i++)
		{
			PlayerRngType key = reader.ReadEnum<PlayerRngType>();
			SerializableRng value = reader.Read<SerializableRng>();
			Rngs[key] = value;
		}
	}
}

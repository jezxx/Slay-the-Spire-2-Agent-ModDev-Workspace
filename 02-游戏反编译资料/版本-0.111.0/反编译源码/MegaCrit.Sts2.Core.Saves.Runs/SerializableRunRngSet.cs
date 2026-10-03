using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using MegaCrit.Sts2.Core.Entities.Rngs;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace MegaCrit.Sts2.Core.Saves.Runs;

public class SerializableRunRngSet : IPacketSerializable
{
	[JsonPropertyName("seed")]
	public string? Seed { get; set; }

	[JsonPropertyName("rngs")]
	public Dictionary<RunRngType, SerializableRng> Rngs { get; set; } = new Dictionary<RunRngType, SerializableRng>();


	public void Serialize(PacketWriter writer)
	{
		writer.WriteString(Seed);
		writer.WriteInt(Rngs.Count, 8);
		RunRngType[] values = Enum.GetValues<RunRngType>();
		foreach (RunRngType runRngType in values)
		{
			if (Rngs.TryGetValue(runRngType, out SerializableRng value))
			{
				writer.WriteEnum(runRngType);
				writer.Write(value);
			}
		}
	}

	public void Deserialize(PacketReader reader)
	{
		Seed = reader.ReadString();
		int num = reader.ReadInt(8);
		for (int i = 0; i < num; i++)
		{
			RunRngType key = reader.ReadEnum<RunRngType>();
			SerializableRng value = reader.Read<SerializableRng>();
			Rngs[key] = value;
		}
	}
}

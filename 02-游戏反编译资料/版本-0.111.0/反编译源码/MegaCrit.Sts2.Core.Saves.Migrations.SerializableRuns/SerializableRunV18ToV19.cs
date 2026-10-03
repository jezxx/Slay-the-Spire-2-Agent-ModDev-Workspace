using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using MegaCrit.Sts2.Core.Entities.Rngs;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Random;

namespace MegaCrit.Sts2.Core.Saves.Migrations.SerializableRuns;

/// <summary>
/// Upgrades counter-based RNGs to the new method of saving RNG state.
/// </summary>
[Migration(typeof(SerializableRun), 18, 19)]
public class SerializableRunV18ToV19 : MigrationBase<SerializableRun>
{
	public const string seedPrefix = "old";

	protected override void ApplyMigration(MigratingData saveData)
	{
		MigrateRunRngs(saveData);
		MigratePlayerRngs(saveData);
		Log.Info("SerializableRun migration v18 -> v19: Upgraded RNG serialization");
	}

	private void MigrateRunRngs(MigratingData saveData)
	{
		if (!(saveData.GetRawNode("rng") is JsonObject jsonObject) || !(jsonObject["seed"] is JsonValue jsonValue) || !jsonValue.TryGetValue<string>(out string value) || !(jsonObject["counters"] is JsonObject jsonObject2))
		{
			return;
		}
		JsonObject jsonObject3 = new JsonObject();
		foreach (KeyValuePair<string, JsonNode> item in jsonObject2)
		{
			if (item.Value is JsonValue jsonValue2 && jsonValue2.TryGetValue<int>(out var value2) && Enum.TryParse<RunRngType>(item.Key, out var result))
			{
				string type = StringHelper.SnakeCase(result.ToString());
				MegaRandom megaRandom = GenerateRng((ulong)StringHelper.GetDeterministicHashCodeOld(value), type, value2);
				SerializableRng serializableRng = new SerializableRng
				{
					counter = value2
				};
				megaRandom.FillSerializableState(serializableRng);
				string json = JsonSerializer.Serialize(serializableRng, JsonSerializationUtility.GetTypeInfo<SerializableRng>());
				jsonObject3[item.Key] = JsonNode.Parse(json);
			}
		}
		jsonObject.Remove("counters");
		jsonObject["rngs"] = jsonObject3;
		jsonObject["seed"] = "old" + value;
	}

	private void MigratePlayerRngs(MigratingData saveData)
	{
		if (!(saveData.GetRawNode("players") is JsonArray jsonArray))
		{
			return;
		}
		foreach (JsonNode item in jsonArray)
		{
			if (!(item?["rng"] is JsonObject jsonObject) || !(jsonObject["seed"] is JsonValue jsonValue) || !jsonValue.TryGetValue<ulong>(out var value) || !(jsonObject["counters"] is JsonObject jsonObject2))
			{
				continue;
			}
			JsonObject jsonObject3 = new JsonObject();
			foreach (KeyValuePair<string, JsonNode> item2 in jsonObject2)
			{
				if (item2.Value is JsonValue jsonValue2 && jsonValue2.TryGetValue<int>(out var value2) && Enum.TryParse<PlayerRngType>(item2.Key, out var result))
				{
					string type = StringHelper.SnakeCase(result.ToString());
					MegaRandom megaRandom = GenerateRng(value, type, value2);
					SerializableRng serializableRng = new SerializableRng
					{
						counter = value2
					};
					megaRandom.FillSerializableState(serializableRng);
					string json = JsonSerializer.Serialize(serializableRng, JsonSerializationUtility.GetTypeInfo<SerializableRng>());
					jsonObject3[item2.Key] = JsonNode.Parse(json);
				}
			}
			jsonObject.Remove("counters");
			jsonObject["rngs"] = jsonObject3;
		}
	}

	public static MegaRandom GenerateRng(ulong seed, string type, int counter)
	{
		uint num = (uint)((int)seed + StringHelper.GetDeterministicHashCodeOld(type));
		MegaRandom megaRandom = new MegaRandom(num);
		for (int i = 0; i < counter; i++)
		{
			megaRandom.NextULong();
		}
		return megaRandom;
	}
}

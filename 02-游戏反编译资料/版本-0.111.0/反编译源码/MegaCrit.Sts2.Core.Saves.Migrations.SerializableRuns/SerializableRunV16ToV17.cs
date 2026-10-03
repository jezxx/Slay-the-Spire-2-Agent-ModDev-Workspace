using System;
using System.Text.Json.Nodes;
using MegaCrit.Sts2.Core.Logging;

namespace MegaCrit.Sts2.Core.Saves.Migrations.SerializableRuns;

/// <summary>
/// Migrating LastingCandy.CombatsSeen to LastingCandy.CombatRewardsSeen
/// </summary>
[Migration(typeof(SerializableRun), 16, 17)]
public class SerializableRunV16ToV17 : MigrationBase<SerializableRun>
{
	protected override void ApplyMigration(MigratingData saveData)
	{
		Log.Info("SerializableRun migration v16 -> v17: Migrating LastingCandy.CombatsSeen to LastingCandy.CombatRewardsSeen");
		JsonObject rawNode = saveData.GetRawNode();
		if (rawNode == null)
		{
			return;
		}
		bool flag = false;
		if (saveData.GetRawNode("pre_finished_room") is JsonObject jsonObject && jsonObject["room_type"] is JsonValue jsonValue && jsonValue.TryGetValue<string>(out string value) && (value.Equals("monster", StringComparison.OrdinalIgnoreCase) || value.Equals("elite", StringComparison.OrdinalIgnoreCase) || value.Equals("boss", StringComparison.OrdinalIgnoreCase)))
		{
			string value2;
			string a = ((jsonObject["encounter_id"] is JsonValue jsonValue2 && jsonValue2.TryGetValue<string>(out value2)) ? value2 : null);
			flag = !string.Equals(a, "ENCOUNTER.BATTLEWORN_DUMMY_EVENT_ENCOUNTER", StringComparison.OrdinalIgnoreCase);
		}
		if (!(rawNode["players"] is JsonArray jsonArray))
		{
			return;
		}
		foreach (JsonNode item in jsonArray)
		{
			if (item == null || !(item["relics"] is JsonArray jsonArray2))
			{
				continue;
			}
			foreach (JsonNode item2 in jsonArray2)
			{
				if (item2 == null || item2["id"] == null || item2["id"].GetValue<string>() != "RELIC.LASTING_CANDY" || !(item2["props"] is JsonObject jsonObject2) || !(jsonObject2["ints"] is JsonArray jsonArray3))
				{
					continue;
				}
				foreach (JsonNode item3 in jsonArray3)
				{
					if (item3 != null && item3["name"] != null && !(item3["name"].GetValue<string>() != "CombatsSeen"))
					{
						item3["name"] = "CombatRewardsSeen";
						if (flag)
						{
							item3["value"] = item3["value"].GetValue<int>() - 1;
						}
					}
				}
			}
		}
	}
}

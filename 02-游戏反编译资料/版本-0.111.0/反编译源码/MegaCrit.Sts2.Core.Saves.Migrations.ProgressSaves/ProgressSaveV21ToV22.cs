using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;

namespace MegaCrit.Sts2.Core.Saves.Migrations.ProgressSaves;

/// <summary>
/// Removes Osty, Byrdpip and Pael's Legion from the enemy stats
/// because they aren't actually enemies.
/// </summary>
[Migration(typeof(SerializableProgress), 21, 22)]
public class ProgressSaveV21ToV22 : MigrationBase<SerializableProgress>
{
	protected override void ApplyMigration(MigratingData saveData)
	{
		Log.Info("Progress save migration v21 -> v22: Removing Osty, Byrdpip and Pael's Legion from enemy stats");
		string[] source = new string[3]
		{
			ModelDb.Monster<Osty>().Id.ToString(),
			ModelDb.Monster<PaelsLegion>().Id.ToString(),
			ModelDb.Monster<Byrdpip>().Id.ToString()
		};
		if (!(saveData.GetRawNode("enemy_stats") is JsonArray jsonArray))
		{
			return;
		}
		List<JsonNode> list = new List<JsonNode>();
		foreach (JsonNode item in jsonArray)
		{
			if (item != null)
			{
				JsonNode jsonNode = item["enemy_id"];
				if (jsonNode != null && source.Contains(jsonNode.GetValue<string>()))
				{
					list.Add(item);
				}
			}
		}
		foreach (JsonNode item2 in list)
		{
			jsonArray.Remove(item2);
		}
	}
}

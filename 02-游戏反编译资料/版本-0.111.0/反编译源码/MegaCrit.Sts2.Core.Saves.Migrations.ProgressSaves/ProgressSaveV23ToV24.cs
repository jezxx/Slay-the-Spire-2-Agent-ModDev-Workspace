using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Timeline;

namespace MegaCrit.Sts2.Core.Saves.Migrations.ProgressSaves;

/// <summary>
/// Moves the score bar unlock count out of total_unlocks and into the epochs that now represent it.
/// </summary>
/// <remarks>
/// v23 and earlier stored the count in total_unlocks, separately from the epochs each unlock granted.
/// <see cref="P:MegaCrit.Sts2.Core.Saves.ProgressState.TotalUnlocks" /> now counts the epochs instead, so the count has to be
/// materialized into them: a save reading "18 unlocks, 17 epochs" becomes 18 epochs, because in the new
/// representation the epochs are the count.
/// <para>
/// Those two numbers can disagree because the end-of-run score bar used to advance the counter without
/// granting (PRG-7234). The counter only moved forward, so the epoch it skipped was gone for good, and
/// where that epoch gated a timeline expansion it stranded everything below it. Counting the epochs
/// instead is what stops it happening again; this is only the format conversion, which every v23 save
/// needs whether or not it lost anything.
/// </para>
/// <para>
/// Reads <see cref="P:MegaCrit.Sts2.Core.Timeline.EpochModel.AgnosticUnlockOrder" /> rather than a frozen copy of it. That list is
/// documented append-only and this only ever reads a prefix, so later additions cannot change what this
/// migration does to an old save.
/// </para>
/// </remarks>
[Migration(typeof(SerializableProgress), 23, 24)]
public class ProgressSaveV23ToV24 : MigrationBase<SerializableProgress>
{
	private static string WireValue(EpochState state)
	{
		return JsonNamingPolicy.SnakeCaseLower.ConvertName(state.ToString());
	}

	protected override void ApplyMigration(MigratingData saveData)
	{
		if (!(saveData.GetRawNode("epochs") is JsonArray jsonArray) || !(saveData.GetRawNode("total_unlocks") is JsonValue jsonValue) || !jsonValue.TryGetValue<int>(out var value))
		{
			return;
		}
		int num = Math.Min(value, EpochModel.AgnosticUnlockOrder.Count);
		if (num <= 0)
		{
			return;
		}
		string text = WireValue(EpochState.Obtained);
		string text2 = WireValue(EpochState.ObtainedNoSlot);
		string text3 = WireValue(EpochState.NotObtained);
		long num2 = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
		for (int i = 0; i < num; i++)
		{
			string epochId = EpochModel.AgnosticUnlockOrder[i];
			JsonNode jsonNode = jsonArray.FirstOrDefault((JsonNode e) => e?["id"]?.GetValue<string>() == epochId);
			if (jsonNode == null)
			{
				Log.Info($"Progress save migration v23 -> v24: {epochId} was earned by unlock {i + 1} but is missing, granting");
				jsonArray.Add(new JsonObject
				{
					["id"] = epochId,
					["state"] = text2,
					["obtain_date"] = num2
				});
			}
			else if (jsonNode["state"]?.GetValue<string>() == text3)
			{
				Log.Info($"Progress save migration v23 -> v24: {epochId} was earned by unlock {i + 1} but was never granted, granting");
				jsonNode["state"] = text;
				jsonNode["obtain_date"] = num2;
			}
		}
	}
}

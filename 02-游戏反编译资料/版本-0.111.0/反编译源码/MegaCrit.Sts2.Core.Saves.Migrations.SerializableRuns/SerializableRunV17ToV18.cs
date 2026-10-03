using System;
using System.Text.Json.Nodes;
using MegaCrit.Sts2.Core.Logging;

namespace MegaCrit.Sts2.Core.Saves.Migrations.SerializableRuns;

/// <summary>
/// Remaps a pre-finished Battleworn Dummy combat room from the old single
/// BattlewornDummyEventEncounter (now abstract, split into V1/V2/V3) to the concrete variant that
/// matches the difficulty the player picked. Without this, loading such a run save resolves the
/// now-missing encounter id to DeprecatedEncounter, and BattlewornDummy.Resume hard-casts it to
/// BattlewornDummyEventEncounter, throwing InvalidCastException and soft-locking the run.
///
/// The old encounter recorded the chosen difficulty (and therefore the reward: V1 potion,
/// V2 card upgrades, V3 relic) in its custom state under "Setting". We map that to the matching
/// concrete encounter so the resumed event grants the correct reward, then drop the stale key that
/// the new encounters no longer read.
/// </summary>
[Migration(typeof(SerializableRun), 17, 18)]
public class SerializableRunV17ToV18 : MigrationBase<SerializableRun>
{
	private const string _oldEncounterId = "ENCOUNTER.BATTLEWORN_DUMMY_EVENT_ENCOUNTER";

	private const string _v1EncounterId = "ENCOUNTER.BATTLEWORN_DUMMY_EVENT_V1_ENCOUNTER";

	private const string _v2EncounterId = "ENCOUNTER.BATTLEWORN_DUMMY_EVENT_V2_ENCOUNTER";

	private const string _v3EncounterId = "ENCOUNTER.BATTLEWORN_DUMMY_EVENT_V3_ENCOUNTER";

	private const string _settingKey = "Setting";

	protected override void ApplyMigration(MigratingData saveData)
	{
		if (saveData.GetRawNode("pre_finished_room") is JsonObject jsonObject && jsonObject["encounter_id"] is JsonValue jsonValue && jsonValue.TryGetValue<string>(out string value) && string.Equals(value, "ENCOUNTER.BATTLEWORN_DUMMY_EVENT_ENCOUNTER", StringComparison.OrdinalIgnoreCase))
		{
			JsonObject jsonObject2 = jsonObject["encounter_state"] as JsonObject;
			string value2;
			string text = ((jsonObject2?["Setting"] is JsonValue jsonValue2 && jsonValue2.TryGetValue<string>(out value2)) ? value2 : null);
			string text2 = ((text == "Setting2") ? "ENCOUNTER.BATTLEWORN_DUMMY_EVENT_V2_ENCOUNTER" : ((!(text == "Setting3")) ? "ENCOUNTER.BATTLEWORN_DUMMY_EVENT_V1_ENCOUNTER" : "ENCOUNTER.BATTLEWORN_DUMMY_EVENT_V3_ENCOUNTER"));
			string text3 = text2;
			jsonObject["encounter_id"] = text3;
			jsonObject2?.Remove("Setting");
			Log.Info($"SerializableRun migration v17 -> v18: remapped Battleworn Dummy encounter to {text3} (setting: {text ?? "none"})");
		}
	}
}

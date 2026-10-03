using System.Text.Json.Nodes;
using MegaCrit.Sts2.Core.ControllerInput;

namespace MegaCrit.Sts2.Core.Saves.Migrations.SettingsSaves;

[Migration(typeof(SettingsSave), 6, 7)]
public class SettingsSaveV6ToV7 : MigrationBase<SettingsSave>
{
	protected override void ApplyMigration(MigratingData saveData)
	{
		if (saveData.GetRawNode("controller_mapping") is JsonObject jsonObject)
		{
			if (jsonObject.ContainsKey("ui_accept"))
			{
				jsonObject.Remove("ui_accept");
			}
			if (!jsonObject.ContainsKey("ui_confirm"))
			{
				jsonObject["ui_confirm"] = Controller.faceButtonNorth.ToString();
			}
			if (!jsonObject.ContainsKey("ui_end_turn"))
			{
				jsonObject["ui_end_turn"] = Controller.faceButtonNorth.ToString();
			}
		}
		if (saveData.GetRawNode("keyboard_mapping") is JsonObject jsonObject2)
		{
			if (jsonObject2.ContainsKey("ui_accept"))
			{
				jsonObject2.Remove("ui_accept");
			}
			if (jsonObject2.ContainsKey("ui_select"))
			{
				jsonObject2.Remove("ui_select");
			}
			if (!jsonObject2.ContainsKey("ui_end_turn"))
			{
				jsonObject2["ui_end_turn"] = "E";
			}
		}
	}
}

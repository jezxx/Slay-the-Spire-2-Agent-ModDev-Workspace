using System.Text.Json.Nodes;
using MegaCrit.Sts2.Core.ControllerInput;

namespace MegaCrit.Sts2.Core.Saves.Migrations.SettingsSaves;

[Migration(typeof(SettingsSave), 7, 8)]
public class SettingsSaveV7ToV8 : MigrationBase<SettingsSave>
{
	protected override void ApplyMigration(MigratingData saveData)
	{
		if (saveData.GetRawNode("keyboard_mapping") is JsonObject jsonObject && jsonObject.ContainsKey(MegaInput.endTurn) && jsonObject.ContainsKey(MegaInput.confirm))
		{
			jsonObject[MegaInput.confirm] = jsonObject[MegaInput.endTurn]?.DeepClone();
		}
	}
}

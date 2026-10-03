using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using MegaCrit.Sts2.Core.ControllerInput;

namespace MegaCrit.Sts2.Core.Saves.Migrations.SettingsSaves;

[Migration(typeof(SettingsSave), 5, 6)]
public class SettingsSaveV5ToV6 : MigrationBase<SettingsSave>
{
	private readonly Dictionary<string, string> _oldToNewMappings = new Dictionary<string, string>
	{
		{
			"controller_d_pad_north",
			Controller.dPadUp
		},
		{
			"controller_d_pad_south",
			Controller.dPadDown
		},
		{
			"controller_d_pad_west",
			Controller.dPadLeft
		},
		{
			"controller_d_pad_east",
			Controller.dPadRight
		},
		{
			"controller_joystick_up",
			Controller.lStickUp
		},
		{
			"controller_joystick_down",
			Controller.lStickDown
		},
		{
			"controller_joystick_left",
			Controller.lStickLeft
		},
		{
			"controller_joystick_right",
			Controller.lStickRight
		},
		{
			"controller_joystick_press",
			Controller.lStickPress
		},
		{
			"controller_l_joystick_up",
			Controller.lStickUp
		},
		{
			"controller_l_joystick_down",
			Controller.lStickDown
		},
		{
			"controller_l_joystick_left",
			Controller.lStickLeft
		},
		{
			"controller_l_joystick_right",
			Controller.lStickRight
		},
		{
			"controller_l_joystick_press",
			Controller.lStickPress
		},
		{
			"controller_r_joystick_up",
			Controller.rStickUp
		},
		{
			"controller_r_joystick_down",
			Controller.rStickDown
		},
		{
			"controller_r_joystick_left",
			Controller.rStickLeft
		},
		{
			"controller_r_joystick_right",
			Controller.rStickRight
		}
	};

	protected override void ApplyMigration(MigratingData saveData)
	{
		if (!(saveData.GetRawNode("controller_mapping") is JsonObject jsonObject))
		{
			return;
		}
		if (!jsonObject.ContainsKey("ui_alt_up"))
		{
			jsonObject["ui_alt_up"] = Controller.rStickUp.ToString();
		}
		if (!jsonObject.ContainsKey("ui_alt_down"))
		{
			jsonObject["ui_alt_down"] = Controller.rStickDown.ToString();
		}
		if (!jsonObject.ContainsKey("ui_alt_left"))
		{
			jsonObject["ui_alt_left"] = Controller.rStickLeft.ToString();
		}
		if (!jsonObject.ContainsKey("ui_alt_right"))
		{
			jsonObject["ui_alt_right"] = Controller.rStickRight.ToString();
		}
		List<string> list = jsonObject.Select<KeyValuePair<string, JsonNode>, string>((KeyValuePair<string, JsonNode> kvp) => kvp.Key).ToList();
		foreach (string item in list)
		{
			if (!jsonObject.ContainsKey(item))
			{
				continue;
			}
			string value = jsonObject[item].GetValue<string>();
			foreach (KeyValuePair<string, string> oldToNewMapping in _oldToNewMappings)
			{
				if (oldToNewMapping.Key == value)
				{
					jsonObject[item] = oldToNewMapping.Value;
				}
			}
		}
	}
}

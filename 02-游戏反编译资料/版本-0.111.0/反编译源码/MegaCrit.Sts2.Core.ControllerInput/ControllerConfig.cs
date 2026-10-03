using System.Collections.Generic;
using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.ControllerInput.ControllerConfigs;
using MegaCrit.Sts2.Core.Helpers;

namespace MegaCrit.Sts2.Core.ControllerInput;

/// <summary>
/// Maps controller specific behavior for different types of controllers.
/// - used to override certain default controller inputs based on the controller types (ie, ps5 controllers use the touchpad to open the map)
/// - Used to dynamically update the hotkey icons depending on what type of controller you are using.
/// </summary>
public abstract class ControllerConfig
{
	private Dictionary<string, string>? _glyphs;

	protected abstract string FolderPath { get; }

	public virtual ControllerMappingType ControllerMappingType => ControllerMappingType.Default;

	/// <summary>
	/// The mapping between Steam Input Actions (defined in the vdf file), and the default controller input they
	/// correspond to.
	/// </summary>
	public virtual Dictionary<string, StringName> SteamInputControllerMap => new Dictionary<string, StringName>
	{
		{
			"Confirm",
			Controller.faceButtonNorth
		},
		{
			"Cancel",
			Controller.faceButtonEast
		},
		{
			"Up",
			Controller.dPadUp
		},
		{
			"Down",
			Controller.dPadDown
		},
		{
			"Left",
			Controller.dPadLeft
		},
		{
			"Right",
			Controller.dPadRight
		},
		{
			"Select",
			Controller.faceButtonSouth
		},
		{
			"Top_Panel",
			Controller.faceButtonWest
		},
		{
			"View_Draw_Pile",
			Controller.leftTrigger
		},
		{
			"View_Discard_Pile",
			Controller.rightTrigger
		},
		{
			"Tab_Right",
			Controller.rightBumper
		},
		{
			"Tab_Left",
			Controller.leftBumper
		},
		{
			"View_Map",
			Controller.selectButton
		},
		{
			"Settings",
			Controller.startButton
		},
		{
			"Peek",
			Controller.lStickPress
		}
	};

	/// <summary>
	/// The default map between controller inputs and specific in game commands.
	/// We have this because inputs can vary between controller types (ex: On ps4 controllers, we use the touchpad instead of
	/// the back/share button to open the map.
	/// </summary>
	public virtual Dictionary<StringName, StringName> DefaultControllerInputMap => new Dictionary<StringName, StringName>
	{
		{
			MegaInput.confirm,
			Controller.faceButtonNorth
		},
		{
			MegaInput.endTurn,
			Controller.faceButtonNorth
		},
		{
			MegaInput.cancel,
			Controller.faceButtonEast
		},
		{
			MegaInput.select,
			Controller.faceButtonSouth
		},
		{
			MegaInput.viewExhaustPileAndTabRight,
			Controller.rightBumper
		},
		{
			MegaInput.viewDeckAndTabLeft,
			Controller.leftBumper
		},
		{
			MegaInput.topPanel,
			Controller.faceButtonWest
		},
		{
			MegaInput.viewDrawPile,
			Controller.leftTrigger
		},
		{
			MegaInput.viewDiscardPile,
			Controller.rightTrigger
		},
		{
			MegaInput.viewMap,
			Controller.selectButton
		},
		{
			MegaInput.peek,
			Controller.lStickPress
		},
		{
			MegaInput.up,
			Controller.dPadUp
		},
		{
			MegaInput.down,
			Controller.dPadDown
		},
		{
			MegaInput.left,
			Controller.dPadLeft
		},
		{
			MegaInput.right,
			Controller.dPadRight
		},
		{
			MegaInput.altUp,
			Controller.rStickUp
		},
		{
			MegaInput.altDown,
			Controller.rStickDown
		},
		{
			MegaInput.altLeft,
			Controller.rStickLeft
		},
		{
			MegaInput.altRight,
			Controller.rStickRight
		},
		{
			MegaInput.pauseAndBack,
			Controller.startButton
		}
	};

	protected virtual string FaceButtonNorthGlyph => ImageHelper.GetImagePath(FolderPath + "/y.tres");

	protected virtual string FaceButtonSouthGlyph => ImageHelper.GetImagePath(FolderPath + "/a.tres");

	protected virtual string FaceButtonEastGlyph => ImageHelper.GetImagePath(FolderPath + "/b.tres");

	protected virtual string FaceButtonWestGlyph => ImageHelper.GetImagePath(FolderPath + "/x.tres");

	private string LeftTriggerGlyph => ImageHelper.GetImagePath(FolderPath + "/lt.tres");

	private string RightTriggerGlyph => ImageHelper.GetImagePath(FolderPath + "/rt.tres");

	private string SelectButtonGlyph => ImageHelper.GetImagePath(FolderPath + "/back.tres");

	private string LeftBumperGlyph => ImageHelper.GetImagePath(FolderPath + "/lb.tres");

	private string RightBumperGlyph => ImageHelper.GetImagePath(FolderPath + "/rb.tres");

	private string StartButtonGlyph => ImageHelper.GetImagePath(FolderPath + "/start.tres");

	private string JoystickPressGlyph => ImageHelper.GetImagePath(FolderPath + "/ls.tres");

	private string DPadNorth => ImageHelper.GetImagePath(FolderPath + "/up.tres");

	private string DPadSouth => ImageHelper.GetImagePath(FolderPath + "/down.tres");

	private string DPadWest => ImageHelper.GetImagePath(FolderPath + "/left.tres");

	private string DPadEast => ImageHelper.GetImagePath(FolderPath + "/right.tres");

	private string Ps4Touchpad => ImageHelper.GetImagePath(FolderPath + "/touchpad.tres");

	private string RightJoystickNorth => ImageHelper.GetImagePath(FolderPath + "/rs_up.tres");

	private string RightJoystickSouth => ImageHelper.GetImagePath(FolderPath + "/rs_down.tres");

	private string RightJoystickEast => ImageHelper.GetImagePath(FolderPath + "/rs_right.tres");

	private string RightJoystickWest => ImageHelper.GetImagePath(FolderPath + "/rs_left.tres");

	private Dictionary<string, string> GlyphMap
	{
		get
		{
			if (_glyphs == null)
			{
				_glyphs = new Dictionary<string, string>
				{
					{
						Controller.faceButtonNorth,
						FaceButtonNorthGlyph
					},
					{
						Controller.faceButtonSouth,
						FaceButtonSouthGlyph
					},
					{
						Controller.faceButtonEast,
						FaceButtonEastGlyph
					},
					{
						Controller.faceButtonWest,
						FaceButtonWestGlyph
					},
					{
						Controller.leftTrigger,
						LeftTriggerGlyph
					},
					{
						Controller.rightTrigger,
						RightTriggerGlyph
					},
					{
						Controller.leftBumper,
						LeftBumperGlyph
					},
					{
						Controller.rightBumper,
						RightBumperGlyph
					},
					{
						Controller.selectButton,
						SelectButtonGlyph
					},
					{
						Controller.startButton,
						StartButtonGlyph
					},
					{
						Controller.lStickPress,
						JoystickPressGlyph
					},
					{
						Controller.dPadUp,
						DPadNorth
					},
					{
						Controller.dPadDown,
						DPadSouth
					},
					{
						Controller.dPadRight,
						DPadEast
					},
					{
						Controller.dPadLeft,
						DPadWest
					},
					{
						Controller.ps4Touchpad,
						Ps4Touchpad
					},
					{
						Controller.rStickUp,
						RightJoystickNorth
					},
					{
						Controller.rStickDown,
						RightJoystickSouth
					},
					{
						Controller.rStickLeft,
						RightJoystickWest
					},
					{
						Controller.rStickRight,
						RightJoystickEast
					}
				};
			}
			return _glyphs;
		}
	}

	public IEnumerable<string> AssetPaths => GlyphMap.Values.Where((string path) => ResourceLoader.Exists(path));

	public static IEnumerable<string> AllAssetPaths => new ControllerConfig[4]
	{
		new SteamControllerConfig(),
		new Ps4Config(),
		new Xbox360Config(),
		new XboxOneConfig()
	}.SelectMany((ControllerConfig c) => c.AssetPaths);

	public Texture2D? GetButtonIcon(string button)
	{
		if (GlyphMap.TryGetValue(button, out string value))
		{
			return ResourceLoader.Load<Texture2D>(value, null, ResourceLoader.CacheMode.Reuse);
		}
		return null;
	}
}

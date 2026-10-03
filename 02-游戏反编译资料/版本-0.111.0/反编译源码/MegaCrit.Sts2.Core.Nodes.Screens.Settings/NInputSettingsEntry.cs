using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Settings;

/// <summary>
/// Represents a single input action and its corresponding hotkey to activate it.
/// Can be clicked on to rebind the key.
/// </summary>
[ScriptPath("res://src/Core/Nodes/Screens/Settings/NInputSettingsEntry.cs")]
public class NInputSettingsEntry : NButton
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : NButton.MethodName
	{
		/// <summary>
		/// Cached name for the 'Create' method.
		/// </summary>
		public static readonly StringName Create = "Create";

		/// <summary>
		/// Cached name for the '_Ready' method.
		/// </summary>
		public new static readonly StringName _Ready = "_Ready";

		/// <summary>
		/// Cached name for the 'UpdateInput' method.
		/// </summary>
		public static readonly StringName UpdateInput = "UpdateInput";

		/// <summary>
		/// Cached name for the 'OnFocus' method.
		/// </summary>
		public new static readonly StringName OnFocus = "OnFocus";

		/// <summary>
		/// Cached name for the 'OnUnfocus' method.
		/// </summary>
		public new static readonly StringName OnUnfocus = "OnUnfocus";

		/// <summary>
		/// Cached name for the '_ExitTree' method.
		/// </summary>
		public new static readonly StringName _ExitTree = "_ExitTree";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : NButton.PropertyName
	{
		/// <summary>
		/// Cached name for the 'InputName' property.
		/// </summary>
		public static readonly StringName InputName = "InputName";

		/// <summary>
		/// Cached name for the '_bg' field.
		/// </summary>
		public static readonly StringName _bg = "_bg";

		/// <summary>
		/// Cached name for the '_inputLabel' field.
		/// </summary>
		public static readonly StringName _inputLabel = "_inputLabel";

		/// <summary>
		/// Cached name for the '_mKbBindingLabel' field.
		/// </summary>
		public static readonly StringName _mKbBindingLabel = "_mKbBindingLabel";

		/// <summary>
		/// Cached name for the '_keyboardOnlyModeBindingLabel' field.
		/// </summary>
		public static readonly StringName _keyboardOnlyModeBindingLabel = "_keyboardOnlyModeBindingLabel";

		/// <summary>
		/// Cached name for the '_missingControllerBindingLabel' field.
		/// </summary>
		public static readonly StringName _missingControllerBindingLabel = "_missingControllerBindingLabel";

		/// <summary>
		/// Cached name for the '_controllerBindingIcon' field.
		/// </summary>
		public static readonly StringName _controllerBindingIcon = "_controllerBindingIcon";

		/// <summary>
		/// Cached name for the '_tween' field.
		/// </summary>
		public static readonly StringName _tween = "_tween";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : NButton.SignalName
	{
	}

	public static readonly Dictionary<StringName, string> commandToLocTitle = new Dictionary<StringName, string>
	{
		{
			MegaInput.confirm,
			"confirm"
		},
		{
			MegaInput.endTurn,
			"endTurn"
		},
		{
			MegaInput.select,
			"select"
		},
		{
			MegaInput.viewDiscardPile,
			"viewDiscard"
		},
		{
			MegaInput.viewDrawPile,
			"viewDraw"
		},
		{
			MegaInput.viewDeckAndTabLeft,
			"viewDeck"
		},
		{
			MegaInput.viewExhaustPileAndTabRight,
			"viewExhaust"
		},
		{
			MegaInput.viewMap,
			"viewMap"
		},
		{
			MegaInput.cancel,
			"cancel"
		},
		{
			MegaInput.peek,
			"peek"
		},
		{
			MegaInput.up,
			"up"
		},
		{
			MegaInput.topPanel,
			"topPanel"
		},
		{
			MegaInput.down,
			"down"
		},
		{
			MegaInput.left,
			"left"
		},
		{
			MegaInput.right,
			"right"
		},
		{
			MegaInput.selectCard1,
			"selectCard1"
		},
		{
			MegaInput.selectCard2,
			"selectCard2"
		},
		{
			MegaInput.selectCard3,
			"selectCard3"
		},
		{
			MegaInput.selectCard4,
			"selectCard4"
		},
		{
			MegaInput.selectCard5,
			"selectCard5"
		},
		{
			MegaInput.selectCard6,
			"selectCard6"
		},
		{
			MegaInput.selectCard7,
			"selectCard7"
		},
		{
			MegaInput.selectCard8,
			"selectCard8"
		},
		{
			MegaInput.selectCard9,
			"selectCard9"
		},
		{
			MegaInput.selectCard10,
			"selectCard10"
		}
	};

	private const string _scenePath = "res://scenes/screens/settings_screen/input_settings_entry.tscn";

	private Control _bg;

	private MegaLabel _inputLabel;

	private MegaLabel _mKbBindingLabel;

	private MegaLabel _keyboardOnlyModeBindingLabel;

	private Control _missingControllerBindingLabel;

	private TextureRect _controllerBindingIcon;

	private Tween? _tween;

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>("res://scenes/screens/settings_screen/input_settings_entry.tscn");

	public StringName InputName { get; private set; }

	public static NInputSettingsEntry Create(string commandName)
	{
		NInputSettingsEntry nInputSettingsEntry = ResourceLoader.Load<PackedScene>("res://scenes/screens/settings_screen/input_settings_entry.tscn", null, ResourceLoader.CacheMode.Reuse).Instantiate<NInputSettingsEntry>(PackedScene.GenEditState.Disabled);
		nInputSettingsEntry.InputName = commandName;
		return nInputSettingsEntry;
	}

	public override void _Ready()
	{
		ConnectSignals();
		_inputLabel = GetNode<MegaLabel>("%InputLabel");
		_mKbBindingLabel = GetNode<MegaLabel>("%MKbBindingInputLabel");
		_keyboardOnlyModeBindingLabel = GetNode<MegaLabel>("%KbModeBindingInputLabel");
		_controllerBindingIcon = GetNode<TextureRect>("%ControllerBindingIcon");
		_missingControllerBindingLabel = GetNode<Control>("%MissingControllerBindingLabel");
		_bg = GetNode<Control>("%Bg");
		string text = commandToLocTitle[InputName];
		_inputLabel.SetTextAutoSize(new LocString("settings_ui", "INPUT_SETTINGS.INPUT_TITLE." + text).GetFormattedText());
		NInputManager.Instance.Connect(NInputManager.SignalName.InputRebound, Callable.From(UpdateInput));
		NControllerManager.Instance.Connect(NControllerManager.SignalName.ControllerDetected, Callable.From(UpdateInput));
		NControllerManager.Instance.Connect(NControllerManager.SignalName.MouseDetected, Callable.From(UpdateInput));
		Connect(CanvasItem.SignalName.VisibilityChanged, Callable.From(UpdateInput));
	}

	private void UpdateInput()
	{
		if (IsVisibleInTree())
		{
			if (NInputManager.remappableMKbInputs.Contains(InputName))
			{
				Key mKbHotkey = NInputManager.Instance.GetMKbHotkey(InputName);
				_mKbBindingLabel.Text = ((mKbHotkey != Key.None) ? mKbHotkey.ToString() : "-");
			}
			else
			{
				_mKbBindingLabel.Text = "-";
			}
			if (NInputManager.remappableKbOnlyInputs.Contains(InputName))
			{
				Key kbOnlyHotkey = NInputManager.Instance.GetKbOnlyHotkey(InputName);
				_keyboardOnlyModeBindingLabel.Text = ((kbOnlyHotkey != Key.None) ? kbOnlyHotkey.ToString() : "-");
			}
			else
			{
				_keyboardOnlyModeBindingLabel.Text = "-";
			}
			_mKbBindingLabel.SelfModulate = ((_mKbBindingLabel.Text == "-") ? StsColors.gray : Colors.White);
			_keyboardOnlyModeBindingLabel.SelfModulate = ((_keyboardOnlyModeBindingLabel.Text == "-") ? StsColors.gray : Colors.White);
			_keyboardOnlyModeBindingLabel.Modulate = ((NControllerManager.Instance.InputType == InputType.KeyboardOnlyMode) ? Colors.White : StsColors.disabledRed);
			_mKbBindingLabel.Modulate = ((NControllerManager.Instance.InputType == InputType.MouseAndKeyboard) ? Colors.White : StsColors.disabledRed);
			if (NInputManager.remappableControllerInputs.Contains(InputName))
			{
				_controllerBindingIcon.Texture = NInputManager.Instance.GetHotkeyIcon(InputName);
				_missingControllerBindingLabel.Visible = false;
			}
			else
			{
				_missingControllerBindingLabel.Visible = true;
			}
			if (!NControllerManager.Instance.ShouldAllowControllerRebinding)
			{
				_controllerBindingIcon.Modulate = StsColors.disabledRed;
			}
			else if (InputName == MegaInput.endTurn)
			{
				_controllerBindingIcon.Modulate = new Color(0.2f, 0.2f, 0.2f);
			}
			else
			{
				_controllerBindingIcon.Modulate = Colors.White;
			}
			if (InputName == MegaInput.endTurn)
			{
				_mKbBindingLabel.Modulate *= new Color(0.6f, 0.6f, 0.6f);
			}
		}
	}

	protected override void OnFocus()
	{
		_tween?.Kill();
		Control bg = _bg;
		Color modulate = _bg.Modulate;
		modulate.A = 0.2f;
		bg.Modulate = modulate;
	}

	protected override void OnUnfocus()
	{
		_tween?.Kill();
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(_bg, "modulate:a", 0f, 0.1);
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		NInputManager.Instance.Disconnect(NInputManager.SignalName.InputRebound, Callable.From(UpdateInput));
		NControllerManager.Instance.Disconnect(NControllerManager.SignalName.ControllerDetected, Callable.From(UpdateInput));
		NControllerManager.Instance.Disconnect(NControllerManager.SignalName.MouseDetected, Callable.From(UpdateInput));
		Disconnect(CanvasItem.SignalName.VisibilityChanged, Callable.From(UpdateInput));
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(6);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "commandName", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateInput, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnfocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 1)
		{
			NInputSettingsEntry from = Create(VariantUtils.ConvertTo<string>(in args[0]));
			ret = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateInput && args.Count == 0)
		{
			UpdateInput();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnFocus && args.Count == 0)
		{
			OnFocus();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnUnfocus && args.Count == 0)
		{
			OnUnfocus();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 1)
		{
			NInputSettingsEntry from = Create(VariantUtils.ConvertTo<string>(in args[0]));
			ret = VariantUtils.CreateFrom(in from);
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.Create)
		{
			return true;
		}
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.UpdateInput)
		{
			return true;
		}
		if (method == MethodName.OnFocus)
		{
			return true;
		}
		if (method == MethodName.OnUnfocus)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.InputName)
		{
			InputName = VariantUtils.ConvertTo<StringName>(in value);
			return true;
		}
		if (name == PropertyName._bg)
		{
			_bg = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._inputLabel)
		{
			_inputLabel = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._mKbBindingLabel)
		{
			_mKbBindingLabel = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._keyboardOnlyModeBindingLabel)
		{
			_keyboardOnlyModeBindingLabel = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._missingControllerBindingLabel)
		{
			_missingControllerBindingLabel = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._controllerBindingIcon)
		{
			_controllerBindingIcon = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._tween)
		{
			_tween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.InputName)
		{
			StringName from = InputName;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName._bg)
		{
			value = VariantUtils.CreateFrom(in _bg);
			return true;
		}
		if (name == PropertyName._inputLabel)
		{
			value = VariantUtils.CreateFrom(in _inputLabel);
			return true;
		}
		if (name == PropertyName._mKbBindingLabel)
		{
			value = VariantUtils.CreateFrom(in _mKbBindingLabel);
			return true;
		}
		if (name == PropertyName._keyboardOnlyModeBindingLabel)
		{
			value = VariantUtils.CreateFrom(in _keyboardOnlyModeBindingLabel);
			return true;
		}
		if (name == PropertyName._missingControllerBindingLabel)
		{
			value = VariantUtils.CreateFrom(in _missingControllerBindingLabel);
			return true;
		}
		if (name == PropertyName._controllerBindingIcon)
		{
			value = VariantUtils.CreateFrom(in _controllerBindingIcon);
			return true;
		}
		if (name == PropertyName._tween)
		{
			value = VariantUtils.CreateFrom(in _tween);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	/// <summary>
	/// Get the property information for all the properties declared in this class.
	/// This method is used by Godot to register the available properties in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.StringName, PropertyName.InputName, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._bg, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._inputLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._mKbBindingLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._keyboardOnlyModeBindingLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._missingControllerBindingLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._controllerBindingIcon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		StringName inputName = PropertyName.InputName;
		StringName from = InputName;
		info.AddProperty(inputName, Variant.From(in from));
		info.AddProperty(PropertyName._bg, Variant.From(in _bg));
		info.AddProperty(PropertyName._inputLabel, Variant.From(in _inputLabel));
		info.AddProperty(PropertyName._mKbBindingLabel, Variant.From(in _mKbBindingLabel));
		info.AddProperty(PropertyName._keyboardOnlyModeBindingLabel, Variant.From(in _keyboardOnlyModeBindingLabel));
		info.AddProperty(PropertyName._missingControllerBindingLabel, Variant.From(in _missingControllerBindingLabel));
		info.AddProperty(PropertyName._controllerBindingIcon, Variant.From(in _controllerBindingIcon));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.InputName, out var value))
		{
			InputName = value.As<StringName>();
		}
		if (info.TryGetProperty(PropertyName._bg, out var value2))
		{
			_bg = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._inputLabel, out var value3))
		{
			_inputLabel = value3.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._mKbBindingLabel, out var value4))
		{
			_mKbBindingLabel = value4.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._keyboardOnlyModeBindingLabel, out var value5))
		{
			_keyboardOnlyModeBindingLabel = value5.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._missingControllerBindingLabel, out var value6))
		{
			_missingControllerBindingLabel = value6.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._controllerBindingIcon, out var value7))
		{
			_controllerBindingIcon = value7.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._tween, out var value8))
		{
			_tween = value8.As<Tween>();
		}
	}
}

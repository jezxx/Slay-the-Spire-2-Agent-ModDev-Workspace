using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
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

[ScriptPath("res://src/Core/Nodes/Screens/Settings/NInputSettingsPanel.cs")]
public class NInputSettingsPanel : NSettingsPanel
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : NSettingsPanel.MethodName
	{
		/// <summary>
		/// Cached name for the '_Ready' method.
		/// </summary>
		public new static readonly StringName _Ready = "_Ready";

		/// <summary>
		/// Cached name for the 'OnViewportSizeChange' method.
		/// </summary>
		public static readonly StringName OnViewportSizeChange = "OnViewportSizeChange";

		/// <summary>
		/// Cached name for the 'OnVisibilityChange' method.
		/// </summary>
		public new static readonly StringName OnVisibilityChange = "OnVisibilityChange";

		/// <summary>
		/// Cached name for the '_ExitTree' method.
		/// </summary>
		public new static readonly StringName _ExitTree = "_ExitTree";

		/// <summary>
		/// Cached name for the 'SetAsListeningEntry' method.
		/// </summary>
		public static readonly StringName SetAsListeningEntry = "SetAsListeningEntry";

		/// <summary>
		/// Cached name for the '_UnhandledKeyInput' method.
		/// </summary>
		public new static readonly StringName _UnhandledKeyInput = "_UnhandledKeyInput";

		/// <summary>
		/// Cached name for the '_Input' method.
		/// </summary>
		public new static readonly StringName _Input = "_Input";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : NSettingsPanel.PropertyName
	{
		/// <summary>
		/// Cached name for the '_minPadding' field.
		/// </summary>
		public new static readonly StringName _minPadding = "_minPadding";

		/// <summary>
		/// Cached name for the '_listeningEntry' field.
		/// </summary>
		public static readonly StringName _listeningEntry = "_listeningEntry";

		/// <summary>
		/// Cached name for the '_kbModeHeader' field.
		/// </summary>
		public static readonly StringName _kbModeHeader = "_kbModeHeader";

		/// <summary>
		/// Cached name for the '_kbModeTickbox' field.
		/// </summary>
		public static readonly StringName _kbModeTickbox = "_kbModeTickbox";

		/// <summary>
		/// Cached name for the '_steamInputPrompt' field.
		/// </summary>
		public static readonly StringName _steamInputPrompt = "_steamInputPrompt";

		/// <summary>
		/// Cached name for the '_resetToDefaultButton' field.
		/// </summary>
		public static readonly StringName _resetToDefaultButton = "_resetToDefaultButton";

		/// <summary>
		/// Cached name for the '_resetLabel' field.
		/// </summary>
		public static readonly StringName _resetLabel = "_resetLabel";

		/// <summary>
		/// Cached name for the '_commandHeader' field.
		/// </summary>
		public static readonly StringName _commandHeader = "_commandHeader";

		/// <summary>
		/// Cached name for the '_mkbHeader' field.
		/// </summary>
		public static readonly StringName _mkbHeader = "_mkbHeader";

		/// <summary>
		/// Cached name for the '_keyboardHeader' field.
		/// </summary>
		public static readonly StringName _keyboardHeader = "_keyboardHeader";

		/// <summary>
		/// Cached name for the '_controllerHeader' field.
		/// </summary>
		public static readonly StringName _controllerHeader = "_controllerHeader";

		/// <summary>
		/// Cached name for the '_listeningPrompt' field.
		/// </summary>
		public static readonly StringName _listeningPrompt = "_listeningPrompt";

		/// <summary>
		/// Cached name for the '_listeningLabel' field.
		/// </summary>
		public static readonly StringName _listeningLabel = "_listeningLabel";

		/// <summary>
		/// Cached name for the '_settingsScreen' field.
		/// </summary>
		public static readonly StringName _settingsScreen = "_settingsScreen";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : NSettingsPanel.SignalName
	{
	}

	private float _minPadding = 50f;

	private NInputSettingsEntry? _listeningEntry;

	private MegaRichTextLabel _kbModeHeader;

	private NTickbox _kbModeTickbox;

	private MegaRichTextLabel _steamInputPrompt;

	private NButton _resetToDefaultButton;

	private MegaLabel _resetLabel;

	private MegaLabel _commandHeader;

	private MegaLabel _mkbHeader;

	private MegaLabel _keyboardHeader;

	private MegaLabel _controllerHeader;

	private Control _listeningPrompt;

	private MegaRichTextLabel _listeningLabel;

	private NSettingsScreen _settingsScreen;

	private LocString CannotRemapLoc => new LocString("settings_ui", "TOAST_CANNOT_REMAP_INPUT");

	private LocString KeyboardOnlyHeaderLoc => new LocString("settings_ui", "INPUT_SETTINGS.KEYBOARD_ONLY_MODE_HEADER");

	private LocString MKbHeaderLoc => new LocString("settings_ui", "INPUT_SETTINGS.MOUSE_KEYBOARD_HEADER");

	private LocString ControllerHeaderLoc => new LocString("settings_ui", "INPUT_SETTINGS.CONTROLLER_HEADER");

	/// <summary>
	/// Nodes are initialized top down based on what you see on the screen.
	/// </summary>
	public override void _Ready()
	{
		base._Ready();
		_settingsScreen = this.GetAncestorOfType<NSettingsScreen>();
		GetViewport().Connect(Viewport.SignalName.SizeChanged, Callable.From(OnViewportSizeChange));
		_kbModeHeader = GetNode<MegaRichTextLabel>("%KeyboardOnlyModeHeader");
		_kbModeHeader.SetTextAutoSize(new LocString("settings_ui", "KEYBOARD_ONLY_MODE_HEADER").GetFormattedText());
		_kbModeTickbox = GetNode<NTickbox>("%KeyboardOnlyModeTickbox");
		_steamInputPrompt = GetNode<MegaRichTextLabel>("%SteamInputPrompt");
		_steamInputPrompt.SetTextAutoSize((!NControllerManager.Instance.ShouldAllowControllerRebinding) ? new LocString("settings_ui", "INPUT_SETTINGS.STEAM_INPUT_DETECTED").GetFormattedText() : new LocString("settings_ui", "INPUT_SETTINGS.STEAM_INPUT_NOT_DETECTED").GetFormattedText());
		_resetToDefaultButton = GetNode<NButton>("%ResetToDefaultButton");
		_resetToDefaultButton.Connect(NClickableControl.SignalName.Released, Callable.From<NClickableControl>(delegate
		{
			NInputManager.Instance.ResetToDefaults();
		}));
		_resetLabel = GetNode<MegaLabel>("%ResetLabel");
		_resetLabel.SetTextAutoSize(new LocString("settings_ui", "INPUT_SETTINGS.RESET_TO_DEFAULT").GetRawText());
		_commandHeader = GetNode<MegaLabel>("%CommandHeader");
		_mkbHeader = GetNode<MegaLabel>("%MKbHeader");
		_keyboardHeader = GetNode<MegaLabel>("%KbModeHeader");
		_controllerHeader = GetNode<MegaLabel>("%ControllerHeader");
		_commandHeader.SetTextAutoSize(new LocString("settings_ui", "INPUT_SETTINGS.COMMAND_HEADER").GetFormattedText());
		_keyboardHeader.SetTextAutoSize(KeyboardOnlyHeaderLoc.GetFormattedText());
		_mkbHeader.SetTextAutoSize(MKbHeaderLoc.GetFormattedText());
		_controllerHeader.SetTextAutoSize(ControllerHeaderLoc.GetFormattedText());
		_listeningPrompt = GetNode<Control>("%ListeningPrompt");
		_listeningLabel = GetNode<MegaRichTextLabel>("%ListeningLabel");
		_listeningLabel.SetTextAutoSize("[sine]" + new LocString("settings_ui", "LISTENING_INPUT").GetRawText() + "[/sine]");
		IReadOnlyList<StringName> readOnlyList = NInputManager.remappableControllerInputs.Concat(NInputManager.remappableMKbInputs).Distinct().ToList();
		foreach (StringName item in readOnlyList)
		{
			NInputSettingsEntry nInputSettingsEntry = NInputSettingsEntry.Create(item);
			nInputSettingsEntry.Connect(NClickableControl.SignalName.Released, Callable.From<NInputSettingsEntry>(SetAsListeningEntry));
			base.Content.AddChildSafely(nInputSettingsEntry);
		}
		UpdateNavigation();
	}

	private async Task RefreshSize()
	{
		await this.AwaitProcessFrame();
		await this.AwaitProcessFrame();
		Vector2 size = GetParent<Control>().Size;
		Vector2 minimumSize = base.Content.GetMinimumSize();
		if (minimumSize.Y + _minPadding >= size.Y)
		{
			base.Size = new Vector2(base.Content.Size.X, minimumSize.Y + size.Y * 0.4f);
		}
	}

	private void OnViewportSizeChange()
	{
		TaskHelper.RunSafely(RefreshSize());
	}

	protected override void OnVisibilityChange()
	{
		base.OnVisibilityChange();
		_listeningEntry = null;
		_listeningPrompt.Visible = false;
		NControllerManager.Instance.StopListeningForRebind();
		TaskHelper.RunSafely(RefreshSize());
	}

	public override void _ExitTree()
	{
		NControllerManager.Instance.StopListeningForRebind();
	}

	private void SetAsListeningEntry(NInputSettingsEntry entry)
	{
		if (NControllerManager.Instance.InputType == InputType.MouseAndKeyboard)
		{
			if (!NInputManager.remappableMKbInputs.Contains(entry.InputName))
			{
				ShowCannotRemapToast(NInputSettingsEntry.commandToLocTitle[entry.InputName], MKbHeaderLoc);
				return;
			}
		}
		else if (NControllerManager.Instance.InputType == InputType.Controller)
		{
			if (!NInputManager.remappableControllerInputs.Contains(entry.InputName))
			{
				ShowCannotRemapToast(NInputSettingsEntry.commandToLocTitle[entry.InputName], ControllerHeaderLoc);
				return;
			}
		}
		else if (!NInputManager.remappableKbOnlyInputs.Contains(entry.InputName))
		{
			ShowCannotRemapToast(NInputSettingsEntry.commandToLocTitle[entry.InputName], KeyboardOnlyHeaderLoc);
			return;
		}
		_listeningEntry = entry;
		_listeningPrompt.Visible = true;
		NControllerManager.Instance.StartListeningForRebind();
	}

	private void ShowCannotRemapToast(string hotkey, LocString controlType)
	{
		LocString locString = new LocString("settings_ui", "INPUT_SETTINGS.INPUT_TITLE." + hotkey);
		LocString cannotRemapLoc = CannotRemapLoc;
		cannotRemapLoc.AddObj("Hotkey", locString.GetFormattedText());
		cannotRemapLoc.AddObj("ControlType", controlType.GetFormattedText());
		_settingsScreen.ShowToast(cannotRemapLoc);
	}

	public override void _UnhandledKeyInput(InputEvent inputEvent)
	{
		if (_listeningEntry == null || !(inputEvent is InputEventKey inputEventKey))
		{
			return;
		}
		if (NControllerManager.Instance.InputType == InputType.Controller)
		{
			GetViewport()?.SetInputAsHandled();
			return;
		}
		if (NControllerManager.Instance.InputType == InputType.KeyboardOnlyMode && NInputManager.remappableKbOnlyInputs.Contains(_listeningEntry.InputName))
		{
			NInputManager.Instance.ModifyKbOnlyKey(_listeningEntry.InputName, inputEventKey.Keycode);
		}
		else if (NInputManager.remappableMKbInputs.Contains(_listeningEntry.InputName))
		{
			NInputManager.Instance.ModifyMKbKey(_listeningEntry.InputName, inputEventKey.Keycode);
		}
		GetViewport()?.SetInputAsHandled();
		_listeningPrompt.Visible = false;
		NControllerManager.Instance.StopListeningForRebind();
		_listeningEntry = null;
	}

	public override void _Input(InputEvent inputEvent)
	{
		if (_listeningEntry == null)
		{
			return;
		}
		StringName[] allControllerInputs = Controller.AllControllerInputs;
		foreach (StringName stringName in allControllerInputs)
		{
			if (inputEvent.IsActionReleased(stringName))
			{
				if (NInputManager.remappableControllerInputs.Contains(_listeningEntry.InputName) && NControllerManager.Instance.ShouldAllowControllerRebinding)
				{
					NInputManager.Instance.ModifyControllerButton(_listeningEntry.InputName, stringName);
				}
				GetViewport()?.SetInputAsHandled();
				_listeningPrompt.Visible = false;
				NControllerManager.Instance.StopListeningForRebind();
				_listeningEntry = null;
				break;
			}
		}
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(7);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnViewportSizeChange, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnVisibilityChange, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetAsListeningEntry, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "entry", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._UnhandledKeyInput, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Input, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnViewportSizeChange && args.Count == 0)
		{
			OnViewportSizeChange();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnVisibilityChange && args.Count == 0)
		{
			OnVisibilityChange();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetAsListeningEntry && args.Count == 1)
		{
			SetAsListeningEntry(VariantUtils.ConvertTo<NInputSettingsEntry>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._UnhandledKeyInput && args.Count == 1)
		{
			_UnhandledKeyInput(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Input && args.Count == 1)
		{
			_Input(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.OnViewportSizeChange)
		{
			return true;
		}
		if (method == MethodName.OnVisibilityChange)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.SetAsListeningEntry)
		{
			return true;
		}
		if (method == MethodName._UnhandledKeyInput)
		{
			return true;
		}
		if (method == MethodName._Input)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._minPadding)
		{
			_minPadding = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._listeningEntry)
		{
			_listeningEntry = VariantUtils.ConvertTo<NInputSettingsEntry>(in value);
			return true;
		}
		if (name == PropertyName._kbModeHeader)
		{
			_kbModeHeader = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._kbModeTickbox)
		{
			_kbModeTickbox = VariantUtils.ConvertTo<NTickbox>(in value);
			return true;
		}
		if (name == PropertyName._steamInputPrompt)
		{
			_steamInputPrompt = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._resetToDefaultButton)
		{
			_resetToDefaultButton = VariantUtils.ConvertTo<NButton>(in value);
			return true;
		}
		if (name == PropertyName._resetLabel)
		{
			_resetLabel = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._commandHeader)
		{
			_commandHeader = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._mkbHeader)
		{
			_mkbHeader = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._keyboardHeader)
		{
			_keyboardHeader = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._controllerHeader)
		{
			_controllerHeader = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._listeningPrompt)
		{
			_listeningPrompt = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._listeningLabel)
		{
			_listeningLabel = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._settingsScreen)
		{
			_settingsScreen = VariantUtils.ConvertTo<NSettingsScreen>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._minPadding)
		{
			value = VariantUtils.CreateFrom(in _minPadding);
			return true;
		}
		if (name == PropertyName._listeningEntry)
		{
			value = VariantUtils.CreateFrom(in _listeningEntry);
			return true;
		}
		if (name == PropertyName._kbModeHeader)
		{
			value = VariantUtils.CreateFrom(in _kbModeHeader);
			return true;
		}
		if (name == PropertyName._kbModeTickbox)
		{
			value = VariantUtils.CreateFrom(in _kbModeTickbox);
			return true;
		}
		if (name == PropertyName._steamInputPrompt)
		{
			value = VariantUtils.CreateFrom(in _steamInputPrompt);
			return true;
		}
		if (name == PropertyName._resetToDefaultButton)
		{
			value = VariantUtils.CreateFrom(in _resetToDefaultButton);
			return true;
		}
		if (name == PropertyName._resetLabel)
		{
			value = VariantUtils.CreateFrom(in _resetLabel);
			return true;
		}
		if (name == PropertyName._commandHeader)
		{
			value = VariantUtils.CreateFrom(in _commandHeader);
			return true;
		}
		if (name == PropertyName._mkbHeader)
		{
			value = VariantUtils.CreateFrom(in _mkbHeader);
			return true;
		}
		if (name == PropertyName._keyboardHeader)
		{
			value = VariantUtils.CreateFrom(in _keyboardHeader);
			return true;
		}
		if (name == PropertyName._controllerHeader)
		{
			value = VariantUtils.CreateFrom(in _controllerHeader);
			return true;
		}
		if (name == PropertyName._listeningPrompt)
		{
			value = VariantUtils.CreateFrom(in _listeningPrompt);
			return true;
		}
		if (name == PropertyName._listeningLabel)
		{
			value = VariantUtils.CreateFrom(in _listeningLabel);
			return true;
		}
		if (name == PropertyName._settingsScreen)
		{
			value = VariantUtils.CreateFrom(in _settingsScreen);
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
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._minPadding, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._listeningEntry, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._kbModeHeader, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._kbModeTickbox, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._steamInputPrompt, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._resetToDefaultButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._resetLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._commandHeader, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._mkbHeader, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._keyboardHeader, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._controllerHeader, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._listeningPrompt, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._listeningLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._settingsScreen, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._minPadding, Variant.From(in _minPadding));
		info.AddProperty(PropertyName._listeningEntry, Variant.From(in _listeningEntry));
		info.AddProperty(PropertyName._kbModeHeader, Variant.From(in _kbModeHeader));
		info.AddProperty(PropertyName._kbModeTickbox, Variant.From(in _kbModeTickbox));
		info.AddProperty(PropertyName._steamInputPrompt, Variant.From(in _steamInputPrompt));
		info.AddProperty(PropertyName._resetToDefaultButton, Variant.From(in _resetToDefaultButton));
		info.AddProperty(PropertyName._resetLabel, Variant.From(in _resetLabel));
		info.AddProperty(PropertyName._commandHeader, Variant.From(in _commandHeader));
		info.AddProperty(PropertyName._mkbHeader, Variant.From(in _mkbHeader));
		info.AddProperty(PropertyName._keyboardHeader, Variant.From(in _keyboardHeader));
		info.AddProperty(PropertyName._controllerHeader, Variant.From(in _controllerHeader));
		info.AddProperty(PropertyName._listeningPrompt, Variant.From(in _listeningPrompt));
		info.AddProperty(PropertyName._listeningLabel, Variant.From(in _listeningLabel));
		info.AddProperty(PropertyName._settingsScreen, Variant.From(in _settingsScreen));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._minPadding, out var value))
		{
			_minPadding = value.As<float>();
		}
		if (info.TryGetProperty(PropertyName._listeningEntry, out var value2))
		{
			_listeningEntry = value2.As<NInputSettingsEntry>();
		}
		if (info.TryGetProperty(PropertyName._kbModeHeader, out var value3))
		{
			_kbModeHeader = value3.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._kbModeTickbox, out var value4))
		{
			_kbModeTickbox = value4.As<NTickbox>();
		}
		if (info.TryGetProperty(PropertyName._steamInputPrompt, out var value5))
		{
			_steamInputPrompt = value5.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._resetToDefaultButton, out var value6))
		{
			_resetToDefaultButton = value6.As<NButton>();
		}
		if (info.TryGetProperty(PropertyName._resetLabel, out var value7))
		{
			_resetLabel = value7.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._commandHeader, out var value8))
		{
			_commandHeader = value8.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._mkbHeader, out var value9))
		{
			_mkbHeader = value9.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._keyboardHeader, out var value10))
		{
			_keyboardHeader = value10.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._controllerHeader, out var value11))
		{
			_controllerHeader = value11.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._listeningPrompt, out var value12))
		{
			_listeningPrompt = value12.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._listeningLabel, out var value13))
		{
			_listeningLabel = value13.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._settingsScreen, out var value14))
		{
			_settingsScreen = value14.As<NSettingsScreen>();
		}
	}
}

using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.CommonUi;

/// <summary>
/// The input glyph we render next to buttons when using a gamepad or we're in keyboard-only mode.
/// Handles logic based on our current control scheme.
/// </summary>
[ScriptPath("res://src/Core/Nodes/CommonUi/NHotkeyIcon.cs")]
public class NHotkeyIcon : Control
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : Control.MethodName
	{
		/// <summary>
		/// Cached name for the '_Ready' method.
		/// </summary>
		public new static readonly StringName _Ready = "_Ready";

		/// <summary>
		/// Cached name for the 'UpdateInput' method.
		/// </summary>
		public static readonly StringName UpdateInput = "UpdateInput";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Control.PropertyName
	{
		/// <summary>
		/// Cached name for the '_controllerIcon' field.
		/// </summary>
		public static readonly StringName _controllerIcon = "_controllerIcon";

		/// <summary>
		/// Cached name for the '_keyboardIcon' field.
		/// </summary>
		public static readonly StringName _keyboardIcon = "_keyboardIcon";

		/// <summary>
		/// Cached name for the '_keyboardHotkeyLabel' field.
		/// </summary>
		public static readonly StringName _keyboardHotkeyLabel = "_keyboardHotkeyLabel";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Control.SignalName
	{
	}

	private TextureRect _controllerIcon;

	private Control _keyboardIcon;

	private MegaLabel _keyboardHotkeyLabel;

	public override void _Ready()
	{
		_controllerIcon = GetNode<TextureRect>("%ControllerIcon");
		_keyboardIcon = GetNode<Control>("%KeyboardIcon");
		_keyboardHotkeyLabel = _keyboardIcon.GetNode<MegaLabel>("%KeyboardLabel");
	}

	public void UpdateInput(string input)
	{
		NControllerManager instance = NControllerManager.Instance;
		if (instance != null)
		{
			Texture2D hotkeyIcon = NInputManager.Instance.GetHotkeyIcon(input);
			if (hotkeyIcon != null)
			{
				_controllerIcon.Texture = hotkeyIcon;
			}
			Key currentHotkey = NInputManager.Instance.GetCurrentHotkey(input);
			switch (currentHotkey)
			{
			case Key.Escape:
				_keyboardHotkeyLabel.Text = "Esc";
				break;
			default:
				_keyboardHotkeyLabel.Text = currentHotkey.ToString();
				break;
			case Key.None:
				break;
			}
			_controllerIcon.Visible = instance.InputType == InputType.Controller;
			_keyboardIcon.Visible = instance.InputType == InputType.KeyboardOnlyMode;
		}
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(2);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateInput, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "input", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
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
		if (method == MethodName.UpdateInput && args.Count == 1)
		{
			UpdateInput(VariantUtils.ConvertTo<string>(in args[0]));
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
		if (method == MethodName.UpdateInput)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._controllerIcon)
		{
			_controllerIcon = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._keyboardIcon)
		{
			_keyboardIcon = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._keyboardHotkeyLabel)
		{
			_keyboardHotkeyLabel = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._controllerIcon)
		{
			value = VariantUtils.CreateFrom(in _controllerIcon);
			return true;
		}
		if (name == PropertyName._keyboardIcon)
		{
			value = VariantUtils.CreateFrom(in _keyboardIcon);
			return true;
		}
		if (name == PropertyName._keyboardHotkeyLabel)
		{
			value = VariantUtils.CreateFrom(in _keyboardHotkeyLabel);
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
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._controllerIcon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._keyboardIcon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._keyboardHotkeyLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._controllerIcon, Variant.From(in _controllerIcon));
		info.AddProperty(PropertyName._keyboardIcon, Variant.From(in _keyboardIcon));
		info.AddProperty(PropertyName._keyboardHotkeyLabel, Variant.From(in _keyboardHotkeyLabel));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._controllerIcon, out var value))
		{
			_controllerIcon = value.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._keyboardIcon, out var value2))
		{
			_keyboardIcon = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._keyboardHotkeyLabel, out var value3))
		{
			_keyboardHotkeyLabel = value3.As<MegaLabel>();
		}
	}
}

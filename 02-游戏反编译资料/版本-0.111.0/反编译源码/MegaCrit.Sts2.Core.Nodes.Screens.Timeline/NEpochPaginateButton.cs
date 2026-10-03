using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Timeline;

[ScriptPath("res://src/Core/Nodes/Screens/Timeline/NEpochPaginateButton.cs")]
public class NEpochPaginateButton : NGoldArrowButton
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : NGoldArrowButton.MethodName
	{
		/// <summary>
		/// Cached name for the 'OnDisable' method.
		/// </summary>
		public new static readonly StringName OnDisable = "OnDisable";

		/// <summary>
		/// Cached name for the 'OnEnable' method.
		/// </summary>
		public new static readonly StringName OnEnable = "OnEnable";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : NGoldArrowButton.PropertyName
	{
		/// <summary>
		/// Cached name for the 'IsLeft' property.
		/// </summary>
		public static readonly StringName IsLeft = "IsLeft";

		/// <summary>
		/// Cached name for the 'Hotkeys' property.
		/// </summary>
		public new static readonly StringName Hotkeys = "Hotkeys";

		/// <summary>
		/// Cached name for the 'ClickedSfx' property.
		/// </summary>
		public new static readonly StringName ClickedSfx = "ClickedSfx";

		/// <summary>
		/// Cached name for the '_isLeft' field.
		/// </summary>
		public static readonly StringName _isLeft = "_isLeft";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : NGoldArrowButton.SignalName
	{
	}

	private bool _isLeft;

	/// <summary>
	/// Whether this arrow is facing left or right
	/// </summary>
	public bool IsLeft
	{
		get
		{
			return _isLeft;
		}
		set
		{
			if (_isLeft != value)
			{
				UnregisterHotkeys();
				_isLeft = value;
				RegisterHotkeys();
				UpdateControllerButton();
			}
		}
	}

	protected override string[] Hotkeys => new string[1] { _isLeft ? MegaInput.left : MegaInput.right };

	protected override string ClickedSfx => "event:/sfx/ui/timeline/ui_timeline_click";

	protected override void OnDisable()
	{
		base.OnDisable();
		base.Visible = false;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		base.Visible = true;
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(2);
		list.Add(new MethodInfo(MethodName.OnDisable, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnEnable, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.OnDisable && args.Count == 0)
		{
			OnDisable();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnEnable && args.Count == 0)
		{
			OnEnable();
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.OnDisable)
		{
			return true;
		}
		if (method == MethodName.OnEnable)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.IsLeft)
		{
			IsLeft = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._isLeft)
		{
			_isLeft = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.IsLeft)
		{
			bool from = IsLeft;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.Hotkeys)
		{
			string[] from2 = Hotkeys;
			value = VariantUtils.CreateFrom(in from2);
			return true;
		}
		if (name == PropertyName.ClickedSfx)
		{
			string from3 = ClickedSfx;
			value = VariantUtils.CreateFrom(in from3);
			return true;
		}
		if (name == PropertyName._isLeft)
		{
			value = VariantUtils.CreateFrom(in _isLeft);
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
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isLeft, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.IsLeft, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.PackedStringArray, PropertyName.Hotkeys, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.String, PropertyName.ClickedSfx, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		StringName isLeft = PropertyName.IsLeft;
		bool from = IsLeft;
		info.AddProperty(isLeft, Variant.From(in from));
		info.AddProperty(PropertyName._isLeft, Variant.From(in _isLeft));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.IsLeft, out var value))
		{
			IsLeft = value.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._isLeft, out var value2))
		{
			_isLeft = value2.As<bool>();
		}
	}
}

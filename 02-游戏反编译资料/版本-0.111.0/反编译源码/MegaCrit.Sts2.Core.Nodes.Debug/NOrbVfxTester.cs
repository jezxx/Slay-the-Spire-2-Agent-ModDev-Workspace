using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Nodes.Orbs;

namespace MegaCrit.Sts2.Core.Nodes.Debug;

[ScriptPath("res://src/Core/Nodes/Debug/NOrbVfxTester.cs")]
public class NOrbVfxTester : Control
{
	private enum OrbVfxTestModelType
	{
		Lightning,
		Dark,
		Frost,
		Glass,
		Plasma
	}

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
		/// Cached name for the '_Input' method.
		/// </summary>
		public new static readonly StringName _Input = "_Input";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Control.PropertyName
	{
		/// <summary>
		/// Cached name for the '_testModelType' field.
		/// </summary>
		public static readonly StringName _testModelType = "_testModelType";

		/// <summary>
		/// Cached name for the '_orbVfx' field.
		/// </summary>
		public static readonly StringName _orbVfx = "_orbVfx";

		/// <summary>
		/// Cached name for the '_basePassiveValue' field.
		/// </summary>
		public static readonly StringName _basePassiveValue = "_basePassiveValue";

		/// <summary>
		/// Cached name for the '_baseEvokeValue' field.
		/// </summary>
		public static readonly StringName _baseEvokeValue = "_baseEvokeValue";

		/// <summary>
		/// Cached name for the '_passiveIncrements' field.
		/// </summary>
		public static readonly StringName _passiveIncrements = "_passiveIncrements";

		/// <summary>
		/// Cached name for the '_playerCenter' field.
		/// </summary>
		public static readonly StringName _playerCenter = "_playerCenter";

		/// <summary>
		/// Cached name for the '_target' field.
		/// </summary>
		public static readonly StringName _target = "_target";

		/// <summary>
		/// Cached name for the '_combatVfxContainer' field.
		/// </summary>
		public static readonly StringName _combatVfxContainer = "_combatVfxContainer";

		/// <summary>
		/// Cached name for the '_isFocused' field.
		/// </summary>
		public static readonly StringName _isFocused = "_isFocused";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Control.SignalName
	{
	}

	[Export(PropertyHint.None, "")]
	private OrbVfxTestModelType _testModelType;

	[Export(PropertyHint.None, "")]
	private NOrbVfx _orbVfx;

	[Export(PropertyHint.None, "")]
	private float _basePassiveValue = 4f;

	[Export(PropertyHint.None, "")]
	private float _baseEvokeValue = 4f;

	[Export(PropertyHint.None, "")]
	private float _passiveIncrements;

	[Export(PropertyHint.None, "")]
	private Node2D _playerCenter;

	[Export(PropertyHint.None, "")]
	private Node2D _target;

	[Export(PropertyHint.None, "")]
	private Control _combatVfxContainer;

	private bool _isFocused;

	private decimal _passiveVal;

	private decimal _evokeVal;

	public override void _Ready()
	{
		_passiveVal = (decimal)_basePassiveValue;
		_evokeVal = (decimal)_baseEvokeValue;
		_orbVfx.SetOverrideCombatVfxContainer(_combatVfxContainer);
		_orbVfx.SetOverridePlayerNode(_playerCenter);
	}

	public override void _Input(InputEvent inputEvent)
	{
		if (!base.Visible)
		{
			return;
		}
		base._Input(inputEvent);
		if (inputEvent is InputEventKey inputEventKey && inputEventKey.Keycode == Key.A && inputEventKey.Pressed)
		{
			if (_testModelType == OrbVfxTestModelType.Dark)
			{
				_evokeVal += _passiveVal;
			}
			_orbVfx.OnPassiveActivated(_passiveVal, _evokeVal);
			if (_orbVfx is NGlassOrbVfx && _passiveVal > 0m)
			{
				(_orbVfx as NGlassOrbVfx).ShowPassiveImpact(new Vector2[1] { _target.GlobalPosition });
			}
			if (_testModelType == OrbVfxTestModelType.Glass)
			{
				_passiveVal = Math.Clamp(_passiveVal - 1m, 0m, (decimal)_basePassiveValue);
			}
			_orbVfx.AfterPassiveActivated(_passiveVal, _evokeVal);
		}
		if (inputEvent is InputEventKey inputEventKey2 && inputEventKey2.Keycode == Key.Z && inputEventKey2.Pressed)
		{
			_passiveVal = (decimal)_basePassiveValue;
			_evokeVal = (decimal)_baseEvokeValue;
			_orbVfx.OnPassiveActivated(0m, _evokeVal);
			_orbVfx.Modulate = new Color(1f, 1f, 1f);
		}
		if (inputEvent is InputEventKey inputEventKey3 && inputEventKey3.Keycode == Key.S && inputEventKey3.Pressed)
		{
			_isFocused = !_isFocused;
			_orbVfx.SetForcedFocusPower(_isFocused);
		}
		if (inputEvent is InputEventKey inputEventKey4 && inputEventKey4.Keycode == Key.D && inputEventKey4.Pressed)
		{
			_orbVfx.OnEvoke(new Vector2[1] { (_testModelType == OrbVfxTestModelType.Frost || _testModelType == OrbVfxTestModelType.Plasma) ? _playerCenter.Position : _target.Position });
			_orbVfx.Modulate = new Color(1f, 1f, 1f, 0f);
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
		if (name == PropertyName._testModelType)
		{
			_testModelType = VariantUtils.ConvertTo<OrbVfxTestModelType>(in value);
			return true;
		}
		if (name == PropertyName._orbVfx)
		{
			_orbVfx = VariantUtils.ConvertTo<NOrbVfx>(in value);
			return true;
		}
		if (name == PropertyName._basePassiveValue)
		{
			_basePassiveValue = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._baseEvokeValue)
		{
			_baseEvokeValue = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._passiveIncrements)
		{
			_passiveIncrements = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._playerCenter)
		{
			_playerCenter = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._target)
		{
			_target = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._combatVfxContainer)
		{
			_combatVfxContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._isFocused)
		{
			_isFocused = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._testModelType)
		{
			value = VariantUtils.CreateFrom(in _testModelType);
			return true;
		}
		if (name == PropertyName._orbVfx)
		{
			value = VariantUtils.CreateFrom(in _orbVfx);
			return true;
		}
		if (name == PropertyName._basePassiveValue)
		{
			value = VariantUtils.CreateFrom(in _basePassiveValue);
			return true;
		}
		if (name == PropertyName._baseEvokeValue)
		{
			value = VariantUtils.CreateFrom(in _baseEvokeValue);
			return true;
		}
		if (name == PropertyName._passiveIncrements)
		{
			value = VariantUtils.CreateFrom(in _passiveIncrements);
			return true;
		}
		if (name == PropertyName._playerCenter)
		{
			value = VariantUtils.CreateFrom(in _playerCenter);
			return true;
		}
		if (name == PropertyName._target)
		{
			value = VariantUtils.CreateFrom(in _target);
			return true;
		}
		if (name == PropertyName._combatVfxContainer)
		{
			value = VariantUtils.CreateFrom(in _combatVfxContainer);
			return true;
		}
		if (name == PropertyName._isFocused)
		{
			value = VariantUtils.CreateFrom(in _isFocused);
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
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._testModelType, PropertyHint.Enum, "Lightning,Dark,Frost,Glass,Plasma", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._orbVfx, PropertyHint.NodeType, "Node2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._basePassiveValue, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._baseEvokeValue, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._passiveIncrements, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._playerCenter, PropertyHint.NodeType, "Node2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._target, PropertyHint.NodeType, "Node2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._combatVfxContainer, PropertyHint.NodeType, "Control", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isFocused, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._testModelType, Variant.From(in _testModelType));
		info.AddProperty(PropertyName._orbVfx, Variant.From(in _orbVfx));
		info.AddProperty(PropertyName._basePassiveValue, Variant.From(in _basePassiveValue));
		info.AddProperty(PropertyName._baseEvokeValue, Variant.From(in _baseEvokeValue));
		info.AddProperty(PropertyName._passiveIncrements, Variant.From(in _passiveIncrements));
		info.AddProperty(PropertyName._playerCenter, Variant.From(in _playerCenter));
		info.AddProperty(PropertyName._target, Variant.From(in _target));
		info.AddProperty(PropertyName._combatVfxContainer, Variant.From(in _combatVfxContainer));
		info.AddProperty(PropertyName._isFocused, Variant.From(in _isFocused));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._testModelType, out var value))
		{
			_testModelType = value.As<OrbVfxTestModelType>();
		}
		if (info.TryGetProperty(PropertyName._orbVfx, out var value2))
		{
			_orbVfx = value2.As<NOrbVfx>();
		}
		if (info.TryGetProperty(PropertyName._basePassiveValue, out var value3))
		{
			_basePassiveValue = value3.As<float>();
		}
		if (info.TryGetProperty(PropertyName._baseEvokeValue, out var value4))
		{
			_baseEvokeValue = value4.As<float>();
		}
		if (info.TryGetProperty(PropertyName._passiveIncrements, out var value5))
		{
			_passiveIncrements = value5.As<float>();
		}
		if (info.TryGetProperty(PropertyName._playerCenter, out var value6))
		{
			_playerCenter = value6.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._target, out var value7))
		{
			_target = value7.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._combatVfxContainer, out var value8))
		{
			_combatVfxContainer = value8.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._isFocused, out var value9))
		{
			_isFocused = value9.As<bool>();
		}
	}
}

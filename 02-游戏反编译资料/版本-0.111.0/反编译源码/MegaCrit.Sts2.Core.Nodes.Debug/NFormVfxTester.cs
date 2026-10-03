using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Nodes.Vfx.Forms;

namespace MegaCrit.Sts2.Core.Nodes.Debug;

[ScriptPath("res://src/Core/Nodes/Debug/NFormVfxTester.cs")]
public class NFormVfxTester : Node2D
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : Node2D.MethodName
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
	public new class PropertyName : Node2D.PropertyName
	{
		/// <summary>
		/// Cached name for the '_formVfx' field.
		/// </summary>
		public static readonly StringName _formVfx = "_formVfx";

		/// <summary>
		/// Cached name for the '_testSpine' field.
		/// </summary>
		public static readonly StringName _testSpine = "_testSpine";

		/// <summary>
		/// Cached name for the '_testBoneName' field.
		/// </summary>
		public static readonly StringName _testBoneName = "_testBoneName";

		/// <summary>
		/// Cached name for the '_testActiveState' field.
		/// </summary>
		public static readonly StringName _testActiveState = "_testActiveState";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Node2D.SignalName
	{
	}

	[Export(PropertyHint.None, "")]
	private NFormVfx _formVfx;

	[Export(PropertyHint.None, "")]
	private Node2D _testSpine;

	[Export(PropertyHint.None, "")]
	private string _testBoneName = "";

	private bool _testActiveState = true;

	public override void _Ready()
	{
		_formVfx.ForceTestBoneName(_testBoneName);
		_formVfx.ForceSetSpineSprite(_testSpine);
		_formVfx.SetActive(_testActiveState);
	}

	public override void _Input(InputEvent inputEvent)
	{
		if (base.Visible)
		{
			base._Input(inputEvent);
			if (inputEvent is InputEventKey inputEventKey && inputEventKey.Keycode == Key.A && inputEventKey.Pressed)
			{
				_testActiveState = !_testActiveState;
				_formVfx.SetActive(_testActiveState);
			}
			if (inputEvent is InputEventKey inputEventKey2 && inputEventKey2.Keycode == Key.S && inputEventKey2.Pressed)
			{
				_formVfx.OnEffectTriggered();
			}
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
		if (name == PropertyName._formVfx)
		{
			_formVfx = VariantUtils.ConvertTo<NFormVfx>(in value);
			return true;
		}
		if (name == PropertyName._testSpine)
		{
			_testSpine = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._testBoneName)
		{
			_testBoneName = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		if (name == PropertyName._testActiveState)
		{
			_testActiveState = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._formVfx)
		{
			value = VariantUtils.CreateFrom(in _formVfx);
			return true;
		}
		if (name == PropertyName._testSpine)
		{
			value = VariantUtils.CreateFrom(in _testSpine);
			return true;
		}
		if (name == PropertyName._testBoneName)
		{
			value = VariantUtils.CreateFrom(in _testBoneName);
			return true;
		}
		if (name == PropertyName._testActiveState)
		{
			value = VariantUtils.CreateFrom(in _testActiveState);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._formVfx, PropertyHint.NodeType, "Node2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._testSpine, PropertyHint.NodeType, "Node2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.String, PropertyName._testBoneName, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._testActiveState, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._formVfx, Variant.From(in _formVfx));
		info.AddProperty(PropertyName._testSpine, Variant.From(in _testSpine));
		info.AddProperty(PropertyName._testBoneName, Variant.From(in _testBoneName));
		info.AddProperty(PropertyName._testActiveState, Variant.From(in _testActiveState));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._formVfx, out var value))
		{
			_formVfx = value.As<NFormVfx>();
		}
		if (info.TryGetProperty(PropertyName._testSpine, out var value2))
		{
			_testSpine = value2.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._testBoneName, out var value3))
		{
			_testBoneName = value3.As<string>();
		}
		if (info.TryGetProperty(PropertyName._testActiveState, out var value4))
		{
			_testActiveState = value4.As<bool>();
		}
	}
}

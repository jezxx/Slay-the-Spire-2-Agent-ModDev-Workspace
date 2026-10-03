using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;

namespace MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;

[ScriptPath("res://src/Core/Nodes/Vfx/Utilities/NValueRamp.cs")]
public class NValueRamp : Node
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : Node.MethodName
	{
		/// <summary>
		/// Cached name for the 'SetIncreasing' method.
		/// </summary>
		public static readonly StringName SetIncreasing = "SetIncreasing";

		/// <summary>
		/// Cached name for the 'ForceValue' method.
		/// </summary>
		public static readonly StringName ForceValue = "ForceValue";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Node.PropertyName
	{
		/// <summary>
		/// Cached name for the '_rampSpeed' field.
		/// </summary>
		public static readonly StringName _rampSpeed = "_rampSpeed";

		/// <summary>
		/// Cached name for the '_rampCurve' field.
		/// </summary>
		public static readonly StringName _rampCurve = "_rampCurve";

		/// <summary>
		/// Cached name for the '_currentValue' field.
		/// </summary>
		public static readonly StringName _currentValue = "_currentValue";

		/// <summary>
		/// Cached name for the '_isIncreasing' field.
		/// </summary>
		public static readonly StringName _isIncreasing = "_isIncreasing";

		/// <summary>
		/// Cached name for the '_didForceValueThisFrame' field.
		/// </summary>
		public static readonly StringName _didForceValueThisFrame = "_didForceValueThisFrame";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Node.SignalName
	{
	}

	[Export(PropertyHint.None, "")]
	private float _rampSpeed = 1f;

	[Export(PropertyHint.None, "")]
	private Curve _rampCurve;

	private float _currentValue;

	private bool _isIncreasing;

	private bool _didForceValueThisFrame = true;

	public bool TryProcess(double delta, out float returnValue)
	{
		bool didForceValueThisFrame = _didForceValueThisFrame;
		_didForceValueThisFrame = false;
		float num = (float)delta * _rampSpeed * (_isIncreasing ? 1f : (-1f));
		bool flag = ((num > 0f) ? (_currentValue >= 1f) : (!(num < 0f) || _currentValue <= 0f));
		bool flag2 = flag;
		if (!didForceValueThisFrame && flag2)
		{
			returnValue = 0f;
			return false;
		}
		_currentValue = Mathf.Clamp(_currentValue + num, 0f, 1f);
		returnValue = _rampCurve.Sample(_currentValue);
		return true;
	}

	public void SetIncreasing(bool isIncreasing)
	{
		_isIncreasing = isIncreasing;
	}

	public void ForceValue(float forcedValue)
	{
		_currentValue = forcedValue;
		_didForceValueThisFrame = true;
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
		list.Add(new MethodInfo(MethodName.SetIncreasing, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "isIncreasing", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ForceValue, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "forcedValue", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.SetIncreasing && args.Count == 1)
		{
			SetIncreasing(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ForceValue && args.Count == 1)
		{
			ForceValue(VariantUtils.ConvertTo<float>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.SetIncreasing)
		{
			return true;
		}
		if (method == MethodName.ForceValue)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._rampSpeed)
		{
			_rampSpeed = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._rampCurve)
		{
			_rampCurve = VariantUtils.ConvertTo<Curve>(in value);
			return true;
		}
		if (name == PropertyName._currentValue)
		{
			_currentValue = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._isIncreasing)
		{
			_isIncreasing = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._didForceValueThisFrame)
		{
			_didForceValueThisFrame = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._rampSpeed)
		{
			value = VariantUtils.CreateFrom(in _rampSpeed);
			return true;
		}
		if (name == PropertyName._rampCurve)
		{
			value = VariantUtils.CreateFrom(in _rampCurve);
			return true;
		}
		if (name == PropertyName._currentValue)
		{
			value = VariantUtils.CreateFrom(in _currentValue);
			return true;
		}
		if (name == PropertyName._isIncreasing)
		{
			value = VariantUtils.CreateFrom(in _isIncreasing);
			return true;
		}
		if (name == PropertyName._didForceValueThisFrame)
		{
			value = VariantUtils.CreateFrom(in _didForceValueThisFrame);
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
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._rampSpeed, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._rampCurve, PropertyHint.ResourceType, "Curve", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._currentValue, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isIncreasing, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._didForceValueThisFrame, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._rampSpeed, Variant.From(in _rampSpeed));
		info.AddProperty(PropertyName._rampCurve, Variant.From(in _rampCurve));
		info.AddProperty(PropertyName._currentValue, Variant.From(in _currentValue));
		info.AddProperty(PropertyName._isIncreasing, Variant.From(in _isIncreasing));
		info.AddProperty(PropertyName._didForceValueThisFrame, Variant.From(in _didForceValueThisFrame));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._rampSpeed, out var value))
		{
			_rampSpeed = value.As<float>();
		}
		if (info.TryGetProperty(PropertyName._rampCurve, out var value2))
		{
			_rampCurve = value2.As<Curve>();
		}
		if (info.TryGetProperty(PropertyName._currentValue, out var value3))
		{
			_currentValue = value3.As<float>();
		}
		if (info.TryGetProperty(PropertyName._isIncreasing, out var value4))
		{
			_isIncreasing = value4.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._didForceValueThisFrame, out var value5))
		{
			_didForceValueThisFrame = value5.As<bool>();
		}
	}
}

using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;

namespace MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;

[ScriptPath("res://src/Core/Nodes/Vfx/Utilities/NShaker.cs")]
public class NShaker : Node
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : Node.MethodName
	{
		/// <summary>
		/// Cached name for the '_Ready' method.
		/// </summary>
		public new static readonly StringName _Ready = "_Ready";

		/// <summary>
		/// Cached name for the '_Process' method.
		/// </summary>
		public new static readonly StringName _Process = "_Process";

		/// <summary>
		/// Cached name for the 'SetTargetTransform' method.
		/// </summary>
		public static readonly StringName SetTargetTransform = "SetTargetTransform";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Node.PropertyName
	{
		/// <summary>
		/// Cached name for the 'Strength' property.
		/// </summary>
		public static readonly StringName Strength = "Strength";

		/// <summary>
		/// Cached name for the '_target' field.
		/// </summary>
		public static readonly StringName _target = "_target";

		/// <summary>
		/// Cached name for the '_maxPosOffset' field.
		/// </summary>
		public static readonly StringName _maxPosOffset = "_maxPosOffset";

		/// <summary>
		/// Cached name for the '_maxRotOffset' field.
		/// </summary>
		public static readonly StringName _maxRotOffset = "_maxRotOffset";

		/// <summary>
		/// Cached name for the '_frequency' field.
		/// </summary>
		public static readonly StringName _frequency = "_frequency";

		/// <summary>
		/// Cached name for the '_strength' field.
		/// </summary>
		public static readonly StringName _strength = "_strength";

		/// <summary>
		/// Cached name for the '_timer' field.
		/// </summary>
		public static readonly StringName _timer = "_timer";

		/// <summary>
		/// Cached name for the '_previousShakePos' field.
		/// </summary>
		public static readonly StringName _previousShakePos = "_previousShakePos";

		/// <summary>
		/// Cached name for the '_previousShakeRot' field.
		/// </summary>
		public static readonly StringName _previousShakeRot = "_previousShakeRot";

		/// <summary>
		/// Cached name for the '_shakePos' field.
		/// </summary>
		public static readonly StringName _shakePos = "_shakePos";

		/// <summary>
		/// Cached name for the '_shakeRot' field.
		/// </summary>
		public static readonly StringName _shakeRot = "_shakeRot";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Node.SignalName
	{
	}

	[Export(PropertyHint.None, "")]
	private Node2D? _target;

	[Export(PropertyHint.None, "")]
	private float _maxPosOffset;

	[Export(PropertyHint.None, "")]
	private float _maxRotOffset;

	[Export(PropertyHint.None, "")]
	private float _frequency;

	[Export(PropertyHint.None, "")]
	private float _strength;

	private float _timer;

	private Vector2 _previousShakePos;

	private float _previousShakeRot;

	private Vector2 _shakePos;

	private float _shakeRot;

	public float Strength
	{
		get
		{
			return _strength;
		}
		set
		{
			_strength = value;
		}
	}

	public override void _Ready()
	{
		base._Ready();
		_timer = 0f;
		_previousShakePos = Vector2.Zero;
		_previousShakeRot = 0f;
		_shakePos = Vector2.Zero;
		_shakeRot = 0f;
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		if (_target != null)
		{
			if (_strength == 0f)
			{
				_target.Position = Vector2.Zero;
				_target.RotationDegrees = 0f;
			}
			else if (_timer > 0f)
			{
				_timer = Mathf.Clamp(_timer - (float)delta, 0f, _frequency);
				float weight = 1f - _timer / _frequency;
				Vector2 position = _previousShakePos.Lerp(_shakePos, weight);
				float rotation = Mathf.Lerp(_previousShakeRot, _shakeRot, weight);
				SetTargetTransform(position, rotation);
			}
			else
			{
				_timer = _frequency;
				_previousShakePos = _shakePos;
				_previousShakeRot = _shakeRot;
				float s = Mathf.DegToRad(GD.Randf() * 360f);
				float num = GD.Randf() * _maxPosOffset;
				Vector2 vector = new Vector2(Mathf.Cos(s), Mathf.Sin(s)).Normalized();
				_shakePos = vector * num * _strength;
				_shakeRot = (float)GD.RandRange(0f - _maxRotOffset, _maxRotOffset);
			}
		}
	}

	private void SetTargetTransform(Vector2 position, float rotation)
	{
		if (_target != null)
		{
			_target.Position = position;
			_target.RotationDegrees = rotation;
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
		List<MethodInfo> list = new List<MethodInfo>(3);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Process, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SetTargetTransform, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Vector2, "position", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Float, "rotation", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
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
		if (method == MethodName._Process && args.Count == 1)
		{
			_Process(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetTargetTransform && args.Count == 2)
		{
			SetTargetTransform(VariantUtils.ConvertTo<Vector2>(in args[0]), VariantUtils.ConvertTo<float>(in args[1]));
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
		if (method == MethodName._Process)
		{
			return true;
		}
		if (method == MethodName.SetTargetTransform)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.Strength)
		{
			Strength = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._target)
		{
			_target = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._maxPosOffset)
		{
			_maxPosOffset = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._maxRotOffset)
		{
			_maxRotOffset = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._frequency)
		{
			_frequency = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._strength)
		{
			_strength = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._timer)
		{
			_timer = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._previousShakePos)
		{
			_previousShakePos = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._previousShakeRot)
		{
			_previousShakeRot = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._shakePos)
		{
			_shakePos = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._shakeRot)
		{
			_shakeRot = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.Strength)
		{
			float from = Strength;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName._target)
		{
			value = VariantUtils.CreateFrom(in _target);
			return true;
		}
		if (name == PropertyName._maxPosOffset)
		{
			value = VariantUtils.CreateFrom(in _maxPosOffset);
			return true;
		}
		if (name == PropertyName._maxRotOffset)
		{
			value = VariantUtils.CreateFrom(in _maxRotOffset);
			return true;
		}
		if (name == PropertyName._frequency)
		{
			value = VariantUtils.CreateFrom(in _frequency);
			return true;
		}
		if (name == PropertyName._strength)
		{
			value = VariantUtils.CreateFrom(in _strength);
			return true;
		}
		if (name == PropertyName._timer)
		{
			value = VariantUtils.CreateFrom(in _timer);
			return true;
		}
		if (name == PropertyName._previousShakePos)
		{
			value = VariantUtils.CreateFrom(in _previousShakePos);
			return true;
		}
		if (name == PropertyName._previousShakeRot)
		{
			value = VariantUtils.CreateFrom(in _previousShakeRot);
			return true;
		}
		if (name == PropertyName._shakePos)
		{
			value = VariantUtils.CreateFrom(in _shakePos);
			return true;
		}
		if (name == PropertyName._shakeRot)
		{
			value = VariantUtils.CreateFrom(in _shakeRot);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._target, PropertyHint.NodeType, "Node2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._maxPosOffset, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._maxRotOffset, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._frequency, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._strength, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._timer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._previousShakePos, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._previousShakeRot, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._shakePos, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._shakeRot, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName.Strength, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		StringName strength = PropertyName.Strength;
		float from = Strength;
		info.AddProperty(strength, Variant.From(in from));
		info.AddProperty(PropertyName._target, Variant.From(in _target));
		info.AddProperty(PropertyName._maxPosOffset, Variant.From(in _maxPosOffset));
		info.AddProperty(PropertyName._maxRotOffset, Variant.From(in _maxRotOffset));
		info.AddProperty(PropertyName._frequency, Variant.From(in _frequency));
		info.AddProperty(PropertyName._strength, Variant.From(in _strength));
		info.AddProperty(PropertyName._timer, Variant.From(in _timer));
		info.AddProperty(PropertyName._previousShakePos, Variant.From(in _previousShakePos));
		info.AddProperty(PropertyName._previousShakeRot, Variant.From(in _previousShakeRot));
		info.AddProperty(PropertyName._shakePos, Variant.From(in _shakePos));
		info.AddProperty(PropertyName._shakeRot, Variant.From(in _shakeRot));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.Strength, out var value))
		{
			Strength = value.As<float>();
		}
		if (info.TryGetProperty(PropertyName._target, out var value2))
		{
			_target = value2.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._maxPosOffset, out var value3))
		{
			_maxPosOffset = value3.As<float>();
		}
		if (info.TryGetProperty(PropertyName._maxRotOffset, out var value4))
		{
			_maxRotOffset = value4.As<float>();
		}
		if (info.TryGetProperty(PropertyName._frequency, out var value5))
		{
			_frequency = value5.As<float>();
		}
		if (info.TryGetProperty(PropertyName._strength, out var value6))
		{
			_strength = value6.As<float>();
		}
		if (info.TryGetProperty(PropertyName._timer, out var value7))
		{
			_timer = value7.As<float>();
		}
		if (info.TryGetProperty(PropertyName._previousShakePos, out var value8))
		{
			_previousShakePos = value8.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._previousShakeRot, out var value9))
		{
			_previousShakeRot = value9.As<float>();
		}
		if (info.TryGetProperty(PropertyName._shakePos, out var value10))
		{
			_shakePos = value10.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._shakeRot, out var value11))
		{
			_shakeRot = value11.As<float>();
		}
	}
}

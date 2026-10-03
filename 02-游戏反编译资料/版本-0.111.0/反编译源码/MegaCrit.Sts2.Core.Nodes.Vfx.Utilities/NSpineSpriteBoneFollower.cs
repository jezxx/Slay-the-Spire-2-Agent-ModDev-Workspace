using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;

namespace MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;

[ScriptPath("res://src/Core/Nodes/Vfx/Utilities/NSpineSpriteBoneFollower.cs")]
public class NSpineSpriteBoneFollower : Node2D
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
		/// Cached name for the 'SetSpineSprite' method.
		/// </summary>
		public static readonly StringName SetSpineSprite = "SetSpineSprite";

		/// <summary>
		/// Cached name for the '_Process' method.
		/// </summary>
		public new static readonly StringName _Process = "_Process";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Node2D.PropertyName
	{
		/// <summary>
		/// Cached name for the '_target' field.
		/// </summary>
		public static readonly StringName _target = "_target";

		/// <summary>
		/// Cached name for the '_boneName' field.
		/// </summary>
		public static readonly StringName _boneName = "_boneName";

		/// <summary>
		/// Cached name for the '_snap' field.
		/// </summary>
		public static readonly StringName _snap = "_snap";

		/// <summary>
		/// Cached name for the '_interpolationSpeed' field.
		/// </summary>
		public static readonly StringName _interpolationSpeed = "_interpolationSpeed";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Node2D.SignalName
	{
	}

	[Export(PropertyHint.None, "")]
	private Node2D _target;

	[Export(PropertyHint.None, "")]
	private string _boneName = "";

	[Export(PropertyHint.None, "")]
	private bool _snap;

	[Export(PropertyHint.None, "")]
	private float _interpolationSpeed = 0.5f;

	private MegaSprite? _targetSprite;

	public override void _Ready()
	{
	}

	public void SetSpineSprite(Node2D target, string boneName)
	{
		SetSpineSprite(new MegaSprite(target), boneName);
	}

	public void SetSpineSprite(MegaSprite spineSprite, string boneName)
	{
		_targetSprite = spineSprite;
		_boneName = boneName;
	}

	public override void _Process(double delta)
	{
		if (_targetSprite == null || string.IsNullOrEmpty(_boneName))
		{
			return;
		}
		Transform2D? globalBoneTransform = _targetSprite.GetGlobalBoneTransform(_boneName);
		if (globalBoneTransform.HasValue)
		{
			if (_snap)
			{
				base.GlobalPosition = globalBoneTransform.Value.Origin;
			}
			else
			{
				base.GlobalPosition = base.GlobalPosition.Lerp(globalBoneTransform.Value.Origin, _interpolationSpeed);
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
		List<MethodInfo> list = new List<MethodInfo>(3);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetSpineSprite, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "target", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false),
			new PropertyInfo(Variant.Type.String, "boneName", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Process, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
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
		if (method == MethodName.SetSpineSprite && args.Count == 2)
		{
			SetSpineSprite(VariantUtils.ConvertTo<Node2D>(in args[0]), VariantUtils.ConvertTo<string>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Process && args.Count == 1)
		{
			_Process(VariantUtils.ConvertTo<double>(in args[0]));
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
		if (method == MethodName.SetSpineSprite)
		{
			return true;
		}
		if (method == MethodName._Process)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._target)
		{
			_target = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._boneName)
		{
			_boneName = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		if (name == PropertyName._snap)
		{
			_snap = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._interpolationSpeed)
		{
			_interpolationSpeed = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._target)
		{
			value = VariantUtils.CreateFrom(in _target);
			return true;
		}
		if (name == PropertyName._boneName)
		{
			value = VariantUtils.CreateFrom(in _boneName);
			return true;
		}
		if (name == PropertyName._snap)
		{
			value = VariantUtils.CreateFrom(in _snap);
			return true;
		}
		if (name == PropertyName._interpolationSpeed)
		{
			value = VariantUtils.CreateFrom(in _interpolationSpeed);
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
		list.Add(new PropertyInfo(Variant.Type.String, PropertyName._boneName, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._snap, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._interpolationSpeed, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._target, Variant.From(in _target));
		info.AddProperty(PropertyName._boneName, Variant.From(in _boneName));
		info.AddProperty(PropertyName._snap, Variant.From(in _snap));
		info.AddProperty(PropertyName._interpolationSpeed, Variant.From(in _interpolationSpeed));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._target, out var value))
		{
			_target = value.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._boneName, out var value2))
		{
			_boneName = value2.As<string>();
		}
		if (info.TryGetProperty(PropertyName._snap, out var value3))
		{
			_snap = value3.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._interpolationSpeed, out var value4))
		{
			_interpolationSpeed = value4.As<float>();
		}
	}
}

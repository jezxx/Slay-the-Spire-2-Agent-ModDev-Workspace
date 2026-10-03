using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;

namespace MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;

[ScriptPath("res://src/Core/Nodes/Vfx/Utilities/NVfxProjectile.cs")]
public class NVfxProjectile : Node2D
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : Node2D.MethodName
	{
		/// <summary>
		/// Cached name for the 'SetEmitting' method.
		/// </summary>
		public static readonly StringName SetEmitting = "SetEmitting";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Node2D.PropertyName
	{
		/// <summary>
		/// Cached name for the 'AlignToVelocity' property.
		/// </summary>
		public static readonly StringName AlignToVelocity = "AlignToVelocity";

		/// <summary>
		/// Cached name for the '_projectileHead' field.
		/// </summary>
		public static readonly StringName _projectileHead = "_projectileHead";

		/// <summary>
		/// Cached name for the '_particles' field.
		/// </summary>
		public static readonly StringName _particles = "_particles";

		/// <summary>
		/// Cached name for the '_alignToVelocity' field.
		/// </summary>
		public static readonly StringName _alignToVelocity = "_alignToVelocity";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Node2D.SignalName
	{
	}

	[Export(PropertyHint.None, "")]
	private Node2D? _projectileHead;

	[Export(PropertyHint.None, "")]
	private GpuParticles2D[] _particles;

	[Export(PropertyHint.None, "")]
	private bool _alignToVelocity;

	public bool AlignToVelocity => _alignToVelocity;

	public void SetEmitting(bool emitting)
	{
		if (_projectileHead != null)
		{
			_projectileHead.Visible = emitting;
		}
		for (int i = 0; i < _particles.Length; i++)
		{
			_particles[i].Emitting = emitting;
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
		List<MethodInfo> list = new List<MethodInfo>(1);
		list.Add(new MethodInfo(MethodName.SetEmitting, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "emitting", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.SetEmitting && args.Count == 1)
		{
			SetEmitting(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.SetEmitting)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._projectileHead)
		{
			_projectileHead = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._particles)
		{
			_particles = VariantUtils.ConvertToSystemArrayOfGodotObject<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._alignToVelocity)
		{
			_alignToVelocity = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.AlignToVelocity)
		{
			bool from = AlignToVelocity;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName._projectileHead)
		{
			value = VariantUtils.CreateFrom(in _projectileHead);
			return true;
		}
		if (name == PropertyName._particles)
		{
			GodotObject[] particles = _particles;
			value = VariantUtils.CreateFromSystemArrayOfGodotObject(particles);
			return true;
		}
		if (name == PropertyName._alignToVelocity)
		{
			value = VariantUtils.CreateFrom(in _alignToVelocity);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._projectileHead, PropertyHint.NodeType, "Node2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Array, PropertyName._particles, PropertyHint.TypeString, "24/34:GPUParticles2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._alignToVelocity, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.AlignToVelocity, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._projectileHead, Variant.From(in _projectileHead));
		StringName particles = PropertyName._particles;
		GodotObject[] particles2 = _particles;
		info.AddProperty(particles, Variant.CreateFrom(particles2));
		info.AddProperty(PropertyName._alignToVelocity, Variant.From(in _alignToVelocity));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._projectileHead, out var value))
		{
			_projectileHead = value.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._particles, out var value2))
		{
			_particles = value2.AsGodotObjectArray<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._alignToVelocity, out var value3))
		{
			_alignToVelocity = value3.As<bool>();
		}
	}
}

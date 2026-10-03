using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;

namespace MegaCrit.Sts2.Core.Nodes.Orbs;

[ScriptPath("res://src/Core/Nodes/Orbs/NDarkOrbVfx.cs")]
public class NDarkOrbVfx : NOrbVfx
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : NOrbVfx.MethodName
	{
		/// <summary>
		/// Cached name for the 'UpdateDarkBgSize' method.
		/// </summary>
		public static readonly StringName UpdateDarkBgSize = "UpdateDarkBgSize";

		/// <summary>
		/// Cached name for the 'OnEvokeInternal' method.
		/// </summary>
		public new static readonly StringName OnEvokeInternal = "OnEvokeInternal";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : NOrbVfx.PropertyName
	{
		/// <summary>
		/// Cached name for the '_superchargedParticles' field.
		/// </summary>
		public static readonly StringName _superchargedParticles = "_superchargedParticles";

		/// <summary>
		/// Cached name for the '_darkBg' field.
		/// </summary>
		public static readonly StringName _darkBg = "_darkBg";

		/// <summary>
		/// Cached name for the '_superchargeThreshold' field.
		/// </summary>
		public static readonly StringName _superchargeThreshold = "_superchargeThreshold";

		/// <summary>
		/// Cached name for the '_darkBgNormalScale' field.
		/// </summary>
		public static readonly StringName _darkBgNormalScale = "_darkBgNormalScale";

		/// <summary>
		/// Cached name for the '_darkBgSuperchargedScale' field.
		/// </summary>
		public static readonly StringName _darkBgSuperchargedScale = "_darkBgSuperchargedScale";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : NOrbVfx.SignalName
	{
	}

	[Export(PropertyHint.None, "")]
	private NParticlesContainer? _superchargedParticles;

	[Export(PropertyHint.None, "")]
	private Node2D _darkBg;

	/// <summary>
	/// Show the supercharged particles when the evoke value is greater than or equal to this value.
	/// </summary>
	[Export(PropertyHint.None, "")]
	private float _superchargeThreshold = 24f;

	private float _darkBgNormalScale = 1f;

	private float _darkBgSuperchargedScale = 1.2f;

	public override void OnPassiveActivated(decimal passiveVal, decimal evokeVal)
	{
		base.OnPassiveActivated(passiveVal, evokeVal);
		bool flag = (float)evokeVal >= _superchargeThreshold;
		if (_superchargedParticles != null)
		{
			_superchargedParticles.SetEmitting(flag);
		}
		UpdateDarkBgSize(flag);
		ShakeOrb(HasFocusPower() ? 1f : 0.65f, 0.55f);
	}

	private void UpdateDarkBgSize(bool isSupercharged)
	{
		float num = (isSupercharged ? _darkBgSuperchargedScale : _darkBgNormalScale);
		if (!Mathf.IsEqualApprox(_darkBg.Scale.X, num))
		{
			Tween tween = GetTree().CreateTween();
			tween.TweenProperty(_darkBg, "scale", Vector2.One * num, 0.25);
		}
	}

	protected override void OnEvokeInternal(Vector2 targetVfxSpawnPosition)
	{
		base.OnEvokeInternal(targetVfxSpawnPosition);
		NVfxProjectileHandler child = NVfxProjectileHandler.Create("vfx/orbs/dark/vfx_dark_orb_evoke_projectile_handler", "vfx/orbs/dark/vfx_dark_orb_evoke_projectile", base.GlobalPosition, targetVfxSpawnPosition, default(Callable));
		base.VfxContainer.AddChildSafely(child);
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
		list.Add(new MethodInfo(MethodName.UpdateDarkBgSize, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "isSupercharged", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnEvokeInternal, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Vector2, "targetVfxSpawnPosition", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.UpdateDarkBgSize && args.Count == 1)
		{
			UpdateDarkBgSize(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnEvokeInternal && args.Count == 1)
		{
			OnEvokeInternal(VariantUtils.ConvertTo<Vector2>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.UpdateDarkBgSize)
		{
			return true;
		}
		if (method == MethodName.OnEvokeInternal)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._superchargedParticles)
		{
			_superchargedParticles = VariantUtils.ConvertTo<NParticlesContainer>(in value);
			return true;
		}
		if (name == PropertyName._darkBg)
		{
			_darkBg = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._superchargeThreshold)
		{
			_superchargeThreshold = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._darkBgNormalScale)
		{
			_darkBgNormalScale = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._darkBgSuperchargedScale)
		{
			_darkBgSuperchargedScale = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._superchargedParticles)
		{
			value = VariantUtils.CreateFrom(in _superchargedParticles);
			return true;
		}
		if (name == PropertyName._darkBg)
		{
			value = VariantUtils.CreateFrom(in _darkBg);
			return true;
		}
		if (name == PropertyName._superchargeThreshold)
		{
			value = VariantUtils.CreateFrom(in _superchargeThreshold);
			return true;
		}
		if (name == PropertyName._darkBgNormalScale)
		{
			value = VariantUtils.CreateFrom(in _darkBgNormalScale);
			return true;
		}
		if (name == PropertyName._darkBgSuperchargedScale)
		{
			value = VariantUtils.CreateFrom(in _darkBgSuperchargedScale);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._superchargedParticles, PropertyHint.NodeType, "Node2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._darkBg, PropertyHint.NodeType, "Node2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._superchargeThreshold, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._darkBgNormalScale, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._darkBgSuperchargedScale, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._superchargedParticles, Variant.From(in _superchargedParticles));
		info.AddProperty(PropertyName._darkBg, Variant.From(in _darkBg));
		info.AddProperty(PropertyName._superchargeThreshold, Variant.From(in _superchargeThreshold));
		info.AddProperty(PropertyName._darkBgNormalScale, Variant.From(in _darkBgNormalScale));
		info.AddProperty(PropertyName._darkBgSuperchargedScale, Variant.From(in _darkBgSuperchargedScale));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._superchargedParticles, out var value))
		{
			_superchargedParticles = value.As<NParticlesContainer>();
		}
		if (info.TryGetProperty(PropertyName._darkBg, out var value2))
		{
			_darkBg = value2.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._superchargeThreshold, out var value3))
		{
			_superchargeThreshold = value3.As<float>();
		}
		if (info.TryGetProperty(PropertyName._darkBgNormalScale, out var value4))
		{
			_darkBgNormalScale = value4.As<float>();
		}
		if (info.TryGetProperty(PropertyName._darkBgSuperchargedScale, out var value5))
		{
			_darkBgSuperchargedScale = value5.As<float>();
		}
	}
}

using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Commands;

namespace MegaCrit.Sts2.Core.Nodes.Orbs;

[ScriptPath("res://src/Core/Nodes/Orbs/NGlassOrbVfx.cs")]
public class NGlassOrbVfx : NOrbVfx
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : NOrbVfx.MethodName
	{
		/// <summary>
		/// Cached name for the 'HasFocusPower' method.
		/// </summary>
		public new static readonly StringName HasFocusPower = "HasFocusPower";

		/// <summary>
		/// Cached name for the 'ShowPassiveImpact' method.
		/// </summary>
		public static readonly StringName ShowPassiveImpact = "ShowPassiveImpact";

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
		/// Cached name for the '_passiveChromaticAberration' field.
		/// </summary>
		public static readonly StringName _passiveChromaticAberration = "_passiveChromaticAberration";

		/// <summary>
		/// Cached name for the '_basePassiveChromaticAberrationStength' field.
		/// </summary>
		public static readonly StringName _basePassiveChromaticAberrationStength = "_basePassiveChromaticAberrationStength";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : NOrbVfx.SignalName
	{
	}

	[Export(PropertyHint.None, "")]
	private GpuParticles2D _passiveChromaticAberration;

	[Export(PropertyHint.None, "")]
	private float _basePassiveChromaticAberrationStength = 0.01f;

	private decimal _basePassiveVal = 4m;

	private static readonly StringName _aberrationStrengthString = new StringName("instance_shader_parameters/base_intensity");

	public override void OnPassiveActivated(decimal passiveVal, decimal evokeVal)
	{
		if (!(passiveVal <= 0m))
		{
			base.OnPassiveActivated(passiveVal, evokeVal);
			float num = (float)(passiveVal / _basePassiveVal);
			_passiveChromaticAberration.Set(_aberrationStrengthString, Mathf.Lerp(0f, _basePassiveChromaticAberrationStength, num));
			ShakeOrb(num, 0.5f);
		}
	}

	public override void AfterPassiveActivated(decimal passiveVal, decimal evokeVal)
	{
		base.AfterPassiveActivated(passiveVal, evokeVal);
		UpdateFocusPowerState();
	}

	protected override bool HasFocusPower()
	{
		if (_orbModel != null && _orbModel.PassiveVal <= 0m)
		{
			return false;
		}
		return base.HasFocusPower();
	}

	public void ShowPassiveImpact(Vector2[] targetVfxSpawnPositions)
	{
		for (int i = 0; i < targetVfxSpawnPositions.Length; i++)
		{
			ShowPassiveImpact(targetVfxSpawnPositions[i]);
		}
	}

	private void ShowPassiveImpact(Vector2 targetVfxSpawnPosition)
	{
		VfxCmd.PlayVfx(targetVfxSpawnPosition, "vfx/orbs/glass/vfx_glass_orb_passive_impact", base.VfxContainer);
	}

	protected override void OnEvokeInternal(Vector2 targetVfxSpawnPosition)
	{
		base.OnEvokeInternal(targetVfxSpawnPosition);
		VfxCmd.PlayVfx(targetVfxSpawnPosition, "vfx/orbs/glass/vfx_glass_orb_evoke_impact", base.VfxContainer);
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(3);
		list.Add(new MethodInfo(MethodName.HasFocusPower, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ShowPassiveImpact, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.PackedVector2Array, "targetVfxSpawnPositions", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
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
		if (method == MethodName.HasFocusPower && args.Count == 0)
		{
			bool from = HasFocusPower();
			ret = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (method == MethodName.ShowPassiveImpact && args.Count == 1)
		{
			ShowPassiveImpact(VariantUtils.ConvertTo<Vector2[]>(in args[0]));
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
		if (method == MethodName.HasFocusPower)
		{
			return true;
		}
		if (method == MethodName.ShowPassiveImpact)
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
		if (name == PropertyName._passiveChromaticAberration)
		{
			_passiveChromaticAberration = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._basePassiveChromaticAberrationStength)
		{
			_basePassiveChromaticAberrationStength = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._passiveChromaticAberration)
		{
			value = VariantUtils.CreateFrom(in _passiveChromaticAberration);
			return true;
		}
		if (name == PropertyName._basePassiveChromaticAberrationStength)
		{
			value = VariantUtils.CreateFrom(in _basePassiveChromaticAberrationStength);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._passiveChromaticAberration, PropertyHint.NodeType, "GPUParticles2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._basePassiveChromaticAberrationStength, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._passiveChromaticAberration, Variant.From(in _passiveChromaticAberration));
		info.AddProperty(PropertyName._basePassiveChromaticAberrationStength, Variant.From(in _basePassiveChromaticAberrationStength));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._passiveChromaticAberration, out var value))
		{
			_passiveChromaticAberration = value.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._basePassiveChromaticAberrationStength, out var value2))
		{
			_basePassiveChromaticAberrationStength = value2.As<float>();
		}
	}
}

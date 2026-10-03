using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;

namespace MegaCrit.Sts2.Core.Nodes.Orbs;

[ScriptPath("res://src/Core/Nodes/Orbs/NPlasmaOrbVfx.cs")]
public class NPlasmaOrbVfx : NOrbVfx
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : NOrbVfx.MethodName
	{
		/// <summary>
		/// Cached name for the 'OnEvokeInternal' method.
		/// </summary>
		public new static readonly StringName OnEvokeInternal = "OnEvokeInternal";

		/// <summary>
		/// Cached name for the 'GetRandomOffset' method.
		/// </summary>
		public static readonly StringName GetRandomOffset = "GetRandomOffset";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : NOrbVfx.PropertyName
	{
		/// <summary>
		/// Cached name for the '_projectileOffsetRange' field.
		/// </summary>
		public static readonly StringName _projectileOffsetRange = "_projectileOffsetRange";

		/// <summary>
		/// Cached name for the '_projectileSpawnInterval' field.
		/// </summary>
		public static readonly StringName _projectileSpawnInterval = "_projectileSpawnInterval";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : NOrbVfx.SignalName
	{
	}

	[Export(PropertyHint.None, "")]
	private Vector2 _projectileOffsetRange;

	[Export(PropertyHint.None, "")]
	private float _projectileSpawnInterval = 0.05f;

	public override void OnPassiveActivated(decimal passiveVal, decimal evokeVal)
	{
		base.OnPassiveActivated(passiveVal, evokeVal);
		ShakeOrb(1f, 0.5f);
		TaskHelper.RunSafely(SpawnProjectile(1, GetPlayerVfxPosition()));
	}

	protected override void OnEvokeInternal(Vector2 targetVfxSpawnPosition)
	{
		base.OnEvokeInternal(targetVfxSpawnPosition);
		TaskHelper.RunSafely(SpawnProjectile(2, GetPlayerVfxPosition()));
	}

	private async Task SpawnProjectile(int count, Vector2 targetPosition)
	{
		for (int i = 0; i < count; i++)
		{
			NVfxProjectileHandler child = NVfxProjectileHandler.Create("vfx/orbs/plasma/vfx_plasma_orb_projectile_handler", "vfx/orbs/plasma/vfx_plasma_orb_projectile", base.GlobalPosition + GetRandomOffset(), targetPosition, (i == count - 1) ? Callable.From(delegate
			{
			}) : default(Callable));
			base.VfxContainer.AddChildSafely(child);
			if (i != count - 1)
			{
				await Cmd.Wait(_projectileSpawnInterval);
			}
		}
	}

	private Vector2 GetRandomOffset()
	{
		float s = Mathf.DegToRad(GD.Randf() * 360f);
		float num = Mathf.Lerp(_projectileOffsetRange.X, _projectileOffsetRange.Y, GD.Randf());
		return new Vector2(Mathf.Cos(s) * num, Mathf.Sin(s) * num);
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
		list.Add(new MethodInfo(MethodName.OnEvokeInternal, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Vector2, "targetVfxSpawnPosition", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.GetRandomOffset, new PropertyInfo(Variant.Type.Vector2, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.OnEvokeInternal && args.Count == 1)
		{
			OnEvokeInternal(VariantUtils.ConvertTo<Vector2>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.GetRandomOffset && args.Count == 0)
		{
			Vector2 from = GetRandomOffset();
			ret = VariantUtils.CreateFrom(in from);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.OnEvokeInternal)
		{
			return true;
		}
		if (method == MethodName.GetRandomOffset)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._projectileOffsetRange)
		{
			_projectileOffsetRange = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._projectileSpawnInterval)
		{
			_projectileSpawnInterval = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._projectileOffsetRange)
		{
			value = VariantUtils.CreateFrom(in _projectileOffsetRange);
			return true;
		}
		if (name == PropertyName._projectileSpawnInterval)
		{
			value = VariantUtils.CreateFrom(in _projectileSpawnInterval);
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
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._projectileOffsetRange, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._projectileSpawnInterval, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._projectileOffsetRange, Variant.From(in _projectileOffsetRange));
		info.AddProperty(PropertyName._projectileSpawnInterval, Variant.From(in _projectileSpawnInterval));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._projectileOffsetRange, out var value))
		{
			_projectileOffsetRange = value.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._projectileSpawnInterval, out var value2))
		{
			_projectileSpawnInterval = value2.As<float>();
		}
	}
}

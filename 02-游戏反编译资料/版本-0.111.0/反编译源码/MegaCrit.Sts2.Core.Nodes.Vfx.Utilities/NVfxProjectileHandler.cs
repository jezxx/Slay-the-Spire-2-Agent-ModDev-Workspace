using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;

[ScriptPath("res://src/Core/Nodes/Vfx/Utilities/NVfxProjectileHandler.cs")]
public class NVfxProjectileHandler : Node2D
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : Node2D.MethodName
	{
		/// <summary>
		/// Cached name for the 'Create' method.
		/// </summary>
		public static readonly StringName Create = "Create";

		/// <summary>
		/// Cached name for the '_Ready' method.
		/// </summary>
		public new static readonly StringName _Ready = "_Ready";

		/// <summary>
		/// Cached name for the '_ExitTree' method.
		/// </summary>
		public new static readonly StringName _ExitTree = "_ExitTree";

		/// <summary>
		/// Cached name for the 'SpawnImpactVfx' method.
		/// </summary>
		public static readonly StringName SpawnImpactVfx = "SpawnImpactVfx";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Node2D.PropertyName
	{
		/// <summary>
		/// Cached name for the '_pathHeightOffsets' field.
		/// </summary>
		public static readonly StringName _pathHeightOffsets = "_pathHeightOffsets";

		/// <summary>
		/// Cached name for the '_heightOffsetRange' field.
		/// </summary>
		public static readonly StringName _heightOffsetRange = "_heightOffsetRange";

		/// <summary>
		/// Cached name for the '_movementCurves' field.
		/// </summary>
		public static readonly StringName _movementCurves = "_movementCurves";

		/// <summary>
		/// Cached name for the '_travelTimeRange' field.
		/// </summary>
		public static readonly StringName _travelTimeRange = "_travelTimeRange";

		/// <summary>
		/// Cached name for the '_impactParticlesScenePath' field.
		/// </summary>
		public static readonly StringName _impactParticlesScenePath = "_impactParticlesScenePath";

		/// <summary>
		/// Cached name for the '_sourceGlobalPosition' field.
		/// </summary>
		public static readonly StringName _sourceGlobalPosition = "_sourceGlobalPosition";

		/// <summary>
		/// Cached name for the '_destinationGlobalPosition' field.
		/// </summary>
		public static readonly StringName _destinationGlobalPosition = "_destinationGlobalPosition";

		/// <summary>
		/// Cached name for the '_projectileScenePath' field.
		/// </summary>
		public static readonly StringName _projectileScenePath = "_projectileScenePath";

		/// <summary>
		/// Cached name for the '_endAction' field.
		/// </summary>
		public static readonly StringName _endAction = "_endAction";

		/// <summary>
		/// Cached name for the '_loadedProjectile' field.
		/// </summary>
		public static readonly StringName _loadedProjectile = "_loadedProjectile";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Node2D.SignalName
	{
	}

	[Export(PropertyHint.None, "")]
	private Curve[] _pathHeightOffsets = Array.Empty<Curve>();

	[Export(PropertyHint.None, "")]
	private Vector2 _heightOffsetRange;

	[Export(PropertyHint.None, "")]
	private Curve[] _movementCurves = Array.Empty<Curve>();

	[Export(PropertyHint.None, "")]
	private Vector2 _travelTimeRange;

	[Export(PropertyHint.None, "")]
	private string _impactParticlesScenePath = "";

	private Vector2 _sourceGlobalPosition;

	private Vector2 _destinationGlobalPosition;

	private string _projectileScenePath;

	private Callable _endAction;

	private NVfxProjectile? _loadedProjectile;

	public static NVfxProjectileHandler? Create(string handlerScenePath, string projectileScenePath, Vector2 sourceGlobalPosition, Vector2 destinationGlobalPosition, Callable endAction)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NVfxProjectileHandler nVfxProjectileHandler = PreloadManager.Cache.GetScene(SceneHelper.GetScenePath(handlerScenePath)).Instantiate<NVfxProjectileHandler>(PackedScene.GenEditState.Disabled);
		nVfxProjectileHandler._sourceGlobalPosition = sourceGlobalPosition;
		nVfxProjectileHandler._destinationGlobalPosition = destinationGlobalPosition;
		nVfxProjectileHandler._projectileScenePath = projectileScenePath;
		nVfxProjectileHandler._endAction = endAction;
		return nVfxProjectileHandler;
	}

	public override void _Ready()
	{
		TaskHelper.RunSafely(PlaySequence());
	}

	public override void _ExitTree()
	{
		if (_loadedProjectile != null)
		{
			_loadedProjectile.QueueFreeSafely();
		}
	}

	private async Task PlaySequence()
	{
		_loadedProjectile = PreloadManager.Cache.GetScene(SceneHelper.GetScenePath(_projectileScenePath)).Instantiate<NVfxProjectile>(PackedScene.GenEditState.Disabled);
		if (_loadedProjectile == null)
		{
			return;
		}
		this.AddChildSafely(_loadedProjectile);
		_loadedProjectile.GlobalPosition = _sourceGlobalPosition;
		_loadedProjectile.SetEmitting(emitting: true);
		Vector2 vector = (_destinationGlobalPosition - _sourceGlobalPosition).Normalized();
		Vector2 normal = new Vector2(vector.Y, 0f - vector.X);
		if ((_sourceGlobalPosition + normal).Y < _sourceGlobalPosition.Y)
		{
			normal *= -1f;
		}
		float num = 0f;
		float projectileDuration = (float)GD.RandRange(_travelTimeRange.X, _travelTimeRange.Y);
		float projectileHeightOffset = (float)GD.RandRange(_heightOffsetRange.X, _heightOffsetRange.Y);
		Curve chosenMovementCurve = _movementCurves[Mathf.RoundToInt(GD.RandRange(0.0, _movementCurves.Length - 1))];
		Curve chosenHeightCurve = _pathHeightOffsets[Mathf.RoundToInt(GD.RandRange(0.0, _pathHeightOffsets.Length - 1))];
		while (num < projectileDuration)
		{
			float offset = num / projectileDuration;
			float weight = chosenMovementCurve.Sample(offset);
			float num2 = chosenHeightCurve.Sample(offset);
			Vector2 vector2 = _sourceGlobalPosition.Lerp(_destinationGlobalPosition, weight);
			Vector2 vector3 = normal * projectileHeightOffset * num2;
			Vector2 vector4 = vector2 + vector3;
			if (_loadedProjectile.AlignToVelocity)
			{
				Vector2 vector5 = vector4 - _loadedProjectile.GlobalPosition;
				float globalRotation = Mathf.Atan2(vector5.Y, vector5.X);
				_loadedProjectile.GlobalRotation = globalRotation;
			}
			_loadedProjectile.GlobalPosition = vector4;
			float num3 = num;
			num = num3 + await this.AwaitProcessFrame();
		}
		_loadedProjectile.GlobalPosition = _destinationGlobalPosition;
		_loadedProjectile.SetEmitting(emitting: false);
		SpawnImpactVfx(_destinationGlobalPosition);
		if ((object)_endAction.Delegate != null)
		{
			_endAction.Call();
		}
		TaskHelper.RunSafely(DelayedFree());
	}

	private void SpawnImpactVfx(Vector2 spawnPosition)
	{
		if (!string.IsNullOrEmpty(_impactParticlesScenePath))
		{
			Control vfxContainer = GetParent<Control>();
			if (NCombatRoom.Instance != null)
			{
				vfxContainer = NCombatRoom.Instance.CombatVfxContainer;
			}
			VfxCmd.PlayVfx(spawnPosition, _impactParticlesScenePath, vfxContainer);
		}
	}

	private async Task DelayedFree()
	{
		await Cmd.Wait(2f);
		this.QueueFreeSafely();
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(4);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "handlerScenePath", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.String, "projectileScenePath", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Vector2, "sourceGlobalPosition", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Vector2, "destinationGlobalPosition", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Callable, "endAction", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SpawnImpactVfx, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Vector2, "spawnPosition", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 5)
		{
			NVfxProjectileHandler from = Create(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<string>(in args[1]), VariantUtils.ConvertTo<Vector2>(in args[2]), VariantUtils.ConvertTo<Vector2>(in args[3]), VariantUtils.ConvertTo<Callable>(in args[4]));
			ret = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SpawnImpactVfx && args.Count == 1)
		{
			SpawnImpactVfx(VariantUtils.ConvertTo<Vector2>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 5)
		{
			NVfxProjectileHandler from = Create(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<string>(in args[1]), VariantUtils.ConvertTo<Vector2>(in args[2]), VariantUtils.ConvertTo<Vector2>(in args[3]), VariantUtils.ConvertTo<Callable>(in args[4]));
			ret = VariantUtils.CreateFrom(in from);
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.Create)
		{
			return true;
		}
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.SpawnImpactVfx)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._pathHeightOffsets)
		{
			_pathHeightOffsets = VariantUtils.ConvertToSystemArrayOfGodotObject<Curve>(in value);
			return true;
		}
		if (name == PropertyName._heightOffsetRange)
		{
			_heightOffsetRange = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._movementCurves)
		{
			_movementCurves = VariantUtils.ConvertToSystemArrayOfGodotObject<Curve>(in value);
			return true;
		}
		if (name == PropertyName._travelTimeRange)
		{
			_travelTimeRange = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._impactParticlesScenePath)
		{
			_impactParticlesScenePath = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		if (name == PropertyName._sourceGlobalPosition)
		{
			_sourceGlobalPosition = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._destinationGlobalPosition)
		{
			_destinationGlobalPosition = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._projectileScenePath)
		{
			_projectileScenePath = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		if (name == PropertyName._endAction)
		{
			_endAction = VariantUtils.ConvertTo<Callable>(in value);
			return true;
		}
		if (name == PropertyName._loadedProjectile)
		{
			_loadedProjectile = VariantUtils.ConvertTo<NVfxProjectile>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._pathHeightOffsets)
		{
			GodotObject[] pathHeightOffsets = _pathHeightOffsets;
			value = VariantUtils.CreateFromSystemArrayOfGodotObject(pathHeightOffsets);
			return true;
		}
		if (name == PropertyName._heightOffsetRange)
		{
			value = VariantUtils.CreateFrom(in _heightOffsetRange);
			return true;
		}
		if (name == PropertyName._movementCurves)
		{
			GodotObject[] pathHeightOffsets = _movementCurves;
			value = VariantUtils.CreateFromSystemArrayOfGodotObject(pathHeightOffsets);
			return true;
		}
		if (name == PropertyName._travelTimeRange)
		{
			value = VariantUtils.CreateFrom(in _travelTimeRange);
			return true;
		}
		if (name == PropertyName._impactParticlesScenePath)
		{
			value = VariantUtils.CreateFrom(in _impactParticlesScenePath);
			return true;
		}
		if (name == PropertyName._sourceGlobalPosition)
		{
			value = VariantUtils.CreateFrom(in _sourceGlobalPosition);
			return true;
		}
		if (name == PropertyName._destinationGlobalPosition)
		{
			value = VariantUtils.CreateFrom(in _destinationGlobalPosition);
			return true;
		}
		if (name == PropertyName._projectileScenePath)
		{
			value = VariantUtils.CreateFrom(in _projectileScenePath);
			return true;
		}
		if (name == PropertyName._endAction)
		{
			value = VariantUtils.CreateFrom(in _endAction);
			return true;
		}
		if (name == PropertyName._loadedProjectile)
		{
			value = VariantUtils.CreateFrom(in _loadedProjectile);
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
		list.Add(new PropertyInfo(Variant.Type.Array, PropertyName._pathHeightOffsets, PropertyHint.TypeString, "24/17:Curve", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._heightOffsetRange, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Array, PropertyName._movementCurves, PropertyHint.TypeString, "24/17:Curve", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._travelTimeRange, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.String, PropertyName._impactParticlesScenePath, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._sourceGlobalPosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._destinationGlobalPosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.String, PropertyName._projectileScenePath, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Callable, PropertyName._endAction, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._loadedProjectile, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		StringName pathHeightOffsets = PropertyName._pathHeightOffsets;
		GodotObject[] pathHeightOffsets2 = _pathHeightOffsets;
		info.AddProperty(pathHeightOffsets, Variant.CreateFrom(pathHeightOffsets2));
		info.AddProperty(PropertyName._heightOffsetRange, Variant.From(in _heightOffsetRange));
		StringName movementCurves = PropertyName._movementCurves;
		pathHeightOffsets2 = _movementCurves;
		info.AddProperty(movementCurves, Variant.CreateFrom(pathHeightOffsets2));
		info.AddProperty(PropertyName._travelTimeRange, Variant.From(in _travelTimeRange));
		info.AddProperty(PropertyName._impactParticlesScenePath, Variant.From(in _impactParticlesScenePath));
		info.AddProperty(PropertyName._sourceGlobalPosition, Variant.From(in _sourceGlobalPosition));
		info.AddProperty(PropertyName._destinationGlobalPosition, Variant.From(in _destinationGlobalPosition));
		info.AddProperty(PropertyName._projectileScenePath, Variant.From(in _projectileScenePath));
		info.AddProperty(PropertyName._endAction, Variant.From(in _endAction));
		info.AddProperty(PropertyName._loadedProjectile, Variant.From(in _loadedProjectile));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._pathHeightOffsets, out var value))
		{
			_pathHeightOffsets = value.AsGodotObjectArray<Curve>();
		}
		if (info.TryGetProperty(PropertyName._heightOffsetRange, out var value2))
		{
			_heightOffsetRange = value2.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._movementCurves, out var value3))
		{
			_movementCurves = value3.AsGodotObjectArray<Curve>();
		}
		if (info.TryGetProperty(PropertyName._travelTimeRange, out var value4))
		{
			_travelTimeRange = value4.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._impactParticlesScenePath, out var value5))
		{
			_impactParticlesScenePath = value5.As<string>();
		}
		if (info.TryGetProperty(PropertyName._sourceGlobalPosition, out var value6))
		{
			_sourceGlobalPosition = value6.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._destinationGlobalPosition, out var value7))
		{
			_destinationGlobalPosition = value7.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._projectileScenePath, out var value8))
		{
			_projectileScenePath = value8.As<string>();
		}
		if (info.TryGetProperty(PropertyName._endAction, out var value9))
		{
			_endAction = value9.As<Callable>();
		}
		if (info.TryGetProperty(PropertyName._loadedProjectile, out var value10))
		{
			_loadedProjectile = value10.As<NVfxProjectile>();
		}
	}
}

using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;

namespace MegaCrit.Sts2.Core.Nodes.Orbs;

[ScriptPath("res://src/Core/Nodes/Orbs/NOrbVfx.cs")]
public class NOrbVfx : Node2D
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
		/// Cached name for the '_ExitTree' method.
		/// </summary>
		public new static readonly StringName _ExitTree = "_ExitTree";

		/// <summary>
		/// Cached name for the 'UpdateFocusPowerState' method.
		/// </summary>
		public static readonly StringName UpdateFocusPowerState = "UpdateFocusPowerState";

		/// <summary>
		/// Cached name for the 'SetForcedFocusPower' method.
		/// </summary>
		public static readonly StringName SetForcedFocusPower = "SetForcedFocusPower";

		/// <summary>
		/// Cached name for the 'HasFocusPower' method.
		/// </summary>
		public static readonly StringName HasFocusPower = "HasFocusPower";

		/// <summary>
		/// Cached name for the 'OnPassiveActivated' method.
		/// </summary>
		public static readonly StringName OnPassiveActivated = "OnPassiveActivated";

		/// <summary>
		/// Cached name for the 'OnEvoke' method.
		/// </summary>
		public static readonly StringName OnEvoke = "OnEvoke";

		/// <summary>
		/// Cached name for the 'SpawnEvokeVfx' method.
		/// </summary>
		public static readonly StringName SpawnEvokeVfx = "SpawnEvokeVfx";

		/// <summary>
		/// Cached name for the 'OnEvokeInternal' method.
		/// </summary>
		public static readonly StringName OnEvokeInternal = "OnEvokeInternal";

		/// <summary>
		/// Cached name for the 'GetPlayerVfxPosition' method.
		/// </summary>
		public static readonly StringName GetPlayerVfxPosition = "GetPlayerVfxPosition";

		/// <summary>
		/// Cached name for the 'SetOverrideCombatVfxContainer' method.
		/// </summary>
		public static readonly StringName SetOverrideCombatVfxContainer = "SetOverrideCombatVfxContainer";

		/// <summary>
		/// Cached name for the 'SetOverridePlayerNode' method.
		/// </summary>
		public static readonly StringName SetOverridePlayerNode = "SetOverridePlayerNode";

		/// <summary>
		/// Cached name for the 'ShakeOrb' method.
		/// </summary>
		public static readonly StringName ShakeOrb = "ShakeOrb";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Node2D.PropertyName
	{
		/// <summary>
		/// Cached name for the 'VfxContainer' property.
		/// </summary>
		public static readonly StringName VfxContainer = "VfxContainer";

		/// <summary>
		/// Cached name for the '_focusedParticles' field.
		/// </summary>
		public static readonly StringName _focusedParticles = "_focusedParticles";

		/// <summary>
		/// Cached name for the '_passiveActivatedParticles' field.
		/// </summary>
		public static readonly StringName _passiveActivatedParticles = "_passiveActivatedParticles";

		/// <summary>
		/// Cached name for the '_passiveActivatedFocusedParticles' field.
		/// </summary>
		public static readonly StringName _passiveActivatedFocusedParticles = "_passiveActivatedFocusedParticles";

		/// <summary>
		/// Cached name for the '_spineShaker' field.
		/// </summary>
		public static readonly StringName _spineShaker = "_spineShaker";

		/// <summary>
		/// Cached name for the '_evokeVfxSceneName' field.
		/// </summary>
		public static readonly StringName _evokeVfxSceneName = "_evokeVfxSceneName";

		/// <summary>
		/// Cached name for the '_forcedFocusPower' field.
		/// </summary>
		public static readonly StringName _forcedFocusPower = "_forcedFocusPower";

		/// <summary>
		/// Cached name for the '_overrideCombatVfxContainer' field.
		/// </summary>
		public static readonly StringName _overrideCombatVfxContainer = "_overrideCombatVfxContainer";

		/// <summary>
		/// Cached name for the '_overridePlayerNode' field.
		/// </summary>
		public static readonly StringName _overridePlayerNode = "_overridePlayerNode";

		/// <summary>
		/// Cached name for the '_shakeTween' field.
		/// </summary>
		public static readonly StringName _shakeTween = "_shakeTween";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Node2D.SignalName
	{
	}

	[Export(PropertyHint.None, "")]
	private NParticlesContainer? _focusedParticles;

	[Export(PropertyHint.None, "")]
	private NParticlesContainer? _passiveActivatedParticles;

	[Export(PropertyHint.None, "")]
	private NParticlesContainer? _passiveActivatedFocusedParticles;

	[Export(PropertyHint.None, "")]
	private NShaker? _spineShaker;

	[Export(PropertyHint.None, "")]
	private string _evokeVfxSceneName = "";

	private static string _evokeVfxScenePath = "vfx/orbs/";

	protected bool _forcedFocusPower;

	private Control? _overrideCombatVfxContainer;

	protected OrbModel? _orbModel;

	protected Player? _owner;

	protected Node2D? _overridePlayerNode;

	private Tween? _shakeTween;

	protected Control VfxContainer
	{
		get
		{
			Control control = _overrideCombatVfxContainer;
			if (control == null)
			{
				NCombatRoom? instance = NCombatRoom.Instance;
				if (instance == null)
				{
					return null;
				}
				control = instance.CombatVfxContainer;
			}
			return control;
		}
	}

	public override void _Ready()
	{
		base._Ready();
		if (_spineShaker != null)
		{
			_spineShaker.Strength = 0f;
		}
	}

	public void Initialize(OrbModel orbModel)
	{
		_orbModel = orbModel;
		_owner = _orbModel.Owner;
		if (_owner != null)
		{
			_owner.Creature.PowerApplied += OnPowerApplied;
			_owner.Creature.PowerIncreased += OnPowerIncreased;
			_owner.Creature.PowerDecreased += OnPowerDecreased;
		}
		if (_orbModel != null)
		{
			_orbModel.PassiveActivated += OnPassiveActivated;
			_orbModel.EvokeActivated += OnEvoke;
		}
		UpdateFocusPowerState();
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		_shakeTween?.Kill();
		if (_owner != null)
		{
			_owner.Creature.PowerApplied -= OnPowerApplied;
			_owner.Creature.PowerIncreased -= OnPowerIncreased;
			_owner.Creature.PowerDecreased -= OnPowerDecreased;
		}
		if (_orbModel != null)
		{
			_orbModel.PassiveActivated -= OnPassiveActivated;
			_orbModel.EvokeActivated -= OnEvoke;
		}
	}

	protected void OnPowerApplied(PowerModel powerModel)
	{
		UpdateFocusPowerState();
	}

	protected void OnPowerIncreased(PowerModel powerModel, int amount, bool silent)
	{
		UpdateFocusPowerState();
	}

	protected void OnPowerDecreased(PowerModel powerModel, bool silent)
	{
		UpdateFocusPowerState();
	}

	protected virtual void UpdateFocusPowerState()
	{
		if (_focusedParticles != null)
		{
			_focusedParticles.SetEmitting(HasFocusPower());
		}
	}

	public virtual void SetForcedFocusPower(bool forcedFocusPower)
	{
		_forcedFocusPower = forcedFocusPower;
		UpdateFocusPowerState();
	}

	protected virtual bool HasFocusPower()
	{
		bool flag = _forcedFocusPower;
		if (_owner != null)
		{
			flag |= _owner.Creature.GetPowerAmount<FocusPower>() > 0;
		}
		return flag;
	}

	public void OnPassiveActivated()
	{
		if (_orbModel != null)
		{
			OnPassiveActivated(_orbModel.PassiveVal, _orbModel.EvokeVal);
		}
	}

	public virtual void OnPassiveActivated(decimal passiveVal, decimal evokeVal)
	{
		if (_passiveActivatedParticles != null)
		{
			_passiveActivatedParticles.Restart();
		}
		if (_passiveActivatedFocusedParticles != null && HasFocusPower())
		{
			_passiveActivatedFocusedParticles.Restart();
		}
	}

	public virtual void AfterPassiveActivated(decimal passiveVal, decimal evokeVal)
	{
	}

	private void OnEvoke(Creature[] targets)
	{
		List<NCreature> list = new List<NCreature>();
		foreach (Creature creature in targets)
		{
			NCreature nCreature = NCombatRoom.Instance?.GetCreatureNode(creature);
			if (nCreature != null)
			{
				list.Add(nCreature);
			}
		}
		OnEvoke(list.ToArray());
	}

	public void OnEvoke(NCreature[] targets)
	{
		Vector2[] array = new Vector2[targets.Length];
		for (int i = 0; i < targets.Length; i++)
		{
			array[i] = targets[i].VfxSpawnPosition;
		}
		OnEvoke(array);
	}

	public void OnEvoke(Vector2[] targetVfxSpawnPositions)
	{
		SpawnEvokeVfx();
		for (int i = 0; i < targetVfxSpawnPositions.Length; i++)
		{
			OnEvokeInternal(targetVfxSpawnPositions[i]);
		}
	}

	private void SpawnEvokeVfx()
	{
		if (!string.IsNullOrEmpty(_evokeVfxSceneName))
		{
			VfxCmd.PlayVfx(base.GlobalPosition, _evokeVfxScenePath + _evokeVfxSceneName, VfxContainer);
		}
	}

	protected virtual void OnEvokeInternal(Vector2 targetVfxSpawnPosition)
	{
	}

	protected Vector2 GetPlayerVfxPosition()
	{
		if (_overridePlayerNode != null)
		{
			return _overridePlayerNode.Position;
		}
		if (_owner == null)
		{
			return base.Position;
		}
		return (NCombatRoom.Instance?.GetCreatureNode(_owner.Creature))?.VfxSpawnPosition ?? base.Position;
	}

	public void SetOverrideCombatVfxContainer(Control overrideCombatVfxContainer)
	{
		_overrideCombatVfxContainer = overrideCombatVfxContainer;
	}

	public void SetOverridePlayerNode(Node2D overridePlayerNode)
	{
		_overridePlayerNode = overridePlayerNode;
	}

	protected void ShakeOrb(float initialStrength, float duration)
	{
		if (_spineShaker != null)
		{
			if (_shakeTween != null && _shakeTween.IsValid())
			{
				_shakeTween.Kill();
			}
			_spineShaker.Strength = initialStrength;
			_shakeTween = GetTree().CreateTween();
			_shakeTween.TweenProperty(_spineShaker, "_strength", 0f, duration).SetEase(Tween.EaseType.OutIn).SetTrans(Tween.TransitionType.Sine);
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
		List<MethodInfo> list = new List<MethodInfo>(13);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateFocusPowerState, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetForcedFocusPower, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "forcedFocusPower", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.HasFocusPower, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnPassiveActivated, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnEvoke, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Array, "targets", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SpawnEvokeVfx, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnEvokeInternal, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Vector2, "targetVfxSpawnPosition", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.GetPlayerVfxPosition, new PropertyInfo(Variant.Type.Vector2, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetOverrideCombatVfxContainer, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "overrideCombatVfxContainer", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SetOverridePlayerNode, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "overridePlayerNode", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ShakeOrb, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "initialStrength", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Float, "duration", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
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
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateFocusPowerState && args.Count == 0)
		{
			UpdateFocusPowerState();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetForcedFocusPower && args.Count == 1)
		{
			SetForcedFocusPower(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.HasFocusPower && args.Count == 0)
		{
			bool from = HasFocusPower();
			ret = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (method == MethodName.OnPassiveActivated && args.Count == 0)
		{
			OnPassiveActivated();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnEvoke && args.Count == 1)
		{
			OnEvoke(VariantUtils.ConvertToSystemArrayOfGodotObject<NCreature>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SpawnEvokeVfx && args.Count == 0)
		{
			SpawnEvokeVfx();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnEvokeInternal && args.Count == 1)
		{
			OnEvokeInternal(VariantUtils.ConvertTo<Vector2>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.GetPlayerVfxPosition && args.Count == 0)
		{
			Vector2 from2 = GetPlayerVfxPosition();
			ret = VariantUtils.CreateFrom(in from2);
			return true;
		}
		if (method == MethodName.SetOverrideCombatVfxContainer && args.Count == 1)
		{
			SetOverrideCombatVfxContainer(VariantUtils.ConvertTo<Control>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetOverridePlayerNode && args.Count == 1)
		{
			SetOverridePlayerNode(VariantUtils.ConvertTo<Node2D>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ShakeOrb && args.Count == 2)
		{
			ShakeOrb(VariantUtils.ConvertTo<float>(in args[0]), VariantUtils.ConvertTo<float>(in args[1]));
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
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.UpdateFocusPowerState)
		{
			return true;
		}
		if (method == MethodName.SetForcedFocusPower)
		{
			return true;
		}
		if (method == MethodName.HasFocusPower)
		{
			return true;
		}
		if (method == MethodName.OnPassiveActivated)
		{
			return true;
		}
		if (method == MethodName.OnEvoke)
		{
			return true;
		}
		if (method == MethodName.SpawnEvokeVfx)
		{
			return true;
		}
		if (method == MethodName.OnEvokeInternal)
		{
			return true;
		}
		if (method == MethodName.GetPlayerVfxPosition)
		{
			return true;
		}
		if (method == MethodName.SetOverrideCombatVfxContainer)
		{
			return true;
		}
		if (method == MethodName.SetOverridePlayerNode)
		{
			return true;
		}
		if (method == MethodName.ShakeOrb)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._focusedParticles)
		{
			_focusedParticles = VariantUtils.ConvertTo<NParticlesContainer>(in value);
			return true;
		}
		if (name == PropertyName._passiveActivatedParticles)
		{
			_passiveActivatedParticles = VariantUtils.ConvertTo<NParticlesContainer>(in value);
			return true;
		}
		if (name == PropertyName._passiveActivatedFocusedParticles)
		{
			_passiveActivatedFocusedParticles = VariantUtils.ConvertTo<NParticlesContainer>(in value);
			return true;
		}
		if (name == PropertyName._spineShaker)
		{
			_spineShaker = VariantUtils.ConvertTo<NShaker>(in value);
			return true;
		}
		if (name == PropertyName._evokeVfxSceneName)
		{
			_evokeVfxSceneName = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		if (name == PropertyName._forcedFocusPower)
		{
			_forcedFocusPower = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._overrideCombatVfxContainer)
		{
			_overrideCombatVfxContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._overridePlayerNode)
		{
			_overridePlayerNode = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._shakeTween)
		{
			_shakeTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.VfxContainer)
		{
			Control from = VfxContainer;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName._focusedParticles)
		{
			value = VariantUtils.CreateFrom(in _focusedParticles);
			return true;
		}
		if (name == PropertyName._passiveActivatedParticles)
		{
			value = VariantUtils.CreateFrom(in _passiveActivatedParticles);
			return true;
		}
		if (name == PropertyName._passiveActivatedFocusedParticles)
		{
			value = VariantUtils.CreateFrom(in _passiveActivatedFocusedParticles);
			return true;
		}
		if (name == PropertyName._spineShaker)
		{
			value = VariantUtils.CreateFrom(in _spineShaker);
			return true;
		}
		if (name == PropertyName._evokeVfxSceneName)
		{
			value = VariantUtils.CreateFrom(in _evokeVfxSceneName);
			return true;
		}
		if (name == PropertyName._forcedFocusPower)
		{
			value = VariantUtils.CreateFrom(in _forcedFocusPower);
			return true;
		}
		if (name == PropertyName._overrideCombatVfxContainer)
		{
			value = VariantUtils.CreateFrom(in _overrideCombatVfxContainer);
			return true;
		}
		if (name == PropertyName._overridePlayerNode)
		{
			value = VariantUtils.CreateFrom(in _overridePlayerNode);
			return true;
		}
		if (name == PropertyName._shakeTween)
		{
			value = VariantUtils.CreateFrom(in _shakeTween);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._focusedParticles, PropertyHint.NodeType, "Node2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._passiveActivatedParticles, PropertyHint.NodeType, "Node2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._passiveActivatedFocusedParticles, PropertyHint.NodeType, "Node2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._spineShaker, PropertyHint.NodeType, "Node", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.String, PropertyName._evokeVfxSceneName, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._forcedFocusPower, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._overrideCombatVfxContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.VfxContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._overridePlayerNode, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._shakeTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._focusedParticles, Variant.From(in _focusedParticles));
		info.AddProperty(PropertyName._passiveActivatedParticles, Variant.From(in _passiveActivatedParticles));
		info.AddProperty(PropertyName._passiveActivatedFocusedParticles, Variant.From(in _passiveActivatedFocusedParticles));
		info.AddProperty(PropertyName._spineShaker, Variant.From(in _spineShaker));
		info.AddProperty(PropertyName._evokeVfxSceneName, Variant.From(in _evokeVfxSceneName));
		info.AddProperty(PropertyName._forcedFocusPower, Variant.From(in _forcedFocusPower));
		info.AddProperty(PropertyName._overrideCombatVfxContainer, Variant.From(in _overrideCombatVfxContainer));
		info.AddProperty(PropertyName._overridePlayerNode, Variant.From(in _overridePlayerNode));
		info.AddProperty(PropertyName._shakeTween, Variant.From(in _shakeTween));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._focusedParticles, out var value))
		{
			_focusedParticles = value.As<NParticlesContainer>();
		}
		if (info.TryGetProperty(PropertyName._passiveActivatedParticles, out var value2))
		{
			_passiveActivatedParticles = value2.As<NParticlesContainer>();
		}
		if (info.TryGetProperty(PropertyName._passiveActivatedFocusedParticles, out var value3))
		{
			_passiveActivatedFocusedParticles = value3.As<NParticlesContainer>();
		}
		if (info.TryGetProperty(PropertyName._spineShaker, out var value4))
		{
			_spineShaker = value4.As<NShaker>();
		}
		if (info.TryGetProperty(PropertyName._evokeVfxSceneName, out var value5))
		{
			_evokeVfxSceneName = value5.As<string>();
		}
		if (info.TryGetProperty(PropertyName._forcedFocusPower, out var value6))
		{
			_forcedFocusPower = value6.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._overrideCombatVfxContainer, out var value7))
		{
			_overrideCombatVfxContainer = value7.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._overridePlayerNode, out var value8))
		{
			_overridePlayerNode = value8.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._shakeTween, out var value9))
		{
			_shakeTween = value9.As<Tween>();
		}
	}
}

using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[GlobalClass]
[ScriptPath("res://src/Core/Nodes/Vfx/NNecrobinderVfx.cs")]
public class NNecrobinderVfx : Node
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
		/// Cached name for the 'OnAnimationEvent' method.
		/// </summary>
		public static readonly StringName OnAnimationEvent = "OnAnimationEvent";

		/// <summary>
		/// Cached name for the 'UpdateFlameVisibility' method.
		/// </summary>
		public static readonly StringName UpdateFlameVisibility = "UpdateFlameVisibility";

		/// <summary>
		/// Cached name for the 'OnScytheFlame1' method.
		/// </summary>
		public static readonly StringName OnScytheFlame1 = "OnScytheFlame1";

		/// <summary>
		/// Cached name for the 'OnScytheFlame2' method.
		/// </summary>
		public static readonly StringName OnScytheFlame2 = "OnScytheFlame2";

		/// <summary>
		/// Cached name for the 'OnAttackSlashStart' method.
		/// </summary>
		public static readonly StringName OnAttackSlashStart = "OnAttackSlashStart";

		/// <summary>
		/// Cached name for the 'OnHurtParticlesStart' method.
		/// </summary>
		public static readonly StringName OnHurtParticlesStart = "OnHurtParticlesStart";

		/// <summary>
		/// Cached name for the 'OnLowHealthStart' method.
		/// </summary>
		public static readonly StringName OnLowHealthStart = "OnLowHealthStart";

		/// <summary>
		/// Cached name for the 'OnLowHealthEnd' method.
		/// </summary>
		public static readonly StringName OnLowHealthEnd = "OnLowHealthEnd";

		/// <summary>
		/// Cached name for the '_ExitTree' method.
		/// </summary>
		public new static readonly StringName _ExitTree = "_ExitTree";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Node.PropertyName
	{
		/// <summary>
		/// Cached name for the '_parent' field.
		/// </summary>
		public static readonly StringName _parent = "_parent";

		/// <summary>
		/// Cached name for the '_headRef' field.
		/// </summary>
		public static readonly StringName _headRef = "_headRef";

		/// <summary>
		/// Cached name for the '_slashShaderMat' field.
		/// </summary>
		public static readonly StringName _slashShaderMat = "_slashShaderMat";

		/// <summary>
		/// Cached name for the '_slashStepBase' field.
		/// </summary>
		public static readonly StringName _slashStepBase = "_slashStepBase";

		/// <summary>
		/// Cached name for the '_slashOpacityBase' field.
		/// </summary>
		public static readonly StringName _slashOpacityBase = "_slashOpacityBase";

		/// <summary>
		/// Cached name for the '_tween' field.
		/// </summary>
		public static readonly StringName _tween = "_tween";

		/// <summary>
		/// Cached name for the '_tween2' field.
		/// </summary>
		public static readonly StringName _tween2 = "_tween2";

		/// <summary>
		/// Cached name for the '_scytheFireParticles1' field.
		/// </summary>
		public static readonly StringName _scytheFireParticles1 = "_scytheFireParticles1";

		/// <summary>
		/// Cached name for the '_scytheFireParticles2' field.
		/// </summary>
		public static readonly StringName _scytheFireParticles2 = "_scytheFireParticles2";

		/// <summary>
		/// Cached name for the '_hurtParticles' field.
		/// </summary>
		public static readonly StringName _hurtParticles = "_hurtParticles";

		/// <summary>
		/// Cached name for the '_lowHealthParticles' field.
		/// </summary>
		public static readonly StringName _lowHealthParticles = "_lowHealthParticles";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Node.SignalName
	{
	}

	private Node2D _parent;

	private MegaSprite _animController;

	private Node2D _headRef;

	private ShaderMaterial? _slashShaderMat;

	private float _slashStepBase = 0.04f;

	private float _slashOpacityBase = 0.4f;

	private static readonly StringName _masterStepString = new StringName("master_step");

	private static readonly StringName _opactyString = new StringName("opacity");

	private Tween _tween;

	private Tween _tween2;

	private GpuParticles2D? _scytheFireParticles1;

	private GpuParticles2D? _scytheFireParticles2;

	private GpuParticles2D? _hurtParticles;

	private GpuParticles2D? _lowHealthParticles;

	public override void _Ready()
	{
		_parent = GetParent<Node2D>();
		_headRef = _parent.GetNode<Node2D>("HeadBoneNode");
		Node nodeOrNull = _parent.GetNodeOrNull("SlashVfxSlot");
		_slashShaderMat = ((nodeOrNull != null) ? (new MegaSlotNode(nodeOrNull).GetNormalMaterial() as ShaderMaterial) : null);
		_scytheFireParticles1 = _parent.GetNodeOrNull<GpuParticles2D>("ScytheVfxSlot1/ScytheParticles");
		_scytheFireParticles2 = _parent.GetNodeOrNull<GpuParticles2D>("ScytheVfxSlot2/ScytheParticles");
		_hurtParticles = _parent.GetNodeOrNull<GpuParticles2D>("HeadBoneNode/HurtParticles");
		_lowHealthParticles = _parent.GetNodeOrNull<GpuParticles2D>("HeadBoneNode/LowHealthParticles");
		_scytheFireParticles1?.SetEmitting(emitting: false);
		_scytheFireParticles2?.SetEmitting(emitting: false);
		_scytheFireParticles1?.SetOneShot(secs: true);
		_scytheFireParticles2?.SetOneShot(secs: true);
		_hurtParticles?.SetEmitting(emitting: false);
		_hurtParticles?.SetOneShot(secs: true);
		_lowHealthParticles?.SetEmitting(emitting: false);
		_animController = new MegaSprite(_parent);
		_animController.ConnectAnimationStarted(Callable.From<GodotObject, GodotObject, GodotObject>(UpdateFlameVisibility));
		_animController.ConnectAnimationEvent(Callable.From<GodotObject, GodotObject, GodotObject, GodotObject>(OnAnimationEvent));
	}

	private void OnAnimationEvent(GodotObject _, GodotObject __, GodotObject ___, GodotObject spineEvent)
	{
		switch (new MegaEvent(spineEvent).GetData().GetEventName())
		{
		case "scythe_fx1":
			OnScytheFlame1();
			break;
		case "scythe_fx2":
			OnScytheFlame2();
			break;
		case "attack_slash_start":
			OnAttackSlashStart();
			break;
		case "low_health_start":
			OnLowHealthStart();
			break;
		case "low_health_end":
			OnLowHealthEnd();
			break;
		case "hurt_particles_start":
			OnHurtParticlesStart();
			break;
		}
	}

	private void UpdateFlameVisibility(GodotObject spineSprite, GodotObject animationState, GodotObject trackEntry)
	{
		_headRef.Visible = new MegaAnimationState(animationState).GetCurrentAnimationName() != "die";
	}

	private void OnScytheFlame1()
	{
		_scytheFireParticles1?.Restart();
	}

	private void OnScytheFlame2()
	{
		_scytheFireParticles2?.Restart();
	}

	private void OnAttackSlashStart()
	{
		_slashShaderMat?.SetShaderParameter(_masterStepString, _slashStepBase);
		_tween?.Kill();
		_tween = CreateTween().SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Quad);
		float num = 0.235f;
		_tween.TweenProperty(_slashShaderMat, "shader_parameter/master_step", num, 0.4000000059604645);
		_slashShaderMat?.SetShaderParameter(_opactyString, _slashOpacityBase);
		_tween2?.Kill();
		_tween2 = CreateTween().SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Quad);
		float num2 = 0f;
		_tween2.TweenProperty(_slashShaderMat, "shader_parameter/opacity", num2, 0.44999998807907104);
	}

	private void OnHurtParticlesStart()
	{
		_hurtParticles?.Restart();
	}

	private void OnLowHealthStart()
	{
		_lowHealthParticles?.SetEmitting(emitting: true);
	}

	private void OnLowHealthEnd()
	{
		_lowHealthParticles?.SetEmitting(emitting: false);
	}

	public override void _ExitTree()
	{
		_tween?.Kill();
		_tween2?.Kill();
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(10);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnAnimationEvent, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "__", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "___", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "spineEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.UpdateFlameVisibility, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "spineSprite", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "animationState", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "trackEntry", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnScytheFlame1, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnScytheFlame2, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnAttackSlashStart, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnHurtParticlesStart, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnLowHealthStart, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnLowHealthEnd, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.OnAnimationEvent && args.Count == 4)
		{
			OnAnimationEvent(VariantUtils.ConvertTo<GodotObject>(in args[0]), VariantUtils.ConvertTo<GodotObject>(in args[1]), VariantUtils.ConvertTo<GodotObject>(in args[2]), VariantUtils.ConvertTo<GodotObject>(in args[3]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateFlameVisibility && args.Count == 3)
		{
			UpdateFlameVisibility(VariantUtils.ConvertTo<GodotObject>(in args[0]), VariantUtils.ConvertTo<GodotObject>(in args[1]), VariantUtils.ConvertTo<GodotObject>(in args[2]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnScytheFlame1 && args.Count == 0)
		{
			OnScytheFlame1();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnScytheFlame2 && args.Count == 0)
		{
			OnScytheFlame2();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnAttackSlashStart && args.Count == 0)
		{
			OnAttackSlashStart();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnHurtParticlesStart && args.Count == 0)
		{
			OnHurtParticlesStart();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnLowHealthStart && args.Count == 0)
		{
			OnLowHealthStart();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnLowHealthEnd && args.Count == 0)
		{
			OnLowHealthEnd();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
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
		if (method == MethodName.OnAnimationEvent)
		{
			return true;
		}
		if (method == MethodName.UpdateFlameVisibility)
		{
			return true;
		}
		if (method == MethodName.OnScytheFlame1)
		{
			return true;
		}
		if (method == MethodName.OnScytheFlame2)
		{
			return true;
		}
		if (method == MethodName.OnAttackSlashStart)
		{
			return true;
		}
		if (method == MethodName.OnHurtParticlesStart)
		{
			return true;
		}
		if (method == MethodName.OnLowHealthStart)
		{
			return true;
		}
		if (method == MethodName.OnLowHealthEnd)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._parent)
		{
			_parent = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._headRef)
		{
			_headRef = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._slashShaderMat)
		{
			_slashShaderMat = VariantUtils.ConvertTo<ShaderMaterial>(in value);
			return true;
		}
		if (name == PropertyName._slashStepBase)
		{
			_slashStepBase = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._slashOpacityBase)
		{
			_slashOpacityBase = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._tween)
		{
			_tween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._tween2)
		{
			_tween2 = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._scytheFireParticles1)
		{
			_scytheFireParticles1 = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._scytheFireParticles2)
		{
			_scytheFireParticles2 = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._hurtParticles)
		{
			_hurtParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._lowHealthParticles)
		{
			_lowHealthParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._parent)
		{
			value = VariantUtils.CreateFrom(in _parent);
			return true;
		}
		if (name == PropertyName._headRef)
		{
			value = VariantUtils.CreateFrom(in _headRef);
			return true;
		}
		if (name == PropertyName._slashShaderMat)
		{
			value = VariantUtils.CreateFrom(in _slashShaderMat);
			return true;
		}
		if (name == PropertyName._slashStepBase)
		{
			value = VariantUtils.CreateFrom(in _slashStepBase);
			return true;
		}
		if (name == PropertyName._slashOpacityBase)
		{
			value = VariantUtils.CreateFrom(in _slashOpacityBase);
			return true;
		}
		if (name == PropertyName._tween)
		{
			value = VariantUtils.CreateFrom(in _tween);
			return true;
		}
		if (name == PropertyName._tween2)
		{
			value = VariantUtils.CreateFrom(in _tween2);
			return true;
		}
		if (name == PropertyName._scytheFireParticles1)
		{
			value = VariantUtils.CreateFrom(in _scytheFireParticles1);
			return true;
		}
		if (name == PropertyName._scytheFireParticles2)
		{
			value = VariantUtils.CreateFrom(in _scytheFireParticles2);
			return true;
		}
		if (name == PropertyName._hurtParticles)
		{
			value = VariantUtils.CreateFrom(in _hurtParticles);
			return true;
		}
		if (name == PropertyName._lowHealthParticles)
		{
			value = VariantUtils.CreateFrom(in _lowHealthParticles);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._parent, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._headRef, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._slashShaderMat, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._slashStepBase, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._slashOpacityBase, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween2, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._scytheFireParticles1, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._scytheFireParticles2, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hurtParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._lowHealthParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._parent, Variant.From(in _parent));
		info.AddProperty(PropertyName._headRef, Variant.From(in _headRef));
		info.AddProperty(PropertyName._slashShaderMat, Variant.From(in _slashShaderMat));
		info.AddProperty(PropertyName._slashStepBase, Variant.From(in _slashStepBase));
		info.AddProperty(PropertyName._slashOpacityBase, Variant.From(in _slashOpacityBase));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
		info.AddProperty(PropertyName._tween2, Variant.From(in _tween2));
		info.AddProperty(PropertyName._scytheFireParticles1, Variant.From(in _scytheFireParticles1));
		info.AddProperty(PropertyName._scytheFireParticles2, Variant.From(in _scytheFireParticles2));
		info.AddProperty(PropertyName._hurtParticles, Variant.From(in _hurtParticles));
		info.AddProperty(PropertyName._lowHealthParticles, Variant.From(in _lowHealthParticles));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._parent, out var value))
		{
			_parent = value.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._headRef, out var value2))
		{
			_headRef = value2.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._slashShaderMat, out var value3))
		{
			_slashShaderMat = value3.As<ShaderMaterial>();
		}
		if (info.TryGetProperty(PropertyName._slashStepBase, out var value4))
		{
			_slashStepBase = value4.As<float>();
		}
		if (info.TryGetProperty(PropertyName._slashOpacityBase, out var value5))
		{
			_slashOpacityBase = value5.As<float>();
		}
		if (info.TryGetProperty(PropertyName._tween, out var value6))
		{
			_tween = value6.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._tween2, out var value7))
		{
			_tween2 = value7.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._scytheFireParticles1, out var value8))
		{
			_scytheFireParticles1 = value8.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._scytheFireParticles2, out var value9))
		{
			_scytheFireParticles2 = value9.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._hurtParticles, out var value10))
		{
			_hurtParticles = value10.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._lowHealthParticles, out var value11))
		{
			_lowHealthParticles = value11.As<GpuParticles2D>();
		}
	}
}

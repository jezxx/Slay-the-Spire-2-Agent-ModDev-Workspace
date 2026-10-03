using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Helpers;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[GlobalClass]
[ScriptPath("res://src/Core/Nodes/Vfx/NTestSubjectVfx.cs")]
public class NTestSubjectVfx : Node
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
		/// Cached name for the 'PlayAnim1' method.
		/// </summary>
		public static readonly StringName PlayAnim1 = "PlayAnim1";

		/// <summary>
		/// Cached name for the 'OnSquirtNeck' method.
		/// </summary>
		public static readonly StringName OnSquirtNeck = "OnSquirtNeck";

		/// <summary>
		/// Cached name for the 'OnStartDizzies' method.
		/// </summary>
		public static readonly StringName OnStartDizzies = "OnStartDizzies";

		/// <summary>
		/// Cached name for the 'OnEndDizzies' method.
		/// </summary>
		public static readonly StringName OnEndDizzies = "OnEndDizzies";

		/// <summary>
		/// Cached name for the 'OnStartEmbers' method.
		/// </summary>
		public static readonly StringName OnStartEmbers = "OnStartEmbers";

		/// <summary>
		/// Cached name for the 'OnStartFlames' method.
		/// </summary>
		public static readonly StringName OnStartFlames = "OnStartFlames";

		/// <summary>
		/// Cached name for the 'OnEndFlames' method.
		/// </summary>
		public static readonly StringName OnEndFlames = "OnEndFlames";

		/// <summary>
		/// Cached name for the 'OnStartBurnVfx' method.
		/// </summary>
		public static readonly StringName OnStartBurnVfx = "OnStartBurnVfx";

		/// <summary>
		/// Cached name for the 'OnEndBurnVfx' method.
		/// </summary>
		public static readonly StringName OnEndBurnVfx = "OnEndBurnVfx";

		/// <summary>
		/// Cached name for the 'TweenOutBurnFire' method.
		/// </summary>
		public static readonly StringName TweenOutBurnFire = "TweenOutBurnFire";

		/// <summary>
		/// Cached name for the 'ClearBurnFire' method.
		/// </summary>
		public static readonly StringName ClearBurnFire = "ClearBurnFire";

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
		/// Cached name for the '_neckParticles' field.
		/// </summary>
		public static readonly StringName _neckParticles = "_neckParticles";

		/// <summary>
		/// Cached name for the '_dizzyParticles' field.
		/// </summary>
		public static readonly StringName _dizzyParticles = "_dizzyParticles";

		/// <summary>
		/// Cached name for the '_emberParticles' field.
		/// </summary>
		public static readonly StringName _emberParticles = "_emberParticles";

		/// <summary>
		/// Cached name for the '_flameParticles' field.
		/// </summary>
		public static readonly StringName _flameParticles = "_flameParticles";

		/// <summary>
		/// Cached name for the '_burnParticles' field.
		/// </summary>
		public static readonly StringName _burnParticles = "_burnParticles";

		/// <summary>
		/// Cached name for the '_targetedBurnParticle' field.
		/// </summary>
		public static readonly StringName _targetedBurnParticle = "_targetedBurnParticle";

		/// <summary>
		/// Cached name for the '_burnParticleFountain' field.
		/// </summary>
		public static readonly StringName _burnParticleFountain = "_burnParticleFountain";

		/// <summary>
		/// Cached name for the '_burnParticleContainer' field.
		/// </summary>
		public static readonly StringName _burnParticleContainer = "_burnParticleContainer";

		/// <summary>
		/// Cached name for the '_burnFire1' field.
		/// </summary>
		public static readonly StringName _burnFire1 = "_burnFire1";

		/// <summary>
		/// Cached name for the '_burnFire2' field.
		/// </summary>
		public static readonly StringName _burnFire2 = "_burnFire2";

		/// <summary>
		/// Cached name for the '_burnFire3' field.
		/// </summary>
		public static readonly StringName _burnFire3 = "_burnFire3";

		/// <summary>
		/// Cached name for the '_burnTween1' field.
		/// </summary>
		public static readonly StringName _burnTween1 = "_burnTween1";

		/// <summary>
		/// Cached name for the '_burnTween2' field.
		/// </summary>
		public static readonly StringName _burnTween2 = "_burnTween2";

		/// <summary>
		/// Cached name for the '_burnTween3' field.
		/// </summary>
		public static readonly StringName _burnTween3 = "_burnTween3";

		/// <summary>
		/// Cached name for the '_burnFire1Scale' field.
		/// </summary>
		public static readonly StringName _burnFire1Scale = "_burnFire1Scale";

		/// <summary>
		/// Cached name for the '_burnFire2Scale' field.
		/// </summary>
		public static readonly StringName _burnFire2Scale = "_burnFire2Scale";

		/// <summary>
		/// Cached name for the '_burnFire3Scale' field.
		/// </summary>
		public static readonly StringName _burnFire3Scale = "_burnFire3Scale";

		/// <summary>
		/// Cached name for the '_burnParticleGlobalScale' field.
		/// </summary>
		public static readonly StringName _burnParticleGlobalScale = "_burnParticleGlobalScale";

		/// <summary>
		/// Cached name for the '_parent' field.
		/// </summary>
		public static readonly StringName _parent = "_parent";

		/// <summary>
		/// Cached name for the '_keyDown' field.
		/// </summary>
		public static readonly StringName _keyDown = "_keyDown";

		/// <summary>
		/// Cached name for the '_doingThing' field.
		/// </summary>
		public static readonly StringName _doingThing = "_doingThing";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Node.SignalName
	{
	}

	private GpuParticles2D _neckParticles;

	private GpuParticles2D _dizzyParticles;

	private GpuParticles2D _emberParticles;

	private GpuParticles2D _flameParticles;

	private GpuParticles2D _burnParticles;

	private GpuParticles2D _targetedBurnParticle;

	private GpuParticles2D _burnParticleFountain;

	private Node2D _burnParticleContainer;

	private TextureRect _burnFire1;

	private TextureRect _burnFire2;

	private TextureRect _burnFire3;

	private Tween? _burnTween1;

	private Tween? _burnTween2;

	private Tween? _burnTween3;

	private Vector2 _burnFire1Scale;

	private Vector2 _burnFire2Scale;

	private Vector2 _burnFire3Scale;

	private Vector2 _burnParticleGlobalScale;

	private Node2D _parent;

	private MegaSprite _animController;

	private MegaSprite _frontBurnVfxController;

	private MegaSprite _backBurnVfxController;

	private bool _keyDown;

	private bool _doingThing;

	public override void _Ready()
	{
		_parent = GetParent<Node2D>();
		_animController = new MegaSprite(_parent);
		_frontBurnVfxController = new MegaSprite(GetNode("../FrontBurnVfxSlot/FrontBurnVfx"));
		_backBurnVfxController = new MegaSprite(GetNode("../BackBurnVfxSlot/BackBurnVfx"));
		_animController.ConnectAnimationEvent(Callable.From<GodotObject, GodotObject, GodotObject, GodotObject>(OnAnimationEvent));
		_neckParticles = _parent.GetNode<GpuParticles2D>("NeckParticlesSlot/NeckParticles");
		_dizzyParticles = _parent.GetNode<GpuParticles2D>("NeckParticlesSlot/DizzyPaticles");
		_emberParticles = _parent.GetNode<GpuParticles2D>("../../EmberParticles");
		_flameParticles = _parent.GetNode<GpuParticles2D>("../../FlameParticles");
		_burnParticles = _parent.GetNode<GpuParticles2D>("../../BurnParticleContainer/BurnParticles");
		_targetedBurnParticle = _parent.GetNode<GpuParticles2D>("../../BurnParticleContainer/TargetedBurnParticle");
		_burnParticleFountain = _parent.GetNode<GpuParticles2D>("../../BurnParticleContainer/BurnParticleFountain");
		_burnParticleContainer = _parent.GetNode<Node2D>("../../BurnParticleContainer");
		_burnFire1 = _parent.GetNode<TextureRect>("../../BurnFire1");
		_burnFire2 = _parent.GetNode<TextureRect>("../../BurnFire2");
		_burnFire3 = _parent.GetNode<TextureRect>("../../BurnFire3");
		_neckParticles.OneShot = true;
		_neckParticles.Emitting = false;
		_dizzyParticles.Emitting = false;
		_emberParticles.OneShot = true;
		_emberParticles.Emitting = false;
		_flameParticles.Emitting = false;
		_burnParticles.Emitting = false;
		_targetedBurnParticle.Emitting = false;
		_burnParticleFountain.Emitting = false;
		_burnParticleGlobalScale = _burnParticleContainer.GlobalScale;
		_burnFire1.Visible = false;
		_burnFire2.Visible = false;
		_burnFire3.Visible = false;
		_burnFire1Scale = _burnFire1.Scale;
		_burnFire2Scale = _burnFire2.Scale;
		_burnFire3Scale = _burnFire3.Scale;
		this.RunWhenSpineReady(_animController, delegate(MegaAnimationState animState)
		{
			animState.SetAnimation("idle_loop3");
		});
		this.RunWhenSpineReady(_frontBurnVfxController, delegate(MegaAnimationState animState)
		{
			animState.SetAnimation("empty");
		});
		this.RunWhenSpineReady(_backBurnVfxController, delegate(MegaAnimationState animState)
		{
			animState.SetAnimation("empty");
		});
	}

	private void OnAnimationEvent(GodotObject _, GodotObject __, GodotObject ___, GodotObject spineEvent)
	{
		string eventName = new MegaEvent(spineEvent).GetData().GetEventName();
		if (eventName == null)
		{
			return;
		}
		switch (eventName.Length)
		{
		case 12:
			switch (eventName[6])
			{
			case 'x':
				if (eventName == "neck_explode")
				{
					OnSquirtNeck();
				}
				break;
			case 'e':
				if (eventName == "start_embers")
				{
					OnStartEmbers();
				}
				break;
			case 'f':
				if (eventName == "start_flames")
				{
					OnStartFlames();
				}
				break;
			case 'r':
				if (eventName == "end_burn_vfx")
				{
					OnEndBurnVfx();
				}
				break;
			}
			break;
		case 13:
			if (eventName == "start_dizzies")
			{
				OnStartDizzies();
			}
			break;
		case 11:
			if (eventName == "end_dizzies")
			{
				OnEndDizzies();
			}
			break;
		case 10:
			if (eventName == "end_flames")
			{
				OnEndFlames();
			}
			break;
		case 14:
			if (eventName == "start_burn_vfx")
			{
				OnStartBurnVfx();
			}
			break;
		}
	}

	private void PlayAnim1()
	{
		_animController.GetAnimationState().SetAnimation("die3", loop: false);
		_animController.GetAnimationState().AddAnimation("idle_loop3");
	}

	private void OnSquirtNeck()
	{
		_neckParticles.Restart();
	}

	private void OnStartDizzies()
	{
		if (!_dizzyParticles.Emitting)
		{
			_dizzyParticles.Emitting = true;
		}
	}

	private void OnEndDizzies()
	{
		_dizzyParticles.Emitting = false;
	}

	private void OnStartEmbers()
	{
		_emberParticles.Restart();
	}

	private void OnStartFlames()
	{
		_flameParticles.Emitting = true;
	}

	private void OnEndFlames()
	{
		_flameParticles.Emitting = false;
	}

	private void OnStartBurnVfx()
	{
		_burnParticleContainer.GlobalScale = _burnParticleGlobalScale;
		_frontBurnVfxController.GetAnimationState().SetAnimation("burn", loop: false);
		_backBurnVfxController.GetAnimationState().SetAnimation("burn", loop: false);
		_burnParticles.Restart();
		_targetedBurnParticle.Emitting = true;
		_burnParticleFountain.Restart();
		TextureRect burnFire = _burnFire1;
		TextureRect burnFire2 = _burnFire2;
		bool flag2 = (_burnFire3.Visible = true);
		bool visible = (burnFire2.Visible = flag2);
		burnFire.Visible = visible;
		TextureRect burnFire3 = _burnFire1;
		TextureRect burnFire4 = _burnFire2;
		Vector2 vector = (_burnFire3.Scale = Vector2.Zero);
		Vector2 scale = (burnFire4.Scale = vector);
		burnFire3.Scale = scale;
		_burnTween1?.Kill();
		_burnTween2?.Kill();
		_burnTween3?.Kill();
		_burnTween1 = CreateTween();
		_burnTween2 = CreateTween();
		_burnTween3 = CreateTween();
		_burnTween1.TweenProperty(_burnFire1, "scale", _burnFire1Scale, 0.10000000149011612).SetDelay(0.20000000298023224);
		_burnTween2.TweenProperty(_burnFire2, "scale", _burnFire2Scale, 0.10000000149011612).SetDelay(0.20000000298023224);
		_burnTween3.TweenProperty(_burnFire3, "scale", _burnFire3Scale, 0.10000000149011612).SetDelay(0.30000001192092896);
		_burnTween3.TweenCallback(Callable.From(TweenOutBurnFire));
	}

	private void OnEndBurnVfx()
	{
		_burnParticles.Emitting = false;
		_targetedBurnParticle.Emitting = false;
		_burnParticleFountain.Emitting = false;
	}

	private void TweenOutBurnFire()
	{
		_burnTween1.Kill();
		_burnTween2.Kill();
		_burnTween3.Kill();
		_burnTween1 = CreateTween();
		_burnTween2 = CreateTween();
		_burnTween3 = CreateTween();
		Vector2 vector = new Vector2(0.2f, 0f);
		_burnTween1.TweenProperty(_burnFire1, "scale", vector, 0.800000011920929).SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Quad)
			.SetDelay(1.2000000476837158);
		_burnTween2.TweenProperty(_burnFire2, "scale", vector, 0.800000011920929).SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Quad)
			.SetDelay(1.100000023841858);
		_burnTween3.TweenProperty(_burnFire3, "scale", vector, 0.800000011920929).SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Quad)
			.SetDelay(1.0);
		_burnTween1.TweenCallback(Callable.From(ClearBurnFire));
	}

	private void ClearBurnFire()
	{
		TextureRect burnFire = _burnFire1;
		TextureRect burnFire2 = _burnFire2;
		bool flag2 = (_burnFire3.Visible = false);
		bool visible = (burnFire2.Visible = flag2);
		burnFire.Visible = visible;
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		_burnTween1?.Kill();
		_burnTween2?.Kill();
		_burnTween3?.Kill();
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(14);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnAnimationEvent, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "__", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "___", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "spineEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.PlayAnim1, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnSquirtNeck, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnStartDizzies, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnEndDizzies, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnStartEmbers, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnStartFlames, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnEndFlames, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnStartBurnVfx, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnEndBurnVfx, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.TweenOutBurnFire, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ClearBurnFire, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.PlayAnim1 && args.Count == 0)
		{
			PlayAnim1();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnSquirtNeck && args.Count == 0)
		{
			OnSquirtNeck();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnStartDizzies && args.Count == 0)
		{
			OnStartDizzies();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnEndDizzies && args.Count == 0)
		{
			OnEndDizzies();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnStartEmbers && args.Count == 0)
		{
			OnStartEmbers();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnStartFlames && args.Count == 0)
		{
			OnStartFlames();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnEndFlames && args.Count == 0)
		{
			OnEndFlames();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnStartBurnVfx && args.Count == 0)
		{
			OnStartBurnVfx();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnEndBurnVfx && args.Count == 0)
		{
			OnEndBurnVfx();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.TweenOutBurnFire && args.Count == 0)
		{
			TweenOutBurnFire();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ClearBurnFire && args.Count == 0)
		{
			ClearBurnFire();
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
		if (method == MethodName.PlayAnim1)
		{
			return true;
		}
		if (method == MethodName.OnSquirtNeck)
		{
			return true;
		}
		if (method == MethodName.OnStartDizzies)
		{
			return true;
		}
		if (method == MethodName.OnEndDizzies)
		{
			return true;
		}
		if (method == MethodName.OnStartEmbers)
		{
			return true;
		}
		if (method == MethodName.OnStartFlames)
		{
			return true;
		}
		if (method == MethodName.OnEndFlames)
		{
			return true;
		}
		if (method == MethodName.OnStartBurnVfx)
		{
			return true;
		}
		if (method == MethodName.OnEndBurnVfx)
		{
			return true;
		}
		if (method == MethodName.TweenOutBurnFire)
		{
			return true;
		}
		if (method == MethodName.ClearBurnFire)
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
		if (name == PropertyName._neckParticles)
		{
			_neckParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._dizzyParticles)
		{
			_dizzyParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._emberParticles)
		{
			_emberParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._flameParticles)
		{
			_flameParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._burnParticles)
		{
			_burnParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._targetedBurnParticle)
		{
			_targetedBurnParticle = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._burnParticleFountain)
		{
			_burnParticleFountain = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._burnParticleContainer)
		{
			_burnParticleContainer = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._burnFire1)
		{
			_burnFire1 = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._burnFire2)
		{
			_burnFire2 = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._burnFire3)
		{
			_burnFire3 = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._burnTween1)
		{
			_burnTween1 = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._burnTween2)
		{
			_burnTween2 = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._burnTween3)
		{
			_burnTween3 = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._burnFire1Scale)
		{
			_burnFire1Scale = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._burnFire2Scale)
		{
			_burnFire2Scale = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._burnFire3Scale)
		{
			_burnFire3Scale = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._burnParticleGlobalScale)
		{
			_burnParticleGlobalScale = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._parent)
		{
			_parent = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._keyDown)
		{
			_keyDown = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._doingThing)
		{
			_doingThing = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._neckParticles)
		{
			value = VariantUtils.CreateFrom(in _neckParticles);
			return true;
		}
		if (name == PropertyName._dizzyParticles)
		{
			value = VariantUtils.CreateFrom(in _dizzyParticles);
			return true;
		}
		if (name == PropertyName._emberParticles)
		{
			value = VariantUtils.CreateFrom(in _emberParticles);
			return true;
		}
		if (name == PropertyName._flameParticles)
		{
			value = VariantUtils.CreateFrom(in _flameParticles);
			return true;
		}
		if (name == PropertyName._burnParticles)
		{
			value = VariantUtils.CreateFrom(in _burnParticles);
			return true;
		}
		if (name == PropertyName._targetedBurnParticle)
		{
			value = VariantUtils.CreateFrom(in _targetedBurnParticle);
			return true;
		}
		if (name == PropertyName._burnParticleFountain)
		{
			value = VariantUtils.CreateFrom(in _burnParticleFountain);
			return true;
		}
		if (name == PropertyName._burnParticleContainer)
		{
			value = VariantUtils.CreateFrom(in _burnParticleContainer);
			return true;
		}
		if (name == PropertyName._burnFire1)
		{
			value = VariantUtils.CreateFrom(in _burnFire1);
			return true;
		}
		if (name == PropertyName._burnFire2)
		{
			value = VariantUtils.CreateFrom(in _burnFire2);
			return true;
		}
		if (name == PropertyName._burnFire3)
		{
			value = VariantUtils.CreateFrom(in _burnFire3);
			return true;
		}
		if (name == PropertyName._burnTween1)
		{
			value = VariantUtils.CreateFrom(in _burnTween1);
			return true;
		}
		if (name == PropertyName._burnTween2)
		{
			value = VariantUtils.CreateFrom(in _burnTween2);
			return true;
		}
		if (name == PropertyName._burnTween3)
		{
			value = VariantUtils.CreateFrom(in _burnTween3);
			return true;
		}
		if (name == PropertyName._burnFire1Scale)
		{
			value = VariantUtils.CreateFrom(in _burnFire1Scale);
			return true;
		}
		if (name == PropertyName._burnFire2Scale)
		{
			value = VariantUtils.CreateFrom(in _burnFire2Scale);
			return true;
		}
		if (name == PropertyName._burnFire3Scale)
		{
			value = VariantUtils.CreateFrom(in _burnFire3Scale);
			return true;
		}
		if (name == PropertyName._burnParticleGlobalScale)
		{
			value = VariantUtils.CreateFrom(in _burnParticleGlobalScale);
			return true;
		}
		if (name == PropertyName._parent)
		{
			value = VariantUtils.CreateFrom(in _parent);
			return true;
		}
		if (name == PropertyName._keyDown)
		{
			value = VariantUtils.CreateFrom(in _keyDown);
			return true;
		}
		if (name == PropertyName._doingThing)
		{
			value = VariantUtils.CreateFrom(in _doingThing);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._neckParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._dizzyParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._emberParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._flameParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._burnParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._targetedBurnParticle, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._burnParticleFountain, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._burnParticleContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._burnFire1, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._burnFire2, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._burnFire3, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._burnTween1, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._burnTween2, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._burnTween3, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._burnFire1Scale, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._burnFire2Scale, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._burnFire3Scale, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._burnParticleGlobalScale, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._parent, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._keyDown, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._doingThing, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._neckParticles, Variant.From(in _neckParticles));
		info.AddProperty(PropertyName._dizzyParticles, Variant.From(in _dizzyParticles));
		info.AddProperty(PropertyName._emberParticles, Variant.From(in _emberParticles));
		info.AddProperty(PropertyName._flameParticles, Variant.From(in _flameParticles));
		info.AddProperty(PropertyName._burnParticles, Variant.From(in _burnParticles));
		info.AddProperty(PropertyName._targetedBurnParticle, Variant.From(in _targetedBurnParticle));
		info.AddProperty(PropertyName._burnParticleFountain, Variant.From(in _burnParticleFountain));
		info.AddProperty(PropertyName._burnParticleContainer, Variant.From(in _burnParticleContainer));
		info.AddProperty(PropertyName._burnFire1, Variant.From(in _burnFire1));
		info.AddProperty(PropertyName._burnFire2, Variant.From(in _burnFire2));
		info.AddProperty(PropertyName._burnFire3, Variant.From(in _burnFire3));
		info.AddProperty(PropertyName._burnTween1, Variant.From(in _burnTween1));
		info.AddProperty(PropertyName._burnTween2, Variant.From(in _burnTween2));
		info.AddProperty(PropertyName._burnTween3, Variant.From(in _burnTween3));
		info.AddProperty(PropertyName._burnFire1Scale, Variant.From(in _burnFire1Scale));
		info.AddProperty(PropertyName._burnFire2Scale, Variant.From(in _burnFire2Scale));
		info.AddProperty(PropertyName._burnFire3Scale, Variant.From(in _burnFire3Scale));
		info.AddProperty(PropertyName._burnParticleGlobalScale, Variant.From(in _burnParticleGlobalScale));
		info.AddProperty(PropertyName._parent, Variant.From(in _parent));
		info.AddProperty(PropertyName._keyDown, Variant.From(in _keyDown));
		info.AddProperty(PropertyName._doingThing, Variant.From(in _doingThing));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._neckParticles, out var value))
		{
			_neckParticles = value.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._dizzyParticles, out var value2))
		{
			_dizzyParticles = value2.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._emberParticles, out var value3))
		{
			_emberParticles = value3.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._flameParticles, out var value4))
		{
			_flameParticles = value4.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._burnParticles, out var value5))
		{
			_burnParticles = value5.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._targetedBurnParticle, out var value6))
		{
			_targetedBurnParticle = value6.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._burnParticleFountain, out var value7))
		{
			_burnParticleFountain = value7.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._burnParticleContainer, out var value8))
		{
			_burnParticleContainer = value8.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._burnFire1, out var value9))
		{
			_burnFire1 = value9.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._burnFire2, out var value10))
		{
			_burnFire2 = value10.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._burnFire3, out var value11))
		{
			_burnFire3 = value11.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._burnTween1, out var value12))
		{
			_burnTween1 = value12.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._burnTween2, out var value13))
		{
			_burnTween2 = value13.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._burnTween3, out var value14))
		{
			_burnTween3 = value14.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._burnFire1Scale, out var value15))
		{
			_burnFire1Scale = value15.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._burnFire2Scale, out var value16))
		{
			_burnFire2Scale = value16.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._burnFire3Scale, out var value17))
		{
			_burnFire3Scale = value17.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._burnParticleGlobalScale, out var value18))
		{
			_burnParticleGlobalScale = value18.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._parent, out var value19))
		{
			_parent = value19.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._keyDown, out var value20))
		{
			_keyDown = value20.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._doingThing, out var value21))
		{
			_doingThing = value21.As<bool>();
		}
	}
}

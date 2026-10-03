using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Helpers;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[ScriptPath("res://src/Core/Nodes/Vfx/NAeonGlassVfx.cs")]
public class NAeonGlassVfx : Node
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
		/// Cached name for the 'OnAnimationStart' method.
		/// </summary>
		public static readonly StringName OnAnimationStart = "OnAnimationStart";

		/// <summary>
		/// Cached name for the 'StartWither' method.
		/// </summary>
		public static readonly StringName StartWither = "StartWither";

		/// <summary>
		/// Cached name for the 'EndWither' method.
		/// </summary>
		public static readonly StringName EndWither = "EndWither";

		/// <summary>
		/// Cached name for the 'StartDie' method.
		/// </summary>
		public static readonly StringName StartDie = "StartDie";

		/// <summary>
		/// Cached name for the 'EndDie' method.
		/// </summary>
		public static readonly StringName EndDie = "EndDie";

		/// <summary>
		/// Cached name for the 'StartSparks' method.
		/// </summary>
		public static readonly StringName StartSparks = "StartSparks";

		/// <summary>
		/// Cached name for the 'StartScrape' method.
		/// </summary>
		public static readonly StringName StartScrape = "StartScrape";

		/// <summary>
		/// Cached name for the 'EndScrape' method.
		/// </summary>
		public static readonly StringName EndScrape = "EndScrape";

		/// <summary>
		/// Cached name for the 'ResetVfx' method.
		/// </summary>
		public static readonly StringName ResetVfx = "ResetVfx";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Node.PropertyName
	{
		/// <summary>
		/// Cached name for the '_ringsSpinningNormal' field.
		/// </summary>
		public static readonly StringName _ringsSpinningNormal = "_ringsSpinningNormal";

		/// <summary>
		/// Cached name for the '_curAnimName' field.
		/// </summary>
		public static readonly StringName _curAnimName = "_curAnimName";

		/// <summary>
		/// Cached name for the '_liquidShaderMat' field.
		/// </summary>
		public static readonly StringName _liquidShaderMat = "_liquidShaderMat";

		/// <summary>
		/// Cached name for the '_baseScrollSpeed' field.
		/// </summary>
		public static readonly StringName _baseScrollSpeed = "_baseScrollSpeed";

		/// <summary>
		/// Cached name for the '_parent' field.
		/// </summary>
		public static readonly StringName _parent = "_parent";

		/// <summary>
		/// Cached name for the '_witherParticles' field.
		/// </summary>
		public static readonly StringName _witherParticles = "_witherParticles";

		/// <summary>
		/// Cached name for the '_leakParticles' field.
		/// </summary>
		public static readonly StringName _leakParticles = "_leakParticles";

		/// <summary>
		/// Cached name for the '_shardParticles' field.
		/// </summary>
		public static readonly StringName _shardParticles = "_shardParticles";

		/// <summary>
		/// Cached name for the '_dumpParticles' field.
		/// </summary>
		public static readonly StringName _dumpParticles = "_dumpParticles";

		/// <summary>
		/// Cached name for the '_topSparkParticles' field.
		/// </summary>
		public static readonly StringName _topSparkParticles = "_topSparkParticles";

		/// <summary>
		/// Cached name for the '_bottomSparkParticles' field.
		/// </summary>
		public static readonly StringName _bottomSparkParticles = "_bottomSparkParticles";

		/// <summary>
		/// Cached name for the '_groundDustParticles' field.
		/// </summary>
		public static readonly StringName _groundDustParticles = "_groundDustParticles";

		/// <summary>
		/// Cached name for the '_groundChunkParticles' field.
		/// </summary>
		public static readonly StringName _groundChunkParticles = "_groundChunkParticles";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Node.SignalName
	{
	}

	private bool _ringsSpinningNormal;

	private string? _curAnimName;

	private static readonly StringName _scrollSpeedString = new StringName("ScrollSpeed");

	private ShaderMaterial? _liquidShaderMat;

	private float _baseScrollSpeed;

	private Node2D _parent;

	private MegaSprite _animController;

	private GpuParticles2D _witherParticles;

	private GpuParticles2D _leakParticles;

	private GpuParticles2D _shardParticles;

	private GpuParticles2D _dumpParticles;

	private GpuParticles2D _topSparkParticles;

	private GpuParticles2D _bottomSparkParticles;

	private GpuParticles2D _groundDustParticles;

	private GpuParticles2D _groundChunkParticles;

	public override void _Ready()
	{
		_parent = GetParent<Node2D>();
		_animController = new MegaSprite(_parent);
		_liquidShaderMat = new MegaSlotNode(_parent.GetNode("LiquidSlot")).GetNormalMaterial() as ShaderMaterial;
		if (_liquidShaderMat != null)
		{
			_baseScrollSpeed = (float)_liquidShaderMat.GetShaderParameter(_scrollSpeedString);
		}
		_witherParticles = _parent.GetNode<GpuParticles2D>("WitherSlot/WitherParticles");
		_leakParticles = _parent.GetNode<GpuParticles2D>("LiquidSlot/LeakParticles");
		_shardParticles = _parent.GetNode<GpuParticles2D>("GlassCenterSlot/ShardParticles");
		_dumpParticles = _parent.GetNode<GpuParticles2D>("GlassCenterSlot/DumpParticles");
		_topSparkParticles = _parent.GetNode<GpuParticles2D>("TopSparksSlot/TopSparkParticles");
		_bottomSparkParticles = _parent.GetNode<GpuParticles2D>("BottomSparksSlot/BottomSparkParticles");
		_groundDustParticles = _parent.GetNode<GpuParticles2D>("GroundPlowSlot/DustParticles");
		_groundChunkParticles = _parent.GetNode<GpuParticles2D>("GroundPlowSlot/ChunkParticles");
		_bottomSparkParticles.OneShot = true;
		_topSparkParticles.OneShot = true;
		_witherParticles.OneShot = true;
		_shardParticles.OneShot = true;
		_dumpParticles.OneShot = true;
		ResetVfx();
		this.RunWhenSpineReady(_animController, delegate(MegaAnimationState animState)
		{
			animState.SetAnimation("_track1/rings_normal", loop: true, 1);
		});
		_animController.ConnectAnimationEvent(Callable.From<GodotObject, GodotObject, GodotObject, GodotObject>(OnAnimationEvent));
		_animController.ConnectAnimationStarted(Callable.From<GodotObject, GodotObject, GodotObject>(OnAnimationStart));
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
			case 'w':
				if (eventName == "start_wither")
				{
					StartWither();
				}
				break;
			case 's':
				if (eventName == "start_sparks")
				{
					StartSparks();
				}
				break;
			}
			break;
		case 10:
			if (eventName == "end_wither")
			{
				EndWither();
			}
			break;
		case 9:
			if (eventName == "start_die")
			{
				StartDie();
			}
			break;
		case 7:
			if (eventName == "end_die")
			{
				EndDie();
			}
			break;
		case 19:
			if (eventName == "start_ground_scrape")
			{
				StartScrape();
			}
			break;
		case 17:
			if (eventName == "end_ground_scrape")
			{
				EndScrape();
			}
			break;
		}
	}

	private void OnAnimationStart(GodotObject spineSprite, GodotObject animationState, GodotObject trackEntry)
	{
		ResetVfx();
		MegaAnimationState animationState2 = _animController.GetAnimationState();
		string currentAnimationName = animationState2.GetCurrentAnimationName();
		if (currentAnimationName == _curAnimName)
		{
			return;
		}
		_curAnimName = currentAnimationName;
		switch (currentAnimationName)
		{
		case "idle_loop":
		case "hurt":
		case "wither":
			if (!_ringsSpinningNormal)
			{
				animationState2.SetAnimation("_track1/rings_normal", loop: true, 1);
			}
			_ringsSpinningNormal = true;
			break;
		}
		switch (currentAnimationName)
		{
		case "attack_heavy":
			animationState2.SetAnimation("_track1/rings_attack_heavy", loop: false, 1);
			animationState2.AddAnimation("_track1/rings_normal", 0f, loop: true, 1);
			_ringsSpinningNormal = false;
			break;
		case "attack_double":
			animationState2.SetAnimation("_track1/rings_attack_double", loop: false, 1);
			animationState2.AddAnimation("_track1/rings_normal", 0f, loop: true, 1);
			_ringsSpinningNormal = false;
			break;
		case "die":
			animationState2.SetAnimation("_track1/rings_die", loop: false, 1);
			_ringsSpinningNormal = false;
			break;
		}
	}

	private void StartWither()
	{
		_witherParticles.Restart();
		_liquidShaderMat?.SetShaderParameter(_scrollSpeedString, -2f);
	}

	private void EndWither()
	{
		ResetVfx();
	}

	private void StartDie()
	{
		_dumpParticles.Restart();
		_leakParticles.Restart();
		_shardParticles.Restart();
	}

	private void EndDie()
	{
		_leakParticles.Emitting = false;
	}

	private void StartSparks()
	{
		_topSparkParticles.Restart();
		_bottomSparkParticles.Restart();
	}

	private void StartScrape()
	{
		_groundChunkParticles.Restart();
		_groundDustParticles.Restart();
	}

	private void EndScrape()
	{
		_groundChunkParticles.Emitting = false;
		_groundDustParticles.Emitting = false;
	}

	private void ResetVfx()
	{
		_liquidShaderMat?.SetShaderParameter(_scrollSpeedString, _baseScrollSpeed);
		_witherParticles.Restart();
		_witherParticles.Emitting = false;
		_leakParticles.Restart();
		_leakParticles.Emitting = false;
		_shardParticles.Emitting = false;
		_dumpParticles.Restart();
		_dumpParticles.Emitting = false;
		_topSparkParticles.Emitting = false;
		_bottomSparkParticles.Emitting = false;
		_groundChunkParticles.Emitting = false;
		_groundDustParticles.Emitting = false;
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(11);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnAnimationEvent, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "__", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "___", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "spineEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnAnimationStart, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "spineSprite", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "animationState", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "trackEntry", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.StartWither, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.EndWither, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.StartDie, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.EndDie, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.StartSparks, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.StartScrape, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.EndScrape, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ResetVfx, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.OnAnimationStart && args.Count == 3)
		{
			OnAnimationStart(VariantUtils.ConvertTo<GodotObject>(in args[0]), VariantUtils.ConvertTo<GodotObject>(in args[1]), VariantUtils.ConvertTo<GodotObject>(in args[2]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.StartWither && args.Count == 0)
		{
			StartWither();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.EndWither && args.Count == 0)
		{
			EndWither();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.StartDie && args.Count == 0)
		{
			StartDie();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.EndDie && args.Count == 0)
		{
			EndDie();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.StartSparks && args.Count == 0)
		{
			StartSparks();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.StartScrape && args.Count == 0)
		{
			StartScrape();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.EndScrape && args.Count == 0)
		{
			EndScrape();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ResetVfx && args.Count == 0)
		{
			ResetVfx();
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
		if (method == MethodName.OnAnimationStart)
		{
			return true;
		}
		if (method == MethodName.StartWither)
		{
			return true;
		}
		if (method == MethodName.EndWither)
		{
			return true;
		}
		if (method == MethodName.StartDie)
		{
			return true;
		}
		if (method == MethodName.EndDie)
		{
			return true;
		}
		if (method == MethodName.StartSparks)
		{
			return true;
		}
		if (method == MethodName.StartScrape)
		{
			return true;
		}
		if (method == MethodName.EndScrape)
		{
			return true;
		}
		if (method == MethodName.ResetVfx)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._ringsSpinningNormal)
		{
			_ringsSpinningNormal = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._curAnimName)
		{
			_curAnimName = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		if (name == PropertyName._liquidShaderMat)
		{
			_liquidShaderMat = VariantUtils.ConvertTo<ShaderMaterial>(in value);
			return true;
		}
		if (name == PropertyName._baseScrollSpeed)
		{
			_baseScrollSpeed = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._parent)
		{
			_parent = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._witherParticles)
		{
			_witherParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._leakParticles)
		{
			_leakParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._shardParticles)
		{
			_shardParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._dumpParticles)
		{
			_dumpParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._topSparkParticles)
		{
			_topSparkParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._bottomSparkParticles)
		{
			_bottomSparkParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._groundDustParticles)
		{
			_groundDustParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._groundChunkParticles)
		{
			_groundChunkParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._ringsSpinningNormal)
		{
			value = VariantUtils.CreateFrom(in _ringsSpinningNormal);
			return true;
		}
		if (name == PropertyName._curAnimName)
		{
			value = VariantUtils.CreateFrom(in _curAnimName);
			return true;
		}
		if (name == PropertyName._liquidShaderMat)
		{
			value = VariantUtils.CreateFrom(in _liquidShaderMat);
			return true;
		}
		if (name == PropertyName._baseScrollSpeed)
		{
			value = VariantUtils.CreateFrom(in _baseScrollSpeed);
			return true;
		}
		if (name == PropertyName._parent)
		{
			value = VariantUtils.CreateFrom(in _parent);
			return true;
		}
		if (name == PropertyName._witherParticles)
		{
			value = VariantUtils.CreateFrom(in _witherParticles);
			return true;
		}
		if (name == PropertyName._leakParticles)
		{
			value = VariantUtils.CreateFrom(in _leakParticles);
			return true;
		}
		if (name == PropertyName._shardParticles)
		{
			value = VariantUtils.CreateFrom(in _shardParticles);
			return true;
		}
		if (name == PropertyName._dumpParticles)
		{
			value = VariantUtils.CreateFrom(in _dumpParticles);
			return true;
		}
		if (name == PropertyName._topSparkParticles)
		{
			value = VariantUtils.CreateFrom(in _topSparkParticles);
			return true;
		}
		if (name == PropertyName._bottomSparkParticles)
		{
			value = VariantUtils.CreateFrom(in _bottomSparkParticles);
			return true;
		}
		if (name == PropertyName._groundDustParticles)
		{
			value = VariantUtils.CreateFrom(in _groundDustParticles);
			return true;
		}
		if (name == PropertyName._groundChunkParticles)
		{
			value = VariantUtils.CreateFrom(in _groundChunkParticles);
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
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._ringsSpinningNormal, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.String, PropertyName._curAnimName, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._liquidShaderMat, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._baseScrollSpeed, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._parent, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._witherParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._leakParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._shardParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._dumpParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._topSparkParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._bottomSparkParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._groundDustParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._groundChunkParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._ringsSpinningNormal, Variant.From(in _ringsSpinningNormal));
		info.AddProperty(PropertyName._curAnimName, Variant.From(in _curAnimName));
		info.AddProperty(PropertyName._liquidShaderMat, Variant.From(in _liquidShaderMat));
		info.AddProperty(PropertyName._baseScrollSpeed, Variant.From(in _baseScrollSpeed));
		info.AddProperty(PropertyName._parent, Variant.From(in _parent));
		info.AddProperty(PropertyName._witherParticles, Variant.From(in _witherParticles));
		info.AddProperty(PropertyName._leakParticles, Variant.From(in _leakParticles));
		info.AddProperty(PropertyName._shardParticles, Variant.From(in _shardParticles));
		info.AddProperty(PropertyName._dumpParticles, Variant.From(in _dumpParticles));
		info.AddProperty(PropertyName._topSparkParticles, Variant.From(in _topSparkParticles));
		info.AddProperty(PropertyName._bottomSparkParticles, Variant.From(in _bottomSparkParticles));
		info.AddProperty(PropertyName._groundDustParticles, Variant.From(in _groundDustParticles));
		info.AddProperty(PropertyName._groundChunkParticles, Variant.From(in _groundChunkParticles));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._ringsSpinningNormal, out var value))
		{
			_ringsSpinningNormal = value.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._curAnimName, out var value2))
		{
			_curAnimName = value2.As<string>();
		}
		if (info.TryGetProperty(PropertyName._liquidShaderMat, out var value3))
		{
			_liquidShaderMat = value3.As<ShaderMaterial>();
		}
		if (info.TryGetProperty(PropertyName._baseScrollSpeed, out var value4))
		{
			_baseScrollSpeed = value4.As<float>();
		}
		if (info.TryGetProperty(PropertyName._parent, out var value5))
		{
			_parent = value5.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._witherParticles, out var value6))
		{
			_witherParticles = value6.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._leakParticles, out var value7))
		{
			_leakParticles = value7.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._shardParticles, out var value8))
		{
			_shardParticles = value8.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._dumpParticles, out var value9))
		{
			_dumpParticles = value9.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._topSparkParticles, out var value10))
		{
			_topSparkParticles = value10.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._bottomSparkParticles, out var value11))
		{
			_bottomSparkParticles = value11.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._groundDustParticles, out var value12))
		{
			_groundDustParticles = value12.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._groundChunkParticles, out var value13))
		{
			_groundChunkParticles = value13.As<GpuParticles2D>();
		}
	}
}

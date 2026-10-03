using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Vfx.Forms;
using MegaCrit.Sts2.Core.Saves;

namespace MegaCrit.Sts2.Core.Nodes.Combat;

[ScriptPath("res://src/Core/Nodes/Combat/NCreatureVisuals.cs")]
public class NCreatureVisuals : Node2D
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : Node2D.MethodName
	{
		/// <summary>
		/// Cached name for the 'GetCurrentBody' method.
		/// </summary>
		public static readonly StringName GetCurrentBody = "GetCurrentBody";

		/// <summary>
		/// Cached name for the '_Ready' method.
		/// </summary>
		public new static readonly StringName _Ready = "_Ready";

		/// <summary>
		/// Cached name for the 'AddFormVfx' method.
		/// </summary>
		public static readonly StringName AddFormVfx = "AddFormVfx";

		/// <summary>
		/// Cached name for the 'RemoveFormVfx' method.
		/// </summary>
		public static readonly StringName RemoveFormVfx = "RemoveFormVfx";

		/// <summary>
		/// Cached name for the '_EnterTree' method.
		/// </summary>
		public new static readonly StringName _EnterTree = "_EnterTree";

		/// <summary>
		/// Cached name for the '_ExitTree' method.
		/// </summary>
		public new static readonly StringName _ExitTree = "_ExitTree";

		/// <summary>
		/// Cached name for the 'SetScaleAndHue' method.
		/// </summary>
		public static readonly StringName SetScaleAndHue = "SetScaleAndHue";

		/// <summary>
		/// Cached name for the 'IsPlayingHurtAnimation' method.
		/// </summary>
		public static readonly StringName IsPlayingHurtAnimation = "IsPlayingHurtAnimation";

		/// <summary>
		/// Cached name for the 'IsPlayingIdleAnimation' method.
		/// </summary>
		public static readonly StringName IsPlayingIdleAnimation = "IsPlayingIdleAnimation";

		/// <summary>
		/// Cached name for the 'TryApplyLiquidOverlay' method.
		/// </summary>
		public static readonly StringName TryApplyLiquidOverlay = "TryApplyLiquidOverlay";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Node2D.PropertyName
	{
		/// <summary>
		/// Cached name for the 'Bounds' property.
		/// </summary>
		public static readonly StringName Bounds = "Bounds";

		/// <summary>
		/// Cached name for the 'IntentPosition' property.
		/// </summary>
		public static readonly StringName IntentPosition = "IntentPosition";

		/// <summary>
		/// Cached name for the 'OrbPosition' property.
		/// </summary>
		public static readonly StringName OrbPosition = "OrbPosition";

		/// <summary>
		/// Cached name for the 'TalkPosition' property.
		/// </summary>
		public static readonly StringName TalkPosition = "TalkPosition";

		/// <summary>
		/// Cached name for the 'IsSpineNode' property.
		/// </summary>
		public static readonly StringName IsSpineNode = "IsSpineNode";

		/// <summary>
		/// Cached name for the 'HasSpineAnimation' property.
		/// </summary>
		public static readonly StringName HasSpineAnimation = "HasSpineAnimation";

		/// <summary>
		/// Cached name for the 'IsUsingPhobiaModeBody' property.
		/// </summary>
		public static readonly StringName IsUsingPhobiaModeBody = "IsUsingPhobiaModeBody";

		/// <summary>
		/// Cached name for the 'FormVfxHolder' property.
		/// </summary>
		public static readonly StringName FormVfxHolder = "FormVfxHolder";

		/// <summary>
		/// Cached name for the 'VfxSpawnPosition' property.
		/// </summary>
		public static readonly StringName VfxSpawnPosition = "VfxSpawnPosition";

		/// <summary>
		/// Cached name for the 'DefaultScale' property.
		/// </summary>
		public static readonly StringName DefaultScale = "DefaultScale";

		/// <summary>
		/// Cached name for the 'Body' property.
		/// </summary>
		public static readonly StringName Body = "Body";

		/// <summary>
		/// Cached name for the '_body' field.
		/// </summary>
		public static readonly StringName _body = "_body";

		/// <summary>
		/// Cached name for the '_phobiaModeBody' field.
		/// </summary>
		public static readonly StringName _phobiaModeBody = "_phobiaModeBody";

		/// <summary>
		/// Cached name for the '_hue' field.
		/// </summary>
		public static readonly StringName _hue = "_hue";

		/// <summary>
		/// Cached name for the '_liquidOverlayTimer' field.
		/// </summary>
		public static readonly StringName _liquidOverlayTimer = "_liquidOverlayTimer";

		/// <summary>
		/// Cached name for the '_savedNormalMaterial' field.
		/// </summary>
		public static readonly StringName _savedNormalMaterial = "_savedNormalMaterial";

		/// <summary>
		/// Cached name for the '_currentLiquidOverlayMaterial' field.
		/// </summary>
		public static readonly StringName _currentLiquidOverlayMaterial = "_currentLiquidOverlayMaterial";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Node2D.SignalName
	{
	}

	private static readonly StringName _overlayInfluence = new StringName("overlay_influence");

	private static readonly StringName _h = new StringName("h");

	private static readonly StringName _tint = new StringName("tint");

	private const double _baseLiquidOverlayDuration = 1.0;

	private Node2D _body;

	private Node2D? _phobiaModeBody;

	private float _hue = 1f;

	private double _liquidOverlayTimer;

	private Material? _savedNormalMaterial;

	private ShaderMaterial? _currentLiquidOverlayMaterial;

	private CancellationTokenSource _cts = new CancellationTokenSource();

	public Control Bounds { get; private set; }

	public Marker2D IntentPosition { get; private set; }

	public Marker2D OrbPosition { get; private set; }

	public Marker2D? TalkPosition { get; private set; }

	private bool IsSpineNode
	{
		get
		{
			if (GodotObject.IsInstanceValid(_body))
			{
				return _body.GetClass() == "SpineSprite";
			}
			return false;
		}
	}

	public bool HasSpineAnimation => SpineBody != null;

	public bool IsUsingPhobiaModeBody => _phobiaModeBody == GetCurrentBody();

	/// <summary>
	/// Control to hold Form Card Vfx for characters (ie Demon Form, Void Form, Serpent Form)
	/// We do this so we can easily track what form vfx we have and destroy old ones as we apply new ones
	/// </summary>
	public Control? FormVfxHolder { get; private set; }

	public MegaSprite? SpineBody { get; private set; }

	public SpineAnimationAccess SpineAnimation => new SpineAnimationAccess(SpineBody);

	/// <summary>
	/// Position we spawn things like hit vfx. Sometimes the center of the creatures isn't always the center of the visual.
	/// </summary>
	public Marker2D VfxSpawnPosition { get; private set; }

	public float DefaultScale { get; set; } = 1f;


	public Node2D Body => _body;

	public Node2D GetCurrentBody()
	{
		Node2D phobiaModeBody = _phobiaModeBody;
		if (phobiaModeBody == null || !phobiaModeBody.Visible)
		{
			return _body;
		}
		return _phobiaModeBody;
	}

	public override void _Ready()
	{
		_body = GetNode<Node2D>("%Visuals");
		_phobiaModeBody = GetNodeOrNull<Node2D>("%PhobiaModeVisuals");
		FormVfxHolder = GetNodeOrNull<Control>("%FormVfx");
		Bounds = GetNode<Control>("%Bounds");
		IntentPosition = GetNode<Marker2D>("%IntentPos");
		VfxSpawnPosition = GetNode<Marker2D>("%CenterPos");
		OrbPosition = (HasNode("%OrbPos") ? GetNode<Marker2D>("%OrbPos") : IntentPosition);
		TalkPosition = (HasNode("%TalkPos") ? GetNode<Marker2D>("%TalkPos") : null);
		if (IsSpineNode)
		{
			SpineBody = new MegaSprite(_body);
			if (SpineBody.GetSkeleton()?.GetData() == null)
			{
				GD.PushWarning($"Spine skeleton data failed to load for {base.Name}, disabling spine animation.");
				SpineBody = null;
			}
		}
		_savedNormalMaterial = null;
		_currentLiquidOverlayMaterial = null;
	}

	public void AddFormVfx(NFormVfx formVfx)
	{
		if (FormVfxHolder == null)
		{
			throw new InvalidOperationException("This creature has no form holder to put this form vfx");
		}
		FormVfxHolder.FreeChildren();
		FormVfxHolder.AddChildSafely(formVfx);
		formVfx.Position = Vector2.Zero;
	}

	public void RemoveFormVfx()
	{
		if (FormVfxHolder == null)
		{
			throw new InvalidOperationException("This creature has no form holder to put this form vfx");
		}
		FormVfxHolder.FreeChildren();
	}

	public override void _EnterTree()
	{
		_cts = new CancellationTokenSource();
	}

	public override void _ExitTree()
	{
		_cts.Cancel();
	}

	public void UpdatePhobiaMode(MonsterModel? model)
	{
		if (_phobiaModeBody != null)
		{
			_phobiaModeBody.Visible = SaveManager.Instance.PrefsSave.PhobiaMode;
			_body.Visible = !_phobiaModeBody.Visible;
		}
		if (SpineBody != null)
		{
			MegaSkeleton skeleton = SpineBody.GetSkeleton();
			if (skeleton != null)
			{
				model?.OnPhobiaModeToggled(SaveManager.Instance.PrefsSave.PhobiaMode, SpineBody, skeleton);
			}
		}
	}

	public void SetUpSkin(MonsterModel model)
	{
		if (SpineBody != null)
		{
			MegaSkeleton skeleton = SpineBody.GetSkeleton();
			if (skeleton != null)
			{
				model.SetupSkins(SpineBody, skeleton);
			}
		}
	}

	public void SetScaleAndHue(float scale, float hue)
	{
		DefaultScale = scale;
		base.Scale = Vector2.One * scale;
		_hue = hue;
		if (!Mathf.IsEqualApprox(hue, 0f) && SpineBody != null)
		{
			Material normalMaterial = SpineBody.GetNormalMaterial();
			ShaderMaterial shaderMaterial;
			if (normalMaterial == null)
			{
				Material material = (ShaderMaterial)PreloadManager.Cache.GetMaterial("res://materials/vfx/hsv.tres");
				shaderMaterial = (ShaderMaterial)material.Duplicate();
				SpineBody.SetNormalMaterial(shaderMaterial);
			}
			else
			{
				shaderMaterial = (ShaderMaterial)normalMaterial;
			}
			shaderMaterial.SetShaderParameter(_h, hue);
		}
	}

	public bool IsPlayingHurtAnimation()
	{
		return SpineAnimation.GetCurrentAnimationName() == "hurt";
	}

	public bool IsPlayingIdleAnimation()
	{
		if (!(SpineAnimation.GetCurrentAnimationName() == "idle_loop"))
		{
			return SpineAnimation.GetCurrentAnimationName() == "low_health_loop";
		}
		return true;
	}

	public void TryApplyLiquidOverlay(Color tint)
	{
		if (_currentLiquidOverlayMaterial != null)
		{
			_currentLiquidOverlayMaterial.SetShaderParameter(_tint, tint);
			_liquidOverlayTimer = 1.0;
		}
		else
		{
			TaskHelper.RunSafely(ApplyLiquidOverlayInternal(tint));
		}
	}

	private async Task ApplyLiquidOverlayInternal(Color tint)
	{
		if (SpineBody != null)
		{
			_savedNormalMaterial = SpineBody.GetNormalMaterial();
			Material material = (ShaderMaterial)PreloadManager.Cache.GetMaterial("res://materials/vfx/potion/potion_liquid_overlay.tres");
			_currentLiquidOverlayMaterial = (ShaderMaterial)material.Duplicate();
			_currentLiquidOverlayMaterial.SetShaderParameter(_tint, tint);
			_currentLiquidOverlayMaterial.SetShaderParameter(_h, _hue);
			_currentLiquidOverlayMaterial.SetShaderParameter(_overlayInfluence, 1f);
			SpineBody.SetNormalMaterial(_currentLiquidOverlayMaterial);
			_liquidOverlayTimer = 1.0;
			while (_liquidOverlayTimer > 0.0)
			{
				double num = (1.0 - _liquidOverlayTimer) / 1.0;
				_currentLiquidOverlayMaterial.SetShaderParameter(_overlayInfluence, 1.0 - num);
				_liquidOverlayTimer -= GetProcessDeltaTime();
				await this.AwaitProcessFrame(_cts.Token);
			}
			SpineBody.SetNormalMaterial(_savedNormalMaterial);
			_currentLiquidOverlayMaterial = null;
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
		List<MethodInfo> list = new List<MethodInfo>(10);
		list.Add(new MethodInfo(MethodName.GetCurrentBody, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AddFormVfx, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "formVfx", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.RemoveFormVfx, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._EnterTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetScaleAndHue, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "scale", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Float, "hue", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.IsPlayingHurtAnimation, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.IsPlayingIdleAnimation, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.TryApplyLiquidOverlay, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Color, "tint", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.GetCurrentBody && args.Count == 0)
		{
			Node2D from = GetCurrentBody();
			ret = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AddFormVfx && args.Count == 1)
		{
			AddFormVfx(VariantUtils.ConvertTo<NFormVfx>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RemoveFormVfx && args.Count == 0)
		{
			RemoveFormVfx();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._EnterTree && args.Count == 0)
		{
			_EnterTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetScaleAndHue && args.Count == 2)
		{
			SetScaleAndHue(VariantUtils.ConvertTo<float>(in args[0]), VariantUtils.ConvertTo<float>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.IsPlayingHurtAnimation && args.Count == 0)
		{
			bool from2 = IsPlayingHurtAnimation();
			ret = VariantUtils.CreateFrom(in from2);
			return true;
		}
		if (method == MethodName.IsPlayingIdleAnimation && args.Count == 0)
		{
			bool from3 = IsPlayingIdleAnimation();
			ret = VariantUtils.CreateFrom(in from3);
			return true;
		}
		if (method == MethodName.TryApplyLiquidOverlay && args.Count == 1)
		{
			TryApplyLiquidOverlay(VariantUtils.ConvertTo<Color>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.GetCurrentBody)
		{
			return true;
		}
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.AddFormVfx)
		{
			return true;
		}
		if (method == MethodName.RemoveFormVfx)
		{
			return true;
		}
		if (method == MethodName._EnterTree)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.SetScaleAndHue)
		{
			return true;
		}
		if (method == MethodName.IsPlayingHurtAnimation)
		{
			return true;
		}
		if (method == MethodName.IsPlayingIdleAnimation)
		{
			return true;
		}
		if (method == MethodName.TryApplyLiquidOverlay)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.Bounds)
		{
			Bounds = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName.IntentPosition)
		{
			IntentPosition = VariantUtils.ConvertTo<Marker2D>(in value);
			return true;
		}
		if (name == PropertyName.OrbPosition)
		{
			OrbPosition = VariantUtils.ConvertTo<Marker2D>(in value);
			return true;
		}
		if (name == PropertyName.TalkPosition)
		{
			TalkPosition = VariantUtils.ConvertTo<Marker2D>(in value);
			return true;
		}
		if (name == PropertyName.FormVfxHolder)
		{
			FormVfxHolder = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName.VfxSpawnPosition)
		{
			VfxSpawnPosition = VariantUtils.ConvertTo<Marker2D>(in value);
			return true;
		}
		if (name == PropertyName.DefaultScale)
		{
			DefaultScale = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._body)
		{
			_body = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._phobiaModeBody)
		{
			_phobiaModeBody = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._hue)
		{
			_hue = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._liquidOverlayTimer)
		{
			_liquidOverlayTimer = VariantUtils.ConvertTo<double>(in value);
			return true;
		}
		if (name == PropertyName._savedNormalMaterial)
		{
			_savedNormalMaterial = VariantUtils.ConvertTo<Material>(in value);
			return true;
		}
		if (name == PropertyName._currentLiquidOverlayMaterial)
		{
			_currentLiquidOverlayMaterial = VariantUtils.ConvertTo<ShaderMaterial>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		Control from;
		if (name == PropertyName.Bounds)
		{
			from = Bounds;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		Marker2D from2;
		if (name == PropertyName.IntentPosition)
		{
			from2 = IntentPosition;
			value = VariantUtils.CreateFrom(in from2);
			return true;
		}
		if (name == PropertyName.OrbPosition)
		{
			from2 = OrbPosition;
			value = VariantUtils.CreateFrom(in from2);
			return true;
		}
		if (name == PropertyName.TalkPosition)
		{
			from2 = TalkPosition;
			value = VariantUtils.CreateFrom(in from2);
			return true;
		}
		bool from3;
		if (name == PropertyName.IsSpineNode)
		{
			from3 = IsSpineNode;
			value = VariantUtils.CreateFrom(in from3);
			return true;
		}
		if (name == PropertyName.HasSpineAnimation)
		{
			from3 = HasSpineAnimation;
			value = VariantUtils.CreateFrom(in from3);
			return true;
		}
		if (name == PropertyName.IsUsingPhobiaModeBody)
		{
			from3 = IsUsingPhobiaModeBody;
			value = VariantUtils.CreateFrom(in from3);
			return true;
		}
		if (name == PropertyName.FormVfxHolder)
		{
			from = FormVfxHolder;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.VfxSpawnPosition)
		{
			from2 = VfxSpawnPosition;
			value = VariantUtils.CreateFrom(in from2);
			return true;
		}
		if (name == PropertyName.DefaultScale)
		{
			float from4 = DefaultScale;
			value = VariantUtils.CreateFrom(in from4);
			return true;
		}
		if (name == PropertyName.Body)
		{
			Node2D from5 = Body;
			value = VariantUtils.CreateFrom(in from5);
			return true;
		}
		if (name == PropertyName._body)
		{
			value = VariantUtils.CreateFrom(in _body);
			return true;
		}
		if (name == PropertyName._phobiaModeBody)
		{
			value = VariantUtils.CreateFrom(in _phobiaModeBody);
			return true;
		}
		if (name == PropertyName._hue)
		{
			value = VariantUtils.CreateFrom(in _hue);
			return true;
		}
		if (name == PropertyName._liquidOverlayTimer)
		{
			value = VariantUtils.CreateFrom(in _liquidOverlayTimer);
			return true;
		}
		if (name == PropertyName._savedNormalMaterial)
		{
			value = VariantUtils.CreateFrom(in _savedNormalMaterial);
			return true;
		}
		if (name == PropertyName._currentLiquidOverlayMaterial)
		{
			value = VariantUtils.CreateFrom(in _currentLiquidOverlayMaterial);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._body, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._phobiaModeBody, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.Bounds, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.IntentPosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.OrbPosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.TalkPosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.IsSpineNode, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.HasSpineAnimation, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.IsUsingPhobiaModeBody, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.FormVfxHolder, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.VfxSpawnPosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName.DefaultScale, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._hue, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._liquidOverlayTimer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._savedNormalMaterial, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._currentLiquidOverlayMaterial, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.Body, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		StringName bounds = PropertyName.Bounds;
		Control from = Bounds;
		info.AddProperty(bounds, Variant.From(in from));
		StringName intentPosition = PropertyName.IntentPosition;
		Marker2D from2 = IntentPosition;
		info.AddProperty(intentPosition, Variant.From(in from2));
		StringName orbPosition = PropertyName.OrbPosition;
		from2 = OrbPosition;
		info.AddProperty(orbPosition, Variant.From(in from2));
		StringName talkPosition = PropertyName.TalkPosition;
		from2 = TalkPosition;
		info.AddProperty(talkPosition, Variant.From(in from2));
		StringName formVfxHolder = PropertyName.FormVfxHolder;
		from = FormVfxHolder;
		info.AddProperty(formVfxHolder, Variant.From(in from));
		StringName vfxSpawnPosition = PropertyName.VfxSpawnPosition;
		from2 = VfxSpawnPosition;
		info.AddProperty(vfxSpawnPosition, Variant.From(in from2));
		StringName defaultScale = PropertyName.DefaultScale;
		float from3 = DefaultScale;
		info.AddProperty(defaultScale, Variant.From(in from3));
		info.AddProperty(PropertyName._body, Variant.From(in _body));
		info.AddProperty(PropertyName._phobiaModeBody, Variant.From(in _phobiaModeBody));
		info.AddProperty(PropertyName._hue, Variant.From(in _hue));
		info.AddProperty(PropertyName._liquidOverlayTimer, Variant.From(in _liquidOverlayTimer));
		info.AddProperty(PropertyName._savedNormalMaterial, Variant.From(in _savedNormalMaterial));
		info.AddProperty(PropertyName._currentLiquidOverlayMaterial, Variant.From(in _currentLiquidOverlayMaterial));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.Bounds, out var value))
		{
			Bounds = value.As<Control>();
		}
		if (info.TryGetProperty(PropertyName.IntentPosition, out var value2))
		{
			IntentPosition = value2.As<Marker2D>();
		}
		if (info.TryGetProperty(PropertyName.OrbPosition, out var value3))
		{
			OrbPosition = value3.As<Marker2D>();
		}
		if (info.TryGetProperty(PropertyName.TalkPosition, out var value4))
		{
			TalkPosition = value4.As<Marker2D>();
		}
		if (info.TryGetProperty(PropertyName.FormVfxHolder, out var value5))
		{
			FormVfxHolder = value5.As<Control>();
		}
		if (info.TryGetProperty(PropertyName.VfxSpawnPosition, out var value6))
		{
			VfxSpawnPosition = value6.As<Marker2D>();
		}
		if (info.TryGetProperty(PropertyName.DefaultScale, out var value7))
		{
			DefaultScale = value7.As<float>();
		}
		if (info.TryGetProperty(PropertyName._body, out var value8))
		{
			_body = value8.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._phobiaModeBody, out var value9))
		{
			_phobiaModeBody = value9.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._hue, out var value10))
		{
			_hue = value10.As<float>();
		}
		if (info.TryGetProperty(PropertyName._liquidOverlayTimer, out var value11))
		{
			_liquidOverlayTimer = value11.As<double>();
		}
		if (info.TryGetProperty(PropertyName._savedNormalMaterial, out var value12))
		{
			_savedNormalMaterial = value12.As<Material>();
		}
		if (info.TryGetProperty(PropertyName._currentLiquidOverlayMaterial, out var value13))
		{
			_currentLiquidOverlayMaterial = value13.As<ShaderMaterial>();
		}
	}
}

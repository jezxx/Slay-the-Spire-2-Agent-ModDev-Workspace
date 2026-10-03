using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Vfx.Forms;

[ScriptPath("res://src/Core/Nodes/Vfx/Forms/NVoidFormVfx.cs")]
public class NVoidFormVfx : NFormVfx
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : NFormVfx.MethodName
	{
		/// <summary>
		/// Cached name for the '_Process' method.
		/// </summary>
		public new static readonly StringName _Process = "_Process";

		/// <summary>
		/// Cached name for the 'UpdateVfx' method.
		/// </summary>
		public static readonly StringName UpdateVfx = "UpdateVfx";

		/// <summary>
		/// Cached name for the 'SetActive' method.
		/// </summary>
		public new static readonly StringName SetActive = "SetActive";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : NFormVfx.PropertyName
	{
		/// <summary>
		/// Cached name for the '_ironcladBoneName' field.
		/// </summary>
		public static readonly StringName _ironcladBoneName = "_ironcladBoneName";

		/// <summary>
		/// Cached name for the '_silentBoneName' field.
		/// </summary>
		public static readonly StringName _silentBoneName = "_silentBoneName";

		/// <summary>
		/// Cached name for the '_regentBoneName' field.
		/// </summary>
		public static readonly StringName _regentBoneName = "_regentBoneName";

		/// <summary>
		/// Cached name for the '_necrobinderBoneName' field.
		/// </summary>
		public static readonly StringName _necrobinderBoneName = "_necrobinderBoneName";

		/// <summary>
		/// Cached name for the '_defectBoneName' field.
		/// </summary>
		public static readonly StringName _defectBoneName = "_defectBoneName";

		/// <summary>
		/// Cached name for the '_swords' field.
		/// </summary>
		public static readonly StringName _swords = "_swords";

		/// <summary>
		/// Cached name for the '_swordsScaleRange' field.
		/// </summary>
		public static readonly StringName _swordsScaleRange = "_swordsScaleRange";

		/// <summary>
		/// Cached name for the '_boneFollower' field.
		/// </summary>
		public static readonly StringName _boneFollower = "_boneFollower";

		/// <summary>
		/// Cached name for the '_valueRamp' field.
		/// </summary>
		public static readonly StringName _valueRamp = "_valueRamp";

		/// <summary>
		/// Cached name for the '_powerActiveParticles' field.
		/// </summary>
		public static readonly StringName _powerActiveParticles = "_powerActiveParticles";

		/// <summary>
		/// Cached name for the '_glowSelfModulateGradient' field.
		/// </summary>
		public static readonly StringName _glowSelfModulateGradient = "_glowSelfModulateGradient";

		/// <summary>
		/// Cached name for the '_glow' field.
		/// </summary>
		public static readonly StringName _glow = "_glow";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : NFormVfx.SignalName
	{
	}

	private static readonly string _scenePath = SceneHelper.GetScenePath("vfx/forms/void/vfx_void_form_idle_vfx");

	[Export(PropertyHint.None, "")]
	private string _ironcladBoneName = "";

	[Export(PropertyHint.None, "")]
	private string _silentBoneName = "";

	[Export(PropertyHint.None, "")]
	private string _regentBoneName = "";

	[Export(PropertyHint.None, "")]
	private string _necrobinderBoneName = "";

	[Export(PropertyHint.None, "")]
	private string _defectBoneName = "";

	[Export(PropertyHint.None, "")]
	private Node2D[] _swords;

	[Export(PropertyHint.None, "")]
	private Vector2 _swordsScaleRange = new Vector2(0.7f, 1f);

	[Export(PropertyHint.None, "")]
	private NSpineSpriteBoneFollower? _boneFollower;

	[Export(PropertyHint.None, "")]
	private NValueRamp _valueRamp;

	[Export(PropertyHint.None, "")]
	private NParticlesContainer? _powerActiveParticles;

	[Export(PropertyHint.None, "")]
	private Gradient _glowSelfModulateGradient;

	[Export(PropertyHint.None, "")]
	private Node2D _glow;

	public static NVoidFormVfx? Create(Creature target)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NCreature creatureNode = NCombatRoom.Instance.GetCreatureNode(target);
		if (creatureNode == null)
		{
			return null;
		}
		NVoidFormVfx nVoidFormVfx = PreloadManager.Cache.GetScene(_scenePath).Instantiate<NVoidFormVfx>(PackedScene.GenEditState.Disabled);
		nVoidFormVfx.Initialize(target.Player);
		creatureNode.Visuals.AddFormVfx(nVoidFormVfx);
		return nVoidFormVfx;
	}

	public override void Initialize(Player owner)
	{
		base.Initialize(owner);
		_valueRamp.SetIncreasing(isIncreasing: true);
		_valueRamp.ForceValue(1f);
		UpdateVfx(1f);
	}

	public override void _Process(double delta)
	{
		if (_valueRamp.TryProcess(delta, out var returnValue))
		{
			UpdateVfx(returnValue);
		}
	}

	private void UpdateVfx(float progress)
	{
		_glow.SelfModulate = _glowSelfModulateGradient.Sample(progress);
		for (int i = 0; i < _swords.Length; i++)
		{
			_swords[i].Scale = Vector2.One * Mathf.Lerp(_swordsScaleRange.X, _swordsScaleRange.Y, progress);
		}
	}

	public override void SetActive(bool isActive)
	{
		base.SetActive(isActive);
		_powerActiveParticles?.SetEmitting(isActive);
		_valueRamp.SetIncreasing(isActive);
	}

	protected override void SetSpineSprite(MegaSprite spineSprite, Node2D sourceNode)
	{
		base.SetSpineSprite(spineSprite, sourceNode);
		if (_boneFollower == null)
		{
			return;
		}
		string boneName = "";
		if (_owner != null)
		{
			CharacterModel character = _owner.Character;
			if (character is Ironclad)
			{
				boneName = _ironcladBoneName;
			}
			else if (character is Silent)
			{
				boneName = _silentBoneName;
			}
			else if (character is Regent)
			{
				boneName = _regentBoneName;
			}
			else if (character is Necrobinder)
			{
				boneName = _necrobinderBoneName;
			}
			else if (character is Defect)
			{
				boneName = _defectBoneName;
			}
		}
		else
		{
			boneName = _testBoneName;
		}
		_boneFollower.SetSpineSprite(spineSprite, boneName);
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
		list.Add(new MethodInfo(MethodName._Process, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.UpdateVfx, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "progress", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SetActive, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "isActive", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName._Process && args.Count == 1)
		{
			_Process(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateVfx && args.Count == 1)
		{
			UpdateVfx(VariantUtils.ConvertTo<float>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetActive && args.Count == 1)
		{
			SetActive(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Process)
		{
			return true;
		}
		if (method == MethodName.UpdateVfx)
		{
			return true;
		}
		if (method == MethodName.SetActive)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._ironcladBoneName)
		{
			_ironcladBoneName = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		if (name == PropertyName._silentBoneName)
		{
			_silentBoneName = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		if (name == PropertyName._regentBoneName)
		{
			_regentBoneName = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		if (name == PropertyName._necrobinderBoneName)
		{
			_necrobinderBoneName = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		if (name == PropertyName._defectBoneName)
		{
			_defectBoneName = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		if (name == PropertyName._swords)
		{
			_swords = VariantUtils.ConvertToSystemArrayOfGodotObject<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._swordsScaleRange)
		{
			_swordsScaleRange = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._boneFollower)
		{
			_boneFollower = VariantUtils.ConvertTo<NSpineSpriteBoneFollower>(in value);
			return true;
		}
		if (name == PropertyName._valueRamp)
		{
			_valueRamp = VariantUtils.ConvertTo<NValueRamp>(in value);
			return true;
		}
		if (name == PropertyName._powerActiveParticles)
		{
			_powerActiveParticles = VariantUtils.ConvertTo<NParticlesContainer>(in value);
			return true;
		}
		if (name == PropertyName._glowSelfModulateGradient)
		{
			_glowSelfModulateGradient = VariantUtils.ConvertTo<Gradient>(in value);
			return true;
		}
		if (name == PropertyName._glow)
		{
			_glow = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._ironcladBoneName)
		{
			value = VariantUtils.CreateFrom(in _ironcladBoneName);
			return true;
		}
		if (name == PropertyName._silentBoneName)
		{
			value = VariantUtils.CreateFrom(in _silentBoneName);
			return true;
		}
		if (name == PropertyName._regentBoneName)
		{
			value = VariantUtils.CreateFrom(in _regentBoneName);
			return true;
		}
		if (name == PropertyName._necrobinderBoneName)
		{
			value = VariantUtils.CreateFrom(in _necrobinderBoneName);
			return true;
		}
		if (name == PropertyName._defectBoneName)
		{
			value = VariantUtils.CreateFrom(in _defectBoneName);
			return true;
		}
		if (name == PropertyName._swords)
		{
			GodotObject[] swords = _swords;
			value = VariantUtils.CreateFromSystemArrayOfGodotObject(swords);
			return true;
		}
		if (name == PropertyName._swordsScaleRange)
		{
			value = VariantUtils.CreateFrom(in _swordsScaleRange);
			return true;
		}
		if (name == PropertyName._boneFollower)
		{
			value = VariantUtils.CreateFrom(in _boneFollower);
			return true;
		}
		if (name == PropertyName._valueRamp)
		{
			value = VariantUtils.CreateFrom(in _valueRamp);
			return true;
		}
		if (name == PropertyName._powerActiveParticles)
		{
			value = VariantUtils.CreateFrom(in _powerActiveParticles);
			return true;
		}
		if (name == PropertyName._glowSelfModulateGradient)
		{
			value = VariantUtils.CreateFrom(in _glowSelfModulateGradient);
			return true;
		}
		if (name == PropertyName._glow)
		{
			value = VariantUtils.CreateFrom(in _glow);
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
		list.Add(new PropertyInfo(Variant.Type.String, PropertyName._ironcladBoneName, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.String, PropertyName._silentBoneName, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.String, PropertyName._regentBoneName, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.String, PropertyName._necrobinderBoneName, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.String, PropertyName._defectBoneName, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Array, PropertyName._swords, PropertyHint.TypeString, "24/34:Node2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._swordsScaleRange, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._boneFollower, PropertyHint.NodeType, "Node2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._valueRamp, PropertyHint.NodeType, "Node", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._powerActiveParticles, PropertyHint.NodeType, "Node2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._glowSelfModulateGradient, PropertyHint.ResourceType, "Gradient", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._glow, PropertyHint.NodeType, "Node2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._ironcladBoneName, Variant.From(in _ironcladBoneName));
		info.AddProperty(PropertyName._silentBoneName, Variant.From(in _silentBoneName));
		info.AddProperty(PropertyName._regentBoneName, Variant.From(in _regentBoneName));
		info.AddProperty(PropertyName._necrobinderBoneName, Variant.From(in _necrobinderBoneName));
		info.AddProperty(PropertyName._defectBoneName, Variant.From(in _defectBoneName));
		StringName swords = PropertyName._swords;
		GodotObject[] swords2 = _swords;
		info.AddProperty(swords, Variant.CreateFrom(swords2));
		info.AddProperty(PropertyName._swordsScaleRange, Variant.From(in _swordsScaleRange));
		info.AddProperty(PropertyName._boneFollower, Variant.From(in _boneFollower));
		info.AddProperty(PropertyName._valueRamp, Variant.From(in _valueRamp));
		info.AddProperty(PropertyName._powerActiveParticles, Variant.From(in _powerActiveParticles));
		info.AddProperty(PropertyName._glowSelfModulateGradient, Variant.From(in _glowSelfModulateGradient));
		info.AddProperty(PropertyName._glow, Variant.From(in _glow));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._ironcladBoneName, out var value))
		{
			_ironcladBoneName = value.As<string>();
		}
		if (info.TryGetProperty(PropertyName._silentBoneName, out var value2))
		{
			_silentBoneName = value2.As<string>();
		}
		if (info.TryGetProperty(PropertyName._regentBoneName, out var value3))
		{
			_regentBoneName = value3.As<string>();
		}
		if (info.TryGetProperty(PropertyName._necrobinderBoneName, out var value4))
		{
			_necrobinderBoneName = value4.As<string>();
		}
		if (info.TryGetProperty(PropertyName._defectBoneName, out var value5))
		{
			_defectBoneName = value5.As<string>();
		}
		if (info.TryGetProperty(PropertyName._swords, out var value6))
		{
			_swords = value6.AsGodotObjectArray<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._swordsScaleRange, out var value7))
		{
			_swordsScaleRange = value7.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._boneFollower, out var value8))
		{
			_boneFollower = value8.As<NSpineSpriteBoneFollower>();
		}
		if (info.TryGetProperty(PropertyName._valueRamp, out var value9))
		{
			_valueRamp = value9.As<NValueRamp>();
		}
		if (info.TryGetProperty(PropertyName._powerActiveParticles, out var value10))
		{
			_powerActiveParticles = value10.As<NParticlesContainer>();
		}
		if (info.TryGetProperty(PropertyName._glowSelfModulateGradient, out var value11))
		{
			_glowSelfModulateGradient = value11.As<Gradient>();
		}
		if (info.TryGetProperty(PropertyName._glow, out var value12))
		{
			_glow = value12.As<Node2D>();
		}
	}
}

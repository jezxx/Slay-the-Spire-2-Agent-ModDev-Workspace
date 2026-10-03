using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Vfx.Forms;

[ScriptPath("res://src/Core/Nodes/Vfx/Forms/NEchoFormVfx.cs")]
public class NEchoFormVfx : NFormVfx
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
		/// Cached name for the 'UpdateModulates' method.
		/// </summary>
		public static readonly StringName UpdateModulates = "UpdateModulates";

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
		/// Cached name for the '_boneFollower' field.
		/// </summary>
		public static readonly StringName _boneFollower = "_boneFollower";

		/// <summary>
		/// Cached name for the '_valueRamp' field.
		/// </summary>
		public static readonly StringName _valueRamp = "_valueRamp";

		/// <summary>
		/// Cached name for the '_glow' field.
		/// </summary>
		public static readonly StringName _glow = "_glow";

		/// <summary>
		/// Cached name for the '_echoLines' field.
		/// </summary>
		public static readonly StringName _echoLines = "_echoLines";

		/// <summary>
		/// Cached name for the '_glowSelfModulateGradient' field.
		/// </summary>
		public static readonly StringName _glowSelfModulateGradient = "_glowSelfModulateGradient";

		/// <summary>
		/// Cached name for the '_echoFormLinesSelfModulateGradient' field.
		/// </summary>
		public static readonly StringName _echoFormLinesSelfModulateGradient = "_echoFormLinesSelfModulateGradient";

		/// <summary>
		/// Cached name for the '_speckParticles' field.
		/// </summary>
		public static readonly StringName _speckParticles = "_speckParticles";

		/// <summary>
		/// Cached name for the '_spineCopier' field.
		/// </summary>
		public static readonly StringName _spineCopier = "_spineCopier";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : NFormVfx.SignalName
	{
	}

	private static readonly string _scenePath = SceneHelper.GetScenePath("vfx/forms/echo/vfx_echo_form_idle_vfx");

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
	private NSpineSpriteBoneFollower? _boneFollower;

	[Export(PropertyHint.None, "")]
	private NValueRamp _valueRamp;

	[Export(PropertyHint.None, "")]
	private Node2D _glow;

	[Export(PropertyHint.None, "")]
	private Node2D _echoLines;

	[Export(PropertyHint.None, "")]
	private Gradient _glowSelfModulateGradient;

	[Export(PropertyHint.None, "")]
	private Gradient _echoFormLinesSelfModulateGradient;

	[Export(PropertyHint.None, "")]
	private GpuParticles2D _speckParticles;

	[Export(PropertyHint.None, "")]
	private NSpineSpriteCopier? _spineCopier;

	public static NEchoFormVfx? Create(Creature target)
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
		NEchoFormVfx nEchoFormVfx = PreloadManager.Cache.GetScene(_scenePath).Instantiate<NEchoFormVfx>(PackedScene.GenEditState.Disabled);
		nEchoFormVfx.Initialize(target.Player);
		nEchoFormVfx.SetActive(isActive: false);
		creatureNode.Visuals.AddFormVfx(nEchoFormVfx);
		return nEchoFormVfx;
	}

	public override void _Process(double delta)
	{
		if (_valueRamp.TryProcess(delta, out var returnValue))
		{
			UpdateModulates(returnValue);
		}
	}

	private void UpdateModulates(float progress)
	{
		_glow.SelfModulate = _glowSelfModulateGradient.Sample(progress);
		_echoLines.SelfModulate = _echoFormLinesSelfModulateGradient.Sample(progress);
	}

	public override void SetActive(bool isActive)
	{
		base.SetActive(isActive);
		_speckParticles.Emitting = isActive;
		_valueRamp.SetIncreasing(isActive);
	}

	protected override void SetSpineSprite(MegaSprite spineSprite, Node2D sourceNode)
	{
		base.SetSpineSprite(spineSprite, sourceNode);
		if (_spineCopier != null)
		{
			_spineCopier.Initialize(spineSprite, sourceNode);
		}
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
		list.Add(new MethodInfo(MethodName.UpdateModulates, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
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
		if (method == MethodName.UpdateModulates && args.Count == 1)
		{
			UpdateModulates(VariantUtils.ConvertTo<float>(in args[0]));
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
		if (method == MethodName.UpdateModulates)
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
		if (name == PropertyName._glow)
		{
			_glow = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._echoLines)
		{
			_echoLines = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._glowSelfModulateGradient)
		{
			_glowSelfModulateGradient = VariantUtils.ConvertTo<Gradient>(in value);
			return true;
		}
		if (name == PropertyName._echoFormLinesSelfModulateGradient)
		{
			_echoFormLinesSelfModulateGradient = VariantUtils.ConvertTo<Gradient>(in value);
			return true;
		}
		if (name == PropertyName._speckParticles)
		{
			_speckParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._spineCopier)
		{
			_spineCopier = VariantUtils.ConvertTo<NSpineSpriteCopier>(in value);
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
		if (name == PropertyName._glow)
		{
			value = VariantUtils.CreateFrom(in _glow);
			return true;
		}
		if (name == PropertyName._echoLines)
		{
			value = VariantUtils.CreateFrom(in _echoLines);
			return true;
		}
		if (name == PropertyName._glowSelfModulateGradient)
		{
			value = VariantUtils.CreateFrom(in _glowSelfModulateGradient);
			return true;
		}
		if (name == PropertyName._echoFormLinesSelfModulateGradient)
		{
			value = VariantUtils.CreateFrom(in _echoFormLinesSelfModulateGradient);
			return true;
		}
		if (name == PropertyName._speckParticles)
		{
			value = VariantUtils.CreateFrom(in _speckParticles);
			return true;
		}
		if (name == PropertyName._spineCopier)
		{
			value = VariantUtils.CreateFrom(in _spineCopier);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._boneFollower, PropertyHint.NodeType, "Node2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._valueRamp, PropertyHint.NodeType, "Node", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._glow, PropertyHint.NodeType, "Node2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._echoLines, PropertyHint.NodeType, "Node2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._glowSelfModulateGradient, PropertyHint.ResourceType, "Gradient", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._echoFormLinesSelfModulateGradient, PropertyHint.ResourceType, "Gradient", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._speckParticles, PropertyHint.NodeType, "GPUParticles2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._spineCopier, PropertyHint.NodeType, "Node2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
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
		info.AddProperty(PropertyName._boneFollower, Variant.From(in _boneFollower));
		info.AddProperty(PropertyName._valueRamp, Variant.From(in _valueRamp));
		info.AddProperty(PropertyName._glow, Variant.From(in _glow));
		info.AddProperty(PropertyName._echoLines, Variant.From(in _echoLines));
		info.AddProperty(PropertyName._glowSelfModulateGradient, Variant.From(in _glowSelfModulateGradient));
		info.AddProperty(PropertyName._echoFormLinesSelfModulateGradient, Variant.From(in _echoFormLinesSelfModulateGradient));
		info.AddProperty(PropertyName._speckParticles, Variant.From(in _speckParticles));
		info.AddProperty(PropertyName._spineCopier, Variant.From(in _spineCopier));
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
		if (info.TryGetProperty(PropertyName._boneFollower, out var value6))
		{
			_boneFollower = value6.As<NSpineSpriteBoneFollower>();
		}
		if (info.TryGetProperty(PropertyName._valueRamp, out var value7))
		{
			_valueRamp = value7.As<NValueRamp>();
		}
		if (info.TryGetProperty(PropertyName._glow, out var value8))
		{
			_glow = value8.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._echoLines, out var value9))
		{
			_echoLines = value9.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._glowSelfModulateGradient, out var value10))
		{
			_glowSelfModulateGradient = value10.As<Gradient>();
		}
		if (info.TryGetProperty(PropertyName._echoFormLinesSelfModulateGradient, out var value11))
		{
			_echoFormLinesSelfModulateGradient = value11.As<Gradient>();
		}
		if (info.TryGetProperty(PropertyName._speckParticles, out var value12))
		{
			_speckParticles = value12.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._spineCopier, out var value13))
		{
			_spineCopier = value13.As<NSpineSpriteCopier>();
		}
	}
}

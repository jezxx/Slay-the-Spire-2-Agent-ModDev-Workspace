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

[ScriptPath("res://src/Core/Nodes/Vfx/Forms/NReaperFormVfx.cs")]
public class NReaperFormVfx : NFormVfx
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : NFormVfx.MethodName
	{
		/// <summary>
		/// Cached name for the 'OnEffectTriggered' method.
		/// </summary>
		public new static readonly StringName OnEffectTriggered = "OnEffectTriggered";
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
		/// Cached name for the '_effectTriggeredParticles' field.
		/// </summary>
		public static readonly StringName _effectTriggeredParticles = "_effectTriggeredParticles";

		/// <summary>
		/// Cached name for the '_boneFollower' field.
		/// </summary>
		public static readonly StringName _boneFollower = "_boneFollower";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : NFormVfx.SignalName
	{
	}

	private static readonly string _scenePath = SceneHelper.GetScenePath("vfx/forms/reaper/vfx_reaper_form_idle_vfx");

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
	private NParticlesContainer? _effectTriggeredParticles;

	[Export(PropertyHint.None, "")]
	private NSpineSpriteBoneFollower? _boneFollower;

	public static NReaperFormVfx? Create(Creature target)
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
		NReaperFormVfx nReaperFormVfx = PreloadManager.Cache.GetScene(_scenePath).Instantiate<NReaperFormVfx>(PackedScene.GenEditState.Disabled);
		nReaperFormVfx.Initialize(target.Player);
		creatureNode.Visuals.AddFormVfx(nReaperFormVfx);
		return nReaperFormVfx;
	}

	public override void OnEffectTriggered()
	{
		base.OnEffectTriggered();
		_effectTriggeredParticles?.Restart();
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
		List<MethodInfo> list = new List<MethodInfo>(1);
		list.Add(new MethodInfo(MethodName.OnEffectTriggered, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.OnEffectTriggered && args.Count == 0)
		{
			OnEffectTriggered();
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.OnEffectTriggered)
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
		if (name == PropertyName._effectTriggeredParticles)
		{
			_effectTriggeredParticles = VariantUtils.ConvertTo<NParticlesContainer>(in value);
			return true;
		}
		if (name == PropertyName._boneFollower)
		{
			_boneFollower = VariantUtils.ConvertTo<NSpineSpriteBoneFollower>(in value);
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
		if (name == PropertyName._effectTriggeredParticles)
		{
			value = VariantUtils.CreateFrom(in _effectTriggeredParticles);
			return true;
		}
		if (name == PropertyName._boneFollower)
		{
			value = VariantUtils.CreateFrom(in _boneFollower);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._effectTriggeredParticles, PropertyHint.NodeType, "Node2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._boneFollower, PropertyHint.NodeType, "Node2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
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
		info.AddProperty(PropertyName._effectTriggeredParticles, Variant.From(in _effectTriggeredParticles));
		info.AddProperty(PropertyName._boneFollower, Variant.From(in _boneFollower));
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
		if (info.TryGetProperty(PropertyName._effectTriggeredParticles, out var value6))
		{
			_effectTriggeredParticles = value6.As<NParticlesContainer>();
		}
		if (info.TryGetProperty(PropertyName._boneFollower, out var value7))
		{
			_boneFollower = value7.As<NSpineSpriteBoneFollower>();
		}
	}
}

using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace MegaCrit.Sts2.Core.Nodes.Vfx.Forms;

[ScriptPath("res://src/Core/Nodes/Vfx/Forms/NFormVfx.cs")]
public class NFormVfx : Node2D
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : Node2D.MethodName
	{
		/// <summary>
		/// Cached name for the 'OnEffectTriggered' method.
		/// </summary>
		public static readonly StringName OnEffectTriggered = "OnEffectTriggered";

		/// <summary>
		/// Cached name for the 'SetActive' method.
		/// </summary>
		public static readonly StringName SetActive = "SetActive";

		/// <summary>
		/// Cached name for the 'ForceSetSpineSprite' method.
		/// </summary>
		public static readonly StringName ForceSetSpineSprite = "ForceSetSpineSprite";

		/// <summary>
		/// Cached name for the 'ForceTestBoneName' method.
		/// </summary>
		public static readonly StringName ForceTestBoneName = "ForceTestBoneName";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Node2D.PropertyName
	{
		/// <summary>
		/// Cached name for the '_isActive' field.
		/// </summary>
		public static readonly StringName _isActive = "_isActive";

		/// <summary>
		/// Cached name for the '_testBoneName' field.
		/// </summary>
		public static readonly StringName _testBoneName = "_testBoneName";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Node2D.SignalName
	{
	}

	protected Player? _owner;

	protected bool _isActive;

	protected string _testBoneName = "";

	public virtual void Initialize(Player owner)
	{
		_owner = owner;
		if (_owner != null)
		{
			NCreature nCreature = NCombatRoom.Instance?.GetCreatureNode(_owner.Creature);
			if (nCreature != null && nCreature.Visuals.HasSpineAnimation)
			{
				SetSpineSprite(nCreature.Visuals.SpineBody, nCreature.Visuals.GetCurrentBody());
			}
		}
	}

	public virtual void OnEffectTriggered()
	{
	}

	public virtual void SetActive(bool isActive)
	{
		_isActive = isActive;
	}

	protected virtual void SetSpineSprite(MegaSprite spineSprite, Node2D sourceNode)
	{
	}

	public void ForceSetSpineSprite(Node2D sourceNode)
	{
		SetSpineSprite(new MegaSprite(sourceNode), sourceNode);
	}

	public void ForceTestBoneName(string testBoneName)
	{
		_testBoneName = testBoneName;
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
		list.Add(new MethodInfo(MethodName.OnEffectTriggered, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetActive, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "isActive", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ForceSetSpineSprite, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "sourceNode", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ForceTestBoneName, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "testBoneName", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
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
		if (method == MethodName.SetActive && args.Count == 1)
		{
			SetActive(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ForceSetSpineSprite && args.Count == 1)
		{
			ForceSetSpineSprite(VariantUtils.ConvertTo<Node2D>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ForceTestBoneName && args.Count == 1)
		{
			ForceTestBoneName(VariantUtils.ConvertTo<string>(in args[0]));
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
		if (method == MethodName.SetActive)
		{
			return true;
		}
		if (method == MethodName.ForceSetSpineSprite)
		{
			return true;
		}
		if (method == MethodName.ForceTestBoneName)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._isActive)
		{
			_isActive = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._testBoneName)
		{
			_testBoneName = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._isActive)
		{
			value = VariantUtils.CreateFrom(in _isActive);
			return true;
		}
		if (name == PropertyName._testBoneName)
		{
			value = VariantUtils.CreateFrom(in _testBoneName);
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
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isActive, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.String, PropertyName._testBoneName, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._isActive, Variant.From(in _isActive));
		info.AddProperty(PropertyName._testBoneName, Variant.From(in _testBoneName));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._isActive, out var value))
		{
			_isActive = value.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._testBoneName, out var value2))
		{
			_testBoneName = value2.As<string>();
		}
	}
}

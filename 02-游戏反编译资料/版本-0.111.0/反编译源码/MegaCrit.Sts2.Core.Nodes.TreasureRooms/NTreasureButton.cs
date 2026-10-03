using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;

namespace MegaCrit.Sts2.Core.Nodes.TreasureRooms;

[ScriptPath("res://src/Core/Nodes/TreasureRooms/NTreasureButton.cs")]
public class NTreasureButton : NButton
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : NButton.MethodName
	{
		/// <summary>
		/// Cached name for the '_Ready' method.
		/// </summary>
		public new static readonly StringName _Ready = "_Ready";

		/// <summary>
		/// Cached name for the 'AnimOut' method.
		/// </summary>
		public static readonly StringName AnimOut = "AnimOut";

		/// <summary>
		/// Cached name for the 'AnimIn' method.
		/// </summary>
		public static readonly StringName AnimIn = "AnimIn";

		/// <summary>
		/// Cached name for the 'UpdateChestSkin' method.
		/// </summary>
		public static readonly StringName UpdateChestSkin = "UpdateChestSkin";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : NButton.PropertyName
	{
		/// <summary>
		/// Cached name for the 'Hotkeys' property.
		/// </summary>
		public new static readonly StringName Hotkeys = "Hotkeys";

		/// <summary>
		/// Cached name for the '_chestNode' field.
		/// </summary>
		public static readonly StringName _chestNode = "_chestNode";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : NButton.SignalName
	{
	}

	private Node2D _chestNode;

	private MegaSprite _chestAnimController;

	private MegaSkin? _regularChestSkin;

	private MegaSkin? _outlineChestSkin;

	protected override string[] Hotkeys => new string[1] { MegaInput.select };

	public override void _Ready()
	{
		ConnectSignals();
		_chestNode = GetNode<Node2D>("%ChestVisual");
		_chestAnimController = new MegaSprite(_chestNode);
	}

	public void Setup(ActModel act)
	{
		_chestAnimController.SetSkeletonDataRes(act.ChestSpineResource);
		MegaSkeleton skeleton = _chestAnimController.GetSkeleton();
		if (skeleton != null)
		{
			MegaSkeletonDataResource data = skeleton.GetData();
			_regularChestSkin = data.FindSkin(act.ChestSpineSkinNameNormal);
			_outlineChestSkin = data.FindSkin(act.ChestSpineSkinNameStroke);
			skeleton.SetSlotsToSetupPose();
			_chestAnimController.GetAnimationState().Apply(skeleton);
			MegaAnimationState animationState = _chestAnimController.GetAnimationState();
			animationState.SetAnimation("animation", loop: false);
			_chestAnimController.GetAnimationState().AddAnimation("shine_fade", 0f, loop: false);
			animationState.SetTimeScale(0f);
			UpdateChestSkin(showOutline: false);
		}
	}

	public void AnimOut()
	{
		_chestAnimController.GetAnimationState().SetTimeScale(1f);
		Tween tween = CreateTween().SetParallel();
		tween.TweenProperty(_chestNode, "modulate", StsColors.halfTransparentWhite, 0.4);
	}

	public void AnimIn()
	{
		_chestAnimController.GetAnimationState().SetTimeScale(1f);
		Tween tween = CreateTween().SetParallel();
		tween.TweenProperty(_chestNode, "modulate", Colors.White, 0.3);
	}

	public void UpdateChestSkin(bool showOutline)
	{
		MegaSkeleton skeleton = _chestAnimController.GetSkeleton();
		if (skeleton != null)
		{
			skeleton.SetSkin(showOutline ? _outlineChestSkin : _regularChestSkin);
			skeleton.SetSlotsToSetupPose();
			_chestAnimController.GetAnimationState().Apply(skeleton);
		}
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(4);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AnimOut, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AnimIn, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateChestSkin, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "showOutline", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
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
		if (method == MethodName.AnimOut && args.Count == 0)
		{
			AnimOut();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimIn && args.Count == 0)
		{
			AnimIn();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateChestSkin && args.Count == 1)
		{
			UpdateChestSkin(VariantUtils.ConvertTo<bool>(in args[0]));
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
		if (method == MethodName.AnimOut)
		{
			return true;
		}
		if (method == MethodName.AnimIn)
		{
			return true;
		}
		if (method == MethodName.UpdateChestSkin)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._chestNode)
		{
			_chestNode = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.Hotkeys)
		{
			string[] from = Hotkeys;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName._chestNode)
		{
			value = VariantUtils.CreateFrom(in _chestNode);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._chestNode, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.PackedStringArray, PropertyName.Hotkeys, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._chestNode, Variant.From(in _chestNode));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._chestNode, out var value))
		{
			_chestNode = value.As<Node2D>();
		}
	}
}

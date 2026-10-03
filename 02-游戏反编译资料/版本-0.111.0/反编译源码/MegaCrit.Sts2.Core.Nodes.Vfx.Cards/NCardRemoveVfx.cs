using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Vfx.Cards;

[ScriptPath("res://src/Core/Nodes/Vfx/Cards/NCardRemoveVfx.cs")]
public class NCardRemoveVfx : Control
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : Control.MethodName
	{
		/// <summary>
		/// Cached name for the 'Create' method.
		/// </summary>
		public static readonly StringName Create = "Create";

		/// <summary>
		/// Cached name for the '_Ready' method.
		/// </summary>
		public new static readonly StringName _Ready = "_Ready";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Control.PropertyName
	{
		/// <summary>
		/// Cached name for the '_anticipationParticles' field.
		/// </summary>
		public static readonly StringName _anticipationParticles = "_anticipationParticles";

		/// <summary>
		/// Cached name for the '_slashStartParticles' field.
		/// </summary>
		public static readonly StringName _slashStartParticles = "_slashStartParticles";

		/// <summary>
		/// Cached name for the '_slashEndParticles' field.
		/// </summary>
		public static readonly StringName _slashEndParticles = "_slashEndParticles";

		/// <summary>
		/// Cached name for the '_cardParticles' field.
		/// </summary>
		public static readonly StringName _cardParticles = "_cardParticles";

		/// <summary>
		/// Cached name for the '_anticipationDuration' field.
		/// </summary>
		public static readonly StringName _anticipationDuration = "_anticipationDuration";

		/// <summary>
		/// Cached name for the '_slashEndDelay' field.
		/// </summary>
		public static readonly StringName _slashEndDelay = "_slashEndDelay";

		/// <summary>
		/// Cached name for the '_cardNode' field.
		/// </summary>
		public static readonly StringName _cardNode = "_cardNode";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Control.SignalName
	{
	}

	public const float deleteCardDelay = 0.4f;

	public static readonly string scenePath = SceneHelper.GetScenePath("vfx/ui/card/vfx_card_remove");

	[Export(PropertyHint.None, "")]
	private NParticlesContainer _anticipationParticles;

	[Export(PropertyHint.None, "")]
	private NParticlesContainer _slashStartParticles;

	[Export(PropertyHint.None, "")]
	private NParticlesContainer _slashEndParticles;

	[Export(PropertyHint.None, "")]
	private NParticlesContainer _cardParticles;

	[Export(PropertyHint.None, "")]
	private float _anticipationDuration = 0.25f;

	[Export(PropertyHint.None, "")]
	private float _slashEndDelay = 0.1f;

	private NCard _cardNode;

	public static NCardRemoveVfx? Create(NCard cardNode)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NCardRemoveVfx nCardRemoveVfx = PreloadManager.Cache.GetScene(scenePath).Instantiate<NCardRemoveVfx>(PackedScene.GenEditState.Disabled);
		nCardRemoveVfx._cardNode = cardNode;
		return nCardRemoveVfx;
	}

	public override void _Ready()
	{
		base.GlobalPosition = _cardNode.GlobalPosition;
		base.Rotation = _cardNode.Rotation;
		_cardParticles.SetEmitting(emitting: false);
		TaskHelper.RunSafely(PlayAnimation());
	}

	private async Task PlayAnimation()
	{
		_anticipationParticles.Restart();
		await Cmd.Wait(_anticipationDuration);
		_slashStartParticles.Restart();
		await Cmd.Wait(_slashEndDelay);
		_slashEndParticles.Restart();
		_cardParticles.Restart();
		TaskHelper.RunSafely(DelayedFree());
	}

	private async Task DelayedFree()
	{
		await Cmd.Wait(2f);
		this.QueueFreeSafely();
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(2);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "cardNode", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 1)
		{
			NCardRemoveVfx from = Create(VariantUtils.ConvertTo<NCard>(in args[0]));
			ret = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 1)
		{
			NCardRemoveVfx from = Create(VariantUtils.ConvertTo<NCard>(in args[0]));
			ret = VariantUtils.CreateFrom(in from);
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.Create)
		{
			return true;
		}
		if (method == MethodName._Ready)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._anticipationParticles)
		{
			_anticipationParticles = VariantUtils.ConvertTo<NParticlesContainer>(in value);
			return true;
		}
		if (name == PropertyName._slashStartParticles)
		{
			_slashStartParticles = VariantUtils.ConvertTo<NParticlesContainer>(in value);
			return true;
		}
		if (name == PropertyName._slashEndParticles)
		{
			_slashEndParticles = VariantUtils.ConvertTo<NParticlesContainer>(in value);
			return true;
		}
		if (name == PropertyName._cardParticles)
		{
			_cardParticles = VariantUtils.ConvertTo<NParticlesContainer>(in value);
			return true;
		}
		if (name == PropertyName._anticipationDuration)
		{
			_anticipationDuration = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._slashEndDelay)
		{
			_slashEndDelay = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._cardNode)
		{
			_cardNode = VariantUtils.ConvertTo<NCard>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._anticipationParticles)
		{
			value = VariantUtils.CreateFrom(in _anticipationParticles);
			return true;
		}
		if (name == PropertyName._slashStartParticles)
		{
			value = VariantUtils.CreateFrom(in _slashStartParticles);
			return true;
		}
		if (name == PropertyName._slashEndParticles)
		{
			value = VariantUtils.CreateFrom(in _slashEndParticles);
			return true;
		}
		if (name == PropertyName._cardParticles)
		{
			value = VariantUtils.CreateFrom(in _cardParticles);
			return true;
		}
		if (name == PropertyName._anticipationDuration)
		{
			value = VariantUtils.CreateFrom(in _anticipationDuration);
			return true;
		}
		if (name == PropertyName._slashEndDelay)
		{
			value = VariantUtils.CreateFrom(in _slashEndDelay);
			return true;
		}
		if (name == PropertyName._cardNode)
		{
			value = VariantUtils.CreateFrom(in _cardNode);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._anticipationParticles, PropertyHint.NodeType, "Node2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._slashStartParticles, PropertyHint.NodeType, "Node2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._slashEndParticles, PropertyHint.NodeType, "Node2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._cardParticles, PropertyHint.NodeType, "Node2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._anticipationDuration, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._slashEndDelay, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._cardNode, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._anticipationParticles, Variant.From(in _anticipationParticles));
		info.AddProperty(PropertyName._slashStartParticles, Variant.From(in _slashStartParticles));
		info.AddProperty(PropertyName._slashEndParticles, Variant.From(in _slashEndParticles));
		info.AddProperty(PropertyName._cardParticles, Variant.From(in _cardParticles));
		info.AddProperty(PropertyName._anticipationDuration, Variant.From(in _anticipationDuration));
		info.AddProperty(PropertyName._slashEndDelay, Variant.From(in _slashEndDelay));
		info.AddProperty(PropertyName._cardNode, Variant.From(in _cardNode));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._anticipationParticles, out var value))
		{
			_anticipationParticles = value.As<NParticlesContainer>();
		}
		if (info.TryGetProperty(PropertyName._slashStartParticles, out var value2))
		{
			_slashStartParticles = value2.As<NParticlesContainer>();
		}
		if (info.TryGetProperty(PropertyName._slashEndParticles, out var value3))
		{
			_slashEndParticles = value3.As<NParticlesContainer>();
		}
		if (info.TryGetProperty(PropertyName._cardParticles, out var value4))
		{
			_cardParticles = value4.As<NParticlesContainer>();
		}
		if (info.TryGetProperty(PropertyName._anticipationDuration, out var value5))
		{
			_anticipationDuration = value5.As<float>();
		}
		if (info.TryGetProperty(PropertyName._slashEndDelay, out var value6))
		{
			_slashEndDelay = value6.As<float>();
		}
		if (info.TryGetProperty(PropertyName._cardNode, out var value7))
		{
			_cardNode = value7.As<NCard>();
		}
	}
}

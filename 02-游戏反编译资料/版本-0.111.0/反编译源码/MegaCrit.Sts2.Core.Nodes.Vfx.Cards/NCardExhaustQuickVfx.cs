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
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Vfx.Cards;

[ScriptPath("res://src/Core/Nodes/Vfx/Cards/NCardExhaustQuickVfx.cs")]
public class NCardExhaustQuickVfx : Control
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
		/// Cached name for the '_ExitTree' method.
		/// </summary>
		public new static readonly StringName _ExitTree = "_ExitTree";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Control.PropertyName
	{
		/// <summary>
		/// Cached name for the '_anticipationParticlesContainer' field.
		/// </summary>
		public static readonly StringName _anticipationParticlesContainer = "_anticipationParticlesContainer";

		/// <summary>
		/// Cached name for the '_particlesContainer' field.
		/// </summary>
		public static readonly StringName _particlesContainer = "_particlesContainer";

		/// <summary>
		/// Cached name for the '_anticipationDuration' field.
		/// </summary>
		public static readonly StringName _anticipationDuration = "_anticipationDuration";

		/// <summary>
		/// Cached name for the '_isFinishing' field.
		/// </summary>
		public static readonly StringName _isFinishing = "_isFinishing";

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

	public static readonly string scenePath = SceneHelper.GetScenePath("vfx/ui/card/vfx_card_exhaust_quick");

	[Export(PropertyHint.None, "")]
	private NParticlesContainer _anticipationParticlesContainer;

	[Export(PropertyHint.None, "")]
	private NParticlesContainer _particlesContainer;

	[Export(PropertyHint.None, "")]
	private float _anticipationDuration = 0.4f;

	private bool _isFinishing;

	private NCard _cardNode;

	public static NCardExhaustQuickVfx? Create(NCard cardNode)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NCardExhaustQuickVfx nCardExhaustQuickVfx = PreloadManager.Cache.GetScene(scenePath).Instantiate<NCardExhaustQuickVfx>(PackedScene.GenEditState.Disabled);
		nCardExhaustQuickVfx._cardNode = cardNode;
		nCardExhaustQuickVfx._anticipationParticlesContainer.Modulate = new Color(1f, 1f, 1f);
		cardNode.CardVfxContainer.AddChildSafely(nCardExhaustQuickVfx);
		return nCardExhaustQuickVfx;
	}

	public async Task PlayAnimation()
	{
		_isFinishing = false;
		_anticipationParticlesContainer.Restart();
		await Cmd.Wait(_anticipationDuration);
		_isFinishing = true;
		_anticipationParticlesContainer.Modulate = new Color(1f, 1f, 1f, 0f);
		Node parent = GetParent();
		Vector2 globalPosition = base.GlobalPosition;
		float rotation = base.Rotation;
		Vector2 scale = _cardNode.Scale;
		parent?.RemoveChildSafely(this);
		if (NCombatRoom.Instance != null)
		{
			NCombatRoom.Instance.Ui.AddChildSafely(this);
			base.GlobalPosition = globalPosition;
			base.Rotation = rotation;
			base.Scale = scale;
		}
		_cardNode.QueueFreeSafely();
		_particlesContainer.Restart();
		TaskHelper.RunSafely(DelayedFree());
	}

	private async Task DelayedFree()
	{
		await Cmd.Wait(2f);
		this.QueueFreeSafely();
	}

	public override void _ExitTree()
	{
		if (!_isFinishing && GodotObject.IsInstanceValid(_cardNode))
		{
			_cardNode.QueueFreeSafely();
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
		List<MethodInfo> list = new List<MethodInfo>(2);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "cardNode", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 1)
		{
			NCardExhaustQuickVfx from = Create(VariantUtils.ConvertTo<NCard>(in args[0]));
			ret = VariantUtils.CreateFrom(in from);
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

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 1)
		{
			NCardExhaustQuickVfx from = Create(VariantUtils.ConvertTo<NCard>(in args[0]));
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
		if (name == PropertyName._anticipationParticlesContainer)
		{
			_anticipationParticlesContainer = VariantUtils.ConvertTo<NParticlesContainer>(in value);
			return true;
		}
		if (name == PropertyName._particlesContainer)
		{
			_particlesContainer = VariantUtils.ConvertTo<NParticlesContainer>(in value);
			return true;
		}
		if (name == PropertyName._anticipationDuration)
		{
			_anticipationDuration = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._isFinishing)
		{
			_isFinishing = VariantUtils.ConvertTo<bool>(in value);
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
		if (name == PropertyName._anticipationParticlesContainer)
		{
			value = VariantUtils.CreateFrom(in _anticipationParticlesContainer);
			return true;
		}
		if (name == PropertyName._particlesContainer)
		{
			value = VariantUtils.CreateFrom(in _particlesContainer);
			return true;
		}
		if (name == PropertyName._anticipationDuration)
		{
			value = VariantUtils.CreateFrom(in _anticipationDuration);
			return true;
		}
		if (name == PropertyName._isFinishing)
		{
			value = VariantUtils.CreateFrom(in _isFinishing);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._anticipationParticlesContainer, PropertyHint.NodeType, "Node2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._particlesContainer, PropertyHint.NodeType, "Node2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._anticipationDuration, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isFinishing, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._cardNode, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._anticipationParticlesContainer, Variant.From(in _anticipationParticlesContainer));
		info.AddProperty(PropertyName._particlesContainer, Variant.From(in _particlesContainer));
		info.AddProperty(PropertyName._anticipationDuration, Variant.From(in _anticipationDuration));
		info.AddProperty(PropertyName._isFinishing, Variant.From(in _isFinishing));
		info.AddProperty(PropertyName._cardNode, Variant.From(in _cardNode));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._anticipationParticlesContainer, out var value))
		{
			_anticipationParticlesContainer = value.As<NParticlesContainer>();
		}
		if (info.TryGetProperty(PropertyName._particlesContainer, out var value2))
		{
			_particlesContainer = value2.As<NParticlesContainer>();
		}
		if (info.TryGetProperty(PropertyName._anticipationDuration, out var value3))
		{
			_anticipationDuration = value3.As<float>();
		}
		if (info.TryGetProperty(PropertyName._isFinishing, out var value4))
		{
			_isFinishing = value4.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._cardNode, out var value5))
		{
			_cardNode = value5.As<NCard>();
		}
	}
}

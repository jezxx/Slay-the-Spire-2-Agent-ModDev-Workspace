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
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Vfx.Cards;

[ScriptPath("res://src/Core/Nodes/Vfx/Cards/NCardExhaustVfx.cs")]
public class NCardExhaustVfx : Control
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
		/// Cached name for the 'SetParticlesPlaying' method.
		/// </summary>
		public static readonly StringName SetParticlesPlaying = "SetParticlesPlaying";

		/// <summary>
		/// Cached name for the 'SetProgress' method.
		/// </summary>
		public static readonly StringName SetProgress = "SetProgress";

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
		/// Cached name for the '_cardParentContainer' field.
		/// </summary>
		public static readonly StringName _cardParentContainer = "_cardParentContainer";

		/// <summary>
		/// Cached name for the '_materialContainer' field.
		/// </summary>
		public static readonly StringName _materialContainer = "_materialContainer";

		/// <summary>
		/// Cached name for the '_particlesContainer' field.
		/// </summary>
		public static readonly StringName _particlesContainer = "_particlesContainer";

		/// <summary>
		/// Cached name for the '_exhaustDuration' field.
		/// </summary>
		public static readonly StringName _exhaustDuration = "_exhaustDuration";

		/// <summary>
		/// Cached name for the '_exhaustCurve' field.
		/// </summary>
		public static readonly StringName _exhaustCurve = "_exhaustCurve";

		/// <summary>
		/// Cached name for the '_erosionBaseRange' field.
		/// </summary>
		public static readonly StringName _erosionBaseRange = "_erosionBaseRange";

		/// <summary>
		/// Cached name for the '_particleHeightRange' field.
		/// </summary>
		public static readonly StringName _particleHeightRange = "_particleHeightRange";

		/// <summary>
		/// Cached name for the '_cardNode' field.
		/// </summary>
		public static readonly StringName _cardNode = "_cardNode";

		/// <summary>
		/// Cached name for the '_position' field.
		/// </summary>
		public static readonly StringName _position = "_position";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Control.SignalName
	{
	}

	public static readonly string scenePath = SceneHelper.GetScenePath("vfx/ui/card/vfx_card_exhaust");

	[Export(PropertyHint.None, "")]
	private Control _cardParentContainer;

	[Export(PropertyHint.None, "")]
	private Control _materialContainer;

	[Export(PropertyHint.None, "")]
	private NParticlesContainer _particlesContainer;

	[Export(PropertyHint.None, "")]
	private float _exhaustDuration = 0.4f;

	[Export(PropertyHint.None, "")]
	private Curve _exhaustCurve;

	[Export(PropertyHint.None, "")]
	private Vector2 _erosionBaseRange;

	[Export(PropertyHint.None, "")]
	private Vector2 _particleHeightRange;

	private NCard _cardNode;

	private Vector2 _position;

	private static readonly StringName _erosionBaseParameter = new StringName("instance_shader_parameters/erosion_base");

	private static readonly StringName _erosionOffsetParameter = new StringName("instance_shader_parameters/erosion_texture_x_offset");

	public static NCardExhaustVfx? Create(NCard cardNode)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NCardExhaustVfx nCardExhaustVfx = PreloadManager.Cache.GetScene(scenePath).Instantiate<NCardExhaustVfx>(PackedScene.GenEditState.Disabled);
		nCardExhaustVfx.SetParticlesPlaying(isPlaying: false);
		nCardExhaustVfx.SetProgress(0f);
		nCardExhaustVfx._position = cardNode.GlobalPosition;
		cardNode.GetParent()?.RemoveChildSafely(cardNode);
		nCardExhaustVfx._cardParentContainer.AddChildSafely(cardNode);
		nCardExhaustVfx._cardNode = cardNode;
		return nCardExhaustVfx;
	}

	private void SetParticlesPlaying(bool isPlaying)
	{
		_particlesContainer.SetEmitting(isPlaying);
	}

	private void SetProgress(float progress)
	{
		float weight = _exhaustCurve.Sample(progress);
		float num = Mathf.Lerp(_erosionBaseRange.X, _erosionBaseRange.Y, weight);
		float y = Mathf.Lerp(_particleHeightRange.X, _particleHeightRange.Y, weight);
		_materialContainer.Set(_erosionBaseParameter, num);
		_particlesContainer.Position = new Vector2(0f, y);
	}

	public async Task PlayAnimation()
	{
		base.GlobalPosition = _position;
		_cardNode.Position = _cardParentContainer.Size / 2f;
		_materialContainer.SelfModulate = new Color(1f, 1f, 1f);
		SetParticlesPlaying(isPlaying: true);
		SetProgress(0f);
		_materialContainer.Set(_erosionOffsetParameter, GD.Randf());
		float num = 0f;
		while (num < _exhaustDuration)
		{
			float progress = num / _exhaustDuration;
			SetProgress(progress);
			float num2 = num;
			num = num2 + await this.AwaitProcessFrame();
		}
		SetProgress(1f);
		SetParticlesPlaying(isPlaying: false);
		_materialContainer.SelfModulate = new Color(1f, 1f, 1f, 0f);
		TaskHelper.RunSafely(DelayedFree());
	}

	private async Task DelayedFree()
	{
		await Cmd.Wait(2f);
		_cardNode.QueueFreeSafely();
		this.QueueFreeSafely();
	}

	public override void _ExitTree()
	{
		if (GodotObject.IsInstanceValid(_cardNode) && IsAncestorOf(_cardNode))
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
		List<MethodInfo> list = new List<MethodInfo>(4);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "cardNode", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SetParticlesPlaying, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "isPlaying", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SetProgress, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "progress", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
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
			NCardExhaustVfx from = Create(VariantUtils.ConvertTo<NCard>(in args[0]));
			ret = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (method == MethodName.SetParticlesPlaying && args.Count == 1)
		{
			SetParticlesPlaying(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetProgress && args.Count == 1)
		{
			SetProgress(VariantUtils.ConvertTo<float>(in args[0]));
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

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 1)
		{
			NCardExhaustVfx from = Create(VariantUtils.ConvertTo<NCard>(in args[0]));
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
		if (method == MethodName.SetParticlesPlaying)
		{
			return true;
		}
		if (method == MethodName.SetProgress)
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
		if (name == PropertyName._cardParentContainer)
		{
			_cardParentContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._materialContainer)
		{
			_materialContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._particlesContainer)
		{
			_particlesContainer = VariantUtils.ConvertTo<NParticlesContainer>(in value);
			return true;
		}
		if (name == PropertyName._exhaustDuration)
		{
			_exhaustDuration = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._exhaustCurve)
		{
			_exhaustCurve = VariantUtils.ConvertTo<Curve>(in value);
			return true;
		}
		if (name == PropertyName._erosionBaseRange)
		{
			_erosionBaseRange = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._particleHeightRange)
		{
			_particleHeightRange = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._cardNode)
		{
			_cardNode = VariantUtils.ConvertTo<NCard>(in value);
			return true;
		}
		if (name == PropertyName._position)
		{
			_position = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._cardParentContainer)
		{
			value = VariantUtils.CreateFrom(in _cardParentContainer);
			return true;
		}
		if (name == PropertyName._materialContainer)
		{
			value = VariantUtils.CreateFrom(in _materialContainer);
			return true;
		}
		if (name == PropertyName._particlesContainer)
		{
			value = VariantUtils.CreateFrom(in _particlesContainer);
			return true;
		}
		if (name == PropertyName._exhaustDuration)
		{
			value = VariantUtils.CreateFrom(in _exhaustDuration);
			return true;
		}
		if (name == PropertyName._exhaustCurve)
		{
			value = VariantUtils.CreateFrom(in _exhaustCurve);
			return true;
		}
		if (name == PropertyName._erosionBaseRange)
		{
			value = VariantUtils.CreateFrom(in _erosionBaseRange);
			return true;
		}
		if (name == PropertyName._particleHeightRange)
		{
			value = VariantUtils.CreateFrom(in _particleHeightRange);
			return true;
		}
		if (name == PropertyName._cardNode)
		{
			value = VariantUtils.CreateFrom(in _cardNode);
			return true;
		}
		if (name == PropertyName._position)
		{
			value = VariantUtils.CreateFrom(in _position);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._cardParentContainer, PropertyHint.NodeType, "Control", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._materialContainer, PropertyHint.NodeType, "Control", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._particlesContainer, PropertyHint.NodeType, "Node2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._exhaustDuration, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._exhaustCurve, PropertyHint.ResourceType, "Curve", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._erosionBaseRange, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._particleHeightRange, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._cardNode, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._position, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._cardParentContainer, Variant.From(in _cardParentContainer));
		info.AddProperty(PropertyName._materialContainer, Variant.From(in _materialContainer));
		info.AddProperty(PropertyName._particlesContainer, Variant.From(in _particlesContainer));
		info.AddProperty(PropertyName._exhaustDuration, Variant.From(in _exhaustDuration));
		info.AddProperty(PropertyName._exhaustCurve, Variant.From(in _exhaustCurve));
		info.AddProperty(PropertyName._erosionBaseRange, Variant.From(in _erosionBaseRange));
		info.AddProperty(PropertyName._particleHeightRange, Variant.From(in _particleHeightRange));
		info.AddProperty(PropertyName._cardNode, Variant.From(in _cardNode));
		info.AddProperty(PropertyName._position, Variant.From(in _position));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._cardParentContainer, out var value))
		{
			_cardParentContainer = value.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._materialContainer, out var value2))
		{
			_materialContainer = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._particlesContainer, out var value3))
		{
			_particlesContainer = value3.As<NParticlesContainer>();
		}
		if (info.TryGetProperty(PropertyName._exhaustDuration, out var value4))
		{
			_exhaustDuration = value4.As<float>();
		}
		if (info.TryGetProperty(PropertyName._exhaustCurve, out var value5))
		{
			_exhaustCurve = value5.As<Curve>();
		}
		if (info.TryGetProperty(PropertyName._erosionBaseRange, out var value6))
		{
			_erosionBaseRange = value6.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._particleHeightRange, out var value7))
		{
			_particleHeightRange = value7.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._cardNode, out var value8))
		{
			_cardNode = value8.As<NCard>();
		}
		if (info.TryGetProperty(PropertyName._position, out var value9))
		{
			_position = value9.As<Vector2>();
		}
	}
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.Relics;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.addons.mega_text;
using Steamworks;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Map;

[ScriptPath("res://src/Core/Nodes/Screens/Map/NMapShareButton.cs")]
public class NMapShareButton : NButton
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
		/// Cached name for the 'OnFocus' method.
		/// </summary>
		public new static readonly StringName OnFocus = "OnFocus";

		/// <summary>
		/// Cached name for the 'OnUnfocus' method.
		/// </summary>
		public new static readonly StringName OnUnfocus = "OnUnfocus";

		/// <summary>
		/// Cached name for the 'Initialize' method.
		/// </summary>
		public static readonly StringName Initialize = "Initialize";

		/// <summary>
		/// Cached name for the 'OnRelease' method.
		/// </summary>
		public new static readonly StringName OnRelease = "OnRelease";

		/// <summary>
		/// Cached name for the 'IsTakingScreenshot' method.
		/// </summary>
		public static readonly StringName IsTakingScreenshot = "IsTakingScreenshot";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : NButton.PropertyName
	{
		/// <summary>
		/// Cached name for the '_buttonImage' field.
		/// </summary>
		public static readonly StringName _buttonImage = "_buttonImage";

		/// <summary>
		/// Cached name for the '_label' field.
		/// </summary>
		public static readonly StringName _label = "_label";

		/// <summary>
		/// Cached name for the '_labelContainer' field.
		/// </summary>
		public static readonly StringName _labelContainer = "_labelContainer";

		/// <summary>
		/// Cached name for the '_tween' field.
		/// </summary>
		public static readonly StringName _tween = "_tween";

		/// <summary>
		/// Cached name for the '_mapScreen' field.
		/// </summary>
		public static readonly StringName _mapScreen = "_mapScreen";

		/// <summary>
		/// Cached name for the '_mapContainer' field.
		/// </summary>
		public static readonly StringName _mapContainer = "_mapContainer";

		/// <summary>
		/// Cached name for the '_mapBgContainer' field.
		/// </summary>
		public static readonly StringName _mapBgContainer = "_mapBgContainer";

		/// <summary>
		/// Cached name for the '_subViewport' field.
		/// </summary>
		public static readonly StringName _subViewport = "_subViewport";

		/// <summary>
		/// Cached name for the '_toastPosition' field.
		/// </summary>
		public static readonly StringName _toastPosition = "_toastPosition";

		/// <summary>
		/// Cached name for the '_toast' field.
		/// </summary>
		public static readonly StringName _toast = "_toast";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : NButton.SignalName
	{
	}

	private static readonly Color _activeButtonColor = new Color("7B1B15");

	private static readonly Color _inactiveButtonColor = new Color("000000C0");

	private static readonly Color _activeLabelColor = Colors.White;

	private static readonly Color _inactiveLabelColor = StsColors.halfTransparentWhite;

	private TextureRect _buttonImage;

	private MegaLabel _label;

	private Control _labelContainer;

	private HoverTip _hoverTip;

	private Tween? _tween;

	private NMapScreen? _mapScreen;

	private Control? _mapContainer;

	private Control? _mapBgContainer;

	private SubViewport? _subViewport;

	private Vector2 _toastPosition;

	private MegaLabel? _toast;

	public override void _Ready()
	{
		ConnectSignals();
		_buttonImage = GetNode<TextureRect>("ButtonImage");
		_labelContainer = GetNode<Control>("LabelContainer");
		_label = GetNode<MegaLabel>("LabelContainer/HBoxContainer/Label");
		LocString locString = new LocString("map", "SHARE.title");
		LocString description = ((PlatformUtil.PrimaryPlatform != PlatformType.Steam) ? new LocString("map", "SHARE.description.other") : new LocString("map", "SHARE.description.steam"));
		_hoverTip = new HoverTip(locString, description);
		_label.SetTextAutoSize(locString.GetFormattedText());
	}

	protected override void OnFocus()
	{
		base.OnFocus();
		_tween?.Kill();
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(this, "scale", Vector2.One * 1.05f, 0.05);
		_tween.TweenProperty(_buttonImage, "modulate", _activeButtonColor, 0.05);
		_tween.TweenProperty(_labelContainer, "modulate", _activeLabelColor, 0.05);
		NHoverTipSet nHoverTipSet = NHoverTipSet.CreateAndShow(this, _hoverTip);
		nHoverTipSet?.SetGlobalPosition(base.GlobalPosition - nHoverTipSet.Size + new Vector2(-10f, base.Size.Y));
	}

	protected override void OnUnfocus()
	{
		base.OnUnfocus();
		_tween?.Kill();
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(this, "scale", Vector2.One, 0.5).SetTrans(Tween.TransitionType.Expo).SetEase(Tween.EaseType.Out);
		_tween.TweenProperty(_buttonImage, "modulate", _inactiveButtonColor, 0.1);
		_tween.TweenProperty(_labelContainer, "modulate", _inactiveLabelColor, 0.1);
		NHoverTipSet.Remove(this);
	}

	public void Initialize(NMapScreen mapScreen, Control mapContainer, Control mapBgContainer)
	{
		_mapScreen = mapScreen;
		_mapContainer = mapContainer;
		_mapBgContainer = mapBgContainer;
		_toast = mapScreen.GetNode<MegaLabel>("%ShareToast");
		_toastPosition = _toast.Position;
		_toast.SelfModulate = Colors.Transparent;
	}

	protected override void OnRelease()
	{
		if (!IsTakingScreenshot())
		{
			TaskHelper.RunSafely(CopyMapScreenshot());
		}
	}

	public bool IsTakingScreenshot()
	{
		return _subViewport != null;
	}

	private async Task CopyMapScreenshot()
	{
		if (_mapScreen == null || _mapContainer == null || _mapBgContainer == null)
		{
			Log.Error("Tried to take map screenshot when share button wasn't ready yet!");
			return;
		}
		NGlobalUi globalUi = NRun.Instance.GlobalUi;
		Control topBar = globalUi.TopBar;
		Control debugInfo = globalUi.DebugInfo;
		NRelicInventory relicInventory = globalUi.RelicInventory;
		float topBarSize = relicInventory.GetBottomOfInventory().Y;
		_subViewport = new SubViewport();
		_subViewport.Size = new Vector2I(Mathf.RoundToInt(_mapBgContainer.Size.X), Mathf.RoundToInt(_mapBgContainer.Size.Y + topBarSize));
		List<(Control node, Node originalParent, Node tempParent, Vector2 position, int index)> borrowed = new List<(Control, Node, Node, Vector2, int)>();
		try
		{
			Control subViewportParent = new Control();
			_subViewport.AddChildSafely(subViewportParent);
			Borrow(_mapContainer, subViewportParent);
			_mapScreen.AddChildSafely(_subViewport);
			_subViewport.RenderTargetUpdateMode = SubViewport.UpdateMode.Once;
			await this.AwaitProcessFrame();
			if (!GodotObject.IsInstanceValid(_mapScreen))
			{
				return;
			}
			subViewportParent.Size = _mapScreen.Size;
			subViewportParent.Position = -_mapBgContainer.Position + topBarSize * Vector2.Down;
			_mapContainer.Size = subViewportParent.Size;
			_mapContainer.Position = Vector2.Zero;
			Borrow(topBar, _subViewport);
			topBar.Position = Vector2.Zero;
			Borrow(debugInfo, _subViewport);
			Borrow(relicInventory, _subViewport);
			await _subViewport.ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
			Image image = _subViewport.GetTexture().GetImage();
			if (image == null)
			{
				Log.Error("Failed to capture map screenshot: subViewport produced no image.");
				return;
			}
			if (PlatformUtil.PrimaryPlatform == PlatformType.Steam)
			{
				if (image.GetFormat() != Image.Format.Rgb8)
				{
					image.Convert(Image.Format.Rgb8);
				}
				byte[] array = image.Data["data"].AsByteArray();
				SteamScreenshots.WriteScreenshot(array, (uint)array.Length, image.GetWidth(), image.GetHeight());
				ShowToast(new LocString("map", "SHARE_TOAST.description.steam"));
				return;
			}
			string text = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
			string text2 = "user://map_screenshot_" + text + ".png";
			Error error = image.SavePng(text2);
			if (error != Error.Ok)
			{
				Log.Error($"Error {error}: Failed to save map screenshot to '{text2}'.");
			}
			else
			{
				TaskHelper.RunSafely(ShowConfirmation(text2));
			}
		}
		finally
		{
			try
			{
				foreach (var (control, node2, node3, position, val) in borrowed.OrderBy<(Control, Node, Node, Vector2, int), int>(((Control node, Node originalParent, Node tempParent, Vector2 position, int index) entry) => entry.index))
				{
					if (GodotObject.IsInstanceValid(control) && GodotObject.IsInstanceValid(node2) && control.GetParent() == node3)
					{
						control.Reparent(node2, keepGlobalTransform: false);
						control.Position = position;
						node2.MoveChildSafely(control, Math.Min(val, node2.GetChildCount() - 1));
					}
				}
			}
			finally
			{
				_subViewport.QueueFreeSafely();
				_subViewport = null;
			}
		}
		void Borrow(Control node, Node tempParent)
		{
			borrowed.Add((node, node.GetParent(), tempParent, node.Position, node.GetIndex()));
			node.Reparent(tempParent, keepGlobalTransform: false);
		}
	}

	private static async Task ShowConfirmation(string screenshotPath)
	{
		screenshotPath = ProjectSettings.GlobalizePath(screenshotPath);
		LocString locString = new LocString("map", "SHARE_POPUP.description");
		locString.Add("path", screenshotPath);
		NGenericPopup nGenericPopup = NGenericPopup.Create();
		NModalContainer.Instance.Add(nGenericPopup);
		if (await nGenericPopup.WaitForConfirmation(locString, new LocString("map", "SHARE_POPUP.title"), new LocString("main_menu_ui", "GENERIC_POPUP.ok"), new LocString("map", "SHARE_POPUP.open")))
		{
			Error error = OS.ShellShowInFileManager(screenshotPath);
			if (error != Error.Ok)
			{
				Log.Error($"Error {error}: Cannot open OS file manager. Screenshot saved to '{screenshotPath}'");
			}
		}
	}

	private void ShowToast(LocString locString)
	{
		if (_toast == null)
		{
			throw new InvalidOperationException("Tried to show toast before initialized!");
		}
		_toast.SetTextAutoSize(locString.GetFormattedText());
		_toast.Position = _toastPosition;
		_toast.SelfModulate = Colors.White;
		Tween tween = _toast.CreateTween();
		tween.TweenProperty(_toast, "position:y", _toastPosition.Y - 40f, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quad);
		tween.TweenInterval(2.0);
		tween.TweenProperty(_toast, "self_modulate:a", 0f, 0.5);
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(6);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnfocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Initialize, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "mapScreen", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false),
			new PropertyInfo(Variant.Type.Object, "mapContainer", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false),
			new PropertyInfo(Variant.Type.Object, "mapBgContainer", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnRelease, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.IsTakingScreenshot, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.OnFocus && args.Count == 0)
		{
			OnFocus();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnUnfocus && args.Count == 0)
		{
			OnUnfocus();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Initialize && args.Count == 3)
		{
			Initialize(VariantUtils.ConvertTo<NMapScreen>(in args[0]), VariantUtils.ConvertTo<Control>(in args[1]), VariantUtils.ConvertTo<Control>(in args[2]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnRelease && args.Count == 0)
		{
			OnRelease();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.IsTakingScreenshot && args.Count == 0)
		{
			bool from = IsTakingScreenshot();
			ret = VariantUtils.CreateFrom(in from);
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
		if (method == MethodName.OnFocus)
		{
			return true;
		}
		if (method == MethodName.OnUnfocus)
		{
			return true;
		}
		if (method == MethodName.Initialize)
		{
			return true;
		}
		if (method == MethodName.OnRelease)
		{
			return true;
		}
		if (method == MethodName.IsTakingScreenshot)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._buttonImage)
		{
			_buttonImage = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._label)
		{
			_label = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._labelContainer)
		{
			_labelContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._tween)
		{
			_tween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._mapScreen)
		{
			_mapScreen = VariantUtils.ConvertTo<NMapScreen>(in value);
			return true;
		}
		if (name == PropertyName._mapContainer)
		{
			_mapContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._mapBgContainer)
		{
			_mapBgContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._subViewport)
		{
			_subViewport = VariantUtils.ConvertTo<SubViewport>(in value);
			return true;
		}
		if (name == PropertyName._toastPosition)
		{
			_toastPosition = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._toast)
		{
			_toast = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._buttonImage)
		{
			value = VariantUtils.CreateFrom(in _buttonImage);
			return true;
		}
		if (name == PropertyName._label)
		{
			value = VariantUtils.CreateFrom(in _label);
			return true;
		}
		if (name == PropertyName._labelContainer)
		{
			value = VariantUtils.CreateFrom(in _labelContainer);
			return true;
		}
		if (name == PropertyName._tween)
		{
			value = VariantUtils.CreateFrom(in _tween);
			return true;
		}
		if (name == PropertyName._mapScreen)
		{
			value = VariantUtils.CreateFrom(in _mapScreen);
			return true;
		}
		if (name == PropertyName._mapContainer)
		{
			value = VariantUtils.CreateFrom(in _mapContainer);
			return true;
		}
		if (name == PropertyName._mapBgContainer)
		{
			value = VariantUtils.CreateFrom(in _mapBgContainer);
			return true;
		}
		if (name == PropertyName._subViewport)
		{
			value = VariantUtils.CreateFrom(in _subViewport);
			return true;
		}
		if (name == PropertyName._toastPosition)
		{
			value = VariantUtils.CreateFrom(in _toastPosition);
			return true;
		}
		if (name == PropertyName._toast)
		{
			value = VariantUtils.CreateFrom(in _toast);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._buttonImage, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._label, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._labelContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._mapScreen, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._mapContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._mapBgContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._subViewport, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._toastPosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._toast, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._buttonImage, Variant.From(in _buttonImage));
		info.AddProperty(PropertyName._label, Variant.From(in _label));
		info.AddProperty(PropertyName._labelContainer, Variant.From(in _labelContainer));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
		info.AddProperty(PropertyName._mapScreen, Variant.From(in _mapScreen));
		info.AddProperty(PropertyName._mapContainer, Variant.From(in _mapContainer));
		info.AddProperty(PropertyName._mapBgContainer, Variant.From(in _mapBgContainer));
		info.AddProperty(PropertyName._subViewport, Variant.From(in _subViewport));
		info.AddProperty(PropertyName._toastPosition, Variant.From(in _toastPosition));
		info.AddProperty(PropertyName._toast, Variant.From(in _toast));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._buttonImage, out var value))
		{
			_buttonImage = value.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._label, out var value2))
		{
			_label = value2.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._labelContainer, out var value3))
		{
			_labelContainer = value3.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._tween, out var value4))
		{
			_tween = value4.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._mapScreen, out var value5))
		{
			_mapScreen = value5.As<NMapScreen>();
		}
		if (info.TryGetProperty(PropertyName._mapContainer, out var value6))
		{
			_mapContainer = value6.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._mapBgContainer, out var value7))
		{
			_mapBgContainer = value7.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._subViewport, out var value8))
		{
			_subViewport = value8.As<SubViewport>();
		}
		if (info.TryGetProperty(PropertyName._toastPosition, out var value9))
		{
			_toastPosition = value9.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._toast, out var value10))
		{
			_toast = value10.As<MegaLabel>();
		}
	}
}

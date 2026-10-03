using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Bestiary;

/// <summary>
/// The current filter the player is viewing for the Bestiary.
/// Unlike the other tickboxes, this one is a radio-button style.
/// Clicking on a pool filter will deselect the others. If this filter
/// is already active, then you can't click on it.
/// </summary>
[ScriptPath("res://src/Core/Nodes/Screens/Bestiary/NBestiaryCharacterFilter.cs")]
public class NBestiaryCharacterFilter : NButton
{
	[Signal]
	public delegate void ToggledEventHandler(NBestiaryCharacterFilter filter);

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
		/// Cached name for the 'OnToggle' method.
		/// </summary>
		public static readonly StringName OnToggle = "OnToggle";

		/// <summary>
		/// Cached name for the 'SetLockedState' method.
		/// </summary>
		public static readonly StringName SetLockedState = "SetLockedState";

		/// <summary>
		/// Cached name for the 'OnRelease' method.
		/// </summary>
		public new static readonly StringName OnRelease = "OnRelease";

		/// <summary>
		/// Cached name for the 'OnFocus' method.
		/// </summary>
		public new static readonly StringName OnFocus = "OnFocus";

		/// <summary>
		/// Cached name for the 'OnUnfocus' method.
		/// </summary>
		public new static readonly StringName OnUnfocus = "OnUnfocus";

		/// <summary>
		/// Cached name for the 'OnPress' method.
		/// </summary>
		public new static readonly StringName OnPress = "OnPress";

		/// <summary>
		/// Cached name for the 'Deselect' method.
		/// </summary>
		public static readonly StringName Deselect = "Deselect";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : NButton.PropertyName
	{
		/// <summary>
		/// Cached name for the 'Total' property.
		/// </summary>
		public static readonly StringName Total = "Total";

		/// <summary>
		/// Cached name for the 'WinRateValue' property.
		/// </summary>
		public static readonly StringName WinRateValue = "WinRateValue";

		/// <summary>
		/// Cached name for the 'WinRate' property.
		/// </summary>
		public static readonly StringName WinRate = "WinRate";

		/// <summary>
		/// Cached name for the 'BestiarySeenQuote' property.
		/// </summary>
		public static readonly StringName BestiarySeenQuote = "BestiarySeenQuote";

		/// <summary>
		/// Cached name for the 'IsSelected' property.
		/// </summary>
		public static readonly StringName IsSelected = "IsSelected";

		/// <summary>
		/// Cached name for the 'IsLocked' property.
		/// </summary>
		public static readonly StringName IsLocked = "IsLocked";

		/// <summary>
		/// Cached name for the '_isSelected' field.
		/// </summary>
		public static readonly StringName _isSelected = "_isSelected";

		/// <summary>
		/// Cached name for the '_isLocked' field.
		/// </summary>
		public static readonly StringName _isLocked = "_isLocked";

		/// <summary>
		/// Cached name for the '_image' field.
		/// </summary>
		public static readonly StringName _image = "_image";

		/// <summary>
		/// Cached name for the '_hsv' field.
		/// </summary>
		public static readonly StringName _hsv = "_hsv";

		/// <summary>
		/// Cached name for the '_controllerSelectionReticle' field.
		/// </summary>
		public static readonly StringName _controllerSelectionReticle = "_controllerSelectionReticle";

		/// <summary>
		/// Cached name for the '_tween' field.
		/// </summary>
		public static readonly StringName _tween = "_tween";

		/// <summary>
		/// Cached name for the 'kills' field.
		/// </summary>
		public static readonly StringName kills = "kills";

		/// <summary>
		/// Cached name for the 'deaths' field.
		/// </summary>
		public static readonly StringName deaths = "deaths";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : NButton.SignalName
	{
		/// <summary>
		/// Cached name for the 'Toggled' signal.
		/// </summary>
		public static readonly StringName Toggled = "Toggled";
	}

	private static readonly StringName _v = new StringName("v");

	private static readonly StringName _s = new StringName("s");

	private static readonly string _scenePath = SceneHelper.GetScenePath("screens/bestiary/bestiary_character_filter");

	private bool _isSelected;

	private bool _isLocked;

	public CharacterModel? character;

	private TextureRect _image;

	private ShaderMaterial _hsv;

	private NSelectionReticle _controllerSelectionReticle;

	private Tween? _tween;

	private const float _focusedMultiplier = 1.2f;

	private const float _pressDownMultiplier = 0.8f;

	private static readonly Vector2 _enabledScale = Vector2.One * 1.2f;

	private static readonly Vector2 _disabledScale = Vector2.One * 0.95f;

	public int kills;

	public int deaths;

	private ToggledEventHandler backing_Toggled;

	public int Total => kills + deaths;

	private double WinRateValue
	{
		get
		{
			if (Total <= 0)
			{
				return 0.0;
			}
			return (double)kills / (double)Total * 100.0;
		}
	}

	public string WinRate
	{
		get
		{
			if (WinRateValue % 1.0 == 0.0)
			{
				return $"{WinRateValue:F0}";
			}
			return $"{WinRateValue:F1}";
		}
	}

	public string BestiarySeenQuote
	{
		get
		{
			if (character != null)
			{
				return character.BestiarySeenQuote.GetFormattedText();
			}
			return string.Empty;
		}
	}

	public LocString? BestiaryKillQuote => character?.BestiaryKillQuote;

	public bool IsSelected
	{
		get
		{
			return _isSelected;
		}
		set
		{
			_isSelected = value;
			OnToggle();
		}
	}

	public bool IsLocked
	{
		get
		{
			return _isLocked;
		}
		set
		{
			_isLocked = value;
			SetLockedState();
		}
	}

	/// <inheritdoc cref="T:MegaCrit.Sts2.Core.Nodes.Screens.Bestiary.NBestiaryCharacterFilter.ToggledEventHandler" />
	public event ToggledEventHandler Toggled
	{
		add
		{
			backing_Toggled = (ToggledEventHandler)Delegate.Combine(backing_Toggled, value);
		}
		remove
		{
			backing_Toggled = (ToggledEventHandler)Delegate.Remove(backing_Toggled, value);
		}
	}

	/// <summary>
	/// If a null character is passed, then this filter is set to "All Characters"
	/// </summary>
	public static NBestiaryCharacterFilter Create(CharacterModel? character)
	{
		NBestiaryCharacterFilter nBestiaryCharacterFilter = PreloadManager.Cache.GetAsset<PackedScene>(_scenePath).Instantiate<NBestiaryCharacterFilter>(PackedScene.GenEditState.Disabled);
		nBestiaryCharacterFilter.character = character;
		return nBestiaryCharacterFilter;
	}

	public override void _Ready()
	{
		ConnectSignals();
		_image = GetNode<TextureRect>("%Image");
		if (character != null)
		{
			_image.Texture = character.IconTexture;
			GetNode<TextureRect>("%Shadow").Texture = character.IconTexture;
		}
		_controllerSelectionReticle = GetNode<NSelectionReticle>("%SelectionReticle");
		_hsv = (ShaderMaterial)_image.GetMaterial();
	}

	private void OnToggle()
	{
		_tween?.Kill();
		_hsv.SetShaderParameter(_s, _isSelected ? 1.1f : 0.5f);
		_hsv.SetShaderParameter(_v, _isSelected ? 1.1f : 0.75f);
		if (!_isSelected)
		{
			_tween = CreateTween().SetParallel();
			_tween.TweenProperty(_image, "scale", _disabledScale, 0.3).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		}
		else
		{
			_tween = CreateTween().SetParallel();
			_tween.TweenProperty(_image, "scale", _enabledScale, 0.2).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);
		}
	}

	private void SetLockedState()
	{
		_image.SelfModulate = (_isLocked ? new Color(1f, 1f, 1f, 0.2f) : Colors.White);
	}

	protected override void OnRelease()
	{
		if (!_isSelected && !_isLocked)
		{
			base.OnRelease();
			IsSelected = !IsSelected;
			EmitSignal(SignalName.Toggled, this);
		}
	}

	protected override void OnFocus()
	{
		if (!_isSelected && !_isLocked)
		{
			base.OnFocus();
			_tween?.Kill();
			_tween = CreateTween().SetParallel();
			_tween.TweenProperty(_image, "scale", (_isSelected ? _enabledScale : _disabledScale) * 1.2f, 0.05);
			if (NControllerManager.Instance.IsUsingDirectionalNavigation)
			{
				_controllerSelectionReticle.OnSelect();
			}
		}
	}

	protected override void OnUnfocus()
	{
		if (!_isSelected && !_isLocked)
		{
			base.OnUnfocus();
			_tween?.Kill();
			_tween = CreateTween().SetParallel();
			_tween.TweenProperty(_image, "scale", _isSelected ? _enabledScale : _disabledScale, 0.3);
			_controllerSelectionReticle.OnDeselect();
		}
	}

	protected override void OnPress()
	{
		if (!_isSelected && !_isLocked)
		{
			base.OnPress();
			_tween?.Kill();
			_tween = CreateTween().SetParallel();
			_tween.TweenProperty(_image, "scale", (_isSelected ? _enabledScale : _disabledScale) * 0.8f, 0.3).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		}
	}

	public void Deselect()
	{
		IsSelected = false;
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(8);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnToggle, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetLockedState, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnRelease, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnfocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnPress, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Deselect, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.OnToggle && args.Count == 0)
		{
			OnToggle();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetLockedState && args.Count == 0)
		{
			SetLockedState();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnRelease && args.Count == 0)
		{
			OnRelease();
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
		if (method == MethodName.OnPress && args.Count == 0)
		{
			OnPress();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Deselect && args.Count == 0)
		{
			Deselect();
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
		if (method == MethodName.OnToggle)
		{
			return true;
		}
		if (method == MethodName.SetLockedState)
		{
			return true;
		}
		if (method == MethodName.OnRelease)
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
		if (method == MethodName.OnPress)
		{
			return true;
		}
		if (method == MethodName.Deselect)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.IsSelected)
		{
			IsSelected = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName.IsLocked)
		{
			IsLocked = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._isSelected)
		{
			_isSelected = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._isLocked)
		{
			_isLocked = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._image)
		{
			_image = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._hsv)
		{
			_hsv = VariantUtils.ConvertTo<ShaderMaterial>(in value);
			return true;
		}
		if (name == PropertyName._controllerSelectionReticle)
		{
			_controllerSelectionReticle = VariantUtils.ConvertTo<NSelectionReticle>(in value);
			return true;
		}
		if (name == PropertyName._tween)
		{
			_tween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName.kills)
		{
			kills = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName.deaths)
		{
			deaths = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.Total)
		{
			int from = Total;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.WinRateValue)
		{
			double from2 = WinRateValue;
			value = VariantUtils.CreateFrom(in from2);
			return true;
		}
		string from3;
		if (name == PropertyName.WinRate)
		{
			from3 = WinRate;
			value = VariantUtils.CreateFrom(in from3);
			return true;
		}
		if (name == PropertyName.BestiarySeenQuote)
		{
			from3 = BestiarySeenQuote;
			value = VariantUtils.CreateFrom(in from3);
			return true;
		}
		bool from4;
		if (name == PropertyName.IsSelected)
		{
			from4 = IsSelected;
			value = VariantUtils.CreateFrom(in from4);
			return true;
		}
		if (name == PropertyName.IsLocked)
		{
			from4 = IsLocked;
			value = VariantUtils.CreateFrom(in from4);
			return true;
		}
		if (name == PropertyName._isSelected)
		{
			value = VariantUtils.CreateFrom(in _isSelected);
			return true;
		}
		if (name == PropertyName._isLocked)
		{
			value = VariantUtils.CreateFrom(in _isLocked);
			return true;
		}
		if (name == PropertyName._image)
		{
			value = VariantUtils.CreateFrom(in _image);
			return true;
		}
		if (name == PropertyName._hsv)
		{
			value = VariantUtils.CreateFrom(in _hsv);
			return true;
		}
		if (name == PropertyName._controllerSelectionReticle)
		{
			value = VariantUtils.CreateFrom(in _controllerSelectionReticle);
			return true;
		}
		if (name == PropertyName._tween)
		{
			value = VariantUtils.CreateFrom(in _tween);
			return true;
		}
		if (name == PropertyName.kills)
		{
			value = VariantUtils.CreateFrom(in kills);
			return true;
		}
		if (name == PropertyName.deaths)
		{
			value = VariantUtils.CreateFrom(in deaths);
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
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isSelected, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isLocked, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._image, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hsv, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._controllerSelectionReticle, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName.kills, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName.deaths, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName.Total, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName.WinRateValue, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.String, PropertyName.WinRate, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.String, PropertyName.BestiarySeenQuote, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.IsSelected, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.IsLocked, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		StringName isSelected = PropertyName.IsSelected;
		bool from = IsSelected;
		info.AddProperty(isSelected, Variant.From(in from));
		StringName isLocked = PropertyName.IsLocked;
		from = IsLocked;
		info.AddProperty(isLocked, Variant.From(in from));
		info.AddProperty(PropertyName._isSelected, Variant.From(in _isSelected));
		info.AddProperty(PropertyName._isLocked, Variant.From(in _isLocked));
		info.AddProperty(PropertyName._image, Variant.From(in _image));
		info.AddProperty(PropertyName._hsv, Variant.From(in _hsv));
		info.AddProperty(PropertyName._controllerSelectionReticle, Variant.From(in _controllerSelectionReticle));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
		info.AddProperty(PropertyName.kills, Variant.From(in kills));
		info.AddProperty(PropertyName.deaths, Variant.From(in deaths));
		info.AddSignalEventDelegate(SignalName.Toggled, backing_Toggled);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.IsSelected, out var value))
		{
			IsSelected = value.As<bool>();
		}
		if (info.TryGetProperty(PropertyName.IsLocked, out var value2))
		{
			IsLocked = value2.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._isSelected, out var value3))
		{
			_isSelected = value3.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._isLocked, out var value4))
		{
			_isLocked = value4.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._image, out var value5))
		{
			_image = value5.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._hsv, out var value6))
		{
			_hsv = value6.As<ShaderMaterial>();
		}
		if (info.TryGetProperty(PropertyName._controllerSelectionReticle, out var value7))
		{
			_controllerSelectionReticle = value7.As<NSelectionReticle>();
		}
		if (info.TryGetProperty(PropertyName._tween, out var value8))
		{
			_tween = value8.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName.kills, out var value9))
		{
			kills = value9.As<int>();
		}
		if (info.TryGetProperty(PropertyName.deaths, out var value10))
		{
			deaths = value10.As<int>();
		}
		if (info.TryGetSignalEventDelegate<ToggledEventHandler>(SignalName.Toggled, out var value11))
		{
			backing_Toggled = value11;
		}
	}

	/// <summary>
	/// Get the signal information for all the signals declared in this class.
	/// This method is used by Godot to register the available signals in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotSignalList()
	{
		List<MethodInfo> list = new List<MethodInfo>(1);
		list.Add(new MethodInfo(SignalName.Toggled, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "filter", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		return list;
	}

	protected void EmitSignalToggled(NBestiaryCharacterFilter filter)
	{
		EmitSignal(SignalName.Toggled, filter);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RaiseGodotClassSignalCallbacks(in godot_string_name signal, NativeVariantPtrArgs args)
	{
		if (signal == SignalName.Toggled && args.Count == 1)
		{
			backing_Toggled?.Invoke(VariantUtils.ConvertTo<NBestiaryCharacterFilter>(in args[0]));
		}
		else
		{
			base.RaiseGodotClassSignalCallbacks(in signal, args);
		}
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassSignal(in godot_string_name signal)
	{
		if (signal == SignalName.Toggled)
		{
			return true;
		}
		return base.HasGodotClassSignal(in signal);
	}
}

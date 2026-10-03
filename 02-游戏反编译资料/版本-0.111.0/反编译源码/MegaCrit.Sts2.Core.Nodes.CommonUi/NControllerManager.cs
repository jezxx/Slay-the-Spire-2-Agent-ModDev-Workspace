using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.ControllerInput.ControllerConfigs;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.CommonUi;

[ScriptPath("res://src/Core/Nodes/CommonUi/NControllerManager.cs")]
public class NControllerManager : Node
{
	[Signal]
	public delegate void ControllerDetectedEventHandler();

	[Signal]
	public delegate void MouseDetectedEventHandler();

	/// <summary>
	/// Fires when we detect that the controller type has changed (ie xbox to ps4).
	/// </summary>
	[Signal]
	public delegate void ControllerTypeChangedEventHandler();

	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : Node.MethodName
	{
		/// <summary>
		/// Cached name for the '_ExitTree' method.
		/// </summary>
		public new static readonly StringName _ExitTree = "_ExitTree";

		/// <summary>
		/// Cached name for the '_Process' method.
		/// </summary>
		public new static readonly StringName _Process = "_Process";

		/// <summary>
		/// Cached name for the '_Input' method.
		/// </summary>
		public new static readonly StringName _Input = "_Input";

		/// <summary>
		/// Cached name for the 'OnControllerTypeChanged' method.
		/// </summary>
		public static readonly StringName OnControllerTypeChanged = "OnControllerTypeChanged";

		/// <summary>
		/// Cached name for the 'CheckForMouseInput' method.
		/// </summary>
		public static readonly StringName CheckForMouseInput = "CheckForMouseInput";

		/// <summary>
		/// Cached name for the 'CheckForControllerInput' method.
		/// </summary>
		public static readonly StringName CheckForControllerInput = "CheckForControllerInput";

		/// <summary>
		/// Cached name for the 'CheckForArrowKeyInput' method.
		/// </summary>
		public static readonly StringName CheckForArrowKeyInput = "CheckForArrowKeyInput";

		/// <summary>
		/// Cached name for the 'ForceMouseMode' method.
		/// </summary>
		public static readonly StringName ForceMouseMode = "ForceMouseMode";

		/// <summary>
		/// Cached name for the 'SwitchToMouseMode' method.
		/// </summary>
		public static readonly StringName SwitchToMouseMode = "SwitchToMouseMode";

		/// <summary>
		/// Cached name for the 'ControlModeChanged' method.
		/// </summary>
		public static readonly StringName ControlModeChanged = "ControlModeChanged";

		/// <summary>
		/// Cached name for the 'OnScreenContextChanged' method.
		/// </summary>
		public static readonly StringName OnScreenContextChanged = "OnScreenContextChanged";

		/// <summary>
		/// Cached name for the 'StartListeningForRebind' method.
		/// </summary>
		public static readonly StringName StartListeningForRebind = "StartListeningForRebind";

		/// <summary>
		/// Cached name for the 'StopListeningForRebind' method.
		/// </summary>
		public static readonly StringName StopListeningForRebind = "StopListeningForRebind";

		/// <summary>
		/// Cached name for the 'GetHotkeyIcon' method.
		/// </summary>
		public static readonly StringName GetHotkeyIcon = "GetHotkeyIcon";

		/// <summary>
		/// Cached name for the 'GetLeftAnalogStickDirection' method.
		/// </summary>
		public static readonly StringName GetLeftAnalogStickDirection = "GetLeftAnalogStickDirection";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Node.PropertyName
	{
		/// <summary>
		/// Cached name for the 'ShouldAllowControllerRebinding' property.
		/// </summary>
		public static readonly StringName ShouldAllowControllerRebinding = "ShouldAllowControllerRebinding";

		/// <summary>
		/// Cached name for the 'ShouldShowInputGlyphs' property.
		/// </summary>
		public static readonly StringName ShouldShowInputGlyphs = "ShouldShowInputGlyphs";

		/// <summary>
		/// Cached name for the 'InputType' property.
		/// </summary>
		public static readonly StringName InputType = "InputType";

		/// <summary>
		/// Cached name for the 'IsUsingDirectionalNavigation' property.
		/// </summary>
		public static readonly StringName IsUsingDirectionalNavigation = "IsUsingDirectionalNavigation";

		/// <summary>
		/// Cached name for the 'ControllerMappingType' property.
		/// </summary>
		public static readonly StringName ControllerMappingType = "ControllerMappingType";

		/// <summary>
		/// Cached name for the '_lastMousePosition' field.
		/// </summary>
		public static readonly StringName _lastMousePosition = "_lastMousePosition";

		/// <summary>
		/// Cached name for the '_label' field.
		/// </summary>
		public static readonly StringName _label = "_label";

		/// <summary>
		/// Cached name for the '_notifyTween' field.
		/// </summary>
		public static readonly StringName _notifyTween = "_notifyTween";

		/// <summary>
		/// Cached name for the '_inputTypeCheckingDisabled' field.
		/// </summary>
		public static readonly StringName _inputTypeCheckingDisabled = "_inputTypeCheckingDisabled";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Node.SignalName
	{
		/// <summary>
		/// Cached name for the 'ControllerDetected' signal.
		/// </summary>
		public static readonly StringName ControllerDetected = "ControllerDetected";

		/// <summary>
		/// Cached name for the 'MouseDetected' signal.
		/// </summary>
		public static readonly StringName MouseDetected = "MouseDetected";

		/// <summary>
		/// Cached name for the 'ControllerTypeChanged' signal.
		/// </summary>
		public static readonly StringName ControllerTypeChanged = "ControllerTypeChanged";
	}

	private IControllerInputStrategy? _inputStrategy;

	/// <summary>
	/// The position we warp the mouse to when we switch to controller mode. This is so it no
	/// longer hovers over the last control it ws positioned at
	/// </summary>
	private static readonly Vector2 _offscreenPos = Vector2.One * -1000f;

	/// <summary>
	/// Used to reset the mouse position to the last place it was before we swapped to controller mode
	/// </summary>
	private Vector2 _lastMousePosition;

	/// <summary>
	/// Minimum relative displacement (squared) to consider a mouse motion event as a warp artifact
	/// rather than real user input. No human mouse movement covers 500+ pixels in a single frame.
	/// </summary>
	private const float _warpDisplacementThresholdSq = 250000f;

	private MegaLabel _label;

	private Tween? _notifyTween;

	/// <summary>
	/// Make sure you know what you are doing when using this.
	/// used to disable switching between InputTypes while we are listening for inputs to rebind them
	/// </summary>
	private bool _inputTypeCheckingDisabled;

	private ControllerDetectedEventHandler backing_ControllerDetected;

	private MouseDetectedEventHandler backing_MouseDetected;

	private ControllerTypeChangedEventHandler backing_ControllerTypeChanged;

	public static NControllerManager? Instance
	{
		get
		{
			if (NGame.Instance == null)
			{
				return null;
			}
			return NGame.Instance.InputManager.ControllerManager;
		}
	}

	public bool ShouldAllowControllerRebinding => _inputStrategy?.ShouldAllowControllerRebinding ?? true;

	public bool ShouldShowInputGlyphs
	{
		get
		{
			InputType inputType = InputType;
			if ((uint)(inputType - 1) <= 1u)
			{
				return true;
			}
			return false;
		}
	}

	public InputType InputType { get; private set; }

	public bool IsUsingDirectionalNavigation
	{
		get
		{
			InputType inputType = InputType;
			if ((uint)(inputType - 1) <= 1u)
			{
				return true;
			}
			return false;
		}
	}

	public Dictionary<StringName, StringName> GetDefaultControllerInputMap
	{
		get
		{
			if (_inputStrategy == null)
			{
				return new SteamControllerConfig().DefaultControllerInputMap;
			}
			return _inputStrategy.GetDefaultControllerInputMap;
		}
	}

	public ControllerMappingType ControllerMappingType
	{
		get
		{
			if (_inputStrategy == null)
			{
				return ControllerMappingType.Default;
			}
			return _inputStrategy.ControllerConfig.ControllerMappingType;
		}
	}

	/// <inheritdoc cref="T:MegaCrit.Sts2.Core.Nodes.CommonUi.NControllerManager.ControllerDetectedEventHandler" />
	public event ControllerDetectedEventHandler ControllerDetected
	{
		add
		{
			backing_ControllerDetected = (ControllerDetectedEventHandler)Delegate.Combine(backing_ControllerDetected, value);
		}
		remove
		{
			backing_ControllerDetected = (ControllerDetectedEventHandler)Delegate.Remove(backing_ControllerDetected, value);
		}
	}

	/// <inheritdoc cref="T:MegaCrit.Sts2.Core.Nodes.CommonUi.NControllerManager.MouseDetectedEventHandler" />
	public event MouseDetectedEventHandler MouseDetected
	{
		add
		{
			backing_MouseDetected = (MouseDetectedEventHandler)Delegate.Combine(backing_MouseDetected, value);
		}
		remove
		{
			backing_MouseDetected = (MouseDetectedEventHandler)Delegate.Remove(backing_MouseDetected, value);
		}
	}

	/// <inheritdoc cref="T:MegaCrit.Sts2.Core.Nodes.CommonUi.NControllerManager.ControllerTypeChangedEventHandler" />
	public event ControllerTypeChangedEventHandler ControllerTypeChanged
	{
		add
		{
			backing_ControllerTypeChanged = (ControllerTypeChangedEventHandler)Delegate.Combine(backing_ControllerTypeChanged, value);
		}
		remove
		{
			backing_ControllerTypeChanged = (ControllerTypeChangedEventHandler)Delegate.Remove(backing_ControllerTypeChanged, value);
		}
	}

	public async Task Init()
	{
		ActiveScreenContext.Instance.Updated += OnScreenContextChanged;
		_label = GetNode<MegaLabel>("Label");
		_label.Modulate = Colors.Transparent;
		_inputStrategy = new SteamControllerInputStrategy();
		await _inputStrategy.Init();
	}

	public override void _ExitTree()
	{
		ActiveScreenContext.Instance.Updated -= OnScreenContextChanged;
	}

	public override void _Process(double delta)
	{
		if (NGame.IsGameFocusedWindow())
		{
			_inputStrategy?.ProcessInput();
		}
	}

	public override void _Input(InputEvent inputEvent)
	{
		if (!_inputTypeCheckingDisabled)
		{
			if (InputType != InputType.Controller)
			{
				CheckForControllerInput(inputEvent);
			}
			if (InputType != 0)
			{
				CheckForMouseInput(inputEvent);
			}
			if (InputType != InputType.KeyboardOnlyMode)
			{
				CheckForArrowKeyInput(inputEvent);
			}
		}
	}

	public void OnControllerTypeChanged()
	{
		EmitSignalControllerTypeChanged();
	}

	/// <summary>
	/// Checks if the input event is from a mouse and notifies the ui that we are now using mouse input
	/// </summary>
	/// <param name="inputEvent"></param>
	private void CheckForMouseInput(InputEvent inputEvent)
	{
		bool flag = inputEvent is InputEventMouseButton;
		bool flag2 = inputEvent is InputEventMouseMotion { Velocity: var velocity } inputEventMouseMotion && velocity.LengthSquared() > 100f && inputEventMouseMotion.Relative.LengthSquared() <= 250000f;
		if (flag || flag2)
		{
			SwitchToMouseMode();
		}
	}

	/// <summary>
	/// Checks if the input event is from a controller and notifies the ui that we are now using controller input
	/// </summary>
	/// <param name="inputEvent"></param>
	private void CheckForControllerInput(InputEvent inputEvent)
	{
		InputEvent inputEvent2 = inputEvent;
		if (NGame.IsGameFocusedWindow() && Controller.AllControllerInputs.Any((StringName i) => inputEvent2.IsActionPressed(i)))
		{
			InputType = InputType.Controller;
			Viewport viewport = GetViewport();
			NGame.Instance?.SetMouseBehaviorRecursive(Control.MouseBehaviorRecursiveEnum.Disabled);
			ActiveScreenContext.Instance.FocusOnDefaultControl();
			EmitSignal(SignalName.ControllerDetected);
			ControlModeChanged();
			viewport?.SetInputAsHandled();
		}
	}

	private void CheckForArrowKeyInput(InputEvent inputEvent)
	{
		if (NGame.IsGameFocusedWindow() && inputEvent is InputEventKey inputEventKey && inputEventKey.IsPressed() && (inputEventKey.Keycode == Key.Up || inputEventKey.Keycode == Key.Down || inputEventKey.Keycode == Key.Left || inputEventKey.Keycode == Key.Right))
		{
			Viewport viewport = GetViewport();
			if (SaveManager.Instance.PrefsSave.KeyboardMode)
			{
				InputType = InputType.KeyboardOnlyMode;
				NGame.Instance?.SetMouseBehaviorRecursive(Control.MouseBehaviorRecursiveEnum.Disabled);
				ActiveScreenContext.Instance.FocusOnDefaultControl();
				EmitSignal(SignalName.ControllerDetected);
				ControlModeChanged();
				viewport?.SetInputAsHandled();
			}
			else if (InputType == InputType.Controller)
			{
				SwitchToMouseMode();
			}
		}
	}

	/// <summary>
	/// WARNING: Normally this should be handled by CheckForMouseInput.
	/// Make sure you know what you are doing if you use this.
	/// </summary>
	public void ForceMouseMode()
	{
		SwitchToMouseMode();
	}

	private void SwitchToMouseMode()
	{
		Viewport viewport = GetViewport();
		InputType = InputType.MouseAndKeyboard;
		viewport?.GuiReleaseFocus();
		NGame.Instance?.SetMouseBehaviorRecursive(Control.MouseBehaviorRecursiveEnum.Inherited);
		EmitSignal(SignalName.MouseDetected);
		ControlModeChanged();
	}

	private void ControlModeChanged()
	{
		_notifyTween?.Kill();
		_notifyTween = CreateTween();
		_notifyTween.TweenProperty(_label, "modulate", Colors.White, 0.25);
		_notifyTween.TweenInterval(0.5);
		_notifyTween.TweenProperty(_label, "modulate", Colors.Transparent, 0.75);
		switch (InputType)
		{
		case InputType.Controller:
			_label.SetTextAutoSize(new LocString("main_menu_ui", "CONTROLLER_DETECTED").GetFormattedText());
			Log.Info("CONTROLLER DETECTED: " + ((_inputStrategy != null) ? _inputStrategy.GetControllerName() : "NONE"));
			break;
		case InputType.MouseAndKeyboard:
			_label.SetTextAutoSize(new LocString("main_menu_ui", "MOUSE_DETECTED").GetFormattedText());
			Log.Info("MOUSE DETECTED");
			break;
		case InputType.KeyboardOnlyMode:
			_label.SetTextAutoSize(new LocString("main_menu_ui", "KEYBOARD_ONLY_DETECTED").GetFormattedText());
			Log.Info("KEYBOARD-MODE DETECTED");
			break;
		}
	}

	private void OnScreenContextChanged()
	{
		if (IsUsingDirectionalNavigation)
		{
			Callable.From(delegate
			{
				ActiveScreenContext.Instance.FocusOnDefaultControl();
			}).CallDeferred();
			return;
		}
		Vector2 mousePosition = GetViewport().GetMousePosition();
		using InputEventMouseMotion inputEventMouseMotion = new InputEventMouseMotion();
		inputEventMouseMotion.Position = mousePosition;
		inputEventMouseMotion.GlobalPosition = mousePosition;
		Input.ParseInputEvent(inputEventMouseMotion);
	}

	public void StartListeningForRebind()
	{
		_inputTypeCheckingDisabled = true;
	}

	public void StopListeningForRebind()
	{
		_inputTypeCheckingDisabled = false;
	}

	public Texture2D? GetHotkeyIcon(string hotkey)
	{
		return _inputStrategy?.GetHotkeyIcon(hotkey);
	}

	public Vector2 GetLeftAnalogStickDirection()
	{
		return _inputStrategy?.GetLeftAnalogStickDirection() ?? Vector2.Zero;
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(15);
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Process, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Input, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnControllerTypeChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.CheckForMouseInput, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.CheckForControllerInput, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.CheckForArrowKeyInput, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ForceMouseMode, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SwitchToMouseMode, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ControlModeChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnScreenContextChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.StartListeningForRebind, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.StopListeningForRebind, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.GetHotkeyIcon, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Texture2D"), exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "hotkey", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.GetLeftAnalogStickDirection, new PropertyInfo(Variant.Type.Vector2, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Process && args.Count == 1)
		{
			_Process(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Input && args.Count == 1)
		{
			_Input(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnControllerTypeChanged && args.Count == 0)
		{
			OnControllerTypeChanged();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CheckForMouseInput && args.Count == 1)
		{
			CheckForMouseInput(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CheckForControllerInput && args.Count == 1)
		{
			CheckForControllerInput(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CheckForArrowKeyInput && args.Count == 1)
		{
			CheckForArrowKeyInput(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ForceMouseMode && args.Count == 0)
		{
			ForceMouseMode();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SwitchToMouseMode && args.Count == 0)
		{
			SwitchToMouseMode();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ControlModeChanged && args.Count == 0)
		{
			ControlModeChanged();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnScreenContextChanged && args.Count == 0)
		{
			OnScreenContextChanged();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.StartListeningForRebind && args.Count == 0)
		{
			StartListeningForRebind();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.StopListeningForRebind && args.Count == 0)
		{
			StopListeningForRebind();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.GetHotkeyIcon && args.Count == 1)
		{
			Texture2D from = GetHotkeyIcon(VariantUtils.ConvertTo<string>(in args[0]));
			ret = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (method == MethodName.GetLeftAnalogStickDirection && args.Count == 0)
		{
			Vector2 from2 = GetLeftAnalogStickDirection();
			ret = VariantUtils.CreateFrom(in from2);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName._Process)
		{
			return true;
		}
		if (method == MethodName._Input)
		{
			return true;
		}
		if (method == MethodName.OnControllerTypeChanged)
		{
			return true;
		}
		if (method == MethodName.CheckForMouseInput)
		{
			return true;
		}
		if (method == MethodName.CheckForControllerInput)
		{
			return true;
		}
		if (method == MethodName.CheckForArrowKeyInput)
		{
			return true;
		}
		if (method == MethodName.ForceMouseMode)
		{
			return true;
		}
		if (method == MethodName.SwitchToMouseMode)
		{
			return true;
		}
		if (method == MethodName.ControlModeChanged)
		{
			return true;
		}
		if (method == MethodName.OnScreenContextChanged)
		{
			return true;
		}
		if (method == MethodName.StartListeningForRebind)
		{
			return true;
		}
		if (method == MethodName.StopListeningForRebind)
		{
			return true;
		}
		if (method == MethodName.GetHotkeyIcon)
		{
			return true;
		}
		if (method == MethodName.GetLeftAnalogStickDirection)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.InputType)
		{
			InputType = VariantUtils.ConvertTo<InputType>(in value);
			return true;
		}
		if (name == PropertyName._lastMousePosition)
		{
			_lastMousePosition = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._label)
		{
			_label = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._notifyTween)
		{
			_notifyTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._inputTypeCheckingDisabled)
		{
			_inputTypeCheckingDisabled = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		bool from;
		if (name == PropertyName.ShouldAllowControllerRebinding)
		{
			from = ShouldAllowControllerRebinding;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.ShouldShowInputGlyphs)
		{
			from = ShouldShowInputGlyphs;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.InputType)
		{
			InputType from2 = InputType;
			value = VariantUtils.CreateFrom(in from2);
			return true;
		}
		if (name == PropertyName.IsUsingDirectionalNavigation)
		{
			from = IsUsingDirectionalNavigation;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.ControllerMappingType)
		{
			ControllerMappingType from3 = ControllerMappingType;
			value = VariantUtils.CreateFrom(in from3);
			return true;
		}
		if (name == PropertyName._lastMousePosition)
		{
			value = VariantUtils.CreateFrom(in _lastMousePosition);
			return true;
		}
		if (name == PropertyName._label)
		{
			value = VariantUtils.CreateFrom(in _label);
			return true;
		}
		if (name == PropertyName._notifyTween)
		{
			value = VariantUtils.CreateFrom(in _notifyTween);
			return true;
		}
		if (name == PropertyName._inputTypeCheckingDisabled)
		{
			value = VariantUtils.CreateFrom(in _inputTypeCheckingDisabled);
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
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.ShouldAllowControllerRebinding, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.ShouldShowInputGlyphs, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._lastMousePosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._label, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._notifyTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._inputTypeCheckingDisabled, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName.InputType, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.IsUsingDirectionalNavigation, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName.ControllerMappingType, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		StringName inputType = PropertyName.InputType;
		InputType from = InputType;
		info.AddProperty(inputType, Variant.From(in from));
		info.AddProperty(PropertyName._lastMousePosition, Variant.From(in _lastMousePosition));
		info.AddProperty(PropertyName._label, Variant.From(in _label));
		info.AddProperty(PropertyName._notifyTween, Variant.From(in _notifyTween));
		info.AddProperty(PropertyName._inputTypeCheckingDisabled, Variant.From(in _inputTypeCheckingDisabled));
		info.AddSignalEventDelegate(SignalName.ControllerDetected, backing_ControllerDetected);
		info.AddSignalEventDelegate(SignalName.MouseDetected, backing_MouseDetected);
		info.AddSignalEventDelegate(SignalName.ControllerTypeChanged, backing_ControllerTypeChanged);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.InputType, out var value))
		{
			InputType = value.As<InputType>();
		}
		if (info.TryGetProperty(PropertyName._lastMousePosition, out var value2))
		{
			_lastMousePosition = value2.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._label, out var value3))
		{
			_label = value3.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._notifyTween, out var value4))
		{
			_notifyTween = value4.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._inputTypeCheckingDisabled, out var value5))
		{
			_inputTypeCheckingDisabled = value5.As<bool>();
		}
		if (info.TryGetSignalEventDelegate<ControllerDetectedEventHandler>(SignalName.ControllerDetected, out var value6))
		{
			backing_ControllerDetected = value6;
		}
		if (info.TryGetSignalEventDelegate<MouseDetectedEventHandler>(SignalName.MouseDetected, out var value7))
		{
			backing_MouseDetected = value7;
		}
		if (info.TryGetSignalEventDelegate<ControllerTypeChangedEventHandler>(SignalName.ControllerTypeChanged, out var value8))
		{
			backing_ControllerTypeChanged = value8;
		}
	}

	/// <summary>
	/// Get the signal information for all the signals declared in this class.
	/// This method is used by Godot to register the available signals in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotSignalList()
	{
		List<MethodInfo> list = new List<MethodInfo>(3);
		list.Add(new MethodInfo(SignalName.ControllerDetected, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(SignalName.MouseDetected, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(SignalName.ControllerTypeChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	protected void EmitSignalControllerDetected()
	{
		EmitSignal(SignalName.ControllerDetected);
	}

	protected void EmitSignalMouseDetected()
	{
		EmitSignal(SignalName.MouseDetected);
	}

	protected void EmitSignalControllerTypeChanged()
	{
		EmitSignal(SignalName.ControllerTypeChanged);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RaiseGodotClassSignalCallbacks(in godot_string_name signal, NativeVariantPtrArgs args)
	{
		if (signal == SignalName.ControllerDetected && args.Count == 0)
		{
			backing_ControllerDetected?.Invoke();
		}
		else if (signal == SignalName.MouseDetected && args.Count == 0)
		{
			backing_MouseDetected?.Invoke();
		}
		else if (signal == SignalName.ControllerTypeChanged && args.Count == 0)
		{
			backing_ControllerTypeChanged?.Invoke();
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
		if (signal == SignalName.ControllerDetected)
		{
			return true;
		}
		if (signal == SignalName.MouseDetected)
		{
			return true;
		}
		if (signal == SignalName.ControllerTypeChanged)
		{
			return true;
		}
		return base.HasGodotClassSignal(in signal);
	}
}

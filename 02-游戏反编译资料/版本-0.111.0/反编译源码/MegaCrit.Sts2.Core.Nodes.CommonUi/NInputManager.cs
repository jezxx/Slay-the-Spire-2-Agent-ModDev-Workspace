using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Debug;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Debug;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Saves;

namespace MegaCrit.Sts2.Core.Nodes.CommonUi;

/// <summary>
/// Listens for keyboard and controller inputs, and map them into input events that the UX
/// can listen for (for navigation or hotkeys)
/// That map can be edited to rebind different keyboard/controller inputs to different UX events
/// </summary>
[ScriptPath("res://src/Core/Nodes/CommonUi/NInputManager.cs")]
public class NInputManager : Node
{
	[Signal]
	public delegate void InputReboundEventHandler();

	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : Node.MethodName
	{
		/// <summary>
		/// Cached name for the '_EnterTree' method.
		/// </summary>
		public new static readonly StringName _EnterTree = "_EnterTree";

		/// <summary>
		/// Cached name for the '_Ready' method.
		/// </summary>
		public new static readonly StringName _Ready = "_Ready";

		/// <summary>
		/// Cached name for the '_UnhandledKeyInput' method.
		/// </summary>
		public new static readonly StringName _UnhandledKeyInput = "_UnhandledKeyInput";

		/// <summary>
		/// Cached name for the 'ProcessDebugKeyInput' method.
		/// </summary>
		public static readonly StringName ProcessDebugKeyInput = "ProcessDebugKeyInput";

		/// <summary>
		/// Cached name for the 'ProcessHotkeyInput' method.
		/// </summary>
		public static readonly StringName ProcessHotkeyInput = "ProcessHotkeyInput";

		/// <summary>
		/// Cached name for the 'ProcessFkbInput' method.
		/// </summary>
		public static readonly StringName ProcessFkbInput = "ProcessFkbInput";

		/// <summary>
		/// Cached name for the '_UnhandledInput' method.
		/// </summary>
		public new static readonly StringName _UnhandledInput = "_UnhandledInput";

		/// <summary>
		/// Cached name for the 'GetCurrentHotkey' method.
		/// </summary>
		public static readonly StringName GetCurrentHotkey = "GetCurrentHotkey";

		/// <summary>
		/// Cached name for the 'GetMKbHotkey' method.
		/// </summary>
		public static readonly StringName GetMKbHotkey = "GetMKbHotkey";

		/// <summary>
		/// Cached name for the 'GetKbOnlyHotkey' method.
		/// </summary>
		public static readonly StringName GetKbOnlyHotkey = "GetKbOnlyHotkey";

		/// <summary>
		/// Cached name for the 'GetHotkeyIcon' method.
		/// </summary>
		public static readonly StringName GetHotkeyIcon = "GetHotkeyIcon";

		/// <summary>
		/// Cached name for the 'ModifyMKbKey' method.
		/// </summary>
		public static readonly StringName ModifyMKbKey = "ModifyMKbKey";

		/// <summary>
		/// Cached name for the 'EnsureEndTurnAndConfirmMkbKeyAreTheSame' method.
		/// </summary>
		public static readonly StringName EnsureEndTurnAndConfirmMkbKeyAreTheSame = "EnsureEndTurnAndConfirmMkbKeyAreTheSame";

		/// <summary>
		/// Cached name for the 'ModifyKbOnlyKey' method.
		/// </summary>
		public static readonly StringName ModifyKbOnlyKey = "ModifyKbOnlyKey";

		/// <summary>
		/// Cached name for the 'ModifyControllerButton' method.
		/// </summary>
		public static readonly StringName ModifyControllerButton = "ModifyControllerButton";

		/// <summary>
		/// Cached name for the 'EnsureEndTurnAndConfirmControllerInputAreTheSame' method.
		/// </summary>
		public static readonly StringName EnsureEndTurnAndConfirmControllerInputAreTheSame = "EnsureEndTurnAndConfirmControllerInputAreTheSame";

		/// <summary>
		/// Cached name for the 'ResetToDefaults' method.
		/// </summary>
		public static readonly StringName ResetToDefaults = "ResetToDefaults";

		/// <summary>
		/// Cached name for the 'OnControllerTypeChanged' method.
		/// </summary>
		public static readonly StringName OnControllerTypeChanged = "OnControllerTypeChanged";

		/// <summary>
		/// Cached name for the 'SaveControllerInputMapping' method.
		/// </summary>
		public static readonly StringName SaveControllerInputMapping = "SaveControllerInputMapping";

		/// <summary>
		/// Cached name for the 'SaveMKbInputMapping' method.
		/// </summary>
		public static readonly StringName SaveMKbInputMapping = "SaveMKbInputMapping";

		/// <summary>
		/// Cached name for the 'SaveFKbInputMapping' method.
		/// </summary>
		public static readonly StringName SaveFKbInputMapping = "SaveFKbInputMapping";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Node.PropertyName
	{
		/// <summary>
		/// Cached name for the 'ControllerManager' property.
		/// </summary>
		public static readonly StringName ControllerManager = "ControllerManager";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Node.SignalName
	{
		/// <summary>
		/// Cached name for the 'InputRebound' signal.
		/// </summary>
		public static readonly StringName InputRebound = "InputRebound";
	}

	private readonly Dictionary<Key, StringName> _debugInputMap = new Dictionary<Key, StringName>
	{
		{
			Key.Key1,
			DebugHotkey.hideTopBar
		},
		{
			Key.Key2,
			DebugHotkey.hideIntents
		},
		{
			Key.Key3,
			DebugHotkey.hideCombatUi
		},
		{
			Key.Key4,
			DebugHotkey.hidePlayContainer
		},
		{
			Key.Key5,
			DebugHotkey.hideHand
		},
		{
			Key.Key6,
			DebugHotkey.hideHpBars
		},
		{
			Key.Key7,
			DebugHotkey.hideTextVfx
		},
		{
			Key.Key8,
			DebugHotkey.hideTargetingUi
		},
		{
			Key.Key9,
			DebugHotkey.slowRewards
		},
		{
			Key.Key0,
			DebugHotkey.hideVersionInfo
		},
		{
			Key.Minus,
			DebugHotkey.speedDown
		},
		{
			Key.Equal,
			DebugHotkey.speedUp
		},
		{
			Key.F1,
			DebugHotkey.hideRestSite
		},
		{
			Key.F3,
			DebugHotkey.hideEventUi
		},
		{
			Key.F4,
			DebugHotkey.hideProceedButton
		},
		{
			Key.F5,
			DebugHotkey.hideHoverTips
		},
		{
			Key.F6,
			DebugHotkey.hideMpCursors
		},
		{
			Key.F7,
			DebugHotkey.hideMpTargeting
		},
		{
			Key.F9,
			DebugHotkey.hideMpIntents
		},
		{
			Key.F10,
			DebugHotkey.hideMpHealthBars
		},
		{
			Key.U,
			DebugHotkey.unlockCharacters
		}
	};

	public static readonly IReadOnlyList<StringName> remappableMKbInputs = new List<StringName>
	{
		MegaInput.cancel,
		MegaInput.viewMap,
		MegaInput.viewDeckAndTabLeft,
		MegaInput.viewDrawPile,
		MegaInput.viewDiscardPile,
		MegaInput.viewExhaustPileAndTabRight,
		MegaInput.confirm,
		MegaInput.endTurn,
		MegaInput.peek,
		MegaInput.up,
		MegaInput.down,
		MegaInput.left,
		MegaInput.right,
		MegaInput.selectCard1,
		MegaInput.selectCard2,
		MegaInput.selectCard3,
		MegaInput.selectCard4,
		MegaInput.selectCard5,
		MegaInput.selectCard6,
		MegaInput.selectCard7,
		MegaInput.selectCard8,
		MegaInput.selectCard9,
		MegaInput.selectCard10
	};

	public static readonly IReadOnlyList<StringName> remappableKbOnlyInputs = new List<StringName>
	{
		MegaInput.select,
		MegaInput.cancel,
		MegaInput.viewMap,
		MegaInput.topPanel,
		MegaInput.viewDeckAndTabLeft,
		MegaInput.viewDrawPile,
		MegaInput.viewDiscardPile,
		MegaInput.viewExhaustPileAndTabRight,
		MegaInput.confirm,
		MegaInput.endTurn,
		MegaInput.peek,
		MegaInput.up,
		MegaInput.down,
		MegaInput.left,
		MegaInput.right,
		MegaInput.selectCard1,
		MegaInput.selectCard2,
		MegaInput.selectCard3,
		MegaInput.selectCard4,
		MegaInput.selectCard5,
		MegaInput.selectCard6,
		MegaInput.selectCard7,
		MegaInput.selectCard8,
		MegaInput.selectCard9,
		MegaInput.selectCard10
	};

	public static readonly IReadOnlyList<StringName> remappableControllerInputs = new List<StringName>
	{
		MegaInput.select,
		MegaInput.cancel,
		MegaInput.viewMap,
		MegaInput.topPanel,
		MegaInput.viewDeckAndTabLeft,
		MegaInput.viewDrawPile,
		MegaInput.viewDiscardPile,
		MegaInput.viewExhaustPileAndTabRight,
		MegaInput.confirm,
		MegaInput.endTurn,
		MegaInput.peek,
		MegaInput.up,
		MegaInput.down,
		MegaInput.left,
		MegaInput.right
	};

	private Dictionary<StringName, Key> _mKbInputMap = new Dictionary<StringName, Key>();

	private Dictionary<StringName, StringName> _controllerInputMap = new Dictionary<StringName, StringName>();

	private Dictionary<StringName, Key> _fKbInputMap = new Dictionary<StringName, Key>();

	private InputReboundEventHandler backing_InputRebound;

	public static NInputManager? Instance
	{
		get
		{
			if (NGame.Instance == null)
			{
				return null;
			}
			return NGame.Instance.InputManager;
		}
	}

	private static Dictionary<StringName, Key> DefaultHotkeyInputMap => new Dictionary<StringName, Key>
	{
		{
			MegaInput.endTurn,
			Key.E
		},
		{
			MegaInput.confirm,
			Key.E
		},
		{
			MegaInput.viewDiscardPile,
			Key.S
		},
		{
			MegaInput.viewDeckAndTabLeft,
			Key.D
		},
		{
			MegaInput.viewExhaustPileAndTabRight,
			Key.X
		},
		{
			MegaInput.viewDrawPile,
			Key.A
		},
		{
			MegaInput.viewMap,
			Key.M
		},
		{
			MegaInput.cancel,
			Key.Escape
		},
		{
			MegaInput.peek,
			Key.Space
		},
		{
			MegaInput.up,
			Key.Up
		},
		{
			MegaInput.down,
			Key.Down
		},
		{
			MegaInput.left,
			Key.Left
		},
		{
			MegaInput.right,
			Key.Right
		},
		{
			MegaInput.pauseAndBack,
			Key.Escape
		},
		{
			MegaInput.selectCard1,
			Key.Key1
		},
		{
			MegaInput.selectCard2,
			Key.Key2
		},
		{
			MegaInput.selectCard3,
			Key.Key3
		},
		{
			MegaInput.selectCard4,
			Key.Key4
		},
		{
			MegaInput.selectCard5,
			Key.Key5
		},
		{
			MegaInput.selectCard6,
			Key.Key6
		},
		{
			MegaInput.selectCard7,
			Key.Key7
		},
		{
			MegaInput.selectCard8,
			Key.Key8
		},
		{
			MegaInput.selectCard9,
			Key.Key9
		},
		{
			MegaInput.selectCard10,
			Key.Key0
		}
	};

	private static Dictionary<StringName, Key> DefaultKbOnlyInputMap => new Dictionary<StringName, Key>
	{
		{
			MegaInput.confirm,
			Key.Enter
		},
		{
			MegaInput.endTurn,
			Key.E
		},
		{
			MegaInput.select,
			Key.Space
		},
		{
			MegaInput.viewDiscardPile,
			Key.D
		},
		{
			MegaInput.viewDeckAndTabLeft,
			Key.Q
		},
		{
			MegaInput.viewExhaustPileAndTabRight,
			Key.F
		},
		{
			MegaInput.viewDrawPile,
			Key.A
		},
		{
			MegaInput.viewMap,
			Key.Tab
		},
		{
			MegaInput.cancel,
			Key.Escape
		},
		{
			MegaInput.peek,
			Key.S
		},
		{
			MegaInput.up,
			Key.Up
		},
		{
			MegaInput.down,
			Key.Down
		},
		{
			MegaInput.left,
			Key.Left
		},
		{
			MegaInput.right,
			Key.Right
		},
		{
			MegaInput.pauseAndBack,
			Key.Escape
		},
		{
			MegaInput.selectCard1,
			Key.Key1
		},
		{
			MegaInput.selectCard2,
			Key.Key2
		},
		{
			MegaInput.selectCard3,
			Key.Key3
		},
		{
			MegaInput.selectCard4,
			Key.Key4
		},
		{
			MegaInput.selectCard5,
			Key.Key5
		},
		{
			MegaInput.selectCard6,
			Key.Key6
		},
		{
			MegaInput.selectCard7,
			Key.Key7
		},
		{
			MegaInput.selectCard8,
			Key.Key8
		},
		{
			MegaInput.selectCard9,
			Key.Key9
		},
		{
			MegaInput.selectCard10,
			Key.Key0
		},
		{
			MegaInput.topPanel,
			Key.W
		}
	};

	public NControllerManager ControllerManager { get; private set; }

	/// <inheritdoc cref="T:MegaCrit.Sts2.Core.Nodes.CommonUi.NInputManager.InputReboundEventHandler" />
	public event InputReboundEventHandler InputRebound
	{
		add
		{
			backing_InputRebound = (InputReboundEventHandler)Delegate.Combine(backing_InputRebound, value);
		}
		remove
		{
			backing_InputRebound = (InputReboundEventHandler)Delegate.Remove(backing_InputRebound, value);
		}
	}

	public override void _EnterTree()
	{
		ControllerManager = GetNode<NControllerManager>("%ControllerManager");
	}

	public override void _Ready()
	{
		ControllerManager.Connect(NControllerManager.SignalName.ControllerTypeChanged, Callable.From(OnControllerTypeChanged));
		TaskHelper.RunSafely(Init());
	}

	private async Task Init()
	{
		await ControllerManager.Init();
		SettingsSave settingsSave = SaveManager.Instance.SettingsSave;
		if (settingsSave.KeyboardMapping.Count > 0)
		{
			Dictionary<StringName, Key> defaultHotkeyInputMap = DefaultHotkeyInputMap;
			_mKbInputMap = new Dictionary<StringName, Key>(defaultHotkeyInputMap);
			foreach (KeyValuePair<string, string> item in settingsSave.KeyboardMapping)
			{
				if (Enum.TryParse<Key>(item.Value, out var result))
				{
					_mKbInputMap[item.Key] = result;
				}
			}
		}
		else
		{
			_mKbInputMap = DefaultHotkeyInputMap;
			SaveMKbInputMapping();
		}
		if (settingsSave.KbOnlyMapping.Count > 0)
		{
			Dictionary<StringName, Key> defaultKbOnlyInputMap = DefaultKbOnlyInputMap;
			_fKbInputMap = new Dictionary<StringName, Key>(defaultKbOnlyInputMap);
			foreach (KeyValuePair<string, string> item2 in settingsSave.KbOnlyMapping)
			{
				if (Enum.TryParse<Key>(item2.Value, out var result2))
				{
					_fKbInputMap[item2.Key] = result2;
				}
			}
		}
		else
		{
			_fKbInputMap = DefaultKbOnlyInputMap;
			SaveFKbInputMapping();
		}
		if (settingsSave.ControllerMapping.Count > 0 && settingsSave.ControllerMappingType == ControllerManager.ControllerMappingType)
		{
			_controllerInputMap = MergeSavedControllerBindings(ControllerManager.GetDefaultControllerInputMap, settingsSave.ControllerMapping);
			return;
		}
		_controllerInputMap = ControllerManager.GetDefaultControllerInputMap;
		SaveControllerInputMapping();
	}

	/// <summary>
	/// Overlays a saved controller mapping onto the defaults, ignoring any saved binding whose
	/// value is not a registered InputMap action. A binding can point at a missing action when a
	/// save was migrated forward by a newer build and then loaded by an older one, or when a save
	/// was hand-edited. Applying it would make <see cref="M:MegaCrit.Sts2.Core.Nodes.CommonUi.NInputManager._UnhandledInput(Godot.InputEvent)" /> call
	/// <c>IsActionPressed</c> on a nonexistent action and spam errors, so the default is kept.
	/// </summary>
	public static Dictionary<StringName, StringName> MergeSavedControllerBindings(Dictionary<StringName, StringName> defaults, Dictionary<string, string> savedMapping)
	{
		Dictionary<StringName, StringName> dictionary = new Dictionary<StringName, StringName>(defaults);
		foreach (KeyValuePair<string, string> item in savedMapping)
		{
			if (InputMap.HasAction(item.Value))
			{
				dictionary[item.Key] = item.Value;
			}
		}
		return dictionary;
	}

	public override void _UnhandledKeyInput(InputEvent inputEvent)
	{
		if (ControllerManager.InputType == InputType.KeyboardOnlyMode)
		{
			ProcessFkbInput(inputEvent);
		}
		else
		{
			ProcessHotkeyInput(inputEvent);
		}
		ProcessDebugKeyInput(inputEvent);
	}

	private void ProcessDebugKeyInput(InputEvent inputEvent)
	{
		if (!(inputEvent is InputEventKey inputEventKey) || PlatformUtil.IsPlatformOverlayOpen() || !DisplayServer.WindowIsFocused() || NDevConsole.IsConsoleVisible || !NGame.IsTrailerMode)
		{
			return;
		}
		foreach (KeyValuePair<Key, StringName> item in _debugInputMap)
		{
			if (inputEventKey.Keycode == item.Key)
			{
				InputEventAction @event = new InputEventAction
				{
					Action = item.Value,
					Pressed = inputEvent.IsPressed()
				};
				Input.ParseInputEvent(@event);
			}
		}
	}

	private void ProcessHotkeyInput(InputEvent inputEvent)
	{
		if (NGame.Instance.Transition.InTransition || !NGame.IsGameFocusedWindow() || !(inputEvent is InputEventKey inputEventKey))
		{
			return;
		}
		foreach (KeyValuePair<StringName, Key> item in _mKbInputMap)
		{
			if (inputEventKey.Keycode == item.Value && !inputEvent.IsEcho())
			{
				InputEventAction @event = new InputEventAction
				{
					Action = item.Key,
					Pressed = inputEvent.IsPressed()
				};
				Input.ParseInputEvent(@event);
			}
		}
	}

	private void ProcessFkbInput(InputEvent inputEvent)
	{
		if (NGame.Instance.Transition.InTransition || !NGame.IsGameFocusedWindow() || !(inputEvent is InputEventKey inputEventKey))
		{
			return;
		}
		foreach (KeyValuePair<StringName, Key> item in _fKbInputMap)
		{
			if (inputEventKey.Keycode == item.Value && !inputEvent.IsEcho())
			{
				InputEventAction @event = new InputEventAction
				{
					Action = item.Key,
					Pressed = inputEvent.IsPressed()
				};
				Input.ParseInputEvent(@event);
			}
		}
	}

	public override void _UnhandledInput(InputEvent inputEvent)
	{
		if (NGame.Instance.Transition.InTransition || !NGame.IsGameFocusedWindow())
		{
			return;
		}
		foreach (KeyValuePair<StringName, StringName> item in _controllerInputMap)
		{
			if (inputEvent.IsActionPressed(item.Value))
			{
				InputEventAction @event = new InputEventAction
				{
					Action = item.Key,
					Pressed = true
				};
				Input.ParseInputEvent(@event);
			}
			else if (inputEvent.IsActionReleased(item.Value))
			{
				InputEventAction event2 = new InputEventAction
				{
					Action = item.Key,
					Pressed = false
				};
				Input.ParseInputEvent(event2);
			}
		}
	}

	public Key GetCurrentHotkey(StringName input)
	{
		if (ControllerManager.InputType != InputType.KeyboardOnlyMode)
		{
			return GetMKbHotkey(input);
		}
		return GetKbOnlyHotkey(input);
	}

	public Key GetMKbHotkey(StringName input)
	{
		if (!_mKbInputMap.TryGetValue(input, out var value))
		{
			return Key.None;
		}
		return value;
	}

	public Key GetKbOnlyHotkey(StringName input)
	{
		if (!_fKbInputMap.TryGetValue(input, out var value))
		{
			return Key.None;
		}
		return value;
	}

	public Texture2D? GetHotkeyIcon(string hotkey)
	{
		if (_controllerInputMap.TryGetValue(hotkey, out StringName value))
		{
			return ControllerManager.GetHotkeyIcon(value);
		}
		return null;
	}

	public void ModifyMKbKey(StringName input, Key shortcutKey)
	{
		KeyValuePair<StringName, Key> keyValuePair = _mKbInputMap.FirstOrDefault<KeyValuePair<StringName, Key>>((KeyValuePair<StringName, Key> kvp) => kvp.Value == shortcutKey && remappableMKbInputs.Contains(kvp.Key));
		if (keyValuePair.Key != null)
		{
			Key key = _mKbInputMap[input];
			_mKbInputMap[keyValuePair.Key] = key;
			EnsureEndTurnAndConfirmMkbKeyAreTheSame(keyValuePair.Key, key);
		}
		_mKbInputMap[input] = shortcutKey;
		EnsureEndTurnAndConfirmMkbKeyAreTheSame(input, shortcutKey);
		SaveMKbInputMapping();
		EmitSignalInputRebound();
	}

	/// <summary>
	/// Ensures that in Mkb mapping, the input for confirm is the same as the input for end turn
	/// </summary>
	private void EnsureEndTurnAndConfirmMkbKeyAreTheSame(StringName input, Key shortcutKey)
	{
		if (input == MegaInput.confirm)
		{
			_mKbInputMap[MegaInput.endTurn] = shortcutKey;
		}
		else if (input == MegaInput.endTurn)
		{
			_mKbInputMap[MegaInput.confirm] = shortcutKey;
		}
	}

	public void ModifyKbOnlyKey(StringName input, Key shortcutKey)
	{
		KeyValuePair<StringName, Key> keyValuePair = _fKbInputMap.FirstOrDefault<KeyValuePair<StringName, Key>>((KeyValuePair<StringName, Key> kvp) => kvp.Value == shortcutKey && remappableKbOnlyInputs.Contains(kvp.Key));
		if (keyValuePair.Key != null)
		{
			Key value = _fKbInputMap[input];
			_fKbInputMap[keyValuePair.Key] = value;
		}
		_fKbInputMap[input] = shortcutKey;
		SaveFKbInputMapping();
		EmitSignalInputRebound();
	}

	public void ModifyControllerButton(StringName input, StringName controllerInput)
	{
		StringName controllerInput2 = controllerInput;
		KeyValuePair<StringName, StringName> keyValuePair = _controllerInputMap.FirstOrDefault<KeyValuePair<StringName, StringName>>((KeyValuePair<StringName, StringName> kvp) => kvp.Value == controllerInput2 && remappableControllerInputs.Contains(kvp.Key));
		if (keyValuePair.Key != null)
		{
			StringName stringName = _controllerInputMap[input];
			_controllerInputMap[keyValuePair.Key] = stringName;
			EnsureEndTurnAndConfirmControllerInputAreTheSame(keyValuePair.Key, stringName);
		}
		_controllerInputMap[input] = controllerInput2;
		EnsureEndTurnAndConfirmControllerInputAreTheSame(input, controllerInput2);
		SaveControllerInputMapping();
		EmitSignalInputRebound();
	}

	/// <summary>
	/// Ensures that in Controller Input mapping, the input for confirm is the same as the input for end turn
	/// </summary>
	private void EnsureEndTurnAndConfirmControllerInputAreTheSame(StringName input, StringName controllerInput)
	{
		if (input == MegaInput.confirm)
		{
			_controllerInputMap[MegaInput.endTurn] = controllerInput;
		}
		else if (input == MegaInput.endTurn)
		{
			_controllerInputMap[MegaInput.confirm] = controllerInput;
		}
	}

	public void ResetToDefaults()
	{
		_mKbInputMap = DefaultHotkeyInputMap;
		_controllerInputMap = ControllerManager.GetDefaultControllerInputMap;
		_fKbInputMap = DefaultKbOnlyInputMap;
		SaveControllerInputMapping();
		SaveMKbInputMapping();
		SaveFKbInputMapping();
		EmitSignalInputRebound();
	}

	private void OnControllerTypeChanged()
	{
		if (ControllerManager.ControllerMappingType != SaveManager.Instance.SettingsSave.ControllerMappingType)
		{
			_controllerInputMap = ControllerManager.GetDefaultControllerInputMap;
			SaveControllerInputMapping();
			EmitSignalInputRebound();
		}
	}

	private void SaveControllerInputMapping()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		foreach (KeyValuePair<StringName, StringName> item in _controllerInputMap)
		{
			dictionary.Add(item.Key.ToString(), item.Value.ToString());
		}
		SaveManager.Instance.SettingsSave.ControllerMappingType = ControllerManager.ControllerMappingType;
		SaveManager.Instance.SettingsSave.ControllerMapping = dictionary;
		SaveManager.Instance.SaveSettings();
	}

	private void SaveMKbInputMapping()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		foreach (KeyValuePair<StringName, Key> item in _mKbInputMap)
		{
			dictionary.Add(item.Key.ToString(), item.Value.ToString());
		}
		SaveManager.Instance.SettingsSave.KeyboardMapping = dictionary;
		SaveManager.Instance.SaveSettings();
	}

	private void SaveFKbInputMapping()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		foreach (KeyValuePair<StringName, Key> item in _fKbInputMap)
		{
			dictionary.Add(item.Key.ToString(), item.Value.ToString());
		}
		SaveManager.Instance.SettingsSave.KbOnlyMapping = dictionary;
		SaveManager.Instance.SaveSettings();
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(21);
		list.Add(new MethodInfo(MethodName._EnterTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._UnhandledKeyInput, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ProcessDebugKeyInput, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ProcessHotkeyInput, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ProcessFkbInput, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._UnhandledInput, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.GetCurrentHotkey, new PropertyInfo(Variant.Type.Int, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.StringName, "input", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.GetMKbHotkey, new PropertyInfo(Variant.Type.Int, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.StringName, "input", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.GetKbOnlyHotkey, new PropertyInfo(Variant.Type.Int, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.StringName, "input", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.GetHotkeyIcon, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Texture2D"), exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "hotkey", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ModifyMKbKey, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.StringName, "input", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Int, "shortcutKey", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.EnsureEndTurnAndConfirmMkbKeyAreTheSame, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.StringName, "input", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Int, "shortcutKey", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ModifyKbOnlyKey, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.StringName, "input", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Int, "shortcutKey", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ModifyControllerButton, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.StringName, "input", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.StringName, "controllerInput", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.EnsureEndTurnAndConfirmControllerInputAreTheSame, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.StringName, "input", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.StringName, "controllerInput", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ResetToDefaults, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnControllerTypeChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SaveControllerInputMapping, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SaveMKbInputMapping, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SaveFKbInputMapping, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName._EnterTree && args.Count == 0)
		{
			_EnterTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._UnhandledKeyInput && args.Count == 1)
		{
			_UnhandledKeyInput(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ProcessDebugKeyInput && args.Count == 1)
		{
			ProcessDebugKeyInput(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ProcessHotkeyInput && args.Count == 1)
		{
			ProcessHotkeyInput(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ProcessFkbInput && args.Count == 1)
		{
			ProcessFkbInput(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._UnhandledInput && args.Count == 1)
		{
			_UnhandledInput(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.GetCurrentHotkey && args.Count == 1)
		{
			Key from = GetCurrentHotkey(VariantUtils.ConvertTo<StringName>(in args[0]));
			ret = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (method == MethodName.GetMKbHotkey && args.Count == 1)
		{
			Key from2 = GetMKbHotkey(VariantUtils.ConvertTo<StringName>(in args[0]));
			ret = VariantUtils.CreateFrom(in from2);
			return true;
		}
		if (method == MethodName.GetKbOnlyHotkey && args.Count == 1)
		{
			Key from3 = GetKbOnlyHotkey(VariantUtils.ConvertTo<StringName>(in args[0]));
			ret = VariantUtils.CreateFrom(in from3);
			return true;
		}
		if (method == MethodName.GetHotkeyIcon && args.Count == 1)
		{
			Texture2D from4 = GetHotkeyIcon(VariantUtils.ConvertTo<string>(in args[0]));
			ret = VariantUtils.CreateFrom(in from4);
			return true;
		}
		if (method == MethodName.ModifyMKbKey && args.Count == 2)
		{
			ModifyMKbKey(VariantUtils.ConvertTo<StringName>(in args[0]), VariantUtils.ConvertTo<Key>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.EnsureEndTurnAndConfirmMkbKeyAreTheSame && args.Count == 2)
		{
			EnsureEndTurnAndConfirmMkbKeyAreTheSame(VariantUtils.ConvertTo<StringName>(in args[0]), VariantUtils.ConvertTo<Key>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ModifyKbOnlyKey && args.Count == 2)
		{
			ModifyKbOnlyKey(VariantUtils.ConvertTo<StringName>(in args[0]), VariantUtils.ConvertTo<Key>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ModifyControllerButton && args.Count == 2)
		{
			ModifyControllerButton(VariantUtils.ConvertTo<StringName>(in args[0]), VariantUtils.ConvertTo<StringName>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.EnsureEndTurnAndConfirmControllerInputAreTheSame && args.Count == 2)
		{
			EnsureEndTurnAndConfirmControllerInputAreTheSame(VariantUtils.ConvertTo<StringName>(in args[0]), VariantUtils.ConvertTo<StringName>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ResetToDefaults && args.Count == 0)
		{
			ResetToDefaults();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnControllerTypeChanged && args.Count == 0)
		{
			OnControllerTypeChanged();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SaveControllerInputMapping && args.Count == 0)
		{
			SaveControllerInputMapping();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SaveMKbInputMapping && args.Count == 0)
		{
			SaveMKbInputMapping();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SaveFKbInputMapping && args.Count == 0)
		{
			SaveFKbInputMapping();
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._EnterTree)
		{
			return true;
		}
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName._UnhandledKeyInput)
		{
			return true;
		}
		if (method == MethodName.ProcessDebugKeyInput)
		{
			return true;
		}
		if (method == MethodName.ProcessHotkeyInput)
		{
			return true;
		}
		if (method == MethodName.ProcessFkbInput)
		{
			return true;
		}
		if (method == MethodName._UnhandledInput)
		{
			return true;
		}
		if (method == MethodName.GetCurrentHotkey)
		{
			return true;
		}
		if (method == MethodName.GetMKbHotkey)
		{
			return true;
		}
		if (method == MethodName.GetKbOnlyHotkey)
		{
			return true;
		}
		if (method == MethodName.GetHotkeyIcon)
		{
			return true;
		}
		if (method == MethodName.ModifyMKbKey)
		{
			return true;
		}
		if (method == MethodName.EnsureEndTurnAndConfirmMkbKeyAreTheSame)
		{
			return true;
		}
		if (method == MethodName.ModifyKbOnlyKey)
		{
			return true;
		}
		if (method == MethodName.ModifyControllerButton)
		{
			return true;
		}
		if (method == MethodName.EnsureEndTurnAndConfirmControllerInputAreTheSame)
		{
			return true;
		}
		if (method == MethodName.ResetToDefaults)
		{
			return true;
		}
		if (method == MethodName.OnControllerTypeChanged)
		{
			return true;
		}
		if (method == MethodName.SaveControllerInputMapping)
		{
			return true;
		}
		if (method == MethodName.SaveMKbInputMapping)
		{
			return true;
		}
		if (method == MethodName.SaveFKbInputMapping)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.ControllerManager)
		{
			ControllerManager = VariantUtils.ConvertTo<NControllerManager>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.ControllerManager)
		{
			NControllerManager from = ControllerManager;
			value = VariantUtils.CreateFrom(in from);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.ControllerManager, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		StringName controllerManager = PropertyName.ControllerManager;
		NControllerManager from = ControllerManager;
		info.AddProperty(controllerManager, Variant.From(in from));
		info.AddSignalEventDelegate(SignalName.InputRebound, backing_InputRebound);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.ControllerManager, out var value))
		{
			ControllerManager = value.As<NControllerManager>();
		}
		if (info.TryGetSignalEventDelegate<InputReboundEventHandler>(SignalName.InputRebound, out var value2))
		{
			backing_InputRebound = value2;
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
		List<MethodInfo> list = new List<MethodInfo>(1);
		list.Add(new MethodInfo(SignalName.InputRebound, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	protected void EmitSignalInputRebound()
	{
		EmitSignal(SignalName.InputRebound);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RaiseGodotClassSignalCallbacks(in godot_string_name signal, NativeVariantPtrArgs args)
	{
		if (signal == SignalName.InputRebound && args.Count == 0)
		{
			backing_InputRebound?.Invoke();
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
		if (signal == SignalName.InputRebound)
		{
			return true;
		}
		return base.HasGodotClassSignal(in signal);
	}
}

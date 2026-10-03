using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.ControllerInput.ControllerConfigs;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace MegaCrit.Sts2.Core.ControllerInput;

public class GodotControllerInputStrategy : IControllerInputStrategy
{
	private static readonly StringName _rStickLeftRaw = "raw_r_stick_left";

	private static readonly StringName _rStickRightRaw = "raw_r_stick_right";

	private static readonly StringName _rStickUpRaw = "raw_r_stick_up";

	private static readonly StringName _rStickDownRaw = "raw_r_stick_down";

	private static readonly StringName _lStickLeftRaw = "raw_l_stick_left";

	private static readonly StringName _lStickRightRaw = "raw_l_stick_right";

	private static readonly StringName _lStickUpRaw = "raw_l_stick_up";

	private static readonly StringName _lStickDownRaw = "raw_l_stick_down";

	private static readonly StringName _leftTriggerRaw = "raw_left_trigger";

	private static readonly StringName _rightTriggerRaw = "raw_right_trigger";

	/// <summary>
	/// Maps analog inputs with pressure sensitive input values (i.e. stick axis being a value between 0.0 and 1.0)
	/// and flattens it out to "isPressed" and "isReleased". This is important multiple events do not fire
	/// as that analog value changes.
	/// </summary>
	private readonly Dictionary<StringName, StringName[]> _analogToDigitalInput = new Dictionary<StringName, StringName[]>
	{
		{
			_rStickUpRaw,
			new StringName[1] { Controller.rStickUp }
		},
		{
			_rStickDownRaw,
			new StringName[1] { Controller.rStickDown }
		},
		{
			_rStickLeftRaw,
			new StringName[1] { Controller.rStickLeft }
		},
		{
			_rStickRightRaw,
			new StringName[1] { Controller.rStickRight }
		},
		{
			_leftTriggerRaw,
			new StringName[1] { Controller.leftTrigger }
		},
		{
			_rightTriggerRaw,
			new StringName[1] { Controller.rightTrigger }
		},
		{
			_lStickUpRaw,
			new StringName[2]
			{
				Controller.dPadUp,
				Controller.lStickUp
			}
		},
		{
			_lStickDownRaw,
			new StringName[2]
			{
				Controller.dPadDown,
				Controller.lStickDown
			}
		},
		{
			_lStickLeftRaw,
			new StringName[2]
			{
				Controller.dPadLeft,
				Controller.lStickLeft
			}
		},
		{
			_lStickRightRaw,
			new StringName[2]
			{
				Controller.dPadRight,
				Controller.lStickRight
			}
		}
	};

	private string? _currentControllerType;

	private ControllerConfig? _controllerConfig;

	public ControllerConfig? ControllerConfig
	{
		get
		{
			if (_controllerConfig == null)
			{
				UpdateControllerConfig();
			}
			return _controllerConfig;
		}
	}

	public Dictionary<StringName, StringName> GetDefaultControllerInputMap
	{
		get
		{
			if (ControllerConfig == null)
			{
				UpdateControllerConfig();
			}
			return ControllerConfig.DefaultControllerInputMap;
		}
	}

	public bool ShouldAllowControllerRebinding => true;

	public Task Init()
	{
		UpdateControllerConfig();
		return Task.FromResult(result: true);
	}

	public void ProcessInput()
	{
		StringName[] allControllerInputs = Controller.AllControllerInputs;
		foreach (StringName action in allControllerInputs)
		{
			if (Input.IsActionJustPressed(action))
			{
				UpdateControllerConfig();
			}
		}
		foreach (KeyValuePair<StringName, StringName[]> item in _analogToDigitalInput)
		{
			if (Input.IsActionJustPressed(item.Key))
			{
				StringName[] value = item.Value;
				foreach (StringName action2 in value)
				{
					InputEventAction @event = new InputEventAction
					{
						Action = action2,
						Pressed = true
					};
					Input.ParseInputEvent(@event);
				}
			}
			else if (Input.IsActionJustReleased(item.Key))
			{
				StringName[] value2 = item.Value;
				foreach (StringName action3 in value2)
				{
					InputEventAction event2 = new InputEventAction
					{
						Action = action3,
						Pressed = false
					};
					Input.ParseInputEvent(event2);
				}
			}
		}
	}

	private void UpdateControllerConfig()
	{
		if (Input.GetConnectedJoypads().Count == 0)
		{
			_controllerConfig = new SteamControllerConfig();
			return;
		}
		string joyName = Input.GetJoyName(0);
		if (!(joyName == _currentControllerType))
		{
			_currentControllerType = joyName;
			if (_currentControllerType.Contains("Xbox One") || _currentControllerType.Contains("XInput"))
			{
				_controllerConfig = new XboxOneConfig();
			}
			else if (_currentControllerType.Contains("Xbox 360"))
			{
				_controllerConfig = new Xbox360Config();
			}
			else if (_currentControllerType.Contains("PS3"))
			{
				_controllerConfig = new Ps4Config();
			}
			else if (_currentControllerType.Contains("PS4") || _currentControllerType.Contains("DualSense"))
			{
				_controllerConfig = new Ps4Config();
			}
			else if (_currentControllerType.Contains("PS5"))
			{
				_controllerConfig = new Ps4Config();
			}
			else if (_currentControllerType.Contains("Switch"))
			{
				_controllerConfig = new SwitchConfig();
			}
			else
			{
				_controllerConfig = new SteamControllerConfig();
			}
			NControllerManager.Instance?.OnControllerTypeChanged();
		}
	}

	public Texture2D? GetHotkeyIcon(string hotkey)
	{
		return _controllerConfig?.GetButtonIcon(hotkey);
	}

	public string GetControllerName()
	{
		if (Input.GetConnectedJoypads().Count == 0)
		{
			return "NONE";
		}
		return Input.GetJoyName(0);
	}

	public Vector2 GetLeftAnalogStickDirection()
	{
		return Input.GetVector(_lStickLeftRaw, _lStickRightRaw, _lStickUpRaw, _lStickDownRaw);
	}
}

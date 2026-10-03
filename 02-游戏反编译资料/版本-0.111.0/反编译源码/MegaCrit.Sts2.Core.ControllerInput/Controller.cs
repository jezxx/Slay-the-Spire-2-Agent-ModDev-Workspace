using Godot;

namespace MegaCrit.Sts2.Core.ControllerInput;

public static class Controller
{
	public static readonly StringName leftTrigger = "controller_left_trigger";

	public static readonly StringName rightTrigger = "controller_right_trigger";

	public static readonly StringName leftBumper = "controller_left_bumper";

	public static readonly StringName rightBumper = "controller_right_bumper";

	public static readonly StringName faceButtonNorth = "controller_face_button_north";

	public static readonly StringName faceButtonSouth = "controller_face_button_south";

	public static readonly StringName faceButtonEast = "controller_face_button_east";

	public static readonly StringName faceButtonWest = "controller_face_button_west";

	public static readonly StringName startButton = "controller_start_button";

	public static readonly StringName selectButton = "controller_select_button";

	public static readonly StringName dPadUp = "controller_d_pad_up";

	public static readonly StringName dPadDown = "controller_d_pad_down";

	public static readonly StringName dPadLeft = "controller_d_pad_left";

	public static readonly StringName dPadRight = "controller_d_pad_right";

	public static readonly StringName lStickPress = "controller_l_stick_press";

	public static readonly StringName lStickLeft = "controller_l_stick_left";

	public static readonly StringName lStickRight = "controller_l_stick_right";

	public static readonly StringName lStickUp = "controller_l_stick_up";

	public static readonly StringName lStickDown = "controller_l_stick_down";

	public static readonly StringName rStickLeft = "controller_r_stick_left";

	public static readonly StringName rStickRight = "controller_r_stick_right";

	public static readonly StringName rStickUp = "controller_r_stick_up";

	public static readonly StringName rStickDown = "controller_r_stick_down";

	public static readonly StringName ps4Touchpad = "ui_controller_touch_pad";

	public static StringName[] AllControllerInputs => new StringName[24]
	{
		dPadRight, dPadUp, dPadDown, dPadLeft, faceButtonEast, faceButtonNorth, faceButtonSouth, faceButtonWest, lStickDown, lStickLeft,
		lStickPress, lStickRight, lStickUp, leftBumper, leftTrigger, rightBumper, rightTrigger, selectButton, startButton, ps4Touchpad,
		rStickLeft, rStickRight, rStickUp, rStickDown
	};
}

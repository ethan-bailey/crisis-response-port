namespace Crisis.Runtime.FullSource
{
using System;
using FullSerializer;
using UnityEngine;

[fsObject("1", new Type[] { })]
public class InputMapping_1
{
	public InputKey left = new InputKey
	{
		name = "Move left",
		key = KeyCode.A,
		description = "Move to the left",
		category = InputKey.Category.Movement
	};

	public InputKey right = new InputKey
	{
		name = "Move right",
		key = KeyCode.D,
		description = "Move to the right",
		category = InputKey.Category.Movement
	};

	public InputKey stand = new InputKey
	{
		name = "Stand",
		key = KeyCode.W,
		description = "Stand straight",
		category = InputKey.Category.Movement
	};

	public InputKey kneel = new InputKey
	{
		name = "Kneel",
		key = KeyCode.S,
		description = "Kneel",
		category = InputKey.Category.Movement
	};

	public InputKey sprint = new InputKey
	{
		name = "Sprint",
		key = KeyCode.LeftShift,
		description = "Sprint while moving",
		category = InputKey.Category.Movement
	};

	public InputKey interact = new InputKey
	{
		name = "Interact",
		key = KeyCode.Space,
		description = "Interact with the environment",
		category = InputKey.Category.Interaction
	};

	public InputKey doorPush = new InputKey
	{
		name = "Push door",
		axis = "ScrollWheel",
		axisDirection = 1f,
		description = "Push door",
		category = InputKey.Category.Interaction
	};

	public InputKey doorPull = new InputKey
	{
		name = "Pull door",
		axis = "ScrollWheel",
		axisDirection = -1f,
		description = "Pull door",
		category = InputKey.Category.Interaction
	};

	public InputKey doorGradual = new InputKey
	{
		name = "Door gradual",
		key = KeyCode.LeftControl,
		description = "Hold to make door manipulations gradual",
		category = InputKey.Category.Interaction
	};

	public InputKey fire = new InputKey
	{
		name = "Fire",
		key = KeyCode.Mouse0,
		description = "Fire the active weapon",
		category = InputKey.Category.Weapons
	};

	public InputKey precision = new InputKey
	{
		name = "Precision",
		key = KeyCode.Mouse1,
		description = "Activate weapon precision mode",
		category = InputKey.Category.Weapons
	};

	public InputKey freeAim = new InputKey
	{
		name = "Free aim",
		key = KeyCode.U,
		description = "Activate free aim mode (experimental)",
		category = InputKey.Category.Weapons
	};

	public InputKey switchWeapons = new InputKey
	{
		name = "Switch",
		key = KeyCode.Tab,
		description = "Switch between primary and secondary weapon",
		category = InputKey.Category.Weapons
	};

	public InputKey fireMode = new InputKey
	{
		name = "Fire mode",
		key = KeyCode.T,
		description = "Cycle fire modes on active weapon",
		longPress = 0.4f,
		category = InputKey.Category.Weapons
	};

	public InputKey magazine = new InputKey
	{
		name = "Reload",
		key = KeyCode.R,
		description = "Change magazines",
		category = InputKey.Category.Weapons
	};

	public InputKey bolt = new InputKey
	{
		name = "Bolt",
		key = KeyCode.B,
		description = "Operate weapon bolt",
		category = InputKey.Category.Weapons
	};

	public InputKey flashlight = new InputKey
	{
		name = "Flashlight",
		key = KeyCode.F,
		description = "Toggle flashlight on active weapon",
		category = InputKey.Category.Weapons
	};

	public InputKey laser = new InputKey
	{
		name = "Laser",
		key = KeyCode.L,
		description = "Toggle laser on active weapon",
		category = InputKey.Category.Weapons
	};

	public InputKey grenade = new InputKey
	{
		name = "Grenade",
		key = KeyCode.G,
		description = "Ready stun grenade",
		category = InputKey.Category.Weapons
	};

	public InputKey entryTeam = new InputKey
	{
		name = "Entry team",
		key = KeyCode.F1,
		description = "Activate the entry team mode (interior view)",
		category = InputKey.Category.Modes
	};

	public InputKey marksman = new InputKey
	{
		name = "Marksman",
		key = KeyCode.F2,
		description = "Activate the marksman mode (exterior view)",
		category = InputKey.Category.Modes
	};

	public InputKey teamMoveTo = new InputKey
	{
		name = "Team move to",
		key = KeyCode.Alpha1,
		description = "Order team to move to your cursor position",
		category = InputKey.Category.Team
	};

	public InputKey teamClearNext = new InputKey
	{
		name = "Team clear next",
		key = KeyCode.Alpha2,
		description = "Order team to clear the room you are facing",
		category = InputKey.Category.Team
	};

	public InputKey teamMoveOut = new InputKey
	{
		name = "Team move out",
		key = KeyCode.Alpha3,
		description = "Order team to move in the direction you are facing",
		category = InputKey.Category.Team
	};

	public InputKey teamHold = new InputKey
	{
		name = "Team hold",
		key = KeyCode.Alpha4,
		description = "Order team to hold its current position",
		category = InputKey.Category.Team
	};

	public InputKey teamFollow = new InputKey
	{
		name = "Team follow",
		key = KeyCode.Alpha5,
		description = "Order team to follow you",
		category = InputKey.Category.Team
	};

	public InputKey teamIncreaseTempo = new InputKey
	{
		name = "Team increase tempo",
		key = KeyCode.UpArrow,
		description = "Order team to move faster",
		category = InputKey.Category.Team
	};

	public InputKey teamDecreaseTempo = new InputKey
	{
		name = "Team decrease tempo",
		key = KeyCode.DownArrow,
		description = "Order team to move slower",
		category = InputKey.Category.Team
	};

	public InputKey teamToggleStun = new InputKey
	{
		name = "Team use stun",
		key = KeyCode.LeftArrow,
		description = "Toggle team's use of stun grenades",
		category = InputKey.Category.Team
	};

	public InputKey marksmanHold = new InputKey
	{
		name = "Marksman hold",
		key = KeyCode.Insert,
		description = "Order marksman to hold fire",
		category = InputKey.Category.Marksman
	};

	public InputKey marksmanEngage = new InputKey
	{
		name = "Marksman engage",
		key = KeyCode.Home,
		description = "Order marksman to engage the current target",
		category = InputKey.Category.Marksman
	};

	public InputKey marksmanFree = new InputKey
	{
		name = "Marksman free",
		key = KeyCode.PageUp,
		description = "Order marksman to engage freely",
		category = InputKey.Category.Marksman
	};

	public InputKey bulletTime = new InputKey
	{
		name = "Bullet time",
		key = KeyCode.Keypad0,
		description = "Enable/disable bullet time triggers",
		category = InputKey.Category.Time
	};

	public InputKey slowMotion = new InputKey
	{
		name = "Slow motion",
		key = KeyCode.KeypadEnter,
		description = "Toggle slow motion",
		category = InputKey.Category.Time
	};

	public InputKey decreaseSlowMotion = new InputKey
	{
		name = "Decrease rate",
		key = KeyCode.KeypadMinus,
		description = "Decrease slow motion rate (slower)",
		category = InputKey.Category.Time
	};

	public InputKey increaseSlowMotion = new InputKey
	{
		name = "Increase rate",
		key = KeyCode.KeypadPlus,
		description = "Increase slow motion rate (faster)",
		category = InputKey.Category.Time
	};

	public InputKey objectives = new InputKey
	{
		name = "Display objectives",
		key = KeyCode.O,
		description = "Display objectives",
		category = InputKey.Category.Misc
	};
}

}

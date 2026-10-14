using System;
using UnityEngine;

public class InputKey
{
	public enum Category
	{
		Misc = 0,
		Movement = 1,
		Interaction = 2,
		Weapons = 3,
		Modes = 4,
		Team = 5,
		Marksman = 6,
		Time = 7
	}

	public KeyCode key;

	public string axis;

	public float axisDirection;

	public bool universal;

	public string name;

	public string description;

	public float axisValue;

	public float longPress = 0.2f;

	public Category category;

	private bool down;

	private bool downLong;

	private bool up;

	private bool upShort;

	private bool upLong;

	private bool enabled = true;

	private bool longEnabled = true;

	public bool Pressed { get; set; }

	public bool PressedLong { get; set; }

	public bool Down
	{
		get
		{
			return down && !DownHandled;
		}
		set
		{
			down = value;
		}
	}

	public bool DownLong
	{
		get
		{
			return LongEnabled && downLong && !DownHandled;
		}
		set
		{
			downLong = value;
		}
	}

	public bool Up
	{
		get
		{
			return up && !UpHandled && !DownHandled;
		}
		set
		{
			up = value;
		}
	}

	public bool UpShort
	{
		get
		{
			return upShort && !UpHandled && !DownHandled;
		}
		set
		{
			upShort = value;
		}
	}

	public bool UpLong
	{
		get
		{
			return upLong && !UpHandled && !DownHandled;
		}
		set
		{
			upLong = value;
		}
	}

	public bool DownHandled { get; set; }

	public bool UpHandled { get; set; }

	public float Timestamp { get; set; }

	public float TimePressed
	{
		get
		{
			return Time.realtimeSinceStartup - Timestamp;
		}
	}

	public bool IsKey
	{
		get
		{
			return axis == null;
		}
	}

	public bool IsAxis
	{
		get
		{
			return axis != null;
		}
	}

	public bool Enabled
	{
		get
		{
			return enabled;
		}
		set
		{
			enabled = (longEnabled = value);
			if (!enabled)
			{
				Reset();
			}
		}
	}

	public bool LongEnabled
	{
		get
		{
			return longEnabled;
		}
		set
		{
			longEnabled = value;
		}
	}

	public string KeyName
	{
		get
		{
			if (IsAxis)
			{
				return string.Format("{0} {1}", axis, (axisDirection != 1f) ? "Down" : "Up");
			}
			if (key != KeyCode.None)
			{
				return key.ToString();
			}
			return "None";
		}
	}

	public InputKey()
	{
		Timestamp = float.PositiveInfinity;
		longPress = InputHandler.DefaultLongPress;
	}

	public void Reset()
	{
		bool flag = (UpLong = false);
		flag = (UpShort = flag);
		flag = (UpHandled = flag);
		flag = (Up = flag);
		flag = (DownLong = flag);
		flag = (DownHandled = flag);
		flag = (Down = flag);
		flag = (PressedLong = flag);
		Pressed = flag;
	}
}

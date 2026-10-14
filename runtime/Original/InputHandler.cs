using System.Collections.Generic;
using UnityEngine;
public class InputHandler : MonoBehaviour {
public static float DefaultLongPress = 0.2f;
private bool inputSuspended;
public InputMapping Keys { get; private set; }
public void SetMapping(InputMapping keys)
	{
		Keys = keys;
	}
private void UpdateKeys()
	{
		foreach (InputKey item in Keys.All)
		{
			bool flag = false;
			if (item.Enabled && (!inputSuspended || item.universal))
			{
				if (item.IsAxis)
				{
					float axis = Input.GetAxis(item.axis);
					flag = axis != 0f && Mathf.Sign(axis) == item.axisDirection;
					item.axisValue = ((!flag) ? 0f : axis);
				}
				else
				{
					flag = Input.GetKey(item.key);
				}
				item.Down = !item.Pressed && flag;
				item.DownLong = !item.PressedLong && item.TimePressed >= item.longPress;
				item.Up = item.Pressed && !flag;
				item.UpShort = item.Up && item.TimePressed < item.longPress;
				item.UpLong = item.Up && item.TimePressed >= item.longPress;
				item.Pressed = flag;
				item.PressedLong = flag && item.TimePressed >= item.longPress;
				if (item.Down)
				{
					item.Timestamp = Time.realtimeSinceStartup;
					item.UpHandled = false;
				}
				else if (!item.Pressed)
				{
					item.Timestamp = float.PositiveInfinity;
					item.DownHandled = false;
				}
			}
		}
	}
public List<InputKey> FindKeyCodeBindings(KeyCode code)
	{
		List<InputKey> list = new List<InputKey>();
		if (code == KeyCode.None)
		{
			return list;
		}
		foreach (InputKey item in Keys.All)
		{
			if (item.key == code)
			{
				list.Add(item);
			}
		}
		return list;
	}
public List<InputKey> FindAxisBindings(string axis, float direction)
	{
		List<InputKey> list = new List<InputKey>();
		if (string.IsNullOrEmpty(axis))
		{
			return list;
		}
		foreach (InputKey item in Keys.All)
		{
			if (!(item.axis != axis) && item.axisDirection == Mathf.Sign(direction))
			{
				list.Add(item);
			}
		}
		return list;
	}
public void EnableAll()
	{
		foreach (InputKey item in Keys.All)
		{
			item.Enabled = true;
		}
	}
public void DisableAll()
	{
		foreach (InputKey item in Keys.All)
		{
			item.Enabled = false;
		}
	}
public void OnMissionStart()
	{
		SuspendInput(true, true);
	}
public void OnMissionEnd()
	{
	}
public void DisableOperatorInput(bool disable = true)
	{
		EnableCategory(InputKey.Category.Interaction, false);
		EnableCategory(InputKey.Category.Movement, false);
	}
public void SuspendInput(bool suspend, bool reset = false)
	{
		inputSuspended = suspend;
		if (reset)
		{
			ResetAll();
		}
	}
public void ResetAll()
	{
		foreach (InputKey item in Keys.All)
		{
			item.Reset();
		}
	}
public void EnableCategory(InputKey.Category category, bool enable = true)
	{
		foreach (InputKey item in Keys.All)
		{
			if (item.category == category)
			{
				item.Reset();
				item.Enabled = enable;
			}
		}
	}
}

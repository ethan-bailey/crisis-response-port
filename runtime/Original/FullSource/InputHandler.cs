namespace Crisis.Runtime.FullSource
{
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InputHandler : MonoBehaviour
{
	public class UpdateInputsEvent : UnityEvent<List<InputKey>>
	{
	}

	public static float DefaultLongPress = 0.2f;

	public UIBindingsTab bindingsDialog;

	public UIBindingConflictDialog conflictDialog;

	public UIBindingOverlay bindingOverlay;

	public ControlBindEvent onStartBinding = new ControlBindEvent();

	public UpdateInputsEvent onConflictUpdate = new UpdateInputsEvent();

	public UnityEventVoid onStopBinding = new UnityEventVoid();

	private UIBindingItem activeBinding;

	private InputKey conflictedInput;

	private bool inputSuspended;

	public InputMapping Keys { get; private set; }

	private void Update()
	{
		if (activeBinding != null)
		{
			UpdateBinding();
		}
		else
		{
			UpdateKeys();
		}
	}

	public void Initialize(UIBindingOverlay overlay, UIBindingConflictDialog conflictDialog)
	{
		bindingOverlay = overlay;
		this.conflictDialog = conflictDialog;
	}

	public void SetMapping(InputMapping keys)
	{
		Keys = keys;
	}

	public void StartBinding(UIBindingItem bindingItem)
	{
		onStartBinding.Invoke(bindingItem);
		activeBinding = bindingItem;
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

	private void UpdateBinding()
	{
		if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyUp(KeyCode.Escape))
		{
			if (Input.GetKeyUp(KeyCode.Escape))
			{
				StopActiveBind();
			}
			return;
		}
		if (Input.anyKeyDown)
		{
			foreach (KeyCode value in Enum.GetValues(typeof(KeyCode)))
			{
				if (Input.GetKey(value))
				{
					SetNewKeyCode(value);
					return;
				}
			}
		}
		float axis = Input.GetAxis("ScrollWheel");
		if (axis != 0f)
		{
			SetNewKeyAxis("ScrollWheel", Mathf.Sign(axis));
		}
	}

	private void SetNewKeyCode(KeyCode code)
	{
		List<InputKey> list = FindKeyCodeBindings(code);
		if (list.Count > 0)
		{
			if (list.Contains(activeBinding.inputKey))
			{
				StopActiveBind();
				return;
			}
			conflictedInput = activeBinding.inputKey;
			StopActiveBind();
			conflictDialog.Show(list[0], conflictedInput);
		}
		else
		{
			KeyCode key = activeBinding.inputKey.key;
			activeBinding.inputKey.key = code;
			activeBinding.inputKey.axis = null;
			activeBinding.inputKey.axisDirection = 0f;
			activeBinding.UpdateTexts();
			onConflictUpdate.Invoke(FindKeyCodeBindings(key));
			StopActiveBind();
		}
	}

	private void SetNewKeyAxis(string axis, float direction)
	{
		List<InputKey> list = FindAxisBindings(axis, direction);
		if (list.Count > 0)
		{
			if (list.Contains(activeBinding.inputKey))
			{
				StopActiveBind();
				return;
			}
			conflictedInput = activeBinding.inputKey;
			StopActiveBind();
			conflictDialog.Show(list[0], conflictedInput);
		}
		else
		{
			string axis2 = activeBinding.inputKey.axis;
			float axisDirection = activeBinding.inputKey.axisDirection;
			activeBinding.inputKey.axis = axis;
			activeBinding.inputKey.axisDirection = Mathf.Sign(direction);
			activeBinding.inputKey.key = KeyCode.None;
			activeBinding.UpdateTexts();
			onConflictUpdate.Invoke(FindAxisBindings(axis2, axisDirection));
			StopActiveBind();
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

	public void StopActiveBind()
	{
		if (!(activeBinding == null))
		{
			bindingOverlay.gameObject.SetActive(false);
			activeBinding.inputKey.DownHandled = true;
			activeBinding.CancelBind();
			activeBinding = null;
			onStopBinding.Invoke();
		}
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

}

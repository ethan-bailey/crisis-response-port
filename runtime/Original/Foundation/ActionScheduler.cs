using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ActionScheduler
{
	public enum DelayType
	{
		ScaledTime = 0,
		UnscaledTime = 1
	}

	private class ScheduledAction
	{
		public float timestamp;

		public DelayType delayType;

		public UnityAction action;

		public ScheduledAction(UnityAction action, float delay, DelayType type)
		{
			this.action = action;
			delayType = type;
			timestamp = delay + ((type != DelayType.ScaledTime) ? Time.realtimeSinceStartup : Time.time);
		}

		public bool Expired(float scaledTime, float unscaledTime)
		{
			return (delayType != DelayType.ScaledTime) ? (unscaledTime >= timestamp) : (scaledTime >= timestamp);
		}
	}

	private List<ScheduledAction> scheduledActions = new List<ScheduledAction>();

	public void Schedule(UnityAction action, float delay, DelayType type = DelayType.ScaledTime)
	{
		scheduledActions.Add(new ScheduledAction(action, delay, type));
	}

	public void Update(float scaledTime, float unscaledTime)
	{
		for (int num = scheduledActions.Count - 1; num >= 0; num--)
		{
			ScheduledAction scheduledAction = scheduledActions[num];
			if (scheduledAction.Expired(scaledTime, unscaledTime))
			{
				if (scheduledAction.action != null)
				{
					scheduledAction.action();
				}
				scheduledActions.RemoveAt(num);
			}
		}
	}
}

using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

using Crisis.Runtime.FullSource;
using Object = UnityEngine.Object;
using BehaviorDesigner.Runtime;
using Action = BehaviorDesigner.Runtime.Tasks.Action;
using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;
using Random = UnityEngine.Random;

namespace CharacterBehavior
{
	[TaskCategory("Occupant")]
	public class FireAtAudio : Action
	{
		private enum Action
		{
			Firing = 0,
			Waiting = 1
		}

		private CharacterBody self;

		private Action action;

		private bool isFiring;

		private bool isWaiting;

		private bool wasKneeling;

		private bool willKneel;

		private float sourcePosition;

		private float waitTimestamp;

		private float waitPeriod;

		private float fireTimestamp;

		private float firePeriod;

		private float aimpointY;

		private float aimpointTimestamp;

		private float aimpointPeriod;

		private Vector2 burstLength;

		private Vector2 waitLength;

		public override void OnAwake()
		{
			self = gameObject.GetComponent<CharacterBody>();
			burstLength = new Vector2(0.05f, 0.5f);
			waitLength = new Vector2(0.1f, 2f);
		}

		public override void OnStart()
		{
			if (self.Blackboard.AudioStimuli != null)
			{
				sourcePosition = self.Blackboard.AudioStimuli.position;
			}
			wasKneeling = self.IsKneeling;
			willKneel = Random.value < 0.5f;
			aimpointY = 1.5f;
			isFiring = false;
		}

		public override void OnEnd()
		{
			self.IsKneeling = wasKneeling;
			self.WeaponAimer.Mode = WeaponAimer.AimMode.Ready;
		}

		public override TaskStatus OnUpdate()
		{
			if (!self.Facing(sourcePosition) && StaticBehavior.TurnAround(self) != TaskStatus.Success)
			{
				return TaskStatus.Running;
			}
			if (Global.Context.Mission.Occupants.FirstConscious != self)
			{
				return TaskStatus.Failure;
			}
			if (Time.time - aimpointTimestamp >= aimpointPeriod)
			{
				aimpointY = Random.Range(0.5f, 1.8f);
				aimpointTimestamp = Time.time;
				aimpointPeriod = Random.Range(0.3f, 1.5f);
			}
			self.WeaponAimer.Mode = WeaponAimer.AimMode.AtWorldPoint;
			self.WeaponAimer.Point = new Vector3(self.Position + self.Direction * 3f, aimpointY, -1.5f);
			switch (action)
			{
			case Action.Firing:
				if (!isFiring)
				{
					isFiring = true;
					fireTimestamp = Time.time;
					firePeriod = Random.Range(burstLength.x, burstLength.y);
				}
				if (isFiring)
				{
					if (Time.time - fireTimestamp < firePeriod)
					{
						self.IsUsingPrecision = false;
						self.PullTrigger();
					}
					else
					{
						action = Action.Waiting;
					}
				}
				break;
			case Action.Waiting:
				if (!isWaiting)
				{
					isWaiting = true;
					waitPeriod = Random.Range(waitLength.x, waitLength.y);
					waitTimestamp = Time.time;
				}
				if (isWaiting && Time.time - waitTimestamp >= waitPeriod)
				{
					isFiring = (isWaiting = false);
					action = Action.Firing;
				}
				break;
			}
			return TaskStatus.Running;
		}
	}
}

using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

using Crisis.Runtime.FullSource;
using Object = UnityEngine.Object;
using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;
using Action = BehaviorDesigner.Runtime.Tasks.Action;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Characters.Behavior.Conditions.Operators
{
	[TaskCategory("Operator")]
	public class SetFollowMovePosition_DEPRECATED : Action
	{
		public float coverDistance = 0.5f;

		public float kneelDistance = 0.75f;

		public float normalDistance = 1f;

		[BehaviorDesigner.Runtime.Tasks.Tooltip("If an obstacle gets closer than this I will stop.")]
		public float obstacleClearance = 1f;

		public SharedFloat position;

		private TeamOperator self;

		private TeamOperator leader;

		public override void OnAwake()
		{
			self = gameObject.GetComponent<TeamOperator>();
			leader = self.Leader;
		}

		public override TaskStatus OnUpdate()
		{
			if (leader == null)
			{
				return TaskStatus.Failure;
			}
			float num = leader.transform.position.x - FollowDistance() * leader.Body.Direction;
			if (leader.FacingDoor)
			{
				DoorController door = leader.Door;
				float num2 = Mathf.Sign(door.transform.position.x - self.Body.Position);
				if (num2 == door.OpeningDirection)
				{
					num = leader.transform.position.x - FollowDistance() * leader.Body.Direction;
				}
				else
				{
					float num3 = Mathf.Max(0.5f, (num2 != 1f) ? door.OpenClearanceRight : door.OpenClearanceLeft);
					float num4 = self.Body.ActiveWeapon.Length() + 1.5f;
					num = door.transform.position.x - (num3 + num4) * num2;
				}
			}
			else
			{
				num = leader.transform.position.x - FollowDistance() * leader.Body.Direction;
			}
			position.Value = num;
			return TaskStatus.Success;
		}

		private float FollowDistance()
		{
			float result = normalDistance;
			if (!leader.Body.WeaponIsHeldHigh())
			{
				result = coverDistance;
			}
			else if (leader.Body.IsKneeling)
			{
				result = kneelDistance;
			}
			return result;
		}
	}
}

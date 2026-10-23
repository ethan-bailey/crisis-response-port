using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

using Crisis.Runtime.FullSource;
using Object = UnityEngine.Object;
using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;
using Action = BehaviorDesigner.Runtime.Tasks.Action;
using Random = UnityEngine.Random;

namespace CharacterBehavior
{
	[TaskCategory("Character/Locomotion")]
	public class SelectMovePosition : Action
	{
		public enum Type
		{
			Position = 0,
			Door = 1,
			Audio = 2
		}

		public Type type;

		[BehaviorDesigner.Runtime.Tasks.Tooltip("The selected position.")]
		public SharedFloat position;

		[BehaviorDesigner.Runtime.Tasks.Tooltip("Door to move toward when type is set to Door.")]
		public SharedDoor door;

		[BehaviorDesigner.Runtime.Tasks.Tooltip("Audio stimuli to move toward when type is set to Audio.")]
		public SharedAudioStimuli audioStimuli;

		[BehaviorDesigner.Runtime.Tasks.Tooltip("The absolute position or relative distance to move. If type is Door or Audio, this is an offset from the target.")]
		public float value;

		[BehaviorDesigner.Runtime.Tasks.Tooltip("Should the value be relative to the current position or absolute? Positive values mean forward in the current direction, negative backward. For Type Door and Audio this only affects the direction of the offset.")]
		public bool relative;

		[BehaviorDesigner.Runtime.Tasks.Tooltip("Should the value be randomized?")]
		public bool randomize;

		[BehaviorDesigner.Runtime.Tasks.Tooltip("If randomized is set the value will be randomized between this low and high.")]
		public Vector2 distanceInterval = new Vector2(0.5f, 3f);

		private CharacterBody self;

		private Occupant occupant;

		public override void OnAwake()
		{
			self = gameObject.GetComponent<CharacterBody>();
			occupant = gameObject.GetComponent<Occupant>();
		}

		public override TaskStatus OnUpdate()
		{
			if ((type == Type.Door && door.Value == null) || (type == Type.Audio && audioStimuli.Value == null))
			{
				return TaskStatus.Failure;
			}
			Vector2 vector = new Vector2(occupant.patrolAreaLeft, occupant.patrolAreaRight);
			float newPosition = GetNewPosition();
			for (int i = 0; i < 10; i++)
			{
				if (newPosition >= vector.x && newPosition <= vector.y)
				{
					position.Value = newPosition;
					return TaskStatus.Success;
				}
				newPosition = GetNewPosition();
			}
			position.Value = self.transform.position.x;
			return TaskStatus.Failure;
		}

		private float GetNewPosition()
		{
			float x = self.transform.position.x;
			if (relative)
			{
				float num = 0f;
				switch (type)
				{
				case Type.Position:
					num = self.transform.position.x;
					break;
				case Type.Door:
					num = door.Value.transform.position.x;
					break;
				case Type.Audio:
					num = audioStimuli.Value.position;
					break;
				}
				float num2 = ((!(Random.value >= 0.5f)) ? (-1f) : 1f);
				float num3 = ((!randomize) ? value : (Random.Range(distanceInterval.x, distanceInterval.y) * num2));
				return num + num3 * self.Direction;
			}
			float num4 = 0f;
			switch (type)
			{
			case Type.Position:
				num4 = 0f;
				break;
			case Type.Door:
				num4 = door.Value.transform.position.x;
				break;
			case Type.Audio:
				num4 = audioStimuli.Value.position;
				break;
			}
			float num5 = ((type != Type.Position) ? Mathf.Sign(self.transform.position.x - num4) : ((!(Random.value >= 0.5f)) ? (-1f) : 1f));
			float num6 = ((!randomize) ? value : (Random.Range(distanceInterval.x, distanceInterval.y) * num5));
			return num4 + num6;
		}
	}
}

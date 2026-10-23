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
	[TaskCategory("Character/Perception")]
	[TaskIcon("Assets/Editor/Behavior icons/ear stimuli.png")]
	[TaskDescription("Tests the distance to the character's current audio stimuli. Returns success if the distance is lower than the comparator.")]
	public class AudioDistance : Conditional
	{
		public SharedAudioStimuli audioStimuli;

		[BehaviorDesigner.Runtime.Tasks.Tooltip("The test returns success when the audio source is closer than this value. In meters.")]
		public float distance;

		[BehaviorDesigner.Runtime.Tasks.Tooltip("If this is set the test will be done against the number of rooms between the source and the character.")]
		public bool useRooms;

		[BehaviorDesigner.Runtime.Tasks.Tooltip("If the number of rooms between the stimuli source and the character is lower or equal the test succeeds.")]
		public int numberOfSeparatingRooms;

		private CharacterBody self;

		public override void OnAwake()
		{
			self = gameObject.GetComponent<CharacterBody>();
		}

		public override TaskStatus OnUpdate()
		{
			if (audioStimuli.Value == null)
			{
				return TaskStatus.Failure;
			}
			if (useRooms)
			{
				int count = 0;
				if (!GetNumberOfSeparatingRooms(ref count) || count > numberOfSeparatingRooms)
				{
					return TaskStatus.Failure;
				}
			}
			else
			{
				float num = Mathf.Abs(audioStimuli.Value.position - self.transform.position.x);
				if (num > distance)
				{
					return TaskStatus.Failure;
				}
			}
			return TaskStatus.Success;
		}

		private bool GetNumberOfSeparatingRooms(ref int count)
		{
			Room room = self.Room;
			Room room2 = self.Mission.RoomAtPosition(audioStimuli.Value.position);
			if (room2 == null || room == null)
			{
				return false;
			}
			if (Mathf.Sign(audioStimuli.Value.position - self.transform.position.x) > 0f)
			{
				while (room != null && room != room2)
				{
					count++;
					room = room.rightRoom;
				}
			}
			else
			{
				while (room != null && room != room2)
				{
					count++;
					room = room.leftRoom;
				}
			}
			return true;
		}
	}
}

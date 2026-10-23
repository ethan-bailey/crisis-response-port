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
	[TaskCategory("Character/Flee")]
	public class SetFleeSource : Action
	{
		[BehaviorDesigner.Runtime.Tasks.Tooltip("Set this flag to clear the current source. This will abort fleeing.")]
		public bool clearSource;

		[BehaviorDesigner.Runtime.Tasks.Tooltip("The type of thing I'm fleeing from.")]
		public ActionSource.Type type;

		public SharedTarget target;

		public SharedAudioStimuli audioStimuli;

		public SharedGrenade grenade;

		[BehaviorDesigner.Runtime.Tasks.Tooltip("Where the information is stored.")]
		public SharedFleeSource fleeSource;

		[BehaviorDesigner.Runtime.Tasks.Tooltip("Position to flee from when that option is selected.")]
		public float position;

		private CharacterBody self;

		public override void OnAwake()
		{
			self = gameObject.GetComponent<CharacterBody>();
		}

		public override TaskStatus OnUpdate()
		{
			if (clearSource)
			{
				fleeSource.Value = null;
				return TaskStatus.Success;
			}
			if (fleeSource.Value == null)
			{
				fleeSource.Value = new ActionSource();
			}
			fleeSource.Value.type = type;
			fleeSource.Value.transform = null;
			fleeSource.Value.position = self.transform.position.x;
			switch (type)
			{
			case ActionSource.Type.Character:
				if (target.Value == null)
				{
					return TaskStatus.Failure;
				}
				fleeSource.Value.transform = target.Value.transform;
				break;
			case ActionSource.Type.Audio:
				if (audioStimuli.Value == null)
				{
					return TaskStatus.Failure;
				}
				fleeSource.Value.position = audioStimuli.Value.position;
				break;
			case ActionSource.Type.Position:
				fleeSource.Value.position = position;
				break;
			case ActionSource.Type.Grenade:
			{
				if (grenade.Value == null)
				{
					return TaskStatus.Failure;
				}
				Rigidbody component = grenade.Value.GetComponent<Rigidbody>();
				fleeSource.Value.position = grenade.Value.transform.position.x;
				break;
			}
			}
			return TaskStatus.Success;
		}
	}
}

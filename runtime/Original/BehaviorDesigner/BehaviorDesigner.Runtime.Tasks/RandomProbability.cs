using System;
using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;
using Action = BehaviorDesigner.Runtime.Tasks.Action;

namespace BehaviorDesigner.Runtime.Tasks
{
	[TaskDescription("The random probability task will return success when the random probability is above the succeed probability. It will otherwise return failure.")]
	[HelpURL("http://www.opsive.com/assets/BehaviorDesigner/documentation.php?id=33")]
	public class RandomProbability : Conditional
	{
		[Tooltip("The chance that the task will return success")]
		public SharedFloat successProbability = 0.5f;

		[Tooltip("Seed the random number generator to make things easier to debug")]
		public SharedInt seed;

		[Tooltip("Do we want to use the seed?")]
		public SharedBool useSeed;

		private Random random;

		public override void OnAwake()
		{
			if (useSeed.Value)
			{
				random = new Random(seed.Value);
			}
			else
			{
				random = new Random();
			}
		}

		public override TaskStatus OnUpdate()
		{
			float num = (float)random.NextDouble();
			if (num < successProbability.Value)
			{
				return TaskStatus.Success;
			}
			return TaskStatus.Failure;
		}

		public override void OnReset()
		{
			successProbability = 0.5f;
			seed = 0;
			useSeed = false;
		}
	}
}

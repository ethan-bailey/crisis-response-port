using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;

namespace BehaviorDesigner.Runtime.Tasks
{
	[TaskDescription("Evaluates the specified conditional task. If the conditional task returns success then the child task is run and the child status is returned. If the conditional task does not return success then the child task is not run and a failure status is immediately returned.")]
	[HelpURL("http://www.opsive.com/assets/BehaviorDesigner/documentation.php?id=146")]
	[TaskIcon("{SkinColor}ConditionalEvaluatorIcon.png")]
	public class ConditionalEvaluator : Decorator
	{
		[Tooltip("Should the conditional task be reevaluated every tick?")]
		public SharedBool reevaluate;

		[InspectTask]
		[Tooltip("The conditional task to evaluate")]
		public Conditional conditionalTask;

		private TaskStatus executionStatus;

		private bool checkConditionalTask = true;

		private bool conditionalTaskFailed;

		public override void OnAwake()
		{
			if (conditionalTask != null)
			{
				conditionalTask.Owner = base.Owner;
				conditionalTask.GameObject = gameObject;
				conditionalTask.Transform = transform;
				conditionalTask.OnAwake();
			}
		}

		public override void OnStart()
		{
			if (conditionalTask != null)
			{
				conditionalTask.OnStart();
			}
		}

		public override bool CanExecute()
		{
			if (checkConditionalTask)
			{
				checkConditionalTask = false;
				OnUpdate();
			}
			if (conditionalTaskFailed)
			{
				return false;
			}
			return executionStatus == TaskStatus.Inactive || executionStatus == TaskStatus.Running;
		}

		public override bool CanReevaluate()
		{
			return reevaluate.Value;
		}

		public override TaskStatus OnUpdate()
		{
			TaskStatus taskStatus = conditionalTask.OnUpdate();
			conditionalTaskFailed = conditionalTask == null || taskStatus == TaskStatus.Failure;
			return taskStatus;
		}

		public override void OnChildExecuted(TaskStatus childStatus)
		{
			executionStatus = childStatus;
		}

		public override TaskStatus OverrideStatus()
		{
			return TaskStatus.Failure;
		}

		public override TaskStatus OverrideStatus(TaskStatus status)
		{
			if (conditionalTaskFailed)
			{
				return TaskStatus.Failure;
			}
			return status;
		}

		public override void OnEnd()
		{
			executionStatus = TaskStatus.Inactive;
			checkConditionalTask = true;
			conditionalTaskFailed = false;
			if (conditionalTask != null)
			{
				conditionalTask.OnEnd();
			}
		}

		public override void OnReset()
		{
			conditionalTask = null;
		}
	}
}

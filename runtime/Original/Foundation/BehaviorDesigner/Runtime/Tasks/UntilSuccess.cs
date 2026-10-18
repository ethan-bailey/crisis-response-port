namespace BehaviorDesigner.Runtime.Tasks
{
	[TaskDescription("The until success task will keep executing its child task until the child task returns success.")]
	[HelpURL("http://www.opsive.com/assets/BehaviorDesigner/documentation.php?id=42")]
	[TaskIcon("{SkinColor}UntilSuccessIcon.png")]
	public class UntilSuccess : Decorator
	{
		private TaskStatus executionStatus;

		public override bool CanExecute()
		{
			return executionStatus == TaskStatus.Failure || executionStatus == TaskStatus.Inactive;
		}

		public override void OnChildExecuted(TaskStatus childStatus)
		{
			executionStatus = childStatus;
		}

		public override void OnEnd()
		{
			executionStatus = TaskStatus.Inactive;
		}
	}
}

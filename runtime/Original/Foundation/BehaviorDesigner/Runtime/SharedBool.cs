using System;

namespace BehaviorDesigner.Runtime
{
	[Serializable]
	public class SharedBool : SharedVariable<bool>
	{
		public static implicit operator SharedBool(bool value)
		{
			SharedBool sharedBool = new SharedBool();
			sharedBool.mValue = value;
			return sharedBool;
		}
	}
}

using System;

namespace BehaviorDesigner.Runtime
{
	[Serializable]
	public class SharedInt : SharedVariable<int>
	{
		public static implicit operator SharedInt(int value)
		{
			SharedInt sharedInt = new SharedInt();
			sharedInt.mValue = value;
			return sharedInt;
		}
	}
}

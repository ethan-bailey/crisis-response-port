using System;

namespace BehaviorDesigner.Runtime
{
	[Serializable]
	public class SharedFloat : SharedVariable<float>
	{
		public static implicit operator SharedFloat(float value)
		{
			SharedFloat sharedFloat = new SharedFloat();
			sharedFloat.Value = value;
			return sharedFloat;
		}
	}
}

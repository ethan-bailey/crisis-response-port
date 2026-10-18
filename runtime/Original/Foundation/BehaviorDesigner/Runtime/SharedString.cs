using System;

namespace BehaviorDesigner.Runtime
{
	[Serializable]
	public class SharedString : SharedVariable<string>
	{
		public static implicit operator SharedString(string value)
		{
			SharedString sharedString = new SharedString();
			sharedString.mValue = value;
			return sharedString;
		}
	}
}

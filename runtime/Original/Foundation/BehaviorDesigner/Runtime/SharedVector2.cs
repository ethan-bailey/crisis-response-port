using System;
using UnityEngine;

namespace BehaviorDesigner.Runtime
{
	[Serializable]
	public class SharedVector2 : SharedVariable<Vector2>
	{
		public static implicit operator SharedVector2(Vector2 value)
		{
			SharedVector2 sharedVector = new SharedVector2();
			sharedVector.mValue = value;
			return sharedVector;
		}
	}
}

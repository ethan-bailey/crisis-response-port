using System.Collections.Generic;
using UnityEngine;

public class IKArmSequence : MonoBehaviour
{
	public List<IKArmAnimationFrame> right = new List<IKArmAnimationFrame>();

	public List<IKArmAnimationFrame> left = new List<IKArmAnimationFrame>();

	public IKArmSequenceRuntime Sequence()
	{
		return new IKArmSequenceRuntime(this);
	}

	public IKArmSequenceRuntime Then(IKArmSequence sequence)
	{
		IKArmSequenceRuntime iKArmSequenceRuntime = new IKArmSequenceRuntime(this);
		iKArmSequenceRuntime.right.AddRange(sequence.right);
		iKArmSequenceRuntime.left.AddRange(sequence.left);
		return iKArmSequenceRuntime;
	}
}

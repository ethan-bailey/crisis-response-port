using System.Collections.Generic;

public class IKArmSequenceRuntime
{
	public List<IKArmAnimationFrame> right = new List<IKArmAnimationFrame>();

	public List<IKArmAnimationFrame> left = new List<IKArmAnimationFrame>();

	public IKArmSequenceRuntime()
	{
	}

	public IKArmSequenceRuntime(IKArmSequence source)
	{
		right.AddRange(source.right);
		left.AddRange(source.left);
	}

	public IKArmSequenceRuntime Append(IKArmSequenceRuntime sequence)
	{
		right.AddRange(sequence.right);
		left.AddRange(sequence.left);
		return this;
	}

	public IKArmSequenceRuntime Append(IKArmSequence sequence)
	{
		right.AddRange(sequence.right);
		left.AddRange(sequence.left);
		return this;
	}
}

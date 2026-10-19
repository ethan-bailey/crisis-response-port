namespace Crisis.Runtime.FullSource
{
using System.Collections.Generic;

public class CharacterComparer : IComparer<CharacterBody>
{
	public float position;

	private bool relative;

	public int Compare(CharacterBody a, CharacterBody b)
	{
		if (relative)
		{
			float num = a.KinematicDistanceToPosition(position);
			float num2 = b.KinematicDistanceToPosition(position);
			if (num > num2)
			{
				return 1;
			}
			if (num < num2)
			{
				return -1;
			}
			return 0;
		}
		if (a.Position < b.Position)
		{
			return -1;
		}
		if (a.Position > b.Position)
		{
			return 1;
		}
		return 0;
	}

	public static IComparer<CharacterBody> RelativePositionDescending(float relativePosition)
	{
		CharacterComparer characterComparer = new CharacterComparer();
		characterComparer.position = relativePosition;
		characterComparer.relative = true;
		return characterComparer;
	}

	public static IComparer<CharacterBody> AbsolutePositionAscending()
	{
		CharacterComparer characterComparer = new CharacterComparer();
		characterComparer.relative = false;
		return characterComparer;
	}
}

}

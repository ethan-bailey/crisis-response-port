namespace Crisis.Runtime.FullSource
{
using System.Collections.Generic;
using UnityEngine;

public class DialogTree : MonoBehaviour
{
	public List<SerializableDialogState> states = new List<SerializableDialogState>();

	public static bool SwapOptionIndices(ref List<SerializableDialogOption> options, int optionA, int optionB)
	{
		if (optionA < 0 || optionB < 0 || optionA >= options.Count || optionB >= options.Count)
		{
			return false;
		}
		SerializableDialogOption other = new SerializableDialogOption(options[optionA]);
		options[optionA].Replace(options[optionB]);
		options[optionB].Replace(other);
		return true;
	}
}

}

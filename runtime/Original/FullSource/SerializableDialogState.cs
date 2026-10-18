namespace Crisis.Runtime.FullSource
{
using System;
using System.Collections.Generic;

[Serializable]
public class SerializableDialogState
{
	public string phrase = "New dialog state";

	public List<SerializableDialogOption> options = new List<SerializableDialogOption>();

	public SerializableDialogState()
	{
	}

	public SerializableDialogState(SerializableDialogState other)
	{
		phrase = other.phrase;
		options = other.options;
	}

	public SerializableDialogState Replace(SerializableDialogState other)
	{
		phrase = other.phrase;
		options.Clear();
		foreach (SerializableDialogOption option in other.options)
		{
			options.Add(new SerializableDialogOption(option));
		}
		return this;
	}
}

}

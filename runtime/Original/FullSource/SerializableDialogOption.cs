namespace Crisis.Runtime.FullSource
{
using System;

[Serializable]
public class SerializableDialogOption
{
	public string phrase = "New dialog option";

	public int nextStateIndex;

	public string eventName;

	public float morale;

	public bool moraleRelative = true;

	public float aggression;

	public bool aggressionRelative = true;

	public float fear;

	public bool fearRelative = true;

	public SerializableDialogOption()
	{
	}

	public SerializableDialogOption(SerializableDialogOption other)
	{
		Replace(other);
	}

	public SerializableDialogOption Replace(SerializableDialogOption other)
	{
		phrase = other.phrase;
		nextStateIndex = other.nextStateIndex;
		eventName = other.eventName;
		morale = other.morale;
		moraleRelative = other.moraleRelative;
		aggression = other.aggression;
		aggressionRelative = other.aggressionRelative;
		fear = other.fear;
		fearRelative = other.fearRelative;
		return this;
	}
}

}

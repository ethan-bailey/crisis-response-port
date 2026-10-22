using UnityEngine;

public class CharacterDialog : MonoBehaviour
{
	public DialogTree tree;

	private CharacterBody body;

	public int CurrentState { get; private set; }

	public void Initialize(DialogTree tree)
	{
		this.tree = tree;
		base.enabled = tree != null;
		body = GetComponent<CharacterBody>();
	}

	public SerializableDialogState GetCurrentState()
	{
		if (CurrentState < 0 || CurrentState >= tree.states.Count)
		{
			return null;
		}
		return tree.states[CurrentState];
	}

	public bool ExecuteOption(ref SerializableDialogOption option)
	{
		if (option.nextStateIndex >= 0 && option.nextStateIndex < tree.states.Count)
		{
			CurrentState = option.nextStateIndex;
		}
		ApplyStats(ref option);
		Event(option.eventName);
		return true;
	}

	private void Event(string name)
	{
		if (name != null && name == "Test")
		{
			body.SetUnconscious();
		}
	}

	private void ApplyStats(ref SerializableDialogOption option)
	{
		body.Blackboard.morale = Mathf.Clamp01(((!option.moraleRelative) ? 0f : body.Blackboard.morale) + option.morale);
		body.Blackboard.aggression = Mathf.Clamp01(((!option.aggressionRelative) ? 0f : body.Blackboard.aggression) + option.aggression);
		body.Blackboard.fear = Mathf.Clamp01(((!option.fearRelative) ? 0f : body.Blackboard.fear) + option.fear);
	}
}

namespace Crisis.Runtime.FullSource
{
using System.Collections.Generic;
using UnityEngine;

public class Occupants : MonoBehaviour
{
	public UnityEventVoid allDisabledEvent = new UnityEventVoid();

	public UnityEventVoid allRestrainedEvent = new UnityEventVoid();

	private Mission mission;

	private List<CharacterBody> bodies = new List<CharacterBody>();

	private Dictionary<string, CharacterBody> occupants = new Dictionary<string, CharacterBody>();

	private int consciousCount;

	private bool allSafe;

	private bool allRestrained;

	public Transform Container { get; private set; }

	public CharacterBody FirstConscious { get; private set; }

	public int Count
	{
		get
		{
			return Container.childCount;
		}
	}

	public int ConsciousCount
	{
		get
		{
			return consciousCount;
		}
	}

	public List<CharacterBody> Bodies
	{
		get
		{
			return bodies;
		}
	}

	public bool AllRenderedSafe { get; private set; }

	public bool AllRestrained { get; private set; }

	private void Awake()
	{
		mission = Global.Context.Mission;
		Container = new GameObject("Occupants").transform;
		Container.SetParent(mission.Containers, true);
	}

	private void Update()
	{
		UpdateFirstConscious();
		if (mission.InteriorMode != null && !mission.AllRoomsChecked)
		{
			return;
		}
		consciousCount = 0;
		allSafe = Container.childCount > 0;
		allRestrained = Container.childCount > 0;
		foreach (Transform item in Container)
		{
			CharacterBody component = item.GetComponent<CharacterBody>();
			if (!component.IsRagdoll)
			{
				consciousCount++;
				if (!component.IsCompliant)
				{
					allSafe = false;
				}
			}
			if (!component.IsRestrained)
			{
				allRestrained = false;
			}
		}
		if (allSafe && !AllRenderedSafe)
		{
			AllRenderedSafe = true;
			allDisabledEvent.Invoke();
		}
		if (allRestrained && !AllRestrained)
		{
			AllRestrained = true;
			allRestrainedEvent.Invoke();
		}
	}

	private void UpdateFirstConscious()
	{
		FirstConscious = null;
		List<CharacterBody> list = new List<CharacterBody>(bodies);
		list.Sort(CharacterComparer.AbsolutePositionAscending());
		foreach (CharacterBody item in list)
		{
			if (!item.IsRagdoll)
			{
				FirstConscious = item;
				break;
			}
		}
	}

	public CharacterBody Occupant(int index)
	{
		if (index < 0 || index >= Container.childCount)
		{
			return null;
		}
		return Container.GetChild(index).GetComponent<CharacterBody>();
	}

	public CharacterBody ClosestToPosition(float position, bool conscious = true)
	{
		CharacterBody result = null;
		float num = float.PositiveInfinity;
		foreach (Transform item in Container)
		{
			CharacterBody component = item.GetComponent<CharacterBody>();
			if (!conscious || !component.IsRagdoll)
			{
				float num2 = Mathf.Abs(component.Position - position);
				if (num2 < num)
				{
					num = num2;
					result = component;
				}
			}
		}
		return result;
	}

	public bool AllSurrendered()
	{
		foreach (Transform item in Container)
		{
			if (item.gameObject.activeInHierarchy)
			{
				return false;
			}
			CharacterBody component = item.GetComponent<CharacterBody>();
			if (!component.IsRagdoll)
			{
			}
		}
		return true;
	}

	public void RegisterBody(CharacterBody body)
	{
		bodies.Add(body);
	}

	public void RegisterOccupantCharacter(CharacterBody occupant)
	{
		if (!occupants.ContainsValue(occupant))
		{
			occupants.Add(occupant.name, occupant);
		}
	}

	public CharacterBody OccupantByName(string name)
	{
		if (occupants.ContainsKey(name))
		{
			return occupants[name];
		}
		return null;
	}
}

}

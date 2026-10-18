using Crisis.Runtime.Weapons;

namespace Crisis.Runtime.FullSource
{
using UnityEngine;

public class BallisticObstacle : MonoBehaviour
{
	private float interactionTimestamp;

	public CharacterBody Interactor { get; private set; }

	public bool QueryInteraction(CharacterBody interactor)
	{
		if (Time.time - interactionTimestamp >= 3f)
		{
			Interactor = null;
		}
		if (interactor == Interactor)
		{
			return true;
		}
		if (Interactor != null)
		{
			return false;
		}
		Interactor = interactor;
		interactionTimestamp = Time.time;
		return true;
	}

	public void Remove(CharacterBody interactor)
	{
		if (interactor.IsPlayer || interactor == Interactor)
		{
			Object.Destroy(base.gameObject);
		}
	}
}

}

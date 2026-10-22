namespace Crisis.Runtime.FullSource
{
using UnityEngine;

public class TeamTutorialMarksmanTarget : MonoBehaviour
{
	public Target target;

	private Collider materialCollider;

	public float LastHitTimestamp { get; private set; }

	private void Start()
	{
		BallisticMaterial component = GetComponent<BallisticMaterial>();
		component.onInteractionBegin.AddListener((Projectile p) =>
		{
			OnHit(p);
		});
		materialCollider = GetComponent<Collider>();
	}

	public void OnHit(Projectile projectile)
	{
		LastHitTimestamp = Time.time;
		if (target != null)
		{
			target.active = false;
		}
	}
}

}

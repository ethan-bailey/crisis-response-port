using Object = UnityEngine.Object;
using UnityEngine;

public class DecayingObject : MonoBehaviour
{
	[Tooltip("The time in seconds before the object is destroyed.")]
	public float lifeTime = 1f;

	[Tooltip("Should the count down start directly?")]
	public bool activateOnStart = true;

	private bool active;

	private float startTime = -100f;

	private void Start()
	{
		if (activateOnStart)
		{
			Activate();
		}
	}

	public void Activate()
	{
		active = true;
		startTime = Time.time;
	}

	private void Update()
	{
		if (active && Time.time - startTime >= lifeTime)
		{
			Object.Destroy(base.gameObject);
		}
	}
}

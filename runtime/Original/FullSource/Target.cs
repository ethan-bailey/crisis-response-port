using Crisis.Runtime.Weapons;

namespace Crisis.Runtime.FullSource
{
using UnityEngine;

public class Target : MonoBehaviour
{
	[Tooltip("This flag indicates whether the threat is active.")]
	public bool active = true;

	public CharacterBody Body { get; private set; }

	private void Awake()
	{
		Body = GetComponent<CharacterBody>();
	}
}

}

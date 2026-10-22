namespace Crisis.Runtime.FullSource
{
using UnityEngine;

public class AudioInstigator : MonoBehaviour
{
	public CharacterBody Body { get; private set; }

	private void Awake()
	{
		Body = GetComponent<CharacterBody>();
	}
}

}

namespace Crisis.Runtime.FullSource
{
using UnityEngine;

public class Interaction : MonoBehaviour
{
	public float range = 1f;

	public string description;

	public virtual void Invoke(CharacterBody body)
	{
	}

	public virtual bool Available(CharacterBody body)
	{
		return Mathf.Abs(base.transform.position.x - body.Position) <= range;
	}
}

}

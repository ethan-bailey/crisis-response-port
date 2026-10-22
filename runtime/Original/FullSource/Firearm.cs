namespace Crisis.Runtime.FullSource
{
using UnityEngine;

public class Firearm : MonoBehaviour
{
	public enum Action
	{
		ShortRecoil = 0
	}

	public Action action;

	public virtual void ReleaseStriker()
	{
	}

	public virtual bool LockOpen()
	{
		return false;
	}
}

}

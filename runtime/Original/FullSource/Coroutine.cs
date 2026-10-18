namespace Crisis.Runtime.FullSource
{
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public static class Coroutine
{
	public static void DelayedAction(float delay, UnityAction action, MonoBehaviour behavior = null)
	{
		if (behavior != null)
		{
			behavior.StartCoroutine(DelayedCallback(delay, action));
			return;
		}
		GlobalContext context = Global.Context;
		if (context != null)
		{
			Global.Context.StartCoroutine(DelayedCallback(delay, action));
		}
		else
		{
			Debug.LogWarning("Can't schedule delayed action without a host behaviour.");
		}
	}

	private static IEnumerator DelayedCallback(float delay, UnityAction callback)
	{
		yield return new WaitForSeconds(delay);
		if (callback != null)
		{
			callback();
		}
	}
}

}

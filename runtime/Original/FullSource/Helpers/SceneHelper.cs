using UnityEngine;

namespace Helpers
{
	internal class SceneHelper
	{
		public static string GetFullScenePath(GameObject obj)
		{
			string text = obj.name;
			while (obj.transform.parent != null)
			{
				obj = obj.transform.parent.gameObject;
				text = "/" + obj.name + text;
			}
			return text;
		}
	}
}

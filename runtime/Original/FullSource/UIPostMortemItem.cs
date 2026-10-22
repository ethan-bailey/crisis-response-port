namespace Crisis.Runtime.FullSource
{
using UnityEngine;
using UnityEngine.UI;

public class UIPostMortemItem : MonoBehaviour
{
	public Text headerText;

	public void Initialize(CharacterBody body, Transform parent)
	{
		base.transform.SetParent(parent);
		base.transform.localPosition = Vector3.zero;
		base.transform.localScale = Vector3.one;
		base.name = body.name;
		string text = body.name + ": ";
		int count = body.HitCount.Count;
		bool flag = count > 0;
		if (flag)
		{
			text += string.Format("{0} hit{1}", count, (count != 1) ? "s" : string.Empty);
		}
		Physiology physiology = body.Physiology;
		text += string.Format("{0}{1}", (!flag) ? string.Empty : ", ", (!physiology.Alive) ? "Expired" : "Alive");
		headerText.text = text;
	}
}

}

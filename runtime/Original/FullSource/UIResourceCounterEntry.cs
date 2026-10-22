namespace Crisis.Runtime.FullSource
{
using UnityEngine;
using UnityEngine.UI;

public class UIResourceCounterEntry : MonoBehaviour
{
	public Text labelText;

	public Text valueText;

	private int max;

	private bool limited;

	private bool colorize;

	public void Initialize(string label, int max, Transform parent, bool colorize)
	{
		labelText.text = label;
		base.transform.SetParent(parent);
		base.transform.localScale = Vector3.one;
		this.max = max;
		this.colorize = colorize;
		limited = max > 0;
	}

	public void SetValue(int value)
	{
		if (limited)
		{
			valueText.text = string.Format("{0}/{1}", value, max);
			if (colorize)
			{
				valueText.color = Color.Lerp(Color.white, Color.red, (float)value / (float)max);
			}
		}
		else
		{
			valueText.text = string.Format("{0}", value);
		}
	}
}

}

namespace Crisis.Runtime.FullSource
{
using UnityEngine;
using UnityEngine.EventSystems;

public class UITooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IEventSystemHandler
{
	[TextArea(5, 10)]
	public string text;

	public float delay = 0.5f;

	protected UITooltipManager manager;

	public bool IsVisible
	{
		get
		{
			return manager != null && manager.Current == this;
		}
	}

	private void Start()
	{
		manager = Global.Context.Tooltip;
	}

	private void OnDisable()
	{
		if (IsVisible)
		{
			manager.Hide();
		}
	}

	public void Set(string text)
	{
		this.text = text;
		if (IsVisible)
		{
			manager.Set(this);
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (manager != null)
		{
			manager.Set(this);
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (manager != null)
		{
			manager.Hide();
		}
	}
}

}

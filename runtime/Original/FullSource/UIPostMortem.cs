namespace Crisis.Runtime.FullSource
{
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIPostMortem : MonoBehaviour
{
	public Button doneButton;

	public Transform itemContainer;

	public UIPostMortemItem itemPrefab;

	private UnityAction onDone;

	private void Start()
	{
		doneButton.onClick.AddListener(OnDoneButtonClicked);
	}

	public void Show(List<CharacterBody> bodies, UnityAction onDoneCallback)
	{
		onDone = onDoneCallback;
		foreach (CharacterBody body in bodies)
		{
			UIPostMortemItem uIPostMortemItem = Object.Instantiate(itemPrefab);
			uIPostMortemItem.Initialize(body, itemContainer);
		}
		base.gameObject.SetActive(true);
	}

	private void OnDoneButtonClicked()
	{
		base.gameObject.SetActive(false);
		if (onDone != null)
		{
			onDone();
		}
	}
}

}

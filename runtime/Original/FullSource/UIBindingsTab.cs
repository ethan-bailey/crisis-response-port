namespace Crisis.Runtime.FullSource
{
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBindingsTab : MonoBehaviour
{
	public UIBindingCategoryHeader bindingCategoryPrefab;

	public UIBindingItem bindingItemPrefab;

	public Transform bindingsContainer;

	private Text hintText;

	private UIBindingOverlay bindingOverlay;

	private InputHandler inputHandler;

	private float enabledTimestamp;

	private bool initialized;

	private void OnEnable()
	{
		inputHandler = Global.Context.InputHandler;
		Deserialize(Global.Context.Settings.Controls);
		enabledTimestamp = Time.realtimeSinceStartup;
		if (bindingOverlay != null)
		{
			bindingOverlay.StartListening();
		}
		initialized = true;
	}

	private void OnDisable()
	{
		if (bindingOverlay != null)
		{
			bindingOverlay.StopListening();
		}
		Save();
	}

	private void Start()
	{
		inputHandler.onConflictUpdate.AddListener(UpdateItems);
	}

	public void Initialize(UISettingsPanel settingsPanel, UIBindingOverlay bindingOverlay)
	{
		hintText = settingsPanel.ribbonText;
		this.bindingOverlay = bindingOverlay;
	}

	private void Save()
	{
		if (initialized)
		{
			Global.Context.Settings.Controls.Save();
		}
	}

	private void Deserialize(InputMapping data)
	{
		if (bindingItemPrefab == null || bindingsContainer == null)
		{
			return;
		}
		Clear();
		foreach (KeyValuePair<InputKey.Category, List<InputKey>> category in data.Categories)
		{
			UIBindingCategoryHeader uIBindingCategoryHeader = Object.Instantiate(bindingCategoryPrefab);
			uIBindingCategoryHeader.text.text = category.Key.ToString();
			uIBindingCategoryHeader.transform.SetParent(bindingsContainer, false);
			foreach (InputKey item in category.Value)
			{
				UIBindingItem uIBindingItem = Object.Instantiate(bindingItemPrefab);
				uIBindingItem.name = item.name;
				uIBindingItem.transform.SetParent(bindingsContainer, false);
				uIBindingItem.Initialize(inputHandler, item);
			}
		}
	}

	private void Clear()
	{
		foreach (Transform item in bindingsContainer.transform)
		{
			Object.Destroy(item.gameObject);
		}
	}

	public UIBindingItem FindBindingUIItemByInputName(string inputName)
	{
		foreach (Transform item in bindingsContainer.transform)
		{
			UIBindingItem component = item.GetComponent<UIBindingItem>();
			if (component == null || component.inputKey == null || component.inputKey.name != inputName)
			{
				continue;
			}
			return component;
		}
		return null;
	}

	public void UpdateItems(List<InputKey> inputs)
	{
		foreach (InputKey input in inputs)
		{
			UIBindingItem uIBindingItem = FindBindingUIItemByInputName(input.name);
			if (!(uIBindingItem == null))
			{
				uIBindingItem.UpdateTexts();
			}
		}
	}
}

}

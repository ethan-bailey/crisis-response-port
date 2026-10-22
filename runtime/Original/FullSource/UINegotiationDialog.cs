namespace Crisis.Runtime.FullSource
{
using UnityEngine;
using UnityEngine.UI;

public class UINegotiationDialog : MonoBehaviour
{
	[Header("References")]
	public NegotiationMode negotiation;

	public UINegotiationOption optionPrefab;

	public Text occupantNameText;

	public Text lastNegotiatorLine;

	public Text lastOccupantLine;

	public Transform optionsContainer;

	[Header("Navigation")]
	public Button next;

	public Button previous;

	private void OnEnable()
	{
		Reset();
		next.onClick.AddListener(negotiation.Next);
		previous.onClick.AddListener(negotiation.Previous);
		negotiation.eventRefreshUI.AddListener(RefreshUI);
	}

	private void Start()
	{
		RefreshUI();
	}

	private void Reset()
	{
		foreach (Transform item in optionsContainer)
		{
			Object.Destroy(item.gameObject);
		}
	}

	public void RefreshUI()
	{
		Reset();
		CharacterBody currentOccupant = negotiation.CurrentOccupant;
		occupantNameText.text = currentOccupant.name;
		if (currentOccupant.Dialog.enabled)
		{
			SerializableDialogState currentState = currentOccupant.Dialog.GetCurrentState();
			if (currentState == null)
			{
				return;
			}
			lastNegotiatorLine.text = "(You have not said anything prior to this)";
			lastOccupantLine.text = currentState.phrase;
			int num = 1;
			{
				foreach (SerializableDialogOption option in currentState.options)
				{
					UINegotiationOption uINegotiationOption = Object.Instantiate(optionPrefab);
					uINegotiationOption.name = string.Format("Option {0}", num++);
					uINegotiationOption.Initialize(currentOccupant.Dialog, option, this);
					uINegotiationOption.transform.SetParent(optionsContainer, false);
				}
				return;
			}
		}
		lastNegotiatorLine.text = string.Empty;
		lastOccupantLine.text = string.Empty;
		UINegotiationOption uINegotiationOption2 = Object.Instantiate(optionPrefab);
		uINegotiationOption2.name = "Null option";
		uINegotiationOption2.text.text = "No options available";
		uINegotiationOption2.transform.SetParent(optionsContainer, false);
	}
}

}

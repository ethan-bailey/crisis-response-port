namespace Crisis.Runtime.FullSource
{
using UnityEngine;
using UnityEngine.UI;

public class UINegotiation : MonoBehaviour
{
	[Header("References")]
	public NegotiationMode negotiation;

	public GameObject openCommunicationPanel;

	public UINegotiationDialog dialog;

	public GameObject occupantsPanel;

	public GameObject peacefulEndPanel;

	public AudioSource audioSource;

	[Header("Navigation")]
	public Button next;

	public Button previous;

	public Button mobilizeEntryTeam;

	public Button buttonDeliverPhone;

	public Button buttonCallPhone;

	public Button buttonEndCall;

	[Header("Modifiers")]
	public Button incrementMorale;

	public Button decrementMorale;

	public Button incrementAggression;

	public Button decrementAggression;

	public Button incrementFear;

	public Button decrementFear;

	public Button surrender;

	[Header("Presentation")]
	public Text occupantName;

	public Image occupantPortrait;

	public Slider morale;

	public Slider aggression;

	public Slider fear;

	public Text occupantSurrenderText;

	[Header("Phone")]
	public AudioClip outgoingPhoneRing;

	private bool cursorWasVisible;

	private float lastRingTimestamp;

	private void OnEnable()
	{
		Reset();
		next.onClick.AddListener(negotiation.Next);
		previous.onClick.AddListener(negotiation.Previous);
		incrementMorale.onClick.AddListener(negotiation.IncrementMorale);
		decrementMorale.onClick.AddListener(negotiation.DecrementMorale);
		incrementAggression.onClick.AddListener(negotiation.IncrementAggression);
		decrementAggression.onClick.AddListener(negotiation.DecrementAggression);
		incrementFear.onClick.AddListener(negotiation.IncrementFear);
		decrementFear.onClick.AddListener(negotiation.DecrementFear);
		surrender.onClick.AddListener(negotiation.Surrender);
		mobilizeEntryTeam.onClick.AddListener(negotiation.MobilizeEntryTeam);
		buttonDeliverPhone.onClick.AddListener(negotiation.MobilizeEntryTeam);
		buttonCallPhone.onClick.AddListener(StartCallPhone);
		buttonEndCall.onClick.AddListener(EndPhoneCall);
		negotiation.eventRefreshUI.AddListener(RefreshUI);
		negotiation.eventPeacefulEnd.AddListener(OnPeacefulEnd);
		cursorWasVisible = Cursor.visible;
		Cursor.visible = true;
	}

	private void OnDisable()
	{
		Cursor.visible = cursorWasVisible;
	}

	private void Start()
	{
		buttonDeliverPhone.gameObject.SetActive(Global.Context.CampaignManager.MissionParameters.phoneDropOffEnabled);
		RefreshUI();
	}

	private void Update()
	{
		if (negotiation.PhoneRinging)
		{
			float num = Time.time - lastRingTimestamp;
			if (num >= CellPhone.RingInterval)
			{
				lastRingTimestamp = Time.time;
				audioSource.PlayOneShot(outgoingPhoneRing);
			}
		}
	}

	private void Reset()
	{
		next.onClick.RemoveAllListeners();
		previous.onClick.RemoveAllListeners();
		incrementMorale.onClick.RemoveAllListeners();
		decrementMorale.onClick.RemoveAllListeners();
		incrementAggression.onClick.RemoveAllListeners();
		decrementAggression.onClick.RemoveAllListeners();
		incrementFear.onClick.RemoveAllListeners();
		decrementFear.onClick.RemoveAllListeners();
		surrender.onClick.RemoveAllListeners();
		mobilizeEntryTeam.onClick.RemoveAllListeners();
	}

	private void RefreshUI()
	{
		bool flag = !negotiation.CurrentOccupant.gameObject.activeInHierarchy;
		occupantName.text = string.Format("{0}", negotiation.CurrentOccupant.name);
		morale.value = negotiation.CurrentOccupant.Blackboard.morale;
		aggression.value = negotiation.CurrentOccupant.Blackboard.aggression;
		fear.value = negotiation.CurrentOccupant.Blackboard.fear;
		surrender.interactable = !flag;
		surrender.GetComponent<UIButton>().disablePointerEvents = flag;
		occupantSurrenderText.text = ((!flag) ? string.Format("Have {0} surrender peacefully", negotiation.CurrentOccupant.name) : string.Format("{0} has surrendered", negotiation.CurrentOccupant.name));
	}

	private void OnPeacefulEnd()
	{
		occupantsPanel.SetActive(false);
		peacefulEndPanel.SetActive(true);
	}

	public void OnPhoneDelivered()
	{
		buttonDeliverPhone.gameObject.SetActive(false);
		buttonCallPhone.gameObject.SetActive(true);
	}

	private void StartCallPhone()
	{
		negotiation.StartCallingPhone();
		buttonCallPhone.gameObject.SetActive(false);
		buttonEndCall.gameObject.SetActive(true);
	}

	private void EndPhoneCall()
	{
		negotiation.EndPhoneCall();
		buttonCallPhone.gameObject.SetActive(true);
		buttonEndCall.gameObject.SetActive(false);
	}

	public void OnPhoneAnswered()
	{
		audioSource.Stop();
		buttonCallPhone.gameObject.SetActive(false);
		openCommunicationPanel.SetActive(false);
		dialog.RefreshUI();
		dialog.gameObject.SetActive(true);
	}
}

}

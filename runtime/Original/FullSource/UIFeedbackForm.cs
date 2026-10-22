namespace Crisis.Runtime.FullSource
{
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIFeedbackForm : MonoBehaviour
{
	public enum Status
	{
		Pending = 0,
		Sent = 1,
		Cancelled = 2
	}

	[Header("Length limits")]
	public int messageRemainingWarning = 100;

	public int contactRemainingWarning = 10;

	[Header("References")]
	public Button sendButton;

	public Button cancelButton;

	public InputField messageInput;

	public Text messageInputHint;

	public Text messageRemainingText;

	public InputField contactInput;

	public Text contactInputHint;

	public Text contactRemainingText;

	public Text serverMessageText;

	private UnityAction onCloseCallback;

	private const int MaxMessageLength = 1024;

	private const int MaxContactLength = 64;

	private bool initialized;

	private bool waiting;

	private int waitState;

	private float waitTimestamp;

	public bool Visible
	{
		get
		{
			return base.gameObject.activeInHierarchy;
		}
	}

	public Status CurrentStatus { get; private set; }

	private void Awake()
	{
		sendButton.onClick.RemoveAllListeners();
		sendButton.onClick.AddListener(OnPressedSend);
		cancelButton.onClick.RemoveAllListeners();
		cancelButton.onClick.AddListener(OnPressedCancel);
		messageInput.characterLimit = 1024;
		messageInput.onValueChanged.RemoveAllListeners();
		messageInput.onValueChanged.AddListener(OnMessageChanged);
		contactInput.characterLimit = 64;
		contactInput.onValueChanged.RemoveAllListeners();
		contactInput.onValueChanged.AddListener(OnContactChanged);
	}

	private void OnEnable()
	{
		EnableInput(false, string.Empty);
		UpdateCharactersRemainingText(messageRemainingText, messageInput.text.Length, 1024, messageRemainingWarning);
		UpdateCharactersRemainingText(contactRemainingText, contactInput.text.Length, 64, contactRemainingWarning);
		if (initialized)
		{
			Global.Context.Connection.PushRequest(new PostFeedbackQuery(Global.Context.ClientGUID, OnReceivedQueryResult));
			waiting = true;
			waitState = 0;
		}
	}

	private void Start()
	{
		Global.Context.Connection.PushRequest(new PostFeedbackQuery(Global.Context.ClientGUID, OnReceivedQueryResult));
		initialized = true;
		waiting = true;
		waitState = 0;
	}

	private void Update()
	{
		if (waiting && Time.time - waitTimestamp >= 1f)
		{
			waitTimestamp = Time.time;
			string text = "Waiting for server";
			ShowStatus(text.PadRight(18 + waitState, '.'));
			waitState++;
			if (waitState > 5)
			{
				waitState = 0;
			}
		}
	}

	public void Show(UnityAction onClosedCallback)
	{
		onCloseCallback = onClosedCallback;
		base.gameObject.SetActive(true);
		CurrentStatus = Status.Pending;
	}

	public void Close(Status status)
	{
		CurrentStatus = status;
		base.gameObject.SetActive(false);
		if (onCloseCallback != null)
		{
			onCloseCallback();
		}
	}

	private void OnReceivedQueryResult(HTTPRequest request)
	{
		waiting = false;
		if (request.StatusOK)
		{
			EnableInput(true, string.Empty);
		}
		else if (request.TimedOut)
		{
			EnableInput(false, "Can't send feedback.\nReason: No server contact.");
			Debug.LogWarning("Can't send feedback.\nReason: No server contact.");
		}
		else if (request.responseData == "Deny")
		{
			EnableInput(false, "You must wait before you can send feedback again.");
			Debug.Log("You must wait before you can send feedback again.");
		}
		else
		{
			EnableInput(false, "Can't send feedback.\nReason: Server error.");
			Debug.LogWarning("Can't send feedback.\nReason: Server error.");
		}
		request = null;
	}

	private void OnPressedSend()
	{
		Global.Context.Connection.PushRequest(new PostFeedback(Global.Context.ClientGUID, Version.Instance.VersionStringWithTags, messageInput.text, contactInput.text, OnReceivedSendResult));
		Debug.Log("Send feedback.");
		Close(Status.Sent);
	}

	private void OnReceivedSendResult(HTTPRequest request)
	{
		if (request.TimedOut)
		{
			Global.Context.MessageBox.ShowOK("The server didn't respond to your request.\nYou can try again later.");
			Debug.LogWarning("The server didn't respond to your request.\nYou can try again later.");
			return;
		}
		if (request.EmptyOrError)
		{
			Global.Context.MessageBox.ShowOK("The server had an error while handling your message.");
			Debug.LogWarning("The server had an error while handling your message.");
			return;
		}
		messageInput.text = string.Empty;
		contactInput.text = string.Empty;
		Global.Context.MessageBox.ShowOK("Thank you for your feedback!\n\nMessage receipt: " + request.responseData);
		Debug.LogFormat("Feedback received by server. Message receipt: {0}", request.responseData);
	}

	private void OnPressedCancel()
	{
		Close(Status.Cancelled);
	}

	private void OnMessageChanged(string value)
	{
		UpdateCharactersRemainingText(messageRemainingText, value.Length, 1024, messageRemainingWarning);
		UpdateEmptyMessageHint();
	}

	private void UpdateEmptyMessageHint()
	{
		ShowStatus((messageInput.text.Length != 0) ? string.Empty : "You must enter a message before you can send.");
		sendButton.interactable = messageInput.text.Length > 0;
	}

	private void OnContactChanged(string value)
	{
		UpdateCharactersRemainingText(contactRemainingText, value.Length, 64, contactRemainingWarning);
	}

	private void UpdateCharactersRemainingText(Text text, int currentLength, int maxLength, int warningThreshold)
	{
		int num = maxLength - currentLength;
		text.text = string.Format("{0}", num);
		text.gameObject.SetActive(num < warningThreshold);
	}

	private void EnableInput(bool enable, string disabledMessage = "")
	{
		sendButton.interactable = enable;
		messageInput.interactable = enable;
		messageInputHint.text = ((!enable) ? string.Empty : "Enter your message here");
		contactInput.interactable = enable;
		contactInputHint.text = ((!enable) ? string.Empty : "Enter where I can reach you (optional)");
		serverMessageText.enabled = !enable;
		serverMessageText.text = disabledMessage;
		serverMessageText.color = Color.white;
		if (enable)
		{
			messageInput.Select();
			messageInput.ActivateInputField();
			UpdateEmptyMessageHint();
		}
	}

	private void ShowStatus(string message)
	{
		serverMessageText.color = Color.gray;
		serverMessageText.enabled = true;
		serverMessageText.text = message;
	}
}

}

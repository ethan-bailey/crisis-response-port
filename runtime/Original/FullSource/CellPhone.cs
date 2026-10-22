namespace Crisis.Runtime.FullSource
{
using UnityEngine;
using UnityEngine.Events;

public class CellPhone : MonoBehaviour
{
	public class CellPhoneAnswerEvent : UnityEvent<CharacterBody>
	{
	}

	public AudioClip ring;

	public static float RingInterval = 5f;

	private CellPhoneAnswerEvent onAnswered = new CellPhoneAnswerEvent();

	private AudioSource audioSource;

	private float startRingTimestamp;

	private float lastRingTimestamp;

	private float answerDelay;

	public bool Ringing { get; private set; }

	public void Initialize(Vector3 position, UnityAction<CharacterBody> onAnsweredCallback)
	{
		audioSource = GetComponent<AudioSource>();
		onAnswered.AddListener(onAnsweredCallback);
		base.transform.position = position;
		base.transform.LookAt(base.transform.position + Random.insideUnitSphere);
		answerDelay = Global.Context.CampaignManager.MissionParameters.phoneAnswerDelay;
	}

	private void Update()
	{
		if (Ringing)
		{
			float num = Time.time - lastRingTimestamp;
			if (num >= RingInterval)
			{
				lastRingTimestamp = Time.time;
				audioSource.PlayOneShot(ring);
			}
			float num2 = Time.time - startRingTimestamp;
			if (num2 >= answerDelay)
			{
				string text = Global.Context.CampaignManager.MissionParameters.phoneAnswerer.name;
				Pickup(Global.Context.Mission.Occupants.OccupantByName(text));
			}
		}
	}

	public void StartRinging()
	{
		Ringing = true;
		startRingTimestamp = (lastRingTimestamp = Time.time);
	}

	public void StopRinging()
	{
		Ringing = false;
		audioSource.Stop();
	}

	public void Pickup(CharacterBody body)
	{
		StopRinging();
		onAnswered.Invoke(body);
	}
}

}

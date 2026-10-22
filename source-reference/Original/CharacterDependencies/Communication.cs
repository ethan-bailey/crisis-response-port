using System.Collections.Generic;
using UnityEngine;

public class Communication : MonoBehaviour
{
	[Tooltip("Seconds to display per character text.")]
	public float textTimeFactor = 0.1f;

	[Tooltip("How fast the text appears in seconds per character.")]
	public float textAppearSpeed = 0.01f;

	[Header("Commands")]
	public List<CommunicationLine> moveThere;

	public List<CommunicationLine> moveOut;

	public List<CommunicationLine> followMe;

	public List<CommunicationLine> holdPosition;

	[Header("Affermatives")]
	public List<CommunicationLine> affermative;

	public List<CommunicationLine> going;

	[Header("General")]
	public List<CommunicationLine> reloading;

	public List<CommunicationLine> coverMe;

	private CharacterBody body;

	private AudioSource audioSource;

	private InteriorModeUI interiorUI;

	private UIVocalizationText vocalizationUI;

	private int charactersMask;

	private string vocalizationBuffer;

	private float vocalizationRemoveTime;

	private List<float> vocalizationTimestamps = new List<float>();

	private float vocalizationSpeed = 0.015f;

	private float lastCharacterTimestamp;

	public bool Vocalizing { get; private set; }

	public string VocalizedText { get; private set; }

	private void Start()
	{
		body = GetComponent<CharacterBody>();
		charactersMask = Masks.Instance.Characters;
		audioSource = body.Audio.communicationAudio;
		if (Global.Context.Mission.InteriorMode != null)
		{
			interiorUI = Global.Context.Mission.InteriorMode.UI;
			vocalizationUI = interiorUI.CreateCharacterVocalizationUI(body);
		}
	}

	public void PlayPhrase(List<CommunicationLine> phrase, float delay = 0.5f)
	{
		if (delay > 0f)
		{
			Coroutine.DelayedAction(delay, () =>
			{
				PlayRandomLine(phrase);
			}, this);
		}
		else
		{
			PlayRandomLine(phrase);
		}
	}

	private void PlayRandomLine(List<CommunicationLine> lines)
	{
		if (lines.Count != 0)
		{
			CommunicationLine line = lines[Random.Range(0, lines.Count)];
			PlayLine(line);
		}
	}

	private void PlayLine(CommunicationLine line)
	{
		if (line.clip != null && line.clip.loadState == AudioDataLoadState.Loaded)
		{
			audioSource.PlayOneShot(line.clip);
		}
	}

	private void Update()
	{
		if (body.IsRagdoll && audioSource.isPlaying)
		{
			audioSource.Stop();
		}
		if (Vocalizing)
		{
			UpdateText();
		}
	}

	private void UpdateText()
	{
		for (int num = vocalizationTimestamps.Count - 1; num >= 0; num--)
		{
			if (Time.time - vocalizationTimestamps[num] >= 3f)
			{
				vocalizationTimestamps.RemoveAt(num);
			}
		}
		if (vocalizationBuffer.Length > 0 && Time.time - lastCharacterTimestamp >= vocalizationSpeed)
		{
			VocalizedText += vocalizationBuffer[0];
			vocalizationBuffer = vocalizationBuffer.Remove(0, 1);
			vocalizationUI.SetText(VocalizedText, vocalizationTimestamps.Count > 2);
			lastCharacterTimestamp = Time.time;
		}
		if (Time.time >= vocalizationRemoveTime)
		{
			Vocalizing = false;
			VocalizedText = string.Empty;
			vocalizationUI.Hide();
		}
	}

	public void VocalizeText(string text)
	{
		Vocalizing = true;
		vocalizationBuffer = text;
		VocalizedText = string.Empty;
		vocalizationRemoveTime = Time.time + (float)text.Length * vocalizationSpeed * 5f;
		vocalizationTimestamps.Add(Time.time);
	}
}

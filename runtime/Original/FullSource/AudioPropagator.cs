namespace Crisis.Runtime.FullSource
{
using System.Collections.Generic;
using UnityEngine;

public sealed class AudioPropagator
{
	private static AudioPropagator instance = null;

	private List<AudioStimuli> sources;

	private List<AudioStimuli> pending;

	public const float WalkAudioPower = 15f;

	public const float SprintAudioPower = 45f;

	public const float SpeedOfSound = 343f;

	public const float AudioSourceLifeTime = 0.41f;

	public const float MaxAudioPropagationDistance = 50f;

	public const float ObstacleFactor = 0.6f;

	private const float ObstacleOffsetMagnitude = 0.25f;

	private const float UpdateInterval = 0.1f;

	private static readonly object padlock = new object();

	private static int collisionMask;

	private float lastUpdate;

	public static AudioPropagator Instance
	{
		get
		{
			lock (padlock)
			{
				if (instance == null)
				{
					instance = new AudioPropagator();
				}
				return instance;
			}
		}
	}

	private AudioPropagator()
	{
		sources = new List<AudioStimuli>();
		pending = new List<AudioStimuli>();
		collisionMask = Masks.Instance.BallisticTargetsSansLimbs;
	}

	public void Update()
	{
		if (!(Time.time - lastUpdate < 0.1f))
		{
			lastUpdate = Time.time;
			CullDecayedSources();
			if (pending.Count > 0)
			{
				sources.InsertRange(0, pending);
				pending.Clear();
			}
		}
	}

	public void Restart()
	{
		sources.Clear();
		pending.Clear();
	}

	public void Emit(float position, float power, AudioStimuliType type, AudioInstigator instigator)
	{
		if (!(instigator == null) && (!(instigator.Body != null) || !instigator.Body.Invisible))
		{
			pending.Insert(0, new AudioStimuli(Time.time, position, power, type, instigator));
		}
	}

	public List<AudioStimuli> Perceivable(CharacterBody listener)
	{
		List<AudioStimuli> list = new List<AudioStimuli>();
		foreach (AudioStimuli source in sources)
		{
			if (IsPerceivable(source, listener))
			{
				list.Add(source);
			}
		}
		return list;
	}

	public bool IsPerceivable(AudioStimuli source, CharacterBody listener)
	{
		if (source.instigator == listener)
		{
			return false;
		}
		if (Time.time < source.timestamp + 0.16f)
		{
			return false;
		}
		if (source.instigator != null && source.instigator.Body != null && source.instigator.Body.team == listener.team)
		{
			return false;
		}
		if (source.power < listener.LowestDetectableAudio)
		{
			return false;
		}
		float x = listener.Head.transform.position.x;
		float num = Mathf.Sign(source.position - x);
		Room room = listener.Room;
		int num2 = 0;
		while (room != null)
		{
			DoorController doorController = room.NextDoor(num);
			if (doorController == null || num != Mathf.Sign(source.position - doorController.Position))
			{
				break;
			}
			if (doorController.Angle < 10f)
			{
				num2++;
			}
			room = room.NextRoom(num);
		}
		return Detectable(listener.LowestDetectableAudio, source.power, Mathf.Abs(source.position - x), num2);
	}

	public static float LocomotionAudioPower(float normalizedVelocity)
	{
		return Mathf.Min(15f + 45f * normalizedVelocity, 45f);
	}

	public static bool Detectable(float lowestDetectable, float sourcePower, float distance, int blockingDoors)
	{
		float num = ((blockingDoors != 0) ? Mathf.Pow(12f, (float)blockingDoors * 0.5f) : 0f);
		float num2 = sourcePower - Mathf.Pow(distance, 2f) - num;
		return num2 >= lowestDetectable;
	}

	private void CullDecayedSources()
	{
		if (sources.Count != 0)
		{
			int num = 0;
			int num2 = sources.Count;
			while (num2 > 0 && Time.time >= sources[num2 - 1].timestamp + 0.41f)
			{
				num++;
				num2--;
			}
			if (num != 0)
			{
				sources.RemoveRange(num2, num);
			}
		}
	}
}

}

namespace Crisis.Runtime.FullSource
{
using System;
using System.Collections.Generic;
using UnityEngine;

public class PoweredLight : MonoBehaviour
{
	[Serializable]
	public class MaterialLightPair
	{
		public string name = "Material";

		public Renderer renderer;

		public int materialIndex;

		public Color litEmissive = Color.white;

		public Color unlitEmissive = Color.black;
	}

	[Header("Appearance")]
	public List<Light> sources = new List<Light>();

	[Tooltip("Renderers and their materials that should switch between emissive and non-emissive material.")]
	public List<MaterialLightPair> litMaterials = new List<MaterialLightPair>();

	[Tooltip("The probability that the light will emit a humming sound.")]
	public float humChance;

	public bool disableShadows;

	[Header("States")]
	public bool switchedOff;

	public bool unpowered;

	public bool broken;

	public bool indestructible;

	public static float ReflectionProbeInfluenceRange = 5f;

	private Room room;

	private AudioSource audioSource;

	private bool emitHumAudio;

	public bool IsOn { get; private set; }

	public bool IsSwitchedOn
	{
		get
		{
			return !switchedOff;
		}
	}

	public bool IsPowered
	{
		get
		{
			return !unpowered;
		}
	}

	public bool IsBroken
	{
		get
		{
			return broken;
		}
	}

	private void Awake()
	{
		audioSource = GetComponent<AudioSource>();
		room = Global.Context.Mission.RoomAtPosition(base.transform.position.x);
		BallisticMaterial component = GetComponent<BallisticMaterial>();
		if (component != null)
		{
			component.onInteractionBegin.AddListener(OnBallisticHit);
		}
		IsOn = true;
		UpdateMaterial();
		emitHumAudio = UnityEngine.Random.value <= humChance;
		UpdateAudioState();
	}

	public void SetSwitchState(bool on)
	{
		switchedOff = !on;
		UpdateState();
	}

	public void SetPowerState(bool powered)
	{
		unpowered = !powered;
		UpdateState();
	}

	public void Break()
	{
		if (!indestructible)
		{
			broken = true;
			UpdateState();
		}
	}

	public void SetShadowsType(LightShadows type)
	{
		if (disableShadows)
		{
			return;
		}
		foreach (Light source in sources)
		{
			source.shadows = type;
		}
	}

	private void UpdateState()
	{
		bool isOn = IsOn;
		bool flag = !broken && !unpowered && !switchedOff;
		if (isOn != flag)
		{
			EnableLightSources(flag);
			IsOn = flag;
			UpdateMaterial();
			UpdateAudioState();
			UpdateRoomLighting();
		}
	}

	private void UpdateMaterial()
	{
		foreach (MaterialLightPair litMaterial in litMaterials)
		{
			Material[] materials = litMaterial.renderer.materials;
			Material material = materials[litMaterial.materialIndex];
			material.SetColor("_EmissionColor", (!IsOn) ? litMaterial.unlitEmissive : litMaterial.litEmissive);
			litMaterial.renderer.materials = materials;
		}
	}

	private void EnableLightSources(bool enable)
	{
		foreach (Light source in sources)
		{
			source.enabled = enable;
		}
	}

	private void UpdateAudioState()
	{
		if (!(audioSource == null))
		{
			if (IsOn && emitHumAudio)
			{
				audioSource.Play();
			}
			else
			{
				audioSource.Stop();
			}
		}
	}

	private void UpdateRoomLighting()
	{
		if (!(room == null))
		{
			room.UpdateLightingForAI();
			room.RenderReflectionProbe();
			if (room.rightRoom != null && base.transform.position.x - room.rightRoom.RightEdge <= ReflectionProbeInfluenceRange)
			{
				room.rightRoom.RenderReflectionProbe();
			}
			if (room.leftRoom != null && room.leftRoom.LeftEdge - base.transform.position.x <= ReflectionProbeInfluenceRange)
			{
				room.leftRoom.RenderReflectionProbe();
			}
		}
	}

	private void OnBallisticHit(Projectile projectile)
	{
		Break();
	}
}

}

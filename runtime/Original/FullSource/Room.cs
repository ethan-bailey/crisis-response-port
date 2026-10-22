using Crisis.Runtime.Weapons;

namespace Crisis.Runtime.FullSource
{
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Room : MonoBehaviour
{
	public DoorController LeftDoor;

	public DoorController RightDoor;

	public Room leftRoom;

	public Room rightRoom;

	public bool occupantsCanSpawn;

	public bool exterior;

	public bool cleared;

	public FogOfWarElement fogOfWar;

	public ReflectionProbe reflectionProbe;

	public Transform exteriorGeometry;

	[Tooltip("Hide these renderers from the marksman view when the Extra is enabled.")]
	public List<Renderer> hideForMarksman = new List<Renderer>();

	[Header("Concussion modifier")]
	[Tooltip("How stun shockwaves are enhanced in this room.\nThe minimum of this and an explosion's (declined) value at a given position is used when applying concussion to characters in the room.")]
	public float concussionFactor = 0.5f;

	[Header("Electricity")]
	[Tooltip("List of powered appliances that are turned on/off according to the building's power state.")]
	public List<PoweredLight> poweredAppliances = new List<PoweredLight>();

	[Tooltip("Should the AI use flashlight in this room?")]
	public bool useFlashlight = true;

	[Tooltip("Enable this to make AI react to lighting changes as defined below. If disabled only the flag above will be used.")]
	public bool useDynamicFlashlight = true;

	[Tooltip("If this many light sources are disabled in the room the AI will use flashlights.")]
	public int disabledLightsCount;

	[Tooltip("When the required number of the light sources listed here are disabled the AI will use flashlights in this room.")]
	public List<Light> aiLightSources = new List<Light>();

	public FireAlarm fireAlarm;

	private bool initialized;

	private float volume;

	private static float ClearRoomDistanceTolerance = 0.5f;

	private int reflectionProbeRenderId;

	public float BackEdge { get; set; }

	public float LeftEdge { get; private set; }

	public float RightEdge { get; private set; }

	public float CenterCoord { get; private set; }

	public float WalkableRangeLeft { get; set; }

	public float WalkableRangeRight { get; private set; }

	public float Width { get; private set; }

	public float Depth { get; private set; }

	public bool HasBeenStunned { get; set; }

	public float ConcussionFactor
	{
		get
		{
			float num = ((!(LeftDoor != null)) ? 1f : (1f - Mathf.Pow(Mathf.Min(LeftDoor.Angle, 90f), 2f) / 8100f * 0.25f));
			float num2 = ((!(RightDoor != null)) ? 1f : (1f - Mathf.Pow(Mathf.Min(RightDoor.Angle, 90f), 2f) / 8100f * 0.25f));
			return Mathf.Max(0.5f, 75f / volume * num * num2);
		}
	}

	public void ConnectPoweredAppliance(PoweredLight appliance)
	{
		poweredAppliances.Add(appliance);
	}

	private void Awake()
	{
		Global.Context.Settings.Extras.onChanged.AddListener(OnExtrasChanged);
		LeftEdge = base.transform.position.x;
		RightEdge = base.transform.position.x + GetComponent<BoxCollider>().size.x;
		Width = RightEdge - LeftEdge;
		BoxCollider component = GetComponent<BoxCollider>();
		Depth = component.size.z;
		volume = component.size.x * component.size.y * component.size.z;
		CenterCoord = LeftEdge + Width * 0.5f;
	}

	private void Start()
	{
		if (!initialized)
		{
			DefaultInitialize();
		}
		OnExtrasChanged(Global.Context.Settings.Extras);
	}

	private void SetWalkableRange()
	{
		WalkableRangeLeft = LeftEdge;
		WalkableRangeRight = RightEdge;
		Room room = leftRoom;
		while (room != null)
		{
			WalkableRangeLeft = room.LeftEdge;
			room = room.leftRoom;
		}
		room = rightRoom;
		while (room != null)
		{
			WalkableRangeRight = room.RightEdge;
			room = room.rightRoom;
		}
	}

	public void DefaultInitialize()
	{
		SetDoors();
		SetWalkableRange();
		initialized = true;
	}

	private void OnExtrasChanged(SerializableExtraSettings newSettings)
	{
		HideExteriorWalls(newSettings.disableExteriorWalls);
	}

	private void HideExteriorWalls(bool hide)
	{
		if (Global.Context.Mission.TutorialMode != null && Global.Context.Mission.TutorialMode.Active)
		{
			return;
		}
		if (exteriorGeometry != null)
		{
			exteriorGeometry.gameObject.SetActive(!hide);
		}
		foreach (Renderer item in hideForMarksman)
		{
			item.enabled = !hide;
		}
	}

	public void RoomBuilderInitialize(Room leftRoom, DoorController leftDoor, float walkableRangeLeft, Room rightRoom, DoorController rightDoor, float walkableRangeRight)
	{
		this.leftRoom = leftRoom;
		this.rightRoom = rightRoom;
		LeftDoor = leftDoor;
		RightDoor = rightDoor;
		WalkableRangeLeft = walkableRangeLeft;
		WalkableRangeRight = walkableRangeRight;
		if (LeftDoor != null)
		{
			LeftDoor.SetRightRoom(this);
		}
		if (RightDoor != null)
		{
			RightDoor.SetLeftRoom(this);
		}
		if (leftDoor != null && leftRoom != null)
		{
			Vector3 vector = new Vector3(base.transform.position.x, 0.5f, leftDoor.HingePosition.z);
			Vector3 vector2 = new Vector3(base.transform.position.x, 0.5f, leftRoom.BackEdge);
			float magnitude = (vector2 - vector).magnitude;
			if (magnitude >= 0.9f)
			{
				leftDoor.openAngle = Random.Range(165f, 180f);
			}
			else
			{
				leftDoor.openAngle = 180f - Mathf.Acos(magnitude / 0.9f) * 57.29578f;
			}
		}
		initialized = true;
	}

	public void Clear(float direction, float position)
	{
		if (!cleared && ((Mathf.Sign(direction) == 1f && position >= RightEdge - ClearRoomDistanceTolerance) || (Mathf.Sign(direction) == -1f && position <= LeftEdge + ClearRoomDistanceTolerance)))
		{
			cleared = true;
		}
	}

	private void SetDoors()
	{
		Ray ray = new Ray(new Vector3(LeftEdge - 0.5f, 1f, -1.5f), Vector3.right);
		RaycastHit[] array = Physics.RaycastAll(ray, Width + 1f, Masks.Instance.Interactables);
		RaycastHit[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			RaycastHit raycastHit = array2[i];
			if (RightDoor != null && LeftDoor != null)
			{
				break;
			}
			if (raycastHit.collider == null)
			{
				continue;
			}
			DoorController component = raycastHit.collider.GetComponent<DoorController>();
			if (!(component == null))
			{
				if (component.transform.position.x < CenterCoord)
				{
					LeftDoor = component;
					LeftDoor.SetRightRoom(this);
				}
				else if (component.transform.position.x > CenterCoord)
				{
					RightDoor = component;
					RightDoor.SetLeftRoom(this);
				}
			}
		}
	}

	public void UpdateLightingForAI()
	{
		if (!useDynamicFlashlight || aiLightSources.Count == 0)
		{
			return;
		}
		int num = 0;
		foreach (Light aiLightSource in aiLightSources)
		{
			if (!aiLightSource.enabled)
			{
				num++;
			}
		}
		useFlashlight = num >= disabledLightsCount;
	}

	public void SetLightShadowsType(LightShadows type)
	{
		foreach (PoweredLight poweredAppliance in poweredAppliances)
		{
			poweredAppliance.SetShadowsType(type);
		}
	}

	public void SetPowerState(bool powered)
	{
		foreach (PoweredLight poweredAppliance in poweredAppliances)
		{
			poweredAppliance.SetPowerState(powered);
		}
	}

	public void RenderReflectionProbe(bool timeSliced = true)
	{
		if (!(reflectionProbe == null) && base.gameObject.activeInHierarchy)
		{
			reflectionProbe.timeSlicingMode = (timeSliced ? ReflectionProbeTimeSlicingMode.IndividualFaces : ReflectionProbeTimeSlicingMode.NoTimeSlicing);
			reflectionProbeRenderId = reflectionProbe.RenderProbe();
		}
	}

	public bool IsReflectionProbeRendered()
	{
		return reflectionProbe == null || reflectionProbe.IsFinishedRendering(reflectionProbeRenderId);
	}

	public float NextEdge(float direction)
	{
		return (!(direction > 0f)) ? LeftEdge : RightEdge;
	}

	public DoorController NextDoor(float direction)
	{
		return (!(direction > 0f)) ? LeftDoor : RightDoor;
	}

	public Room NextRoom(float direction)
	{
		return (!(direction > 0f)) ? leftRoom : rightRoom;
	}

	public Vector2 GetPropableFloorInterval()
	{
		float x = ((!(LeftDoor != null)) ? 0f : ((LeftDoor.OpeningDirection != 1f) ? 0f : 0.9f));
		float y = ((!(RightDoor != null)) ? Width : ((RightDoor.OpeningDirection != -1f) ? Width : (Width - 0.9f)));
		return new Vector2(x, y);
	}

	public bool TriggerFireAlarm(float minSeconds, float maxSeconds, float probability = 1f)
	{
		if (fireAlarm == null)
		{
			return false;
		}
		if (Random.value > probability)
		{
			return false;
		}
		float seconds = Random.Range(minSeconds, maxSeconds);
		fireAlarm.ActivateIn(seconds);
		return true;
	}
}

}

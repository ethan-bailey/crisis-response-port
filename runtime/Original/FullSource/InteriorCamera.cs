namespace Crisis.Runtime.FullSource
{
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class InteriorCamera : MonoBehaviour
{
	public enum ChangeSubjectDirection
	{
		Next = 0,
		Previous = 1
	}

	private enum ConsciousState
	{
		Conscious = 0,
		Unconscious = 1,
		Either = 2
	}

	public Camera interiorCamera;

	public Transform audioListener;

	[Header("Smoothing")]
	public float smoothing = 2f;

	[Range(0f, 12f)]
	public float fovSmoothing = 4f;

	[Header("Offsets")]
	[Range(0f, 1f)]
	[Tooltip("The distance from the subject the camera will center. In percent of the frustum width.")]
	public float centerOffset = 0.3f;

	[Tooltip("How far away from obstacles the scrolling should stop. In meters.")]
	public float obstacleHoldOff = 2f;

	[Tooltip("How much distance to keep on the player's side of doors when entering glue-to-door mode. In meters.")]
	public float doorHoldOff;

	[Header("Overrides")]
	public bool overrideFieldOfView;

	public float fieldOfViewOverride = 20f;

	public bool disableScrollPastDoors;

	public UnityEventVoid onSubjectChanged = new UnityEventVoid();

	private float cameraDistance;

	private float subjectToObstacle = 4.5f;

	private float targetFOV;

	private float staticFov;

	private float dynamicFovMin;

	private float dynamicFovMax;

	private int obstacleMask;

	private bool useDynamicFov;

	private bool settingsInitialized;

	private CharacterBody subject;

	private RaycastHit hit;

	private Vector3 targetPosition;

	private InteriorMode interiorMode;

	private InputHandler input;

	private List<CharacterBody> operators;

	private List<CharacterBody> occupants;

	private NoiseAndGrain grain;

	private BloomOptimized bloom;

	private CameraMotionBlur blur;

	private DepthOfField depthOfField;

	public CharacterBody Subject
	{
		get
		{
			return subject;
		}
		set
		{
			if (!(subject == value))
			{
				if (subject != null)
				{
					subject.Physiology.HeartBeatAudio = null;
				}
				subject = value;
				onSubjectChanged.Invoke();
			}
		}
	}

	public float FrustumWidth { get; private set; }

	public float AIViewDistance
	{
		get
		{
			return FrustumWidth * (1f - centerOffset);
		}
	}

	private float StaticFieldOfView
	{
		get
		{
			return (!overrideFieldOfView) ? staticFov : fieldOfViewOverride;
		}
	}

	private float MaxFieldOfView
	{
		get
		{
			return (!useDynamicFov) ? StaticFieldOfView : dynamicFovMax;
		}
	}

	private void Awake()
	{
		interiorCamera = GetComponent<Camera>();
		targetFOV = interiorCamera.fieldOfView;
		cameraDistance = -1.5f - base.transform.position.z;
		grain = GetComponent<NoiseAndGrain>();
		bloom = GetComponent<BloomOptimized>();
		blur = GetComponent<CameraMotionBlur>();
		depthOfField = GetComponent<DepthOfField>();
	}

	private void Start()
	{
		obstacleMask = Masks.Instance.ObstaclesDoors;
		Mission mission = Global.Context.Mission;
		interiorMode = mission.InteriorMode;
		input = Global.Context.InputHandler;
		operators = ((!(mission.Operators != null)) ? null : mission.Operators.Bodies);
		occupants = ((!(mission.Occupants != null)) ? null : mission.Occupants.Bodies);
		if (Subject == null)
		{
			Subject = FirstCharacter(ConsciousState.Conscious, operators);
		}
		InitializeSettings();
		SetInitialPosition();
	}

	private void InitializeSettings()
	{
		if (!settingsInitialized)
		{
			settingsInitialized = true;
			SerializableGraphicsSettings graphics = Global.Context.Settings.Graphics;
			SerializablePerformanceSettings performance = Global.Context.Settings.Performance;
			useDynamicFov = graphics.useDynamicFieldOfView;
			dynamicFovMin = graphics.dynamicFieldOfViewMin;
			dynamicFovMax = graphics.dynamicFieldOfViewMax;
			staticFov = graphics.staticFieldOfView;
			grain.enabled = performance.cameraGrain;
			bloom.enabled = performance.cameraBloom;
			blur.enabled = performance.cameraBlur;
			graphics.onUseDynamicFovChanged.AddListener((bool enable) =>
			{
				useDynamicFov = enable;
			});
			graphics.onDynamicFieldOfViewChanged.AddListener((float min, float max) =>
			{
				dynamicFovMin = min;
				dynamicFovMax = max;
			});
			graphics.onStaticFieldOfViewChanged.AddListener((float angle) =>
			{
				staticFov = angle;
			});
			performance.onCameraGrainChanged.AddListener((bool enable) =>
			{
				grain.enabled = enable;
			});
			performance.onCameraBloomChanged.AddListener((bool enable) =>
			{
				bloom.enabled = enable;
			});
			performance.onCameraBlurChanged.AddListener((bool enable) =>
			{
				blur.enabled = enable;
			});
		}
	}

	public float SubjectViewDistance()
	{
		if (Subject == null)
		{
			return FrustumWidth;
		}
		if (Subject.Direction == 1f)
		{
			float num = base.transform.position.x - FrustumWidth * 0.5f;
			float num2 = Subject.Eye.position.x - num;
			return FrustumWidth - num2;
		}
		float num3 = base.transform.position.x + FrustumWidth * 0.5f;
		float num4 = num3 - Subject.Eye.position.x;
		return FrustumWidth - num4;
	}

	private void SetInitialPosition()
	{
		UpdateTargetPosition();
		targetPosition = base.transform.position;
	}

	public void SetManualTargetPosition(float worldX)
	{
		Vector3 position = new Vector3(worldX, base.transform.position.y, base.transform.position.z);
		base.transform.position = position;
		targetPosition = position;
	}

	private void Update()
	{
		UpdateTargetPosition();
		UpdateListenerPosition();
		base.transform.position = Vector3.Lerp(base.transform.position, targetPosition, smoothing * Time.unscaledDeltaTime);
		interiorCamera.fieldOfView = Mathf.Lerp(interiorCamera.fieldOfView, targetFOV, fovSmoothing * Time.unscaledDeltaTime);
		FrustumWidth = 2f * cameraDistance * Mathf.Tan(interiorCamera.fieldOfView * 0.5f * ((float)Math.PI / 180f)) * interiorCamera.aspect;
	}

	private void UpdateTargetPosition()
	{
		if (Subject == null)
		{
			return;
		}
		float num = ((!Subject.IsRagdoll) ? (FrustumWidth * centerOffset) : 0f);
		bool flag = Physics.Raycast(Subject.transform.position, Subject.transform.TransformDirection(Vector3.right), out hit, num + obstacleHoldOff, obstacleMask);
		Rigidbody component = Subject.GetComponent<Rigidbody>();
		Vector3 vector = ((!(component != null)) ? Vector3.zero : component.velocity);
		Vector3 vector2 = Subject.KinematicWorldPosition + vector * 0.25f + Subject.transform.TransformDirection(num, 0f, 0f);
		if (flag)
		{
			DoorController doorController = null;
			subjectToObstacle = num;
			float num2 = 10f;
			float num3 = 10f;
			if (!disableScrollPastDoors && hit.collider != null)
			{
				subjectToObstacle = Mathf.Abs(hit.collider.transform.position.x - Subject.transform.position.x);
				Transform parent = hit.collider.transform.parent;
				if (parent != null && parent.parent != null)
				{
					parent = parent.parent;
					doorController = parent.GetComponent<DoorController>();
					if (doorController != null)
					{
						num2 = Mathf.Abs(doorController.transform.position.x - Subject.transform.position.x);
						if (Subject.ActiveWeapon != null)
						{
							num3 = Mathf.Abs(doorController.transform.position.x - Subject.ActiveWeapon.Muzzle.position.x);
						}
					}
				}
			}
			if (num2 >= 1.5f && num3 >= 0.2f)
			{
				targetPosition = new Vector3(hit.point.x + hit.normal.x * obstacleHoldOff, base.transform.position.y, base.transform.position.z);
				float num4 = subjectToObstacle / interiorCamera.aspect;
				if (useDynamicFov)
				{
					targetFOV = Mathf.Clamp(2f * Mathf.Atan(num4 / cameraDistance) * 57.29578f, dynamicFovMin, dynamicFovMax);
				}
				else
				{
					targetFOV = StaticFieldOfView;
				}
			}
			else
			{
				targetPosition = new Vector3(vector2.x - doorHoldOff, base.transform.position.y, base.transform.position.z);
				targetFOV = MaxFieldOfView;
			}
		}
		else
		{
			targetPosition = new Vector3(vector2.x, base.transform.position.y, base.transform.position.z);
			targetFOV = MaxFieldOfView;
		}
	}

	private void UpdateListenerPosition()
	{
		if (!(Subject == null))
		{
			float x = Subject.Eye.position.x;
			audioListener.position = new Vector3(x, 1.25f, -1.5f);
		}
	}

	private bool SubjectHasValidPosition(CharacterBody body)
	{
		float sqrMagnitude = body.KinematicWorldPosition.sqrMagnitude;
		return !float.IsNaN(sqrMagnitude) && !float.IsInfinity(sqrMagnitude) && sqrMagnitude < 10000f;
	}

	public void ChangeSubject(ChangeSubjectDirection direction, bool onlyOperators = false)
	{
		if (Subject == null)
		{
			AssignNewSubject();
		}
		List<CharacterBody> spatiallySortedBodies = Global.Context.Mission.GetSpatiallySortedBodies();
		for (int i = 0; i < spatiallySortedBodies.Count; i++)
		{
			if (!(spatiallySortedBodies[i] == Subject))
			{
				continue;
			}
			bool flag = false;
			for (int j = 0; j < spatiallySortedBodies.Count; j++)
			{
				int num = ((direction != ChangeSubjectDirection.Next) ? (i - 1 - j) : ((i + 1 + j) % spatiallySortedBodies.Count));
				if (num < 0)
				{
					num = spatiallySortedBodies.Count + num;
				}
				if ((!onlyOperators || spatiallySortedBodies[num].IsOperator) && SubjectHasValidPosition(spatiallySortedBodies[num]))
				{
					Subject = spatiallySortedBodies[num];
					flag = true;
					break;
				}
			}
			if (flag)
			{
				break;
			}
		}
	}

	private void AssignNewSubject()
	{
		Subject = FirstCharacter(ConsciousState.Conscious, operators);
		if (Subject == null)
		{
			Subject = FirstCharacter(ConsciousState.Conscious, occupants);
		}
		if (Subject == null)
		{
			Subject = FirstCharacter(ConsciousState.Either, operators);
		}
	}

	private CharacterBody FirstCharacter(ConsciousState conscious, List<CharacterBody> list)
	{
		if (list == null)
		{
			return null;
		}
		foreach (CharacterBody item in list)
		{
			if (SubjectHasValidPosition(item))
			{
				if (conscious == ConsciousState.Conscious && !item.IsRagdoll)
				{
					return item;
				}
				if (conscious == ConsciousState.Unconscious && item.IsRagdoll)
				{
					return item;
				}
				if (conscious == ConsciousState.Either)
				{
					return item;
				}
			}
		}
		return null;
	}

	public void EnableDepthOfField(bool enable)
	{
		depthOfField.enabled = enable;
	}
}

}

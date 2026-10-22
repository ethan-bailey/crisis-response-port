namespace Crisis.Runtime.FullSource
{
using UnityEngine;

public class AlternateCamera : MonoBehaviour
{
	public float lerpFactor = 0.05f;

	public float fovLerpFactor = 0.05f;

	public float sensitivity = 0.01f;

	public float z = 10f;

	[Tooltip("Radius of dead zone in center of screen, in percent of screen width.")]
	[Range(0f, 0.5f)]
	public float deadZone = 0.1f;

	public AnimationCurve sensitivityTransition;

	private Transform target;

	private InputHandler input;

	private Vector3 screenCenterPosition;

	private float targetFov = 60f;

	private float deadZonePixels = 100f;

	public Camera Camera { get; private set; }

	public bool Locked { get; private set; }

	private void Awake()
	{
		Camera = GetComponent<Camera>();
		target = new GameObject("Alternate camera target").transform;
		input = Global.Context.InputHandler;
	}

	private void Start()
	{
		int width = 0;
		int height = 0;
		Global.Context.GetCanvasResolution(ref width, ref height);
		deadZonePixels = (float)width * deadZone;
		screenCenterPosition = new Vector3(width, height, 0f) * 0.5f;
		InteriorCamera interiorCamera = Global.Context.Mission.InteriorMode.interiorCamera;
		Transform obj = base.transform;
		Vector3 position = interiorCamera.transform.position;
		target.transform.position = position;
		obj.position = position;
		interiorCamera.enabled = false;
		targetFov = Camera.fieldOfView;
	}

	private void Update()
	{
		if (Time.deltaTime == 0f)
		{
			return;
		}
		if (!Locked)
		{
			Vector3 vector = Input.mousePosition - screenCenterPosition;
			float time = Mathf.Max(vector.magnitude - deadZonePixels, 0f) * sensitivity;
			float num = sensitivityTransition.Evaluate(time);
			if (num > float.Epsilon)
			{
				base.transform.position += vector.normalized * num;
			}
			float axis = Input.GetAxis("ScrollWheel");
			bool pressed = input.Keys.sprint.Pressed;
			if (Mathf.Abs(axis) > float.Epsilon)
			{
				float num2 = Mathf.Sign(axis);
				if (num2 > 0f)
				{
					if (pressed)
					{
						Vector3 localEulerAngles = Camera.transform.localEulerAngles;
						localEulerAngles.x += 5f;
						Camera.transform.localEulerAngles = localEulerAngles;
					}
					else
					{
						targetFov *= 0.8f;
					}
				}
				else if (pressed)
				{
					Vector3 localEulerAngles2 = Camera.transform.localEulerAngles;
					localEulerAngles2.x -= 5f;
					Camera.transform.localEulerAngles = localEulerAngles2;
				}
				else
				{
					targetFov *= 1.25f;
				}
			}
			Camera.fieldOfView = Mathf.Lerp(Camera.fieldOfView, targetFov, Time.unscaledDeltaTime * fovLerpFactor);
		}
		if (input.Keys.doorGradual.Down)
		{
			Locked = !Locked;
		}
		if (Input.GetKeyDown(KeyCode.J))
		{
			Camera.cullingMask ^= 1 << LayerMask.NameToLayer("Exterior geometry");
			Camera.cullingMask ^= 1 << LayerMask.NameToLayer("Windows");
		}
	}
}

}

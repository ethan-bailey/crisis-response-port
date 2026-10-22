namespace Crisis.Runtime.FullSource
{
using UnityEngine;

public class MarksmanCamera : MonoBehaviour
{
	public bool useDynamicClipping;

	[Tooltip("How far ahead and behind the target plane the camera will clip. In meters.")]
	public float clippingRange = 25f;

	[Header("Scope")]
	public RectTransform canvas;

	public RectTransform leftSide;

	public RectTransform rightSide;

	public Camera marksmanCamera;

	public RectTransform boltActionImageTransform;

	private Marksman marksman;

	private InputHandler inputHandler;

	private RaycastHit hit = default(RaycastHit);

	private Ray ray = default(Ray);

	private int mask;

	private const float reticleWeight = 0.00075f;

	private const float reticleDotSpacing = 0.05f;

	private float aspect;

	private void Awake()
	{
		mask = Masks.Instance.Aim;
		inputHandler = Global.Context.InputHandler;
		Cursor.lockState = CursorLockMode.Locked;
		aspect = Global.Context.GetCanvasAspectRatio();
	}

	private void Start()
	{
		ray.origin = base.transform.position;
		AdjustForAspectRatio();
		marksman = Global.Context.Mission.Marksman;
	}

	private void OnDisable()
	{
		Cursor.lockState = CursorLockMode.None;
	}

	private void Update()
	{
		base.transform.position = marksman.transform.position;
		base.transform.rotation = marksman.transform.rotation;
		AdjustForAspectRatio();
		if (useDynamicClipping)
		{
			AdjustClippingPlanes();
		}
	}

	private void AdjustForAspectRatio()
	{
		float size = (canvas.rect.width - canvas.rect.height) * 0.5f;
		leftSide.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
		rightSide.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
	}

	private void AdjustClippingPlanes()
	{
		ray.direction = base.transform.forward;
		if (!Physics.Raycast(ray, out hit, 1000f, mask))
		{
			marksmanCamera.farClipPlane = Mathf.Abs(-1.5f - base.transform.position.z) + clippingRange;
			marksmanCamera.nearClipPlane = Mathf.Max(marksmanCamera.farClipPlane - clippingRange * 2f, 0.5f);
		}
		else
		{
			marksmanCamera.farClipPlane = (hit.point - base.transform.position).magnitude + clippingRange;
			marksmanCamera.nearClipPlane = Mathf.Max(marksmanCamera.farClipPlane - clippingRange * 2f, 0.5f);
		}
	}

	public void SetFieldOfView(float value)
	{
		marksmanCamera.fieldOfView = value;
	}
}

}

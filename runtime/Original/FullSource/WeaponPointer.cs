namespace Crisis.Runtime.FullSource
{
using UnityEngine;
using UnityEngine.UI;

public class WeaponPointer : MonoBehaviour
{
	public Transform mainDot;

	public Transform startDot;

	public Transform endDot;

	public GameObject aimQuadPrefab;

	private Ray ray;

	private Transform aimQuad;

	private Vector3 localPoint;

	private RaycastHit raycastHit;

	private Camera interiorCamera;

	private CharacterBody controlledBody;

	private UIInteractionMenu interactionMenu;

	private Image mainImage;

	private Image startImage;

	private Image endImage;

	private InteriorMode interiorMode;

	private InputHandler input;

	private bool precisionMode;

	private int raycastMask;

	public Vector3 WorldPoint { get; private set; }

	public bool Visible
	{
		get
		{
			return base.gameObject.activeInHierarchy;
		}
	}

	private void Awake()
	{
		if (aimQuad == null)
		{
			GameObject gameObject = Object.Instantiate(aimQuadPrefab);
			gameObject.name = aimQuadPrefab.name;
			aimQuad = gameObject.transform;
		}
		mainImage = mainDot.GetComponent<Image>();
		startImage = startDot.GetComponent<Image>();
		endImage = endDot.GetComponent<Image>();
	}

	private void Start()
	{
		GlobalContext context = Global.Context;
		raycastMask = Masks.Instance.Aim;
		input = Global.Context.InputHandler;
		interiorMode = context.Mission.InteriorMode;
		interiorCamera = interiorMode.interiorCamera.interiorCamera;
		interactionMenu = interiorMode.UI.InteractionMenu;
		context.Mission.UI.PauseMenu.onShow.AddListener(Hide);
	}

	private void Update()
	{
		if (Time.timeScale == 0f)
		{
			return;
		}
		if (controlledBody == null || !controlledBody.IsPlayer || interiorCamera == null)
		{
			if (Visible)
			{
				Hide();
			}
			return;
		}
		aimQuad.position = new Vector3(controlledBody.Position + controlledBody.Direction * 20f, 2.5f, -1.5f);
		if (!interactionMenu.Visible)
		{
			Camera camera = ((!(interiorMode.AlternateCamera != null) || !interiorMode.AlternateCamera.enabled) ? interiorCamera : interiorMode.AlternateCamera.Camera);
			ray = camera.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out raycastHit, 50f, raycastMask))
			{
				localPoint = controlledBody.transform.InverseTransformPoint(raycastHit.point);
			}
		}
		WorldPoint = controlledBody.transform.TransformPoint(localPoint);
		mainDot.position = interiorCamera.WorldToScreenPoint(WorldPoint);
		WeaponBase activeWeapon = controlledBody.ActiveWeapon;
		if (activeWeapon == null)
		{
			SetPrecisionMode(false);
			return;
		}
		float magnitude = (WorldPoint - activeWeapon.Muzzle.transform.position).magnitude;
		Vector3 vector = activeWeapon.Muzzle.transform.position + activeWeapon.Muzzle.forward * magnitude;
		bool flag = !activeWeapon.ReadyToFire && !activeWeapon.IsCycling && input.Keys.fire.Pressed;
		Image image = mainImage;
		Color color = ((!flag) ? Color.white : Color.red);
		endImage.color = color;
		color = color;
		startImage.color = color;
		image.color = color;
		mainImage.enabled = !flag;
		if (controlledBody.IsUsingPrecision != precisionMode)
		{
			precisionMode = controlledBody.IsUsingPrecision;
			SetPrecisionMode(precisionMode);
		}
		if (precisionMode)
		{
			startDot.position = interiorCamera.WorldToScreenPoint(vector - activeWeapon.Muzzle.forward * activeWeapon.PrecisionHalfDistance);
			endDot.position = interiorCamera.WorldToScreenPoint(vector + activeWeapon.Muzzle.forward * activeWeapon.PrecisionHalfDistance);
		}
	}

	public void Show()
	{
		if (!(controlledBody == null) && !controlledBody.IsRagdoll)
		{
			base.gameObject.SetActive(true);
			mainImage.enabled = true;
			startImage.enabled = true;
			endImage.enabled = true;
			Cursor.lockState = CursorLockMode.Confined;
			Cursor.visible = false;
		}
	}

	public void Hide()
	{
		base.gameObject.SetActive(false);
		mainImage.enabled = false;
		startImage.enabled = false;
		endImage.enabled = false;
	}

	public void SetPrecisionMode(bool active)
	{
		startDot.gameObject.SetActive(active);
		endDot.gameObject.SetActive(active);
	}

	public void AssignBody(CharacterBody body)
	{
		controlledBody = body;
		if (body != null)
		{
			localPoint = new Vector3(controlledBody.Direction * 2f, 0f, 0f);
		}
	}
}

}

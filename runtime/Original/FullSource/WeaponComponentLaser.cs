namespace Crisis.Runtime.FullSource
{
using UnityEngine;

public class WeaponComponentLaser : MonoBehaviour
{
	public Color color = Color.green;

	public Transform laserSource;

	public GameObject laserDotPrefab;

	private bool active;

	private bool suspended;

	private bool pendingTurnOn;

	private int obstaclesMask;

	private int obstaclesAndLimbsMask;

	private GameObject dot;

	private RaycastHit raycastHit;

	private LineRenderer lineRenderer;

	private AudioSource audioSource;

	private WeaponBase weapon;

	private float maxLength = 100f;

	private float dotOffset = 0.005f;

	public bool IsOn
	{
		get
		{
			return lineRenderer.enabled;
		}
	}

	public bool IsActive
	{
		get
		{
			return active;
		}
	}

	private void OnEnable()
	{
		if (active && dot != null)
		{
			dot.SetActive(true);
		}
	}

	private void OnDisable()
	{
		if (dot != null)
		{
			dot.SetActive(false);
		}
	}

	private void Awake()
	{
		obstaclesMask = Masks.Instance.BallisticTargetsSansLimbs;
		obstaclesAndLimbsMask = Masks.Instance.BallisticTargets;
		audioSource = GetComponent<AudioSource>();
		lineRenderer = GetComponent<LineRenderer>();
		LineRenderer obj = lineRenderer;
		Color color = this.color;
		lineRenderer.endColor = color;
		obj.startColor = color;
	}

	private void Start()
	{
		weapon = GetComponent<WeaponComponent>().Weapon;
		if (weapon == null)
		{
			Object.Destroy(this);
			return;
		}
		dot = Object.Instantiate(laserDotPrefab);
		dot.GetComponent<Light>().color = color;
		dot.SetActive(active);
	}

	public void Toggle()
	{
		if (!suspended && !(dot == null))
		{
			active = !active;
			dot.SetActive(active);
			lineRenderer.enabled = active;
			audioSource.Play();
		}
	}

	public void Suspend(bool suspend)
	{
		suspended = suspend;
		bool flag = !suspend && active;
		if (flag != IsOn)
		{
			audioSource.Play();
		}
		if (dot != null)
		{
			dot.SetActive(flag);
		}
		lineRenderer.enabled = flag;
	}

	public void SetState(bool enable = true, bool audio = true)
	{
		if (enable != active)
		{
			active = enable;
			audioSource.Play();
			if (enable)
			{
				pendingTurnOn = true;
			}
			else
			{
				active = false;
				dot.SetActive(false);
				lineRenderer.enabled = false;
			}
			if (audio)
			{
				audioSource.Play();
			}
		}
	}

	public void SetActive(bool active)
	{
		this.active = active;
	}

	private void Update()
	{
		if ((active || pendingTurnOn) && !(dot == null))
		{
			if (weapon.Owner != null && weapon.Owner.IsUsingPrecision)
			{
				DrawLaserPrecision(weapon);
			}
			else
			{
				DrawLaserNonPrecision();
			}
			if (pendingTurnOn)
			{
				pendingTurnOn = false;
				active = true;
				dot.SetActive(true);
				lineRenderer.enabled = true;
			}
		}
	}

	private void DrawLaserPrecision(WeaponBase weapon)
	{
		Vector3 position = laserSource.position;
		Vector3 zero = Vector3.zero;
		Vector3 vector = weapon.Muzzle.position - position;
		float magnitude = (weapon.Muzzle.position - position).magnitude;
		float num = weapon.PrecisionBegin + magnitude;
		float num2 = weapon.PrecisionEnd + magnitude;
		zero = (Physics.Raycast(position, laserSource.right, out raycastHit, weapon.PrecisionBegin, obstaclesMask) ? (raycastHit.point + raycastHit.normal * dotOffset) : (Physics.Raycast(position + laserSource.right * num, laserSource.right, out raycastHit, weapon.PrecisionDistance, obstaclesAndLimbsMask) ? (raycastHit.point + raycastHit.normal * dotOffset) : ((!Physics.Raycast(position + laserSource.right * num2, laserSource.right, out raycastHit, maxLength, obstaclesMask)) ? (position + laserSource.right * maxLength) : (raycastHit.point + raycastHit.normal * dotOffset))));
		dot.transform.position = zero;
		lineRenderer.SetPosition(0, position);
		lineRenderer.SetPosition(1, zero);
	}

	private void DrawLaserNonPrecision()
	{
		lineRenderer.SetPosition(0, laserSource.position);
		Ray ray = new Ray(laserSource.position, laserSource.right);
		if (Physics.Raycast(ray, out raycastHit, maxLength, obstaclesAndLimbsMask))
		{
			dot.transform.position = raycastHit.point - laserSource.right * 0.01f;
			lineRenderer.SetPosition(1, dot.transform.position);
		}
		else
		{
			lineRenderer.SetPosition(1, laserSource.position + laserSource.right * maxLength);
		}
	}
}

}

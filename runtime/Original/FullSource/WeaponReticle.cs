namespace Crisis.Runtime.FullSource
{
using System;
using UnityEngine;

public class WeaponReticle : MonoBehaviour
{
	public int vertexCount = 12;

	public GameObject cross;

	private float segment;

	private InputHandler input;

	private LineRenderer lineRenderer;

	private Operators operators;

	private void Awake()
	{
		vertexCount = Mathf.Max(vertexCount, 3);
		lineRenderer = GetComponent<LineRenderer>();
		lineRenderer.positionCount = vertexCount;
		segment = (float)Math.PI * 2f / (float)(vertexCount - 1);
	}

	private void Start()
	{
		input = Global.Context.InputHandler;
		operators = Global.Context.Mission.Operators;
	}

	private void Update()
	{
		PlayerController playerController = operators.PlayerController;
		if (playerController == null || playerController.Body == null || playerController.Body.ActiveWeapon == null)
		{
			lineRenderer.enabled = false;
			return;
		}
		WeaponBase activeWeapon = playerController.Body.ActiveWeapon;
		DrawCircle(activeWeapon);
	}

	private void DrawCircle(WeaponBase weapon)
	{
		lineRenderer.enabled = true;
		float precisionCenterCoord = weapon.PrecisionCenterCoord;
		base.transform.position = weapon.BorePoint;
		base.transform.rotation = Quaternion.LookRotation(base.transform.position - weapon.Muzzle.position);
		float num = ((!(weapon.ammunition != null)) ? 0f : (weapon.ammunition.DiameterInMeters + Mathf.Tan(weapon.CombinedInaccuracy * ((float)Math.PI / 180f)) * precisionCenterCoord));
		if (!weapon.ReadyToFire && !weapon.IsCycling && input.Keys.fire.Pressed)
		{
			lineRenderer.enabled = false;
			cross.gameObject.SetActive(true);
			return;
		}
		cross.gameObject.SetActive(false);
		lineRenderer.widthMultiplier = 1f;
		lineRenderer.material.color = Color.white;
		float num2 = Mathf.Lerp(0.005f, 0.05f, num * 2f);
		LineRenderer obj = lineRenderer;
		float num3 = num2;
		lineRenderer.endWidth = num3;
		obj.startWidth = num3;
		lineRenderer.positionCount = vertexCount;
		lineRenderer.enabled = true;
		for (int i = 0; i < vertexCount; i++)
		{
			float f = segment * (float)i;
			lineRenderer.SetPosition(i, new Vector3(Mathf.Cos(f), Mathf.Sin(f), 0f) * num);
		}
	}
}

}

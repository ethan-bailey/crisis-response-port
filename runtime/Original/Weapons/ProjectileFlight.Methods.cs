using UnityEngine;
using System.Collections.Generic;
using Object=UnityEngine.Object;
using Random=UnityEngine.Random;
using Crisis.Runtime.Weapons;
public partial class Projectile {
	public List<UnityEngine.MeshFilter> fragmentMeshes = new();
	public UnityEngine.Material fragmentMaterial;
	public float particleCountScaler = 1f;
public void StepFlight()
	{
		float num = 0f;
		float deltaTime = Time.deltaTime;
		int num2 = 3;
		bool flag = TestTermination();
		if (!flag && base.gameObject.activeInHierarchy && num < deltaTime && num2 > 0)
		{
			num2--;
			Vector3 position = base.transform.position;
			float num3 = deltaTime - num;
			float num4 = Velocity * num3;
			NextPosition = base.transform.position + base.transform.forward * num4 + GravityVelocityAtTime(Time.time + num3) * num3;
			Vector3 normalized = (NextPosition - base.transform.position).normalized;
			bool flag2 = Physics.Raycast(base.transform.position, normalized, out nonDiscriminatingResult, num4, ballisticMask);
			BallisticResult ballisticResult = BallisticResult.Continue;
			if (flag2)
			{
				base.transform.position = nonDiscriminatingResult.point;
				Limb component = nonDiscriminatingResult.collider.GetComponent<Limb>();
				TutorialSoftTarget component2 = nonDiscriminatingResult.collider.GetComponent<TutorialSoftTarget>();
				if (component == null && component2 == null)
				{
					ballisticResult = HandleGenericHit(nonDiscriminatingResult);
				}
				else if (!FiredWithPrecision)
				{
					ballisticResult = HandleLimbHit(nonDiscriminatingResult);
				}
				else
				{
					float num5 = DistanceAtPosition(NextPosition);
					if (num5 >= segmentBeginMagnitude && Distance < segmentEndMagnitude)
					{
						float num6 = Mathf.Max(Distance, segmentBeginMagnitude);
						float num7 = Mathf.Min(num5, segmentEndMagnitude);
						float maxDistance = num7 - num6;
						raycastRay.origin = StartPosition + normalized * num6;
						raycastRay.direction = normalized;
						if (Physics.Raycast(raycastRay, out limbRaycastResult, maxDistance, limbMask))
						{
							ballisticResult = HandleLimbHit(limbRaycastResult);
						}
					}
				}
				IsStray = true;
			}
			else
			{
				base.transform.position = NextPosition;
			}
			num += (position - base.transform.position).magnitude / Velocity;
			/* Non-explosive source FMJ scope; asserted at configuration load. */
			if (ballisticResult == BallisticResult.Stop || ballisticResult == BallisticResult.Error)
			{
				flag = true;
			}
		}
		UpdateRotation();
		if (flag)
		{
			Finished=true;Object.Destroy(base.gameObject);
		}
	}
private void InitializeVelocity(float velocityModifier)
	{
		float num = 1f + Random.Range(Ammunition.velocityModifier * -0.5f, Ammunition.velocityModifier * 0.5f);
		float baseVelocity = (Velocity = Ammunition.velocity * num * velocityModifier);
		BaseVelocity = baseVelocity;
		StartEnergy = VelocityToEnergy(BaseVelocity);
	}
public bool TestTermination()
	{
		return Time.time - SpawnTime > 1f || Energy < 2f || Mathf.Abs(base.transform.position.x) > 100f || base.transform.position.y <= -0.1f || Mathf.Abs(base.transform.position.y) > 20f || Mathf.Abs(base.transform.position.z) > 400f;
	}
public void AssignNewSpin()
	{
		Spin = -360f * (Velocity / (RiflingTwistRate * 0.0254f));
	}
public void UpdateRotation()
	{
		float magnitude = (InteractionPosition - base.transform.position).magnitude;
		Vector3 localEulerAngles = magnitude * -360f * new Vector3(Tumble.x, Tumble.y, Tumble.z);
		meshTransform.localEulerAngles = localEulerAngles;
	}
}
namespace Crisis.Runtime.FullSource
{
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;
using System.Collections.Generic;
using UnityEngine;

public class VitalFunctions : MonoBehaviour
{
	public List<LimbVitalFunction> vitalFunctions;

	private LimbPhysiology physiology;

	private float capsuleOffset;

	private float capsuleHeight;

	private void Awake()
	{
		physiology = GetComponent<LimbPhysiology>();
	}

	private void Start()
	{
		CapsuleCollider component = GetComponent<CapsuleCollider>();
		capsuleHeight = ((!(component != null)) ? 0.5f : component.height);
		capsuleOffset = ((!(component != null)) ? 0f : (capsuleHeight * 0.5f - component.center.y));
		if (physiology == null)
		{
			Object.Destroy(this);
		}
	}

	public void HitTest(Projectile projectile, Vector3 entry, Vector3 exit, float shock)
	{
		float num = Mathf.Abs(base.transform.InverseTransformPoint(entry).y + capsuleOffset) / capsuleHeight;
		float num2 = Mathf.Abs(base.transform.InverseTransformPoint(exit).y + capsuleOffset) / capsuleHeight;
		float num3 = (num + num2) * 0.5f;
		foreach (LimbVitalFunction vitalFunction in vitalFunctions)
		{
			float value = Random.value;
			if (!(value > vitalFunction.probability))
			{
				float num4 = vitalFunction.factor.Evaluate(num3);
				if (num4 > 0f)
				{
					physiology.InflictVitalTrauma(vitalFunction, projectile.DisplacedMaterialVolume * num4, shock * num4, num3);
					physiology.Physiology.AnnounceHit(vitalFunction.name, projectile);
				}
			}
		}
	}
}

}

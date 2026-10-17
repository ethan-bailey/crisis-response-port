using UnityEngine;

public class LimbArmor : MonoBehaviour
{
	public ArmorClass.Type type;

	[Header("Front")]
	public bool hasFrontArmor;

	[Tooltip("The range of covered area on the limb's front. Ranges from zero to one, where zero is the bottom and one is the top of the limb.")]
	public Vector2 frontCoverage = new Vector2(0f, 1f);

	public float frontDamage;

	[Header("Back")]
	public bool hasBackArmor;

	[Tooltip("The range of covered area on the limb's back. Ranges from zero to one, where zero is the bottom and one is the top of the limb.")]
	public Vector2 backCoverage = new Vector2(0f, 1f);

	[Tooltip("How much abuse the back of the armor can take before a projectile is guaranteed to go through")]
	public float backDamage;

	private Limb limb;

	private CharacterBody body;

	private float limbHeight;

	private void Awake()
	{
		limb = GetComponent<Limb>();
		body = limb.Body;
		CapsuleCollider component = GetComponent<CapsuleCollider>();
		limbHeight = ((!(component != null)) ? 0.5f : component.height);
	}

	public Projectile Test(Projectile projectile, bool entry)
	{
		if (type == ArmorClass.Type.None)
		{
			return projectile;
		}
		bool flag = Mathf.Sign(base.transform.InverseTransformDirection(projectile.transform.forward).x) * ((!entry) ? (-1f) : 1f) < 0f;
		if (flag)
		{
			if (!HasCoverage(projectile, hasFrontArmor, frontCoverage))
			{
				return projectile;
			}
		}
		else if (!HasCoverage(projectile, hasBackArmor, backCoverage))
		{
			return projectile;
		}
		float damage = ((!flag) ? backDamage : frontDamage);
		float energy = projectile.Energy;
		bool flag2 = ArmorClass.PenetrationTest(type, damage, projectile);
		float num = energy - projectile.Energy;
		if (flag)
		{
			frontDamage += num;
		}
		else
		{
			backDamage += num;
		}
		if (!flag2)
		{
			projectile.Status = BallisticResult.Stop;
		}
		return projectile;
	}

	private bool HasCoverage(Projectile projectile, bool hasCoverage, Vector2 coveredRange)
	{
		if (!hasCoverage)
		{
			return false;
		}
		float num = Mathf.Abs(base.transform.InverseTransformPoint(projectile.transform.position).y) / limbHeight;
		if (num < coveredRange.x || num > coveredRange.y)
		{
			return false;
		}
		return true;
	}

	public void Disable()
	{
		type = ArmorClass.Type.None;
	}
}

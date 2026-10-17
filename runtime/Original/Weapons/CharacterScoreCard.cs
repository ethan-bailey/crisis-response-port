using UnityEngine;
namespace Crisis.Runtime.Weapons;
// Original CharacterScoreCard.cs, unchanged body.
public class CharacterScoreCard
{
	private const float ReactionTimeMargin = 0.33f;

	private const float UnconsciousPenalty = 0.15f;

	private const float RestrainedPenalty = 0.15f;

	private const float CompliantPenalty = 0.1f;

	private const float UnarmedPenalty = 0.05f;

	private CharacterBody self;

	public int CompliantCount { get; private set; }

	public int RestrainedCount { get; private set; }

	public int UnconsciousCount { get; private set; }

	public int UncheckedCount { get; private set; }

	public float Compliant { get; private set; }

	public float Restrained { get; private set; }

	public float Unconscious { get; private set; }

	public float Unchecked { get; private set; }

	public CharacterScoreCard(CharacterBody body)
	{
		self = body;
	}

	public void RegisterPenalties(Projectile projectile, float time, float burstModifier = 1f)
	{
		if (!projectile.FiredWithPrecision && projectile.Owner != null && !projectile.Owner.CanSeePosition(projectile.transform.position.x))
		{
			Unchecked += 0.005f;
			UncheckedCount++;
		}
		float num = ((!projectile.IsStray || !(projectile.Owner != null)) ? 1f : 0.5f);
		float num2 = num * burstModifier;
		if (!self.Conscious && time - self.Physiology.UnconsciousTimestamp >= 0.33f)
		{
			Unconscious += 0.15f * num2;
			UnconsciousCount++;
		}
		else if (self.IsRestrained)
		{
			Restrained += 0.15f * num2;
			RestrainedCount++;
		}
		else if (self.IsCompliant)
		{
			Compliant += 0.1f * num2;
			CompliantCount++;
		}
	}
}

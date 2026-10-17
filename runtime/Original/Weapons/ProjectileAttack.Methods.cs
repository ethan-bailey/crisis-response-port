using UnityEngine;
using Crisis.Runtime.Weapons;
// Capability implemented by a recovered Marksman host, never an empty callback.
public interface IOriginalMarksmanAttack { void ValidateBindings(); void OnSuccessfulAttack(CharacterBody victim); }
public partial class Projectile {
 public CharacterBody FirstHitCharacter {get;private set;}
private void RecordAttackInfo(CharacterBody victim)
	{
		if (FiredByMarksman)
		{
			victim.OnWasAttacked(this);
			((IOriginalMarksmanAttack)MarksmanOwner).OnSuccessfulAttack(victim);
		}
		else
		{
			if (FirstHitCharacter == null && Owner != null)
			{
				Owner.OnSuccessfulAttack(victim, SwarmId);
			}
			victim.OnWasAttacked(this);
		}
		FirstHitCharacter = victim;
	}
}

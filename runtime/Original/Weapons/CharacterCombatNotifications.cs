using UnityEngine;
using Crisis.Runtime.Character;
namespace Crisis.Runtime.Weapons;

// Original CharacterBody combat methods. Engine/presentation dependencies of
// OnWasHit are explicit capabilities; live collisions require them before any
// tissue mutation. Test observers are confined to Checks, never bound by game.
public interface IOriginalCharacterHitEffects {
 void ValidateBindings();
 float FirstInjuryTimestamp {set;}
 bool AllRoomsChecked {get;}
 bool HasOccupants {get;}
 bool AllOccupantsRenderedSafe {get;}
 bool ShowInteractionMarkers {get;}
 bool VoicePlaying {get;}
 void InvokeBulletTimeOnHit(bool isPlayer);
 void ShowStabilizeMarker(bool show);
 void Hurt(float delay);
 void DropWeapons(bool doNotDropTriggerPulledWeapon);
}
public partial class CharacterBody {
 public OriginalPhysiologyConditions Physiology=>SourcePhysiology;
 public bool Conscious=>Physiology.Conscious;
 public bool IsRestrained {get;set;}
 public bool IsCompliant {get;set;}
 public bool IsOperator {get;private set;}
 public Transform Eye {get;private set;}
 public float VisibleRange {get;set;}
 public CharacterScoreCard ScoreCard {get;private set;}
 public HashSet<int> HitCount {get;private set;}
 public float LastAttackTimestamp {get;set;}
 public CharacterBody LastAttackTarget {get;set;}
 public float LastHitTimestamp {get;set;}
 public CharacterBody LastHitBy {get;set;}
 public float LastHitEnergy {get;set;}
 public float LastDirectionAttackedFrom {get;set;}
 public Limb LastHitLimb {get;set;}
 public int ShotsHit {get;private set;}
 int lastProjectileHitById,hitCount;
 float hurtAudioTimestamp;
 IOriginalCharacterHitEffects hitEffects;
 public void InitializeOriginalCombat(Transform eye,float visibleRange,bool isOperator,
      IOriginalCharacterHitEffects effects){
  if(SourcePhysiology==null)throw new InvalidOperationException("Initialize source physiology first");
  if(ScoreCard!=null)throw new InvalidOperationException("Combat state already initialized");
  Eye=eye??throw new ArgumentNullException(nameof(eye));
  if(!float.IsFinite(visibleRange)||visibleRange<0)throw new ArgumentOutOfRangeException(nameof(visibleRange));
  VisibleRange=visibleRange;IsOperator=isOperator;
  hitEffects=effects??throw new ArgumentNullException(nameof(effects));
  hitEffects.ValidateBindings();
  // CharacterBody.Awake original initialization; do not reset at each shot.
  ScoreCard=new CharacterScoreCard(this);HitCount=new HashSet<int>();
  LastHitTimestamp=-100f;LastAttackTimestamp=-100f;
 }
 public void RequireOriginalCombat(bool victimHit){
  if(Physiology==null||ScoreCard==null||HitCount==null||Eye==null)
   throw new InvalidOperationException("Original combat state/visibility has not been initialized");
  if(victimHit)(hitEffects??throw new NotSupportedException("Original hit effects are not bound")).ValidateBindings();
 }
 float HemorrhagingScore(){
  float num=0f;
  foreach(Limb limb in limbs)if(limb.LimbPhysiology!=null)num+=limb.LimbPhysiology.HemorrhagingScore;
  return Mathf.Max(1f-num/300f,0f);
 }
public bool CanSeePosition(float position)
	{
		float f = position - Eye.position.x;
		float num = Mathf.Sign(f);
		if (num != Direction || Mathf.Abs(f) > VisibleRange)
		{
			return false;
		}
		return true;
	}

public void OnSuccessfulAttack(CharacterBody victim, int projectileId)
	{
		LastAttackTarget = victim;
		LastAttackTimestamp = Time.time;
		if (projectileId != lastProjectileHitById)
		{
			ShotsHit++;
			lastProjectileHitById = projectileId;
		}
	}

public void OnWasAttacked(Projectile projectile)
	{
		bool flag = Time.time - LastHitTimestamp < 1f;
		hitCount = ((!flag) ? 1 : (hitCount + 1));
		float burstModifier = ((!flag) ? 1f : (0.2f / (float)hitCount));
		ScoreCard.RegisterPenalties(projectile, Time.time, burstModifier);
		LastHitTimestamp = Time.time;
		LastHitBy = projectile.Owner;
		LastHitEnergy = projectile.InteractionEnergy;
		LastDirectionAttackedFrom = Mathf.Sign(base.transform.forward.x) * -1f;
	}

public void OnWasHit(Projectile projectile)
	{
		hitEffects.FirstInjuryTimestamp = Time.time;
		hitEffects.InvokeBulletTimeOnHit(IsPlayer);
		EventHit.Invoke();
		HitCount.Add(projectile.SwarmId);
		if (HemorrhagingScore() < 1f && hitEffects.AllRoomsChecked && hitEffects.HasOccupants && hitEffects.AllOccupantsRenderedSafe && hitEffects.ShowInteractionMarkers)
		{
			hitEffects.ShowStabilizeMarker(true);
		}
		if (!Physiology.invulnerable && Time.time - hurtAudioTimestamp > 10f && UnityEngine.Random.value >= Mathf.Clamp(Physiology.ShockResistance, 0.25f, 0.9f))
		{
			float num = UnityEngine.Random.Range(1.5f, 4f);
			hitEffects.Hurt(num);
			hurtAudioTimestamp = Time.time + num;
		}
		if (!IsOperator && Conscious)
		{
			if (Physiology.ShouldLoseConsciousness() && Physiology.ShockNormalized * 2f * UnityEngine.Random.value >= Physiology.ShockResistance)
			{
				hitEffects.DropWeapons(false);
			}
			if (UnityEngine.Random.value < 0.25f && !hitEffects.VoicePlaying)
			{
				hitEffects.Hurt(0.3f);
			}
		}
	}
}

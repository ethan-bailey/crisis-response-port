#!/usr/bin/env python3
"""Check recovered combat method bodies against preserved developer source."""
import hashlib,json,re
from pathlib import Path
R=Path(__file__).resolve().parents[1]
def method(text,signature):
 start=text.index(signature);i=text.index('{',start);depth=1;j=i+1
 while depth:
  depth+=(text[j]=='{')-(text[j]=='}');j+=1
 return text[start:j]
def norm(text):return '\n'.join(line.strip() for line in text.strip().splitlines() if line.strip())
source=(R/'source-reference/Original/CharacterBody.cs').read_text()
runtime=(R/'runtime/Original/Weapons/CharacterCombatNotifications.cs').read_text()
for sig in ['public bool CanSeePosition(float position)','public void OnSuccessfulAttack(CharacterBody victim, int projectileId)','public void OnWasAttacked(Projectile projectile)']:
 assert norm(method(source,sig))==norm(method(runtime,sig)),sig
sig='public void OnWasHit(Projectile projectile)';hit=method(source,sig)
substitutions={'Mission.FirstInjuryTimestamp':'hitEffects.FirstInjuryTimestamp','InvokeBulletTimeOnHit();':'hitEffects.InvokeBulletTimeOnHit(IsPlayer);','Physiology.HemorrhagingScore()':'HemorrhagingScore()','Mission.AllRoomsChecked':'hitEffects.AllRoomsChecked','Mission.Occupants != null':'hitEffects.HasOccupants','Mission.Occupants.AllRenderedSafe':'hitEffects.AllOccupantsRenderedSafe','Global.Context.Settings.General.showInteractionMarkers':'hitEffects.ShowInteractionMarkers','ShowStabilizeMarker(true);':'hitEffects.ShowStabilizeMarker(true);','Audio.Hurt(':'hitEffects.Hurt(','DropWeapons(false);':'hitEffects.DropWeapons(false);','Audio.VoicePlaying':'hitEffects.VoicePlaying'}
for a,b in substitutions.items():hit=hit.replace(a,b)
assert norm(hit)==norm(method(runtime,sig)),sig
source=(R/'source-reference/Original/ProjectileInteractions/Projectile.cs').read_text()
runtime=(R/'runtime/Original/Weapons/ProjectileAttack.Methods.cs').read_text()
sig='private void RecordAttackInfo(CharacterBody victim)'
expected=method(source,sig).replace('MarksmanOwner.OnSuccessfulAttack(victim);','((IOriginalMarksmanAttack)MarksmanOwner).OnSuccessfulAttack(victim);')
assert norm(expected)==norm(method(runtime,sig)),sig
original=(R/'source-reference/Original/ProjectileInteractions/CharacterScoreCard.cs').read_bytes()
manifest=json.loads((R/'runtime/Original/Weapons/combat-source-provenance.json').read_text())
assert hashlib.sha256(original).hexdigest()==manifest['files']['CharacterScoreCard.cs']
runtime=(R/'runtime/Original/Weapons/CharacterScoreCard.cs').read_text()
assert norm(original.decode())==norm(runtime[runtime.index('public class CharacterScoreCard'):])
assert manifest['liveBindingsComplete'] is False
print(json.dumps({'originalCombatMethods':'PASS','sourceMethods':6,'liveBindingsComplete':False}))

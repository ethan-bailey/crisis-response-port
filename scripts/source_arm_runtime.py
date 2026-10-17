"""Select original IK methods and exact prefab references for the existing host.

This extends the existing explicit weapon-host source slice. It does not claim
the whole original CharacterBody/IKArmController or ragdoll ArmController runs.
"""
import hashlib
from extract_runtime_sources import block


SIGNATURES = ['private void Awake()', 'private void Update()', 'private void UpdateTransition()',
              'public void SetTargetImmediate(', 'public void TargetTransition(Transform target, float seconds',
              'public void TargetTransition(Transform target, Vector3 worldSpaceOffset', 'public void StartWait()',
              'public void ReturnTransforms()', 'public void SetHandAngle(', 'public float GetHandAngle(',
              'public void SetOnWeapon(', 'public void SetUnarmed()']


def build_arm_transition(source):
    methods = [block(source, signature) for signature in SIGNATURES]
    adaptations = {
        '\t\tJoint component = upperArm.GetComponent<Joint>();\n': '',
        '\t\tJoint component2 = lowerArm.GetComponent<Joint>();\n': '',
        'Debug.DrawLine(TransitionTarget.position, TransitionTarget.position + worldSpaceOffset, Color.yellow);': ''
    }
    # The two queried Joint locals are never subsequently read by original IK.
    # Removing those dead queries adds no fake Joint component or physics call.
    for before, after in adaptations.items():
        methods = [method.replace(before, after) for method in methods]
    header = '''using UnityEngine;
using System;
using System.Collections.Generic;
using Object=UnityEngine.Object;
using Random=UnityEngine.Random;
namespace Crisis.Runtime.Weapons;
// Original selected method bodies. Explicit host scheduling, not full controller playback.
public class ArmIKSolver:Behaviour {
 public enum HandRotationMode{Local=0,Global=1,TargetLocal=2}
 public AnimationCurve interpolation;public bool flip;
 public Limb upperArm,lowerArm;public Hand hand;public bool smoothing;public float smoothFactor=10f;
 public Transform ikTarget,transitionTarget;private Limb handLimb;
 private Hand.Mode transitionHandMode;private HandRotationMode transitionHandRotationMode;
 private float upperArmLength,lowerArmLength,upperArmAngle,lowerArmAngle;
 private float transitionTimestamp,transitionPeriod,transitionHandAngle;
 private bool transition;private static float HalfPi=(float)Math.PI/2f;
 public bool HasSolution{get;private set;}public bool OnTarget{get;private set;}public bool OnWeapon{get;private set;}
 public bool Transitioning=>transition;
'''
    return header + block(source, 'public Transform TransitionTarget') + '''
 public void InitializeSourcePose()=>Awake();
 public void Step()=>Update();
''' + '\n'.join(methods) + '\n}\n'


def arm_bindings(records):
    by_id = {record['fileID']: record for record in records}
    def transform_for_go(go):
        return next(r['fileID'] for r in records if r['classID'] == 4 and r['fields']['m_GameObject']['fileID'] == go)
    result = {}
    for record in records:
        fields = record['fields']
        if record['classID'] != 114 or 'smoothFactor' not in fields or 'upperArm' not in fields:
            continue
        def go_for_component(key):
            return by_id[fields[key]['fileID']]['fields']['m_GameObject']['fileID']
        hand = by_id[fields['hand']['fileID']]['fields']
        upper = go_for_component('upperArm')
        name = by_id[upper]['fields']['m_Name']
        side = 'right' if name == 'Right upper arm' else 'left' if name == 'Left upper arm' else None
        if side is None or side in result:
            raise ValueError('Unexpected original arm solver hierarchy')
        result[side] = dict(solverTransform=transform_for_go(fields['m_GameObject']['fileID']),
                            upperTransform=transform_for_go(upper), lowerTransform=transform_for_go(go_for_component('lowerArm')),
                            handTransform=transform_for_go(go_for_component('hand')), settings=fields,
                            handComponent=hand, variants={key:dict(gameObject=hand[key]['fileID'],
                                transform=transform_for_go(hand[key]['fileID']), active=bool(by_id[hand[key]['fileID']]['fields']['m_IsActive']))
                                for key in ('fist', 'trigger', 'forwardAssist')})
    if set(result) != {'left', 'right'}:
        raise ValueError('Expected original left and right solvers')
    return result


def provenance(source, output):
    return dict(sourceSha256=hashlib.sha256(source.encode()).hexdigest(),
                outputSha256=hashlib.sha256(output.encode()).hexdigest(),
                selectedMethods={signature:hashlib.sha256(block(source, signature).encode()).hexdigest() for signature in SIGNATURES},
                adaptations=['Namespace and explicit existing host state views',
                             'Remove two unread Joint query locals from Awake; no Joint stand-in',
                             'Omit debug DrawLine; no visual gameplay effect',
                             'Explicit InitializeSourcePose/Step entry points; unselected Start/controller callbacks excluded'],
                fullIKArmControllerPlayback=False, ragdollArmControllerExecuted=False)


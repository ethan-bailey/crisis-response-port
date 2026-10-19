namespace Crisis.Runtime.FullSource
{
using System.Collections.Generic;
using UnityEngine;

public class Blackboard : MonoBehaviour
{
	[Header("Targets")]
	public CharacterBody FleeTarget;

	public HashSet<CharacterBody> Friends;

	public HashSet<Target> Enemies;

	public float targetFirstSeenTimestamp;

	public float targetLastSeenTimestamp;

	[Header("Miscellaneous")]
	public float grenadePosition;

	[Header("Weapons")]
	[Tooltip("The weapon skill determines how close the character will attempt to get to the intended target angle when aiming. It also determines recoil compensation.")]
	[Range(0f, 1f)]
	public float weaponSkill = 1f;

	[Header("Stats")]
	[Tooltip("When zero surrender. When high, make quicker decisions.")]
	[Range(0f, 1f)]
	public float morale = 0.5f;

	[Tooltip("When high, tend towards aggressive actions.")]
	[Range(0f, 1f)]
	public float aggression = 0.5f;

	[Tooltip("When high, tend to avoid confrontations.")]
	[Range(0f, 1f)]
	public float fear = 0.5f;

	[Header("Occupant")]
	public bool negotiable;

	[Header("Operator")]
	public OperatorCommand operatorCommand = OperatorCommand.Follow;

	[Header("Follow")]
	[Tooltip("Greater than height of capsule collider and less than interaction distance.")]
	public float collisionDistance = 0.725f;

	[Tooltip("When following I will move to this distance from my target.")]
	public float followDistance = 2f;

	[Tooltip("When my follow target is closer than this I will move away.")]
	public float leaderTooClose = 1.5f;

	[Tooltip("When my follow target is further away than this I will move closer.")]
	public float leaderTooFar = 2.5f;

	[Tooltip("When covering my leader I will move this close.")]
	public float coverDistance = 0.5f;

	[Header("Engage")]
	[Tooltip("When engaging I will keep this distance from my target.")]
	public float engageDistance = 3f;

	[Tooltip("When my target is closer than this I will move away.")]
	public float targetTooClose = 2.5f;

	[Tooltip("When my target is further away than this I will move closer.")]
	public float targetTooFar = 4f;

	[Tooltip("I will defend myself when target comes closer than this.")]
	public float selfDefenseDistance = 1.5f;

	[Tooltip("I will defend myself when target's weapon angle to me is less than this.")]
	public float aimAtTolerance = 25f;

	[Tooltip("For how long I will keep focus on my target once it's neutralized.")]
	public float clearTargetPeriod = 2f;

	[Tooltip("Prefer to aim for the head of my targets. Otherwise the torso.")]
	public bool preferHeadTarget = true;

	[Header("Fear")]
	public float fearToRunFromExplosions = 0.25f;

	public Target Target { get; set; }

	public CharacterBody TargetBody
	{
		get
		{
			return (!(Target != null)) ? null : Target.Body;
		}
	}

	public float TargetVisibleTime { get; set; }

	public float TargetDecayTime { get; set; }

	public AudioStimuli AudioStimuli { get; set; }

	public float AudioDecay { get; set; }

	public float MovePosition { get; set; }

	public ActionSource FleeSource { get; set; }

	public GameObject LastCollider { get; set; }

	public Vector3 Aimpoint { get; set; }

	public Grenade ThrownGrenade { get; set; }

	public Grenade PerceivedGrenade { get; set; }

	public float GrenadeSeenTimestamp { get; set; }

	public bool SprintWhenInvestigating { get; set; }

	public WeaponBase SecureWeaponTarget { get; set; }

	public bool HasMasterKey { get; set; }

	public bool EquipCollectedWeapon { get; set; }

	public CharacterBody PendingRestrainTarget { get; set; }

	public CharacterBody PendingGauzeTarget { get; set; }

	public CharacterBody CharacterInteractionTarget { get; set; }

	public float AimAngle { get; set; }

	public DoorController Door { get; set; }

	public float FollowTargetDirection { get; set; }

	public float PatrolWaitTimestamp { get; set; }

	public float PatrolWaitPeriod { get; set; }

	public float DoorTimer { get; set; }

	public float GrenadeTimer { get; set; }

	public float KneelTimer { get; set; }

	public float WeaponTriggerPeriod { get; set; }

	public float LastVisualOnTarget { get; set; }

	public float EyeMovementTimer { get; set; }

	public bool WillBlindFire { get; set; }

	public float BlindFireDistance { get; set; }

	public float ThrowStunPosition { get; set; }

	public float DecideActionTimestamp { get; set; }

	public float ThrowStunTimestamp { get; set; }

	public float StunDetonateTimestamp { get; set; }

	public float MagazineChangeTimestamp { get; set; }

	public float TargetEliminatedTimestamp { get; set; }

	public float FleeTargetTimestamp { get; set; }

	public float LatestCommandTimestamp { get; set; }

	public float ReleaseTriggerTimestamp { get; set; }

	public SuspectAction SuspectAction { get; set; }

	private void Awake()
	{
		Friends = new HashSet<CharacterBody>();
		Enemies = new HashSet<Target>();
		FleeSource = new ActionSource();
		DecideActionTimestamp = -100f;
		ThrowStunTimestamp = float.PositiveInfinity;
		MagazineChangeTimestamp = float.NegativeInfinity;
		TargetEliminatedTimestamp = -100f;
		FleeTargetTimestamp = -100f;
		GrenadeSeenTimestamp = float.PositiveInfinity;
		StunDetonateTimestamp = float.PositiveInfinity;
		ThrowStunPosition = float.PositiveInfinity;
	}
}

}

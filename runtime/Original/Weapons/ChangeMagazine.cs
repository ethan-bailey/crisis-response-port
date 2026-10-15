using UnityEngine;
using System;
using System.Collections.Generic;
using Object=UnityEngine.Object;
using Random=UnityEngine.Random;
namespace Crisis.Runtime.Weapons;



	public class ChangeMagazine : WeaponTask
	{
		private enum State
		{
			Begin = 0,
			OpenBolt = 1,
			ReturnBoltArm = 2,
			State0 = 3,
			State1 = 4,
			State2 = 5,
			State3 = 6,
			State4 = 7,
			State5 = 8,
			State6 = 9,
			State7 = 10,
			State8 = 11,
			State9 = 12,
			State10 = 13,
			State11 = 14
		}

		public bool quick;

		public bool isPlayer;

		private CharacterBody self;

		private WeaponBase weapon;

		private CyclingBolt bolt;

		private ArmIKSolver magazineArm;

		private ArmIKSolver boltArm;

		private State state;

		private InputHandler input;

		private float timestamp;

		private float waitTimestamp;

		private float skillDelay;

		private bool automaticBoltFeed;

		private bool usingRightArm;

		private bool hasMoreAmmunition;

		private bool openBoltStarted;

		public override void OnAwake()
		{
			self = gameObject.GetComponent<CharacterBody>();
			input = Global.Context.InputHandler;
			skillDelay = (1f - self.Blackboard.weaponSkill) * 1f;
		}

		public override void OnStart()
		{
			state = State.Begin;
			timestamp = (waitTimestamp = Time.time);
			self.IsManipulatingEquipment = true;
			weapon = self.ActiveWeapon;
			bolt = weapon.cyclingBolt;
			openBoltStarted = false;
			if (weapon != null)
			{
				usingRightArm = weapon.fireFromMagazine;
				magazineArm = ((!usingRightArm) ? self.leftArm : self.rightArm);
				boltArm = ((!weapon.leftHandBoltManipulation) ? self.rightArm : self.leftArm);
				hasMoreAmmunition = self.Equipment.HasAmmunitionForWeapon(weapon);
				automaticBoltFeed = !weapon.fireFromMagazine && (!isPlayer || Global.Context.Settings.General.reloadHelp);
			}
		}

		public override void OnEnd()
		{
			self.PendingAction = CharacterBody.CharacterAction.None;
			self.IsManipulatingEquipment = false;
			CyclingBolt cyclingBolt = weapon.cyclingBolt;
			if (automaticBoltFeed && cyclingBolt != null && !cyclingBolt.Live && (cyclingBolt.CarryingLiveRound || (weapon.Magazine != null && !weapon.Magazine.Empty)))
			{
				self.PendingAction = CharacterBody.CharacterAction.WorkBolt;
			}
		}

		public override TaskStatus OnUpdate()
		{
			if (weapon == null)
			{
				return TaskStatus.Failure;
			}
			if (isPlayer && (self.ActiveWeapon != weapon || (input.Keys.fire.Down && state >= State.State9)))
			{
				input.Keys.fire.DownHandled = true;
				magazineArm.SetOnWeapon(weapon, 0.1f, usingRightArm);
				return TaskStatus.Success;
			}
			switch (state)
			{
			case State.Begin:
				if (!bolt.lockOpen || bolt.IsOpen || bolt.Live || !automaticBoltFeed)
				{
					state = State.State0;
				}
				else
				{
					if (weapon.boltManipulationTransitionTime > 0f)
					{
						boltArm.TargetTransition(weapon.boltHandleTransform, weapon.boltManipulationTransitionTime);
					}
					state++;
				}
				return TaskStatus.Running;
			case State.OpenBolt:
				if (!boltArm.OnTarget || bolt.IsMoving)
				{
					return TaskStatus.Running;
				}
				if (!self.IsPlayer && Time.time - waitTimestamp < skillDelay)
				{
					return TaskStatus.Running;
				}
				if (!openBoltStarted)
				{
					openBoltStarted = true;
					bolt.Open(true);
				}
				else if (!bolt.IsMoving)
				{
					if (weapon.boltManipulationTransitionTime > 0f)
					{
						boltArm.SetOnWeapon(weapon, weapon.boltManipulationTransitionTime, !weapon.leftHandBoltManipulation);
					}
					state++;
				}
				return TaskStatus.Running;
			case State.ReturnBoltArm:
				if (!self.IsPlayer && Time.time - waitTimestamp < skillDelay)
				{
					return TaskStatus.Running;
				}
				waitTimestamp = Time.time;
				if (boltArm.OnTarget)
				{
					state++;
				}
				return TaskStatus.Running;
			case State.State0:
			{
				state++;
				Vector3 worldSpaceOffset6 = weapon.transform.TransformVector(weapon.magazineInsertTrack);
				magazineArm.TargetTransition(weapon.magazineTransform, worldSpaceOffset6, 0.2f);
				if (weapon.useInternalMagazine && weapon.type == WeaponBrace.Freehand)
				{
					weapon.transform.SetParent(self.leftArm.hand.grabPoint);
					weapon.transform.localPosition = Vector3.zero;
				}
				return TaskStatus.Running;
			}
			case State.State1:
				if (magazineArm.OnTarget)
				{
					if (!self.IsPlayer && Time.time - waitTimestamp < skillDelay)
					{
						return TaskStatus.Running;
					}
					Vector3 worldSpaceOffset = weapon.transform.TransformVector(weapon.magazineInsertTrack * 2f);
					magazineArm.TargetTransition(weapon.magazineTransform, worldSpaceOffset, 0.1f);
					waitTimestamp = Time.time;
					if (weapon.useInternalMagazine)
					{
						state = State.State8;
						weapon.RemoveMagazine();
					}
					else if (quick)
					{
						state = State.State3;
						weapon.RemoveMagazine(true);
					}
					else
					{
						state = State.State2;
						weapon.RemoveMagazine();
						self.MoveMagazineToHand(false);
					}
				}
				return TaskStatus.Running;
			case State.State2:
				if (magazineArm.OnTarget)
				{
					state++;
					Vector3 worldSpaceOffset4 = self.Torso.transform.TransformVector(new Vector3(0f, 0.1f, 0f));
					magazineArm.TargetTransition(self.chestRigIKTarget, worldSpaceOffset4, 0.25f, Hand.Mode.Fist, -45f);
				}
				return TaskStatus.Running;
			case State.State3:
				if (magazineArm.OnTarget)
				{
					if (!hasMoreAmmunition)
					{
						magazineArm.SetOnWeapon(weapon, 0.1f, usingRightArm);
						return TaskStatus.Success;
					}
					if (!self.IsPlayer && Time.time - waitTimestamp < skillDelay)
					{
						return TaskStatus.Running;
					}
					waitTimestamp = Time.time;
					state++;
					magazineArm.TargetTransition(self.chestRigIKTarget, (!quick) ? 0.5f : 0.2f, Hand.Mode.Fist, -25f);
				}
				return TaskStatus.Running;
			case State.State4:
				if (magazineArm.OnTarget)
				{
					state++;
					if (quick)
					{
						self.MoveMagazineToHand(usingRightArm);
					}
					Vector3 worldSpaceOffset5 = weapon.transform.TransformVector(weapon.magazineInsertTrack * 2f);
					magazineArm.TargetTransition(weapon.magazineTransform, worldSpaceOffset5, 0.3f);
				}
				return TaskStatus.Running;
			case State.State5:
				if (magazineArm.OnTarget)
				{
					state++;
					Vector3 worldSpaceOffset3 = weapon.transform.TransformVector(weapon.magazineInsertTrack);
					magazineArm.TargetTransition(weapon.magazineTransform, worldSpaceOffset3, 0.1f);
				}
				return TaskStatus.Running;
			case State.State6:
				if (magazineArm.OnTarget)
				{
					state++;
					Magazine fullestMagazineForWeapon = self.Equipment.GetFullestMagazineForWeapon(weapon);
					self.ActiveWeapon.InsertMagazine(fullestMagazineForWeapon);
					magazineArm.SetOnWeapon(weapon, 0.1f, usingRightArm);
				}
				return TaskStatus.Running;
			case State.State7:
				if (magazineArm.OnTarget)
				{
					return TaskStatus.Success;
				}
				return TaskStatus.Running;
			case State.State8:
				if (magazineArm.OnTarget)
				{
					state++;
					magazineArm.TargetTransition(self.chestRigIKTarget, 0.2f, Hand.Mode.Fist, -25f);
				}
				return TaskStatus.Running;
			case State.State9:
				if (magazineArm.OnTarget)
				{
					if (!self.IsPlayer && Time.time - waitTimestamp < skillDelay)
					{
						return TaskStatus.Running;
					}
					waitTimestamp = Time.time;
					state++;
					Vector3 worldSpaceOffset2 = weapon.transform.TransformVector(weapon.magazineInsertTrack * 2f);
					magazineArm.TargetTransition(weapon.magazineTransform, worldSpaceOffset2, 0.3f);
				}
				return TaskStatus.Running;
			case State.State10:
				if (magazineArm.OnTarget)
				{
					if (weapon.Magazine.Full || !self.Equipment.HasAmmunitionForWeapon(weapon))
					{
						state++;
						if (weapon.fireFromMagazine)
						{
							self.ActiveWeapon.ReturnMagazine();
						}
						magazineArm.SetOnWeapon(weapon, 0.1f, usingRightArm);
					}
					else
					{
						if (!self.IsPlayer && Time.time - waitTimestamp < skillDelay)
						{
							return TaskStatus.Running;
						}
						waitTimestamp = Time.time;
						state = State.State9;
						self.InsertSingleRound();
						magazineArm.TargetTransition(weapon.magazineTransform, 0.3f);
					}
				}
				return TaskStatus.Running;
			case State.State11:
				if (magazineArm.OnTarget)
				{
					if (weapon.type == WeaponBrace.Freehand)
					{
						weapon.transform.SetParent(self.pistolPivot);
						weapon.transform.localPosition = Vector3.zero;
					}
					return TaskStatus.Success;
				}
				return TaskStatus.Running;
			default:
				return TaskStatus.Running;
			}
		}
	}

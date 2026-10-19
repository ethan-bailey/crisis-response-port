namespace Crisis.Runtime.FullSource
{
using System.Collections.Generic;
using UnityEngine;

public class ScoreKeeper : MonoBehaviour
{
	public const int MaximumScore = 100;

	private Mission mission;

	private float wounded = 0.95f;

	private float critical = 0.5f;

	private float uncheckedRoom = -0.1f;

	private float unpacifiedOccupant = -0.025f;

	private float unrestrainedOccupant = -0.025f;

	private float unsecuredWeapon = -0.01f;

	public int Score { get; private set; }

	public bool Success { get; private set; }

	public float OperatorsScore { get; private set; }

	public float OperatorsRatio { get; private set; }

	public int OperatorsCount { get; private set; }

	public int UnharmedOperators { get; private set; }

	public int WoundedOperators { get; private set; }

	public int CriticalOperators { get; private set; }

	public int KilledOperators { get; private set; }

	public float OccupantsScore { get; private set; }

	public float OccupantsRatio { get; private set; }

	public int OccupantsCount { get; private set; }

	public int UnharmedOccupants { get; private set; }

	public int WoundedOccupants { get; private set; }

	public int CriticalOccupants { get; private set; }

	public int KilledOccupants { get; private set; }

	public int UncheckedRooms { get; private set; }

	public int UnpacifiedOccupants { get; private set; }

	public int UnrestrainedOccupants { get; private set; }

	public int UnsecuredWeapons { get; private set; }

	public int UnstabilizedOccupants { get; private set; }

	public int AttacksOnUnarmedCount { get; private set; }

	public int AttacksOnCompliantCount { get; private set; }

	public int AttacksOnRestrainedCount { get; private set; }

	public int AttacksOnUnconsciousCount { get; private set; }

	public int UncheckedAttacksCount { get; private set; }

	public float AttacksOnUnarmed { get; private set; }

	public float AttacksOnCompliant { get; private set; }

	public float AttacksOnRestrained { get; private set; }

	public float AttacksOnUnconscious { get; private set; }

	public float UncheckedAttacks { get; private set; }

	public float MissionTime { get; private set; }

	public float CriticalTime { get; private set; }

	public int OperatorShotsFired { get; private set; }

	public int OperatorShotsHit { get; private set; }

	public int MarksmanShotsFired { get; private set; }

	public int MarksmanShotsHit { get; private set; }

	public int OccupantShotsFired { get; private set; }

	public int OccupantShotsHit { get; private set; }

	public int OperatorStunGrenades { get; private set; }

	public int OperatorFragGrenades { get; private set; }

	private void Start()
	{
		mission = Global.Context.Mission;
	}

	public void Calculate()
	{
		Reset();
		bool flag = Global.Context.SceneTransition.CurrentScene == SceneTransition.Scene.Marksman;
		TallyOperators();
		TallyOccupants(flag);
		if (!flag)
		{
			TallyRooms();
			TallyWeapons();
			TallyDeductions();
		}
		RecordDetails();
		OperatorsRatio = OperatorsScore / (float)OperatorsCount;
		OccupantsRatio = OccupantsScore / (float)OccupantsCount;
		float num = (float)UncheckedRooms * uncheckedRoom + (float)UnpacifiedOccupants * unpacifiedOccupant + (float)UnrestrainedOccupants * unrestrainedOccupant + (float)UnsecuredWeapons * unsecuredWeapon - AttacksOnUnarmed;
		float num2 = (OperatorsRatio + OccupantsRatio) / 2f + num;
		Score = Mathf.RoundToInt(Mathf.Clamp(100f * num2, 0f, 100f));
	}

	private void Reset()
	{
		Score = 0;
		OperatorsCount = 0;
		OccupantsCount = 0;
		UnharmedOperators = 0;
		WoundedOperators = 0;
		CriticalOperators = 0;
		KilledOperators = 0;
		UnharmedOccupants = 0;
		WoundedOccupants = 0;
		CriticalOccupants = 0;
		KilledOccupants = 0;
		UncheckedRooms = 0;
		UnsecuredWeapons = 0;
		UnpacifiedOccupants = 0;
		UnrestrainedOccupants = 0;
		UnstabilizedOccupants = 0;
		AttacksOnUnarmed = 0f;
		AttacksOnCompliant = 0f;
		AttacksOnRestrained = 0f;
		AttacksOnUnconscious = 0f;
		UncheckedAttacks = 0f;
		OperatorShotsFired = 0;
		OccupantShotsFired = 0;
		OperatorStunGrenades = 0;
		OperatorFragGrenades = 0;
	}

	private void TallyOccupants(bool isMarksmanMission)
	{
		Transform container = mission.Occupants.Container;
		OccupantsCount = container.childCount;
		OccupantsScore = 0f;
		foreach (Transform item in container)
		{
			CharacterBody component = item.GetComponent<CharacterBody>();
			float num = component.Physiology.Score();
			OccupantsScore += num;
			if (num > wounded)
			{
				UnharmedOccupants++;
			}
			else if (num > critical && num <= wounded)
			{
				WoundedOccupants++;
			}
			else if (num > 0f && num <= critical)
			{
				CriticalOccupants++;
			}
			else
			{
				KilledOccupants++;
			}
			if (!isMarksmanMission)
			{
				if (!component.IsRestrained)
				{
					UnrestrainedOccupants++;
					if (component.Kinematic && !component.IsCompliant)
					{
						UnpacifiedOccupants++;
					}
				}
				if (component.ActiveWeapon != null)
				{
					UnsecuredWeapons++;
				}
			}
			OccupantShotsFired += component.ShotsFired;
			OccupantShotsHit += component.ShotsHit;
		}
	}

	private void TallyOperators()
	{
		if (mission.Operators == null)
		{
			MarksmanShotsFired = ((mission.Marksman != null) ? mission.Marksman.ShotsFired : 0);
			MarksmanShotsHit = ((mission.Marksman != null) ? mission.Marksman.ShotsHit : 0);
			OperatorsScore = 1f;
			return;
		}
		OperatorsScore = 0f;
		List<CharacterBody> bodies = mission.Operators.Bodies;
		OperatorsCount = bodies.Count;
		foreach (CharacterBody item in bodies)
		{
			Physiology physiology = item.Physiology;
			float num = item.Physiology.Score();
			OperatorsScore += num;
			if (num > wounded)
			{
				UnharmedOperators++;
			}
			else if (num > critical && num <= wounded)
			{
				WoundedOperators++;
			}
			else if (num > 0f && num <= critical)
			{
				CriticalOperators++;
			}
			else
			{
				KilledOperators++;
			}
			OperatorShotsFired += item.ShotsFired;
			OperatorShotsHit += item.ShotsHit;
			OperatorStunGrenades += item.StunGrenadesUsed;
			OperatorFragGrenades += item.FragGrenadesUsed;
		}
		MarksmanShotsFired = ((mission.Marksman != null) ? mission.Marksman.ShotsFired : 0);
		MarksmanShotsHit = ((mission.Marksman != null) ? mission.Marksman.ShotsHit : 0);
	}

	private void TallyRooms()
	{
		List<Room> rooms = mission.rooms;
		foreach (Room item in rooms)
		{
			if (item.gameObject.activeInHierarchy && !item.cleared)
			{
				UncheckedRooms++;
			}
		}
	}

	private void TallyWeapons()
	{
		UnsecuredWeapons += mission.WeaponsContainer.childCount;
	}

	private void TallyDeductions()
	{
		AttacksOnUnarmed = 0f;
		List<CharacterBody> bodies = mission.Occupants.Bodies;
		if (bodies.Count == 0)
		{
			return;
		}
		foreach (CharacterBody item in bodies)
		{
			CharacterScoreCard scoreCard = item.ScoreCard;
			AttacksOnCompliantCount += scoreCard.CompliantCount;
			AttacksOnRestrainedCount += scoreCard.RestrainedCount;
			AttacksOnUnconsciousCount += scoreCard.UnconsciousCount;
			UncheckedAttacksCount += scoreCard.UncheckedCount;
			AttacksOnCompliant += scoreCard.Compliant;
			AttacksOnRestrained += scoreCard.Restrained;
			AttacksOnUnconscious += scoreCard.Unconscious;
			UncheckedAttacks += scoreCard.Unchecked;
		}
	}

	private void RecordDetails()
	{
		Success = mission.missionSuccess;
		MissionTime = mission.EndTimestamp - mission.MissionStartTimestamp;
		CriticalTime = mission.AllClearTimestamp - mission.FirstInjuryTimestamp;
	}
}

}

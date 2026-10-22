namespace Crisis.Runtime.FullSource
{
using System.Collections.Generic;
using UnityEngine;

public class TeamTutorial : Tutorial
{
	private enum State
	{
		Welcome = 0,
		Intro = 1,
		EnterFirstRoom = 2,
		MoveToCommand = 3,
		OpenSecondDoor = 4,
		ClearRoom = 5,
		ToggleStun = 6,
		IncreaseTempo = 7,
		ClearWithStun = 8,
		ClearSlow = 9,
		FollowPlayer = 10,
		Marksman = 11
	}

	[Header("Doors")]
	public DoorController firstDoor;

	public DoorController secondDoor;

	public DoorController thirdDoor;

	public DoorController fourthDoor;

	public DoorController fifthDoor;

	public DoorController sixthDoor;

	[Header("Rooms")]
	public Room secondRoom;

	public Room thirdRoom;

	public Room fourthRoom;

	[Header("Marksman")]
	public AmmunitionBase marksmanAmmunition;

	public List<Target> marksmanTargets = new List<Target>();

	public GameObject targetOccluder;

	[Header("Referenced objects")]
	public Transform arrow;

	public Transform indicationTarget;

	public IndicatorArrow indicatorArrow;

	public AnimationCurve arrowSpeedFunction;

	public UIInteriorModeTextIndicator topIndicator;

	public UIInteriorModeTextIndicator bottomIndicator;

	public GameObject uiArrow;

	private State state;

	private Mission mission;

	private InputHandler input;

	private CharacterBody player;

	private WeaponPointer pointer;

	private Transform indicationSource;

	private Transform indicationTargetTransform;

	private InteriorCamera tutorialCamera;

	private UIInteractionMenu interactionMenu;

	private Operators operators;

	private TeamController playerGroup;

	private TeamController taskGroup;

	private Marksman marksman;

	private bool completed;

	private float indicationTimer;

	private float reusableTimestamp;

	private int reusableIndex1;

	private int reusableIndex2;

	private void Awake()
	{
		GlobalContext context = Global.Context;
		input = context.InputHandler;
		input.SuspendInput(false);
		input.DisableAll();
		mission = context.Mission;
		interactionMenu = mission.InteriorMode.UI.InteractionMenu;
		interactionMenu.LongPressDisabledInTutorial = true;
		pointer = mission.InteriorMode.UI.weaponPointer;
		mission.TimeScaler.SetTutorialMode(true);
	}

	private void Start()
	{
		tutorialCamera = mission.InteriorMode.interiorCamera;
		tutorialCamera.overrideFieldOfView = true;
		tutorialCamera.disableScrollPastDoors = true;
		interactionMenu.ShortPressDisabledInTutorial = true;
		interactionMenu.LongPressDisabledInTutorial = true;
		interactionMenu.QuickActionVisualizationDisabled = true;
		operators = mission.Operators;
		playerGroup = operators.MainGroup;
		taskGroup = operators.SupportGroup;
		marksman = mission.Marksman;
		mission.InteriorMode.UI.marksmanIndicator.gameObject.SetActive(false);
		firstDoor.lockState = DoorLockState.Unlocked;
		firstDoor.SetOpen();
		secondDoor.lockState = DoorLockState.Unlocked;
		secondDoor.SetClosed();
		thirdDoor.lockState = DoorLockState.Locked;
		thirdDoor.SetClosed();
		fourthDoor.lockState = DoorLockState.Unlocked;
		fourthDoor.SetClosed();
		fifthDoor.lockState = DoorLockState.Unlocked;
		fifthDoor.SetClosed();
		sixthDoor.lockState = DoorLockState.Unlocked;
		sixthDoor.SetOpen();
		player = mission.Operators.First;
		player.transform.position = startPosition;
		player.ActiveWeapon.SetFireMode(WeaponBase.FireMode.Safe);
		player.InactiveWeapon.SetFireMode(WeaponBase.FireMode.Safe);
		Global.Context.Usage.session.teamTutorials++;
		InitWelcome();
	}

	private void OnDisable()
	{
		input.EnableAll();
	}

	private void Update()
	{
		switch (state)
		{
		case State.Welcome:
			Welcome();
			break;
		case State.Intro:
			Intro();
			break;
		case State.EnterFirstRoom:
			EnterFirstRoom();
			break;
		case State.MoveToCommand:
			MoveToCommand();
			break;
		case State.OpenSecondDoor:
			OpenSecondDoor();
			break;
		case State.ClearRoom:
			ClearRoom();
			break;
		case State.ToggleStun:
			ToggleStun();
			break;
		case State.IncreaseTempo:
			IncreaseTempo();
			break;
		case State.ClearWithStun:
			ClearWithStun();
			break;
		case State.ClearSlow:
			ClearSlow();
			break;
		case State.FollowPlayer:
			FollowPlayer();
			break;
		case State.Marksman:
			Marksman();
			break;
		default:
			TutorialComplete();
			break;
		}
		UpdateArrowIndication();
	}

	private void InitWelcome()
	{
		input.Keys.interact.Enabled = true;
		topIndicator.Display("Welcome to the team command tutorial.", false);
		bottomIndicator.Display(string.Format("Press <{0}> to begin.", input.Keys.interact.KeyName), false);
	}

	private void Welcome()
	{
		if (input.Keys.interact.Down)
		{
			InitIntro();
		}
	}

	private void InitIntro()
	{
		state++;
		reusableIndex1 = 0;
		topIndicator.Hide();
		bottomIndicator.Display(string.Format("Your teammates are controlled by the computer. You control them by issuing commands. Press <{0}> to continue.", input.Keys.interact.KeyName));
	}

	private void Intro()
	{
		if (input.Keys.interact.Down)
		{
			InitEnterFirstRoom();
		}
	}

	private void InitEnterFirstRoom()
	{
		state++;
		input.Keys.interact.Enabled = false;
		bottomIndicator.Display("By default they follow you. Move into the first room.");
		input.EnableCategory(InputKey.Category.Movement);
		SetMarkerAndTrace(player.Abdomen.transform, new Vector3(1.7f, 0f, -1.5f));
	}

	private void EnterFirstRoom()
	{
		if (MoveDistance() < 0.2f)
		{
			InitMoveToCommand();
		}
	}

	private void InitMoveToCommand()
	{
		state++;
		bottomIndicator.Display(string.Format("You can order your team to move to positions. Make them move to the arrow. Hold your cursor at the arrow and press <{0}>.", input.Keys.teamMoveTo.KeyName));
		SetTargetTrace(false);
		SetPositionMarker(true, new Vector3(4.5f, 0.5f, -1.5f));
		input.Keys.teamMoveTo.Enabled = true;
	}

	private void MoveToCommand()
	{
		if (taskGroup.targetPosition > indicatorArrow.transform.position.x)
		{
			taskGroup.targetPosition = indicatorArrow.transform.position.x;
		}
		if (GroupMoveDistance() < 0.5f)
		{
			InitOpenSecondDoor();
		}
	}

	private void InitOpenSecondDoor()
	{
		state++;
		SetPositionMarker(true, new Vector3(secondDoor.Position, 1.2f, -1.5f), 90f);
		bottomIndicator.Display(string.Format("Now order them to open the door. Place the cursor just past the door and press <{0}> again.", input.Keys.teamMoveTo.KeyName));
	}

	private void OpenSecondDoor()
	{
		if (secondDoor.openState == DoorOpenState.Open)
		{
			InitClearRoom();
		}
	}

	private void InitClearRoom()
	{
		state++;
		reusableIndex1 = 0;
		SetPositionMarker(false);
		SetTargetTrace(false);
		input.Keys.teamMoveTo.Enabled = false;
		input.Keys.teamClearNext.Enabled = true;
		bottomIndicator.Display(string.Format("You can also tell them to clear rooms. When you do this they will clear the room that is right in front of them. Press <{0}> to make them clear the room now.", input.Keys.teamClearNext.KeyName));
	}

	private void ClearRoom()
	{
		if (reusableIndex1 == 0 && taskGroup.Leader.Body.IsMoving)
		{
			reusableIndex1 = 1;
		}
		else if (reusableIndex1 == 1 && input.Keys.teamClearNext.Pressed)
		{
			reusableIndex1 = 2;
			reusableTimestamp = Time.time;
			bottomIndicator.Hide();
		}
		else if (reusableIndex1 == 2 && !taskGroup.Leader.Body.IsMoving && taskGroup.Leader.Body.Position >= secondRoom.LeftEdge + 1f && Time.time - reusableTimestamp > 1f)
		{
			input.Keys.teamClearNext.Enabled = false;
			InitToggleStun();
		}
	}

	private void InitToggleStun()
	{
		state++;
		bottomIndicator.Display(string.Format("Good. They are controlling the room. When entering rooms they can also use stun grenades. Press <{0}> to toggle their use of stun grenades.", input.Keys.teamToggleStun.KeyName));
		input.Keys.teamToggleStun.Enabled = true;
	}

	private void ToggleStun()
	{
		if (taskGroup.UseStunGrenades)
		{
			InitIncreaseTempo();
		}
	}

	private void InitIncreaseTempo()
	{
		state++;
		input.Keys.teamToggleStun.Enabled = false;
		input.Keys.teamIncreaseTempo.Enabled = true;
		bottomIndicator.Display(string.Format("Now they will use a stun grenade before entering the next room. Let's make them move faster too. Press <{0}> to make them sprint.", input.Keys.teamIncreaseTempo.KeyName));
	}

	private void IncreaseTempo()
	{
		if (taskGroup.tempo == CharacterBody.Tempo.Quick)
		{
			InitClearWithStun();
		}
	}

	private void InitClearWithStun()
	{
		state++;
		reusableIndex1 = 0;
		input.Keys.teamIncreaseTempo.Enabled = false;
		input.Keys.teamClearNext.Enabled = true;
		bottomIndicator.Display(string.Format("Now they will use a stun grenade before moving quickly into the room. Press <{0}> to give the order.", input.Keys.teamClearNext.KeyName));
	}

	private void ClearWithStun()
	{
		if (input.Keys.teamClearNext.Enabled && taskGroup.targetPosition > taskGroup.Leader.CurrentRoom.RightEdge)
		{
			input.Keys.teamClearNext.Enabled = false;
		}
		if (reusableIndex1 == 0 && taskGroup.Leader.Body.IsHoldingGrenade)
		{
			reusableIndex1 = 1;
			bottomIndicator.Hide();
		}
		if (thirdRoom.cleared && !taskGroup.Leader.Body.IsMoving)
		{
			InitClearSlow();
		}
	}

	private void InitClearSlow()
	{
		state++;
		reusableIndex1 = 0;
		input.Keys.teamClearNext.Enabled = false;
		input.Keys.teamDecreaseTempo.Enabled = true;
		bottomIndicator.Display(string.Format("The team can move slow and quiet as well. Press <{0}> two times to make them move slowly.", input.Keys.teamDecreaseTempo.KeyName));
	}

	private void ClearSlow()
	{
		if (reusableIndex1 == 0 && taskGroup.tempo == CharacterBody.Tempo.Slow)
		{
			reusableIndex1 = 1;
			input.Keys.teamDecreaseTempo.Enabled = false;
			input.Keys.teamToggleStun.Enabled = true;
			bottomIndicator.Display(string.Format("When moving carefully they will also check doors for traps.\nNow tell them to stop using the stun grenades again. Press <{0}> to do that.", input.Keys.teamToggleStun.KeyName));
		}
		else if (reusableIndex1 == 1 && !taskGroup.UseStunGrenades)
		{
			reusableIndex1 = 2;
			input.Keys.teamToggleStun.Enabled = false;
			input.Keys.teamClearNext.Enabled = true;
			bottomIndicator.Display(string.Format("Now press <{0}> to have them clear the next room.", input.Keys.teamClearNext.KeyName));
		}
		else if (reusableIndex1 == 2 && taskGroup.Leader.Body.IsMoving && taskGroup.Leader.Body.Position >= 19f)
		{
			reusableIndex1 = 3;
			bottomIndicator.Hide();
			input.Keys.teamClearNext.Enabled = false;
		}
		else if (reusableIndex1 == 3 && fourthRoom.cleared && !taskGroup.Leader.Body.IsMoving)
		{
			InitFollowPlayer();
		}
	}

	private void InitFollowPlayer()
	{
		state++;
		reusableIndex1 = 0;
		input.Keys.teamFollow.Enabled = true;
		bottomIndicator.Display(string.Format("You can instruct the team to follow you around by pressing <{0}>. Do that now.", input.Keys.teamFollow.KeyName));
	}

	private void FollowPlayer()
	{
		if (reusableIndex1 == 0 && playerGroup.ActiveMemberCount > 1)
		{
			reusableIndex1 = 1;
			bottomIndicator.Display(string.Format("Now they are following you again. Take them back into the previous room."));
			SetMarkerAndTrace(player.Pelvis.transform, new Vector3(14f, 0f, -1.5f));
			input.Keys.teamFollow.Enabled = false;
		}
		else if (reusableIndex1 == 1 && MoveDistance() < 0.25f)
		{
			reusableIndex1 = 2;
			SetPositionMarker(false);
			SetTargetTrace(false);
			input.EnableCategory(InputKey.Category.Movement, false);
			bottomIndicator.Display(string.Format("You can also tell the team to stop. Press <{0}> now to make them hold their position.", input.Keys.teamHold.KeyName));
			input.Keys.teamHold.Enabled = true;
		}
		else if (reusableIndex1 == 2 && playerGroup.ActiveMemberCount == 1)
		{
			reusableIndex1 = 3;
			input.Keys.teamHold.Enabled = false;
			input.EnableCategory(InputKey.Category.Movement);
			SetMarkerAndTrace(player.Pelvis.transform, new Vector3(fifthDoor.Position - 0.05f, 1f, -1.5f), 90f);
			bottomIndicator.Display(string.Format("Go to the indicated door. The team will stay behind."));
		}
		else if (reusableIndex1 == 3 && MoveDistance() < 1f)
		{
			InitMarksman();
			SetPositionMarker(false);
			SetTargetTrace(false);
		}
	}

	private void InitMarksman()
	{
		state++;
		reusableIndex1 = 0;
		input.EnableCategory(InputKey.Category.Interaction);
		bottomIndicator.Display(string.Format("Open the door and go into the next room."));
		SetMarkerAndTrace(player.Pelvis.transform, new Vector3(25.5f, 1f, -1.5f));
		marksman.TutorialReplaceAmmunition(marksmanAmmunition, true);
	}

	private void Marksman()
	{
		if (reusableIndex1 == 0 && MoveDistance() < 0.5f)
		{
			input.EnableCategory(InputKey.Category.Interaction, false);
			input.Keys.interact.Enabled = true;
			bottomIndicator.Display(string.Format("The team marksman lies in position outside the building. You can give the marksman orders just as you can the team. (Press {0})", input.Keys.interact.KeyName));
			reusableIndex1 = 1;
			SetPositionMarker(false);
			SetTargetTrace(false);
		}
		else if (reusableIndex1 == 1 && input.Keys.interact.Down)
		{
			reusableIndex1 = 2;
			bottomIndicator.Display(string.Format("The marksman's current target is highlighted by a round icon. When it is green the marksman has a clear shot and is ready to take it. (Press {0})", input.Keys.interact.KeyName));
			input.Keys.interact.DownHandled = true;
		}
		else if (reusableIndex1 == 2 && input.Keys.interact.Down)
		{
			reusableIndex1 = 3;
			bottomIndicator.Display(string.Format("When it is gray the marksman is not ready. If there is also a cross inside it means that there is not a clear line of fire. (Press {0})", input.Keys.interact.KeyName));
			input.Keys.interact.DownHandled = true;
			targetOccluder.SetActive(true);
		}
		else if (reusableIndex1 == 3 && input.Keys.interact.Down)
		{
			reusableIndex1 = 4;
			bottomIndicator.Display(string.Format("By default the marksman tracks the target closest to you. If you move around the room you will see the target change. (Press {0})", input.Keys.interact.KeyName));
			input.Keys.interact.DownHandled = true;
			targetOccluder.SetActive(false);
		}
		else if (reusableIndex1 == 4 && input.Keys.interact.Down)
		{
			reusableIndex1 = 5;
			interactionMenu.LongPressDisabledInTutorial = false;
			interactionMenu.TeamMenuDisabled = true;
			interactionMenu.MarksmanMenuDisabled = true;
			bottomIndicator.Display(string.Format("You can tag a specific target to make the marksman stick to it. You do this from the interaction menu. (Press {0})", input.Keys.interact.KeyName));
			input.Keys.interact.DownHandled = true;
		}
		else if (reusableIndex1 == 5 && input.Keys.interact.Down)
		{
			reusableIndex1 = 6;
			reusableIndex2 = 0;
			SetPositionMarker(true, marksmanTargets[1].transform.position + Vector3.up * 0.5f);
			bottomIndicator.Display(string.Format("Place the cursor on the leftmost target, then press and hold <{0}> to open the interaction menu.", input.Keys.interact.KeyName));
			input.Keys.interact.DownHandled = true;
		}
		else if (reusableIndex1 == 6 && interactionMenu.Visible)
		{
			if (interactionMenu.TargetAtPointer == marksmanTargets[1])
			{
				reusableIndex1 = 7;
				reusableIndex2 = 0;
				SetPositionMarker(false);
				bottomIndicator.Display(string.Format("Click the item named 'Tag for marksman'.", input.Keys.interact.KeyName));
			}
			else if (reusableIndex2 == 0)
			{
				bottomIndicator.Display(string.Format("First place the cursor on the leftmost target. Then press and hold <{0}>.", input.Keys.interact.KeyName));
				reusableIndex2 = 1;
			}
		}
		else if (reusableIndex1 == 7)
		{
			if (marksman.MarkedTarget == marksmanTargets[1])
			{
				reusableIndex1 = 8;
				interactionMenu.LongPressDisabledInTutorial = true;
				SetMarkerAndTrace(player.Pelvis.transform, new Vector3(27f, 0f, -1.5f));
				bottomIndicator.Display(string.Format("Now move out of the way."));
				interactionMenu.TeamMenuDisabled = false;
				interactionMenu.MarksmanMenuDisabled = false;
			}
			else if (reusableIndex2 == 0 && marksman.MarkedTarget != null)
			{
				bottomIndicator.Display(string.Format("You tagged {0}. Tag {1} to continue. It is the first one from the left.", marksman.MarkedTarget.name, marksmanTargets[1].name));
				reusableIndex2 = 1;
			}
		}
		else if (reusableIndex1 == 8 && MoveDistance() < 0.25f)
		{
			reusableIndex1 = 9;
			SetPositionMarker(false);
			SetTargetTrace(false);
			input.EnableCategory(InputKey.Category.Movement, false);
			input.Keys.marksmanEngage.Enabled = true;
			bottomIndicator.Display(string.Format("Give the marksman order to fire on the current target by pressing <{0}>.", input.Keys.marksmanEngage.KeyName));
		}
		else if (reusableIndex1 == 9 && !marksmanTargets[1].active)
		{
			reusableIndex1 = 10;
			bottomIndicator.Display(string.Format("With the order you gave, the marksman only fires once. The icon in the top left corner displays the marksman's current status. (Press <{0}>)", input.Keys.interact.KeyName));
			input.EnableCategory(InputKey.Category.Movement);
			input.Keys.marksmanEngage.Enabled = false;
			uiArrow.SetActive(true);
			mission.InteriorMode.UI.marksmanIndicator.gameObject.SetActive(true);
		}
		else if (reusableIndex1 == 10 && input.Keys.interact.Down)
		{
			reusableIndex1 = 11;
			bottomIndicator.Display(string.Format("Move to the far side of the targets."));
			SetMarkerAndTrace(player.Pelvis.transform, new Vector3(32.5f, 0f, -1.5f));
			input.Keys.interact.DownHandled = true;
			uiArrow.SetActive(false);
		}
		else if (reusableIndex1 == 11 && MoveDistance() < 0.25f)
		{
			reusableIndex1 = 12;
			SetPositionMarker(false);
			SetTargetTrace(false);
			input.EnableCategory(InputKey.Category.Movement, false);
			bottomIndicator.Display(string.Format("You can also let the marksman engage freely. The marksman then engages all available targets in turn. (Press <{0}>)", input.Keys.interact.KeyName));
			input.Keys.interact.DownHandled = true;
			uiArrow.SetActive(false);
		}
		else if (reusableIndex1 == 12 && input.Keys.interact.Down)
		{
			reusableIndex1 = 13;
			input.Keys.marksmanFree.Enabled = true;
			bottomIndicator.Display(string.Format("Press <{0}> to have the marksman engage the remaining targets.", input.Keys.marksmanFree.KeyName));
			input.Keys.interact.DownHandled = true;
		}
		else if (reusableIndex1 == 13 && !marksmanTargets[0].active && !marksmanTargets[2].active && !marksmanTargets[3].active)
		{
			reusableIndex1 = 14;
			bottomIndicator.Display(string.Format("All the key commands used so far are also available from the interaction menu. (Press <{0}>)", input.Keys.interact.KeyName));
			sixthDoor.inoperable = true;
		}
		else if (reusableIndex1 == 14 && input.Keys.interact.Down)
		{
			state++;
		}
	}

	private void TutorialComplete()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			mission.ReturnToTitle();
		}
		if (!completed)
		{
			bottomIndicator.Display("That concludes the tutorial. Press <Escape> to exit.");
			input.EnableAll();
			SetTargetTrace(false);
			SetPositionMarker(false);
			interactionMenu.QuickActionVisualizationDisabled = false;
			interactionMenu.ShortPressDisabledInTutorial = false;
			interactionMenu.LongPressDisabledInTutorial = false;
			interactionMenu.TeamMenuDisabled = false;
			interactionMenu.MarksmanMenuDisabled = false;
			completed = true;
		}
	}

	private void StartArrowIndication(Transform source, Transform target)
	{
		indicationTimer = 0f;
		arrow.gameObject.SetActive(true);
		indicationSource = source;
		indicationTargetTransform = target;
	}

	private void SetTargetTrace(bool enable, Transform source = null, Vector3 position = default(Vector3))
	{
		arrow.gameObject.SetActive(enable);
		if (enable)
		{
			indicationTimer = 0f;
			indicationSource = source;
			indicationTarget.transform.position = position;
			indicationTargetTransform = indicationTarget;
		}
	}

	private void SetMarkerAndTrace(Transform source, Vector3 position, float angle = 0f)
	{
		SetTargetTrace(true, source, position);
		SetPositionMarker(true, position, angle);
	}

	private void UpdateArrowIndication()
	{
		if (arrow.gameObject.activeInHierarchy)
		{
			indicationTimer += Time.deltaTime;
			if (indicationTimer >= 1f)
			{
				indicationTimer = 0f;
			}
			Vector3 vector = indicationTargetTransform.transform.position - indicationSource.transform.position;
			float magnitude = vector.magnitude;
			arrow.position = indicationSource.transform.position + vector.normalized * magnitude * arrowSpeedFunction.Evaluate(indicationTimer);
			float z = Mathf.Atan2(vector.y, vector.x) * 57.29578f;
			arrow.rotation = Quaternion.Euler(0f, 0f, z);
		}
	}

	private float MoveDistance()
	{
		return Mathf.Abs(indicationTargetTransform.transform.position.x - player.transform.position.x);
	}

	private float GroupMoveDistance()
	{
		if (taskGroup.Leader == null)
		{
			return float.PositiveInfinity;
		}
		return Mathf.Abs(indicationTargetTransform.transform.position.x - taskGroup.Leader.Body.Position);
	}

	private void SetPositionMarker(bool enable, Vector3 position = default(Vector3), float angle = 0f)
	{
		indicatorArrow.SetActive(enable);
		if (enable)
		{
			Transform obj = indicatorArrow.transform;
			indicationTargetTransform.transform.position = position;
			obj.position = position;
			indicatorArrow.transform.rotation = Quaternion.Euler(0f, 0f, angle);
		}
	}

	public Target TargetClosestToPosition(float position)
	{
		Target result = null;
		float num = float.PositiveInfinity;
		foreach (Target marksmanTarget in marksmanTargets)
		{
			if (marksmanTarget.active)
			{
				float num2 = Mathf.Abs(marksmanTarget.transform.position.x - position);
				if (num2 < num)
				{
					num = num2;
					result = marksmanTarget;
				}
			}
		}
		return result;
	}
}

}

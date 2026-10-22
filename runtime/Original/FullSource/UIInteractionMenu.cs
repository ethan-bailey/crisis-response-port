namespace Crisis.Runtime.FullSource
{
using System.Collections.Generic;
using UnityEngine;

public class UIInteractionMenu : MonoBehaviour
{
	public delegate bool InteractionCondition();

	public delegate void InteractionAction();

	public Dictionary<string, UIInteractionMenuItem> items = new Dictionary<string, UIInteractionMenuItem>();

	[Tooltip("The menu will float this distance away from the player character.")]
	public float offsetDistance = 1.5f;

	[Tooltip("How far away from obstacles the menu should be stopped.")]
	public float obstacleClearance = 0.5f;

	public float heightAboveGround = 0.6f;

	public float angle = 20f;

	public float updateInterval = 0.1f;

	[Header("References")]
	public GameObject menu;

	public GameObject menuItemPrefab;

	public Transform actionsContainer;

	public Transform teamActionsContainer;

	public Transform marksmanActionsContainer;

	private Mission mission;

	private Marksman marksman;

	private InputHandler input;

	private Operators operators;

	private Canvas parentCanvas;

	private CharacterBody playerBody;

	private TeamOperator playerOperator;

	private RectTransform rectTransform;

	private TeamController teamController;

	private PlayerController playerController;

	private UIHotkeysIndicator hotkeysIndicator;

	private UIInteractionMenuItem noActionItem;

	private UIInteractionMenuItem tagForMarksmanItem;

	private UIInteractionMenuItem gauzeItem;

	private UIInteractionMenuItem restrainItem;

	private UIInteractionMenuItem secureWeaponItem;

	private UIInteractionMenuItem pickUpWeaponItem;

	private UIInteractionMenuItem checkVitalsItem;

	private UIInteractionMenuItem marksmanDisregardItem;

	private UIInteractionMenuItem toggleStunIten;

	private int obstacleMask;

	private float zPos;

	private float lastEvaluation = -100f;

	private float occupantAtPointerEvaluation;

	private float actionCancelledTimestamp;

	private bool phoneDropEnabled;

	private float phoneDropPosition;

	private float phoneDropRadius;

	private bool standDownEnabled;

	private float standDownLeft;

	private float standDownRight;

	private bool resumeCursorVisibility;

	public UIInteriorModeTextIndicator TextIndicator { get; private set; }

	public UIInteractionMenuItem AvailableAction { get; private set; }

	public UIInteractionMenuItem ExecutingAction { get; private set; }

	public UIInteractionMenuItem CancelledAction { get; private set; }

	public bool Visible { get; private set; }

	public bool CurrentActionCancelled { get; private set; }

	public Target TargetAtPointer { get; private set; }

	public bool QuickActionVisualizationDisabled { get; set; }

	public bool ShortPressDisabledInTutorial { get; set; }

	public bool LongPressDisabledInTutorial { get; set; }

	public bool TeamMenuDisabled { get; set; }

	public bool MarksmanMenuDisabled { get; set; }

	private void Awake()
	{
		menu.SetActive(false);
		mission = Global.Context.Mission;
		rectTransform = GetComponent<RectTransform>();
	}

	private void Start()
	{
		operators = mission.Operators;
		input = Global.Context.InputHandler;
		TextIndicator = mission.UI.indicatorLowerLeft;
		obstacleMask = Masks.Instance.BallisticTargetsSansLimbs;
		parentCanvas = mission.InteriorMode.UI.Canvas;
		zPos = parentCanvas.transform.position.z;
		marksman = mission.Marksman;
		teamController = mission.Operators.MainGroup;
		MissionParameters missionParameters = Global.Context.CampaignManager.MissionParameters;
		if (missionParameters != null)
		{
			phoneDropEnabled = missionParameters.phoneDropOffEnabled;
			phoneDropPosition = missionParameters.phoneDropOffPosition;
			phoneDropRadius = missionParameters.phoneDropOffArea;
			standDownEnabled = missionParameters.teamStandDownEnabled;
			standDownLeft = missionParameters.teamStandDownLeftPosition;
			standDownRight = missionParameters.teamStandDownRightPosition;
		}
		hotkeysIndicator = mission.InteriorMode.UI.hotkeysIndicator;
		mission.UI.PauseMenu.onShow.AddListener(Hide);
		BuildMenu();
	}

	private void BuildMenu()
	{
		UIInteractionMenuItem uIInteractionMenuItem = CreateAction("No action available", string.Empty, string.Empty, null, NoActionsAvailable, false);
		noActionItem = uIInteractionMenuItem.GetComponent<UIInteractionMenuItem>();
		uIInteractionMenuItem.transform.SetParent(actionsContainer, false);
		AddAction("Disarm trap", "Disarming...", string.Empty, DisarmTrap, DisarmTrapAvailable);
		AddAction("Remove obstacle", "Removing...", string.Empty, RemoveObstacle, RemoveObstacleAvailable);
		AddAction("Open door", string.Empty, string.Empty, OpenDoor, OpenDoorAvailable);
		secureWeaponItem = AddAction("Secure weapon", "Securing weapon...", string.Empty, SecureWeapon, SecureWeaponAvailable);
		restrainItem = AddAction("Restrain", "Restraining...", string.Empty, Restrain, RestrainAvailable);
		AddAction("Close door", string.Empty, string.Empty, CloseDoor, CloseDoorAvailable);
		AddAction("Lock door", string.Empty, string.Empty, LockDoor, LockDoorAvailable);
		AddAction("Unlock door", string.Empty, string.Empty, UnlockDoor, UnlockDoorAvailable);
		gauzeItem = AddAction("Use gauze", "Applying gauze...", string.Empty, UseGauze, UseGauzeAvailable);
		AddAction("Examine door", "Examining...", string.Empty, ExamineDoor, ExamineDoorAvailable);
		AddAction("Finish mission", string.Empty, string.Empty, FinishMission, FinishMissionAvailable, null, true, true);
		AddAction("Use gauze on self", "Applying gauze...", string.Empty, UseGauzeOnSelf, UseGauzeOnSelfAvailable);
		checkVitalsItem = AddAction("Check vitals", "Checking vitals...", string.Empty, CheckVitals, CheckVitalsAvailable);
		tagForMarksmanItem = AddAction("Tag for marksman", string.Empty, string.Empty, TagForMarksman, TagForMarksmanAvailable);
		AddAction("Drop weapon", string.Empty, string.Format("<{0}>", input.Keys.dropWeapon.KeyName), DropWeapon, DropWeaponAvailable, null, true, true);
		pickUpWeaponItem = AddAction("Pick up weapon", "Picking up weapon...", string.Empty, PickUpWeapon, PickUpWeaponAvailable);
		AddAction("Toggle power", string.Empty, string.Empty, TogglePower, TogglePowerAvailable);
		if (phoneDropEnabled)
		{
			AddAction("Place cell phone", string.Empty, string.Empty, PlaceCellPhone, PlaceCellPhoneAvailable);
		}
		if (standDownEnabled)
		{
			AddAction("Team stand down", string.Empty, string.Empty, StandDown, StandDownAvailable);
		}
		AddAction("Team...", string.Empty, string.Empty, OpenTeamMenu, TeamMenuAvailable, null, false);
		AddAction("Marksman...", string.Empty, string.Empty, OpenMarksmanMenu, MarksmanMenuAvailable, null, false);
		AddAction("Move there", string.Empty, string.Format("<{0}>", input.Keys.teamMoveTo.KeyName), TeamMoveThere, null, teamActionsContainer);
		AddAction("Clear next room", string.Empty, string.Format("<{0}>", input.Keys.teamClearNext.KeyName), TeamClearNextRoom, TeamClearNextRoomAvailable, teamActionsContainer);
		AddAction("Move ahead", string.Empty, string.Format("<{0}>", input.Keys.teamMoveOut.KeyName), TeamMoveAhead, null, teamActionsContainer);
		AddAction("Hold position", string.Empty, string.Format("<{0}>", input.Keys.teamHold.KeyName), TeamHoldPosition, null, teamActionsContainer);
		AddAction("Follow me", string.Empty, string.Format("<{0}>", input.Keys.teamFollow.KeyName), TeamFollowMe, null, teamActionsContainer);
		AddAction("Increase tempo", string.Empty, string.Format("<{0}>", input.Keys.teamIncreaseTempo.KeyName), TeamIncreaseTempo, TeamIncreaseTempoAvailable, teamActionsContainer);
		AddAction("Decrease tempo", string.Empty, string.Format("<{0}>", input.Keys.teamDecreaseTempo.KeyName), TeamDecreaseTempo, TeamDecreaseTempoAvailable, teamActionsContainer);
		toggleStunIten = AddAction(ToggleStunItemText(), string.Empty, string.Format("<{0}>", input.Keys.teamToggleStun.KeyName), TeamToggleStun, null, teamActionsContainer);
		AddAction("Switch to marksman", string.Empty, string.Format("<{0}>", input.Keys.marksman.KeyName), () =>
		{
			mission.SetMode(Mission.MissionView.Marksman);
		}, MarksmanMenuAvailable, marksmanActionsContainer);
		AddAction("Engage target", string.Empty, string.Format("<{0}>", input.Keys.marksmanEngage.KeyName), MarksmanEngage, MarksmanEngageAvailable, marksmanActionsContainer);
		AddAction("Engage freely", string.Empty, string.Format("<{0}>", input.Keys.marksmanFree.KeyName), MarksmanEngageFreely, MarksmanEngageFreelyAvailable, marksmanActionsContainer);
		AddAction("Hold fire", string.Empty, string.Format("<{0}>", input.Keys.marksmanHold.KeyName), MarksmanHold, MarksmanHoldAvailable, marksmanActionsContainer);
		marksmanDisregardItem = AddAction("Disregard target", string.Empty, string.Empty, MarksmanDisregard, MarksmanDisregardAvailable, marksmanActionsContainer);
	}

	public void Show()
	{
		if (SetPlayerReference())
		{
			resumeCursorVisibility = Cursor.visible;
			Cursor.visible = true;
			menu.SetActive(true);
			base.transform.position = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f);
			hotkeysIndicator.Show();
			UpdateActionAvailability();
			Visible = true;
		}
	}

	public void Hide()
	{
		menu.SetActive(false);
		actionsContainer.gameObject.SetActive(true);
		teamActionsContainer.gameObject.SetActive(false);
		marksmanActionsContainer.gameObject.SetActive(false);
		hotkeysIndicator.Hide();
		Cursor.visible = resumeCursorVisibility;
		Visible = false;
	}

	private UIInteractionMenuItem AddAction(string text, string executionText, string indexText, InteractionAction action, InteractionCondition condition = null, Transform parent = null, bool executable = true, bool noQuickExecute = false)
	{
		UIInteractionMenuItem uIInteractionMenuItem = CreateAction(text, executionText, indexText, action, condition, executable, noQuickExecute);
		uIInteractionMenuItem.transform.SetParent(parent ?? actionsContainer, false);
		items.Add(text, uIInteractionMenuItem);
		return uIInteractionMenuItem;
	}

	private UIInteractionMenuItem CreateAction(string text, string executionText, string indexText, InteractionAction action, InteractionCondition condition = null, bool executable = true, bool noQuickExecute = false)
	{
		GameObject gameObject = Object.Instantiate(menuItemPrefab);
		gameObject.name = text;
		UIInteractionMenuItem component = gameObject.GetComponent<UIInteractionMenuItem>();
		component.Initialize(this, text, executionText, indexText, action, condition, executable, noQuickExecute);
		return component;
	}

	public bool EnableAction(string action, bool enable)
	{
		UIInteractionMenuItem value;
		if (items.TryGetValue(action, out value))
		{
			value.disabled = !enable;
			return true;
		}
		return false;
	}

	private void Update()
	{
		if (!ShortPressDisabledInTutorial && input.Keys.interact.UpShort && SetPlayerReference())
		{
			ExecuteFirstAction();
			Hide();
		}
		if (!LongPressDisabledInTutorial && !Visible && input.Keys.interact.DownLong)
		{
			Show();
		}
		if (Visible && !input.Keys.interact.Pressed)
		{
			Hide();
		}
		UpdateActionAvailability();
		if (playerBody != null && Visible)
		{
			if (playerBody.IsRagdoll)
			{
				Hide();
			}
			else
			{
				UpdatePosition();
			}
		}
	}

	private void UpdatePosition()
	{
		Vector3 position = new Vector3(Input.mousePosition.x, base.transform.position.y, 0f);
		Rect rect = rectTransform.rect;
		Rect rect2 = parentCanvas.GetComponent<RectTransform>().rect;
		Vector2 vector = rect.size * 0.5f;
		position.x = Mathf.Clamp(position.x, vector.x, rect2.width * parentCanvas.scaleFactor - vector.x);
		position.y = Mathf.Clamp(position.y, vector.y, rect2.height * parentCanvas.scaleFactor - vector.y);
		base.transform.position = position;
	}

	public void CancelAction()
	{
		if (!CurrentActionCancelled && ExecutingAction != null)
		{
			actionCancelledTimestamp = Time.realtimeSinceStartup;
			CurrentActionCancelled = true;
			CancelledAction = ExecutingAction;
			ExecutingAction = null;
		}
	}

	public void ExecuteAction(UIInteractionMenuItem action = null)
	{
		if (!(action == null) && action.executable)
		{
			CancelledAction = null;
			ExecutingAction = action;
			if (string.IsNullOrEmpty(action.executionText))
			{
				SetActionComplete();
			}
		}
	}

	public void SetActionComplete()
	{
		ExecutingAction = null;
	}

	private void UpdateActionAvailability()
	{
		if (Time.realtimeSinceStartup - lastEvaluation < updateInterval)
		{
			return;
		}
		if (CurrentActionCancelled && Time.realtimeSinceStartup - actionCancelledTimestamp >= 1f)
		{
			CurrentActionCancelled = false;
			CancelledAction = null;
		}
		lastEvaluation = Time.realtimeSinceStartup;
		AvailableAction = null;
		SetPlayerReference();
		if (playerBody == null)
		{
			return;
		}
		Transform transform = null;
		transform = (teamActionsContainer.gameObject.activeInHierarchy ? teamActionsContainer : ((!marksmanActionsContainer.gameObject.activeInHierarchy) ? actionsContainer : marksmanActionsContainer));
		for (int i = 0; i < transform.childCount; i++)
		{
			Transform child = transform.GetChild(i);
			UIInteractionMenuItem component = child.GetComponent<UIInteractionMenuItem>();
			if (component.executable && component.EvaluateAndSetAvailability() && !component.noQuickExecute && AvailableAction == null)
			{
				AvailableAction = component;
			}
		}
	}

	private void ExecuteFirstAction()
	{
		for (int i = 0; i < actionsContainer.childCount; i++)
		{
			UIInteractionMenuItem component = actionsContainer.GetChild(i).GetComponent<UIInteractionMenuItem>();
			if (!(component == null) && component.executable && !component.noQuickExecute && component.action != null && component.EvaluateAvailability())
			{
				ExecutingAction = component;
				ExecutingAction.ExecuteAction();
				CurrentActionCancelled = false;
				break;
			}
		}
	}

	private bool SetPlayerReference()
	{
		playerBody = null;
		for (int i = 0; i < operators.Bodies.Count; i++)
		{
			CharacterBody characterBody = operators.Bodies[i];
			if (!(characterBody.GetComponent<PlayerController>() == null))
			{
				playerBody = characterBody.GetComponent<CharacterBody>();
				playerOperator = characterBody.GetComponent<TeamOperator>();
				playerController = characterBody.GetComponent<PlayerController>();
				return true;
			}
		}
		return false;
	}

	public bool FinishMissionAvailable()
	{
		return mission.Occupants != null && mission.Occupants.AllRenderedSafe;
	}

	public bool NoActionsAvailable()
	{
		for (int i = 0; i < actionsContainer.childCount; i++)
		{
			GameObject gameObject = actionsContainer.GetChild(i).gameObject;
			if (!(gameObject.GetComponent<UIInteractionMenuItem>() == noActionItem) && gameObject.activeInHierarchy)
			{
				return false;
			}
		}
		return true;
	}

	public bool OpenDoorAvailable()
	{
		DoorController doorController = playerBody.FindDoor(2f);
		if (doorController == null || doorController.inoperable || !doorController.InteractorInRange(playerBody))
		{
			return false;
		}
		DoorLockState knownLockState = doorController.KnownLockState;
		if (doorController.openState == DoorOpenState.Closed && knownLockState != DoorLockState.Locked && knownLockState != DoorLockState.Locking)
		{
			return true;
		}
		return false;
	}

	public bool CloseDoorAvailable()
	{
		DoorController doorController = playerBody.FindDoor(2f);
		if (doorController == null || doorController.inoperable || !doorController.InteractorInRange(playerBody))
		{
			return false;
		}
		if (doorController.Angle < 90f)
		{
			return false;
		}
		return true;
	}

	public bool LockDoorAvailable()
	{
		DoorController doorController = playerBody.FindDoor(playerBody.interactionRange);
		if (doorController == null || doorController.inoperable || !doorController.InteractorInRange(playerBody))
		{
			return false;
		}
		if (doorController.openState == DoorOpenState.Closed && doorController.KnownLockState == DoorLockState.Unlocked)
		{
			return true;
		}
		return false;
	}

	public bool UnlockDoorAvailable()
	{
		DoorController doorController = playerBody.FindDoor(playerBody.interactionRange);
		if (doorController == null || doorController.inoperable || !doorController.InteractorInRange(playerBody))
		{
			return false;
		}
		if (doorController.openState == DoorOpenState.Closed && doorController.KnownLockState == DoorLockState.Locked)
		{
			return true;
		}
		return false;
	}

	public bool ExamineDoorAvailable()
	{
		DoorController doorController = playerBody.FindDoor(playerBody.interactionRange);
		if (doorController == null || !doorController.InteractorInRange(playerBody))
		{
			return false;
		}
		bool flag = doorController.openState == DoorOpenState.Closed;
		bool flag2 = doorController.KnownLockState == DoorLockState.Unknown;
		bool flag3 = doorController.TrapState == DoorController.DoorTrapState.Unknown;
		return flag && (flag2 || flag3);
	}

	public bool SecureWeaponAvailable()
	{
		WeaponBase weaponBase = playerBody.FindWeapon(playerBody.interactionRange * 0.5f);
		if (weaponBase != null)
		{
			pickUpWeaponItem.SetText((!playerBody.HasFreeWeaponSlot) ? "Secure weapon" : string.Format("Pick up {0}", weaponBase.name));
			return true;
		}
		return false;
	}

	public bool PickUpWeaponAvailable()
	{
		WeaponBase weaponBase = playerBody.FindWeapon(playerBody.interactionRange * 0.5f);
		if (weaponBase != null)
		{
			pickUpWeaponItem.SetText(string.Format("Pick up {0}", weaponBase.name));
			return true;
		}
		return false;
	}

	public bool DropWeaponAvailable()
	{
		return playerBody.ActiveWeapon != null;
	}

	public bool RestrainAvailable()
	{
		if (!playerBody.Equipment.plasticuffs)
		{
			return false;
		}
		List<CharacterBody> list = playerBody.FindBodies(true, 2f, true, false);
		foreach (CharacterBody item in list)
		{
			if (item.IsCompliantForInteraction && !item.IsRestrained)
			{
				restrainItem.SetText(string.Format("Restrain ({0})", item.name));
				return true;
			}
		}
		return false;
	}

	public bool CheckVitalsAvailable()
	{
		List<CharacterBody> list = playerBody.FindBodies();
		if (list.Count == 0)
		{
			return false;
		}
		checkVitalsItem.SetText(string.Format("Check vitals ({0})", list[0].name));
		return true;
	}

	public bool UseGauzeOnSelfAvailable()
	{
		if (playerBody.Physiology.invulnerable || playerBody.Equipment.gauze == 0)
		{
			return false;
		}
		if (playerBody.Physiology.HemorrhagingScore() == 1f)
		{
			return false;
		}
		return true;
	}

	public bool UseGauzeAvailable()
	{
		if (playerBody.Equipment.gauze == 0)
		{
			return false;
		}
		List<CharacterBody> list = playerBody.FindBodies();
		foreach (CharacterBody item in list)
		{
			if (item.CanBeGauzed)
			{
				gauzeItem.SetText(string.Format("Use gauze ({0})", item.name));
				return true;
			}
		}
		return false;
	}

	public bool TeamMenuAvailable()
	{
		return !TeamMenuDisabled && playerOperator.Follower != null;
	}

	public bool TeamOpenDoorAvailable()
	{
		return false;
	}

	public bool TeamClearNextRoomAvailable()
	{
		return playerOperator.CommandClearNextRoomAvailable();
	}

	public bool TeamIncreaseTempoAvailable()
	{
		return operators.MainGroup.tempo < CharacterBody.Tempo.Quick;
	}

	public void TeamIncreaseTempo()
	{
		operators.CommandIncreaseTempo();
	}

	public bool TeamDecreaseTempoAvailable()
	{
		return operators.MainGroup.tempo > CharacterBody.Tempo.Slow;
	}

	public void TeamDecreaseTempo()
	{
		operators.CommandDecreaseTempo();
	}

	public bool MarksmanMenuAvailable()
	{
		return !MarksmanMenuDisabled && marksman != null;
	}

	public void FinishMission()
	{
		mission.UI.PauseMenu.ConfirmStandDownInteraction();
	}

	public void CloseDoor()
	{
		if (!(playerBody == null))
		{
			DoorController doorController = playerBody.FindDoor(playerBody.interactionRange);
			if (!(doorController == null) && doorController.InteractorInRange(playerBody))
			{
				doorController.Close(playerBody, true);
			}
		}
	}

	public void OpenDoor()
	{
		if (!(playerBody == null))
		{
			DoorController doorController = playerBody.FindDoor(playerBody.interactionRange);
			if (!(doorController == null) && doorController.InteractorInRange(playerBody))
			{
				doorController.Open(playerBody, true);
			}
		}
	}

	public void UnlockDoor()
	{
		if (!(playerBody == null))
		{
			DoorController doorController = playerBody.FindDoor(playerBody.interactionRange);
			if (!(doorController == null) && doorController.InteractorInRange(playerBody))
			{
				doorController.Unlock(playerBody, true);
			}
		}
	}

	public void LockDoor()
	{
		if (!(playerBody == null))
		{
			DoorController doorController = playerBody.FindDoor(playerBody.interactionRange);
			if (!(doorController == null) && doorController.InteractorInRange(playerBody))
			{
				doorController.Lock(playerBody, true);
			}
		}
	}

	public bool DisarmTrapAvailable()
	{
		DoorController doorController = playerBody.FindDoor(playerBody.interactionRange);
		if (doorController == null || !doorController.InteractorInRange(playerBody))
		{
			return false;
		}
		return doorController.IsRigged;
	}

	public void DisarmTrap()
	{
		playerBody.PendingAction = CharacterBody.CharacterAction.DisarmTrap;
	}

	public bool RemoveObstacleAvailable()
	{
		if (playerBody == null)
		{
			return false;
		}
		return playerBody.FindObstacle() != null;
	}

	public void RemoveObstacle()
	{
		if (playerBody != null && playerBody.FindObstacle() != null)
		{
			playerBody.PendingAction = CharacterBody.CharacterAction.RemoveObstacle;
		}
	}

	public void ExamineDoor()
	{
		playerBody.PendingAction = CharacterBody.CharacterAction.ExamineDoor;
	}

	public void SecureWeapon()
	{
		if (!(playerBody == null))
		{
			WeaponBase weaponBase = playerBody.FindWeapon(playerBody.interactionRange * 0.5f);
			if (!(weaponBase == null))
			{
				playerBody.Blackboard.SecureWeaponTarget = weaponBase;
				playerBody.PendingAction = CharacterBody.CharacterAction.SecureWeapon;
				playerBody.Blackboard.EquipCollectedWeapon = playerBody.HasFreeWeaponSlot;
			}
		}
	}

	public void PickUpWeapon()
	{
		if (!(playerBody == null))
		{
			WeaponBase weaponBase = playerBody.FindWeapon(playerBody.interactionRange * 0.5f);
			if (!(weaponBase == null))
			{
				playerBody.Blackboard.SecureWeaponTarget = weaponBase;
				playerBody.PendingAction = CharacterBody.CharacterAction.SecureWeapon;
				playerBody.Blackboard.EquipCollectedWeapon = true;
			}
		}
	}

	public void DropWeapon()
	{
		if (!(playerBody == null))
		{
			WeaponBase activeWeapon = playerBody.ActiveWeapon;
			if (activeWeapon != null)
			{
				playerBody.DropWeapon(activeWeapon);
				playerBody.armController.SwitchWeapons();
			}
		}
	}

	public void CheckVitals()
	{
		if (!(playerBody == null))
		{
			List<CharacterBody> list = playerBody.FindBodies();
			if (list.Count != 0)
			{
				CharacterBody characterInteractionTarget = list[0];
				playerBody.Blackboard.CharacterInteractionTarget = characterInteractionTarget;
				playerBody.PendingAction = CharacterBody.CharacterAction.CheckVitals;
			}
		}
	}

	public void UseGauzeOnSelf()
	{
		playerBody.ApplyGauzeOnSelf();
	}

	public void UseGauze()
	{
		if (playerBody == null)
		{
			return;
		}
		List<CharacterBody> list = playerBody.FindBodies(true, 2f, false);
		foreach (CharacterBody item in list)
		{
			if (item.IsCompliantForInteraction && !item.Physiology.Exsanguinated && item.Physiology.Hemorrhaging > 0f)
			{
				playerBody.Blackboard.PendingGauzeTarget = item;
				return;
			}
		}
		List<CharacterBody> list2 = playerBody.FindBodies(true, 2f, true, false);
		foreach (CharacterBody item2 in list2)
		{
			if (item2.IsCompliantForInteraction && !item2.Physiology.Exsanguinated && item2.Physiology.Hemorrhaging > 0f)
			{
				playerBody.Blackboard.PendingGauzeTarget = item2;
				break;
			}
		}
	}

	public void Restrain()
	{
		if (playerBody == null)
		{
			return;
		}
		List<CharacterBody> list = playerBody.FindBodies(true, 2f, true, false);
		foreach (CharacterBody item in list)
		{
			if (!item.IsRestrained && item.IsCompliantForInteraction && !item.IsOperator)
			{
				playerBody.Blackboard.PendingRestrainTarget = item;
				break;
			}
		}
	}

	public bool PlaceCellPhoneAvailable()
	{
		if (playerBody == null)
		{
			return false;
		}
		if (mission.negotiationMode.PhoneDelivered)
		{
			return false;
		}
		if (Mathf.Abs(phoneDropPosition - playerBody.Position) < phoneDropRadius)
		{
			return true;
		}
		return false;
	}

	public void PlaceCellPhone()
	{
		playerBody.GetComponent<TeamOperator>().DropCellPhone();
	}

	public bool StandDownAvailable()
	{
		if (playerBody == null)
		{
			return false;
		}
		if (playerBody.Position >= standDownLeft && playerBody.Position <= standDownRight)
		{
			return true;
		}
		return false;
	}

	public void StandDown()
	{
		mission.EntryTeamStandDown();
	}

	public bool TogglePowerAvailable()
	{
		if (playerBody == null)
		{
			return false;
		}
		if (playerBody.Position <= -8f)
		{
			return true;
		}
		return false;
	}

	public void TogglePower()
	{
		mission.toggleElectricity.Invoke(playerBody);
	}

	public void OpenTeamMenu()
	{
		actionsContainer.gameObject.SetActive(false);
		teamActionsContainer.gameObject.SetActive(true);
	}

	public void TeamFollowMe()
	{
		operators.CommandFollowMe();
	}

	public void TeamMoveThere()
	{
		operators.CommandMoveTo();
	}

	public void TeamMoveAhead()
	{
		operators.CommandMoveOut();
	}

	public void TeamHoldPosition()
	{
		operators.CommandHoldPosition();
	}

	public void TeamClearNextRoom()
	{
		operators.CommandClearNextRoom();
	}

	public void TeamToggleStun()
	{
		operators.CommandToggleStunUsage();
		toggleStunIten.SetText(ToggleStunItemText());
	}

	private string ToggleStunItemText()
	{
		return string.Format("{0}", (!operators.MainGroup.UseStunGrenades) ? "Use stun" : "Dont use stun");
	}

	public void OpenMarksmanMenu()
	{
		actionsContainer.gameObject.SetActive(false);
		marksmanActionsContainer.gameObject.SetActive(true);
	}

	public bool MarksmanEngageAvailable()
	{
		return marksman.computerMode != Marksman.ComputerMode.Engage;
	}

	public void MarksmanEngage()
	{
		marksman.ComputerCommand(Marksman.ComputerMode.Engage);
	}

	public bool TagForMarksmanAvailable()
	{
		if (marksman == null)
		{
			return false;
		}
		if (Time.realtimeSinceStartup - occupantAtPointerEvaluation >= 0.1f)
		{
			float x = mission.InteriorMode.UI.weaponPointer.WorldPoint.x;
			if (mission.TutorialMode.Active)
			{
				TeamTutorial teamTutorial = mission.TutorialMode.Tutorial as TeamTutorial;
				if (teamTutorial != null)
				{
					TargetAtPointer = teamTutorial.TargetClosestToPosition(x);
				}
			}
			else
			{
				CharacterBody characterBody = mission.Occupants.ClosestToPosition(x);
				if (characterBody != null)
				{
					bool flag = Mathf.Abs(characterBody.Position - x) <= 1f;
					bool flag2 = Physics.Raycast(playerBody.RightUpperArm.transform.position, Vector3.right * Mathf.Sign(characterBody.Position - playerBody.Position), Mathf.Abs(characterBody.Position - playerBody.Position), Masks.Instance.Doors);
					if (!flag || flag2)
					{
						characterBody = null;
					}
				}
				TargetAtPointer = ((!(characterBody != null)) ? null : characterBody.Target);
			}
			occupantAtPointerEvaluation = Time.realtimeSinceStartup;
			if (TargetAtPointer != null)
			{
				tagForMarksmanItem.SetText(string.Format("Tag for marksman ({0})", TargetAtPointer.name));
			}
		}
		return TargetAtPointer != null && TargetAtPointer != marksman.MarkedTarget;
	}

	public void TagForMarksman()
	{
		if (!(TargetAtPointer == null))
		{
			marksman.ComputerSetTarget(TargetAtPointer);
			marksmanDisregardItem.SetText(string.Format("Disregard {0}", TargetAtPointer.name));
		}
	}

	public bool MarksmanDisregardAvailable()
	{
		return marksman.MarkedTarget != null;
	}

	public void MarksmanDisregard()
	{
		marksman.ComputerSetTarget(null);
	}

	public bool MarksmanEngageFreelyAvailable()
	{
		return marksman.computerMode != Marksman.ComputerMode.Free;
	}

	public void MarksmanEngageFreely()
	{
		marksman.ComputerCommand(Marksman.ComputerMode.Free);
	}

	public bool MarksmanHoldAvailable()
	{
		return marksman.computerMode != Marksman.ComputerMode.Hold;
	}

	public void MarksmanHold()
	{
		marksman.ComputerCommand(Marksman.ComputerMode.Hold);
	}
}

}

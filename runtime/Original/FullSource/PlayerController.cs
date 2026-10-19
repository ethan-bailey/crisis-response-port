namespace Crisis.Runtime.FullSource
{
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	private InputHandler input;

	private InteriorCamera interiorCamera;

	private UIFireModeIndicator fireModeIndicator;

	private UIMagazineIndicator magazineIndicator;

	private UIInteriorModeTextIndicator indicatorLowerLeft;

	private UIInteriorModeTextIndicator indicatorUpperLeft;

	private UIPhysiologyIndicator physiologyIndicator;

	private InteriorMode interiorMode;

	private Communication comm;

	private Mission mission;

	private TimeScaler timeScaler;

	private Settings settings;

	private bool staging;

	public CharacterBody Body { get; private set; }

	public TeamOperator Operator { get; private set; }

	public WeaponPointer WeaponPointer { get; private set; }

	private void Awake()
	{
		Body = GetComponent<CharacterBody>();
		Operator = GetComponent<TeamOperator>();
		comm = GetComponent<Communication>();
		input = Global.Context.InputHandler;
		mission = Global.Context.Mission;
		staging = Version.Instance.Staging;
		timeScaler = mission.TimeScaler;
		interiorMode = mission.InteriorMode;
		interiorCamera = interiorMode.interiorCamera;
		physiologyIndicator = interiorMode.UI.physiologyIndicator;
		fireModeIndicator = interiorMode.UI.fireModeIndicator;
		magazineIndicator = interiorMode.UI.magazineIndicator;
		indicatorLowerLeft = mission.UI.indicatorLowerLeft;
		indicatorUpperLeft = mission.UI.indicatorUpperLeft;
	}

	private void Start()
	{
		settings = Global.Context.Settings;
		settings.Extras.onChanged.AddListener(OnSettingsChanged);
		Body.EventHit.AddListener(OnHit);
		Body.EventRagdoll.AddListener(OnRagdoll);
		Body.EventWeaponActivated.AddListener(OnWeaponActivated);
		Operator.EventSuspectDown.AddListener(OnSuspectDown);
		Operator.EventOperatorDown.AddListener(OnOperatorDown);
		if (Body.ActiveWeapon != null)
		{
			Body.ActiveWeapon.onNewMagazine.AddListener(IndicateMagazineStatus);
		}
		if (Body.InactiveWeapon != null)
		{
			Body.InactiveWeapon.onNewMagazine.AddListener(IndicateMagazineStatus);
		}
		InitializeControl();
	}

	private void InitializeControl()
	{
		WeaponPointer = interiorMode.UI.weaponPointer;
		WeaponPointer.AssignBody(Body);
		WeaponPointer.Show();
		interiorCamera.Subject = Body;
		InitializeWeaponSettings();
		Body.WeaponAimer.useSights = true;
		mission.Operators.PlayerController = this;
		mission.Operators.MainGroup.AssignAsMember(Operator);
	}

	private void InitializeWeaponSettings()
	{
		SerializableGeneralSettings general = Global.Context.Settings.General;
		bool lightOn = general.lightOn;
		bool laserOn = general.laserOn;
		WeaponBase.FireMode pistolFireMode = general.pistolFireMode;
		WeaponBase.FireMode rifleFireMode = general.rifleFireMode;
		WeaponBase activeWeapon = Body.ActiveWeapon;
		if (activeWeapon != null)
		{
			activeWeapon.SetFireMode(general.FireMode(activeWeapon.weaponType), false);
			if (activeWeapon.ComponentFlashlight != null)
			{
				activeWeapon.ComponentFlashlight.SetState(lightOn, false);
			}
			if (activeWeapon.ComponentLaser != null)
			{
				activeWeapon.ComponentLaser.SetState(laserOn, false);
			}
		}
		activeWeapon = Body.InactiveWeapon;
		if (activeWeapon != null)
		{
			activeWeapon.SetFireMode(general.FireMode(activeWeapon.weaponType), false);
			if (Body.InactiveWeapon.ComponentFlashlight != null)
			{
				Body.InactiveWeapon.ComponentFlashlight.SetActive(lightOn);
			}
			if (Body.InactiveWeapon.ComponentLaser != null)
			{
				Body.InactiveWeapon.ComponentLaser.SetActive(laserOn);
			}
		}
	}

	private void RemoveControl()
	{
		WeaponPointer.Hide();
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
		Object.Destroy(this);
	}

	private void Update()
	{
		if (mission.View != Mission.MissionView.EntryTeam || Body.IsRagdoll)
		{
			return;
		}
		Body.IsSprinting = input.Keys.sprint.Pressed;
		float direction = 0f;
		if (input.Keys.left.Pressed)
		{
			direction = -1f;
		}
		else if (input.Keys.right.Pressed)
		{
			direction = 1f;
		}
		Body.Move(direction);
		bool visible = interiorMode.UI.InteractionMenu.Visible;
		if (input.Keys.kneel.Down)
		{
			Body.IsKneeling = true;
		}
		else if (input.Keys.stand.Down)
		{
			Body.IsKneeling = false;
		}
		Body.FreeAimActive = input.Keys.freeAim.Pressed;
		WeaponBase activeWeapon = Body.ActiveWeapon;
		if (!visible && !mission.Paused)
		{
			if (Body.IsHoldingGrenade && input.Keys.fire.DownLong)
			{
				Grenade readyGrenade = Body.ReadyGrenade;
				if (readyGrenade != null)
				{
					readyGrenade.PullPin(Body);
					readyGrenade.ThrowSpoon(Random.onUnitSphere);
					readyGrenade.StartFuse(Body);
				}
			}
			if (Body.IsHoldingGrenade && input.Keys.fire.Up)
			{
				Body.PullGrenadePin();
				input.Keys.fire.DownHandled = true;
				Body.InitiateGrenadeThrow();
			}
			if (activeWeapon != null)
			{
				CyclingBolt cyclingBolt = activeWeapon.cyclingBolt;
				bool pressed = input.Keys.precision.Pressed;
				Body.IsUsingPrecision = pressed;
				if (activeWeapon.ActiveFireMode == WeaponBase.FireMode.Auto && input.Keys.fire.Pressed)
				{
					Body.PullTrigger();
				}
				else if (input.Keys.fire.Down && !Body.IsManipulatingEquipment && activeWeapon.ActiveFireMode != WeaponBase.FireMode.Safe)
				{
					Body.PullTrigger();
				}
				bool reloadHelp = settings.General.reloadHelp;
				bool flag = activeWeapon.Magazine != null && !activeWeapon.Magazine.Empty;
				bool flag2 = ((cyclingBolt.IsOpen || cyclingBolt.Malfunctioned) && (flag || activeWeapon.cyclingBolt.CarryingLiveRound)) || (cyclingBolt.IsClosed && !activeWeapon.HammerReady && flag);
				if (input.Keys.fire.Down)
				{
					if (activeWeapon.ActiveFireMode == WeaponBase.FireMode.Safe)
					{
						indicatorLowerLeft.Indicate(string.Format("Hit <{0}> to release safety", input.Keys.fireMode.KeyName));
					}
					else if (!cyclingBolt.Live && !cyclingBolt.IsMoving)
					{
						if (flag2)
						{
							if (reloadHelp || activeWeapon.manualCycling)
							{
								Body.PendingAction = CharacterBody.CharacterAction.WorkBolt;
							}
							else if (cyclingBolt.Malfunctioned)
							{
								indicatorLowerLeft.Indicate(string.Format("Hit <{0}> to clear malfunction", input.Keys.bolt.KeyName));
							}
							else
							{
								indicatorLowerLeft.Indicate(string.Format("Hit <{0}> to cycle bolt", input.Keys.bolt.KeyName));
							}
						}
						else if (activeWeapon.Magazine == null || activeWeapon.Magazine.Empty)
						{
							if (!Body.Equipment.HasAmmunitionForWeapon(activeWeapon))
							{
								indicatorLowerLeft.Indicate("Out of ammo");
							}
							else if (reloadHelp)
							{
								Body.PendingAction = CharacterBody.CharacterAction.SwitchMagazine;
							}
							else
							{
								indicatorLowerLeft.Indicate(string.Format("Hit <{0}> to {1}", input.Keys.magazine.KeyName, (!activeWeapon.useInternalMagazine) ? "change magazine" : "reload"));
							}
						}
					}
				}
				if (input.Keys.bolt.Down || (activeWeapon.manualCycling && flag2 && reloadHelp))
				{
					Body.PendingAction = CharacterBody.CharacterAction.WorkBolt;
				}
				if (input.Keys.magazine.Down)
				{
					Body.PendingAction = CharacterBody.CharacterAction.SwitchMagazine;
					if (!Body.Equipment.HasAmmunitionForWeapon(activeWeapon))
					{
						indicatorLowerLeft.Indicate("Out of ammo");
					}
				}
				if (input.Keys.fireMode.UpShort)
				{
					activeWeapon.CycleFireModes();
					fireModeIndicator.Indicate(activeWeapon.ActiveFireMode);
					input.Keys.fireMode.UpHandled = true;
				}
				else if (input.Keys.fireMode.DownLong)
				{
					activeWeapon.SetFireMode(WeaponBase.FireMode.Safe);
					fireModeIndicator.Indicate(activeWeapon.ActiveFireMode);
				}
			}
		}
		if (input.Keys.switchWeapons.Down)
		{
			Body.armController.SwitchWeapons();
		}
		else if (input.Keys.dropWeapon.Down && Body.DropWeapon(activeWeapon) != null)
		{
			Body.armController.SwitchWeapons();
		}
		if (input.Keys.doorPush.Down)
		{
			if (input.Keys.doorGradual.Pressed)
			{
				Body.GradualManipulateDoor(input.Keys.doorPush.axisValue, true);
			}
			else
			{
				Body.ManipulateDoor(input.Keys.doorPush.axisValue, true);
			}
			input.Keys.doorPush.DownHandled = true;
		}
		else if (input.Keys.doorPull.Down)
		{
			if (input.Keys.doorGradual.Pressed)
			{
				Body.GradualManipulateDoor(input.Keys.doorPull.axisValue, true);
			}
			else
			{
				Body.ManipulateDoor(input.Keys.doorPull.axisValue, true);
			}
			input.Keys.doorPull.DownHandled = true;
		}
		if (activeWeapon != null)
		{
			if (input.Keys.flashlight.Down)
			{
				activeWeapon.ToggleLight();
			}
			else if (input.Keys.laser.Down)
			{
				activeWeapon.ToggleLaser();
			}
		}
		if (input.Keys.stunGrenade.Down)
		{
			if (Body.IsHoldingGrenade)
			{
				Body.armController.ReturnGrenade();
			}
			else if (Body.Equipment.stunGrenades > 0)
			{
				Body.armController.ReadyGrenade(Grenade.Type.Stun);
			}
			else
			{
				indicatorLowerLeft.Indicate("No stun grenades");
			}
		}
		if (input.Keys.fragGrenade.Down)
		{
			if (Body.IsHoldingGrenade)
			{
				Body.armController.ReturnGrenade();
			}
			else if (Body.Equipment.fragGrenades > 0)
			{
				Body.armController.ReadyGrenade(Grenade.Type.Frag);
			}
			else
			{
				indicatorLowerLeft.Indicate("No frag grenades");
			}
		}
		Vector3 position = Body.RightUpperArm.transform.position;
		HandleChangeDirection(position, visible);
		HandleWeaponAim(activeWeapon, position, visible);
		if (staging && Input.GetKeyDown(KeyCode.K))
		{
			Body.Physiology.SetDead();
		}
	}

	private void HandleChangeDirection(Vector3 shoulderPosition, bool interactionMenuActive)
	{
		if (interactionMenuActive || Body.FreeAimActive)
		{
			return;
		}
		if (WeaponPointer.WorldPoint.x >= shoulderPosition.x)
		{
			if (Body.Direction != 1f)
			{
				Body.Turn(1f);
			}
		}
		else if (Body.Direction != -1f)
		{
			Body.Turn(-1f);
		}
	}

	private void HandleWeaponAim(WeaponBase weapon, Vector3 shoulderPosition, bool interactionMenuActive)
	{
		if (weapon == null || !weapon.IsBlocked)
		{
			Vector3 worldPoint = WeaponPointer.WorldPoint;
			Vector3 vector = worldPoint - shoulderPosition;
			Body.AimPoint = ((!(vector.sqrMagnitude >= 0.122499995f)) ? (worldPoint + vector.normalized * 0.35f) : worldPoint);
		}
	}

	public void OnMissionBegin()
	{
		OnWeaponActivated();
	}

	public void OnHit()
	{
		physiologyIndicator.Flash();
	}

	public void OnRagdoll()
	{
		WeaponPointer.gameObject.SetActive(false);
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
		Object.Destroy(this);
	}

	public void OnWeaponActivated()
	{
		WeaponBase activeWeapon = Body.ActiveWeapon;
		if (!(activeWeapon == null))
		{
			string text = ((!(activeWeapon.ammunition != null)) ? "No ammunition" : ((!string.IsNullOrEmpty(activeWeapon.ammunition.shortName)) ? activeWeapon.ammunition.shortName : activeWeapon.ammunition.type.ToString()));
			bool flag = activeWeapon.ammunition != null && activeWeapon.ammunition.velocity < 343f;
			string statusText = string.Format("{0}, {1}{2}, {3}", activeWeapon.name, text, (!flag) ? string.Empty : " Subsonic", activeWeapon.ActiveFireMode.ToString());
			indicatorLowerLeft.Indicate(statusText, 2f, false);
		}
	}

	public void OnSuspectDown()
	{
		indicatorUpperLeft.Indicate("Suspect down", 1.5f);
	}

	public void OnOperatorDown()
	{
		indicatorUpperLeft.Indicate("Operator down", 1.5f);
	}

	private void OnSettingsChanged(SerializableExtraSettings newSettings)
	{
		Body.Invisible = newSettings.playerInvisible;
		Body.Physiology.invulnerable = newSettings.playerInvulnerable;
		if (newSettings.playerSpectator)
		{
			RemoveControl();
		}
	}

	private void IndicateMagazineStatus()
	{
		WeaponBase activeWeapon = Body.ActiveWeapon;
		if (activeWeapon == null)
		{
			return;
		}
		Magazine magazine = activeWeapon.Magazine;
		if (magazine != null)
		{
			string statusText = "Heavy magazine";
			if (magazine.Ratio < 0.25f)
			{
				statusText = "Light magazine";
			}
			else if (magazine.Ratio < 0.65f)
			{
				statusText = "Medium magazine";
			}
			indicatorLowerLeft.Indicate(statusText);
		}
	}

	public CharacterBody GetVocalizationTarget()
	{
		RaycastHit hitInfo = default(RaycastHit);
		if (Physics.Raycast(interiorCamera.interiorCamera.ScreenPointToRay(Input.mousePosition), out hitInfo, 25f, Masks.Instance.Limbs))
		{
			return hitInfo.collider.GetComponent<Limb>().Body;
		}
		Ray ray = new Ray(Body.Pelvis.transform.position + Vector3.right * Body.Direction * 0.5f, Vector3.right * Body.Direction);
		if (Physics.Raycast(ray, out hitInfo, Body.VisibleRange - 0.5f, Masks.Instance.Limbs))
		{
			return hitInfo.collider.GetComponent<Limb>().Body;
		}
		return null;
	}
}

}

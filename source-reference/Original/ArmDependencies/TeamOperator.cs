using UnityEngine;

public class TeamOperator : MonoBehaviour
{
	public enum TeamActivity
	{
		Follow = 0,
		Hold = 1,
		Lead = 2
	}

	[Header("Vocalizations")]
	public CharacterVocalization vocalAnnounce;

	public CharacterVocalization vocalStop;

	public CharacterVocalization vocalDropWeapon;

	public CharacterVocalization vocalShowHands;

	public CharacterVocalization vocalTurnAround;

	public CharacterVocalization vocalMoveBackwards;

	public CharacterVocalization vocalOnKnees;

	public CharacterVocalization vocalOnGauze;

	[Header("Behavior")]
	public bool restrainOccupants;

	public bool administerFirstAid;

	public bool secureWeapons;

	[Header("Visual equipment")]
	public GameObject plasticuffsVisual;

	public GameObject medicPouchVisual;

	[Header("IK targets")]
	public Transform medicPouchTransform;

	public UnityEventVoid EventSuspectDown = new UnityEventVoid();

	public UnityEventVoid EventOperatorDown = new UnityEventVoid();

	private GameObject heldPlasticuff;

	private GameObject heldGauze;

	public Blackboard Blackboard { get; private set; }

	public CharacterBody Body { get; private set; }

	public Communication Communication { get; private set; }

	public TeamController Team { get; set; }

	public Mission Mission { get; private set; }

	public TeamOperator Leader { get; set; }

	public TeamOperator Follower { get; set; }

	public Room CurrentRoom
	{
		get
		{
			return Body.Room;
		}
	}

	public Room NextRoom { get; set; }

	public TeamActivity TeamTask { get; set; }

	public int TeamPosition { get; set; }

	public bool IsGroupLeader
	{
		get
		{
			return Team.Leader == this;
		}
	}

	public bool IsRearGuard
	{
		get
		{
			return Team.RearGuard == this;
		}
	}

	public DoorController Door { get; set; }

	public bool FacingDoor { get; set; }

	private void Awake()
	{
		TeamTask = TeamActivity.Hold;
		Mission = Global.Context.Mission;
		Body = GetComponent<CharacterBody>();
		Body.EventUnconscious.AddListener(OnUnconscious);
		Body.ActionAttentionPeriod = Random.Range(3f, 8f);
		Communication = GetComponent<Communication>();
		Team = Mission.Operators.MainGroup;
		Team.RegisterOperator(Body);
		Blackboard = Body.Blackboard;
	}

	private void Start()
	{
		plasticuffsVisual.SetActive(Body.Equipment.plasticuffs);
		medicPouchVisual.SetActive(Body.Equipment.gauze > 0);
	}

	private void Update()
	{
		if (Body.IsRagdoll || Body.IsPlayer)
		{
			return;
		}
		UpdateAttention();
		UpdateFlashlight();
		if (Body.HasActiveTarget())
		{
			SetEngagePosition();
			return;
		}
		TeamActivity teamTask = TeamTask;
		if (teamTask == TeamActivity.Lead)
		{
			SetLeadPosition();
		}
	}

	private void SetEngagePosition()
	{
		Target target = Blackboard.Target;
		float num = ((!(target.Body != null)) ? target.transform.position.x : target.Body.Position);
		float b = num - 2.5f * Body.Direction;
		if (Body.Direction == 1f)
		{
			float a = ((!(Leader != null)) ? float.PositiveInfinity : (Leader.Body.Position - FollowDistance(Leader) * Leader.Body.Direction));
			Blackboard.MovePosition = Mathf.Min(a, b);
		}
		else
		{
			float a2 = ((!(Leader != null)) ? float.NegativeInfinity : (Leader.Body.Position - FollowDistance(Leader) * Leader.Body.Direction));
			Blackboard.MovePosition = Mathf.Max(a2, b);
		}
	}

	private void SetFollowPosition()
	{
		if (Leader == null)
		{
			return;
		}
		float movePosition = Leader.Body.Position - FollowDistance(Leader) * Leader.Body.Direction;
		if (Leader.FacingDoor)
		{
			DoorController door = Team.Leader.Door;
			float num = Mathf.Sign(door.transform.position.x - Body.Position);
			if (num == door.OpeningDirection)
			{
				movePosition = Leader.Body.Position - FollowDistance(Leader) * Leader.Body.Direction;
			}
			else
			{
				float num2 = Mathf.Max(0.5f, (num != 1f) ? door.OpenClearanceRight : door.OpenClearanceLeft);
				float num3 = 0.25f + ((!(Body.ActiveWeapon != null)) ? 0f : Body.ActiveWeapon.Length());
				movePosition = door.transform.position.x - (num2 + num3) * num;
			}
		}
		Blackboard.MovePosition = movePosition;
	}

	private void SetLeadPosition()
	{
		if (Team.holdPosition)
		{
			Blackboard.MovePosition = Body.Position;
			return;
		}
		Blackboard.MovePosition = Team.targetPosition;
		float num = Time.time - Blackboard.ThrowStunTimestamp;
		bool flag = num > 0f && num < 2.5f;
		float num2 = Mathf.Abs(Team.targetPosition - Body.Position);
		Body.IsSprinting = flag || (Team.tempo == CharacterBody.Tempo.Quick && num2 > 2f);
	}

	public float FollowDistance(TeamOperator leader)
	{
		float result = 1f;
		if (Body.HasActiveTarget())
		{
			result = 0.55f;
		}
		else if (leader != null && leader.FacingDoor && !IsRearGuard)
		{
			result = 0.55f;
		}
		else if (!leader.Body.WeaponIsHeldHigh())
		{
			result = 0.5f;
		}
		else if (leader.Body.IsKneeling)
		{
			result = 0.6f;
		}
		return result;
	}

	public bool CoveringLeader()
	{
		if (Leader == null || Body.IsMoving)
		{
			return false;
		}
		WeaponBase activeWeapon = Body.ActiveWeapon;
		if (activeWeapon != null && Mathf.Abs(activeWeapon.Angle) > 20f)
		{
			return false;
		}
		return true;
	}

	public void ReportSuspectDown()
	{
		EventSuspectDown.Invoke();
	}

	public void ReportOperatorDown()
	{
		EventOperatorDown.Invoke();
	}

	public void DropCellPhone()
	{
		Body.Mission.SpawnCellPhone(new Vector3(Body.RightHand.transform.position.x, 0.75f, Body.transform.position.z));
	}

	private void UpdateAttention()
	{
		bool flag = Leader != null && Time.time - Leader.Body.LastFiredTimestamp < 1f;
		bool flag2 = Leader != null && Time.time - Leader.Body.Blackboard.ThrowStunTimestamp < 1f;
		bool flag3 = CoveringLeader();
		bool flag4 = Time.time - Body.LastActionTimestamp > Body.ActionAttentionPeriod;
		Body.AtAttention = IsGroupLeader || flag3 || flag4 || flag || flag2;
	}

	private void UpdateFlashlight()
	{
		if (!(Body.ActiveWeapon == null) && !(Body.ActiveWeapon.ComponentFlashlight == null))
		{
			bool flag = false;
			float num = Mathf.Max(Body.VisibleRange - 0.1f, Body.VisionRange * 0.8f);
			Vector3 origin = Body.Head.transform.position + Vector3.right * num * Body.Direction;
			origin.y = 5f;
			RaycastHit hitInfo;
			if (Physics.Raycast(origin, Vector3.down, out hitInfo, 10f, Masks.Instance.Rooms))
			{
				Room component = hitInfo.collider.GetComponent<Room>();
				flag |= component != null && component.useFlashlight;
			}
			Body.ActiveWeapon.ComponentFlashlight.SetState(flag);
		}
	}

	private void OnUnconscious()
	{
		AssignNewInteriorCameraSubject();
	}

	private void AssignNewInteriorCameraSubject()
	{
		InteriorCamera interiorCamera = Mission.InteriorMode.interiorCamera;
		if (!(Body != interiorCamera.Subject) && !(Follower == null))
		{
			interiorCamera.Subject = Follower.Body;
		}
	}

	public void Vocalize(CharacterBody target)
	{
		if (target == null || !target.Conscious)
		{
			Body.Vocalize(vocalAnnounce.Any());
		}
		else if (Mathf.Abs(target.Velocity) > 0.1f)
		{
			Body.Vocalize(vocalStop.Any());
			target.Blackboard.MovePosition = target.Position;
			target.IsCompliant = true;
		}
		else if (target.ActiveWeapon != null)
		{
			Body.Vocalize(vocalDropWeapon.Any());
			target.DropWeapons(false);
			target.IsCompliant = true;
		}
		else if (!target.IsRestrained && Mathf.Min(target.LeftHand.transform.position.y, target.RightHand.transform.position.y) < target.Head.transform.position.y)
		{
			Body.Vocalize(vocalShowHands.Any());
			target.armController.Surrender();
			target.IsCompliant = true;
		}
		else if (target.Facing(Body))
		{
			Body.Vocalize(vocalTurnAround.Any());
			target.IsCompliant = true;
			target.Turn();
		}
		else if (Mathf.Abs(target.Position - Body.Position) > 2f)
		{
			Body.Vocalize(vocalMoveBackwards.Any());
			target.Blackboard.MovePosition = target.Position - target.Direction;
			target.IsCompliant = true;
		}
		else if (!target.IsKneeling)
		{
			Body.Vocalize(vocalOnKnees.Any());
			target.IsKneeling = true;
			target.IsCompliant = true;
		}
	}

	public bool CommandClearNextRoomAvailable()
	{
		Room room = Body.Room;
		if (room == null)
		{
			return false;
		}
		Room room2 = ((!Body.FacingRight) ? room.leftRoom : room.rightRoom);
		return room2 != null;
	}

	public void InstantiatePlasticuffInHand()
	{
		heldPlasticuff = Object.Instantiate(Global.Context.EquipmentList.plasticuffs);
		heldPlasticuff.transform.SetParent(Body.leftArm.hand.grabPoint);
		heldPlasticuff.transform.localPosition = Vector3.zero;
		heldPlasticuff.transform.localScale = new Vector3(1f, 1f, 0.2f);
	}

	public void DestroyPlasticuffInHand()
	{
		if (!(heldPlasticuff == null))
		{
			Object.Destroy(heldPlasticuff);
		}
	}

	public void InstantiateGauzeInHand()
	{
		heldGauze = Object.Instantiate(Global.Context.EquipmentList.gauze);
		heldGauze.transform.SetParent(Body.rightArm.hand.grabPoint);
		heldGauze.transform.localPosition = Vector3.zero;
		heldGauze.transform.localScale = new Vector3(1f, 1f, 0.2f);
	}

	public void RemoveGauzeFromHand()
	{
		if (!(heldGauze == null))
		{
			heldGauze.transform.SetParent(null);
			heldGauze.transform.localScale = Vector3.one;
			Rigidbody rigidbody = heldGauze.AddComponent<Rigidbody>();
			rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
			heldGauze = null;
		}
	}
}

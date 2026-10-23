using BehaviorDesigner.Runtime.Tasks;
using Crisis.Runtime.FullSource;
using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;
using UnityEngine;

namespace Crisis.Runtime.FullSource
{
public static class StaticBehavior
{
	public static TaskStatus TurnAround(CharacterBody body)
	{
		WeaponBase activeWeapon = body.ActiveWeapon;
		if (activeWeapon != null && !activeWeapon.IsBlocked)
		{
			body.AimPoint = new Vector3(body.Position + activeWeapon.armConfiguration.retractDistance * body.Direction, 0f, body.transform.position.z);
			Vector3 position = activeWeapon.Muzzle.position;
			Vector3 right = activeWeapon.transform.right;
			Vector3 vector = body.AimPoint - position;
			float num = Vector3.Angle(vector, right);
			if (num > 10f)
			{
				return TaskStatus.Running;
			}
		}
		body.Turn();
		body.IsLoweringWeapon = false;
		return TaskStatus.Success;
	}

	public static TaskStatus OperatorHandleDoor(TeamOperator op, DoorController door, float targetPosition, bool openUncleared)
	{
		if (door.Angle >= 90f)
		{
			return TaskStatus.Success;
		}
		if (door.interactor != null && door.interactor != op.Body)
		{
			return TaskStatus.Running;
		}
		if (!openUncleared)
		{
			Room room = door.RoomOppositeFrom(op.Body.Position);
			if (room != null && !room.cleared)
			{
				return TaskStatus.Success;
			}
		}
		CharacterBody body = op.Body;
		if ((body.Direction == 1f && targetPosition < door.Position) || (body.Direction == -1f && targetPosition > door.Position))
		{
			return TaskStatus.Success;
		}
		float f = door.Position - body.Position;
		float direction = Mathf.Sign(f);
		float num = Mathf.Abs(f);
		if (num > ((!body.IsSprinting) ? 1.5f : 3f))
		{
			return TaskStatus.Success;
		}
		if (body.Blackboard.LastCollider != door.physicalDoor)
		{
			body.IsSprinting = false;
			body.Move(direction);
		}
		else
		{
			if (door.lockState == DoorLockState.Locked || door.lockState == DoorLockState.Unlocking)
			{
				if (door.lockState == DoorLockState.Locked)
				{
					door.Unlock(body);
				}
				return TaskStatus.Running;
			}
			if (door.openState != DoorOpenState.Opening)
			{
				door.Open(body);
			}
		}
		return TaskStatus.Running;
	}

	public static TaskStatus OccupantHandleDoorWhileMoving(CharacterBody body, float destinationPosition, bool sprint, float proximity)
	{
		float f = destinationPosition - body.Position;
		float num = Mathf.Abs(f);
		float direction = Mathf.Sign(f);
		DoorController doorController = ((!(body.Room != null)) ? null : body.Room.NextDoor(direction));
		if (doorController == null)
		{
			return TaskStatus.Success;
		}
		if (doorController.interactor != null && doorController.interactor != body)
		{
			return TaskStatus.Running;
		}
		if (Mathf.Abs(doorController.Position - body.Position) < proximity)
		{
			return OccupantOpenDoor(body, doorController, destinationPosition, sprint);
		}
		return TaskStatus.Success;
	}

	public static TaskStatus OccupantOpenDoor(CharacterBody body, DoorController door, float targetPosition, bool sprint)
	{
		if (door.Angle >= 90f)
		{
			return TaskStatus.Success;
		}
		float f = door.Position - body.Position;
		float direction = Mathf.Sign(f);
		float num = Mathf.Abs(f);
		if (num > ((!body.IsSprinting) ? 1.5f : 3f))
		{
			return TaskStatus.Success;
		}
		if (!body.Facing(door.Position))
		{
			TurnAround(body);
			return TaskStatus.Running;
		}
		float num2 = Mathf.Abs(door.Knob(body.Position).position.x - body.Position);
		if (body.Blackboard.LastCollider != door.physicalDoor || num2 > 1f)
		{
			body.IsSprinting = sprint;
			body.Move(direction);
		}
		else
		{
			if (door.lockState == DoorLockState.Unlocking)
			{
				return TaskStatus.Running;
			}
			if (door.lockState == DoorLockState.Locked)
			{
				if (door.CanOperateLock(body))
				{
					door.Unlock(body);
					return TaskStatus.Running;
				}
				return TaskStatus.Failure;
			}
			if (door.Trap != null && door.Trap.Armed)
			{
				door.Trap.Disarm();
			}
			if (door.openState != DoorOpenState.Opening)
			{
				door.Open(body);
			}
		}
		return TaskStatus.Running;
	}

	public static TaskStatus OccupantCloseDoorWhileMoving(CharacterBody body, float destinationPosition, bool sprint, bool lockAfter)
	{
		float f = destinationPosition - body.Position;
		float num = Mathf.Sign(f);
		DoorController doorController = ((!(body.Room != null)) ? null : body.Room.NextDoor(num * -1f));
		if (doorController == null || !doorController.InteractorInRange(body))
		{
			return TaskStatus.Success;
		}
		if (doorController.interactor != null && doorController.interactor != body)
		{
			return TaskStatus.Running;
		}
		lockAfter = lockAfter && doorController.CanOperateLock(body);
		return OccupantCloseDoor(body, doorController, destinationPosition, sprint, lockAfter);
	}

	public static TaskStatus OccupantCloseDoor(CharacterBody body, DoorController door, float targetPosition, bool sprint, bool lockAfter)
	{
		if (door.openState == DoorOpenState.Closed && (!lockAfter || (lockAfter && (door.lockState == DoorLockState.Locked || door.lockState == DoorLockState.Locking))))
		{
			return TaskStatus.Success;
		}
		float num = Mathf.Sign(targetPosition - body.Position);
		float num2 = ((num != 1f) ? (door.Position - door.OpenClearanceLeft - 0.5f) : (door.Position + door.OpenClearanceRight + 0.5f));
		Debug.DrawLine(new Vector3(num2, 0f, -1.5f), new Vector3(num2, 2f, -1.5f), Color.yellow, 1f);
		float f = num2 - body.Position;
		float direction = Mathf.Sign(f);
		float num3 = Mathf.Abs(f);
		if (num3 > 0.25f)
		{
			body.IsSprinting = sprint;
			body.Move(direction);
			return TaskStatus.Running;
		}
		if (door.openState != DoorOpenState.Closed && door.openState != DoorOpenState.Closing && !body.Facing(door.Position))
		{
			TurnAround(body);
			return TaskStatus.Running;
		}
		if (door.openState == DoorOpenState.Open || door.openState == DoorOpenState.Opening || door.openState == DoorOpenState.Partial || door.openState == DoorOpenState.PartialMoving)
		{
			door.Close(body);
			return TaskStatus.Running;
		}
		if (door.openState == DoorOpenState.Closing)
		{
			return TaskStatus.Running;
		}
		if (door.Trap != null && !door.Trap.Detonated && !door.Trap.Armed)
		{
			door.Trap.Arm();
		}
		if (lockAfter)
		{
			door.Lock(body);
		}
		return TaskStatus.Success;
	}
}
}

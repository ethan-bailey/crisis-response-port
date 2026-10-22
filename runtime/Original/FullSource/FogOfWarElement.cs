namespace Crisis.Runtime.FullSource
{
using System;
using UnityEngine;

public class FogOfWarElement : MonoBehaviour
{
	private Room room;

	private Renderer quadRenderer;

	private DoorController leftDoor;

	private DoorController rightDoor;

	private InteriorCamera interiorCamera;

	private float homePosition;

	private float centerPosition;

	public bool Active
	{
		get
		{
			return quadRenderer.enabled;
		}
	}

	private void Awake()
	{
		quadRenderer = GetComponent<Renderer>();
	}

	private void Start()
	{
		interiorCamera = Global.Context.Mission.InteriorMode.interiorCamera;
		homePosition = base.transform.position.x;
		room = Global.Context.Mission.RoomAtPosition(base.transform.position.x);
		centerPosition = room.transform.position.x + room.Width * 0.5f;
		leftDoor = room.LeftDoor;
		rightDoor = room.RightDoor;
		UpdateState(Global.Context.Mission.Operators.First);
	}

	private void Update()
	{
		UpdateState(interiorCamera.Subject);
	}

	private void UpdateState(CharacterBody subject)
	{
		if (subject == null)
		{
			return;
		}
		bool flag = subject.Room == room;
		quadRenderer.enabled = !flag;
		if (flag)
		{
			return;
		}
		float num = Mathf.Sign(homePosition - subject.transform.position.x);
		Ray ray = new Ray(subject.Eye.transform.position - Vector3.right * subject.Direction * 0.5f, Vector3.right * num);
		bool flag2 = Physics.Raycast(ray, Mathf.Abs(homePosition - subject.transform.position.x) + 0.5f, Masks.Instance.Doors);
		quadRenderer.enabled = flag2;
		if (!flag2)
		{
			return;
		}
		bool flag3 = leftDoor != null && subject.transform.position.x < leftDoor.transform.position.x;
		bool flag4 = rightDoor != null && subject.transform.position.x > rightDoor.transform.position.x;
		if ((flag3 && room.leftRoom != null && room.leftRoom.fogOfWar.Active) || (flag4 && room.rightRoom != null && room.rightRoom.fogOfWar.Active))
		{
			quadRenderer.enabled = true;
			base.transform.position = new Vector3(homePosition, base.transform.position.y, base.transform.position.z);
			return;
		}
		if (flag3)
		{
			if (leftDoor.Angle > 0f)
			{
				float num2 = leftDoor.HingePosition.z + 3f;
				float num3 = Mathf.Min(Mathf.Tan(leftDoor.Angle * ((float)Math.PI / 180f)) * num2, 100f);
				base.transform.position = new Vector3(centerPosition + num3, base.transform.position.y, base.transform.position.z);
			}
			else
			{
				base.transform.position = new Vector3(centerPosition, base.transform.position.y, base.transform.position.z);
			}
		}
		if (flag4)
		{
			if (rightDoor.Angle > 0f)
			{
				float z = rightDoor.HingePosition.z;
				float num4 = Mathf.Min(Mathf.Tan(rightDoor.Angle * ((float)Math.PI / 180f)) * z, 100f);
				base.transform.position = new Vector3(centerPosition + num4, base.transform.position.y, base.transform.position.z);
			}
			else
			{
				base.transform.position = new Vector3(centerPosition, base.transform.position.y, base.transform.position.z);
			}
		}
	}
}

}

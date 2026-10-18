namespace Crisis.Runtime.FullSource
{
using System.Collections.Generic;
using UnityEngine;

public class Doors : MonoBehaviour
{
	private List<DoorController> doors = new List<DoorController>();

	private void Awake()
	{
		Global.Context.Settings.Extras.onChanged.AddListener(OnExtrasChanged);
	}

	public void Register(DoorController door)
	{
		doors.Add(door);
	}

	private void OnExtrasChanged(SerializableExtraSettings newSettings)
	{
		foreach (DoorController door in doors)
		{
			door.SetLockDisabledState(newSettings.disableLocks);
		}
	}
}

}

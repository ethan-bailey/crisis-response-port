namespace Crisis.Runtime.FullSource
{
using System;
using UnityEngine;

public class WeaponComponentSlot : MonoBehaviour
{
	private static string ComponentsPath = "Weapons/Components/";

	public static char[] SplitDelimiters = new char[3] { ',', ';', ' ' };

	[Header("Compatibility")]
	public WeaponComponentSlotType type;

	public WeaponComponentSlotOrientation orientation;

	[Tooltip("Qualifiers to satisfy compatibility conditions stated by components. This only restricts components that have qualifiers specified; in that case at least one must be in this list for it to match.")]
	public string qualifiers;

	[Tooltip("To make a component exclusive for this slot, enter a qualifier name:\n 1. In this field, and\n 2. In that component's qualifiers")]
	public string componentQualifiers;

	public WeaponComponent defaultComponent;

	public WeaponComponent component;

	[Tooltip("If set, the component will become a child of this transform when gameplay starts.")]
	public Transform runtimeParent;

	public bool selectable = true;

	public bool required;

	public SerializableWeaponComponentSlot Serialize()
	{
		SerializableWeaponComponentSlot serializableWeaponComponentSlot = new SerializableWeaponComponentSlot();
		serializableWeaponComponentSlot.name = base.name;
		serializableWeaponComponentSlot.type = type;
		serializableWeaponComponentSlot.orientation = orientation;
		serializableWeaponComponentSlot.defaultComponent = ((!(defaultComponent != null)) ? string.Empty : defaultComponent.name);
		serializableWeaponComponentSlot.component = ((!(component != null)) ? string.Empty : component.name);
		serializableWeaponComponentSlot.required = required;
		return serializableWeaponComponentSlot;
	}

	public void Deserialize(SerializableWeaponComponentSlot data)
	{
		base.name = data.name;
		type = data.type;
		orientation = data.orientation;
		defaultComponent = LoadComponentResource(data.defaultComponent);
		component = LoadComponentResource(data.component);
		required = data.required;
	}

	public bool QualifierMatch(string qualifiers)
	{
		if (string.IsNullOrEmpty(componentQualifiers))
		{
			return string.IsNullOrEmpty(qualifiers);
		}
		string[] array = qualifiers.Split(SplitDelimiters, StringSplitOptions.RemoveEmptyEntries);
		string[] array2 = array;
		foreach (string value in array2)
		{
			if (componentQualifiers.Contains(value))
			{
				return true;
			}
		}
		return string.IsNullOrEmpty(qualifiers);
	}

	private WeaponComponent LoadComponentResource(string name)
	{
		GameObject gameObject = Resources.Load(ComponentsPath + name) as GameObject;
		if (gameObject == null)
		{
			return null;
		}
		return gameObject.GetComponent<WeaponComponent>();
	}

	public void StartRuntime()
	{
		if (!(runtimeParent == null))
		{
			base.transform.SetParent(runtimeParent, true);
		}
	}
}

}

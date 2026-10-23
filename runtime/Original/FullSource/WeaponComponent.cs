namespace Crisis.Runtime.FullSource
{
using System;
using System.Collections.Generic;
using UnityEngine;

public class WeaponComponent : MonoBehaviour
{
	public enum Function
	{
		None = 0,
		Magazine = 1,
		Optic = 2,
		Suppressor = 3,
		Light = 4,
		Laser = 5
	}

	public string componentName = "Component";

	[Tooltip("The weight of the component in kilograms.")]
	public float weight;

	[Tooltip("Which functions does this component have?")]
	public List<Function> functions = new List<Function>();

	[Tooltip("Enable this if the component's default orientation is Bottom.")]
	public bool flipVertical;

	public Transform colliderTransform;

	[Tooltip("This transform is used for arms to grab at.")]
	public Transform ikTarget;

	[Header("Compatibility")]
	[Tooltip("Which types of slot does this component fit on?")]
	public List<WeaponComponentSlotType> slotTypes = new List<WeaponComponentSlotType>();

	[Tooltip("In which orientation can this component be attached?")]
	public List<WeaponComponentSlotOrientation> orientations = new List<WeaponComponentSlotOrientation>();

	[Tooltip("Qualifiers to satisfy compatibility conditions stated by components. This only restricts slots that have qualifiers specified; in that case at least one must be in this list for it to match.")]
	public string qualifiers;

	[Tooltip("To make this component work on only selected slots, enter a qualifier name here and in the qualifiers field of the slots it should attach to.")]
	public string slotQualifiers;

	public WeaponBase Weapon { get; set; }

	public bool Compatible(WeaponComponentSlot slot)
	{
		bool flag = false;
		foreach (WeaponComponentSlotType slotType in slotTypes)
		{
			flag = flag || slotType == slot.type;
		}
		if (!flag)
		{
			return false;
		}
		bool flag2 = orientations.Count > 0 && orientations[0] == WeaponComponentSlotOrientation.Any;
		if (!flag2)
		{
			foreach (WeaponComponentSlotOrientation orientation in orientations)
			{
				flag2 = flag2 || orientation == slot.orientation;
			}
		}
		if (!flag2)
		{
			return false;
		}
		bool flag3 = slot.QualifierMatch(qualifiers);
		bool flag4 = QualifierMatch(slot.qualifiers);
		return flag3 && flag4;
	}

	public bool QualifierMatch(string qualifiers)
	{
		if (string.IsNullOrEmpty(slotQualifiers))
		{
			return string.IsNullOrEmpty(qualifiers);
		}
		if (string.IsNullOrEmpty(qualifiers))
		{
			return false;
		}
		string[] array = qualifiers.Split(WeaponComponentSlot.SplitDelimiters, StringSplitOptions.RemoveEmptyEntries);
		string[] array2 = array;
		foreach (string value in array2)
		{
			if (slotQualifiers.Contains(value))
			{
				return true;
			}
		}
		return false;
	}

	public void SetOrientation(WeaponComponentSlotOrientation slotOrientation)
	{
		float y = ((slotOrientation != WeaponComponentSlotOrientation.Bottom) ? 1f : (-1f)) * ((!flipVertical) ? 1f : (-1f));
		base.transform.localScale = new Vector3(1f, y, 1f);
	}

	public Rect RenderBounds()
	{
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 0f;
		bool flag = true;
		if (colliderTransform == null)
		{
			return default(Rect);
		}
		Collider component = colliderTransform.GetComponent<Collider>();
		if (component != null)
		{
			Bounds bounds = component.bounds;
			num = bounds.min.x;
			num3 = bounds.min.y;
			num2 = bounds.max.x;
			num4 = bounds.max.y;
			flag = false;
		}
		foreach (Transform item in colliderTransform)
		{
			Collider component2 = item.GetComponent<Collider>();
			if (!(component2 == null))
			{
				Bounds bounds2 = component2.bounds;
				if (flag)
				{
					num = bounds2.min.x;
					num3 = bounds2.min.y;
					num2 = bounds2.max.x;
					num4 = bounds2.max.y;
					flag = false;
				}
				else
				{
					num = Mathf.Min(num, bounds2.min.x);
					num3 = Mathf.Min(num3, bounds2.min.y);
					num2 = Mathf.Max(num2, bounds2.max.x);
					num4 = Mathf.Max(num4, bounds2.max.y);
				}
			}
		}
		Vector2 vector = new Vector2(num2 - num, num4 - num3);
		return new Rect(num, num3, vector.x, vector.y);
	}

	public virtual void ClearWeaponReferences()
	{
	}

	public SerializableWeaponComponent Serialize()
	{
		SerializableWeaponComponent serializableWeaponComponent = new SerializableWeaponComponent();
		serializableWeaponComponent.name = componentName;
		serializableWeaponComponent.slotTypes = slotTypes;
		serializableWeaponComponent.orientations = orientations;
		serializableWeaponComponent.functions = functions;
		return serializableWeaponComponent;
	}

	public void Deserialize(SerializableWeaponComponent data)
	{
		componentName = data.name;
		slotTypes = data.slotTypes;
		orientations = data.orientations;
		functions = data.functions;
	}
}

}

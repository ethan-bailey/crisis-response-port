namespace Crisis.Runtime.FullSource
{
using UnityEngine;

public class MarksmanRifle : MonoBehaviour
{
	public Calibre caliber;

	public AmmunitionBase defaultAmmunition;

	public AmmunitionBase Ammunition { get; set; }

	public void Deserialize(SerializableMarksmanRifle data)
	{
		Ammunition = AmmunitionBase.GetAmmunition(data.ammunitionPrefab, defaultAmmunition);
	}

	public SerializableMarksmanRifle Serialize()
	{
		SerializableMarksmanRifle serializableMarksmanRifle = new SerializableMarksmanRifle();
		serializableMarksmanRifle.prefabName = base.name;
		serializableMarksmanRifle.ammunitionPrefab = ((!(Ammunition != null)) ? string.Empty : Ammunition.name);
		return serializableMarksmanRifle;
	}
}

}

using System;

[Serializable]
public class TutorialEquipment
{
	public bool enabled = true;

	public SerializableWeapon primary = new SerializableWeapon();

	public SerializableWeapon secondary = new SerializableWeapon();

	public SerializableItems items = new SerializableItems();

	public ArmorClass.Type armor = ArmorClass.Type.TypeIV;
}

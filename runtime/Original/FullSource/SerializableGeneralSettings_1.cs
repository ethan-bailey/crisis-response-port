namespace Crisis.Runtime.FullSource
{
using Crisis.Runtime.Weapons;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;
using System;
using FullSerializer;

[fsObject("1", new Type[] { })]
public class SerializableGeneralSettings_1
{
	public enum OnOffEnum
	{
		Off = 0,
		On = 1
	}

	public WeaponBase.FireMode pistolFireMode = WeaponBase.FireMode.Semi;

	public WeaponBase.FireMode smgFireMode = WeaponBase.FireMode.Auto;

	public WeaponBase.FireMode shotgunFireMode = WeaponBase.FireMode.Semi;

	public WeaponBase.FireMode rifleFireMode = WeaponBase.FireMode.Auto;

	public bool lightOn;

	public bool laserOn;

	public bool showInteractionMarkers = true;

	public bool reloadHelp = true;

	public bool showMarksmanTargetIndication = true;

	public bool disableMarksmanBobbing;

	public float marksmanMouseSensitivity = 1f;
}

}

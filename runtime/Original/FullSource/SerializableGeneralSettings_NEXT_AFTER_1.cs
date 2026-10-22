namespace Crisis.Runtime.FullSource
{
using Crisis.Runtime.Weapons;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;
using System;
using FullSerializer;

[fsObject("NEXT-AFTER-1", new Type[] { typeof(SerializableGeneralSettings_1) })]
public class SerializableGeneralSettings_NEXT_AFTER_1
{
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

	public int grenadeTimerMode;

	public SerializableGeneralSettings_NEXT_AFTER_1()
	{
	}

	public SerializableGeneralSettings_NEXT_AFTER_1(SerializableGeneralSettings_1 other)
	{
		pistolFireMode = other.pistolFireMode;
		smgFireMode = other.smgFireMode;
		shotgunFireMode = other.shotgunFireMode;
		rifleFireMode = other.rifleFireMode;
		lightOn = other.lightOn;
		laserOn = other.laserOn;
		showInteractionMarkers = other.showInteractionMarkers;
		reloadHelp = other.reloadHelp;
		showMarksmanTargetIndication = other.showMarksmanTargetIndication;
		disableMarksmanBobbing = other.disableMarksmanBobbing;
		marksmanMouseSensitivity = other.marksmanMouseSensitivity;
		grenadeTimerMode = 2;
	}
}

}

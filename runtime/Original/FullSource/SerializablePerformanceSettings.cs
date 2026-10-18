namespace Crisis.Runtime.FullSource
{
using Crisis.Runtime.Weapons;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;
using System;
using System.IO;
using FullSerializer;
using UnityEngine;
using UnityEngine.Events;

[fsObject("0_0_9_4792", new Type[] { }, MemberSerialization = fsMemberSerialization.OptIn)]
public class SerializablePerformanceSettings
{
	public enum DynamicLimbDecalsType
	{
		All = 0,
		OnlyHead = 1,
		Off = 2
	}

	public enum ImpactEmissionsType
	{
		All = 0,
		OnlyHead = 1,
		AllExceptHead = 2,
		Disabled = 3
	}

	public enum PaintTexturePixelsPerMeter
	{
		Off = 0,
		VeryLow = 16,
		Low = 32,
		Medium = 48,
		High = 64,
		VeryHigh = 96
	}

	public enum ParticlesLifetime
	{
		VeryShort = 1,
		Short = 5,
		Normal = 20,
		Long = 60,
		VeryLong = 180,
		SuperLong = 3600
	}

	public enum WoundStreakLifetime
	{
		VeryShort = 1,
		Short = 2,
		Normal = 3,
		Long = 5,
		VeryLong = 7,
		SuperLong = 10
	}

	public class PerformanceSettingsChangedEvent : UnityEvent<SerializablePerformanceSettings>
	{
	}

	[fsProperty]
	public int preset = 3;

	[fsProperty]
	public int qualityLevel = 3;

	[fsProperty]
	public bool cameraGrain = true;

	[fsProperty]
	public bool cameraBloom = true;

	[fsProperty]
	public bool cameraBlur;

	[fsProperty]
	public bool woundDecals = true;

	[fsProperty]
	public int shadows = 2;

	[fsProperty]
	public int paintTexturePixelsPerMeter = 64;

	[fsProperty]
	public int dynamicLimbDecalMode;

	[fsProperty]
	public int particlesLifetime = 20;

	[fsProperty]
	public int woundStreakLifetime = 3;

	[fsProperty]
	public int impactEmissionsMode;

	[fsProperty]
	public int maxEnvironmentHitEffects = 20;

	[fsProperty]
	public int maxEnvironmentHitMeshes = 40;

	[fsProperty]
	public int maxGoreHitEffects = 20;

	[fsProperty]
	public int maxSoakSpots = 10;

	[fsProperty]
	public int maxWoundStreaks = 20;

	[fsProperty]
	public int maxBloodStreams = 10;

	[fsProperty]
	public int maxWallStreaks = 20;

	[fsProperty]
	public int maxFloorStreaks = 10;

	[fsProperty]
	public int maxFloorPools = 10;

	[fsProperty]
	public int maxBloodParticles = 800;

	[fsProperty]
	public int maxGibs = 100;

	public PerformanceSettingsChangedEvent onChanged = new PerformanceSettingsChangedEvent();

	public UnityEventInt onShadowsChanged = new UnityEventInt();

	public UnityEventBool onCameraGrainChanged = new UnityEventBool();

	public UnityEventBool onCameraBloomChanged = new UnityEventBool();

	public UnityEventBool onCameraBlurChanged = new UnityEventBool();

	public UnityEventInt onPaintTextureResolutionChanged = new UnityEventInt();

	public const string Filename = "performance.dat";

	public bool HeadEmissionsEnabled
	{
		get
		{
			return impactEmissionsMode <= 1;
		}
	}

	public bool BodyEmissionsEnabled
	{
		get
		{
			return impactEmissionsMode <= 2;
		}
	}

	public bool SurfaceDecalsEnabled
	{
		get
		{
			return paintTexturePixelsPerMeter > 0;
		}
	}

	public DynamicLimbDecalsType DynamicLimbDecalMode
	{
		get
		{
			return (DynamicLimbDecalsType)dynamicLimbDecalMode;
		}
	}

	public bool Different(SerializablePerformanceSettings other)
	{
		bool flag = false;
		flag |= preset != other.preset;
		flag |= qualityLevel != other.qualityLevel;
		flag |= cameraGrain != other.cameraGrain;
		flag |= cameraBloom != other.cameraBloom;
		flag |= cameraBlur != other.cameraBlur;
		flag |= woundDecals != other.woundDecals;
		flag |= shadows != other.shadows;
		flag |= paintTexturePixelsPerMeter != other.paintTexturePixelsPerMeter;
		flag |= impactEmissionsMode != other.impactEmissionsMode;
		flag |= particlesLifetime != other.particlesLifetime;
		flag |= woundStreakLifetime != other.woundStreakLifetime;
		flag |= dynamicLimbDecalMode != other.dynamicLimbDecalMode;
		flag |= maxEnvironmentHitEffects != other.maxEnvironmentHitEffects;
		flag |= maxEnvironmentHitMeshes != other.maxEnvironmentHitMeshes;
		flag |= maxGoreHitEffects != other.maxGoreHitEffects;
		flag |= maxSoakSpots != other.maxSoakSpots;
		flag |= maxWoundStreaks != other.maxWoundStreaks;
		flag |= maxBloodStreams != other.maxBloodStreams;
		flag |= maxWallStreaks != other.maxWallStreaks;
		flag |= maxFloorStreaks != other.maxFloorStreaks;
		flag |= maxFloorPools != other.maxFloorPools;
		flag |= maxBloodParticles != other.maxBloodParticles;
		return flag | (maxGibs != other.maxGibs);
	}

	public void Overwrite(SerializablePerformanceSettings other)
	{
		preset = other.preset;
		qualityLevel = other.qualityLevel;
		cameraGrain = other.cameraGrain;
		cameraBloom = other.cameraBloom;
		cameraBlur = other.cameraBlur;
		woundDecals = other.woundDecals;
		shadows = other.shadows;
		paintTexturePixelsPerMeter = other.paintTexturePixelsPerMeter;
		impactEmissionsMode = other.impactEmissionsMode;
		particlesLifetime = other.particlesLifetime;
		woundStreakLifetime = other.woundStreakLifetime;
		dynamicLimbDecalMode = other.dynamicLimbDecalMode;
		maxEnvironmentHitEffects = other.maxEnvironmentHitEffects;
		maxEnvironmentHitMeshes = other.maxEnvironmentHitMeshes;
		maxGoreHitEffects = other.maxGoreHitEffects;
		maxSoakSpots = other.maxSoakSpots;
		maxWoundStreaks = other.maxWoundStreaks;
		maxBloodStreams = other.maxBloodStreams;
		maxWallStreaks = other.maxWallStreaks;
		maxFloorStreaks = other.maxFloorStreaks;
		maxFloorPools = other.maxFloorPools;
		maxBloodParticles = other.maxBloodParticles;
		maxGibs = other.maxGibs;
	}

	public SerializablePerformanceSettings Copy()
	{
		SerializablePerformanceSettings serializablePerformanceSettings = new SerializablePerformanceSettings();
		serializablePerformanceSettings.Overwrite(this);
		return serializablePerformanceSettings;
	}

	public static int PaintTextureResolutionIndex(int value)
	{
		return EnumHelper.GetEnumIndex(typeof(PaintTexturePixelsPerMeter), value, 3);
	}

	public static int PaintTextureResolutionValue(int index)
	{
		return EnumHelper.GetEnumValue(typeof(PaintTexturePixelsPerMeter), index, 48);
	}

	public static int ParticlesLifetimeIndex(int value)
	{
		return EnumHelper.GetEnumIndex(typeof(ParticlesLifetime), value, 2);
	}

	public static int ParticlesLifetimeValue(int index)
	{
		return EnumHelper.GetEnumValue(typeof(ParticlesLifetime), index, 20);
	}

	public static int WoundStreakLifetimeIndex(int value)
	{
		return EnumHelper.GetEnumIndex(typeof(WoundStreakLifetime), value, 2);
	}

	public static int WoundStreakLifetimeValue(int index)
	{
		return EnumHelper.GetEnumValue(typeof(WoundStreakLifetime), index, 3);
	}

	public void Save()
	{
		if (!Serializer.SerializeToDisk(this, typeof(SerializablePerformanceSettings), Application.persistentDataPath + "/performance.dat"))
		{
			Debug.LogWarning("Failed to save performance settings.");
		}
		onChanged.Invoke(this);
	}

	public void Load()
	{
		SerializablePerformanceSettings serializablePerformanceSettings = null;
		try
		{
			serializablePerformanceSettings = (SerializablePerformanceSettings)Serializer.DeserializeFromDisk(typeof(SerializablePerformanceSettings), Application.persistentDataPath + "/performance.dat");
		}
		catch (IOException)
		{
		}
		catch (Exception)
		{
			Global.Context.ClearSettingsFile("performance.dat", "performance", false);
			Global.Context.MessageBox.ShowOK("Failed to load performance settings.\nDefaults have been restored.");
		}
		if (serializablePerformanceSettings == null)
		{
			serializablePerformanceSettings = new SerializablePerformanceSettings();
		}
		Overwrite(serializablePerformanceSettings);
	}

	public void InvokeAll()
	{
		onCameraGrainChanged.Invoke(cameraGrain);
		onCameraBloomChanged.Invoke(cameraBloom);
		onCameraBlurChanged.Invoke(cameraBlur);
		onPaintTextureResolutionChanged.Invoke(paintTexturePixelsPerMeter);
	}

	public void ClearMissionListeners()
	{
		onCameraGrainChanged.RemoveAllListeners();
		onCameraBloomChanged.RemoveAllListeners();
		onCameraBlurChanged.RemoveAllListeners();
		onShadowsChanged.RemoveAllListeners();
	}

	public static SerializablePerformanceSettings GetPreset(int preset)
	{
		return GetPreset((QualityLevels.QualityLevel)preset);
	}

	public static SerializablePerformanceSettings GetPreset(QualityLevels.QualityLevel preset)
	{
		switch (preset)
		{
		case QualityLevels.QualityLevel.Fastest:
		{
			SerializablePerformanceSettings serializablePerformanceSettings = new SerializablePerformanceSettings();
			serializablePerformanceSettings.preset = 0;
			serializablePerformanceSettings.qualityLevel = 0;
			serializablePerformanceSettings.cameraGrain = false;
			serializablePerformanceSettings.cameraBloom = false;
			serializablePerformanceSettings.cameraBlur = false;
			serializablePerformanceSettings.woundDecals = false;
			serializablePerformanceSettings.shadows = 0;
			serializablePerformanceSettings.paintTexturePixelsPerMeter = 0;
			serializablePerformanceSettings.dynamicLimbDecalMode = 2;
			serializablePerformanceSettings.particlesLifetime = 1;
			serializablePerformanceSettings.woundStreakLifetime = 1;
			serializablePerformanceSettings.impactEmissionsMode = 3;
			serializablePerformanceSettings.maxEnvironmentHitEffects = 0;
			serializablePerformanceSettings.maxEnvironmentHitMeshes = 0;
			serializablePerformanceSettings.maxGoreHitEffects = 0;
			serializablePerformanceSettings.maxSoakSpots = 0;
			serializablePerformanceSettings.maxWoundStreaks = 0;
			serializablePerformanceSettings.maxBloodStreams = 0;
			serializablePerformanceSettings.maxWallStreaks = 0;
			serializablePerformanceSettings.maxFloorStreaks = 0;
			serializablePerformanceSettings.maxFloorPools = 0;
			serializablePerformanceSettings.maxBloodParticles = 0;
			serializablePerformanceSettings.maxGibs = 0;
			return serializablePerformanceSettings;
		}
		case QualityLevels.QualityLevel.Fast:
		{
			SerializablePerformanceSettings serializablePerformanceSettings = new SerializablePerformanceSettings();
			serializablePerformanceSettings.preset = 1;
			serializablePerformanceSettings.qualityLevel = 1;
			serializablePerformanceSettings.cameraGrain = false;
			serializablePerformanceSettings.cameraBloom = false;
			serializablePerformanceSettings.cameraBlur = false;
			serializablePerformanceSettings.woundDecals = true;
			serializablePerformanceSettings.shadows = 0;
			serializablePerformanceSettings.paintTexturePixelsPerMeter = 16;
			serializablePerformanceSettings.dynamicLimbDecalMode = 2;
			serializablePerformanceSettings.particlesLifetime = 1;
			serializablePerformanceSettings.woundStreakLifetime = 1;
			serializablePerformanceSettings.impactEmissionsMode = 1;
			serializablePerformanceSettings.maxEnvironmentHitEffects = 4;
			serializablePerformanceSettings.maxEnvironmentHitMeshes = 8;
			serializablePerformanceSettings.maxGoreHitEffects = 4;
			serializablePerformanceSettings.maxSoakSpots = 2;
			serializablePerformanceSettings.maxWoundStreaks = 4;
			serializablePerformanceSettings.maxBloodStreams = 2;
			serializablePerformanceSettings.maxWallStreaks = 4;
			serializablePerformanceSettings.maxFloorStreaks = 2;
			serializablePerformanceSettings.maxFloorPools = 2;
			serializablePerformanceSettings.maxBloodParticles = 150;
			serializablePerformanceSettings.maxGibs = 20;
			return serializablePerformanceSettings;
		}
		case QualityLevels.QualityLevel.Simple:
		{
			SerializablePerformanceSettings serializablePerformanceSettings = new SerializablePerformanceSettings();
			serializablePerformanceSettings.preset = 2;
			serializablePerformanceSettings.qualityLevel = 2;
			serializablePerformanceSettings.cameraGrain = true;
			serializablePerformanceSettings.cameraBloom = false;
			serializablePerformanceSettings.cameraBlur = false;
			serializablePerformanceSettings.woundDecals = true;
			serializablePerformanceSettings.shadows = 1;
			serializablePerformanceSettings.paintTexturePixelsPerMeter = 32;
			serializablePerformanceSettings.dynamicLimbDecalMode = 1;
			serializablePerformanceSettings.particlesLifetime = 5;
			serializablePerformanceSettings.woundStreakLifetime = 2;
			serializablePerformanceSettings.impactEmissionsMode = 0;
			serializablePerformanceSettings.maxEnvironmentHitEffects = 8;
			serializablePerformanceSettings.maxEnvironmentHitMeshes = 15;
			serializablePerformanceSettings.maxGoreHitEffects = 8;
			serializablePerformanceSettings.maxSoakSpots = 4;
			serializablePerformanceSettings.maxWoundStreaks = 8;
			serializablePerformanceSettings.maxBloodStreams = 4;
			serializablePerformanceSettings.maxWallStreaks = 8;
			serializablePerformanceSettings.maxFloorStreaks = 4;
			serializablePerformanceSettings.maxFloorPools = 4;
			serializablePerformanceSettings.maxBloodParticles = 325;
			serializablePerformanceSettings.maxGibs = 40;
			return serializablePerformanceSettings;
		}
		case QualityLevels.QualityLevel.Good:
			return new SerializablePerformanceSettings();
		case QualityLevels.QualityLevel.Beautiful:
		{
			SerializablePerformanceSettings serializablePerformanceSettings = new SerializablePerformanceSettings();
			serializablePerformanceSettings.preset = 4;
			serializablePerformanceSettings.qualityLevel = 4;
			serializablePerformanceSettings.cameraGrain = true;
			serializablePerformanceSettings.cameraBloom = true;
			serializablePerformanceSettings.cameraBlur = true;
			serializablePerformanceSettings.woundDecals = true;
			serializablePerformanceSettings.shadows = 2;
			serializablePerformanceSettings.paintTexturePixelsPerMeter = 64;
			serializablePerformanceSettings.dynamicLimbDecalMode = 0;
			serializablePerformanceSettings.particlesLifetime = 60;
			serializablePerformanceSettings.woundStreakLifetime = 5;
			serializablePerformanceSettings.impactEmissionsMode = 0;
			serializablePerformanceSettings.maxEnvironmentHitEffects = 35;
			serializablePerformanceSettings.maxEnvironmentHitMeshes = 70;
			serializablePerformanceSettings.maxGoreHitEffects = 35;
			serializablePerformanceSettings.maxSoakSpots = 18;
			serializablePerformanceSettings.maxWoundStreaks = 35;
			serializablePerformanceSettings.maxBloodStreams = 18;
			serializablePerformanceSettings.maxWallStreaks = 35;
			serializablePerformanceSettings.maxFloorStreaks = 18;
			serializablePerformanceSettings.maxFloorPools = 18;
			serializablePerformanceSettings.maxBloodParticles = 1450;
			serializablePerformanceSettings.maxGibs = 180;
			return serializablePerformanceSettings;
		}
		case QualityLevels.QualityLevel.Fantastic:
		{
			SerializablePerformanceSettings serializablePerformanceSettings = new SerializablePerformanceSettings();
			serializablePerformanceSettings.preset = 5;
			serializablePerformanceSettings.qualityLevel = 5;
			serializablePerformanceSettings.cameraGrain = true;
			serializablePerformanceSettings.cameraBloom = true;
			serializablePerformanceSettings.cameraBlur = true;
			serializablePerformanceSettings.woundDecals = true;
			serializablePerformanceSettings.shadows = 2;
			serializablePerformanceSettings.paintTexturePixelsPerMeter = 96;
			serializablePerformanceSettings.dynamicLimbDecalMode = 0;
			serializablePerformanceSettings.particlesLifetime = 180;
			serializablePerformanceSettings.woundStreakLifetime = 7;
			serializablePerformanceSettings.impactEmissionsMode = 0;
			serializablePerformanceSettings.maxEnvironmentHitEffects = 50;
			serializablePerformanceSettings.maxEnvironmentHitMeshes = 100;
			serializablePerformanceSettings.maxGoreHitEffects = 50;
			serializablePerformanceSettings.maxSoakSpots = 25;
			serializablePerformanceSettings.maxWoundStreaks = 50;
			serializablePerformanceSettings.maxBloodStreams = 25;
			serializablePerformanceSettings.maxWallStreaks = 50;
			serializablePerformanceSettings.maxFloorStreaks = 25;
			serializablePerformanceSettings.maxFloorPools = 25;
			serializablePerformanceSettings.maxBloodParticles = 2000;
			serializablePerformanceSettings.maxGibs = 250;
			return serializablePerformanceSettings;
		}
		case QualityLevels.QualityLevel.Unlimited:
		{
			SerializablePerformanceSettings serializablePerformanceSettings = new SerializablePerformanceSettings();
			serializablePerformanceSettings.preset = 6;
			serializablePerformanceSettings.qualityLevel = 5;
			serializablePerformanceSettings.cameraGrain = true;
			serializablePerformanceSettings.cameraBloom = true;
			serializablePerformanceSettings.cameraBlur = true;
			serializablePerformanceSettings.woundDecals = true;
			serializablePerformanceSettings.shadows = 2;
			serializablePerformanceSettings.paintTexturePixelsPerMeter = 96;
			serializablePerformanceSettings.dynamicLimbDecalMode = 0;
			serializablePerformanceSettings.particlesLifetime = 3600;
			serializablePerformanceSettings.woundStreakLifetime = 10;
			serializablePerformanceSettings.impactEmissionsMode = 0;
			serializablePerformanceSettings.maxEnvironmentHitEffects = -1;
			serializablePerformanceSettings.maxEnvironmentHitMeshes = -1;
			serializablePerformanceSettings.maxGoreHitEffects = -1;
			serializablePerformanceSettings.maxSoakSpots = -1;
			serializablePerformanceSettings.maxWoundStreaks = -1;
			serializablePerformanceSettings.maxBloodStreams = -1;
			serializablePerformanceSettings.maxWallStreaks = -1;
			serializablePerformanceSettings.maxFloorStreaks = -1;
			serializablePerformanceSettings.maxFloorPools = -1;
			serializablePerformanceSettings.maxBloodParticles = -1;
			serializablePerformanceSettings.maxGibs = -1;
			return serializablePerformanceSettings;
		}
		default:
			return new SerializablePerformanceSettings();
		}
	}
}

}

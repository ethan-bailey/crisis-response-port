namespace Crisis.Runtime.FullSource
{
using Crisis.Runtime.Weapons;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;
using System;
using FullSerializer;
using UnityEngine;

[fsObject("0_0_8_005", new Type[] { typeof(SerializableGraphicsSettings_v3) })]
public class SerializableGraphicsSettings_0_0_8_005
{
	public enum ParticlesLifetime
	{
		ExtraShort = 0,
		Short = 1,
		Normal = 2,
		Long = 3,
		ExtraLong = 4,
		Permanent = 5
	}

	public enum CameraFieldOfViewMode
	{
		Dynamic = 0,
		Static = 1
	}

	public enum DynamicLimbDecalsMode
	{
		All = 0,
		OnlyHead = 1,
		Off = 2
	}

	public enum DynamicLimbDecalsCount
	{
		Low = 0,
		Medium = 1,
		High = 2,
		Higher = 3,
		Unlimited = 4
	}

	public int vsync = 1;

	public bool showFrameRate;

	public LightShadows shadows = LightShadows.Soft;

	public int paintTexturePixelsPerMeter = 64;

	public bool impactMist = true;

	public bool impactEmissions = true;

	public bool headEmissions = true;

	public bool woundEmissions = true;

	public bool wallStreaks = true;

	public bool woundDecals = true;

	public ParticlesLifetime particlesLifetime = ParticlesLifetime.Normal;

	public CameraFieldOfViewMode cameraFieldOfViewMode = CameraFieldOfViewMode.Static;

	public float dynamicFieldOfViewMin = 20f;

	public float dynamicFieldOfViewMax = 35f;

	public float staticFieldOfView = 20f;

	public bool cameraNoise = true;

	public bool cameraBloom = true;

	public bool applyCensoring;

	public DynamicLimbDecalsMode dynamicLimbDecalMode;

	public DynamicLimbDecalsCount dynamicLimbDecalCount = DynamicLimbDecalsCount.High;

	public bool floorPools = true;

	public bool floorStreaks = true;

	public float ambientLightScale = 1f;

	public SerializableGraphicsSettings_0_0_8_005(SerializableGraphicsSettings_v3 data)
	{
		vsync = data.vsync;
		showFrameRate = data.showFrameRate;
		shadows = data.shadows;
		paintTexturePixelsPerMeter = data.paintTexturePixelsPerMeter;
		impactMist = data.impactMist;
		impactEmissions = data.impactEmissions;
		headEmissions = data.headEmissions;
		woundEmissions = data.woundEmissions;
		wallStreaks = data.wallStreaks;
		woundDecals = data.woundDecals;
		particlesLifetime = (ParticlesLifetime)data.particlesLifetime;
		cameraFieldOfViewMode = (CameraFieldOfViewMode)data.cameraFieldOfViewMode;
		dynamicFieldOfViewMin = data.dynamicFieldOfViewMin;
		dynamicFieldOfViewMax = data.dynamicFieldOfViewMax;
		staticFieldOfView = data.staticFieldOfView;
		cameraNoise = data.cameraNoise;
		cameraBloom = data.cameraBloom;
		applyCensoring = false;
		dynamicLimbDecalMode = (DynamicLimbDecalsMode)data.dynamicLimbDecalMode;
		dynamicLimbDecalCount = (DynamicLimbDecalsCount)data.dynamicLimbDecalCount;
		floorPools = data.floorPools;
		floorStreaks = data.floorStreaks;
		ambientLightScale = 1f;
	}
}

}

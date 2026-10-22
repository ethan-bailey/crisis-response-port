namespace Crisis.Runtime.FullSource
{
using Crisis.Runtime.Weapons;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;
using System;
using FullSerializer;
using UnityEngine;

[fsObject("0.0.7.008", new Type[] { typeof(SerializableGraphicsSettings_v1) })]
public class SerializableGraphicsSettings_v_0_0_7_008
{
	public enum PaintTextureResolution
	{
		Off = 0,
		Simple = 1,
		Decent = 2,
		Good = 3,
		Best = 4
	}

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

	public PaintTextureResolution paintTexturePixelsPerMeter = PaintTextureResolution.Best;

	public bool impactMist = true;

	public bool impactEmissions = true;

	public bool headEmissions = true;

	public bool woundEmissions = true;

	public bool wallStreaks = true;

	public bool woundDecals = true;

	public ParticlesLifetime particlesLifetime = ParticlesLifetime.Long;

	public CameraFieldOfViewMode cameraFieldOfViewMode = CameraFieldOfViewMode.Static;

	public float dynamicFieldOfViewMin = 20f;

	public float dynamicFieldOfViewMax = 35f;

	public float staticFieldOfView = 20f;

	public bool cameraNoise = true;

	public bool cameraBloom = true;

	public bool applyCensoring;

	public DynamicLimbDecalsMode dynamicLimbDecalMode;

	public DynamicLimbDecalsCount dynamicLimbDecalCount = DynamicLimbDecalsCount.High;

	public SerializableGraphicsSettings_v_0_0_7_008(SerializableGraphicsSettings_v1 data)
	{
		vsync = data.vsync;
		showFrameRate = data.showFrameRate;
		shadows = data.shadows;
		paintTexturePixelsPerMeter = (PaintTextureResolution)data.paintTexturePixelsPerMeter;
		impactMist = data.impactMist;
		impactEmissions = data.impactEmissions;
		headEmissions = data.headEmissions;
		woundEmissions = data.woundEmissions;
		wallStreaks = data.wallStreaks;
		woundDecals = data.bodyDecals;
		particlesLifetime = (ParticlesLifetime)data.particlesLifetime;
		cameraFieldOfViewMode = (CameraFieldOfViewMode)data.cameraFieldOfViewMode;
		dynamicFieldOfViewMin = data.dynamicFieldOfViewMin;
		dynamicFieldOfViewMax = data.dynamicFieldOfViewMax;
		staticFieldOfView = data.staticFieldOfView;
		cameraNoise = data.cameraNoise;
		cameraBloom = data.cameraBloom;
		applyCensoring = false;
		dynamicLimbDecalMode = DynamicLimbDecalsMode.All;
		dynamicLimbDecalCount = DynamicLimbDecalsCount.High;
	}

	public static int PaintTextureResolutionValue(PaintTextureResolution value)
	{
		switch (value)
		{
		case PaintTextureResolution.Off:
			return 0;
		case PaintTextureResolution.Simple:
			return 16;
		case PaintTextureResolution.Decent:
			return 32;
		case PaintTextureResolution.Good:
			return 48;
		case PaintTextureResolution.Best:
			return 64;
		default:
			return 64;
		}
	}
}

}

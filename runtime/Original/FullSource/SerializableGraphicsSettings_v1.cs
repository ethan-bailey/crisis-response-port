namespace Crisis.Runtime.FullSource
{
using Crisis.Runtime.Weapons;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;
using System;
using FullSerializer;
using UnityEngine;

[fsObject("1", new Type[] { })]
public class SerializableGraphicsSettings_v1
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

	public int vsync = 1;

	public bool showFrameRate;

	public LightShadows shadows = LightShadows.Soft;

	public PaintTextureResolution paintTexturePixelsPerMeter = PaintTextureResolution.Best;

	public bool impactMist = true;

	public bool impactEmissions = true;

	public bool headEmissions = true;

	public bool woundEmissions = true;

	public bool wallStreaks = true;

	public bool bodyDecals = true;

	public ParticlesLifetime particlesLifetime = ParticlesLifetime.Long;

	public CameraFieldOfViewMode cameraFieldOfViewMode = CameraFieldOfViewMode.Static;

	public float dynamicFieldOfViewMin = 20f;

	public float dynamicFieldOfViewMax = 35f;

	public float staticFieldOfView = 20f;

	public bool cameraNoise = true;

	public bool cameraBloom = true;
}

}

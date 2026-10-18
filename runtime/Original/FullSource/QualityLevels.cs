namespace Crisis.Runtime.FullSource
{
using Crisis.Runtime.Weapons;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;
using UnityEngine;

public static class QualityLevels
{
	public enum QualityLevel
	{
		Fastest = 0,
		Fast = 1,
		Simple = 2,
		Good = 3,
		Beautiful = 4,
		Fantastic = 5,
		Unlimited = 6
	}

	public static void SetQualityLevel(int level)
	{
		SetQualityLevel((QualityLevel)level);
	}

	public static void SetQualityLevel(QualityLevel level)
	{
		switch (level)
		{
		case QualityLevel.Fastest:
			QualitySettings.pixelLightCount = 2;
			QualitySettings.masterTextureLimit = 2;
			QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
			QualitySettings.antiAliasing = 0;
			QualitySettings.softParticles = false;
			QualitySettings.realtimeReflectionProbes = false;
			QualitySettings.billboardsFaceCameraPosition = false;
			QualitySettings.resolutionScalingFixedDPIFactor = 1f;
			QualitySettings.shadows = ShadowQuality.Disable;
			QualitySettings.shadowResolution = ShadowResolution.Low;
			QualitySettings.shadowProjection = ShadowProjection.CloseFit;
			QualitySettings.shadowDistance = 10f;
			QualitySettings.shadowmaskMode = ShadowmaskMode.Shadowmask;
			QualitySettings.shadowNearPlaneOffset = 2f;
			QualitySettings.shadowCascades = 0;
			QualitySettings.lodBias = 0.4f;
			QualitySettings.maximumLODLevel = 0;
			QualitySettings.particleRaycastBudget = 128;
			QualitySettings.asyncUploadTimeSlice = 2;
			QualitySettings.asyncUploadBufferSize = 4;
			break;
		case QualityLevel.Fast:
			QualitySettings.pixelLightCount = 4;
			QualitySettings.masterTextureLimit = 1;
			QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
			QualitySettings.antiAliasing = 0;
			QualitySettings.softParticles = false;
			QualitySettings.realtimeReflectionProbes = false;
			QualitySettings.billboardsFaceCameraPosition = false;
			QualitySettings.resolutionScalingFixedDPIFactor = 1f;
			QualitySettings.shadows = ShadowQuality.Disable;
			QualitySettings.shadowResolution = ShadowResolution.Low;
			QualitySettings.shadowProjection = ShadowProjection.CloseFit;
			QualitySettings.shadowDistance = 10f;
			QualitySettings.shadowmaskMode = ShadowmaskMode.Shadowmask;
			QualitySettings.shadowNearPlaneOffset = 2f;
			QualitySettings.shadowCascades = 0;
			QualitySettings.lodBias = 0.7f;
			QualitySettings.maximumLODLevel = 0;
			QualitySettings.particleRaycastBudget = 256;
			QualitySettings.asyncUploadTimeSlice = 2;
			QualitySettings.asyncUploadBufferSize = 4;
			break;
		case QualityLevel.Simple:
			QualitySettings.pixelLightCount = 8;
			QualitySettings.masterTextureLimit = 0;
			QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;
			QualitySettings.antiAliasing = 0;
			QualitySettings.softParticles = false;
			QualitySettings.realtimeReflectionProbes = false;
			QualitySettings.billboardsFaceCameraPosition = false;
			QualitySettings.resolutionScalingFixedDPIFactor = 1f;
			QualitySettings.shadows = ShadowQuality.HardOnly;
			QualitySettings.shadowResolution = ShadowResolution.Low;
			QualitySettings.shadowProjection = ShadowProjection.CloseFit;
			QualitySettings.shadowDistance = 20f;
			QualitySettings.shadowmaskMode = ShadowmaskMode.Shadowmask;
			QualitySettings.shadowNearPlaneOffset = 2f;
			QualitySettings.shadowCascades = 0;
			QualitySettings.lodBias = 1f;
			QualitySettings.maximumLODLevel = 0;
			QualitySettings.particleRaycastBudget = 512;
			QualitySettings.asyncUploadTimeSlice = 2;
			QualitySettings.asyncUploadBufferSize = 4;
			break;
		case QualityLevel.Good:
			QualitySettings.pixelLightCount = 16;
			QualitySettings.masterTextureLimit = 0;
			QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
			QualitySettings.antiAliasing = 2;
			QualitySettings.softParticles = false;
			QualitySettings.realtimeReflectionProbes = true;
			QualitySettings.billboardsFaceCameraPosition = true;
			QualitySettings.resolutionScalingFixedDPIFactor = 1f;
			QualitySettings.shadows = ShadowQuality.All;
			QualitySettings.shadowResolution = ShadowResolution.Medium;
			QualitySettings.shadowProjection = ShadowProjection.StableFit;
			QualitySettings.shadowDistance = 30f;
			QualitySettings.shadowmaskMode = ShadowmaskMode.DistanceShadowmask;
			QualitySettings.shadowNearPlaneOffset = 2f;
			QualitySettings.shadowCascades = 2;
			QualitySettings.lodBias = 1.5f;
			QualitySettings.maximumLODLevel = 0;
			QualitySettings.particleRaycastBudget = 1024;
			QualitySettings.asyncUploadTimeSlice = 2;
			QualitySettings.asyncUploadBufferSize = 4;
			break;
		case QualityLevel.Beautiful:
			QualitySettings.pixelLightCount = 32;
			QualitySettings.masterTextureLimit = 0;
			QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
			QualitySettings.antiAliasing = 4;
			QualitySettings.softParticles = true;
			QualitySettings.realtimeReflectionProbes = true;
			QualitySettings.billboardsFaceCameraPosition = true;
			QualitySettings.resolutionScalingFixedDPIFactor = 1f;
			QualitySettings.shadows = ShadowQuality.All;
			QualitySettings.shadowResolution = ShadowResolution.High;
			QualitySettings.shadowProjection = ShadowProjection.StableFit;
			QualitySettings.shadowDistance = 40f;
			QualitySettings.shadowmaskMode = ShadowmaskMode.DistanceShadowmask;
			QualitySettings.shadowNearPlaneOffset = 2f;
			QualitySettings.shadowCascades = 4;
			QualitySettings.lodBias = 1.75f;
			QualitySettings.maximumLODLevel = 0;
			QualitySettings.particleRaycastBudget = 2048;
			QualitySettings.asyncUploadTimeSlice = 2;
			QualitySettings.asyncUploadBufferSize = 4;
			break;
		case QualityLevel.Fantastic:
			QualitySettings.pixelLightCount = 64;
			QualitySettings.masterTextureLimit = 0;
			QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
			QualitySettings.antiAliasing = 8;
			QualitySettings.softParticles = true;
			QualitySettings.realtimeReflectionProbes = true;
			QualitySettings.billboardsFaceCameraPosition = true;
			QualitySettings.resolutionScalingFixedDPIFactor = 1f;
			QualitySettings.shadows = ShadowQuality.All;
			QualitySettings.shadowResolution = ShadowResolution.VeryHigh;
			QualitySettings.shadowProjection = ShadowProjection.StableFit;
			QualitySettings.shadowDistance = 50f;
			QualitySettings.shadowmaskMode = ShadowmaskMode.DistanceShadowmask;
			QualitySettings.shadowNearPlaneOffset = 2f;
			QualitySettings.shadowCascades = 4;
			QualitySettings.lodBias = 2f;
			QualitySettings.maximumLODLevel = 0;
			QualitySettings.particleRaycastBudget = 4096;
			QualitySettings.asyncUploadTimeSlice = 2;
			QualitySettings.asyncUploadBufferSize = 4;
			break;
		}
	}
}

}

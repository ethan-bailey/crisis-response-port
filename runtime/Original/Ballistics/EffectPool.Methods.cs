using System;
using System.Collections.Generic;
using UnityEngine;
using Object=UnityEngine.Object;
using Random=UnityEngine.Random;
public partial class PerformanceManager {
public BallisticHitEffect CreateEnvironmentalHitEffect(BallisticHitEffect prefab)
	{
		if (prefab == null)
		{
			return null;
		}
		if (!UnlimitedEnvironmentHitEffects && environmentHitEffects.Count >= MaxEnvironmentHitEffects && environmentHitEffects.Count > 0)
		{
			if (environmentHitEffects.First.Value != null)
			{
				Object.Destroy(environmentHitEffects.First.Value);
			}
			environmentHitEffects.RemoveFirst();
		}
		BallisticHitEffect ballisticHitEffect = Object.Instantiate(prefab);
		environmentHitEffects.AddLast(ballisticHitEffect.gameObject);
		return ballisticHitEffect;
	}
public BallisticHitMeshEffect SpawnEnvironmentalHitMesh(BallisticHitMeshEffect prefab, Projectile projectile, Transform parent)
	{
		if (prefab == null)
		{
			return null;
		}
		if (!UnlimitedEnvironmentHitMeshes && environmentHitMeshes.Count >= MaxEnvironmentHitMeshes && environmentHitMeshes.Count > 0)
		{
			if (environmentHitMeshes.First.Value != null)
			{
				Object.Destroy(environmentHitMeshes.First.Value);
			}
			environmentHitMeshes.RemoveFirst();
		}
		BallisticHitMeshEffect ballisticHitMeshEffect = Object.Instantiate(prefab);
		ballisticHitMeshEffect.Play(parent, projectile);
		environmentHitMeshes.AddLast(ballisticHitMeshEffect.gameObject);
		return ballisticHitMeshEffect;
	}
}

using System;
using System.Collections.Generic;
using UnityEngine;
using Object=UnityEngine.Object;
using Random=UnityEngine.Random;

public partial class BallisticHitMeshEffect : MonoBehaviour
{

	public float scaleRatio = 0.01f;

	public float maxScale = 5f;

	private float spawnTime;

	private float targetScale;

	private float animationTime;

	private void Update()
	{
		float num = Mathf.Lerp(0.01f, targetScale, (Time.time - spawnTime) / animationTime);
		base.transform.localScale = new Vector3(1f, 1f, num);
		if (num >= targetScale)
		{
			Object.Destroy(this);
		}
	}

	public void Play(Transform parent, Projectile projectile)
	{
		base.transform.position = projectile.Position;
		base.transform.rotation = projectile.transform.rotation;
		base.transform.SetParent(parent, true);
		base.transform.localScale = new Vector3(1f, 1f, 0.01f);
		base.gameObject.layer = parent.gameObject.layer;
		spawnTime = Time.time;
		animationTime = Mathf.Clamp((!(projectile.Velocity > 0f)) ? 0f : (1f / projectile.Velocity), 0f, 0.2f);
		targetScale = Mathf.Clamp(projectile.Velocity * scaleRatio * Random.Range(0.5f, 1f), 0f, maxScale);
	}
}

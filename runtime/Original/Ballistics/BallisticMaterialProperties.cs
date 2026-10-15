using System;
using System.Collections.Generic;
using UnityEngine;
using Object=UnityEngine.Object;
using Random=UnityEngine.Random;

public class BallisticMaterialProperties : MonoBehaviour
{


	public float surfaceThickness;

	public float surfaceDensity = 1000f;

	public float maximumDeflection;


	public float density = 1000f;

	public Vector2 densityRandomness = new Vector2(1f, 1f);


	public BallisticHitEffect entryEffect;

	public BallisticHitEffect exitEffect;

	public BallisticHitMeshEffect exitHole;
}

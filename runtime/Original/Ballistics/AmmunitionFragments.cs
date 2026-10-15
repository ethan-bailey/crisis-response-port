using System;
using System.Collections.Generic;
using UnityEngine;
using Object=UnityEngine.Object;
using Random=UnityEngine.Random;
public partial class AmmunitionBase {
public enum ExplosionMode
	{
		None = 0,
		OnFragmentation = 1,
		Timed = 2,
		TimedAfterFragmentation = 3
	}
[Serializable]
public class FragmentationSpecification
	{
		public float mass = 1f;

		public float diameter = 5f;

		public float deflection;

		public FragmentationSpecification()
		{
		}

		public FragmentationSpecification(FragmentationSpecification original)
		{
			mass = original.mass;
			diameter = original.diameter;
			deflection = original.deflection;
		}
	}
public List<FragmentationSpecification> specificFragmentation=new();
public ExplosionMode explosionMode;
}

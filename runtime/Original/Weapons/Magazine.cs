using UnityEngine;
using System;
using System.Collections.Generic;
using Object=UnityEngine.Object;
using Random=UnityEngine.Random;
namespace Crisis.Runtime.Weapons;

public class Magazine
{
	public int capacity;

	public int roundCount;

	public string calibre;

	public float malfunctionRate;

	private WeaponBase weapon;

	private bool fireFromMagazine;

	public bool Empty
	{
		get
		{
			return roundCount <= 0;
		}
	}

	public bool Full
	{
		get
		{
			return roundCount == capacity;
		}
	}

	public float Ratio
	{
		get
		{
			return (float)roundCount / (float)capacity;
		}
	}

	public bool UseMalfunctions { get; set; }

	public Magazine(WeaponBase weapon)
	{
		this.weapon = weapon;
		calibre = weapon.calibre;
		malfunctionRate = weapon.ComponentMagazine.malfunctionRate;
		fireFromMagazine = weapon.fireFromMagazine;
		capacity = weapon.ComponentMagazine.capacity - (fireFromMagazine ? 1 : 0);
		if (weapon.ammunition != null)
		{
			roundCount = capacity;
		}
	}

	public bool RemoveOne(bool manualFeed)
	{
		if (!manualFeed && UseMalfunctions && Random.value <= malfunctionRate * weapon.MalfunctionsMultiplier)
		{
			return false;
		}
		if (roundCount > 0)
		{
			roundCount--;
			return true;
		}
		return false;
	}

	public void AddOne()
	{
		if (roundCount < capacity)
		{
			roundCount++;
		}
	}

	public void Fill()
	{
		roundCount = capacity;
	}
}

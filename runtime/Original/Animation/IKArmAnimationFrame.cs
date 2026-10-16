using System;
using UnityEngine;

[Serializable]
public class IKArmAnimationFrame
{
	public string name;

	public IKArmAction action;

	public IKArmTarget transform;

	public Vector3 offset;

	public float time = 0.1f;

	public IKControllerInvoke callback;

	public int waitForSiblingFrame;
}

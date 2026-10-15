using System;
using System.Collections.Generic;
using UnityEngine;
using Object=UnityEngine.Object;
using Random=UnityEngine.Random;

public class BallisticWindow : BallisticMaterial
{
	public Texture2D texture;

	public Texture2D holeTexture;

	private Texture2D textureInstance;

	public override void Awake()
	{
		base.Awake();
		Renderer component = GetComponent<Renderer>();
		textureInstance = Object.Instantiate(texture);
		component.material.mainTexture = textureInstance;
	}

	private void OnDestroy()
	{
		if (textureInstance != null)
		{
			Object.Destroy(textureInstance);
		}
	}

	public void Reset()
	{
		if (textureInstance != null)
		{
			Object.Destroy(textureInstance);
		}
		Renderer component = GetComponent<Renderer>();
		textureInstance = Object.Instantiate(texture);
		component.material.mainTexture = textureInstance;
	}

	public override bool PassThroughWithoutContact(Vector3 worldPosition, Projectile projectile)
	{
		Vector2 vector = WorldPointToTextureSpace(worldPosition);
		if (TestPixel((int)vector.x, (int)vector.y))
		{
			return false;
		}
		projectile.transform.position = worldPosition + projectile.transform.forward * width;
		return true;
	}

	protected override void FindInternalDistanceAndExitPosition(Projectile projectile)
	{
		availableInternalDistance = 0f;
		int num = FindMarksmanEntryAndExit(projectile.transform.position, projectile.transform.forward, out worldEntryPosition, out worldExitPosition);
		foundPotentialExit = num >= 2;
		if (foundPotentialExit)
		{
			availableInternalDistance = (worldEntryPosition - worldExitPosition).magnitude;
		}
	}

	private bool TestPixel(int x, int y)
	{
		if (x < 0 || x >= textureInstance.width || y < 0 || y >= textureInstance.height)
		{
			return false;
		}
		return textureInstance.GetPixel(x, y).a > 0.25f;
	}

	private int FindMarksmanEntryAndExit(Vector3 worldPosition, Vector3 worldDirection, out Vector3 worldEntry, out Vector3 worldExit)
	{
		Vector2 vector = WorldPointToTextureSpace(worldPosition);
		if (TestPixel((int)vector.x, (int)vector.y))
		{
			worldEntry = worldPosition;
			worldExit = worldPosition + worldDirection * width;
			StampAlpha(holeTexture, (int)vector.x, (int)vector.y);
			return 3;
		}
		worldEntry = (worldExit = Vector3.zero);
		return 0;
	}

	private Vector2 WorldPointToTextureSpace(Vector3 worldPoint)
	{
		Vector3 vector = base.transform.InverseTransformPoint(worldPoint) + new Vector3(0.5f, 0.5f, 0f);
		return new Vector2(vector.x * (float)textureInstance.width, vector.y * (float)textureInstance.height);
	}

	private void StampAlpha(Texture2D stamp, int x, int y)
	{
		if (stamp == null)
		{
			return;
		}
		Rect a = new Rect(x - stamp.width / 2, y - stamp.height / 2, stamp.width, stamp.height);
		Rect b = new Rect(0f, 0f, textureInstance.width, textureInstance.height);
		Rect intersect = default(Rect);
		if (StaticUtilities.RectIntersect(a, b, out intersect))
		{
			int x2 = Mathf.FloorToInt(intersect.x);
			int y2 = Mathf.FloorToInt(intersect.y);
			int num = Mathf.FloorToInt(intersect.width);
			int num2 = Mathf.FloorToInt(intersect.height);
			Color[] pixels = textureInstance.GetPixels(x2, y2, num, num2);
			int x3 = ((x + stamp.width < textureInstance.width) ? (stamp.width - num) : 0);
			int y3 = ((y + stamp.height < textureInstance.height) ? (stamp.height - num2) : 0);
			Color[] pixels2 = stamp.GetPixels(x3, y3, num, num2, 0);
			for (int i = 0; i < pixels.Length; i++)
			{
				pixels[i].a = pixels[i].a - pixels2[i].a;
			}
			textureInstance.SetPixels(x2, y2, num, num2, pixels);
			textureInstance.Apply();
		}
	}
}

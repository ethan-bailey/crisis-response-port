using System.Collections.Generic;
using UnityEngine;

public class LimbTextureData
{
	public Texture2D skinTexture;

	public Texture2D staticDamage;

	public Texture2D staticBlood;

	public Texture2D staticBurn;

	public DynamicLimbTextureData dynamic;

	private LimbRenderer owner;

	private Rect readSourceRect;

	public Vector2 PixelsPerMeter { get; private set; }

	public int Width { get; private set; }

	public int Height { get; private set; }

	public void Initialize(LimbRenderer owner, List<Texture2D> skins, Collider collider)
	{
		if (skins.Count == 0)
		{
			Debug.LogError("Missing skins for LimbTextureData.");
			return;
		}
		this.owner = owner;
		skinTexture = skins[Random.Range(0, skins.Count)];
		Width = skinTexture.width;
		Height = skinTexture.height;
		PixelsPerMeter = new Vector2((float)Width / collider.bounds.size.x, (float)Height / collider.bounds.size.y);
		readSourceRect = new Rect(0f, 0f, Width, Height);
		staticDamage = new Texture2D(Width, Height, TextureFormat.RGBA32, false);
		Color[] pixels = skinTexture.GetPixels();
		for (long num = 0L; num < pixels.LongLength; num++)
		{
			pixels[num].r = (pixels[num].b = 0f);
			pixels[num].g = ((!(pixels[num].a > 0f)) ? 1f : 0f);
		}
		staticDamage.SetPixels(pixels);
		staticDamage.Apply();
	}

	public void SetStatic(bool initialize = false)
	{
		if (initialize)
		{
			staticBlood = new Texture2D(Width, Height, TextureFormat.RGBA32, false);
			staticBurn = new Texture2D(Width, Height, TextureFormat.RGBA32, false);
		}
		Rect source = new Rect(0f, 0f, Width, Height);
		RenderTexture.active = dynamic.Damage;
		staticDamage.ReadPixels(source, 0, 0);
		staticDamage.Apply();
		RenderTexture.active = dynamic.Blood;
		staticBlood.ReadPixels(source, 0, 0);
		staticBlood.Apply();
		RenderTexture.active = dynamic.Burn;
		staticBurn.ReadPixels(source, 0, 0);
		staticBurn.Apply();
	}

	public void SetDynamic(bool initialize = false)
	{
		dynamic = DynamicLimbTexturePool.Instance.Create(owner, Width, Height);
		if (initialize)
		{
			Graphics.Blit(staticDamage, dynamic.Damage);
			owner.FillTexture(dynamic.Blood, Color.white);
			owner.FillTexture(dynamic.Burn, Color.white);
		}
		else
		{
			Graphics.Blit(staticDamage, dynamic.Damage);
			Graphics.Blit(staticBlood, dynamic.Blood);
			Graphics.Blit(staticBurn, dynamic.Burn);
		}
		RenderTexture.active = null;
	}

	public void Apply()
	{
		staticDamage.ReadPixels(readSourceRect, 0, 0);
		staticDamage.Apply();
	}

	public Color GetPixel(int x, int y)
	{
		return staticDamage.GetPixel(x, y);
	}

	public void Release()
	{
		if (staticDamage != null)
		{
			Object.Destroy(staticDamage);
		}
		if (staticBlood != null)
		{
			Object.Destroy(staticBlood);
		}
		if (staticBurn != null)
		{
			Object.Destroy(staticBurn);
		}
	}
}

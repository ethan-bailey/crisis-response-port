namespace Crisis.Runtime.FullSource
{
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;
using UnityEngine;

public class DynamicLimbTextureData
{
	public LimbRenderer renderer;

	public RenderTexture Damage { get; private set; }

	public RenderTexture Blood { get; private set; }

	public RenderTexture Burn { get; private set; }

	public void Initialize(LimbRenderer owner, int width, int height)
	{
		renderer = owner;
		Damage = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
		Blood = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
		Burn = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
	}

	public void Release()
	{
		if (Damage != null)
		{
			Damage.Release();
			Object.Destroy(Damage);
		}
		if (Blood != null)
		{
			Blood.Release();
			Object.Destroy(Blood);
		}
		if (Burn != null)
		{
			Burn.Release();
			Object.Destroy(Burn);
		}
	}
}

}

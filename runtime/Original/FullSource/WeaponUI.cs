namespace Crisis.Runtime.FullSource
{
using UnityEngine;
using UnityEngine.Events;

public class WeaponUI : MonoBehaviour
{
	public class TextureUpdatedEvent : UnityEvent<Texture>
	{
	}

	public TextureUpdatedEvent onTextureUpdated = new TextureUpdatedEvent();

	[Header("Read-only!")]
	public RenderTexture texture;

	private WeaponRenderer weaponRenderer;

	public RenderTexture Texture
	{
		get
		{
			if (texture == null)
			{
				texture = new RenderTexture(365, 200, 16);
				texture.anisoLevel = 4;
				texture.antiAliasing = 4;
				texture.filterMode = FilterMode.Trilinear;
				Render();
				return texture;
			}
			return texture;
		}
	}

	public void Render()
	{
		if (weaponRenderer == null)
		{
			weaponRenderer = Global.Context.MainMenu.weaponRenderer;
		}
		if (weaponRenderer != null)
		{
			weaponRenderer.Render(GetComponent<WeaponBase>(), texture);
			onTextureUpdated.Invoke(texture);
		}
	}
}

}

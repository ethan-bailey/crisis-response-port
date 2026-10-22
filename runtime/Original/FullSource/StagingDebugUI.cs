namespace Crisis.Runtime.FullSource
{
using UnityEngine;
using UnityEngine.UI;

public class StagingDebugUI : MonoBehaviour
{
	public Text text;

	private StaticDebug debug;

	private DynamicLimbTexturePool texturePool;

	private void Awake()
	{
		debug = StaticDebug.Instance;
		texturePool = DynamicLimbTexturePool.Instance;
	}

	private void LateUpdate()
	{
		text.text = string.Format("GL passes: {0}\nDamage passes: {1}\nBlood passes: {2}\nActive render targets: {3}", debug.GlPasses, debug.DamagePasses, debug.BloodPasses, texturePool.ActiveTextures);
		debug.Reset();
	}
}

}

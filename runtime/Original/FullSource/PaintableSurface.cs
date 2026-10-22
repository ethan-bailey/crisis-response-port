namespace Crisis.Runtime.FullSource
{
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;
using UnityEngine;

public class PaintableSurface : MonoBehaviour
{
	public enum Mode
	{
		Floor,
		Ceiling,
		Left,
		Right,
		Rear
	}

	private delegate Vector2 ModeDependentWorldToPixel(Vector3 worldPoint);

	[Tooltip("The orientation of the paintable surface area.")]
	public Mode mode;

	[Tooltip("Scales the length of vertical blood streaks on vertical surfaces. Higher value gives shorter streaks. On horizontal surfaces: Scales pool surface area where 1.0 yields the largest area.")]
	[Range(1f, 10f)]
	public float friction = 1f;

	[Tooltip("Used for vertical surfaces only. The height of the surface in meters.")]
	public float surfaceHeight = 2.5f;

	[Tooltip("How long the surface must wait between repaints. In seconds.")]
	[Range(0f, 1f)]
	public float repaintInterval;

	[Tooltip("The index for the material that will get the paint texture.")]
	public int paintableMaterialIndex;

	public Texture2D bloodMask;

	private Color baseColor = Color.clear;

	private Color paintColor = Color.white;

	private Color alphaColor;

	private static float RearWallTextureResolutionMultiplier = 1.5f;

	private MeshRenderer meshRenderer;

	private ModeDependentWorldToPixel delegatedWorldToPixel;

	private Transform streaksTransform;

	private PerformanceManager perfMgr;

	private int width;

	private int height;

	private float fWidth;

	private float fHeight;

	private float appliedTimestamp;

	private bool dirty;

	private bool useStreaks;

	public float PixelsPerMeter { get; private set; }

	private void Awake()
	{
		GameObject gameObject = new GameObject("Streaks");
		streaksTransform = gameObject.transform;
		streaksTransform.SetParent(transform);
		streaksTransform.localPosition = Vector3.zero;
	}

	private void Start()
	{
		if (!Global.Context.Settings.Performance.SurfaceDecalsEnabled)
		{
			Object.Destroy(this);
			return;
		}
		perfMgr = PerformanceManager.Instance;
		useStreaks = mode >= Mode.Left && perfMgr.WallStreaksEnabled;
		alphaColor = paintColor;
		InitializeTexture(Global.Context.Settings.Performance.paintTexturePixelsPerMeter);
		switch (mode)
		{
		default:
			delegatedWorldToPixel = FloorWorldToPixel;
			break;
		case Mode.Ceiling:
			delegatedWorldToPixel = CeilingWorldToPixel;
			break;
		case Mode.Left:
			delegatedWorldToPixel = LeftWorldToPixel;
			break;
		case Mode.Right:
			delegatedWorldToPixel = RightWorldToPixel;
			break;
		case Mode.Rear:
			delegatedWorldToPixel = RearWorldToPixel;
			break;
		}
		Global.Context.Mission.onPaintTextureResolutionChanged.AddListener(InitializeTexture);
	}

	private void InitializeTexture(int pixelsPerMeter)
	{
		PixelsPerMeter = pixelsPerMeter;
		if (pixelsPerMeter <= 0)
		{
			if (bloodMask != null)
			{
				Object.DestroyImmediate(bloodMask);
			}
			return;
		}
		switch (mode)
		{
		case Mode.Left:
		case Mode.Right:
			width = Mathf.NextPowerOfTwo((int)(transform.localScale.z * PixelsPerMeter));
			height = Mathf.NextPowerOfTwo((int)(surfaceHeight * PixelsPerMeter));
			break;
		case Mode.Rear:
			width = Mathf.NextPowerOfTwo((int)(transform.localScale.x * PixelsPerMeter * RearWallTextureResolutionMultiplier));
			height = Mathf.NextPowerOfTwo((int)(surfaceHeight * PixelsPerMeter * RearWallTextureResolutionMultiplier));
			break;
		default:
			width = Mathf.NextPowerOfTwo((int)(transform.localScale.z * PixelsPerMeter));
			height = Mathf.NextPowerOfTwo((int)(transform.localScale.x * PixelsPerMeter));
			break;
		}
		fWidth = width;
		fHeight = height;
		if (bloodMask != null)
		{
			Object.DestroyImmediate(bloodMask);
		}
		bloodMask = new Texture2D(width, height)
		{
			name = $"{name} blood mask"
		};
		bloodMask.wrapMode = TextureWrapMode.Clamp;
		meshRenderer = GetComponent<MeshRenderer>();
		meshRenderer.materials[paintableMaterialIndex].SetTexture("_Bloodmask", bloodMask);
		Color[] pixels = bloodMask.GetPixels();
		for (int i = 0; i < pixels.Length; i++)
		{
			ref Color reference = ref pixels[i];
			reference = baseColor;
		}
		bloodMask.SetPixels(pixels);
		bloodMask.Apply();
		appliedTimestamp = Time.realtimeSinceStartup;
	}

	private void Update()
	{
		if (dirty && !(Time.realtimeSinceStartup - appliedTimestamp < repaintInterval))
		{
			bloodMask.Apply();
			dirty = false;
			appliedTimestamp = Time.realtimeSinceStartup;
		}
	}

	public bool PaintPixel(Vector3 worldPoint, int retries = 0, float distance = 2f)
	{
		int num = 0;
		Vector2 insideUnitCircle = Random.insideUnitCircle;
		Vector2 vector = delegatedWorldToPixel(worldPoint);
		while (bloodMask.GetPixel((int)vector.x, (int)vector.y).a > 0.5f && num < retries)
		{
			vector += insideUnitCircle * distance;
			num++;
		}
		if (num >= retries)
		{
			return false;
		}
		bloodMask.SetPixel((int)vector.x, (int)vector.y, paintColor);
		dirty = true;
		return true;
	}

	public bool PaintPixelNoRetry(Vector3 worldPoint)
	{
		Vector2 vector = delegatedWorldToPixel(worldPoint);
		int x = (int)vector.x;
		int y = (int)vector.y;
		bool flag = bloodMask.GetPixel(x, y).a <= 0.5f;
		if (flag)
		{
			bloodMask.SetPixel(x, y, paintColor);
			dirty = true;
		}
		return flag;
	}

	public void PaintLine(Vector3 worldStart, Vector3 worldEnd, float maxLength = float.PositiveInfinity)
	{
		Vector3 vector = worldEnd - worldStart;
		float magnitude = vector.magnitude;
		if (!(magnitude > maxLength))
		{
			float num = magnitude * PixelsPerMeter * 1.5f;
			for (int i = 0; (float)i < num; i++)
			{
				Vector3 worldPoint = worldStart + vector * ((float)i / num);
				PaintPixelNoRetry(worldPoint);
			}
		}
	}

	public void PaintWedge(Vector3 fromWorldPosition, Vector3 toWorldPosition, float startWidth, float endWidth)
	{
		Vector3 vector = toWorldPosition - fromWorldPosition;
		float magnitude = vector.magnitude;
		float num = magnitude * PixelsPerMeter * 1.5f;
		float num2 = endWidth - startWidth;
		for (int i = 0; (float)i < num; i++)
		{
			float num3 = (float)i / num;
			float num4 = startWidth + num2 * num3;
			Vector3 vector2 = fromWorldPosition + vector * num3;
			Vector3 vector3 = Vector3.Cross(vector, Vector3.up).normalized * num4 * 0.5f;
			PaintLine(vector2 - vector3, vector2 + vector3);
		}
	}

	public void PaintPixel(Vector2 pixel)
	{
		bloodMask.SetPixel((int)pixel.x, (int)pixel.y, paintColor);
		dirty = true;
	}

	public float PaintPixelAddAlpha(Vector3 worldPoint, float alpha = 1f)
	{
		Vector2 vector = delegatedWorldToPixel(worldPoint);
		int x = (int)vector.x;
		int y = (int)vector.y;
		Color pixel = bloodMask.GetPixel(x, y);
		alphaColor.a = Mathf.Clamp01(pixel.a + alpha);
		float result = alphaColor.a - pixel.a;
		bloodMask.SetPixel(x, y, alphaColor);
		dirty = true;
		return result;
	}

	public void SpawnStreak(Vector3 position, Vector3 velocity)
	{
		if (useStreaks)
		{
			WallStreakPainter wallStreakPainter = perfMgr.CreateWallStreak(streaksTransform, position);
			wallStreakPainter.Initialize(this, velocity);
		}
	}

	private Vector2 TexelToPixel(Vector2 texel)
	{
		return new Vector2(texel.x * fWidth, texel.y * fHeight);
	}

	private Vector2 FloorWorldToPixel(Vector3 worldPoint)
	{
		return TexelToPixel(FloorWorldToTexel(worldPoint));
	}

	public Vector2 FloorWorldToTexel(Vector3 worldPoint)
	{
		Vector3 vector = transform.InverseTransformPoint(worldPoint);
		return new Vector2(1f - vector.z, vector.x);
	}

	private Vector2 CeilingWorldToPixel(Vector3 worldPoint)
	{
		return TexelToPixel(CeilingWorldToTexel(worldPoint));
	}

	public Vector2 CeilingWorldToTexel(Vector3 worldPoint)
	{
		Vector3 vector = transform.InverseTransformPoint(worldPoint);
		return new Vector2(vector.z, vector.x);
	}

	private Vector2 LeftWorldToPixel(Vector3 worldPoint)
	{
		Vector2 vector = LeftWorldToTexel(worldPoint);
		return new Vector2(vector.x * fWidth, vector.y * fHeight);
	}

	public Vector2 LeftWorldToTexel(Vector3 worldPoint)
	{
		Vector3 vector = transform.InverseTransformPoint(worldPoint);
		return new Vector2(1f - vector.z, vector.y / surfaceHeight);
	}

	private Vector2 RightWorldToPixel(Vector3 worldPoint)
	{
		Vector2 vector = RightWorldToTexel(worldPoint);
		return new Vector2(vector.x * fWidth, vector.y * fHeight);
	}

	public Vector2 RightWorldToTexel(Vector3 worldPoint)
	{
		Vector3 vector = transform.InverseTransformPoint(worldPoint);
		return new Vector2(vector.z, vector.y / surfaceHeight);
	}

	private Vector2 RearWorldToPixel(Vector3 worldPoint)
	{
		Vector2 vector = RearWorldToTexel(worldPoint);
		return new Vector2(vector.x * fWidth, vector.y * fHeight);
	}

	public Vector2 RearWorldToTexel(Vector3 worldPoint)
	{
		Vector3 vector = transform.InverseTransformPoint(worldPoint);
		return new Vector2(vector.x, vector.y / surfaceHeight);
	}
}

}

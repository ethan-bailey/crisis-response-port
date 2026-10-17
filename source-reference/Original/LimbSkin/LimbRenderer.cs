using System;
using System.Collections.Generic;
using UnityEngine;

public class LimbRenderer : MonoBehaviour
{
	public enum TraceResult
	{
		Failure = 0,
		Entry = 1,
		EntryExit = 2
	}

	private enum EdgeFindMode
	{
		Closest = 0,
		Furthest = 1
	}

	public bool useDynamicDecals = true;

	[Tooltip("A texture from this list is picked at random for the limb skin.")]
	public List<Texture2D> skins;

	[Tooltip("The base resolution for the data textures.")]
	public int resolution = 64;

	[Tooltip("The material switched to when the limb becomes damaged.")]
	public Material dynamicMaterial;

	[Header("Material debug")]
	public Material staticMaterialInstance;

	public Material dynamicMaterialInstance;

	public RenderTexture debugDynamicDamage;

	public RenderTexture debugDynamicBlood;

	public RenderTexture debugDynamicBurn;

	public Texture2D debugStaticDamage;

	public Texture2D debugStaticBlood;

	public Texture2D debugStaticBurn;

	private LimbTextureData textureData;

	private Material renderMaterialInstance;

	private Collider limbCollider;

	private BoxCollider2D boxCollider;

	private MeshRenderer meshRenderer;

	private Color woundColor;

	private bool staticInitialized;

	private bool dynamicInitialized;

	public bool IsDynamic { get; private set; }

	private void OnDestroy()
	{
		if (staticMaterialInstance != null)
		{
			UnityEngine.Object.Destroy(staticMaterialInstance);
		}
		if (dynamicMaterialInstance != null)
		{
			UnityEngine.Object.Destroy(dynamicMaterialInstance);
		}
		textureData.Release();
	}

	private void Awake()
	{
		meshRenderer = GetComponent<MeshRenderer>();
		boxCollider = GetComponent<BoxCollider2D>();
		limbCollider = base.transform.parent.GetComponent<Collider>();
		staticMaterialInstance = meshRenderer.material;
		textureData = new LimbTextureData();
		textureData.Initialize(this, skins, limbCollider);
		renderMaterialInstance = MissionHelper.Instance.RenderMaterialInstance;
		woundColor = new Color(0f, 0.5f, 0f);
		meshRenderer.material.SetTexture("_MainTex", textureData.skinTexture);
		IsDynamic = false;
	}

	public void SetStatic()
	{
		if (IsDynamic)
		{
			textureData.SetStatic(!staticInitialized);
			dynamicMaterialInstance.SetTexture("_MainTex", textureData.skinTexture);
			dynamicMaterialInstance.SetTexture("_DamageMask", textureData.staticDamage);
			dynamicMaterialInstance.SetTexture("_BloodMask", textureData.staticBlood);
			dynamicMaterialInstance.SetTexture("_BurnMask", textureData.staticBurn);
			meshRenderer.material = dynamicMaterialInstance;
			debugStaticDamage = textureData.staticDamage;
			debugStaticBlood = textureData.staticBlood;
			debugStaticBurn = textureData.staticBurn;
			textureData.dynamic = null;
			IsDynamic = (staticInitialized = false);
		}
	}

	public void SetDynamic()
	{
		if (!IsDynamic)
		{
			textureData.SetDynamic(!dynamicInitialized);
			if (!dynamicInitialized)
			{
				dynamicMaterialInstance = UnityEngine.Object.Instantiate(dynamicMaterial);
			}
			dynamicMaterialInstance.SetTexture("_MainTex", textureData.skinTexture);
			dynamicMaterialInstance.SetTexture("_DamageMask", textureData.dynamic.Damage);
			dynamicMaterialInstance.SetTexture("_BloodMask", textureData.dynamic.Blood);
			dynamicMaterialInstance.SetTexture("_BurnMask", textureData.dynamic.Burn);
			meshRenderer.material = dynamicMaterialInstance;
			debugDynamicDamage = textureData.dynamic.Damage;
			debugDynamicBlood = textureData.dynamic.Blood;
			debugDynamicBurn = textureData.dynamic.Burn;
			IsDynamic = (dynamicInitialized = true);
		}
	}

	public void BeginDamagePass()
	{
		RenderTexture.active = textureData.dynamic.Damage;
		BeginGlPass();
	}

	public void BeginBloodPass()
	{
		RenderTexture.active = textureData.dynamic.Blood;
		BeginGlPass();
	}

	public void EndDamagePass()
	{
		textureData.Apply();
		EndPass();
	}

	private void BeginGlPass()
	{
		renderMaterialInstance.SetPass(0);
		GL.PushMatrix();
		GL.LoadPixelMatrix(0f, textureData.Width, textureData.Height, 0f);
		StaticDebug.Instance.GlPasses++;
		GL.Begin(4);
	}

	public void EndPass()
	{
		GL.End();
		GL.PopMatrix();
		RenderTexture.active = null;
	}

	public void DrawDamage(Vector3 worldPosition, float radiusMillimeters, float depth, float irregularity = 0.3f)
	{
		StaticDebug.Instance.DamagePasses++;
		Vector3 localPoint = WorldToTextureSpaceInvertY(worldPosition);
		RenderPoint(localPoint, new Color(0f, 1f, 0f, depth), radiusMillimeters, irregularity, Mathf.Max(10, Mathf.RoundToInt(radiusMillimeters * 2f)));
	}

	public void DrawBlood(Vector3 worldPosition, float radiusMillimeters, float opacity)
	{
		StaticDebug.Instance.BloodPasses++;
		Vector3 localPoint = WorldToTextureSpaceInvertY(worldPosition);
		RenderPoint(localPoint, new Color(0.6f, 0f, 0f, opacity), radiusMillimeters, 0.2f, Mathf.Max(10, Mathf.RoundToInt(radiusMillimeters)));
	}

	public void ClearBlood(Vector3 worldPosition, float radiusMillimeters, float opacity)
	{
		StaticDebug.Instance.BloodPasses++;
		Vector3 localPoint = WorldToTextureSpaceInvertY(worldPosition);
		RenderTexture.active = textureData.dynamic.Blood;
		RenderSeparatePoint(localPoint, new Color(1f, 1f, 1f, opacity), radiusMillimeters, 0.2f, Mathf.Max(10, Mathf.RoundToInt(radiusMillimeters)));
		RenderTexture.active = null;
	}

	public void DrawBurn(Vector3 worldPosition, float radiusMillimeters, float opacity)
	{
		SetDynamic();
		Vector3 localPoint = WorldToTextureSpaceInvertY(worldPosition);
		int vertices = Mathf.RoundToInt(radiusMillimeters * 0.1f);
		Color color = new Color(0f, 0f, 0f, opacity);
		RenderTexture.active = textureData.dynamic.Burn;
		renderMaterialInstance.SetPass(0);
		GL.PushMatrix();
		GL.LoadPixelMatrix(0f, textureData.Width, textureData.Height, 0f);
		StaticDebug.Instance.GlPasses++;
		GL.Begin(4);
		RenderPoint(localPoint, color, radiusMillimeters, 0.1f, vertices);
		RenderPoint(localPoint, color, radiusMillimeters * 0.9f, 0.1f, vertices);
		RenderPoint(localPoint, color, radiusMillimeters * 0.8f, 0.1f, vertices);
		GL.End();
		GL.PopMatrix();
		RenderTexture.active = null;
	}

	public void ClearBurn(Vector3 worldPosition, float radiusMillimeters)
	{
		Vector3 localPoint = WorldToTextureSpaceInvertY(worldPosition);
		RenderTexture.active = textureData.dynamic.Burn;
		RenderSeparatePoint(localPoint, new Color(1f, 1f, 1f), radiusMillimeters, 0.1f, Mathf.RoundToInt(radiusMillimeters));
		RenderTexture.active = null;
	}

	public TraceResult FindEntryExit(Vector3 worldOrigin, Vector3 worldDirection, float threshold, ref Vector3 worldEntryPoint, ref Vector3 worldExitPoint)
	{
		Vector3 point = Vector3.zero;
		Vector3 worldOrigin2 = worldOrigin - worldDirection * 0.25f;
		if (!FindEdgePoint(worldOrigin2, worldDirection, ref point, EdgeFindMode.Closest))
		{
			return TraceResult.Failure;
		}
		Vector3 vector = base.transform.InverseTransformDirection(worldDirection);
		Vector2 vector2 = new Vector2(vector.x, vector.y);
		Vector2 texel = WorldPointToTextureSpace(point);
		Vector2 point2 = default(Vector2);
		if (PixelTrace(texel, vector2, ref point2, threshold))
		{
			worldEntryPoint = TexelToWorldPoint(point2);
			if (FindEdgePoint(worldOrigin2, worldDirection, ref point, EdgeFindMode.Furthest))
			{
				texel = WorldPointToTextureSpace(point);
				if (PixelTrace(texel, vector2 * -1f, ref point2, threshold))
				{
					worldExitPoint = TexelToWorldPoint(point2);
					return TraceResult.EntryExit;
				}
			}
			return TraceResult.Entry;
		}
		return TraceResult.Failure;
	}

	public TraceResult FindMarksmanEntryAndExit(Vector3 worldPosition, Vector3 worldDirection, ref Vector3 worldEntry, ref Vector3 worldExit)
	{
		Vector2 vector = WorldPointToTextureSpace(worldPosition);
		if (TestPixel((int)vector.x, (int)vector.y, 0f))
		{
			worldEntry = worldPosition;
			worldExit = worldPosition + worldDirection * 0.2f;
			return TraceResult.EntryExit;
		}
		return TraceResult.Failure;
	}

	private void RenderPoint(Vector3 localPoint, Color color, float radiusMillimeters, float irregularity = 0f, int vertices = 5)
	{
		float x = localPoint.x;
		float y = localPoint.y;
		float num = (float)Math.PI * 2f;
		float num2 = num / (float)vertices;
		float num3 = 0f;
		float num4 = radiusMillimeters * 0.001f;
		GL.Color(color);
		for (int i = 0; i < vertices; i++)
		{
			float num5 = num4 - num4 * irregularity * UnityEngine.Random.value;
			GL.Vertex3(x + num5 * textureData.PixelsPerMeter.x * Mathf.Sin(num3), y + num5 * textureData.PixelsPerMeter.y * Mathf.Cos(num3), 0f);
			GL.Vertex3(x + num5 * textureData.PixelsPerMeter.x * Mathf.Sin(num3 + num2), y + num5 * textureData.PixelsPerMeter.y * Mathf.Cos(num3 + num2), 0f);
			GL.Vertex3(x, y, 0f);
			num3 += num2;
		}
	}

	private void RenderSeparatePoint(Vector3 localPoint, Color color, float radiusMillimeters, float irregularity = 0f, int vertices = 5)
	{
		float x = localPoint.x;
		float y = localPoint.y;
		float num = (float)Math.PI * 2f;
		float num2 = num / (float)vertices;
		float num3 = 0f;
		float num4 = radiusMillimeters * 0.001f;
		renderMaterialInstance.SetPass(0);
		GL.PushMatrix();
		GL.LoadPixelMatrix(0f, textureData.Width, textureData.Height, 0f);
		StaticDebug.Instance.GlPasses++;
		GL.Begin(4);
		GL.Color(color);
		for (int i = 0; i < vertices; i++)
		{
			float num5 = num4 - num4 * irregularity * UnityEngine.Random.value;
			GL.Vertex3(x + num5 * textureData.PixelsPerMeter.x * Mathf.Sin(num3), y + num5 * textureData.PixelsPerMeter.y * Mathf.Cos(num3), 0f);
			GL.Vertex3(x + num5 * textureData.PixelsPerMeter.x * Mathf.Sin(num3 + num2), y + num5 * textureData.PixelsPerMeter.y * Mathf.Cos(num3 + num2), 0f);
			GL.Vertex3(x, y, 0f);
			num3 += num2;
		}
		GL.End();
		GL.PopMatrix();
	}

	public void FillTexture(RenderTexture texture, Color color)
	{
		if (!(texture == null))
		{
			RenderTexture.active = texture;
			renderMaterialInstance.SetPass(0);
			GL.PushMatrix();
			GL.LoadPixelMatrix(0f, textureData.Width, textureData.Height, 0f);
			StaticDebug.Instance.GlPasses++;
			GL.Begin(7);
			GL.Color(color);
			GL.Vertex3(0f, texture.height, 0f);
			GL.Vertex3(texture.width, texture.height, 0f);
			GL.Vertex3(texture.width, 0f, 0f);
			GL.Vertex3(0f, 0f, 0f);
			GL.End();
			GL.PopMatrix();
			RenderTexture.active = null;
		}
	}

	private bool PixelTrace(Vector2 texel, Vector2 direction, ref Vector2 point, float threshold, float stepSize = 1.4f, int maxTries = 200)
	{
		for (int i = 0; i < maxTries; i++)
		{
			if (TestPixel(Mathf.FloorToInt(texel.x), Mathf.FloorToInt(texel.y), threshold))
			{
				point = new Vector2(Mathf.FloorToInt(texel.x), Mathf.FloorToInt(texel.y));
				return true;
			}
			texel += direction * stepSize;
		}
		return false;
	}

	private bool TestPixel(int x, int y, float threshold)
	{
		return textureData.GetPixel(x, y).g <= threshold;
	}

	private Vector2 WorldPointToTextureSpace(Vector3 worldPoint)
	{
		Vector3 vector = base.transform.InverseTransformPoint(worldPoint) + Vector3.one * 0.5f;
		return new Vector2(vector.x * (float)textureData.Width, vector.y * (float)textureData.Height);
	}

	private Vector3 WorldToTextureSpaceInvertY(Vector3 worldPosition)
	{
		Vector3 a = Vector3.Scale(base.transform.InverseTransformPoint(worldPosition), new Vector3(1f, -1f, 1f)) + new Vector3(0.5f, 0.5f, 0f);
		Vector3 b = new Vector3(textureData.Width, textureData.Height, 0f);
		return Vector3.Scale(a, b);
	}

	private Vector3 TexelToWorldPoint(Vector2 texel)
	{
		return base.transform.TransformPoint(TexelToLocalPoint(texel));
	}

	private Vector3 TexelToLocalPoint(Vector2 texel)
	{
		return new Vector3(texel.x / (float)textureData.Width - 0.5f, texel.y / (float)textureData.Height - 0.5f, 0f);
	}

	private bool FindEdgePoint(Vector3 worldOrigin, Vector3 worldDirection, ref Vector3 point, EdgeFindMode mode)
	{
		Vector3 vector = base.transform.InverseTransformPoint(worldOrigin);
		Vector3 vector2 = base.transform.InverseTransformVector(worldDirection);
		Vector3 vector3 = vector + vector2 * 1000f;
		Vector2 b = new Vector2(-0.5f, 0.5f);
		Vector2 vector4 = new Vector2(0.5f, 0.5f);
		Vector2 vector5 = new Vector2(-0.5f, -0.5f);
		Vector2 b2 = new Vector2(0.5f, -0.5f);
		Vector2 zero = Vector2.zero;
		Vector2 zero2 = Vector2.zero;
		Vector2 zero3 = Vector2.zero;
		Vector2 zero4 = Vector2.zero;
		List<Vector2> list = new List<Vector2>();
		Vector2 intersection = Vector2.zero;
		if (Intersects(vector, vector3, b, vector5, ref intersection))
		{
			list.Add(intersection);
		}
		if (Intersects(vector, vector3, vector4, b2, ref intersection))
		{
			list.Add(intersection);
		}
		if (Intersects(vector, vector3, b, vector4, ref intersection))
		{
			list.Add(intersection);
		}
		if (Intersects(vector, vector3, vector5, b2, ref intersection))
		{
			list.Add(intersection);
		}
		float num = ((mode != EdgeFindMode.Closest) ? float.NegativeInfinity : float.PositiveInfinity);
		Vector2 vector6 = Vector2.zero;
		Vector2 vector7 = new Vector2(vector.x, vector.y);
		if (mode == EdgeFindMode.Closest)
		{
			foreach (Vector2 item in list)
			{
				float magnitude = (item - vector7).magnitude;
				if (magnitude < num)
				{
					num = magnitude;
					vector6 = item;
				}
			}
		}
		else
		{
			foreach (Vector2 item2 in list)
			{
				float magnitude2 = (item2 - vector7).magnitude;
				if (magnitude2 > num)
				{
					num = magnitude2;
					vector6 = item2;
				}
			}
		}
		if (vector6 != Vector2.zero)
		{
			point = base.transform.TransformPoint(vector6);
			return true;
		}
		return false;
	}

	private bool Intersects(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2, ref Vector2 intersection)
	{
		Vector2 vector = a2 - a1;
		Vector2 vector2 = b2 - b1;
		float num = vector.x * vector2.y - vector.y * vector2.x;
		if (num == 0f)
		{
			return false;
		}
		Vector2 vector3 = b1 - a1;
		float num2 = (vector3.x * vector2.y - vector3.y * vector2.x) / num;
		if (num2 < 0f || num2 > 1f)
		{
			return false;
		}
		float num3 = (vector3.x * vector.y - vector3.y * vector.x) / num;
		if (num3 < 0f || num3 > 1f)
		{
			return false;
		}
		intersection = a1 + num2 * vector;
		return true;
	}
}

namespace Crisis.Runtime.FullSource
{
using UnityEngine;
using UnityEngine.UI;

public class MarksmanBoltActionVisualization : MonoBehaviour
{
	[Header("Model")]
	public Transform bolt;

	public Transform boltSlot;

	[Tooltip("How far the bolt can rotate. In degrees.")]
	public float rotationRange = 180f;

	[Tooltip("How far the bolt can translate. In meters.")]
	public float translationRange = 0.1f;

	[Tooltip("How a round being transferred from the top magazine slot into the chamber rotates during that translation. X: Bolt's normalized translation. Y: Angle of round in degrees.")]
	public AnimationCurve roundFeedRotation;

	[Tooltip("How a round carried by the bolt from magazine into the chamber translates in Y. X: Bolt's normalized translation. Y: Round's Y normalized translation offset. The offset is scaled by the round diameter.")]
	public AnimationCurve roundFeedTranslation;

	[Tooltip("How rounds in the magazine are raised along Y when a round is removed from the magazine. X: Bolt's normalized translation. Y: Rounds' Y normalized translation offset. The offset is scaled by the round diameter.")]
	public AnimationCurve followerTranslation;

	public MarksmanRound roundPrefab;

	public Transform magazine;

	public Transform casingsContainer;

	public Transform magazineFollower;

	[Header("Ejection")]
	public float ejectorForce = 1f;

	[Header("Rendering")]
	[Tooltip("The object that contains the raw image component. Is shown and hidden when bolt manipulations occur.")]
	public GameObject visualization;

	public Camera renderCamera;

	public RawImage image;

	private Vector3 homePosition;

	private Vector3 homeRotation;

	private Vector3 boltSlotHomePosition;

	private Vector3 followerHomePosition;

	private RenderTexture texture;

	private static float roundDiameter = 0.012f;

	private static float roundFeedOffset = 0.0115f;

	private static float roundRaiseAmountOnEmptyCarrier = 0.0015f;

	private static float stackOffsetX = 0.00475f;

	private static float stackOffsetY = 0.007f;

	private static float stackOffsetFirstY = 0.011f;

	private bool feeding;

	private bool initialized;

	public int MagazineSize { get; set; }

	private void Awake()
	{
		int width = 0;
		int height = 0;
		Global.Context.GetCanvasResolution(ref width, ref height);
		float num = (float)height * 0.75f;
		int num2 = Mathf.NextPowerOfTwo((int)num);
		texture = new RenderTexture(num2, num2, 16);
		renderCamera.targetTexture = texture;
		image.texture = texture;
		MagazineSize = 5;
		homePosition = bolt.localPosition;
		homeRotation = bolt.localEulerAngles;
		boltSlotHomePosition = boltSlot.localPosition;
		followerHomePosition = magazineFollower.localPosition;
		RectTransform component = image.GetComponent<RectTransform>();
		component.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, num);
		component.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, num);
		initialized = true;
	}

	private void OnDestroy()
	{
		texture.Release();
		Object.Destroy(texture);
	}

	public void Initialize(bool hasAmmunition)
	{
		if (hasAmmunition)
		{
			ReplaceMagazine();
			InstantiateRoundInChamber();
		}
	}

	public void SetTransforms(float rotation, float translation)
	{
		Vector3 vector = Vector3.forward * translation * translationRange;
		bolt.localPosition = homePosition - vector;
		bolt.localEulerAngles = homeRotation + Vector3.forward * rotation * rotationRange;
		boltSlot.localEulerAngles = new Vector3((!feeding) ? 0f : (0f - roundFeedRotation.Evaluate(1f - translation)), 0f, 0f);
		float num = roundRaiseAmountOnEmptyCarrier * Mathf.Max(translation - 0.95f, 0f) / 0.05f;
		float y = ((!feeding) ? 0f : (roundFeedTranslation.Evaluate(1f - translation) * roundFeedOffset + num));
		boltSlot.localPosition = boltSlotHomePosition - vector - new Vector3(0f, y, 0f);
		if (feeding)
		{
			magazine.localPosition = new Vector3(0f, followerTranslation.Evaluate(1f - translation) * roundFeedOffset, 0f);
			Transform transform = ((magazine.childCount != 0) ? magazine.GetChild(0) : null);
			if (transform != null)
			{
				float num2 = Mathf.Max(translation - 0.5f, 0f) / 0.5f;
				Vector3 localPosition = transform.localPosition;
				localPosition.x = stackOffsetX * ((!(transform.localEulerAngles.z < 0.2f)) ? 1f : (-1f)) * num2;
				transform.localPosition = localPosition;
			}
			if (translation <= 0.05f)
			{
				foreach (Transform item in magazine)
				{
					Vector3 localPosition2 = item.localPosition;
					localPosition2.y += ((item.GetSiblingIndex() != 0) ? stackOffsetY : stackOffsetFirstY);
					item.localPosition = localPosition2;
				}
				feeding = false;
			}
		}
		else
		{
			float y2 = roundRaiseAmountOnEmptyCarrier * Mathf.Max(translation - 0.95f, 0f) / 0.05f;
			magazine.localPosition = new Vector3(0f, y2, 0f);
		}
		if (!feeding)
		{
			UpdateRoundTransforms();
		}
		SetFollowerPosition();
	}

	private void SetFollowerPosition()
	{
		if (initialized)
		{
			Transform transform = ((magazine.childCount >= 2) ? magazine.GetChild(magazine.childCount - 2) : null);
			float y = ((!(transform == null)) ? (magazine.InverseTransformPoint(transform.position).y - roundDiameter * 0.5f) : 0f);
			magazineFollower.localPosition = new Vector3(followerHomePosition.x, y, followerHomePosition.z);
		}
	}

	private void InstantiateRoundInChamber()
	{
		MarksmanRound marksmanRound = Object.Instantiate(roundPrefab);
		marksmanRound.name = roundPrefab.name;
		marksmanRound.transform.SetParent(boltSlot);
		marksmanRound.transform.localPosition = Vector3.zero;
		marksmanRound.transform.localRotation = bolt.localRotation;
		marksmanRound.gameObject.layer = LayerMask.NameToLayer("Render to texture");
	}

	public void ClearAndReplaceMagazine(MarksmanRound prefab)
	{
		ClearMagazine();
		for (int i = 0; i < MagazineSize; i++)
		{
			bool flag = i == 0;
			bool flag2 = i % 2 == 0;
			MarksmanRound marksmanRound = Object.Instantiate(prefab);
			marksmanRound.name = prefab.name;
			marksmanRound.transform.SetParent(magazine);
			marksmanRound.transform.localEulerAngles = new Vector3(0f, 0f, (!flag2) ? 0.25f : 0f);
			float y = ((!flag) ? (0f - stackOffsetFirstY - (float)(magazine.childCount - 2) * stackOffsetY) : 0f);
			float x = ((!flag) ? (stackOffsetX * ((!flag2) ? 1f : (-1f))) : 0f);
			marksmanRound.transform.localPosition = new Vector3(x, y, 0f);
		}
		SetFollowerPosition();
	}

	public void ClearMagazine()
	{
		for (int num = magazine.childCount - 1; num >= 0; num--)
		{
			Object.Destroy(magazine.GetChild(num).gameObject);
		}
	}

	public void ReplaceMagazine()
	{
		ReplaceMagazine(roundPrefab);
	}

	public void ReplaceMagazine(MarksmanRound prefab)
	{
		while (magazine.childCount < MagazineSize)
		{
			bool flag = magazine.childCount == 0;
			bool flag2 = magazine.childCount % 2 == 0;
			MarksmanRound marksmanRound = Object.Instantiate(prefab);
			marksmanRound.name = prefab.name;
			marksmanRound.transform.SetParent(magazine);
			marksmanRound.transform.localEulerAngles = new Vector3(0f, 0f, (!flag2) ? 0.25f : 0f);
			float y = ((!flag) ? (0f - stackOffsetFirstY - (float)(magazine.childCount - 2) * stackOffsetY) : 0f);
			float x = ((!flag) ? (stackOffsetX * ((!flag2) ? 1f : (-1f))) : 0f);
			marksmanRound.transform.localPosition = new Vector3(x, y, 0f);
		}
		SetFollowerPosition();
	}

	public void UpdateRoundTransforms()
	{
		int num = 0;
		foreach (Transform item in magazine)
		{
			bool flag = num == 0;
			bool flag2 = num % 2 == 0;
			MarksmanRound component = item.GetComponent<MarksmanRound>();
			component.transform.localEulerAngles = new Vector3(0f, 0f, (!flag2) ? 0.25f : 0f);
			float x = ((!flag) ? (stackOffsetX * ((!flag2) ? 1f : (-1f))) : 0f);
			float y = ((!flag) ? (0f - stackOffsetFirstY - (float)(num - 2) * stackOffsetY) : 0f);
			component.transform.localPosition = new Vector3(x, y, 0f);
			num++;
		}
	}

	public void BoltPickupRound()
	{
		if (magazine.childCount != 0)
		{
			feeding = true;
			Transform child = magazine.GetChild(0);
			float z = child.transform.position.z - boltSlot.position.z;
			child.transform.SetParent(boltSlot);
			child.transform.localPosition = new Vector3(0f, 0f, z);
			boltSlot.transform.localPosition = new Vector3(0f, roundFeedTranslation.Evaluate(0f) - roundRaiseAmountOnEmptyCarrier, 0f);
		}
	}

	public void DetonateRound()
	{
		if (boltSlot.childCount != 0)
		{
			boltSlot.GetChild(0).GetComponent<MarksmanRound>().Detonate();
		}
	}

	public Vector3 ProjectilePosition()
	{
		if (boltSlot.childCount == 0)
		{
			return base.transform.position;
		}
		return boltSlot.GetChild(0).GetComponent<MarksmanRound>().projectile.transform.position;
	}

	public void BoltDisposeRound(float forceScale)
	{
		if (boltSlot.childCount != 0)
		{
			MarksmanRound component = boltSlot.GetChild(0).GetComponent<MarksmanRound>();
			component.transform.SetParent(casingsContainer);
			component.transform.position += Vector3.up * 0.02f;
			component.Eject(ejectorForce * forceScale, bolt, base.transform.TransformVector(new Vector3(Random.Range(0.2f, 0.75f), Random.Range(0.75f, 1f), Random.Range(-1f, -0.5f))));
		}
	}

	public void ApplyCasingsForce(Vector3 force)
	{
		foreach (Transform item in casingsContainer)
		{
			item.GetComponent<Rigidbody>().AddForce(force);
		}
	}

	public void Display(bool show)
	{
		visualization.SetActive(show);
	}

	public void Render()
	{
		renderCamera.Render();
		if (!visualization.activeInHierarchy)
		{
			Display(true);
		}
	}
}

}

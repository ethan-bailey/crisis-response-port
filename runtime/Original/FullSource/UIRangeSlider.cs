namespace Crisis.Runtime.FullSource
{
using UnityEngine;
using UnityEngine.EventSystems;

public class UIRangeSlider : MonoBehaviour, IPointerDownHandler, IDragHandler, IEventSystemHandler
{
	[Header("Operation")]
	public float minValue;

	public float maxValue = 1f;

	public UnityEventFloatFloat onValueChanged = new UnityEventFloatFloat();

	[Header("References")]
	public Transform lowHandle;

	public Transform highHandle;

	public Transform slideArea;

	public RectTransform fillBar;

	public RectTransform fillArea;

	private RectTransform draggingPlane;

	private Transform activeHandle;

	private bool isInteractable = true;

	public float LowValue { get; private set; }

	public float HighValue { get; private set; }

	public float NormalizedLowValue { get; private set; }

	public float NormalizedHighValue { get; private set; }

	public bool interactable
	{
		get
		{
			return isInteractable;
		}
		set
		{
			isInteractable = value;
		}
	}

	public void SetValues(float low, float high)
	{
		SetLowValue(low);
		SetHighValue(high);
		UpdateValues();
		SetFillTransform();
	}

	private void SetLowValue(float value)
	{
		float lowPosition = (value - minValue) / (maxValue - minValue);
		SetLowPosition(lowPosition);
	}

	private void SetHighValue(float value)
	{
		float highPosition = (value - minValue) / (maxValue - minValue);
		SetHighPosition(highPosition);
	}

	private void SetLowPosition(float normalizedValue)
	{
		Vector3[] array = new Vector3[4];
		slideArea.GetComponent<RectTransform>().GetWorldCorners(array);
		float x = array[0].x;
		float num = array[2].x - x;
		lowHandle.position = new Vector3(x + normalizedValue * num, lowHandle.position.y, lowHandle.position.z);
		SetFillTransform();
	}

	private void SetHighPosition(float normalizedValue)
	{
		Vector3[] array = new Vector3[4];
		slideArea.GetComponent<RectTransform>().GetWorldCorners(array);
		float x = array[0].x;
		float num = array[2].x - x;
		highHandle.position = new Vector3(x + normalizedValue * num, highHandle.position.y, highHandle.position.z);
		SetFillTransform();
	}

	public void OnPointerDown(PointerEventData data)
	{
		if (isInteractable)
		{
			IntializeDrag(data);
			SetHandlePosition(data);
		}
	}

	public void OnDrag(PointerEventData data)
	{
		if (isInteractable)
		{
			SetHandlePosition(data);
		}
	}

	private void SetHandlePosition(PointerEventData data)
	{
		Vector3 worldPoint;
		if (RectTransformUtility.ScreenPointToWorldPointInRectangle(draggingPlane, data.position, data.pressEventCamera, out worldPoint))
		{
			Vector3[] array = new Vector3[4];
			slideArea.GetComponent<RectTransform>().GetWorldCorners(array);
			float x = Mathf.Clamp(worldPoint.x, array[0].x, array[2].x);
			activeHandle.position = new Vector3(x, activeHandle.position.y, activeHandle.position.z);
			activeHandle.rotation = draggingPlane.rotation;
			AdjustOtherHandle();
			SetFillTransform();
			UpdateValues();
		}
	}

	private void IntializeDrag(PointerEventData data)
	{
		Canvas canvas = FindInParents<Canvas>(base.gameObject);
		if (!(canvas == null))
		{
			draggingPlane = canvas.transform as RectTransform;
			activeHandle = GetClosestHandle(data);
		}
	}

	private void AdjustOtherHandle()
	{
		if (activeHandle == null)
		{
			return;
		}
		if (activeHandle == lowHandle)
		{
			if (activeHandle.position.x > highHandle.position.x)
			{
				highHandle.position = new Vector3(activeHandle.position.x, highHandle.position.y, highHandle.position.z);
			}
		}
		else if (activeHandle.position.x < lowHandle.position.x)
		{
			lowHandle.position = new Vector3(activeHandle.position.x, lowHandle.position.y, lowHandle.position.z);
		}
	}

	private void SetFillTransform()
	{
		fillBar.position = fillArea.TransformPoint(lowHandle.localPosition);
		float size = Mathf.Abs(fillArea.rect.width) * (NormalizedHighValue - NormalizedLowValue);
		fillBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
	}

	private void UpdateValues()
	{
		Vector3[] array = new Vector3[4];
		slideArea.GetComponent<RectTransform>().GetWorldCorners(array);
		float x = array[0].x;
		float num = array[2].x - x;
		bool flag = false;
		float num2 = (lowHandle.position.x - x) / num;
		flag = flag || num2 != NormalizedLowValue;
		NormalizedLowValue = num2;
		float num3 = (highHandle.position.x - x) / num;
		flag = flag || num3 != NormalizedHighValue;
		NormalizedHighValue = num3;
		float num4 = minValue + NormalizedLowValue * (maxValue - minValue);
		flag = flag || num4 != LowValue;
		LowValue = num4;
		float num5 = minValue + NormalizedHighValue * (maxValue - minValue);
		flag = flag || num5 != HighValue;
		HighValue = num5;
		if (flag)
		{
			onValueChanged.Invoke(LowValue, HighValue);
		}
	}

	private Transform GetClosestHandle(PointerEventData data)
	{
		Vector3 worldPoint;
		if (RectTransformUtility.ScreenPointToWorldPointInRectangle(draggingPlane, data.position, data.pressEventCamera, out worldPoint))
		{
			float x = worldPoint.x;
			float x2 = lowHandle.position.x;
			float x3 = highHandle.position.x;
			float num = Mathf.Abs(x - x2);
			float num2 = Mathf.Abs(x - x3);
			if (x >= x3 && x >= x2)
			{
				return highHandle;
			}
			if (x < x2 && x < x3)
			{
				return lowHandle;
			}
			if (num <= num2)
			{
				return lowHandle;
			}
			return highHandle;
		}
		return lowHandle;
	}

	public static T FindInParents<T>(GameObject go) where T : Component
	{
		if (go == null)
		{
			return (T)null;
		}
		T component = go.GetComponent<T>();
		if (component != null)
		{
			return component;
		}
		Transform parent = go.transform.parent;
		while (parent != null && component == null)
		{
			component = parent.gameObject.GetComponent<T>();
			parent = parent.parent;
		}
		return component;
	}

	public SerializableRangeSlider Serialize()
	{
		return new SerializableRangeSlider(LowValue, HighValue);
	}

	public void Deserialize(SerializableRangeSlider data)
	{
		SetValues(data.low, data.high);
	}

	public void Deserialize(float low, float high)
	{
		SetValues(low, high);
	}
}

}

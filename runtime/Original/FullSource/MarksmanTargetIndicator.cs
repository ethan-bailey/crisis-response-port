namespace Crisis.Runtime.FullSource
{
using UnityEngine;

public class MarksmanTargetIndicator : MonoBehaviour
{
	public enum Status
	{
		Ready = 0,
		NotReady = 1,
		Occluded = 2
	}

	public Sprite readySprite;

	public Sprite notReadySprite;

	public Sprite occludedSprite;

	private SpriteRenderer spriteRenderer;

	private void Awake()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
	}

	public void SetStatus(Status status, float position)
	{
		switch (status)
		{
		case Status.Ready:
			spriteRenderer.sprite = readySprite;
			break;
		case Status.NotReady:
			spriteRenderer.sprite = notReadySprite;
			break;
		case Status.Occluded:
			spriteRenderer.sprite = occludedSprite;
			break;
		}
		base.transform.position = new Vector3(position, 2.15f, -1.5f);
	}

	public void SetVisible(bool visible)
	{
		base.gameObject.SetActive(visible);
	}
}

}

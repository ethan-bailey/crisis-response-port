namespace Crisis.Runtime.FullSource
{
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;
using UnityEngine;
using UnityEngine.UI;

public class UIVocalizationText : MonoBehaviour
{
	public Text text;

	public GameObject textLayout;

	[Tooltip("Text offset in the character's head local space.")]
	public Vector2 offset;

	private CharacterBody owner;

	private Camera camera;

	private void Update()
	{
		Vector3 position = owner.Head.transform.position + new Vector3(Mathf.Min(owner.VisibleRange, offset.x) * owner.Direction, offset.y, 0f);
		base.transform.position = camera.WorldToScreenPoint(position);
	}

	public void Initialize(CharacterBody body, Transform parent)
	{
		owner = body;
		base.transform.SetParent(parent);
		base.transform.localScale = Vector3.one;
		camera = Global.Context.Mission.InteriorMode.interiorCamera.interiorCamera;
		base.name = body.name;
		textLayout.SetActive(false);
	}

	public void SetText(string text, bool capitalize = false)
	{
		this.text.text = ((!capitalize) ? text : text.ToUpper());
		textLayout.SetActive(true);
	}

	public void Hide()
	{
		textLayout.SetActive(false);
	}
}

}

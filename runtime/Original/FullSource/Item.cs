namespace Crisis.Runtime.FullSource
{
using UnityEngine;

public class Item : MonoBehaviour
{
	[Tooltip("Image to use for this in UI.")]
	public Sprite sprite;

	[Tooltip("Number of usages for one slot of this item.")]
	public int quantity = 1;

	public ItemType type;

	[TextArea(5, 10)]
	public string description;
}

}

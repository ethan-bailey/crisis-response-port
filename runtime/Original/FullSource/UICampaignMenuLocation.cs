namespace Crisis.Runtime.FullSource
{
using UnityEngine;

public class UICampaignMenuLocation : MonoBehaviour
{
	public string title = "Untitled location";

	public Sprite background;

	public UICampaignMenuLocationTemplate template;

	public virtual void OnEnable()
	{
		template.SetLocation(this);
	}
}

}

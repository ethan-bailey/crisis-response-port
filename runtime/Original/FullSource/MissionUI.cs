namespace Crisis.Runtime.FullSource
{
using UnityEngine;

public class MissionUI : MonoBehaviour
{
	public const float WeaponIndicationTime = 2f;

	public GameObject missionIntro;

	public UIMissionEndOverlay endOverlay;

	public MissionEndMenu missionEndMenu;

	public UIMissionStatus missionStatusIndicator;

	public UIInteriorModeTextIndicator indicatorUpperLeft;

	public UIInteriorModeTextIndicator indicatorUpperRight;

	public UIInteriorModeTextIndicator indicatorLowerLeft;

	public UIInteriorModeTextIndicator indicatorLowerRight;

	public UIHitAnnouncements hitAnnouncements;

	public UIResourceCounter resourceCounter;

	public UIPostMortem postMortem;

	[Header("Prefab references")]
	public PauseMenu pauseMenuPrefab;

	public PauseMenu PauseMenu { get; private set; }

	private void Awake()
	{
		if (PauseMenu == null)
		{
			PauseMenu = Object.Instantiate(pauseMenuPrefab);
			PauseMenu.transform.SetParent(base.transform, false);
			PauseMenu.name = pauseMenuPrefab.name;
			PauseMenu.gameObject.SetActive(false);
		}
	}

	public void PlayIntro()
	{
		missionIntro.SetActive(true);
	}

	public bool ToggleResourceCountersVisibility()
	{
		if (!Version.Instance.Staging)
		{
			return false;
		}
		resourceCounter.gameObject.SetActive(!resourceCounter.gameObject.activeInHierarchy);
		return resourceCounter.gameObject.activeInHierarchy;
	}
}

}

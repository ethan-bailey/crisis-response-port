namespace Crisis.Runtime.FullSource
{
using UnityEngine;

public class InteriorModeUI : MonoBehaviour
{
	public UIInteractionMenu interactionMenuPrefab;

	public UIVocalizationText vocalizationTextPrefab;

	public WeaponPointer weaponPointer;

	public WeaponReticle worldSpaceReticlePrefab;

	public UIFireModeIndicator fireModeIndicator;

	public UIMagazineIndicator magazineIndicator;

	public UIInteriorModeInteractionIndicator interactionIndicator;

	public UIPhysiologyIndicator physiologyIndicator;

	public UIResumeControlBanner resumeControlBanner;

	public UIHotkeysIndicator hotkeysIndicator;

	public UIMarksmanIndicator marksmanIndicator;

	public Transform censorContainer;

	private UIInteractionMenu interactionMenu;

	private Transform vocalizationsContainer;

	public UIInteractionMenu InteractionMenu
	{
		get
		{
			return interactionMenu;
		}
	}

	public Canvas Canvas { get; private set; }

	public bool ElementsHidden { get; private set; }

	private void Awake()
	{
		Canvas = GetComponent<Canvas>();
		if (interactionMenu == null)
		{
			interactionMenu = Object.Instantiate(interactionMenuPrefab);
			interactionMenu.transform.SetParent(base.transform);
		}
		WeaponReticle weaponReticle = Object.Instantiate(worldSpaceReticlePrefab);
		weaponReticle.name = worldSpaceReticlePrefab.name;
		GameObject gameObject = new GameObject("Vocalizations");
		vocalizationsContainer = gameObject.transform;
		vocalizationsContainer.SetParent(base.transform);
		vocalizationsContainer.localScale = Vector3.one;
		vocalizationsContainer.localPosition = Vector3.zero;
	}

	public void HideElements(bool hide)
	{
		ElementsHidden = hide;
		fireModeIndicator.gameObject.SetActive(!ElementsHidden);
		magazineIndicator.gameObject.SetActive(!ElementsHidden);
		interactionIndicator.gameObject.SetActive(!ElementsHidden);
		physiologyIndicator.gameObject.SetActive(!ElementsHidden);
		resumeControlBanner.gameObject.SetActive(!ElementsHidden);
		hotkeysIndicator.gameObject.SetActive(!ElementsHidden);
		marksmanIndicator.gameObject.SetActive(!ElementsHidden);
	}

	public UIVocalizationText CreateCharacterVocalizationUI(CharacterBody body)
	{
		UIVocalizationText uIVocalizationText = Object.Instantiate(vocalizationTextPrefab);
		uIVocalizationText.Initialize(body, vocalizationsContainer);
		return uIVocalizationText;
	}
}

}

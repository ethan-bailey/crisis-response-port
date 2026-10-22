namespace Crisis.Runtime.FullSource
{
using UnityEngine;
using UnityEngine.UI;

public class UINegotiationOption : MonoBehaviour
{
	public Text text;

	public Button button;

	private CharacterDialog dialog;

	private UINegotiationDialog dialogUI;

	private SerializableDialogOption option;

	public void Initialize(CharacterDialog dialog, SerializableDialogOption option, UINegotiationDialog dialogUI)
	{
		text.text = option.phrase;
		this.dialog = dialog;
		this.dialogUI = dialogUI;
		this.option = new SerializableDialogOption(option);
		button.onClick.AddListener(Activate);
	}

	private void Activate()
	{
		dialog.ExecuteOption(ref option);
		dialogUI.RefreshUI();
	}
}

}

namespace Crisis.Runtime.FullSource
{
using UnityEngine;
using UnityEngine.UI;

public class UIPhysiologyVisualizer : MonoBehaviour
{
	[Header("Physiology")]
	public bool physiologyEnabled;

	public GameObject physiologyGroup;

	public Slider bloodPressure;

	public Slider diastolicPressure;

	public Slider bloodVolume;

	public Slider bloodOxygen;

	public Slider lungsOxygen;

	public Slider respirationCycle;

	public Slider workload;

	public Slider shock;

	public Slider concussion;

	public Slider brainTrauma;

	public GameObject bloodPressureRow;

	public GameObject distolicPressureRow;

	public GameObject bloodVolumeRow;

	public GameObject bloodOxygenRow;

	public GameObject lungsOxygenRow;

	public GameObject respirationCycleRow;

	public GameObject workloadRow;

	public GameObject shockRow;

	public GameObject concussionRow;

	public GameObject brainTraumaRow;

	[Header("Psychology")]
	public bool psychologyEnabled;

	public GameObject psychologyGroup;

	public Slider moraleSlider;

	public Slider aggressionSlider;

	public Slider fearSlider;

	private RectTransform canvasTransform;

	private int visibleFields;

	private void Start()
	{
		physiologyGroup.SetActive(physiologyEnabled);
		psychologyGroup.SetActive(psychologyEnabled);
	}

	public void SetVisibleFields(int fields)
	{
		visibleFields = fields;
		canvasTransform = base.transform.GetChild(0).GetComponent<RectTransform>();
		if (physiologyEnabled)
		{
			workloadRow.SetActive(fields >= 1);
			bloodOxygenRow.SetActive(fields >= 2);
			bloodPressureRow.SetActive(fields >= 3);
			respirationCycleRow.SetActive(fields >= 4);
			lungsOxygenRow.SetActive(fields >= 4);
			distolicPressureRow.SetActive(fields >= 5);
			bloodVolumeRow.SetActive(fields >= 5);
		}
		else
		{
			visibleFields = 3;
		}
	}

	public void SetCanvasHeight()
	{
		int num = 1;
		switch (visibleFields)
		{
		case 1:
			num = 1;
			break;
		case 2:
			num = 2;
			break;
		case 3:
			num = 3;
			break;
		case 4:
			num = 5;
			break;
		case 5:
			num = 7;
			break;
		}
		if (shockRow.activeInHierarchy)
		{
			num++;
		}
		if (concussionRow.activeInHierarchy)
		{
			num++;
		}
		if (brainTraumaRow.activeInHierarchy)
		{
			num++;
		}
		canvasTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (float)num * 0.075f);
	}
}

}

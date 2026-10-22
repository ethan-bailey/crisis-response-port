namespace Crisis.Runtime.FullSource
{
using UnityEngine;

public class CharacterPhysiologyVisualizer : MonoBehaviour
{
	public GameObject visualizerPrefab;

	private GameObject visualizer;

	private CharacterBody body;

	private Physiology physiology;

	private OccupantFear fear;

	private OccupantMorale morale;

	private UIPhysiologyVisualizer ui;

	private Operators operators;

	private bool visible;

	private int visibleFields = 5;

	private void Awake()
	{
		body = GetComponent<CharacterBody>();
		physiology = body.Physiology;
		fear = body.GetComponent<OccupantFear>();
		morale = body.GetComponent<OccupantMorale>();
	}

	private void Start()
	{
		operators = Global.Context.Mission.Operators;
	}

	public void ToggleVisibility()
	{
		Show(!visible);
	}

	public void Show(bool show)
	{
		if (show)
		{
			if (visualizer == null)
			{
				visualizer = Object.Instantiate(visualizerPrefab);
			}
			ui = visualizer.GetComponent<UIPhysiologyVisualizer>();
			ui.brainTraumaRow.SetActive(false);
			ui.SetVisibleFields(visibleFields);
		}
		else
		{
			Object.Destroy(visualizer);
		}
		bool flag = (base.enabled = show);
		visible = flag;
	}

	private void Update()
	{
		bool flag = body.IsPlayer || (operators != null && operators.PlayerController != null && operators.PlayerController.Body.CanSeePosition(body.Position));
		ui.gameObject.SetActive(flag);
		if (!flag)
		{
			return;
		}
		visualizer.transform.position = new Vector3(body.transform.position.x + 0.5f * body.Direction, 0f, -1.6f);
		if (ui.physiologyEnabled)
		{
			ui.bloodPressure.value = physiology.BloodPressure;
			ui.diastolicPressure.value = physiology.DiastolicPressure;
			ui.bloodVolume.value = physiology.BloodVolumeNormalized;
			ui.bloodOxygen.value = physiology.BloodOxygen;
			ui.lungsOxygen.value = physiology.LungsOxygenNormalized;
			ui.respirationCycle.value = physiology.Respiration;
			ui.workload.value = physiology.WorkIntensity;
			ui.shock.value = physiology.ShockNormalized;
			ui.concussion.value = physiology.Concussion;
			ui.brainTrauma.value = physiology.BrainTraumaNormalized;
			ui.shockRow.SetActive(physiology.ShockNormalized > 0f);
			ui.concussionRow.SetActive(physiology.Concussion > 0f);
			if (!ui.brainTraumaRow.activeInHierarchy && physiology.BrainTraumaNormalized > 0f)
			{
				ui.brainTraumaRow.SetActive(true);
			}
		}
		else
		{
			ui.aggressionSlider.value = Mathf.Clamp01(body.Blackboard.aggression);
			if (fear != null)
			{
				ui.fearSlider.value = Mathf.Clamp01(fear.Value);
			}
			if (morale != null)
			{
				ui.moraleSlider.value = Mathf.Clamp01(morale.Value);
			}
		}
		ui.SetCanvasHeight();
	}

	public void AddField()
	{
		visibleFields = Mathf.Clamp(visibleFields + 1, 1, 5);
		ui.SetVisibleFields(visibleFields);
	}

	public void RemoveField()
	{
		visibleFields = Mathf.Clamp(visibleFields - 1, 1, 5);
		ui.SetVisibleFields(visibleFields);
	}
}

}

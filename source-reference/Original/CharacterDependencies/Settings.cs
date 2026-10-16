using UnityEngine;

public class Settings : MonoBehaviour
{
	private SerializableGeneralSettings general = new SerializableGeneralSettings();

	private InputMapping controls = new InputMapping();

	private SerializablePerformanceSettings performance = new SerializablePerformanceSettings();

	private SerializableGraphicsSettings graphics = new SerializableGraphicsSettings();

	private SerializableAudioSettings audio = new SerializableAudioSettings();

	private SerializableTimeSettings time = new SerializableTimeSettings();

	private SerializableExtraSettings extras = new SerializableExtraSettings();

	private SerializableOccupantSettings occupants = new SerializableOccupantSettings();

	private SerializableLocationSettings location = new SerializableLocationSettings();

	public SerializableGeneralSettings General
	{
		get
		{
			return general;
		}
		set
		{
			general.Overwrite(value);
			general.Save();
		}
	}

	public SerializableAudioSettings Audio
	{
		get
		{
			return audio;
		}
		set
		{
			audio.Overwrite(value);
			audio.Save();
		}
	}

	public InputMapping Controls
	{
		get
		{
			return controls;
		}
	}

	public SerializablePerformanceSettings Performance
	{
		get
		{
			return performance;
		}
		set
		{
			performance.Overwrite(value);
			performance.Save();
		}
	}

	public SerializablePerformanceSettings BenchmarkPerformance
	{
		get
		{
			return performance;
		}
		set
		{
			performance.Overwrite(value);
			performance.InvokeAll();
		}
	}

	public SerializableGraphicsSettings Graphics
	{
		get
		{
			return graphics;
		}
		set
		{
			graphics.Overwrite(value);
			graphics.Save();
		}
	}

	public SerializableTimeSettings Time
	{
		get
		{
			return time;
		}
		set
		{
			time.Overwrite(value);
			time.Save();
		}
	}

	public SerializableExtraSettings Extras
	{
		get
		{
			return extras;
		}
		set
		{
			extras.Overwrite(value);
			extras.Save();
		}
	}

	public SerializableOccupantSettings Occupants
	{
		get
		{
			return occupants;
		}
		set
		{
			occupants.Overwrite(value);
			occupants.Save();
		}
	}

	public SerializableLocationSettings Location
	{
		get
		{
			return location;
		}
		set
		{
			location.Overwrite(value);
			location.Save();
		}
	}

	public void ClearMissionListeners()
	{
		General.ClearMissionListeners();
		Performance.ClearMissionListeners();
		Graphics.ClearMissionListeners();
		Extras.ClearMissionListeners();
	}

	public void Load()
	{
		general.Load();
		controls.Load();
		performance.Load();
		graphics.Load();
		audio.Load();
		time.Load();
		extras.Load();
		occupants.Load();
		location.Load();
	}
}

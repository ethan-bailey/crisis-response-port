namespace Crisis.Runtime.FullSource
{
using Crisis.Runtime.Weapons;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;
using System;
using System.IO;
using FullSerializer;
using UnityEngine;
using UnityEngine.Events;

[fsObject("0_0_9_4792", new Type[] { typeof(SerializableGraphicsSettings_0_0_8_005) }, MemberSerialization = fsMemberSerialization.OptIn)]
public class SerializableGraphicsSettings
{
	public enum CameraFieldOfViewMode
	{
		Dynamic = 0,
		Static = 1
	}

	public class GraphicsSettingsChangedEvent : UnityEvent<SerializableGraphicsSettings>
	{
	}

	[fsProperty]
	public bool showFrameRate;

	[fsProperty]
	public bool useDynamicFieldOfView;

	[fsProperty]
	public bool applyCensoring;

	[fsProperty]
	public bool vsync = true;

	[fsProperty]
	public float dynamicFieldOfViewMin = 20f;

	[fsProperty]
	public float dynamicFieldOfViewMax = 35f;

	[fsProperty]
	public float staticFieldOfView = 20f;

	[fsProperty]
	public float ambientLightScale = 1f;

	public GraphicsSettingsChangedEvent onChanged = new GraphicsSettingsChangedEvent();

	public UnityEventIntInt onChangedResolution = new UnityEventIntInt();

	public UnityEventFloat onAmbientLightChanged = new UnityEventFloat();

	public UnityEventBool onCensoringChanged = new UnityEventBool();

	public UnityEventBool onUseDynamicFovChanged = new UnityEventBool();

	public UnityEventFloat onStaticFieldOfViewChanged = new UnityEventFloat();

	public UnityEventFloatFloat onDynamicFieldOfViewChanged = new UnityEventFloatFloat();

	public const string Filename = "graphics.dat";

	public int VsyncCount
	{
		get
		{
			return vsync ? 1 : 0;
		}
	}

	public SerializableGraphicsSettings(SerializableGraphicsSettings_0_0_8_005 data)
	{
		vsync = data.vsync > 0;
		showFrameRate = data.showFrameRate;
		useDynamicFieldOfView = data.cameraFieldOfViewMode == SerializableGraphicsSettings_0_0_8_005.CameraFieldOfViewMode.Dynamic;
		dynamicFieldOfViewMin = data.dynamicFieldOfViewMin;
		dynamicFieldOfViewMax = data.dynamicFieldOfViewMax;
		staticFieldOfView = data.staticFieldOfView;
		applyCensoring = false;
		ambientLightScale = data.ambientLightScale;
	}

	public SerializableGraphicsSettings()
	{
	}

	public bool Different(SerializableGraphicsSettings other)
	{
		bool flag = false;
		flag |= vsync != other.vsync;
		flag |= showFrameRate != other.showFrameRate;
		flag |= useDynamicFieldOfView != other.useDynamicFieldOfView;
		flag |= dynamicFieldOfViewMin != other.dynamicFieldOfViewMin;
		flag |= dynamicFieldOfViewMax != other.dynamicFieldOfViewMax;
		flag |= staticFieldOfView != other.staticFieldOfView;
		flag |= applyCensoring != other.applyCensoring;
		return flag | (ambientLightScale != other.ambientLightScale);
	}

	public void Overwrite(SerializableGraphicsSettings other)
	{
		vsync = other.vsync;
		showFrameRate = other.showFrameRate;
		useDynamicFieldOfView = other.useDynamicFieldOfView;
		dynamicFieldOfViewMin = other.dynamicFieldOfViewMin;
		dynamicFieldOfViewMax = other.dynamicFieldOfViewMax;
		staticFieldOfView = other.staticFieldOfView;
		applyCensoring = other.applyCensoring;
		ambientLightScale = other.ambientLightScale;
	}

	public SerializableGraphicsSettings Copy()
	{
		SerializableGraphicsSettings serializableGraphicsSettings = new SerializableGraphicsSettings();
		serializableGraphicsSettings.Overwrite(this);
		return serializableGraphicsSettings;
	}

	public void Save()
	{
		if (!Serializer.SerializeToDisk(this, typeof(SerializableGraphicsSettings), Application.persistentDataPath + "/graphics.dat"))
		{
			Debug.LogWarning("Failed to save graphics settings.");
		}
		onChanged.Invoke(this);
	}

	public void Load()
	{
		SerializableGraphicsSettings serializableGraphicsSettings = null;
		try
		{
			serializableGraphicsSettings = (SerializableGraphicsSettings)Serializer.DeserializeFromDisk(typeof(SerializableGraphicsSettings), Application.persistentDataPath + "/graphics.dat");
		}
		catch (IOException)
		{
		}
		catch (Exception)
		{
			Global.Context.ClearSettingsFile("graphics.dat", "video", false);
			Global.Context.MessageBox.ShowOK("Failed to load video settings.\nDefaults have been restored.");
		}
		if (serializableGraphicsSettings == null)
		{
			serializableGraphicsSettings = new SerializableGraphicsSettings();
		}
		Overwrite(serializableGraphicsSettings);
	}

	public void Apply(Resolution resolution, bool fullscreen)
	{
		Debug.Log("Applying graphics settings.");
		bool flag = fullscreen != Screen.fullScreen;
		bool flag2 = resolution.width != Screen.currentResolution.width || resolution.height != Screen.currentResolution.height || resolution.refreshRate != Screen.currentResolution.refreshRate;
		if (flag)
		{
			Debug.LogFormat("Windowed mode will change to: {0}", (!fullscreen) ? "On" : "Off");
		}
		if (flag2 || flag)
		{
			Debug.Log("Changing resolution...");
			Screen.SetResolution(resolution.width, resolution.height, fullscreen, resolution.refreshRate);
			ApplyVsyncCount();
			Debug.LogFormat("Changed resolution to: {0} x {1} @ {2} Hz", resolution.width, resolution.height, resolution.refreshRate);
		}
		if (flag || flag2)
		{
			onChangedResolution.Invoke(resolution.width, resolution.height);
		}
		Debug.Log("Done applying graphics settings.");
	}

	public void ApplyVsyncCount()
	{
		QualitySettings.vSyncCount = VsyncCount;
	}

	public void ClearMissionListeners()
	{
		onAmbientLightChanged.RemoveAllListeners();
		onCensoringChanged.RemoveAllListeners();
		onDynamicFieldOfViewChanged.RemoveAllListeners();
		onUseDynamicFovChanged.RemoveAllListeners();
		onStaticFieldOfViewChanged.RemoveAllListeners();
	}
}

}

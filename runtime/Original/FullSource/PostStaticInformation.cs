namespace Crisis.Runtime.FullSource
{
using UnityEngine;
using UnityEngine.Events;

public class PostStaticInformation : HTTPPost
{
	public PostStaticInformation(string guid, string systemLanguage, UnityAction<HTTPRequest> callback = null)
		: base(callback)
	{
		Append("3");
		Append(guid);
		Append(systemLanguage);
		Append(SystemInfo.deviceModel);
		Append(SystemInfo.graphicsDeviceName);
		Append("{0}", (int)SystemInfo.graphicsDeviceType);
		Append(SystemInfo.graphicsDeviceVendor);
		Append(SystemInfo.graphicsDeviceVersion);
		Append("{0}", SystemInfo.graphicsMemorySize);
		Append("{0}", SystemInfo.graphicsShaderLevel);
		Append(SystemInfo.operatingSystem);
		Append("{0}", SystemInfo.processorCount);
		Append("{0}", SystemInfo.processorFrequency);
		Append(SystemInfo.processorType);
		Append("{0}", SystemInfo.systemMemorySize);
	}
}

}

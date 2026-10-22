namespace Crisis.Runtime.FullSource
{
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine.Events;

public class HTTPPost : HTTPRequest
{
	[Serializable]
	public class FormField
	{
		public string name;

		public string value;

		public FormField(string name, string value)
		{
			this.name = name;
			this.value = value;
		}
	}

	public const string LiveHeaderKey = "3brF2xhpdA9qSzw5TGIL6PlNK0Qg8XOs";

	public const string StagingHeaderKey = "vrfhlu2cEkQHV3Z0qDJIKz8ewWmjogdM";

	public const string Delimiter = "|";

	private List<FormField> fields = new List<FormField>();

	private string packetData = string.Empty;

	public HTTPPost(UnityAction<HTTPRequest> onFinish = null, UnityAction<HTTPRequest> onTimeOut = null)
		: base(onFinish, onTimeOut)
	{
		type = Type.POST;
	}

	protected override void ThreadFunction()
	{
		if (string.IsNullOrEmpty(packetData))
		{
			exception = true;
			error = "Sending empty package is aborted.";
			return;
		}
		WebRequest webRequest = WebRequest.Create(url);
		webRequest.Method = "POST";
		webRequest.ContentType = "application/x-www-form-urlencoded";
		webRequest.Headers.Add("KEY", (!Version.Instance.Staging) ? "3brF2xhpdA9qSzw5TGIL6PlNK0Qg8XOs" : "vrfhlu2cEkQHV3Z0qDJIKz8ewWmjogdM");
		byte[] bytes = Encoding.ASCII.GetBytes("data=" + Encryption.Encrypt(packetData));
		webRequest.ContentLength = bytes.Length;
		using (Stream stream = webRequest.GetRequestStream())
		{
			stream.Write(bytes, 0, bytes.Length);
		}
		using (WebResponse webResponse = webRequest.GetResponse())
		{
			using (Stream stream2 = webResponse.GetResponseStream())
			{
				using (StreamReader streamReader = new StreamReader(stream2))
				{
					responseData = streamReader.ReadToEnd();
				}
			}
		}
		if (!string.IsNullOrEmpty(responseData))
		{
			responseArray = responseData.Split(HTTPRequest.Delimeter);
		}
		else
		{
			exception = true;
			error = "Null or empty response.";
		}
	}

	protected void Append(string data)
	{
		packetData += string.Format("{0}{1}", (packetData.Length != 0) ? "|" : string.Empty, data);
	}

	protected void Append(string format, params object[] args)
	{
		string arg = string.Format(format, args);
		packetData += string.Format("{0}{1}", (packetData.Length != 0) ? "|" : string.Empty, arg);
	}
}

}

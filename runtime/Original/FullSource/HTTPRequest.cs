using Threading;
using UnityEngine;
using UnityEngine.Events;

public class HTTPRequest : ThreadedJob
{
	protected enum Type
	{
		GET = 0,
		POST = 1
	}

	protected static char Delimeter = '|';

	public string responseData;

	public string[] responseArray;

	public float executionTime;

	public bool exception;

	protected string url;

	protected string error;

	protected Type type;

	private UnityAction<HTTPRequest> onFinished;

	private UnityAction<HTTPRequest> onTimedOut;

	private float timestamp;

	public bool StatusOK
	{
		get
		{
			return responseData == "OK";
		}
	}

	public bool IsNullOrEmpty
	{
		get
		{
			return string.IsNullOrEmpty(responseData);
		}
	}

	public bool HasError
	{
		get
		{
			return responseData.Length >= 4 && responseData.Substring(0, 4) == "Code";
		}
	}

	public string ErrorCode
	{
		get
		{
			return (!HasError) ? string.Empty : responseData.Substring(5);
		}
	}

	public bool EmptyOrError
	{
		get
		{
			return HasError || IsNullOrEmpty;
		}
	}

	public HTTPRequest(UnityAction<HTTPRequest> onFinished, UnityAction<HTTPRequest> onTimedOut)
	{
		this.onFinished = onFinished;
		this.onTimedOut = onTimedOut;
	}

	public void Finalize(string url, double timeoutMilliseconds)
	{
		this.url = url;
		base.timeoutMilliseconds = timeoutMilliseconds;
	}

	protected override void OnStarting()
	{
		if (Version.Instance.Staging)
		{
			timestamp = Time.realtimeSinceStartup;
			Debug.LogFormat("HTTP request: {0}", GetType().FullName);
		}
	}

	protected override void OnFinished()
	{
		if (Version.Instance.Staging)
		{
			if (responseData.Length >= 4 && responseData.Substring(0, 4) == "Code")
			{
				Debug.LogWarningFormat("HTTP response: {0}, {1}, {2:F3} seconds", GetType().FullName, responseData, Time.realtimeSinceStartup - timestamp);
			}
			else
			{
				Debug.LogFormat("HTTP response: {0}, {1}, {2:F3} seconds", GetType().FullName, responseData, Time.realtimeSinceStartup - timestamp);
			}
			if (exception)
			{
				Debug.LogWarningFormat("HTTP error: {0}", error);
			}
		}
		if (!exception && onFinished != null)
		{
			onFinished(this);
		}
	}

	protected override void OnTimedOut()
	{
		if (Version.Instance.Staging)
		{
			Debug.LogWarningFormat("HTTP timeout: {0}, {1:F1} seconds", GetType().FullName, Time.realtimeSinceStartup - timestamp);
		}
		if (onTimedOut != null)
		{
			onTimedOut(this);
		}
	}
}

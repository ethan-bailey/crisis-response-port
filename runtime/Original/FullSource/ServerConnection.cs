namespace Crisis.Runtime.FullSource
{
using System.Collections.Generic;
using UnityEngine;

public class ServerConnection : MonoBehaviour
{
	public static string LiveURL = "http://crisisresponsegame.com/crserver_v4.php";

	public static string StagingURL = "http://crisisresponsegame.com/crstaging_v3.php";

	public bool communicationEnabled = true;

	public bool disableInEditor = true;

	public double timeoutMilliseconds = 15000.0;

	[Tooltip("If this number of timeouts occur communication is disabled.")]
	public int timeoutsBeforeDisable = 2;

	private HTTPRequest executing;

	private LinkedList<HTTPRequest> queue = new LinkedList<HTTPRequest>();

	private int numberOfTimeouts;

	private string ServerURL { get; set; }

	private void Awake()
	{
	}

	private void Start()
	{
		ServerURL = ((!Version.Instance.Staging) ? LiveURL : StagingURL);
	}

	public void PushRequest(HTTPRequest request)
	{
		if (communicationEnabled)
		{
			queue.AddLast(request);
		}
	}

	private void Update()
	{
		if (!communicationEnabled)
		{
			return;
		}
		if (executing != null)
		{
			if (executing.Update())
			{
				if (executing.TimedOut)
				{
					RegisterTimeout();
				}
				executing = null;
			}
		}
		else if (queue.First != null)
		{
			executing = queue.First.Value;
			queue.RemoveFirst();
			executing.Finalize(ServerURL, timeoutMilliseconds);
			executing.Start();
		}
	}

	private void RegisterTimeout()
	{
		numberOfTimeouts++;
		if (numberOfTimeouts >= timeoutsBeforeDisable)
		{
			communicationEnabled = false;
		}
	}
}

}

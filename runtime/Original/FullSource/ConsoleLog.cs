namespace Crisis.Runtime.FullSource
{
using System.Collections.Generic;
using UnityEngine;

public class ConsoleLog
{
	private static ConsoleLog instance = null;

	private static readonly object padlock = new object();

	public UnityEventVoid onNewMessage = new UnityEventVoid();

	private const int MaxMessages = 1024;

	private const int MaxHistory = 10;

	public static ConsoleLog Instance
	{
		get
		{
			lock (padlock)
			{
				if (instance == null)
				{
					string item = string.Format("This is {0} {1} version {2}.\nUse the console to enter commands. Type 'help' and hit return for a list of commands.", Version.Instance.GameTitle, Version.Instance.Codename, Version.Instance.VersionStringWithTags);
					ConsoleLog consoleLog = new ConsoleLog();
					consoleLog.Messages = new List<string> { item };
					consoleLog.InputHistory = new List<string>();
					instance = consoleLog;
				}
				return instance;
			}
		}
	}

	public List<string> Messages { get; private set; }

	public List<string> InputHistory { get; private set; }

	public void WriteLine(string message, bool playerInput = false)
	{
		if (Messages.Count >= 1024)
		{
			Messages.RemoveRange(0, Messages.Count - 1025);
		}
		if (playerInput)
		{
			if (InputHistory.Count >= 10)
			{
				InputHistory.RemoveRange(0, InputHistory.Count - 10 + 1);
			}
			InputHistory.Add(message);
			message = ">> " + message;
		}
		Debug.Log(message);
		Messages.Add(message);
		onNewMessage.Invoke();
	}
}

}

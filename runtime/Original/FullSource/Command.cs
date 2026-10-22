namespace Crisis.Runtime.FullSource
{
using System.Collections.Generic;
using UnityEngine;
using Utilities;

public class Command : MonoBehaviour
{
	public BuildAvailability availability;

	public List<CommandArgument> arguments = new List<CommandArgument>();

	public List<string> aliases = new List<string>();

	[Tooltip("Unlisted commands are hidden from players.")]
	public bool unlisted;

	[TextArea(3, 10)]
	public string description;

	[Header("Button")]
	[Tooltip("Add the command to the console window as a button?")]
	public bool showButton;

	[Tooltip("Text to display on the button.")]
	public string buttonText;

	[Tooltip("Optional raw string of arguments for the command.")]
	public string buttonArguments;

	public virtual bool Execute(string raw)
	{
		Debug.LogWarning("Executing an unimplemented raw command.");
		return false;
	}

	public string Syntax()
	{
		string text = base.name.ToLower();
		bool flag = false;
		if (arguments.Count > 0)
		{
			text += " ";
			bool flag2 = true;
			foreach (CommandArgument argument in arguments)
			{
				if (!flag2)
				{
					text += " ";
				}
				if (!flag && argument.optional)
				{
					text += "[";
					flag = true;
				}
				text = text + "<" + argument.name + ">";
				flag2 = false;
			}
			if (flag)
			{
				text += "]";
			}
			foreach (CommandArgument argument2 in arguments)
			{
				text = text + "\n  " + argument2.Syntax();
			}
		}
		return text;
	}

	public bool Match(string name)
	{
		if (name.ToLower() == base.name.ToLower())
		{
			return true;
		}
		foreach (string alias in aliases)
		{
			if (alias.ToLower() == name.ToLower())
			{
				return true;
			}
		}
		return false;
	}

	public bool ParseArguments(string[] rawArguments, out List<object> result)
	{
		result = new List<object>();
		for (int i = 0; i < arguments.Count; i++)
		{
			CommandArgument commandArgument = arguments[i];
			int num = i + 1;
			if (num >= rawArguments.Length)
			{
				if (!commandArgument.optional)
				{
					Debug.Log("Missing required argument.");
					return false;
				}
				break;
			}
			object obj = commandArgument.Parse(rawArguments[num]);
			if (obj != null)
			{
				result.Add(obj);
			}
		}
		return true;
	}
}

}

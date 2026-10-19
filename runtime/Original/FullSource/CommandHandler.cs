namespace Crisis.Runtime.FullSource
{
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

public class CommandHandler : MonoBehaviour
{
	public List<Command> all = new List<Command>();

	[Header("References")]
	public Transform buttonContainer;

	public Button buttonPrefab;

	private ConsoleLog log;

	private bool staging;

	public List<Command> Commands { get; private set; }

	private void Awake()
	{
		log = ConsoleLog.Instance;
	}

	private void Start()
	{
		staging = Version.Instance.Staging;
		AddCommands();
	}

	public bool HandleUserInput(string input)
	{
		if (string.IsNullOrEmpty(input))
		{
			return false;
		}
		log.WriteLine(input, true);
		return ProcessRaw(input);
	}

	public bool ProcessRaw(string raw)
	{
		string value = string.Empty;
		if (!PopString(ref raw, ref value))
		{
			log.WriteLine("Syntax error.");
			return false;
		}
		Command command = CommandByName(value);
		if (command == null)
		{
			log.WriteLine("No command by that name. Type 'help' and hit Return for a list of commands.");
			return false;
		}
		return command.Execute(raw);
	}

	public Command CommandByName(string name)
	{
		foreach (Command command in Commands)
		{
			if (command.Match(name))
			{
				return command;
			}
		}
		return null;
	}

	private void AddCommands()
	{
		Commands = new List<Command>();
		foreach (Command item in all)
		{
			if (Available(item) && !Commands.Contains(item))
			{
				Commands.Add(item);
				if (item.showButton)
				{
					AddCommandButton(item);
				}
			}
		}
	}

	private void AddCommandButton(Command command)
	{
		Button button = Object.Instantiate(buttonPrefab);
		button.transform.SetParent(buttonContainer);
		button.transform.localScale = Vector3.one;
		button.name = command.name;
		button.transform.GetChild(0).GetComponent<Text>().text = command.buttonText;
		button.onClick.AddListener(() =>
		{
			command.Execute(command.buttonArguments);
		});
	}

	private bool Available(Command command)
	{
		return command.availability == BuildAvailability.EnabledForAll || (command.availability == BuildAvailability.StagingOnly && staging);
	}

	public static bool PopBoolean(ref string rawInput, ref bool value)
	{
		int num = rawInput.IndexOf(' ');
		if (num == -1)
		{
			return false;
		}
		string text = rawInput.Substring(0, num);
		value = text == "yes" || text == "1" || text == "true";
		rawInput = rawInput.Substring(num + 1);
		return true;
	}

	public static bool PopInteger(ref string rawInput, ref int value)
	{
		int num = rawInput.IndexOf(' ');
		if (num == -1)
		{
			return false;
		}
		if (int.TryParse(rawInput.Substring(0, num), out value))
		{
			rawInput = rawInput.Substring(num + 1);
			return true;
		}
		return false;
	}

	public static bool PopFloat(ref string rawInput, ref float value)
	{
		int num = rawInput.IndexOf(' ');
		if (num == -1)
		{
			if (float.TryParse(rawInput.Substring(0), out value))
			{
				rawInput = string.Empty;
				return true;
			}
		}
		else if (float.TryParse(rawInput.Substring(0, num), out value))
		{
			rawInput = rawInput.Substring(num + 1);
			return true;
		}
		return false;
	}

	public static bool PopString(ref string input, ref string value)
	{
		int num = input.IndexOf('"');
		if (num == -1)
		{
			int num2 = input.IndexOf(' ');
			if (num2 == -1)
			{
				value = input;
				input = string.Empty;
				return !string.IsNullOrEmpty(value);
			}
			value = input.Substring(0, num2);
			input = input.Substring(num2 + 1);
			return true;
		}
		int num3 = input.IndexOf('"', num);
		if (num3 == -1)
		{
			ConsoleLog.Instance.WriteLine("Unterminated string argument.");
			return false;
		}
		value = input.Substring(num, num - num3);
		input = input.Substring(num3 + 1);
		return true;
	}
}

}

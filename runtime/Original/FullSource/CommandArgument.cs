namespace Crisis.Runtime.FullSource
{
using System;
using System.Collections.Generic;

[Serializable]
public class CommandArgument
{
	public enum Type
	{
		Boolean = 0,
		Integer = 1,
		Float = 2,
		String = 3
	}

	public string name;

	public Type type;

	public bool optional;

	public List<string> validValues = new List<string>();

	public string description;

	public string Syntax()
	{
		string text = StringHelper.LowerCaseFirstUpper(string.Format("{0}{1}", (!optional) ? string.Empty : "Optional ", type.ToString()));
		string text2 = string.Empty;
		foreach (string validValue in validValues)
		{
			text2 += string.Format("{0}'{1}'", (!string.IsNullOrEmpty(text2)) ? ", " : string.Empty, validValue);
		}
		if (!string.IsNullOrEmpty(text2))
		{
			text2 = " Valid values: " + text2;
		}
		return string.Format("{0}: {1}. {2}{3}", name, text, description, text2);
	}

	public object Parse(string rawArgument)
	{
		switch (type)
		{
		case Type.Boolean:
			return rawArgument == "yes" || rawArgument == "true" || rawArgument == "1";
		case Type.Integer:
		{
			int result2 = 0;
			if (int.TryParse(rawArgument, out result2))
			{
				return result2;
			}
			return null;
		}
		case Type.Float:
		{
			float result = 0f;
			if (float.TryParse(rawArgument, out result))
			{
				return result;
			}
			return null;
		}
		case Type.String:
			return rawArgument;
		default:
			return null;
		}
	}
}

}

namespace Crisis.Runtime.FullSource
{
public static class StringHelper
{
	public static string FriendlyName(string name)
	{
		string text = string.Empty;
		bool flag = true;
		bool flag2 = false;
		bool flag3 = false;
		foreach (char c in name)
		{
			if (!flag)
			{
				bool flag4 = char.IsNumber(c);
				bool flag5 = char.IsUpper(c);
				if ((flag4 && !flag2) || (flag5 && !flag3))
				{
					text += " ";
				}
				flag2 = flag4;
				flag3 = flag5;
			}
			text += c;
			flag = false;
		}
		return text;
	}

	public static string LowerCaseFirstUpper(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return string.Empty;
		}
		return text.Substring(0, 1).ToUpper() + text.Substring(1).ToLower();
	}
}

}

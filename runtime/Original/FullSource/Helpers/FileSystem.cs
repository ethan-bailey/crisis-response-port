using System;
using System.IO;

namespace Helpers
{
	public static class FileSystem
	{
		public static string MakeFilenameValid(string name)
		{
			char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
			for (int num = 0; num != -1; num = name.IndexOfAny(invalidFileNameChars))
			{
				name.Remove(num, 1);
			}
			return name;
		}

		public static bool IsSubfolder(string parentPath, string childPath)
		{
			Uri uri = new Uri(parentPath);
			for (DirectoryInfo parent = new DirectoryInfo(childPath).Parent; parent != null; parent = parent.Parent)
			{
				if (new Uri(parent.FullName) == uri)
				{
					return true;
				}
			}
			return false;
		}
	}
}

using Object = UnityEngine.Object;
using System;

namespace BehaviorDesigner.Runtime.Tasks;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class TaskIconAttribute : Attribute
{
	public readonly string mIconPath;

	public string IconPath => mIconPath;

	public TaskIconAttribute(string iconPath)
	{
		mIconPath = iconPath;
	}
}

using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;
using Object = UnityEngine.Object;
using Task = BehaviorDesigner.Runtime.Tasks.Task;
using System;
using System.Collections.Generic;
using System.Reflection;
using BehaviorDesigner.Runtime.Tasks;

namespace BehaviorDesigner.Runtime;

public class TaskUtility
{
	[NonSerialized]
	private static Dictionary<string, Type> typeLookup = new Dictionary<string, Type>();

	private static List<string> loadedAssemblies = null;

	private static Dictionary<Type, FieldInfo[]> allFieldsLookup = new Dictionary<Type, FieldInfo[]>();

	private static Dictionary<Type, FieldInfo[]> publicFieldsLookup = new Dictionary<Type, FieldInfo[]>();

	private static Dictionary<FieldInfo, Dictionary<Type, bool>> hasFieldLookup = new Dictionary<FieldInfo, Dictionary<Type, bool>>();

	public static object CreateInstance(Type t)
	{
		if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
		{
			t = Nullable.GetUnderlyingType(t);
		}
		return Activator.CreateInstance(t, nonPublic: true);
	}

	public static FieldInfo[] GetAllFields(Type t)
	{
		FieldInfo[] value = null;
		if (!allFieldsLookup.TryGetValue(t, out value))
		{
			List<FieldInfo> fieldList = ObjectPool.Get<List<FieldInfo>>();
			fieldList.Clear();
			BindingFlags flags = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			GetFields(t, ref fieldList, (int)flags);
			value = fieldList.ToArray();
			ObjectPool.Return(fieldList);
			allFieldsLookup.Add(t, value);
		}
		return value;
	}

	public static FieldInfo[] GetPublicFields(Type t)
	{
		FieldInfo[] value = null;
		if (!publicFieldsLookup.TryGetValue(t, out value))
		{
			List<FieldInfo> fieldList = ObjectPool.Get<List<FieldInfo>>();
			fieldList.Clear();
			BindingFlags flags = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public;
			GetFields(t, ref fieldList, (int)flags);
			value = fieldList.ToArray();
			ObjectPool.Return(fieldList);
			publicFieldsLookup.Add(t, value);
		}
		return value;
	}

	private static void GetFields(Type t, ref List<FieldInfo> fieldList, int flags)
	{
		if (t != null && !t.Equals(typeof(ParentTask)) && !t.Equals(typeof(Task)) && !t.Equals(typeof(SharedVariable)))
		{
			FieldInfo[] fields = t.GetFields((BindingFlags)flags);
			for (int i = 0; i < fields.Length; i++)
			{
				fieldList.Add(fields[i]);
			}
			GetFields(t.BaseType, ref fieldList, flags);
		}
	}

	public static Type GetTypeWithinAssembly(string typeName)
	{
		if (typeLookup.TryGetValue(typeName, out var value))
		{
			return value;
		}
		value = Type.GetType(typeName);
		if (value == null)
		{
			if (loadedAssemblies == null)
			{
				loadedAssemblies = new List<string>();
				Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
				for (int i = 0; i < assemblies.Length; i++)
				{
					loadedAssemblies.Add(assemblies[i].FullName);
				}
			}
			for (int j = 0; j < loadedAssemblies.Count; j++)
			{
				value = Type.GetType(typeName + "," + loadedAssemblies[j]);
				if (value != null)
				{
					break;
				}
			}
			// Property mappings in the original Behavior Designer assets store
			// short component names (for example "Blackboard"). The upstream
			// project type lived in Assembly-CSharp, while this runtime compiles it
			// under Crisis.Runtime.FullSource, so assembly-qualified lookup misses.
			if (value == null && !typeName.Contains('.'))
			{
				foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
				{
					Type[] types;
					try { types = assembly.GetTypes(); }
					catch (ReflectionTypeLoadException error) { types = error.Types.Where(type => type != null).ToArray(); }
					value = types.FirstOrDefault(type => type.Name == typeName);
					if (value != null) break;
				}
			}
		}
		if (value != null)
		{
			typeLookup.Add(typeName, value);
		}
		return value;
	}

	public static bool CompareType(Type t, string typeName)
	{
		Type type = Type.GetType(typeName + ", Assembly-CSharp");
		if (type == null)
		{
			type = Type.GetType(typeName + ", Assembly-CSharp-firstpass");
		}
		return t.Equals(type);
	}

	public static bool HasAttribute(FieldInfo field, Type attribute)
	{
		if (field == null)
		{
			return false;
		}
		if (!hasFieldLookup.TryGetValue(field, out var value))
		{
			value = new Dictionary<Type, bool>();
			hasFieldLookup.Add(field, value);
		}
		if (!value.TryGetValue(attribute, out var value2))
		{
			value2 = field.GetCustomAttributes(attribute, inherit: false).Length > 0;
			value.Add(attribute, value2);
		}
		return value2;
	}
}

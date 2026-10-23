using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;
using Object = UnityEngine.Object;
using Task = BehaviorDesigner.Runtime.Tasks.Task;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public static class BinaryDeserialization
{
	private class ObjectFieldMap
	{
		public object obj;

		public FieldInfo fieldInfo;

		public ObjectFieldMap(object o, FieldInfo f)
		{
			obj = o;
			fieldInfo = f;
		}
	}

	private class ObjectFieldMapComparer : IEqualityComparer<ObjectFieldMap>
	{
		public bool Equals(ObjectFieldMap a, ObjectFieldMap b)
		{
			if (object.ReferenceEquals(a, null))
			{
				return false;
			}
			if (object.ReferenceEquals(b, null))
			{
				return false;
			}
			return a.obj.Equals(b.obj) && a.fieldInfo.Equals(b.fieldInfo);
		}

		public int GetHashCode(ObjectFieldMap a)
		{
			return (a != null) ? (a.obj.ToString().GetHashCode() + a.fieldInfo.ToString().GetHashCode()) : 0;
		}
	}

	private static GlobalVariables globalVariables;

	private static Dictionary<ObjectFieldMap, List<int>> taskIDs;

	private static SHA1 shaHash;

	private static bool updatedSerialization;

	private static bool shaHashSerialization;

	private static bool strHashSerialization;

	public static void Load(BehaviorSource behaviorSource)
	{
		Load(behaviorSource.TaskData, behaviorSource);
	}

	public static void Load(TaskSerializationData taskData, BehaviorSource behaviorSource)
	{
		if (taskData != null && string.IsNullOrEmpty(taskData.Version))
		{
			BinaryDeserializationDeprecated.Load(taskData, behaviorSource);
			return;
		}
		behaviorSource.EntryTask = null;
		behaviorSource.RootTask = null;
		behaviorSource.DetachedTasks = null;
		behaviorSource.Variables = null;
		FieldSerializationData fieldSerializationData;
		if (taskData == null || (fieldSerializationData = taskData.fieldSerializationData).byteData == null || fieldSerializationData.byteData.Count == 0)
		{
			return;
		}
		fieldSerializationData.byteDataArray = fieldSerializationData.byteData.ToArray();
		taskIDs = null;
		Version version = new Version(taskData.Version);
		updatedSerialization = version.CompareTo(new Version("1.5.7")) >= 0;
		shaHashSerialization = (strHashSerialization = false);
		if (updatedSerialization)
		{
			shaHashSerialization = version.CompareTo(new Version("1.5.9")) >= 0;
			if (shaHashSerialization)
			{
				strHashSerialization = version.CompareTo(new Version("1.5.11")) >= 0;
			}
		}
		if (taskData.variableStartIndex != null)
		{
			List<SharedVariable> list = new List<SharedVariable>();
			Dictionary<int, int> dictionary = ObjectPool.Get<Dictionary<int, int>>();
			for (int i = 0; i < taskData.variableStartIndex.Count; i++)
			{
				int num = taskData.variableStartIndex[i];
				int num2 = ((i + 1 < taskData.variableStartIndex.Count) ? taskData.variableStartIndex[i + 1] : ((taskData.startIndex == null || taskData.startIndex.Count <= 0) ? fieldSerializationData.startIndex.Count : taskData.startIndex[0]));
				dictionary.Clear();
				for (int j = num; j < num2; j++)
				{
					dictionary.Add(fieldSerializationData.fieldNameHash[j], fieldSerializationData.startIndex[j]);
				}
				SharedVariable sharedVariable = BytesToSharedVariable(fieldSerializationData, dictionary, fieldSerializationData.byteDataArray, taskData.variableStartIndex[i], behaviorSource, fromField: false, 0);
				if (sharedVariable != null)
				{
					list.Add(sharedVariable);
				}
			}
			ObjectPool.Return(dictionary);
			behaviorSource.Variables = list;
		}
		List<Task> taskList = new List<Task>();
		if (taskData.types != null)
		{
			for (int k = 0; k < taskData.types.Count; k++)
			{
				LoadTask(taskData, fieldSerializationData, ref taskList, ref behaviorSource);
			}
		}
		if (taskData.parentIndex.Count != taskList.Count)
		{
			Debug.LogError("Deserialization Error: parent index count does not match task list count");
			return;
		}
		for (int l = 0; l < taskData.parentIndex.Count; l++)
		{
			if (taskData.parentIndex[l] == -1)
			{
				if (behaviorSource.EntryTask == null)
				{
					behaviorSource.EntryTask = taskList[l];
					continue;
				}
				if (behaviorSource.DetachedTasks == null)
				{
					behaviorSource.DetachedTasks = new List<Task>();
				}
				behaviorSource.DetachedTasks.Add(taskList[l]);
			}
			else if (taskData.parentIndex[l] == 0)
			{
				behaviorSource.RootTask = taskList[l];
			}
			else if (taskData.parentIndex[l] != -1 && taskList[taskData.parentIndex[l]] is ParentTask parentTask)
			{
				int index = ((parentTask.Children != null) ? parentTask.Children.Count : 0);
				parentTask.AddChild(taskList[l], index);
			}
		}
		if (taskIDs == null)
		{
			return;
		}
		foreach (ObjectFieldMap key in taskIDs.Keys)
		{
			List<int> list2 = taskIDs[key];
			Type fieldType = key.fieldInfo.FieldType;
			if (typeof(IList).IsAssignableFrom(fieldType))
			{
				if (fieldType.IsArray)
				{
					Type elementType = fieldType.GetElementType();
					int num3 = 0;
					for (int m = 0; m < list2.Count; m++)
					{
						Task task = taskList[list2[m]];
						if (elementType.IsAssignableFrom(task.GetType()))
						{
							num3++;
						}
					}
					int num4 = 0;
					Array array = Array.CreateInstance(elementType, num3);
					for (int n = 0; n < array.Length; n++)
					{
						Task task2 = taskList[list2[n]];
						if (elementType.IsAssignableFrom(task2.GetType()))
						{
							array.SetValue(task2, num4);
							num4++;
						}
					}
					key.fieldInfo.SetValue(key.obj, array);
					continue;
				}
				Type type = fieldType.GetGenericArguments()[0];
				IList list3 = TaskUtility.CreateInstance(typeof(List<>).MakeGenericType(type)) as IList;
				for (int num5 = 0; num5 < list2.Count; num5++)
				{
					Task task3 = taskList[list2[num5]];
					if (type.IsAssignableFrom(task3.GetType()))
					{
						list3.Add(task3);
					}
				}
				key.fieldInfo.SetValue(key.obj, list3);
			}
			else
			{
				key.fieldInfo.SetValue(key.obj, taskList[list2[0]]);
			}
		}
	}

	public static void Load(GlobalVariables globalVariables, string version)
	{
		if (globalVariables == null)
		{
			return;
		}
		if (string.IsNullOrEmpty(version))
		{
			BinaryDeserializationDeprecated.Load(globalVariables);
			return;
		}
		globalVariables.Variables = null;
		FieldSerializationData fieldSerializationData;
		if (globalVariables.VariableData == null || (fieldSerializationData = globalVariables.VariableData.fieldSerializationData).byteData == null || fieldSerializationData.byteData.Count == 0)
		{
			return;
		}
		if (fieldSerializationData.typeName.Count > 0)
		{
			BinaryDeserializationDeprecated.Load(globalVariables);
			return;
		}
		VariableSerializationData variableData = globalVariables.VariableData;
		fieldSerializationData.byteDataArray = fieldSerializationData.byteData.ToArray();
		Version version2 = new Version(globalVariables.Version);
		updatedSerialization = version2.CompareTo(new Version("1.5.7")) >= 0;
		shaHashSerialization = (strHashSerialization = false);
		if (updatedSerialization)
		{
			shaHashSerialization = version2.CompareTo(new Version("1.5.9")) >= 0;
			if (shaHashSerialization)
			{
				strHashSerialization = version2.CompareTo(new Version("1.5.11")) >= 0;
			}
		}
		if (variableData.variableStartIndex == null)
		{
			return;
		}
		List<SharedVariable> list = new List<SharedVariable>();
		Dictionary<int, int> dictionary = ObjectPool.Get<Dictionary<int, int>>();
		for (int i = 0; i < variableData.variableStartIndex.Count; i++)
		{
			int num = variableData.variableStartIndex[i];
			int num2 = ((i + 1 >= variableData.variableStartIndex.Count) ? fieldSerializationData.startIndex.Count : variableData.variableStartIndex[i + 1]);
			dictionary.Clear();
			for (int j = num; j < num2; j++)
			{
				dictionary.Add(fieldSerializationData.fieldNameHash[j], fieldSerializationData.startIndex[j]);
			}
			SharedVariable sharedVariable = BytesToSharedVariable(fieldSerializationData, dictionary, fieldSerializationData.byteDataArray, variableData.variableStartIndex[i], globalVariables, fromField: false, 0);
			if (sharedVariable != null)
			{
				list.Add(sharedVariable);
			}
		}
		ObjectPool.Return(dictionary);
		globalVariables.Variables = list;
	}

	public static void LoadTask(TaskSerializationData taskSerializationData, FieldSerializationData fieldSerializationData, ref List<Task> taskList, ref BehaviorSource behaviorSource)
	{
		int count = taskList.Count;
		int num = taskSerializationData.startIndex[count];
		int num2 = ((count + 1 >= taskSerializationData.startIndex.Count) ? fieldSerializationData.startIndex.Count : taskSerializationData.startIndex[count + 1]);
		Dictionary<int, int> dictionary = ObjectPool.Get<Dictionary<int, int>>();
		dictionary.Clear();
		for (int i = num; i < num2; i++)
		{
			if (!dictionary.ContainsKey(fieldSerializationData.fieldNameHash[i]))
			{
				dictionary.Add(fieldSerializationData.fieldNameHash[i], fieldSerializationData.startIndex[i]);
			}
		}
		Task task = null;
		Type type = TaskUtility.GetTypeWithinAssembly(taskSerializationData.types[count]);
		if (type == null)
		{
			bool flag = false;
			for (int j = 0; j < taskSerializationData.parentIndex.Count; j++)
			{
				if (count == taskSerializationData.parentIndex[j])
				{
					flag = true;
					break;
				}
			}
			type = ((!flag) ? typeof(UnknownTask) : typeof(UnknownParentTask));
		}
		task = TaskUtility.CreateInstance(type) as Task;
		if (task is UnknownTask)
		{
			UnknownTask unknownTask = task as UnknownTask;
			for (int k = num; k < num2; k++)
			{
				unknownTask.fieldNameHash.Add(fieldSerializationData.fieldNameHash[k]);
				unknownTask.startIndex.Add(fieldSerializationData.startIndex[k] - fieldSerializationData.startIndex[num]);
			}
			for (int l = fieldSerializationData.startIndex[num]; l <= fieldSerializationData.startIndex[num2 - 1]; l++)
			{
				unknownTask.dataPosition.Add(fieldSerializationData.dataPosition[l] - fieldSerializationData.dataPosition[fieldSerializationData.startIndex[num]]);
			}
			num2 = ((count + 1 >= taskSerializationData.startIndex.Count || taskSerializationData.startIndex[count + 1] >= fieldSerializationData.dataPosition.Count) ? fieldSerializationData.byteData.Count : fieldSerializationData.dataPosition[taskSerializationData.startIndex[count + 1]]);
			for (int m = fieldSerializationData.dataPosition[fieldSerializationData.startIndex[num]]; m < num2; m++)
			{
				unknownTask.byteData.Add(fieldSerializationData.byteData[m]);
			}
			unknownTask.unityObjects = fieldSerializationData.unityObjects;
		}
		task.Owner = behaviorSource.Owner.GetObject() as Behavior;
		taskList.Add(task);
		// Some shipped Behavior Designer trees omit optional task metadata fields.
		// LoadField returns null for an absent hash, so direct unboxing crashes
		// before the original tree can initialize. Keep deterministic safe defaults.
		object taskID = LoadField(fieldSerializationData, dictionary, typeof(int), "ID", 0, null);
		task.ID = taskID is int id ? id : count;
		task.FriendlyName = LoadField(fieldSerializationData, dictionary, typeof(string), "FriendlyName", 0, null) as string ?? task.GetType().Name;
		object isInstant = LoadField(fieldSerializationData, dictionary, typeof(bool), "IsInstant", 0, null);
		task.IsInstant = isInstant is bool instant && instant;
		object obj;
		if ((obj = LoadField(fieldSerializationData, dictionary, typeof(bool), "Disabled", 0, null)) != null)
		{
			task.Disabled = (bool)obj;
		}
		LoadNodeData(fieldSerializationData, dictionary, taskList[count]);
		if (task.GetType().Equals(typeof(UnknownTask)) || task.GetType().Equals(typeof(UnknownParentTask)))
		{
			if (!task.FriendlyName.Contains("Unknown "))
			{
				task.FriendlyName = $"Unknown {task.FriendlyName}";
			}
			task.NodeData.Comment = "Unknown Task. Right click and Replace to locate new task.";
		}
		LoadFields(fieldSerializationData, dictionary, taskList[count], 0, behaviorSource);
		ObjectPool.Return(dictionary);
	}

	private static void LoadNodeData(FieldSerializationData fieldSerializationData, Dictionary<int, int> fieldIndexMap, Task task)
	{
		NodeData nodeData = new NodeData();
		object offset = LoadField(fieldSerializationData, fieldIndexMap, typeof(Vector2), "NodeDataOffset", 0, null);
		nodeData.Offset = offset is Vector2 nodeOffset ? nodeOffset : Vector2.zero;
		nodeData.Comment = LoadField(fieldSerializationData, fieldIndexMap, typeof(string), "NodeDataComment", 0, null) as string ?? string.Empty;
		object breakpoint = LoadField(fieldSerializationData, fieldIndexMap, typeof(bool), "NodeDataIsBreakpoint", 0, null);
		nodeData.IsBreakpoint = breakpoint is bool isBreakpoint && isBreakpoint;
		object collapsed = LoadField(fieldSerializationData, fieldIndexMap, typeof(bool), "NodeDataCollapsed", 0, null);
		nodeData.Collapsed = collapsed is bool isCollapsed && isCollapsed;
		object obj = LoadField(fieldSerializationData, fieldIndexMap, typeof(int), "NodeDataColorIndex", 0, null);
		if (obj != null)
		{
			nodeData.ColorIndex = (int)obj;
		}
		obj = LoadField(fieldSerializationData, fieldIndexMap, typeof(List<string>), "NodeDataWatchedFields", 0, null);
		if (obj != null)
		{
			nodeData.WatchedFieldNames = new List<string>();
			nodeData.WatchedFields = new List<FieldInfo>();
			IList list = obj as IList;
			for (int i = 0; i < list.Count; i++)
			{
				FieldInfo field = task.GetType().GetField((string)list[i], BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (field != null)
				{
					nodeData.WatchedFieldNames.Add(field.Name);
					nodeData.WatchedFields.Add(field);
				}
			}
		}
		task.NodeData = nodeData;
	}

	private static void LoadFields(FieldSerializationData fieldSerializationData, Dictionary<int, int> fieldIndexMap, object obj, int hashPrefix, IVariableSource variableSource)
	{
		FieldInfo[] allFields = TaskUtility.GetAllFields(obj.GetType());
		for (int i = 0; i < allFields.Length; i++)
		{
			if (!TaskUtility.HasAttribute(allFields[i], typeof(NonSerializedAttribute)) && ((!allFields[i].IsPrivate && !allFields[i].IsFamily) || TaskUtility.HasAttribute(allFields[i], typeof(SerializeField))) && (!(obj is ParentTask) || !allFields[i].Name.Equals("children")))
			{
				object obj2 = LoadField(fieldSerializationData, fieldIndexMap, allFields[i].FieldType, allFields[i].Name, hashPrefix, variableSource, obj, allFields[i]);
				if (obj2 != null && !object.ReferenceEquals(obj2, null) && !obj2.Equals(null))
				{
					allFields[i].SetValue(obj, obj2);
				}
			}
		}
	}

	private static object LoadField(FieldSerializationData fieldSerializationData, Dictionary<int, int> fieldIndexMap, Type fieldType, string fieldName, int hashPrefix, IVariableSource variableSource, object obj = null, FieldInfo fieldInfo = null)
	{
		int num = hashPrefix;
		num = ((!shaHashSerialization) ? (num + (LegacyStringHash(fieldType.Name) + LegacyStringHash(fieldName))) : (num + (StringHash(fieldType.Name.ToString(), strHashSerialization) + StringHash(fieldName, strHashSerialization))));
		if (!fieldIndexMap.TryGetValue(num, out var value))
		{
			if (fieldType.IsAbstract)
			{
				return null;
			}
			if (typeof(SharedVariable).IsAssignableFrom(fieldType))
			{
				SharedVariable sharedVariable = TaskUtility.CreateInstance(fieldType) as SharedVariable;
				if (fieldInfo.GetValue(obj) is SharedVariable sharedVariable2)
				{
					sharedVariable.SetValue(sharedVariable2.GetValue());
				}
				return sharedVariable;
			}
			return null;
		}
		object obj2 = null;
		if (typeof(IList).IsAssignableFrom(fieldType))
		{
			int num2 = BytesToInt(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[value]);
			if (fieldType.IsArray)
			{
				Type elementType = fieldType.GetElementType();
				if (elementType == null)
				{
					return null;
				}
				Array array = Array.CreateInstance(elementType, num2);
				for (int i = 0; i < num2; i++)
				{
					object obj3 = LoadField(fieldSerializationData, fieldIndexMap, elementType, i.ToString(), num / ((!updatedSerialization) ? 1 : (i + 1)), variableSource, obj, fieldInfo);
					array.SetValue((!object.ReferenceEquals(obj3, null) && !obj3.Equals(null)) ? obj3 : null, i);
				}
				obj2 = array;
			}
			else
			{
				Type type = fieldType;
				while (!type.IsGenericType)
				{
					type = type.BaseType;
				}
				Type type2 = type.GetGenericArguments()[0];
				IList list = ((!fieldType.IsGenericType) ? (TaskUtility.CreateInstance(fieldType) as IList) : (TaskUtility.CreateInstance(typeof(List<>).MakeGenericType(type2)) as IList));
				for (int j = 0; j < num2; j++)
				{
					object obj4 = LoadField(fieldSerializationData, fieldIndexMap, type2, j.ToString(), num / ((!updatedSerialization) ? 1 : (j + 1)), variableSource, obj, fieldInfo);
					list.Add((!object.ReferenceEquals(obj4, null) && !obj4.Equals(null)) ? obj4 : null);
				}
				obj2 = list;
			}
		}
		else if (typeof(Task).IsAssignableFrom(fieldType))
		{
			if (fieldInfo != null && TaskUtility.HasAttribute(fieldInfo, typeof(InspectTaskAttribute)))
			{
				string text = BytesToString(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[value], GetFieldSize(fieldSerializationData, value));
				if (!string.IsNullOrEmpty(text))
				{
					Type typeWithinAssembly = TaskUtility.GetTypeWithinAssembly(text);
					if (typeWithinAssembly != null)
					{
						obj2 = TaskUtility.CreateInstance(typeWithinAssembly);
						LoadFields(fieldSerializationData, fieldIndexMap, obj2, num, variableSource);
					}
				}
			}
			else
			{
				if (taskIDs == null)
				{
					taskIDs = new Dictionary<ObjectFieldMap, List<int>>(new ObjectFieldMapComparer());
				}
				int item = BytesToInt(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[value]);
				ObjectFieldMap key = new ObjectFieldMap(obj, fieldInfo);
				if (taskIDs.ContainsKey(key))
				{
					taskIDs[key].Add(item);
				}
				else
				{
					List<int> list2 = new List<int>();
					list2.Add(item);
					taskIDs.Add(key, list2);
				}
			}
		}
		else if (typeof(SharedVariable).IsAssignableFrom(fieldType))
		{
			obj2 = BytesToSharedVariable(fieldSerializationData, fieldIndexMap, fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[value], variableSource, fromField: true, num);
		}
		else if (typeof(UnityEngine.Object).IsAssignableFrom(fieldType))
		{
			int index = BytesToInt(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[value]);
			obj2 = IndexToUnityObject(index, fieldSerializationData);
		}
		else if (fieldType.Equals(typeof(int)) || fieldType.IsEnum)
		{
			obj2 = BytesToInt(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[value]);
		}
		else if (fieldType.Equals(typeof(uint)))
		{
			obj2 = BytesToUInt(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[value]);
		}
		else if (fieldType.Equals(typeof(float)))
		{
			obj2 = BytesToFloat(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[value]);
		}
		else if (fieldType.Equals(typeof(double)))
		{
			obj2 = BytesToDouble(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[value]);
		}
		else if (fieldType.Equals(typeof(long)))
		{
			obj2 = BytesToLong(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[value]);
		}
		else if (fieldType.Equals(typeof(bool)))
		{
			obj2 = BytesToBool(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[value]);
		}
		else if (fieldType.Equals(typeof(string)))
		{
			obj2 = BytesToString(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[value], GetFieldSize(fieldSerializationData, value));
		}
		else if (fieldType.Equals(typeof(byte)))
		{
			obj2 = BytesToByte(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[value]);
		}
		else if (fieldType.Equals(typeof(Vector2)))
		{
			obj2 = BytesToVector2(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[value]);
		}
		else if (fieldType.Equals(typeof(Vector3)))
		{
			obj2 = BytesToVector3(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[value]);
		}
		else if (fieldType.Equals(typeof(Vector4)))
		{
			obj2 = BytesToVector4(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[value]);
		}
		else if (fieldType.Equals(typeof(Quaternion)))
		{
			obj2 = BytesToQuaternion(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[value]);
		}
		else if (fieldType.Equals(typeof(Color)))
		{
			obj2 = BytesToColor(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[value]);
		}
		else if (fieldType.Equals(typeof(Rect)))
		{
			obj2 = BytesToRect(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[value]);
		}
		else if (fieldType.Equals(typeof(Matrix4x4)))
		{
			obj2 = BytesToMatrix4x4(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[value]);
		}
		else if (fieldType.Equals(typeof(AnimationCurve)))
		{
			obj2 = BytesToAnimationCurve(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[value]);
		}
		else if (fieldType.Equals(typeof(LayerMask)))
		{
			obj2 = BytesToLayerMask(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[value]);
		}
		else if (fieldType.IsClass || (fieldType.IsValueType && !fieldType.IsPrimitive))
		{
			obj2 = TaskUtility.CreateInstance(fieldType);
			LoadFields(fieldSerializationData, fieldIndexMap, obj2, num, variableSource);
			return obj2;
		}
		return obj2;
	}

	public static int StringHash(string value, bool fastHash)
	{
		if (string.IsNullOrEmpty(value))
		{
			return 0;
		}
		if (fastHash)
		{
			int num = 23;
			int length = value.Length;
			for (int i = 0; i < length; i++)
			{
				num = num * 31 + value[i];
			}
			return num;
		}
		byte[] bytes = Encoding.UTF8.GetBytes(value);
		if (shaHash == null)
		{
			shaHash = new SHA1Managed();
		}
		byte[] value2 = shaHash.ComputeHash(bytes);
		return BitConverter.ToInt32(value2, 0);
	}

	// Unity's legacy Behavior Designer field hashes were produced by Mono's
	// deterministic string hash. .NET 8 randomizes string.GetHashCode per
	// process, so using it here makes serialized v1.5.7–1.5.8 field lookups fail
	// (including nested [InspectTask] references). Preserve the original Mono
	// polynomial hash for those pre-SHA serialization versions.
	private static int LegacyStringHash(string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			return 0;
		}
		int hash = 0;
		unchecked
		{
			for (int i = 0; i < value.Length; i++)
			{
				hash = hash * 31 + value[i];
			}
		}
		return hash;
	}

	private static int GetFieldSize(FieldSerializationData fieldSerializationData, int fieldIndex)
	{
		return ((fieldIndex + 1 >= fieldSerializationData.dataPosition.Count) ? fieldSerializationData.byteData.Count : fieldSerializationData.dataPosition[fieldIndex + 1]) - fieldSerializationData.dataPosition[fieldIndex];
	}

	private static int BytesToInt(byte[] bytes, int dataPosition)
	{
		return BitConverter.ToInt32(bytes, dataPosition);
	}

	private static uint BytesToUInt(byte[] bytes, int dataPosition)
	{
		return BitConverter.ToUInt32(bytes, dataPosition);
	}

	private static float BytesToFloat(byte[] bytes, int dataPosition)
	{
		return BitConverter.ToSingle(bytes, dataPosition);
	}

	private static double BytesToDouble(byte[] bytes, int dataPosition)
	{
		return BitConverter.ToDouble(bytes, dataPosition);
	}

	private static long BytesToLong(byte[] bytes, int dataPosition)
	{
		return BitConverter.ToInt64(bytes, dataPosition);
	}

	private static bool BytesToBool(byte[] bytes, int dataPosition)
	{
		return BitConverter.ToBoolean(bytes, dataPosition);
	}

	private static string BytesToString(byte[] bytes, int dataPosition, int dataSize)
	{
		if (dataSize == 0)
		{
			return string.Empty;
		}
		return Encoding.UTF8.GetString(bytes, dataPosition, dataSize);
	}

	private static byte BytesToByte(byte[] bytes, int dataPosition)
	{
		return bytes[dataPosition];
	}

	private static Color BytesToColor(byte[] bytes, int dataPosition)
	{
		Color black = Color.black;
		black.r = BitConverter.ToSingle(bytes, dataPosition);
		black.g = BitConverter.ToSingle(bytes, dataPosition + 4);
		black.b = BitConverter.ToSingle(bytes, dataPosition + 8);
		black.a = BitConverter.ToSingle(bytes, dataPosition + 12);
		return black;
	}

	private static Vector2 BytesToVector2(byte[] bytes, int dataPosition)
	{
		Vector2 zero = Vector2.zero;
		zero.x = BitConverter.ToSingle(bytes, dataPosition);
		zero.y = BitConverter.ToSingle(bytes, dataPosition + 4);
		return zero;
	}

	private static Vector3 BytesToVector3(byte[] bytes, int dataPosition)
	{
		Vector3 zero = Vector3.zero;
		zero.x = BitConverter.ToSingle(bytes, dataPosition);
		zero.y = BitConverter.ToSingle(bytes, dataPosition + 4);
		zero.z = BitConverter.ToSingle(bytes, dataPosition + 8);
		return zero;
	}

	private static Vector4 BytesToVector4(byte[] bytes, int dataPosition)
	{
		Vector4 zero = Vector4.zero;
		zero.x = BitConverter.ToSingle(bytes, dataPosition);
		zero.y = BitConverter.ToSingle(bytes, dataPosition + 4);
		zero.z = BitConverter.ToSingle(bytes, dataPosition + 8);
		zero.w = BitConverter.ToSingle(bytes, dataPosition + 12);
		return zero;
	}

	private static Quaternion BytesToQuaternion(byte[] bytes, int dataPosition)
	{
		Quaternion identity = Quaternion.identity;
		identity.x = BitConverter.ToSingle(bytes, dataPosition);
		identity.y = BitConverter.ToSingle(bytes, dataPosition + 4);
		identity.z = BitConverter.ToSingle(bytes, dataPosition + 8);
		identity.w = BitConverter.ToSingle(bytes, dataPosition + 12);
		return identity;
	}

	private static Rect BytesToRect(byte[] bytes, int dataPosition)
	{
		return new Rect
		{
			x = BitConverter.ToSingle(bytes, dataPosition),
			y = BitConverter.ToSingle(bytes, dataPosition + 4),
			width = BitConverter.ToSingle(bytes, dataPosition + 8),
			height = BitConverter.ToSingle(bytes, dataPosition + 12)
		};
	}

	private static Matrix4x4 BytesToMatrix4x4(byte[] bytes, int dataPosition)
	{
		Matrix4x4 identity = Matrix4x4.identity;
		identity.m00 = BitConverter.ToSingle(bytes, dataPosition);
		identity.m01 = BitConverter.ToSingle(bytes, dataPosition + 4);
		identity.m02 = BitConverter.ToSingle(bytes, dataPosition + 8);
		identity.m03 = BitConverter.ToSingle(bytes, dataPosition + 12);
		identity.m10 = BitConverter.ToSingle(bytes, dataPosition + 16);
		identity.m11 = BitConverter.ToSingle(bytes, dataPosition + 20);
		identity.m12 = BitConverter.ToSingle(bytes, dataPosition + 24);
		identity.m13 = BitConverter.ToSingle(bytes, dataPosition + 28);
		identity.m20 = BitConverter.ToSingle(bytes, dataPosition + 32);
		identity.m21 = BitConverter.ToSingle(bytes, dataPosition + 36);
		identity.m22 = BitConverter.ToSingle(bytes, dataPosition + 40);
		identity.m23 = BitConverter.ToSingle(bytes, dataPosition + 44);
		identity.m30 = BitConverter.ToSingle(bytes, dataPosition + 48);
		identity.m31 = BitConverter.ToSingle(bytes, dataPosition + 52);
		identity.m32 = BitConverter.ToSingle(bytes, dataPosition + 56);
		identity.m33 = BitConverter.ToSingle(bytes, dataPosition + 60);
		return identity;
	}

	private static AnimationCurve BytesToAnimationCurve(byte[] bytes, int dataPosition)
	{
		AnimationCurve animationCurve = new AnimationCurve();
		int num = BitConverter.ToInt32(bytes, dataPosition);
		for (int i = 0; i < num; i++)
		{
			animationCurve.AddKey(new Keyframe
			{
				time = BitConverter.ToSingle(bytes, dataPosition + 4),
				value = BitConverter.ToSingle(bytes, dataPosition + 8),
				inTangent = BitConverter.ToSingle(bytes, dataPosition + 12),
				outTangent = BitConverter.ToSingle(bytes, dataPosition + 16),
				tangentMode = BitConverter.ToInt32(bytes, dataPosition + 20)
			});
			dataPosition += 20;
		}
		animationCurve.preWrapMode = (WrapMode)BitConverter.ToInt32(bytes, dataPosition + 4);
		animationCurve.postWrapMode = (WrapMode)BitConverter.ToInt32(bytes, dataPosition + 8);
		return animationCurve;
	}

	private static LayerMask BytesToLayerMask(byte[] bytes, int dataPosition)
	{
		return new LayerMask
		{
			value = BytesToInt(bytes, dataPosition)
		};
	}

	private static UnityEngine.Object IndexToUnityObject(int index, FieldSerializationData activeFieldSerializationData)
	{
		if (index < 0 || index >= activeFieldSerializationData.unityObjects.Count)
		{
			return null;
		}
		return activeFieldSerializationData.unityObjects[index];
	}

	private static SharedVariable BytesToSharedVariable(FieldSerializationData fieldSerializationData, Dictionary<int, int> fieldIndexMap, byte[] bytes, int dataPosition, IVariableSource variableSource, bool fromField, int hashPrefix)
	{
		SharedVariable sharedVariable = null;
		string text = (string)LoadField(fieldSerializationData, fieldIndexMap, typeof(string), "Type", hashPrefix, null);
		if (string.IsNullOrEmpty(text))
		{
			return null;
		}
		string name = (string)LoadField(fieldSerializationData, fieldIndexMap, typeof(string), "Name", hashPrefix, null);
		bool flag = Convert.ToBoolean(LoadField(fieldSerializationData, fieldIndexMap, typeof(bool), "IsShared", hashPrefix, null));
		bool flag2 = Convert.ToBoolean(LoadField(fieldSerializationData, fieldIndexMap, typeof(bool), "IsGlobal", hashPrefix, null));
		if (flag && fromField)
		{
			if (!flag2)
			{
				sharedVariable = variableSource.GetVariable(name);
			}
			else
			{
				if (globalVariables == null)
				{
					globalVariables = GlobalVariables.Instance;
				}
				if (globalVariables != null)
				{
					sharedVariable = globalVariables.GetVariable(name);
				}
			}
		}
		Type typeWithinAssembly = TaskUtility.GetTypeWithinAssembly(text);
		if (typeWithinAssembly == null)
		{
			return null;
		}
		bool flag3 = true;
		if (sharedVariable == null || !(flag3 = sharedVariable.GetType().Equals(typeWithinAssembly)))
		{
			sharedVariable = TaskUtility.CreateInstance(typeWithinAssembly) as SharedVariable;
			if (sharedVariable == null)
			{
				throw new InvalidOperationException($"Behavior Designer could not construct shared variable type '{text}' from serialized variable '{name}'.");
			}
			sharedVariable.Name = name;
			sharedVariable.IsShared = flag;
			sharedVariable.IsGlobal = flag2;
			sharedVariable.NetworkSync = Convert.ToBoolean(LoadField(fieldSerializationData, fieldIndexMap, typeof(bool), "NetworkSync", hashPrefix, null));
			if (!flag2)
			{
				sharedVariable.PropertyMapping = (string)LoadField(fieldSerializationData, fieldIndexMap, typeof(string), "PropertyMapping", hashPrefix, null);
				sharedVariable.PropertyMappingOwner = (GameObject)LoadField(fieldSerializationData, fieldIndexMap, typeof(GameObject), "PropertyMappingOwner", hashPrefix, null);
				sharedVariable.InitializePropertyMapping(variableSource as BehaviorSource);
			}
			if (!flag3)
			{
				sharedVariable.IsShared = true;
			}
			LoadFields(fieldSerializationData, fieldIndexMap, sharedVariable, hashPrefix, variableSource);
		}
		return sharedVariable;
	}
}

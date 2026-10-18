using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;
using Object = UnityEngine.Object;
using Task = BehaviorDesigner.Runtime.Tasks.Task;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace BehaviorDesigner.Runtime;

public class JSONDeserializationDeprecated : UnityEngine.Object
{
	private struct TaskField
	{
		public Task task;

		public FieldInfo fieldInfo;

		public TaskField(Task t, FieldInfo f)
		{
			task = t;
			fieldInfo = f;
		}
	}

	private static Dictionary<TaskField, List<int>> taskIDs = null;

	private static GlobalVariables globalVariables = null;

	private static Dictionary<int, Dictionary<string, object>> serializationCache = new Dictionary<int, Dictionary<string, object>>();

	public static void Load(TaskSerializationData taskData, BehaviorSource behaviorSource)
	{
		behaviorSource.EntryTask = null;
		behaviorSource.RootTask = null;
		behaviorSource.DetachedTasks = null;
		behaviorSource.Variables = null;
		if (!serializationCache.TryGetValue(taskData.JSONSerialization.GetHashCode(), out var value))
		{
			value = MiniJSON.Deserialize(taskData.JSONSerialization) as Dictionary<string, object>;
			serializationCache.Add(taskData.JSONSerialization.GetHashCode(), value);
		}
		if (value == null)
		{
			Debug.Log("Failed to deserialize");
			return;
		}
		taskIDs = new Dictionary<TaskField, List<int>>();
		Dictionary<int, Task> IDtoTask = new Dictionary<int, Task>();
		DeserializeVariables(behaviorSource, value, taskData.fieldSerializationData.unityObjects);
		if (value.ContainsKey("EntryTask"))
		{
			behaviorSource.EntryTask = DeserializeTask(behaviorSource, value["EntryTask"] as Dictionary<string, object>, ref IDtoTask, taskData.fieldSerializationData.unityObjects);
		}
		if (value.ContainsKey("RootTask"))
		{
			behaviorSource.RootTask = DeserializeTask(behaviorSource, value["RootTask"] as Dictionary<string, object>, ref IDtoTask, taskData.fieldSerializationData.unityObjects);
		}
		if (value.ContainsKey("DetachedTasks"))
		{
			List<Task> list = new List<Task>();
			foreach (Dictionary<string, object> item in value["DetachedTasks"] as IEnumerable)
			{
				list.Add(DeserializeTask(behaviorSource, item, ref IDtoTask, taskData.fieldSerializationData.unityObjects));
			}
			behaviorSource.DetachedTasks = list;
		}
		if (taskIDs == null || taskIDs.Count <= 0)
		{
			return;
		}
		foreach (TaskField key in taskIDs.Keys)
		{
			List<int> list2 = taskIDs[key];
			Type fieldType = key.fieldInfo.FieldType;
			if (key.fieldInfo.FieldType.IsArray)
			{
				int num = 0;
				for (int i = 0; i < list2.Count; i++)
				{
					Task task = IDtoTask[list2[i]];
					if (task.GetType().Equals(fieldType.GetElementType()) || task.GetType().IsSubclassOf(fieldType.GetElementType()))
					{
						num++;
					}
				}
				Array array = Array.CreateInstance(fieldType.GetElementType(), num);
				int num2 = 0;
				for (int j = 0; j < list2.Count; j++)
				{
					Task task2 = IDtoTask[list2[j]];
					if (task2.GetType().Equals(fieldType.GetElementType()) || task2.GetType().IsSubclassOf(fieldType.GetElementType()))
					{
						array.SetValue(task2, num2);
						num2++;
					}
				}
				key.fieldInfo.SetValue(key.task, array);
			}
			else
			{
				Task task3 = IDtoTask[list2[0]];
				if (task3.GetType().Equals(key.fieldInfo.FieldType) || task3.GetType().IsSubclassOf(key.fieldInfo.FieldType))
				{
					key.fieldInfo.SetValue(key.task, task3);
				}
			}
		}
		taskIDs = null;
	}

	public static void Load(string serialization, GlobalVariables globalVariables)
	{
		if (globalVariables == null)
		{
			return;
		}
		if (!(MiniJSON.Deserialize(serialization) is Dictionary<string, object> dict))
		{
			Debug.Log("Failed to deserialize");
			return;
		}
		if (globalVariables.VariableData == null)
		{
			globalVariables.VariableData = new VariableSerializationData();
		}
		DeserializeVariables(globalVariables, dict, globalVariables.VariableData.fieldSerializationData.unityObjects);
	}

	private static void DeserializeVariables(IVariableSource variableSource, Dictionary<string, object> dict, List<UnityEngine.Object> unityObjects)
	{
		if (dict.TryGetValue("Variables", out var value))
		{
			List<SharedVariable> list = new List<SharedVariable>();
			IList list2 = value as IList;
			for (int i = 0; i < list2.Count; i++)
			{
				SharedVariable item = DeserializeSharedVariable(list2[i] as Dictionary<string, object>, variableSource, fromSource: true, unityObjects);
				list.Add(item);
			}
			variableSource.SetAllVariables(list);
		}
	}

	public static Task DeserializeTask(BehaviorSource behaviorSource, Dictionary<string, object> dict, ref Dictionary<int, Task> IDtoTask, List<UnityEngine.Object> unityObjects)
	{
		Task task = null;
		try
		{
			Type type = TaskUtility.GetTypeWithinAssembly(dict["ObjectType"] as string);
			if (type == null)
			{
				type = ((!dict.ContainsKey("Children")) ? typeof(UnknownTask) : typeof(UnknownParentTask));
			}
			task = TaskUtility.CreateInstance(type) as Task;
		}
		catch (Exception)
		{
		}
		if (task == null)
		{
			return null;
		}
		task.Owner = behaviorSource.Owner.GetObject() as Behavior;
		task.ID = Convert.ToInt32(dict["ID"]);
		if (dict.TryGetValue("Name", out var value))
		{
			task.FriendlyName = (string)value;
		}
		if (dict.TryGetValue("Instant", out value))
		{
			task.IsInstant = Convert.ToBoolean(value);
		}
		if (dict.TryGetValue("Disabled", out value))
		{
			task.Disabled = Convert.ToBoolean(value);
		}
		IDtoTask.Add(task.ID, task);
		task.NodeData = DeserializeNodeData(dict["NodeData"] as Dictionary<string, object>, task);
		if (task.GetType().Equals(typeof(UnknownTask)) || task.GetType().Equals(typeof(UnknownParentTask)))
		{
			if (!task.FriendlyName.Contains("Unknown "))
			{
				task.FriendlyName = $"Unknown {task.FriendlyName}";
			}
			if (!task.NodeData.Comment.Contains("Loaded from an unknown type. Was a task renamed or deleted?"))
			{
				task.NodeData.Comment = $"Loaded from an unknown type. Was a task renamed or deleted?{((!task.NodeData.Comment.Equals(string.Empty)) ? $"\0{task.NodeData.Comment}" : string.Empty)}";
			}
		}
		DeserializeObject(task, task, dict, behaviorSource, unityObjects);
		if (task is ParentTask && dict.TryGetValue("Children", out value) && task is ParentTask parentTask)
		{
			foreach (Dictionary<string, object> item in value as IEnumerable)
			{
				Task child = DeserializeTask(behaviorSource, item, ref IDtoTask, unityObjects);
				int index = ((parentTask.Children != null) ? parentTask.Children.Count : 0);
				parentTask.AddChild(child, index);
			}
		}
		return task;
	}

	private static NodeData DeserializeNodeData(Dictionary<string, object> dict, Task task)
	{
		NodeData nodeData = new NodeData();
		if (dict.TryGetValue("Offset", out var value))
		{
			nodeData.Offset = StringToVector2((string)value);
		}
		if (dict.TryGetValue("FriendlyName", out value))
		{
			task.FriendlyName = (string)value;
		}
		if (dict.TryGetValue("Comment", out value))
		{
			nodeData.Comment = (string)value;
		}
		if (dict.TryGetValue("IsBreakpoint", out value))
		{
			nodeData.IsBreakpoint = Convert.ToBoolean(value);
		}
		if (dict.TryGetValue("Collapsed", out value))
		{
			nodeData.Collapsed = Convert.ToBoolean(value);
		}
		if (dict.TryGetValue("ColorIndex", out value))
		{
			nodeData.ColorIndex = Convert.ToInt32(value);
		}
		if (dict.TryGetValue("WatchedFields", out value))
		{
			nodeData.WatchedFieldNames = new List<string>();
			nodeData.WatchedFields = new List<FieldInfo>();
			IList list = value as IList;
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
		return nodeData;
	}

	private static SharedVariable DeserializeSharedVariable(Dictionary<string, object> dict, IVariableSource variableSource, bool fromSource, List<UnityEngine.Object> unityObjects)
	{
		if (dict == null)
		{
			return null;
		}
		SharedVariable sharedVariable = null;
		if (!fromSource && variableSource != null && dict.TryGetValue("Name", out var value))
		{
			dict.TryGetValue("IsGlobal", out var value2);
			if (!dict.TryGetValue("IsGlobal", out value2) || !Convert.ToBoolean(value2))
			{
				sharedVariable = variableSource.GetVariable(value as string);
			}
			else
			{
				if (globalVariables == null)
				{
					globalVariables = GlobalVariables.Instance;
				}
				if (globalVariables != null)
				{
					sharedVariable = globalVariables.GetVariable(value as string);
				}
			}
		}
		Type typeWithinAssembly = TaskUtility.GetTypeWithinAssembly(dict["Type"] as string);
		if (typeWithinAssembly == null)
		{
			return null;
		}
		bool flag = true;
		if (sharedVariable == null || !(flag = sharedVariable.GetType().Equals(typeWithinAssembly)))
		{
			sharedVariable = TaskUtility.CreateInstance(typeWithinAssembly) as SharedVariable;
			sharedVariable.Name = dict["Name"] as string;
			if (dict.TryGetValue("IsShared", out var value3))
			{
				sharedVariable.IsShared = Convert.ToBoolean(value3);
			}
			if (dict.TryGetValue("IsGlobal", out value3))
			{
				sharedVariable.IsGlobal = Convert.ToBoolean(value3);
			}
			if (dict.TryGetValue("NetworkSync", out value3))
			{
				sharedVariable.NetworkSync = Convert.ToBoolean(value3);
			}
			if (!sharedVariable.IsGlobal && dict.TryGetValue("PropertyMapping", out value3))
			{
				sharedVariable.PropertyMapping = value3 as string;
				if (dict.TryGetValue("PropertyMappingOwner", out value3))
				{
					sharedVariable.PropertyMappingOwner = IndexToUnityObject(Convert.ToInt32(value3), unityObjects) as GameObject;
				}
				sharedVariable.InitializePropertyMapping(variableSource as BehaviorSource);
			}
			if (!flag)
			{
				sharedVariable.IsShared = true;
			}
			DeserializeObject(null, sharedVariable, dict, variableSource, unityObjects);
		}
		return sharedVariable;
	}

	private static void DeserializeObject(Task task, object obj, Dictionary<string, object> dict, IVariableSource variableSource, List<UnityEngine.Object> unityObjects)
	{
		if (dict == null)
		{
			return;
		}
		FieldInfo[] allFields = TaskUtility.GetAllFields(obj.GetType());
		for (int i = 0; i < allFields.Length; i++)
		{
			if (dict.TryGetValue(string.Concat(allFields[i].FieldType, ",", allFields[i].Name), out var value) || dict.TryGetValue(allFields[i].Name, out value))
			{
				if (typeof(IList).IsAssignableFrom(allFields[i].FieldType))
				{
					if (!(value is IList list))
					{
						continue;
					}
					Type type;
					if (allFields[i].FieldType.IsArray)
					{
						type = allFields[i].FieldType.GetElementType();
					}
					else
					{
						Type type2 = allFields[i].FieldType;
						while (!type2.IsGenericType)
						{
							type2 = type2.BaseType;
						}
						type = type2.GetGenericArguments()[0];
					}
					if (type.Equals(typeof(Task)) || type.IsSubclassOf(typeof(Task)))
					{
						if (taskIDs != null)
						{
							List<int> list2 = new List<int>();
							for (int j = 0; j < list.Count; j++)
							{
								list2.Add(Convert.ToInt32(list[j]));
							}
							taskIDs.Add(new TaskField(task, allFields[i]), list2);
						}
					}
					else if (allFields[i].FieldType.IsArray)
					{
						Array array = Array.CreateInstance(type, list.Count);
						for (int k = 0; k < list.Count; k++)
						{
							array.SetValue(ValueToObject(task, type, list[k], variableSource, unityObjects), k);
						}
						allFields[i].SetValue(obj, array);
					}
					else
					{
						IList list3 = ((!allFields[i].FieldType.IsGenericType) ? (TaskUtility.CreateInstance(allFields[i].FieldType) as IList) : (TaskUtility.CreateInstance(typeof(List<>).MakeGenericType(type)) as IList));
						for (int l = 0; l < list.Count; l++)
						{
							list3.Add(ValueToObject(task, type, list[l], variableSource, unityObjects));
						}
						allFields[i].SetValue(obj, list3);
					}
					continue;
				}
				Type fieldType = allFields[i].FieldType;
				if (fieldType.Equals(typeof(Task)) || fieldType.IsSubclassOf(typeof(Task)))
				{
					if (TaskUtility.HasAttribute(allFields[i], typeof(InspectTaskAttribute)))
					{
						Dictionary<string, object> dictionary = value as Dictionary<string, object>;
						Type typeWithinAssembly = TaskUtility.GetTypeWithinAssembly(dictionary["ObjectType"] as string);
						if (typeWithinAssembly != null)
						{
							Task task2 = TaskUtility.CreateInstance(typeWithinAssembly) as Task;
							DeserializeObject(task2, task2, dictionary, variableSource, unityObjects);
							allFields[i].SetValue(task, task2);
						}
					}
					else if (taskIDs != null)
					{
						List<int> list4 = new List<int>();
						list4.Add(Convert.ToInt32(value));
						taskIDs.Add(new TaskField(task, allFields[i]), list4);
					}
				}
				else
				{
					allFields[i].SetValue(obj, ValueToObject(task, fieldType, value, variableSource, unityObjects));
				}
			}
			else if (typeof(SharedVariable).IsAssignableFrom(allFields[i].FieldType) && !allFields[i].FieldType.IsAbstract)
			{
				if (dict.TryGetValue(string.Concat(allFields[i].FieldType, ",", allFields[i].Name), out value))
				{
					SharedVariable sharedVariable = TaskUtility.CreateInstance(allFields[i].FieldType) as SharedVariable;
					sharedVariable.SetValue(ValueToObject(task, allFields[i].FieldType, value, variableSource, unityObjects));
					allFields[i].SetValue(obj, sharedVariable);
				}
				else
				{
					SharedVariable value2 = TaskUtility.CreateInstance(allFields[i].FieldType) as SharedVariable;
					allFields[i].SetValue(obj, value2);
				}
			}
		}
	}

	private static object ValueToObject(Task task, Type type, object obj, IVariableSource variableSource, List<UnityEngine.Object> unityObjects)
	{
		if (type.Equals(typeof(SharedVariable)) || type.IsSubclassOf(typeof(SharedVariable)))
		{
			SharedVariable sharedVariable = DeserializeSharedVariable(obj as Dictionary<string, object>, variableSource, fromSource: false, unityObjects);
			if (sharedVariable == null)
			{
				sharedVariable = TaskUtility.CreateInstance(type) as SharedVariable;
			}
			return sharedVariable;
		}
		if (type.Equals(typeof(UnityEngine.Object)) || type.IsSubclassOf(typeof(UnityEngine.Object)))
		{
			return IndexToUnityObject(Convert.ToInt32(obj), unityObjects);
		}
		if (type.IsPrimitive || type.Equals(typeof(string)))
		{
			try
			{
				return Convert.ChangeType(obj, type);
			}
			catch (Exception)
			{
				return null;
			}
		}
		if (type.IsSubclassOf(typeof(Enum)))
		{
			try
			{
				return Enum.Parse(type, (string)obj);
			}
			catch (Exception)
			{
				return null;
			}
		}
		if (type.Equals(typeof(Vector2)))
		{
			return StringToVector2((string)obj);
		}
		if (type.Equals(typeof(Vector3)))
		{
			return StringToVector3((string)obj);
		}
		if (type.Equals(typeof(Vector4)))
		{
			return StringToVector4((string)obj);
		}
		if (type.Equals(typeof(Quaternion)))
		{
			return StringToQuaternion((string)obj);
		}
		if (type.Equals(typeof(Matrix4x4)))
		{
			return StringToMatrix4x4((string)obj);
		}
		if (type.Equals(typeof(Color)))
		{
			return StringToColor((string)obj);
		}
		if (type.Equals(typeof(Rect)))
		{
			return StringToRect((string)obj);
		}
		if (type.Equals(typeof(LayerMask)))
		{
			return ValueToLayerMask(Convert.ToInt32(obj));
		}
		if (type.Equals(typeof(AnimationCurve)))
		{
			return ValueToAnimationCurve((Dictionary<string, object>)obj);
		}
		object obj2 = TaskUtility.CreateInstance(type);
		DeserializeObject(task, obj2, obj as Dictionary<string, object>, variableSource, unityObjects);
		return obj2;
	}

	private static Vector2 StringToVector2(string vector2String)
	{
		string[] array = vector2String.Substring(1, vector2String.Length - 2).Split(',');
		return new Vector2(float.Parse(array[0]), float.Parse(array[1]));
	}

	private static Vector3 StringToVector3(string vector3String)
	{
		string[] array = vector3String.Substring(1, vector3String.Length - 2).Split(',');
		return new Vector3(float.Parse(array[0]), float.Parse(array[1]), float.Parse(array[2]));
	}

	private static Vector4 StringToVector4(string vector4String)
	{
		string[] array = vector4String.Substring(1, vector4String.Length - 2).Split(',');
		return new Vector4(float.Parse(array[0]), float.Parse(array[1]), float.Parse(array[2]), float.Parse(array[3]));
	}

	private static Quaternion StringToQuaternion(string quaternionString)
	{
		string[] array = quaternionString.Substring(1, quaternionString.Length - 2).Split(',');
		return new Quaternion(float.Parse(array[0]), float.Parse(array[1]), float.Parse(array[2]), float.Parse(array[3]));
	}

	private static Matrix4x4 StringToMatrix4x4(string matrixString)
	{
		string[] array = matrixString.Split(null);
		return new Matrix4x4
		{
			m00 = float.Parse(array[0]),
			m01 = float.Parse(array[1]),
			m02 = float.Parse(array[2]),
			m03 = float.Parse(array[3]),
			m10 = float.Parse(array[4]),
			m11 = float.Parse(array[5]),
			m12 = float.Parse(array[6]),
			m13 = float.Parse(array[7]),
			m20 = float.Parse(array[8]),
			m21 = float.Parse(array[9]),
			m22 = float.Parse(array[10]),
			m23 = float.Parse(array[11]),
			m30 = float.Parse(array[12]),
			m31 = float.Parse(array[13]),
			m32 = float.Parse(array[14]),
			m33 = float.Parse(array[15])
		};
	}

	private static Color StringToColor(string colorString)
	{
		string[] array = colorString.Substring(5, colorString.Length - 6).Split(',');
		return new Color(float.Parse(array[0]), float.Parse(array[1]), float.Parse(array[2]), float.Parse(array[3]));
	}

	private static Rect StringToRect(string rectString)
	{
		string[] array = rectString.Substring(1, rectString.Length - 2).Split(',');
		return new Rect(float.Parse(array[0].Substring(2, array[0].Length - 2)), float.Parse(array[1].Substring(3, array[1].Length - 3)), float.Parse(array[2].Substring(7, array[2].Length - 7)), float.Parse(array[3].Substring(8, array[3].Length - 8)));
	}

	private static LayerMask ValueToLayerMask(int value)
	{
		return new LayerMask
		{
			value = value
		};
	}

	private static AnimationCurve ValueToAnimationCurve(Dictionary<string, object> value)
	{
		AnimationCurve animationCurve = new AnimationCurve();
		if (value.TryGetValue("Keys", out var value2))
		{
			List<object> list = value2 as List<object>;
			for (int i = 0; i < list.Count; i++)
			{
				List<object> list2 = list[i] as List<object>;
				Keyframe key = new Keyframe((float)Convert.ChangeType(list2[0], typeof(float)), (float)Convert.ChangeType(list2[1], typeof(float)), (float)Convert.ChangeType(list2[2], typeof(float)), (float)Convert.ChangeType(list2[3], typeof(float)));
				key.tangentMode = (int)Convert.ChangeType(list2[4], typeof(int));
				animationCurve.AddKey(key);
			}
		}
		if (value.TryGetValue("PreWrapMode", out value2))
		{
			animationCurve.preWrapMode = (WrapMode)(int)Enum.Parse(typeof(WrapMode), (string)value2);
		}
		if (value.TryGetValue("PostWrapMode", out value2))
		{
			animationCurve.postWrapMode = (WrapMode)(int)Enum.Parse(typeof(WrapMode), (string)value2);
		}
		return animationCurve;
	}

	private static UnityEngine.Object IndexToUnityObject(int index, List<UnityEngine.Object> unityObjects)
	{
		if (index < 0 || index >= unityObjects.Count)
		{
			return null;
		}
		return unityObjects[index];
	}
}

using System;
using System.Collections.Generic;
using FullSerializer.Internal;

namespace FullSerializer
{
	public class fsSerializer
	{
		internal class fsLazyCycleDefinitionWriter
		{
			private Dictionary<int, fsData> _pendingDefinitions = new Dictionary<int, fsData>();

			private HashSet<int> _references = new HashSet<int>();

			public void WriteDefinition(int id, fsData data)
			{
				if (_references.Contains(id))
				{
					EnsureDictionary(data);
					data.AsDictionary[Key_ObjectDefinition] = new fsData(id.ToString());
				}
				else
				{
					_pendingDefinitions[id] = data;
				}
			}

			public void WriteReference(int id, Dictionary<string, fsData> dict)
			{
				if (_pendingDefinitions.ContainsKey(id))
				{
					fsData fsData2 = _pendingDefinitions[id];
					EnsureDictionary(fsData2);
					fsData2.AsDictionary[Key_ObjectDefinition] = new fsData(id.ToString());
					_pendingDefinitions.Remove(id);
				}
				else
				{
					_references.Add(id);
				}
				dict[Key_ObjectReference] = new fsData(id.ToString());
			}

			public void Clear()
			{
				_pendingDefinitions.Clear();
				_references.Clear();
			}
		}

		private static HashSet<string> _reservedKeywords;

		private static readonly string Key_ObjectReference;

		private static readonly string Key_ObjectDefinition;

		private static readonly string Key_InstanceType;

		private static readonly string Key_Version;

		private static readonly string Key_Content;

		private Dictionary<Type, fsBaseConverter> _cachedConverterTypeInstances;

		private Dictionary<Type, fsBaseConverter> _cachedConverters;

		private Dictionary<Type, List<fsObjectProcessor>> _cachedProcessors;

		private readonly List<fsConverter> _availableConverters;

		private readonly Dictionary<Type, fsDirectConverter> _availableDirectConverters;

		private readonly List<fsObjectProcessor> _processors;

		private readonly fsCyclicReferenceManager _references;

		private readonly fsLazyCycleDefinitionWriter _lazyReferenceWriter;

		private readonly Dictionary<Type, Type> _abstractTypeRemap;

		public fsContext Context;

		public fsConfig Config;

		static fsSerializer()
		{
			Key_ObjectReference = string.Format("{0}ref", fsGlobalConfig.InternalFieldPrefix);
			Key_ObjectDefinition = string.Format("{0}id", fsGlobalConfig.InternalFieldPrefix);
			Key_InstanceType = string.Format("{0}type", fsGlobalConfig.InternalFieldPrefix);
			Key_Version = string.Format("{0}version", fsGlobalConfig.InternalFieldPrefix);
			Key_Content = string.Format("{0}content", fsGlobalConfig.InternalFieldPrefix);
			_reservedKeywords = new HashSet<string> { Key_ObjectReference, Key_ObjectDefinition, Key_InstanceType, Key_Version, Key_Content };
		}

		public fsSerializer()
		{
			_cachedConverterTypeInstances = new Dictionary<Type, fsBaseConverter>();
			_cachedConverters = new Dictionary<Type, fsBaseConverter>();
			_cachedProcessors = new Dictionary<Type, List<fsObjectProcessor>>();
			_references = new fsCyclicReferenceManager();
			_lazyReferenceWriter = new fsLazyCycleDefinitionWriter();
			_availableConverters = new List<fsConverter>
			{
				new fsNullableConverter
				{
					Serializer = this
				},
				new fsGuidConverter
				{
					Serializer = this
				},
				new fsTypeConverter
				{
					Serializer = this
				},
				new fsDateConverter
				{
					Serializer = this
				},
				new fsEnumConverter
				{
					Serializer = this
				},
				new fsPrimitiveConverter
				{
					Serializer = this
				},
				new fsArrayConverter
				{
					Serializer = this
				},
				new fsDictionaryConverter
				{
					Serializer = this
				},
				new fsIEnumerableConverter
				{
					Serializer = this
				},
				new fsKeyValuePairConverter
				{
					Serializer = this
				},
				new fsWeakReferenceConverter
				{
					Serializer = this
				},
				new fsReflectedConverter
				{
					Serializer = this
				}
			};
			_availableDirectConverters = new Dictionary<Type, fsDirectConverter>();
			_processors = new List<fsObjectProcessor>
			{
				new fsSerializationCallbackProcessor()
			};
			_processors.Add(new fsSerializationCallbackReceiverProcessor());
			_abstractTypeRemap = new Dictionary<Type, Type>();
			SetDefaultStorageType(typeof(ICollection<>), typeof(List<>));
			SetDefaultStorageType(typeof(IList<>), typeof(List<>));
			SetDefaultStorageType(typeof(IDictionary<, >), typeof(Dictionary<, >));
			Context = new fsContext();
			Config = new fsConfig();
			foreach (Type converter in fsConverterRegistrar.Converters)
			{
				AddConverter((fsBaseConverter)Activator.CreateInstance(converter));
			}
		}

		public static bool IsReservedKeyword(string key)
		{
			return _reservedKeywords.Contains(key);
		}

		private static bool IsObjectReference(fsData data)
		{
			if (!data.IsDictionary)
			{
				return false;
			}
			return data.AsDictionary.ContainsKey(Key_ObjectReference);
		}

		private static bool IsObjectDefinition(fsData data)
		{
			if (!data.IsDictionary)
			{
				return false;
			}
			return data.AsDictionary.ContainsKey(Key_ObjectDefinition);
		}

		private static bool IsVersioned(fsData data)
		{
			if (!data.IsDictionary)
			{
				return false;
			}
			return data.AsDictionary.ContainsKey(Key_Version);
		}

		private static bool IsTypeSpecified(fsData data)
		{
			if (!data.IsDictionary)
			{
				return false;
			}
			return data.AsDictionary.ContainsKey(Key_InstanceType);
		}

		private static bool IsWrappedData(fsData data)
		{
			if (!data.IsDictionary)
			{
				return false;
			}
			return data.AsDictionary.ContainsKey(Key_Content);
		}

		public static void StripDeserializationMetadata(ref fsData data)
		{
			if (data.IsDictionary && data.AsDictionary.ContainsKey(Key_Content))
			{
				data = data.AsDictionary[Key_Content];
			}
			if (data.IsDictionary)
			{
				Dictionary<string, fsData> asDictionary = data.AsDictionary;
				asDictionary.Remove(Key_ObjectReference);
				asDictionary.Remove(Key_ObjectDefinition);
				asDictionary.Remove(Key_InstanceType);
				asDictionary.Remove(Key_Version);
			}
		}

		private static void ConvertLegacyData(ref fsData data)
		{
			if (!data.IsDictionary)
			{
				return;
			}
			Dictionary<string, fsData> asDictionary = data.AsDictionary;
			if (asDictionary.Count <= 2)
			{
				string key = "ReferenceId";
				string key2 = "SourceId";
				string key3 = "Data";
				string key4 = "Type";
				string key5 = "Data";
				if (asDictionary.Count == 2 && asDictionary.ContainsKey(key4) && asDictionary.ContainsKey(key5))
				{
					data = asDictionary[key5];
					EnsureDictionary(data);
					ConvertLegacyData(ref data);
					data.AsDictionary[Key_InstanceType] = asDictionary[key4];
				}
				else if (asDictionary.Count == 2 && asDictionary.ContainsKey(key2) && asDictionary.ContainsKey(key3))
				{
					data = asDictionary[key3];
					EnsureDictionary(data);
					ConvertLegacyData(ref data);
					data.AsDictionary[Key_ObjectDefinition] = asDictionary[key2];
				}
				else if (asDictionary.Count == 1 && asDictionary.ContainsKey(key))
				{
					data = fsData.CreateDictionary();
					data.AsDictionary[Key_ObjectReference] = asDictionary[key];
				}
			}
		}

		private static void Invoke_OnBeforeSerialize(List<fsObjectProcessor> processors, Type storageType, object instance)
		{
			for (int i = 0; i < processors.Count; i++)
			{
				processors[i].OnBeforeSerialize(storageType, instance);
			}
		}

		private static void Invoke_OnAfterSerialize(List<fsObjectProcessor> processors, Type storageType, object instance, ref fsData data)
		{
			for (int num = processors.Count - 1; num >= 0; num--)
			{
				processors[num].OnAfterSerialize(storageType, instance, ref data);
			}
		}

		private static void Invoke_OnBeforeDeserialize(List<fsObjectProcessor> processors, Type storageType, ref fsData data)
		{
			for (int i = 0; i < processors.Count; i++)
			{
				processors[i].OnBeforeDeserialize(storageType, ref data);
			}
		}

		private static void Invoke_OnBeforeDeserializeAfterInstanceCreation(List<fsObjectProcessor> processors, Type storageType, object instance, ref fsData data)
		{
			for (int i = 0; i < processors.Count; i++)
			{
				processors[i].OnBeforeDeserializeAfterInstanceCreation(storageType, instance, ref data);
			}
		}

		private static void Invoke_OnAfterDeserialize(List<fsObjectProcessor> processors, Type storageType, object instance)
		{
			for (int num = processors.Count - 1; num >= 0; num--)
			{
				processors[num].OnAfterDeserialize(storageType, instance);
			}
		}

		private static void EnsureDictionary(fsData data)
		{
			if (!data.IsDictionary)
			{
				fsData value = data.Clone();
				data.BecomeDictionary();
				data.AsDictionary[Key_Content] = value;
			}
		}

		private void RemapAbstractStorageTypeToDefaultType(ref Type storageType)
		{
			if (!storageType.IsInterface && !storageType.IsAbstract)
			{
				return;
			}
			Type value2;
			if (storageType.IsGenericType)
			{
				Type value;
				if (_abstractTypeRemap.TryGetValue(storageType.GetGenericTypeDefinition(), out value))
				{
					Type[] genericArguments = storageType.GetGenericArguments();
					storageType = value.MakeGenericType(genericArguments);
				}
			}
			else if (_abstractTypeRemap.TryGetValue(storageType, out value2))
			{
				storageType = value2;
			}
		}

		public void AddProcessor(fsObjectProcessor processor)
		{
			_processors.Add(processor);
			_cachedProcessors = new Dictionary<Type, List<fsObjectProcessor>>();
		}

		public void RemoveProcessor<TProcessor>()
		{
			int num = 0;
			while (num < _processors.Count)
			{
				if (_processors[num] is TProcessor)
				{
					_processors.RemoveAt(num);
				}
				else
				{
					num++;
				}
			}
			_cachedProcessors = new Dictionary<Type, List<fsObjectProcessor>>();
		}

		public void SetDefaultStorageType(Type abstractType, Type defaultStorageType)
		{
			if (!abstractType.IsInterface && !abstractType.IsAbstract)
			{
				throw new ArgumentException("|abstractType| must be an interface or abstract type");
			}
			_abstractTypeRemap[abstractType] = defaultStorageType;
		}

		private List<fsObjectProcessor> GetProcessors(Type type)
		{
			fsObjectAttribute attribute = fsPortableReflection.GetAttribute<fsObjectAttribute>(type);
			List<fsObjectProcessor> value;
			if (attribute != null && attribute.Processor != null)
			{
				fsObjectProcessor item = (fsObjectProcessor)Activator.CreateInstance(attribute.Processor);
				value = new List<fsObjectProcessor>();
				value.Add(item);
				_cachedProcessors[type] = value;
			}
			else if (!_cachedProcessors.TryGetValue(type, out value))
			{
				value = new List<fsObjectProcessor>();
				for (int i = 0; i < _processors.Count; i++)
				{
					fsObjectProcessor fsObjectProcessor2 = _processors[i];
					if (fsObjectProcessor2.CanProcess(type))
					{
						value.Add(fsObjectProcessor2);
					}
				}
				_cachedProcessors[type] = value;
			}
			return value;
		}

		public void AddConverter(fsBaseConverter converter)
		{
			if (converter.Serializer != null)
			{
				throw new InvalidOperationException("Cannot add a single converter instance to multiple fsConverters -- please construct a new instance for " + converter);
			}
			if (converter is fsDirectConverter)
			{
				fsDirectConverter fsDirectConverter2 = (fsDirectConverter)converter;
				_availableDirectConverters[fsDirectConverter2.ModelType] = fsDirectConverter2;
			}
			else
			{
				if (!(converter is fsConverter))
				{
					throw new InvalidOperationException(string.Concat("Unable to add converter ", converter, "; the type association strategy is unknown. Please use either fsDirectConverter or fsConverter as your base type."));
				}
				_availableConverters.Insert(0, (fsConverter)converter);
			}
			converter.Serializer = this;
			_cachedConverters = new Dictionary<Type, fsBaseConverter>();
		}

		private fsBaseConverter GetConverter(Type type, Type overrideConverterType)
		{
			if (overrideConverterType != null)
			{
				fsBaseConverter value;
				if (!_cachedConverterTypeInstances.TryGetValue(overrideConverterType, out value))
				{
					value = (fsBaseConverter)Activator.CreateInstance(overrideConverterType);
					value.Serializer = this;
					_cachedConverterTypeInstances[overrideConverterType] = value;
				}
				return value;
			}
			fsBaseConverter value2;
			if (_cachedConverters.TryGetValue(type, out value2))
			{
				return value2;
			}
			fsObjectAttribute attribute = fsPortableReflection.GetAttribute<fsObjectAttribute>(type);
			if (attribute != null && attribute.Converter != null)
			{
				value2 = (fsBaseConverter)Activator.CreateInstance(attribute.Converter);
				value2.Serializer = this;
				fsBaseConverter fsBaseConverter2 = value2;
				_cachedConverters[type] = fsBaseConverter2;
				return fsBaseConverter2;
			}
			fsForwardAttribute attribute2 = fsPortableReflection.GetAttribute<fsForwardAttribute>(type);
			if (attribute2 != null)
			{
				value2 = new fsForwardConverter(attribute2);
				value2.Serializer = this;
				fsBaseConverter fsBaseConverter2 = value2;
				_cachedConverters[type] = fsBaseConverter2;
				return fsBaseConverter2;
			}
			if (!_cachedConverters.TryGetValue(type, out value2))
			{
				if (_availableDirectConverters.ContainsKey(type))
				{
					value2 = _availableDirectConverters[type];
					fsBaseConverter fsBaseConverter2 = value2;
					_cachedConverters[type] = fsBaseConverter2;
					return fsBaseConverter2;
				}
				for (int i = 0; i < _availableConverters.Count; i++)
				{
					if (_availableConverters[i].CanProcess(type))
					{
						value2 = _availableConverters[i];
						fsBaseConverter fsBaseConverter2 = value2;
						_cachedConverters[type] = fsBaseConverter2;
						return fsBaseConverter2;
					}
				}
			}
			throw new InvalidOperationException("Internal error -- could not find a converter for " + type);
		}

		public fsResult TrySerialize<T>(T instance, out fsData data)
		{
			return TrySerialize(typeof(T), instance, out data);
		}

		public fsResult TryDeserialize<T>(fsData data, ref T instance)
		{
			object result = instance;
			fsResult result2 = TryDeserialize(data, typeof(T), ref result);
			if (result2.Succeeded)
			{
				instance = (T)result;
			}
			return result2;
		}

		public fsResult TrySerialize(Type storageType, object instance, out fsData data)
		{
			return TrySerialize(storageType, null, instance, out data);
		}

		public fsResult TrySerialize(Type storageType, Type overrideConverterType, object instance, out fsData data)
		{
			List<fsObjectProcessor> processors = GetProcessors((instance != null) ? instance.GetType() : storageType);
			Invoke_OnBeforeSerialize(processors, storageType, instance);
			if (object.ReferenceEquals(instance, null))
			{
				data = new fsData();
				Invoke_OnAfterSerialize(processors, storageType, instance, ref data);
				return fsResult.Success;
			}
			fsResult result = InternalSerialize_1_ProcessCycles(storageType, overrideConverterType, instance, out data);
			Invoke_OnAfterSerialize(processors, storageType, instance, ref data);
			return result;
		}

		private fsResult InternalSerialize_1_ProcessCycles(Type storageType, Type overrideConverterType, object instance, out fsData data)
		{
			try
			{
				_references.Enter();
				fsBaseConverter converter = GetConverter(instance.GetType(), overrideConverterType);
				if (!converter.RequestCycleSupport(instance.GetType()))
				{
					return InternalSerialize_2_Inheritance(storageType, overrideConverterType, instance, out data);
				}
				if (_references.IsReference(instance))
				{
					data = fsData.CreateDictionary();
					_lazyReferenceWriter.WriteReference(_references.GetReferenceId(instance), data.AsDictionary);
					return fsResult.Success;
				}
				_references.MarkSerialized(instance);
				fsResult result = InternalSerialize_2_Inheritance(storageType, overrideConverterType, instance, out data);
				if (result.Failed)
				{
					return result;
				}
				_lazyReferenceWriter.WriteDefinition(_references.GetReferenceId(instance), data);
				return result;
			}
			finally
			{
				if (_references.Exit())
				{
					_lazyReferenceWriter.Clear();
				}
			}
		}

		private fsResult InternalSerialize_2_Inheritance(Type storageType, Type overrideConverterType, object instance, out fsData data)
		{
			fsResult result = InternalSerialize_3_ProcessVersioning(overrideConverterType, instance, out data);
			if (result.Failed)
			{
				return result;
			}
			if (storageType != instance.GetType() && GetConverter(storageType, overrideConverterType).RequestInheritanceSupport(storageType))
			{
				EnsureDictionary(data);
				data.AsDictionary[Key_InstanceType] = new fsData(instance.GetType().FullName);
			}
			return result;
		}

		private fsResult InternalSerialize_3_ProcessVersioning(Type overrideConverterType, object instance, out fsData data)
		{
			fsOption<fsVersionedType> versionedType = fsVersionManager.GetVersionedType(instance.GetType());
			if (versionedType.HasValue)
			{
				fsVersionedType value = versionedType.Value;
				fsResult result = InternalSerialize_4_Converter(overrideConverterType, instance, out data);
				if (result.Failed)
				{
					return result;
				}
				EnsureDictionary(data);
				data.AsDictionary[Key_Version] = new fsData(value.VersionString);
				return result;
			}
			return InternalSerialize_4_Converter(overrideConverterType, instance, out data);
		}

		private fsResult InternalSerialize_4_Converter(Type overrideConverterType, object instance, out fsData data)
		{
			Type type = instance.GetType();
			return GetConverter(type, overrideConverterType).TrySerialize(instance, out data, type);
		}

		public fsResult TryDeserialize(fsData data, Type storageType, ref object result)
		{
			return TryDeserialize(data, storageType, null, ref result);
		}

		public fsResult TryDeserialize(fsData data, Type storageType, Type overrideConverterType, ref object result)
		{
			if (data.IsNull)
			{
				result = null;
				List<fsObjectProcessor> processors = GetProcessors(storageType);
				Invoke_OnBeforeDeserialize(processors, storageType, ref data);
				Invoke_OnAfterDeserialize(processors, storageType, null);
				return fsResult.Success;
			}
			ConvertLegacyData(ref data);
			try
			{
				_references.Enter();
				List<fsObjectProcessor> processors2;
				fsResult result2 = InternalDeserialize_1_CycleReference(overrideConverterType, data, storageType, ref result, out processors2);
				if (result2.Succeeded)
				{
					Invoke_OnAfterDeserialize(processors2, storageType, result);
				}
				return result2;
			}
			finally
			{
				_references.Exit();
			}
		}

		private fsResult InternalDeserialize_1_CycleReference(Type overrideConverterType, fsData data, Type storageType, ref object result, out List<fsObjectProcessor> processors)
		{
			if (IsObjectReference(data))
			{
				int id = int.Parse(data.AsDictionary[Key_ObjectReference].AsString);
				result = _references.GetReferenceObject(id);
				processors = GetProcessors(result.GetType());
				return fsResult.Success;
			}
			return InternalDeserialize_2_Version(overrideConverterType, data, storageType, ref result, out processors);
		}

		private fsResult InternalDeserialize_2_Version(Type overrideConverterType, fsData data, Type storageType, ref object result, out List<fsObjectProcessor> processors)
		{
			if (IsVersioned(data))
			{
				string asString = data.AsDictionary[Key_Version].AsString;
				fsOption<fsVersionedType> versionedType = fsVersionManager.GetVersionedType(storageType);
				if (versionedType.HasValue && versionedType.Value.VersionString != asString)
				{
					fsResult success = fsResult.Success;
					List<fsVersionedType> path;
					success += fsVersionManager.GetVersionImportPath(asString, versionedType.Value, out path);
					if (success.Failed)
					{
						processors = GetProcessors(storageType);
						return success;
					}
					success += InternalDeserialize_3_Inheritance(overrideConverterType, data, path[0].ModelType, ref result, out processors);
					if (success.Failed)
					{
						return success;
					}
					for (int i = 1; i < path.Count; i++)
					{
						result = path[i].Migrate(result);
					}
					if (IsObjectDefinition(data))
					{
						int id = int.Parse(data.AsDictionary[Key_ObjectDefinition].AsString);
						_references.AddReferenceWithId(id, result);
					}
					processors = GetProcessors(success.GetType());
					return success;
				}
			}
			return InternalDeserialize_3_Inheritance(overrideConverterType, data, storageType, ref result, out processors);
		}

		private fsResult InternalDeserialize_3_Inheritance(Type overrideConverterType, fsData data, Type storageType, ref object result, out List<fsObjectProcessor> processors)
		{
			fsResult success = fsResult.Success;
			Type storageType2 = storageType;
			if (IsTypeSpecified(data))
			{
				fsData fsData2 = data.AsDictionary[Key_InstanceType];
				if (!fsData2.IsString)
				{
					success.AddMessage(string.Concat(Key_InstanceType, " value must be a string (in ", data, ")"));
				}
				else
				{
					string asString = fsData2.AsString;
					Type type = fsTypeCache.GetType(asString);
					if (type == null)
					{
						success += fsResult.Fail("Unable to locate specified type \"" + asString + "\"");
					}
					else if (!storageType.IsAssignableFrom(type))
					{
						success.AddMessage(string.Concat("Ignoring type specifier; a field/property of type ", storageType, " cannot hold an instance of ", type));
					}
					else
					{
						storageType2 = type;
					}
				}
			}
			RemapAbstractStorageTypeToDefaultType(ref storageType2);
			processors = GetProcessors(storageType2);
			if (success.Failed)
			{
				return success;
			}
			Invoke_OnBeforeDeserialize(processors, storageType, ref data);
			if (object.ReferenceEquals(result, null) || result.GetType() != storageType2)
			{
				result = GetConverter(storageType2, overrideConverterType).CreateInstance(data, storageType2);
			}
			Invoke_OnBeforeDeserializeAfterInstanceCreation(processors, storageType, result, ref data);
			return success += InternalDeserialize_4_Cycles(overrideConverterType, data, storageType2, ref result);
		}

		private fsResult InternalDeserialize_4_Cycles(Type overrideConverterType, fsData data, Type resultType, ref object result)
		{
			if (IsObjectDefinition(data))
			{
				int id = int.Parse(data.AsDictionary[Key_ObjectDefinition].AsString);
				_references.AddReferenceWithId(id, result);
			}
			return InternalDeserialize_5_Converter(overrideConverterType, data, resultType, ref result);
		}

		private fsResult InternalDeserialize_5_Converter(Type overrideConverterType, fsData data, Type resultType, ref object result)
		{
			if (IsWrappedData(data))
			{
				data = data.AsDictionary[Key_Content];
			}
			return GetConverter(resultType, overrideConverterType).TryDeserialize(data, ref result, resultType);
		}
	}
}

using System;
using UnityEngine;
using UnityEngine.Events;

namespace FullSerializer.Internal.Converters
{
	public class UnityEvent_Converter : fsConverter
	{
		public override bool CanProcess(Type type)
		{
			return typeof(UnityEvent).Resolve().IsAssignableFrom(type) && !type.IsGenericType;
		}

		public override bool RequestCycleSupport(Type storageType)
		{
			return false;
		}

		public override fsResult TryDeserialize(fsData data, ref object instance, Type storageType)
		{
			Type type = (Type)instance;
			fsResult success = fsResult.Success;
			instance = JsonUtility.FromJson(fsJsonPrinter.CompressedJson(data), type);
			return success;
		}

		public override fsResult TrySerialize(object instance, out fsData serialized, Type storageType)
		{
			fsResult success = fsResult.Success;
			serialized = fsJsonParser.Parse(JsonUtility.ToJson(instance));
			return success;
		}
	}
}

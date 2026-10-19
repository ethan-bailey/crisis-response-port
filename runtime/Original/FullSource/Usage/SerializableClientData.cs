using Crisis.Runtime.FullSource;
using System;
using FullSerializer;
using UnityEngine;

namespace Usage
{
	[fsObject("1", new Type[] { })]
	public class SerializableClientData
	{
		[fsIgnore]
		public const string Filename = "main.dat";

		public bool recordData = true;

		public bool reportedHardware;

		public uint previousSessionId;

		public bool previousSessionClosed = true;

		public void Overwrite(SerializableClientData other)
		{
			recordData = other.recordData;
			reportedHardware = other.reportedHardware;
			previousSessionId = other.previousSessionId;
			previousSessionClosed = other.previousSessionClosed;
		}

		public void Save()
		{
			if (!Serializer.SerializeToDisk(this, typeof(SerializableClientData), Application.persistentDataPath + "/main.dat"))
			{
				Debug.LogWarning("Failed to save main settings.");
			}
		}

		public void Load()
		{
			SerializableClientData serializableClientData = (SerializableClientData)Serializer.DeserializeFromDisk(typeof(SerializableClientData), Application.persistentDataPath + "/main.dat");
			if (serializableClientData == null)
			{
				Debug.LogWarning("Failed to load main settings.");
			}
			else
			{
				Overwrite(serializableClientData);
			}
		}
	}
}

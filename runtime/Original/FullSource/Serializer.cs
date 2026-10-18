namespace Crisis.Runtime.FullSource
{
using Crisis.Runtime.Weapons;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using FullSerializer;

public static class Serializer
{
	private static readonly fsSerializer serializer = new fsSerializer();

	public static bool SerializeToDisk(object data, Type type, string path)
	{
		try
		{
			string graph = SerializeToString(type, data);
			using (FileStream serializationStream = File.Create(path))
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				binaryFormatter.Serialize(serializationStream, graph);
				return true;
			}
		}
		catch (Exception)
		{
			return false;
		}
	}

	public static object DeserializeFromDisk(Type type, string path)
	{
		if (File.Exists(path))
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			using (FileStream serializationStream = File.Open(path, FileMode.Open))
			{
				string encryptedState = (string)binaryFormatter.Deserialize(serializationStream);
				return DeserializeToString(type, encryptedState);
			}
		}
		throw new IOException(string.Format("File does not exist: {0}", path));
	}

	private static string SerializeToString(Type type, object value)
	{
		fsData data;
		serializer.TrySerialize(type, value, out data).AssertSuccessWithoutWarnings();
		string prm_text_to_encrypt = fsJsonPrinter.CompressedJson(data);
		return Encryption.Encrypt(prm_text_to_encrypt);
	}

	private static object DeserializeToString(Type type, string encryptedState)
	{
		string input = Encryption.Decrypt(encryptedState);
		fsData data = fsJsonParser.Parse(input);
		object result = null;
		serializer.TryDeserialize(data, type, ref result).AssertSuccessWithoutWarnings();
		return result;
	}
}

}

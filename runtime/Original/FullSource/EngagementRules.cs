namespace Crisis.Runtime.FullSource
{
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class EngagementRules : MonoBehaviour
{
	public static string Filename = "rules.dat";

	public RoeCondition operatorsCondition;

	public List<EngagementRule> operatorRules = new List<EngagementRule>();

	public bool Evaluate(CharacterBody body)
	{
		return false;
	}

	private void ApplySimplifiedToLists()
	{
		operatorRules.Clear();
		operatorRules.Add(new EngagementRule
		{
			condition = operatorsCondition,
			permit = RoePermission.Destroy
		});
	}

	public void Save()
	{
		ApplySimplifiedToLists();
		SerializableEngagementRules graph = Pack();
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		FileStream fileStream = File.Create(Application.persistentDataPath + "/" + Filename);
		binaryFormatter.Serialize(fileStream, graph);
		fileStream.Close();
	}

	public void Load()
	{
		if (File.Exists(Application.persistentDataPath + "/" + Filename))
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			FileStream fileStream = File.Open(Application.persistentDataPath + "/" + Filename, FileMode.Open);
			SerializableEngagementRules data = (SerializableEngagementRules)binaryFormatter.Deserialize(fileStream);
			fileStream.Close();
			Unpack(data);
		}
	}

	private SerializableEngagementRules Pack()
	{
		SerializableEngagementRules serializableEngagementRules = new SerializableEngagementRules();
		serializableEngagementRules.operatorsCondition = operatorsCondition;
		return serializableEngagementRules;
	}

	private void Unpack(SerializableEngagementRules data)
	{
		operatorsCondition = data.operatorsCondition;
		ApplySimplifiedToLists();
	}
}

}

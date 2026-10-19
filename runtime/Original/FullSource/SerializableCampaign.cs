namespace Crisis.Runtime.FullSource
{
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[Serializable]
public class SerializableCampaign
{
	public string name = "Untitled campaign";

	public SerializableTeamRoster roster = new SerializableTeamRoster();

	public int day = 1;

	public int finishedMissions;

	public static string DirName = "Campaigns";

	public bool SavedOK { get; private set; }

	public bool LoadedOK { get; private set; }

	public void Save()
	{
		SavedOK = false;
		string campaignsFolder = Global.Context.CampaignsFolder;
		if (!Directory.Exists(campaignsFolder))
		{
			Directory.CreateDirectory(campaignsFolder);
		}
		using (FileStream serializationStream = File.Create(campaignsFolder + "/" + name))
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			binaryFormatter.Serialize(serializationStream, this);
		}
		SavedOK = true;
	}

	public bool Load(string path)
	{
		LoadedOK = false;
		if (!File.Exists(path))
		{
			Global.Context.MessageBox.ShowOK("The file could not be found!");
			return false;
		}
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		using (FileStream serializationStream = File.Open(path, FileMode.Open))
		{
			SerializableCampaign other = binaryFormatter.Deserialize(serializationStream) as SerializableCampaign;
			Overwrite(other);
		}
		LoadedOK = true;
		return true;
	}

	public void Overwrite(SerializableCampaign other)
	{
		name = other.name;
		roster.Overwrite(other.roster);
		day = other.day;
		finishedMissions = other.finishedMissions;
	}
}

}

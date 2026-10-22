namespace Crisis.Runtime.FullSource
{
using UnityEngine;

public class CharacterNameGenerator : MonoBehaviour
{
	public CharacterNames nameDatabase;

	public string FirstName(bool male)
	{
		return (!male) ? nameDatabase.femaleFirstNames[Random.Range(0, nameDatabase.femaleFirstNames.Count)] : nameDatabase.maleFirstNames[Random.Range(0, nameDatabase.maleFirstNames.Count)];
	}

	public string LastName()
	{
		return nameDatabase.lastNames[Random.Range(0, nameDatabase.lastNames.Count)];
	}
}

}

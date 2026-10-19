namespace Crisis.Runtime.FullSource
{
using System.Collections.Generic;
using UnityEngine;

public class CharacterVocalization : MonoBehaviour
{
	public List<string> texts;

	public string Any()
	{
		return (texts.Count != 0) ? texts[Random.Range(0, texts.Count)] : string.Empty;
	}
}

}

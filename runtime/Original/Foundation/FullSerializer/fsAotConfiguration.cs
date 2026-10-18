using System;
using System.Collections.Generic;
using UnityEngine;

namespace FullSerializer
{
	[CreateAssetMenu(menuName = "Full Serializer AOT Configuration")]
	public class fsAotConfiguration : ScriptableObject
	{
		public enum AotState
		{
			Default = 0,
			Enabled = 1,
			Disabled = 2
		}

		[Serializable]
		public struct Entry
		{
			public AotState State;

			public string FullTypeName;

			public Entry(Type type)
			{
				FullTypeName = type.FullName;
				State = AotState.Default;
			}

			public Entry(Type type, AotState state)
			{
				FullTypeName = type.FullName;
				State = state;
			}
		}

		public List<Entry> aotTypes = new List<Entry>();

		public string outputDirectory = "Assets/AotModels";

		public bool TryFindEntry(Type type, out Entry result)
		{
			string fullName = type.FullName;
			foreach (Entry aotType in aotTypes)
			{
				if (aotType.FullTypeName == fullName)
				{
					result = aotType;
					return true;
				}
			}
			result = default(Entry);
			return false;
		}

		public void UpdateOrAddEntry(Entry entry)
		{
			for (int i = 0; i < aotTypes.Count; i++)
			{
				if (aotTypes[i].FullTypeName == entry.FullTypeName)
				{
					aotTypes[i] = entry;
					return;
				}
			}
			aotTypes.Add(entry);
		}
	}
}

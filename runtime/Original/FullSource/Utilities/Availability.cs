using Crisis.Runtime.FullSource;
using System;

namespace Utilities
{
	[Serializable]
	public class Availability
	{
		public BuildAvailability build;

		public TeamAvailability team;

		public bool IsAvailable(bool isStaging)
		{
			switch (build)
			{
			case BuildAvailability.EnabledForAll:
				return true;
			case BuildAvailability.StagingOnly:
				return isStaging;
			case BuildAvailability.DisabledForAll:
				return false;
			default:
				return false;
			}
		}

		public bool IsAvailable(CharacterBody.Team team)
		{
			switch (this.team)
			{
			case TeamAvailability.EnabledForAll:
				return true;
			case TeamAvailability.OperatorsOnly:
				return team == CharacterBody.Team.Operators;
			case TeamAvailability.OccupantsOnly:
				return team == CharacterBody.Team.Occupants;
			case TeamAvailability.DisabledForAll:
				return false;
			default:
				return false;
			}
		}

		public bool IsAvailable(bool isStaging, CharacterBody.Team team)
		{
			bool flag = IsAvailable(isStaging);
			bool flag2 = IsAvailable(team);
			return flag && flag2;
		}
	}
}

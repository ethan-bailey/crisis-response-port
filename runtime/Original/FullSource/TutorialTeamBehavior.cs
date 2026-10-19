namespace Crisis.Runtime.FullSource
{
using System;

[Serializable]
public class TutorialTeamBehavior
{
	public RoePermission teamEngageMode;

	public TeamController.Command teamOrder = TeamController.Command.FollowMe;

	public CharacterBody.Tempo teamTempo = CharacterBody.Tempo.Normal;

	public bool teamUseStunGrenades = true;

	public Marksman.ComputerMode marksmanMode;

	public bool marksmanThreatOverride = true;
}

}

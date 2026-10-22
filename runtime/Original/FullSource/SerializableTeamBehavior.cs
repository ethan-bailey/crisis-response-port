namespace Crisis.Runtime.FullSource
{
using System;
using FullSerializer;

[fsObject("1", new Type[] { })]
public class SerializableTeamBehavior
{
	public RoePermission teamEngageMode;

	public TeamController.Command teamOrder = TeamController.Command.FollowMe;

	public CharacterBody.Tempo teamTempo = CharacterBody.Tempo.Normal;

	public bool teamUseStunGrenades = true;

	public Marksman.ComputerMode marksmanMode;

	public bool marksmanThreatOverride = true;

	public SerializableTeamBehavior()
	{
	}

	public SerializableTeamBehavior(TutorialTeamBehavior data)
	{
		teamEngageMode = data.teamEngageMode;
		teamOrder = data.teamOrder;
		teamTempo = data.teamTempo;
		teamUseStunGrenades = data.teamUseStunGrenades;
		marksmanMode = data.marksmanMode;
		marksmanThreatOverride = data.marksmanThreatOverride;
	}
}

}

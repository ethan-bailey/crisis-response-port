namespace Crisis.Runtime.FullSource
{
public class FirearmShortRecoil : Firearm
{
	public FirearmBolt bolt;

	public FirearmBarrel barrel;

	public void Initialize()
	{
		if (bolt != null)
		{
			bolt.Initialize(LockOpen);
		}
	}

	public override void ReleaseStriker()
	{
		if (bolt != null)
		{
			bolt.Open(false);
		}
	}

	public override bool LockOpen()
	{
		return false;
	}
}

}

namespace Crisis.Runtime.FullSource
{
public class Recoil
{
	public float timestamp;

	public float kick;

	public float climb;

	public Recoil(float time, float kickMagnitude, float climbMagnitude)
	{
		timestamp = time;
		kick = kickMagnitude;
		climb = climbMagnitude;
	}
}

}

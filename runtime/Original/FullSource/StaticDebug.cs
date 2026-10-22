namespace Crisis.Runtime.FullSource
{
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;
public sealed class StaticDebug
{
	private static StaticDebug instance = null;

	private static readonly object padlock = new object();

	public ulong GlPasses { get; set; }

	public ulong DamagePasses { get; set; }

	public ulong BloodPasses { get; set; }

	public static StaticDebug Instance
	{
		get
		{
			lock (padlock)
			{
				if (instance == null)
				{
					instance = new StaticDebug();
				}
				return instance;
			}
		}
	}

	public void Reset()
	{
		GlPasses = 0uL;
		DamagePasses = 0uL;
		BloodPasses = 0uL;
	}
}

}

namespace Crisis.Runtime.FullSource
{
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;
using UnityEngine;

public class BloodStreamGenerator
{
	private MissionHelper helper;

	private BloodStream activeStream;

	private PerformanceManager perfMgr;

	private Emitter bloodEmitter;

	public bool Emitting => activeStream != null;

	public BloodStreamGenerator(Emitter bloodEmitter)
	{
		helper = MissionHelper.Instance;
		perfMgr = PerformanceManager.Instance;
		this.bloodEmitter = bloodEmitter;
	}

	public bool Update(float pressure, Vector3 position, Vector3 velocity)
	{
		if (pressure > 0.05f)
		{
			if (!Emitting)
			{
				activeStream = perfMgr.CreateBloodStream(bloodEmitter);
			}
			if (activeStream == null)
			{
				return false;
			}
			activeStream.Emit(pressure, position, velocity);
			return true;
		}
		if (Emitting)
		{
			activeStream.Terminate(position, velocity);
			activeStream = null;
			return true;
		}
		return false;
	}
}

}

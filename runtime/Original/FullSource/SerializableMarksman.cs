namespace Crisis.Runtime.FullSource
{
using System;
using FullSerializer;

[fsObject("1", new Type[] { })]
public class SerializableMarksman
{
	public bool enabled = true;

	public SerializableMarksmanRifle rifle = new SerializableMarksmanRifle();

	public float normalizedDistance = 1f;

	public float elevation = 1f;

	public float normalizedOffset = 1f;
}

}

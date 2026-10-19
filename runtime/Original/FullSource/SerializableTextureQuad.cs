namespace Crisis.Runtime.FullSource
{
using UnityEngine;

public class SerializableTextureQuad
{
	public Vector2 upperLeft;

	public Vector2 upperRight;

	public Vector2 lowerLeft;

	public Vector2 lowerRight;

	public string label;

	public Vector3 SizeRatio
	{
		get
		{
			Vector2 vector = upperRight - lowerLeft;
			float x = ((!(vector.x > vector.y)) ? CrMath.RoundTruncate(vector.x / vector.y, 3) : 1f);
			float y = ((!(vector.y > vector.x)) ? CrMath.RoundTruncate(vector.y / vector.x, 3) : 1f);
			return new Vector3(x, y, 1f);
		}
	}

	public SerializableTextureQuad(Vector2 lowerLeftNormalized, Vector2 upperRightNormalized, string label)
	{
		lowerLeft = lowerLeftNormalized;
		upperLeft = new Vector2(lowerLeftNormalized.x, upperRightNormalized.y);
		upperRight = upperRightNormalized;
		lowerRight = new Vector2(upperRightNormalized.x, lowerLeftNormalized.y);
	}
}

}

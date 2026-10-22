namespace Crisis.Runtime.FullSource
{
using UnityEngine;

public class BallisticMaterialProperties : MonoBehaviour
{
	[Header("Surface properties")]
	[Tooltip("The thickness of the surface layer of this material. In meters. Set to zero to disable surface layer. NOTE: Is not used in penetration tests: There the projectile's length is used.")]
	public float surfaceThickness;

	[Tooltip("The surface layer density of the material. More energy is spent penetrating a dense material.")]
	public float surfaceDensity = 1000f;

	[Tooltip("The maximum angle of deflection a projectile will get per decimeter of travel through this material. In degrees.")]
	public float maximumDeflection;

	[Header("Interior properties")]
	[Tooltip("The interior density of the material. More energy is spent penetrating a dense material.")]
	public float density = 1000f;

	[Tooltip("The calculated penetration distance is scaled by a random value from this range.")]
	public Vector2 densityRandomness = new Vector2(1f, 1f);

	[Header("Effects")]
	[Tooltip("The effect that will be spawned when a projectile enters this object.")]
	public BallisticHitEffect entryEffect;

	[Tooltip("The effect that will be spawned when a projectile exits this object.")]
	public BallisticHitEffect exitEffect;

	[Tooltip("A game object that will be spawn on projectile exit positions.")]
	public BallisticHitMeshEffect exitHole;
}

}

using UnityEngine;

namespace Crisis.Runtime.Weapons;

// The developer-authored Limb.ArmorEntryTest and Limb.HandleEntry calculations
// from source-reference/Original/CharacterExecution/Limb.cs.
// They execute on the recovered BallisticMaterial / Projectile types.
//
// Audio is a required game integration boundary, not a silent no-op: do not
// activate projectile-limb collision until the genuine impact audio action is
// attached and the original interior/exit wound renderer has been ported.
public partial class Limb
{
    public Action<Limb> OriginalImpactAudio {get;set;}

    private Projectile ArmorEntryTest(Projectile projectile)
    {
        float energy = projectile.Energy;
        Armor.Test(projectile, entry: true);
        float num = energy - projectile.Energy;
        if (num > 0f)
        {
            AddTrauma(0f, projectile.transform.forward * num * projectile.ForceMultiplier, projectile.transform.position);
            LimbPhysiology.AddShock(num);
        }
        return projectile;
    }

    protected override Projectile HandleEntry(Projectile projectile, Vector3 normal, bool simulation = false)
    {
        if (LimbPhysiology == null)
            throw new InvalidOperationException("Original limb entry requires original LimbPhysiology");
        if (OriginalImpactAudio == null)
            throw new NotSupportedException("Original CharacterAudio armor/exhale actions must be bound before limb hits are enabled");
        if (Armor != null && Armor.type != ArmorClass.Type.None)
        {
            ArmorEntryTest(projectile);
        }
        OriginalImpactAudio(this);
        float energy = projectile.Energy;
        base.HandleEntry(projectile, normal, simulation);
        float num = energy - projectile.Energy;
        AddTrauma(projectile.DisplacedMaterialVolume, projectile.transform.forward * num * projectile.ForceMultiplier, projectile.transform.position);
        LimbPhysiology.AddShock(num);
        return projectile;
    }

    public LimbArmor Armor => GetComponent<LimbArmor>();
}

namespace Crisis.Runtime.FullSource;

// The transport-era ArmorClass accepts the transport projectile type. This
// bridge keeps the recovered armor decision against the recovered projectile.
internal static class FullSourceArmorBridge {
    public static bool PenetrationTest(global::ArmorClass.Type type,float damage,Projectile projectile){
        ArgumentNullException.ThrowIfNull(projectile);
        float distance=0f,threshold=0f,modifier=1f;
        switch(type){
            case global::ArmorClass.Type.TypeI: distance=.005f;threshold=1500f;break;
            case global::ArmorClass.Type.TypeIIA: distance=.005f;threshold=2500f;break;
            case global::ArmorClass.Type.TypeII: distance=.005f;threshold=4000f;modifier=2f;break;
            case global::ArmorClass.Type.TypeIIIA: distance=.005f;threshold=6000f;modifier=3f;break;
            case global::ArmorClass.Type.TypeIII: distance=.01f;threshold=6000f;modifier=4f;break;
            case global::ArmorClass.Type.TypeIV: distance=.015f;threshold=8000f;modifier=.5f;break;
        }
        float ratio=damage/(threshold*modifier);
        float factor=UnityEngine.Random.Range(1f-ratio,1f-ratio*.25f);
        float cost=projectile.EnergyToPenetrate(distance,threshold)*factor;
        bool penetrates=cost<projectile.Energy;
        projectile.ConvertEnergyToFriction(cost);
        return penetrates;
    }
}

using System.Text.Json;

namespace UnityEngine
{
    // ParticleSystem's native serialized modules are retained as typed JSON values
    // until a host driver can execute their full Unity lifecycle semantics.
    public sealed partial class ParticleSystem
    {
        public float lengthInSec { get; set; }
        public bool looping { get; set; }
        public bool moveWithTransform { get; set; }
        public bool playOnAwake { get; set; }
        public bool prewarm { get; set; }
        public int scalingMode { get; set; }
        public float simulationSpeed { get; set; }
        public int stopAction { get; set; }
        public bool useRigidbodyForVelocity { get; set; }
        public bool useUnscaledTime { get; set; }
        public bool autoRandomSeed { get; set; }
        public JsonElement startDelay { get; set; }
        public JsonElement initialModuleData { get; set; }
        public JsonElement shapeModuleData { get; set; }
        public JsonElement emissionModuleData { get; set; }
        public JsonElement sizeModuleData { get; set; }
        public JsonElement rotationModuleData { get; set; }
        public JsonElement colorModuleData { get; set; }
        public JsonElement uvModuleData { get; set; }
        public JsonElement velocityModuleData { get; set; }
        public JsonElement inheritVelocityModuleData { get; set; }
        public JsonElement forceModuleData { get; set; }
        public JsonElement externalForcesModuleData { get; set; }
        public JsonElement clampVelocityModuleData { get; set; }
        public JsonElement noiseModuleData { get; set; }
        public JsonElement sizeBySpeedModuleData { get; set; }
        public JsonElement rotationBySpeedModuleData { get; set; }
        public JsonElement colorBySpeedModuleData { get; set; }
        public JsonElement collisionModuleData { get; set; }
        public JsonElement triggerModuleData { get; set; }
        public JsonElement subModuleData { get; set; }
        public JsonElement lightsModuleData { get; set; }
        public JsonElement trailModuleData { get; set; }
        public JsonElement customDataModuleData { get; set; }
    }
}

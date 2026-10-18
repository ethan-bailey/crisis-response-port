using System;

namespace UnityEngine
{
    // Unity's particle modules are value-type handles to the owning component.
    // Original emission rules remain in Emitter; the host is responsible for
    // authentic particle rendering and simulation.
    public sealed class ParticleSystem : Component
    {
        public readonly struct MinMaxCurve
        {
            public float constantMin { get; }
            public float constantMax { get; }
            public MinMaxCurve(float constant) : this(constant, constant) { }
            public MinMaxCurve(float min, float max)
            {
                constantMin = min;
                constantMax = max;
            }
            public static implicit operator MinMaxCurve(float constant) => new MinMaxCurve(constant);
        }

        public readonly struct MainModule
        {
            private readonly ParticleSystem owner;
            internal MainModule(ParticleSystem owner) { this.owner = owner; }
            public int maxParticles
            {
                get => owner.maximumParticles;
                set
                {
                    if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
                    owner.maximumParticles = value;
                }
            }
            public MinMaxCurve startSpeed
            {
                get => owner.startSpeedValue;
                set => owner.startSpeedValue = value;
            }
        }

        public readonly struct ShapeModule
        {
            private readonly ParticleSystem owner;
            internal ShapeModule(ParticleSystem owner) { this.owner = owner; }
            public float radius
            {
                get => owner.shapeRadius;
                set
                {
                    if (!float.IsFinite(value) || value < 0) throw new ArgumentOutOfRangeException(nameof(value));
                    owner.shapeRadius = value;
                }
            }
            public float angle
            {
                get => owner.shapeAngle;
                set
                {
                    if (!float.IsFinite(value)) throw new ArgumentOutOfRangeException(nameof(value));
                    owner.shapeAngle = value;
                }
            }
        }

        public readonly struct EmissionState
        {
            public readonly int maxParticles;
            public readonly MinMaxCurve startSpeed;
            public readonly float radius;
            public readonly float angle;
            public EmissionState(int maxParticles, MinMaxCurve startSpeed, float radius, float angle)
            {
                this.maxParticles = maxParticles;
                this.startSpeed = startSpeed;
                this.radius = radius;
                this.angle = angle;
            }
        }

        public interface IParticleDriver
        {
            void Emit(ParticleSystem system, int count, EmissionState state);
        }

        private int maximumParticles = 1000;
        private MinMaxCurve startSpeedValue = new MinMaxCurve(5f);
        private float shapeRadius = 1f;
        private float shapeAngle = 25f;
        public IParticleDriver Driver { get; set; }
        public MainModule main => new MainModule(this);
        public ShapeModule shape => new ShapeModule(this);

        public void Emit(int count)
        {
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
            if (Driver == null)
                throw new NotSupportedException("Original particle simulation and render driver is not bound");
            Driver.Emit(this, count, new EmissionState(maximumParticles, startSpeedValue, shapeRadius, shapeAngle));
        }
    }
}

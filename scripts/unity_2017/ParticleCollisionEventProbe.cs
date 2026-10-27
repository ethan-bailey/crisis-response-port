// Copy into Assets/Editor in the recovered Unity project, then run
// Crisis Response > Probe Particle Collision Event from the menu.
// Output: Temp/crisis-response-particle-collision-event-probe.json
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class ParticleCollisionEventProbe
{
    private const string RequiredUnityVersion = "2017.2.0f3";
    private const string OutputPath = "Temp/crisis-response-particle-collision-event-probe.json";

    [Serializable]
    private sealed class VectorRecord { public float x; public float y; public float z; }

    [Serializable]
    private sealed class Report
    {
        public string unityVersion;
        public float simulationStep;
        public int simulatedSteps;
        public VectorRecord initialPosition;
        public VectorRecord initialVelocity;
        public VectorRecord wallCenter;
        public VectorRecord wallSize;
        public int eventCount;
        public int firstEventStep;
        public VectorRecord intersection;
        public VectorRecord velocity;
        public VectorRecord normal;
    }

    [MenuItem("Crisis Response/Probe Particle Collision Event")]
    public static void Run()
    {
        if (Application.unityVersion != RequiredUnityVersion)
            throw new InvalidOperationException("Particle collision probe requires Unity " + RequiredUnityVersion +
                "; running " + Application.unityVersion);

        GameObject emitterObject = null;
        GameObject wall = null;
        try
        {
            emitterObject = new GameObject("Crisis Response collision-event emitter");
            ParticleSystem emitter = emitterObject.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = emitter.main;
            main.loop = false;
            main.playOnAwake = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 4;
            ParticleSystem.EmissionModule emission = emitter.emission;
            emission.enabled = false;
            ParticleSystem.CollisionModule collision = emitter.collision;
            collision.enabled = true;
            collision.type = ParticleSystemCollisionType.World;
            collision.mode = ParticleSystemCollisionMode.Collision3D;
            collision.quality = ParticleSystemCollisionQuality.High;
            collision.sendCollisionMessages = true;
            collision.collidesWith = ~0;
            collision.dampen = new ParticleSystem.MinMaxCurve(0f);
            collision.bounce = new ParticleSystem.MinMaxCurve(0f);
            collision.lifetimeLoss = new ParticleSystem.MinMaxCurve(0f);

            wall = new GameObject("Crisis Response collision-event wall");
            wall.transform.position = Vector3.zero;
            BoxCollider wallCollider = wall.AddComponent<BoxCollider>();
            wallCollider.size = new Vector3(0.1f, 4f, 4f);

            var particle = new ParticleSystem.Particle();
            particle.position = new Vector3(-1f, 0f, 0f);
            particle.velocity = new Vector3(4f, 0f, 0f);
            particle.startLifetime = 2f;
            particle.remainingLifetime = 2f;
            particle.startSize = 0.05f;
            emitter.SetParticles(new[] { particle }, 1);

            const float step = 0.01f;
            const int maximumSteps = 100;
            var collisionEvents = new List<ParticleCollisionEvent>();
            int eventCount = 0;
            int firstEventStep = -1;
            for (int index = 0; index < maximumSteps; index++)
            {
                emitter.Simulate(step, true, false, false);
                collisionEvents.Clear();
                int count = emitter.GetCollisionEvents(wall, collisionEvents);
                if (count > 0)
                {
                    eventCount = count;
                    firstEventStep = index + 1;
                    break;
                }
            }
            if (eventCount == 0 || collisionEvents.Count == 0)
                throw new InvalidOperationException("Version-locked particle fixture produced no collision event");

            ParticleCollisionEvent first = collisionEvents[0];
            var report = new Report
            {
                unityVersion = Application.unityVersion,
                simulationStep = step,
                simulatedSteps = maximumSteps,
                initialPosition = Vector(new Vector3(-1f, 0f, 0f)),
                initialVelocity = Vector(new Vector3(4f, 0f, 0f)),
                wallCenter = Vector(wall.transform.position),
                wallSize = Vector(wallCollider.size),
                eventCount = eventCount,
                firstEventStep = firstEventStep,
                intersection = Vector(first.intersection),
                velocity = Vector(first.velocity),
                normal = Vector(first.normal)
            };
            string absoluteOutputPath = Path.Combine(Directory.GetCurrentDirectory(), OutputPath);
            Directory.CreateDirectory(Path.GetDirectoryName(absoluteOutputPath));
            File.WriteAllText(absoluteOutputPath, JsonUtility.ToJson(report, true));
            Debug.Log("Particle collision-event probe wrote " + absoluteOutputPath +
                "; event step=" + firstEventStep + "; count=" + eventCount);
        }
        finally
        {
            if (emitterObject != null) UnityEngine.Object.DestroyImmediate(emitterObject);
            if (wall != null) UnityEngine.Object.DestroyImmediate(wall);
        }
    }

    private static VectorRecord Vector(Vector3 value)
    {
        return new VectorRecord { x = value.x, y = value.y, z = value.z };
    }
}

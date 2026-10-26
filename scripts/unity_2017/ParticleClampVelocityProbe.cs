// Copy into Assets/Editor in the recovered Unity 2017.2 project, then run
// Crisis Response > Probe Clamp Velocity Modules from the Unity menu.
// Output: Temp/cpr-clamp-velocity-probe.json
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class ParticleClampVelocityProbe
{
    [Serializable]
    private class Report
    {
        public string unityVersion;
        public List<PrefabModule> prefabModules = new List<PrefabModule>();
        public List<Simulation> simulations = new List<Simulation>();
    }

    [Serializable]
    private class PrefabModule
    {
        public string assetPath;
        public string hierarchyPath;
        public bool enabled;
        public float limitConstant;
        public float limitScalar;
        public int separateAxes;
        public string space;
        public float dampen;
        public float dragConstant;
        public float dragScalar;
        public float multiplyDragByParticleSize;
        public float multiplyDragByParticleVelocity;
    }

    [Serializable]
    private class Simulation
    {
        public float stepSeconds;
        public float initialSpeed;
        public float limit;
        public float dampen;
        public float speedAfterStep;
        public float xAfterStep;
        public float yAfterStep;
        public float zAfterStep;
    }

    [MenuItem("Crisis Response/Probe Clamp Velocity Modules")]
    public static void Run()
    {
        var report = new Report();
        report.unityVersion = Application.unityVersion;
        if (!report.unityVersion.StartsWith("2017.2."))
            throw new InvalidOperationException("Expected Unity 2017.2.x, found " + report.unityVersion);

        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
        Array.Sort(prefabGuids, StringComparer.Ordinal);
        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;
            ParticleSystem[] systems = prefab.GetComponentsInChildren<ParticleSystem>(true);
            Array.Sort(systems, delegate(ParticleSystem a, ParticleSystem b)
            {
                return HierarchyPath(a.transform).CompareTo(HierarchyPath(b.transform));
            });
            foreach (ParticleSystem system in systems)
            {
                var module = system.limitVelocityOverLifetime;
                if (!module.enabled) continue;
                var row = new PrefabModule();
                row.assetPath = path;
                row.hierarchyPath = HierarchyPath(system.transform);
                row.enabled = module.enabled;
                row.limitConstant = module.limit.constant;
                row.limitScalar = module.limit.curveMultiplier;
                row.separateAxes = module.separateAxes ? 1 : 0;
                row.space = module.space.ToString();
                row.dampen = module.dampen;
                row.dragConstant = module.drag.constant;
                row.dragScalar = module.drag.curveMultiplier;
                row.multiplyDragByParticleSize = module.multiplyDragByParticleSize ? 1 : 0;
                row.multiplyDragByParticleVelocity = module.multiplyDragByParticleVelocity ? 1 : 0;
                report.prefabModules.Add(row);
            }
        }

        float[] steps = { 1f / 60f, 1f / 30f, 0.05f, 0.1f };
        float[] speeds = { 0.5f, 1f, 2f, 10f };
        float[] dampenValues = { 0f, 0.5f, 0.98f };
        foreach (float step in steps)
        foreach (float speed in speeds)
        foreach (float dampen in dampenValues)
            report.simulations.Add(SimulateOneStep(step, speed, 1f, dampen));

        string output = Path.Combine(Directory.GetCurrentDirectory(), "Temp/cpr-clamp-velocity-probe.json");
        Directory.CreateDirectory(Path.GetDirectoryName(output));
        File.WriteAllText(output, JsonUtility.ToJson(report, true));
        Debug.Log("Clamp velocity probe wrote " + output + "; authored active modules=" + report.prefabModules.Count);
        AssetDatabase.Refresh();
    }

    private static Simulation SimulateOneStep(float step, float speed, float limit, float dampen)
    {
        var gameObject = new GameObject("CPR Clamp Probe");
        gameObject.hideFlags = HideFlags.HideAndDontSave;
        try
        {
            ParticleSystem system = gameObject.AddComponent<ParticleSystem>();
            var main = system.main;
            main.playOnAwake = false;
            main.loop = false;
            main.startLifetime = 10f;
            main.startSpeed = 0f;
            main.maxParticles = 4;
            var module = system.limitVelocityOverLifetime;
            module.enabled = true;
            module.separateAxes = false;
            module.limit = new ParticleSystem.MinMaxCurve(limit);
            module.dampen = dampen;
            var emit = new ParticleSystem.EmitParams();
            emit.velocity = new Vector3(speed, 0f, 0f);
            emit.startLifetime = 10f;
            system.Emit(emit, 1);
            system.Simulate(step, false, false, true);
            var particles = new ParticleSystem.Particle[4];
            int count = system.GetParticles(particles);
            if (count != 1)
                throw new InvalidOperationException("Expected one probe particle, got " + count);
            Vector3 velocity = particles[0].velocity;
            var result = new Simulation();
            result.stepSeconds = step;
            result.initialSpeed = speed;
            result.limit = limit;
            result.dampen = dampen;
            result.xAfterStep = velocity.x;
            result.yAfterStep = velocity.y;
            result.zAfterStep = velocity.z;
            result.speedAfterStep = velocity.magnitude;
            return result;
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(gameObject);
        }
    }

    private static string HierarchyPath(Transform transform)
    {
        var parts = new List<string>();
        while (transform != null)
        {
            parts.Add(transform.name);
            transform = transform.parent;
        }
        parts.Reverse();
        return String.Join("/", parts.ToArray());
    }
}

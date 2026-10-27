// Copy into Assets/Editor in the recovered Unity project, then run
// Crisis Response > Probe AudioSource Spatial Defaults from the menu.
// Output: Temp/crisis-response-audio-spatial-default-probe.json
// This probe intentionally refuses every editor except the source project's
// exact 2017.2.0f3 version.
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class AudioSpatialDefaultProbe
{
    private const string RequiredUnityVersion = "2017.2.0f3";
    private const string ProbePrefabPath = "Assets/__CrisisResponseAudioSpatialDefaultProbe.prefab";
    private const string OutputPath = "Temp/crisis-response-audio-spatial-default-probe.json";

    [Serializable]
    private sealed class Report
    {
        public string unityVersion;
        public string probePrefabPath;
        public float spatialBlend;
        public float dopplerLevel;
        public float minDistance;
        public float maxDistance;
        public float panStereo;
        public string rolloffMode;
        public float spread;
        public float reverbZoneMix;
        public bool spatialize;
        public bool spatializePostEffects;
        public bool bypassEffects;
        public bool bypassListenerEffects;
        public bool bypassReverbZones;
        public bool playOnAwake;
        public string serializedYamlSha256;
        public string serializedYamlBase64;
        public bool serializedYamlContainsSpatialBlend;
    }

    [MenuItem("Crisis Response/Probe AudioSource Spatial Defaults")]
    public static void Run()
    {
        if (Application.unityVersion != RequiredUnityVersion)
            throw new InvalidOperationException("AudioSource default probe requires Unity " + RequiredUnityVersion +
                "; running " + Application.unityVersion);

        GameObject root = null;
        try
        {
            AssetDatabase.DeleteAsset(ProbePrefabPath);
            root = new GameObject("Crisis Response AudioSource default probe");
            AudioSource source = root.AddComponent<AudioSource>();
            PrefabUtility.CreatePrefab(ProbePrefabPath, root);
            AssetDatabase.SaveAssets();
            AssetDatabase.ForceReserializeAssets(new[] { ProbePrefabPath });
            AssetDatabase.SaveAssets();

            string absolutePrefabPath = Path.Combine(Directory.GetCurrentDirectory(), ProbePrefabPath);
            byte[] yaml = File.ReadAllBytes(absolutePrefabPath);
            string yamlText = Encoding.UTF8.GetString(yaml);
            var report = new Report
            {
                unityVersion = Application.unityVersion,
                probePrefabPath = ProbePrefabPath,
                spatialBlend = source.spatialBlend,
                dopplerLevel = source.dopplerLevel,
                minDistance = source.minDistance,
                maxDistance = source.maxDistance,
                panStereo = source.panStereo,
                rolloffMode = source.rolloffMode.ToString(),
                spread = source.spread,
                reverbZoneMix = source.reverbZoneMix,
                spatialize = source.spatialize,
                spatializePostEffects = source.spatializePostEffects,
                bypassEffects = source.bypassEffects,
                bypassListenerEffects = source.bypassListenerEffects,
                bypassReverbZones = source.bypassReverbZones,
                playOnAwake = source.playOnAwake,
                serializedYamlSha256 = Sha256Hex(yaml),
                serializedYamlBase64 = Convert.ToBase64String(yaml),
                serializedYamlContainsSpatialBlend = yamlText.Contains("SpatialBlend:") || yamlText.Contains("m_SpatialBlend:")
            };

            string absoluteOutputPath = Path.Combine(Directory.GetCurrentDirectory(), OutputPath);
            Directory.CreateDirectory(Path.GetDirectoryName(absoluteOutputPath));
            File.WriteAllText(absoluteOutputPath, JsonUtility.ToJson(report, true));
            Debug.Log("AudioSource spatial-default probe wrote " + absoluteOutputPath +
                "; serialized YAML SHA-256=" + report.serializedYamlSha256);
        }
        finally
        {
            if (root != null)
                UnityEngine.Object.DestroyImmediate(root);
            AssetDatabase.DeleteAsset(ProbePrefabPath);
            AssetDatabase.Refresh();
        }
    }

    private static string Sha256Hex(byte[] bytes)
    {
        using (SHA256 sha = SHA256.Create())
        {
            byte[] hash = sha.ComputeHash(bytes);
            var builder = new StringBuilder(hash.Length * 2);
            foreach (byte value in hash)
                builder.Append(value.ToString("x2"));
            return builder.ToString();
        }
    }
}

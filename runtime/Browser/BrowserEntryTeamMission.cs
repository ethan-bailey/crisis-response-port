using Crisis.Runtime.FullSource;
using UnityEngine;

namespace Crisis.Runtime.Browser;

/// <summary>Starts the recovered EntryTeam mission in the incremental source world.</summary>
public static class BrowserEntryTeamMission
{
    public sealed record Session(SceneLoadResult Mission, CharacterBody Actor, int SourceActorCount);
    sealed class DisplayDriver : IDisplaySettingsDriver
    {
        public Resolution CurrentResolution { get; private set; } = new() { width = 1280, height = 720, refreshRate = 60 };
        public bool FullScreen { get; private set; }
        public void SetResolution(int width, int height, bool fullScreen, int refreshRate)
        {
            CurrentResolution = new() { width = width, height = height, refreshRate = refreshRate };
            FullScreen = fullScreen;
        }
    }

    sealed class QualityDriver : IQualitySettingsDriver
    {
        readonly Dictionary<string, object> values = new(StringComparer.Ordinal);
        public T Get<T>(string property) => values.TryGetValue(property, out var value) && value is T typed ? typed : default;
        public void Set<T>(string property, T value) => values[property] = value;
    }

    sealed class SceneDriver : UnityEngine.SceneManagement.ISceneManagerDriver
    {
        public UnityEngine.SceneManagement.Scene GetActiveScene() => new(3, "Mission 01");
        public void LoadScene(int buildIndex) => throw new NotSupportedException("Browser EntryTeam session does not change scenes");
        public void LoadScene(string sceneName) => throw new NotSupportedException("Browser EntryTeam session does not change scenes");
    }

    public static Session Start(OriginalSourcePackage package, RuntimeWorld world, bool enablePlayerBehavior = false)
    {
        ArgumentNullException.ThrowIfNull(package);
        ArgumentNullException.ThrowIfNull(world);
        world.MakeCurrent();
        new CpuTextureTransferDriver(world).Bind();
        var audioEvents = new BrowserAudioEventQueue();
        world.AudioSourceDriver = audioEvents;
        Resources.Configure(package, world);
        Screen.Bind(world, new DisplayDriver());
        QualitySettings.Bind(world, new QualityDriver());
        Shader.Resolver ??= name => new Shader { name = name, isSupported = false };
        UnityEngine.SceneManagement.SceneManager.Bind(world, new SceneDriver());
        Material.TextureDriver ??= (_, _) => { };
        UnityEngine.Audio.AudioMixer.Bind(world, audioEvents);

        SceneLoader.LoadPrefabInstance(package, "Assets/Resources/Global context.prefab", world, requireComplete: false);
        foreach (var source in world.Objects.OfType<AudioSource>()) source.Driver = audioEvents;
        world.Initialize();

        var context = Global.Context ?? throw new InvalidOperationException("Recovered Global Context did not initialize");
        context.Usage.enabled = false;
        context.CommandHandler.enabled = false;
        context.MissionMetaData.DeactivateTutorial();

        var team = new SerializableTeam();
        team.marksman.enabled = false;
        var loadout = new SerializableEquipment();
        loadout.primary.prefabName = "M4A1";
        loadout.primary.ammunitionPrefabName = "5.56-FMJ";
        loadout.secondary.prefabName = "Glock-18";
        loadout.secondary.ammunitionPrefabName = "9-FMJ";
        loadout.items.itemPrefabNames.AddRange(["Primary magazine", "Secondary magazine", "Plasticuffs", "Gauze"]);
        team.entryMembers.Add(loadout);
        typeof(OperatorSpawnData).GetProperty(nameof(OperatorSpawnData.Team),
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)!
            .SetValue(context.OperatorSpawnData, team);

        SceneLoadResult mission = SceneLoader.Load(package, "Assets/Scenes/Mission 01.unity", world);
        // Package presence is not decoded audio. The browser driver defers
        // these events until its verified-source WebAudio decoder is ready.
        var missionRoot = world.Objects.OfType<Mission>().Single();
        missionRoot.sandboxStartMode = Mission.MissionView.EntryTeam;
        world.Objects.OfType<MissionUI>().Single().missionIntro = null;
        foreach (var renderer in world.Objects.OfType<Renderer>().ToArray()) renderer.material.OnTextureAssigned ??= (_, _) => { };
        foreach (var source in world.Objects.OfType<AudioSource>().ToArray()) source.Driver ??= audioEvents;
        foreach (var pointer in world.Objects.OfType<WeaponPointer>().ToArray())
        {
            pointer.enabled = false;
            var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
            void BindImage(string field, Transform dot)
            {
                if (dot != null && dot.GetComponent<UnityEngine.UI.Image>() is { } image)
                    pointer.GetType().GetField(field, flags)?.SetValue(pointer, image);
            }
            BindImage("mainImage", pointer.mainDot);
            BindImage("startImage", pointer.startDot);
            BindImage("endImage", pointer.endDot);
        }
        foreach (var vocalization in world.Objects.OfType<UIVocalizationText>().ToArray()) vocalization.enabled = false;
        foreach (var fog in world.Objects.OfType<FogOfWarElement>().ToArray()) fog.enabled = false;
        void SuppressUnboundBehavior()
        {
            foreach (var tree in world.Objects.OfType<BehaviorDesigner.Runtime.BehaviorTree>().ToArray())
            {
                var body = tree.GetComponent<CharacterBody>();
                if (!enablePlayerBehavior || body == null || !body.IsPlayer) tree.enabled = false;
            }
        }
        SuppressUnboundBehavior();
        foreach (var rigidbody in world.Objects.OfType<Rigidbody>().ToArray()) rigidbody.PhysicsForceSink ??= (_, _, _) => { };
        void PrepareMissionFrame()
        {
            foreach (var vocalization in world.Objects.OfType<UIVocalizationText>().ToArray()) vocalization.enabled = false;
            foreach (var fog in world.Objects.OfType<FogOfWarElement>().ToArray()) fog.enabled = false;
            SuppressUnboundBehavior();
            foreach (var source in world.Objects.OfType<AudioSource>().ToArray()) source.Driver ??= audioEvents;
        }
        world.Tick(1f / 60f, PrepareMissionFrame);
        foreach (var rigidbody in world.Objects.OfType<Rigidbody>().ToArray()) rigidbody.PhysicsForceSink ??= (_, _, _) => { };
        world.Tick(1f / 60f, PrepareMissionFrame);

        var startedMission = world.Objects.OfType<Mission>().SingleOrDefault() ?? throw new InvalidOperationException("Mission 01 did not initialize");
        if (!startedMission.Started || startedMission.InteriorMode?.UI?.InteractionMenu == null)
            throw new InvalidOperationException("EntryTeam Mission did not initialize the original InteriorMode InteractionMenu");
        var actors = world.Objects.OfType<CharacterBody>()
            .Where(body => body && body.IsPlayer && body.GetComponent<PlayerController>() is { isActiveAndEnabled: true })
            .ToArray();
        if (actors.Length != 1) throw new InvalidOperationException($"Expected one active original player actor, found {actors.Length}");
        audioEvents.Clear();
        return new Session(mission, actors[0], world.Objects.OfType<CharacterBody>().Count());
    }
}

using UnityEngine;

namespace Crisis.Runtime;

/// <summary>
/// Bounded transport for original Unity ParticleSystem callbacks. This records
/// source commands and asset/component provenance; it does not simulate or draw.
/// </summary>
public sealed class BrowserParticleEventQueue : ParticleSystem.IParticleDriver
{
    const int MaximumPendingEvents = 256;
    readonly List<object> pending = [];
    public int DroppedEvents { get; private set; }

    public void Emit(ParticleSystem system, int count, ParticleSystem.EmissionState state) => Enqueue(system, new
    {
        action = "emit", count,
        maxParticles = state.maxParticles,
        startSpeed = new[] { state.startSpeed.constantMin, state.startSpeed.constantMax },
        startLifetime = new[] { state.startLifetime.constantMin, state.startLifetime.constantMax },
        startSizeMultiplier = state.startSizeMultiplier,
        randomSeed = state.randomSeed,
        radius = state.radius,
        angle = state.angle
    });

    public void Simulate(ParticleSystem system, float time, bool withChildren, bool restart, bool fixedTimeStep) =>
        Enqueue(system, new { action = "simulate", simulationTime = time, withChildren, restart, fixedTimeStep });

    public void Play(ParticleSystem system) => Enqueue(system, new { action = "play" });

    void Enqueue(ParticleSystem system, object command)
    {
        ArgumentNullException.ThrowIfNull(system);
        if (pending.Count >= MaximumPendingEvents) { DroppedEvents++; return; }
        var sourceRenderer = system.gameObject.GetComponent<ParticleSystemRenderer>();
        var attachedRigidbody = system.GetComponentInParent<Rigidbody>();
        var transform = system.transform;
        var position = transform.position;
        var rotation = transform.rotation;
        var hierarchy = new Stack<string>();
        for (var current = transform; current != null; current = current.parent)
            hierarchy.Push(current.name ?? string.Empty);
        pending.Add(new
        {
            type = "particleCommand",
            time = Time.time,
            systemId = system.GetInstanceID(),
            sourceGuid = system.OriginalAssetGuid,
            sourceFileId = system.OriginalAssetFileID,
            sourcePath = system.OriginalAssetPath,
            sourceAssetSha256 = system.OriginalAssetSha256,
            sourceFields = system.OriginalSerializedFields,
            renderer = sourceRenderer == null ? null : new
            {
                sourceGuid = sourceRenderer.OriginalAssetGuid,
                sourceFileId = sourceRenderer.OriginalAssetFileID,
                sourcePath = sourceRenderer.OriginalAssetPath,
                sourceAssetSha256 = sourceRenderer.OriginalAssetSha256,
                fields = sourceRenderer.OriginalSerializedFields
            },
            hierarchyPath = string.Join("/", hierarchy),
            position = new[] { position.x, position.y, position.z },
            rotation = new[] { rotation.x, rotation.y, rotation.z, rotation.w },
            attachedRigidbodyVelocity = attachedRigidbody == null ? new[] { 0f, 0f, 0f } : new[]
            {
                attachedRigidbody.velocity.x,
                attachedRigidbody.velocity.y,
                attachedRigidbody.velocity.z
            },
            command
        });
    }

    public object[] Drain()
    {
        var result = pending.ToArray();
        pending.Clear();
        return result;
    }
}

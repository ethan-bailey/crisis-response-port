using System.Reflection;
using System.Text.Json;
using Crisis.Runtime.FullSource;
using UnityEngine;

namespace Crisis.Runtime;

/// <summary>
/// Browser input/tick boundary for an already initialized original player actor.
/// The original Global Context, Mission, PlayerController, and actor remain the
/// owners of input interpretation, movement, aim, IK, and weapon callbacks.
/// </summary>
public sealed class FullSourceActorBridge
{
    readonly RuntimeWorld world;
    readonly CharacterBody body;
    readonly PlayerController controller;
    readonly Crisis.Runtime.FullSource.InputHandler input;
    readonly Rigidbody rigidbody;
    readonly Dictionary<string, FieldInfo> actions;
    HashSet<KeyCode> drivenKeys=[];
    readonly List<object> physicsCommands=[];
    readonly Action<Vector3,Vector3,ForceMode> priorActorPhysicsSink;
    int lastFixedSteps;

    public FullSourceActorBridge(RuntimeWorld world, CharacterBody body)
    {
        this.world = world ?? throw new ArgumentNullException(nameof(world));
        this.body = body ?? throw new ArgumentNullException(nameof(body));
        world.MakeCurrent();

        var context = Global.Context ?? throw new InvalidOperationException("FullSource actor bridge requires the original Global.Context");
        if (!world.Objects.Any(value => ReferenceEquals(value, context.gameObject)) ||
            !world.Objects.Any(value => ReferenceEquals(value, body.gameObject)))
            throw new InvalidOperationException("FullSource actor and Global.Context must belong to the same RuntimeWorld");

        input = context.InputHandler ?? throw new InvalidOperationException("FullSource actor bridge requires the original Global.Context.InputHandler");
        if (!world.Objects.Any(value => ReferenceEquals(value, input.gameObject)))
            throw new InvalidOperationException("FullSource InputHandler must belong to the actor RuntimeWorld");

        var candidates = world.Objects.OfType<PlayerController>()
            .Where(candidate => candidate && candidate.isActiveAndEnabled && ReferenceEquals(candidate.Body, body))
            .ToArray();
        if (candidates.Length != 1)
            throw new InvalidOperationException($"Expected one active original PlayerController owned by the requested actor, found {candidates.Length}");
        controller = candidates[0];
        if (!body.IsPlayer)
            throw new InvalidOperationException("Requested FullSource CharacterBody is not the original player actor");

        rigidbody = body.Rigidbody ? body.Rigidbody : throw new InvalidOperationException("FullSource player actor has no authored Rigidbody");
        priorActorPhysicsSink = rigidbody.PhysicsForceSink;
        rigidbody.PhysicsForceSink = (force, position, mode) =>
        {
            priorActorPhysicsSink?.Invoke(force, position, mode);
            CaptureActorForce(force, position, mode);
        };

        if (input.Keys == null)
            throw new InvalidOperationException("FullSource InputHandler has no authored InputMapping");
        actions = typeof(Crisis.Runtime.FullSource.InputMapping).GetFields(BindingFlags.Instance | BindingFlags.Public)
            .Where(field => field.FieldType == typeof(InputKey))
            .ToDictionary(field => field.Name, StringComparer.Ordinal);
    }

    /// <summary>
    /// Apply browser action levels and advance the original world once. Source
    /// InputHandler.Update and PlayerController.Update then run in RuntimeWorld's
    /// normal lifecycle order; source CharacterBody/weapon callbacks own results.
    /// </summary>
    public object Step(float elapsedSeconds, string actionsJson, float mouseX, float mouseY)
    {
        if (!float.IsFinite(elapsedSeconds) || elapsedSeconds < 0f)
            throw new ArgumentOutOfRangeException(nameof(elapsedSeconds));
        if (!float.IsFinite(mouseX) || !float.IsFinite(mouseY))
            throw new ArgumentException("Browser pointer coordinates must be finite");
        RequirePlayablePlayerUI();

        world.MakeCurrent();
        using var document = JsonDocument.Parse(actionsJson);
        var values = document.RootElement;
        var nextKeys=new HashSet<KeyCode>();
        foreach (var field in actions.Values)
        {
            var key = (InputKey)field.GetValue(input.Keys);
            if (key == null || key.key == KeyCode.None) continue;
            var pressed = values.TryGetProperty(field.Name, out var value) && value.ValueKind == JsonValueKind.True;
            if(pressed)nextKeys.Add(key.key);
        }
        // Several authored actions can share one physical key. Treat keyboard
        // state as the union of their action levels so a later false action
        // cannot release a key another true action is holding.
        foreach(var key in drivenKeys.Concat(nextKeys).Distinct())Input.SetKey(key,nextKeys.Contains(key));
        drivenKeys=nextKeys;
        Input.mousePosition = new Vector3(mouseX, mouseY, 0f);
        physicsCommands.Clear();
        // The browser currently owns a PhysX actor for the controlled player.
        // Keep unrelated Mission actors ticking without throwing when their
        // original source code emits a force that has no browser actor sink.
        foreach (var actorBody in world.Objects.OfType<Rigidbody>())
            if (!ReferenceEquals(actorBody, rigidbody)) actorBody.PhysicsForceSink ??= (_, _, _) => { };
        var fixedTimeBefore = Time.fixedTime;
        world.Tick(elapsedSeconds, () =>
        {
            // Starting an authored UI/resource prefab can construct a helper
            // RuntimeWorld and reset the process-wide Unity input adapter.
            // Reapply the browser levels after lifecycle/fixed callbacks, just
            // before the original InputHandler.Update consumes them.
            world.MakeCurrent();
            foreach (var key in drivenKeys.Concat(nextKeys).Distinct()) Input.SetKey(key, nextKeys.Contains(key));
            Input.mousePosition = new Vector3(mouseX, mouseY, 0f);
        });
        lastFixedSteps = Math.Max(0, (int)MathF.Round((Time.fixedTime - fixedTimeBefore) / Time.fixedDeltaTime));
        return Snapshot();
    }

    void CaptureActorForce(Vector3 force, Vector3 position, ForceMode mode)
    {
        if (mode != ForceMode.Acceleration || force.y != 0f || force.z != 0f)
            throw new NotSupportedException($"FullSource player physics command is not supported by the browser capsule bridge (mode={mode}, force={force})");
        physicsCommands.Add(new
        {
            force = new[] { force.x, force.y, force.z },
            position = new[] { position.x, position.y, position.z },
            mode = mode.ToString()
        });
    }

    /// <summary>
    /// Feed the browser PhysX result back into the original actor before its
    /// next source update. Source CharacterBody remains the owner of movement
    /// decisions; PhysX owns collision integration.
    /// </summary>
    public object SynchronizePhysics(float x, float y, float z, float velocityX, float velocityY, float velocityZ)
    {
        var values = new[] { x, y, z, velocityX, velocityY, velocityZ };
        if (values.Any(value => !float.IsFinite(value)))
            throw new ArgumentException("Browser PhysX state must contain only finite values");
        world.MakeCurrent();
        body.transform.position = new Vector3(x, y, z);
        rigidbody.velocity = new Vector3(velocityX, velocityY, velocityZ);
        return Snapshot();
    }

    /// <summary>Supply the browser-rendered AimQuad hit to the original WeaponPointer.</summary>
    public void SetAimWorldPoint(float x, float y, float z)
    {
        var point = new Vector3(x, y, z);
        if (!float.IsFinite(x) || !float.IsFinite(y) || !float.IsFinite(z))
            throw new ArgumentException("Browser aim target must contain only finite values");
        world.MakeCurrent();
        var pointer = controller.WeaponPointer ?? throw new InvalidOperationException("Original PlayerController has no WeaponPointer");
        typeof(Crisis.Runtime.FullSource.WeaponPointer)
            .GetField("localPoint", BindingFlags.Instance | Bindin
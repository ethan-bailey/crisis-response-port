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
    readonly Action<Rigidbody,Vector3,Vector3,ForceMode> priorWorldPhysicsForceSink;
    readonly Action<Rigidbody,Vector3,ForceMode> priorWorldPhysicsTorqueSink;
    readonly List<object> worldPhysicsCommands=[];
    readonly Dictionary<int, GameObject> trackedCasingObjects=[];
    readonly FieldInfo accelerationField;
    readonly BrowserAudioEventQueue audioEventQueue;
    int lastFixedSteps;
    float pendingMovementMagnitude = 1f;
    bool movementInputWasActive;
    float lastTickWalkSpeedCap;
    float lastTickSprintSpeedCap;

    public FullSourceActorBridge(RuntimeWorld world, CharacterBody body)
    {
        this.world = world ?? throw new ArgumentNullException(nameof(world));
        this.body = body ?? throw new ArgumentNullException(nameof(body));
        world.MakeCurrent();
        audioEventQueue = world.AudioSourceDriver as BrowserAudioEventQueue;

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
        accelerationField = typeof(CharacterBody).GetField("acceleration", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new MissingFieldException(typeof(CharacterBody).FullName, "acceleration");
        priorActorPhysicsSink = rigidbody.PhysicsForceSink;
        rigidbody.PhysicsForceSink = (force, position, mode) =>
        {
            priorActorPhysicsSink?.Invoke(force, position, mode);
            CaptureActorForce(force, position, mode);
        };
        // Original weapons add Rigidbody components to ejected casing clones at
        // runtime. Those bodies have no per-instance browser driver yet, so
        // preserve the world's existing driver and expose the source requests
        // in the same actor snapshot consumed by the browser boundary.
        priorWorldPhysicsForceSink = world.PhysicsForceSink;
        priorWorldPhysicsTorqueSink = world.PhysicsTorqueSink;
        world.PhysicsForceSink = (target, force, position, mode) =>
        {
            priorWorldPhysicsForceSink?.Invoke(target, force, position, mode);
            CaptureWorldForce(target, force, position, mode);
        };
        world.PhysicsTorqueSink = (target, torque, mode) =>
        {
            priorWorldPhysicsTorqueSink?.Invoke(target, torque, mode);
            CaptureWorldTorque(target, torque, mode);
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
    public object Step(float elapsedSeconds, string actionsJson, float mouseX, float mouseY, float movementMagnitude = 1f)
    {
        if (!float.IsFinite(elapsedSeconds) || elapsedSeconds < 0f)
            throw new ArgumentOutOfRangeException(nameof(elapsedSeconds));
        if (!float.IsFinite(mouseX) || !float.IsFinite(mouseY))
            throw new ArgumentException("Browser pointer coordinates must be finite");
        if (!float.IsFinite(movementMagnitude) || movementMagnitude < 0f || movementMagnitude > 1f)
            throw new ArgumentOutOfRangeException(nameof(movementMagnitude), "Analog movement magnitude must be between zero and one");
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
        physicsCommands.Clear();
        worldPhysicsCommands.Clear();
        var fixedTimeBefore = Time.fixedTime;
        var hasMovementInput =
            ((values.TryGetProperty("left", out var leftAction) && leftAction.ValueKind == JsonValueKind.True) ||
             (values.TryGetProperty("right", out var rightAction) && rightAction.ValueKind == JsonValueKind.True));
        if (hasMovementInput) pendingMovementMagnitude = movementMagnitude;
        var hasPendingSourceAcceleration = (float)(accelerationField.GetValue(body) ?? 0f) != 0f;
        var scaleSourceMovement = pendingMovementMagnitude < 1f &&
            (hasMovementInput || movementInputWasActive || hasPendingSourceAcceleration);
        var authoredWalkSpeed = body.walkSpeed;
        var authoredSprintSpeed = body.sprintSpeed;
        lastTickWalkSpeedCap = authoredWalkSpeed;
        lastTickSprintSpeedCap = authoredSprintSpeed;
        try
        {
            if (scaleSourceMovement)
            {
                // Keep the original CharacterBody movement and force formula in
                // charge, but set its per-frame speed cap from the touch stick's
                // eased walk magnitude. Restore authored values even on errors.
                body.walkSpeed = authoredWalkSpeed * pendingMovementMagnitude;
                body.sprintSpeed = authoredSprintSpeed * pendingMovementMagnitude;
                lastTickWalkSpeedCap = body.walkSpeed;
                lastTickSprintSpeedCap = body.sprintSpeed;
            }
            world.Tick(elapsedSeconds,()=>
            {
                foreach(var key in drivenKeys.Concat(nextKeys).Distinct())Input.SetKey(key,nextKeys.Contains(key));
                drivenKeys=nextKeys;
                Input.mousePosition = new Vector3(mouseX, mouseY, 0f);
            });
        }
        finally
        {
            body.walkSpeed = authoredWalkSpeed;
            body.sprintSpeed = authoredSprintSpeed;
        }
        var accelerationStillPending = (float)(accelerationField.GetValue(body) ?? 0f) != 0f;
        if (!hasMovementInput && !accelerationStillPending) pendingMovementMagnitude = 1f;
        movementInputWasActive = hasMovementInput;
        lastFixedSteps = Math.Max(0, (int)MathF.Round((Time.fixedTime - fixedTimeBefore) / Time.fixedDeltaTime));
        return Snapshot(audioEventQueue?.Drain() ?? [], (world.ParticleSystemDriver as BrowserParticleEventQueue)?.Drain() ?? []);
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

    void CaptureWorldForce(Rigidbody target, Vector3 force, Vector3 position, ForceMode mode)
    {
        if (target.gameObject.GetComponent<Casing>())
            trackedCasingObjects[target.GetInstanceID()] = target.gameObject;
        worldPhysicsCommands.Add(new
        {
            body = target.GetInstanceID(),
            name = target.gameObject.name,
            source = target.gameObject.GetComponent<Casing>() ? "Casing" : "Rigidbody",
            operation = "force",
            time = Time.time,
            sourcePosition = new[] { target.transform.position.x, target.transform.position.y, target.transform.position.z },
            sourceRotation = new[] { target.transform.rotation.x, target.transform.rotation.y, target.transform.rotation.z, target.transform.rotation.w },
            force = new[] { force.x, force.y, force.z },
            position = new[] { position.x, position.y, position.z },
            mass = target.mass,
            mode = mode.ToString()
        });
    }

    void CaptureWorldTorque(Rigidbody target, Vector3 torque, ForceMode mode)
    {
        worldPhysicsCommands.Add(new
        {
            body = target.GetInstanceID(),
            name = target.gameObject.name,
            source = target.gameObject.GetComponent<Casing>() ? "Casing" : "Rigidbody",
            operation = "torque",
            time = Time.time,
            sourcePosition = new[] { target.transform.position.x, target.transform.position.y, target.transform.position.z },
            sourceRotation = new[] { target.transform.rotation.x, target.transform.rotation.y, target.transform.rotation.z, target.transform.rotation.w },
            torque = new[] { torque.x, torque.y, torque.z },
            mass = target.mass,
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

    public Vector3 SetAimWorldPoint(float x,float y,float z)
    {
        if(!float.IsFinite(x)||!float.IsFinite(y)||!float.IsFinite(z))throw new ArgumentException("Browser aim target must contain only finite values");
        world.MakeCurrent();
        var point=new Vector3(x,y,z);
        var pointer=controller.WeaponPointer??throw new InvalidOperationException("Original PlayerController has no WeaponPointer");
        typeof(WeaponPointer).GetField("localPoint",BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(pointer,body.transform.InverseTransformPoint(point));
        typeof(WeaponPointer).GetProperty(nameof(WeaponPointer.WorldPoint),BindingFlags.Instance|BindingFlags.Public)!.SetValue(pointer,point);
        return point;
    }

    /// <summary>Round-trip recovered casing Rigidbody state through the shared browser PhysX scene.</summary>
    public object SynchronizeCasingPhysics(string statesJson)
    {
        world.MakeCurrent();
        using var document = JsonDocument.Parse(statesJson);
        if (document.RootElement.ValueKind != JsonValueKind.Array)
            throw new ArgumentException("Browser casing physics state must be an array", nameof(statesJson));
        foreach (var state in document.RootElement.EnumerateArray())
        {
            if (!state.TryGetProperty("body", out var idElement) || !idElement.TryGetInt32(out var id) ||
                !trackedCasingObjects.TryGetValue(id, out var gameObject) || !gameObject)
                continue;
            var position = ReadVector3(state, "position");
            var rotation = ReadQuaternion(state, "rotation");
            var velocity = ReadVector3(state, "velocity");
            var angularVelocity = ReadVector3(state, "angularVelocity");
            gameObject.transform.position = position;
            gameObject.transform.rotation = rotation;
            var casing = gameObject.GetComponent<Casing>();
            var body = gameObject.GetComponent<Rigidbody>();
            if (casing && body)
            {
                body.velocity = velocity;
                body.angularVelocity = angularVelocity;
                if (state.TryGetProperty("sleeping", out var sleeping) && sleeping.ValueKind == JsonValueKind.True)
                {
                    if (position.z < -2.8f && position.y > 0.5f)
                    {
                        UnityEngine.Object.Destroy(gameObject);
                        trackedCasingObjects.Remove(id);
                    }
                    else casing.DisablePhysicsFromBrowser();
                }
            }
        }
        var active = new List<object>();
        foreach (var (id, gameObject) in trackedCasingObjects.ToArray())
        {
            if (!gameObject)
            {
                trackedCasingObjects.Remove(id);
                continue;
            }
            var casing = gameObject.GetComponent<Casing>();
            var body = gameObject.GetComponent<Rigidbody>();
            if (casing && body)
                active.Add(CasingState(id, gameObject, body, false));
            else if (!casing && !body)
                active.Add(CasingState(id, gameObject, null, true));
        }
        return active;
    }

    static Vector3 ReadVector3(JsonElement state, string key)
    {
        var values = state.GetProperty(key).EnumerateArray().Select(value => value.GetSingle()).ToArray();
        if (values.Length != 3 || values.Any(value => !float.IsFinite(value)))
            throw new ArgumentException($"Browser casing {key} must contain three finite values");
        return new Vector3(values[0], values[1], values[2]);
    }

    static Quaternion ReadQuaternion(JsonElement state, string key)
    {
        var values = state.GetProperty(key).EnumerateArray().Select(value => value.GetSingle()).ToArray();
        if (values.Length != 4 || values.Any(value => !float.IsFinite(value)))
            throw new ArgumentException($"Browser casing {key} must contain four finite values");
        return new Quaternion(values[0], values[1], values[2], values[3]);
    }

    static object CasingState(int id, GameObject gameObject, Rigidbody body, bool settled)
    {
        var position = gameObject.transform.position;
        var rotation = gameObject.transform.rotation;
        var velocity = body ? body.velocity : Vector3.zero;
        var angularVelocity = body ? body.angularVelocity : Vector3.zero;
        return new
        {
            body = id,
            position = new[] { position.x, position.y, position.z },
            rotation = new[] { rotation.x, rotation.y, rotation.z, rotation.w },
            velocity = new[] { velocity.x, velocity.y, velocity.z },
            angularVelocity = new[] { angularVelocity.x, angularVelocity.y, angularVelocity.z },
            mass = body ? body.mass : 0f,
            settled
        };
    }

    void RequirePlayablePlayerUI()
    {
        var mission = Global.Context.Mission;
        var interior = mission?.InteriorMode;
        var ui = interior?.UI;
        if (ui?.InteractionMenu == null)
            throw new NotSupportedException("FullSource actor stepping requires the recovered Mission InteriorMode UI and InteractionMenu; refusing to tick a partial player context");
    }

    public object Snapshot() => Snapshot([], []);

    object Snapshot(object[] audioEvents, object[] particleEvents)
    {
        world.MakeCurrent();
        var poses = new List<object>();
        var stack = new Stack<(Transform transform, string path)>();
        stack.Push((body.transform, body.transform.name));
        while (stack.Count > 0)
        {
            var (transform, path) = stack.Pop();
            var position = transform.localPosition;
            var rotation = transform.localRotation;
            var scale = transform.localScale;
            poses.Add(new
            {
                path,
                p = new[] { position.x, position.y, position.z },
                q = new[] { rotation.x, rotation.y, rotation.z, rotation.w },
                s = new[] { scale.x, scale.y, scale.z },
                activeSelf = transform.gameObject.activeSelf
            });
            for (var i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i);
                stack.Push((child, path + "/" + child.name));
            }
        }

        var velocity = body.Rigidbody ? body.Rigidbody.velocity : Vector3.zero;
        return new
        {
            owner = "FullSource.CharacterBody",
            sourceTime = Time.time,
            audioMixerTime = Time.realtimeSinceStartup,
            playerController = controller.GetInstanceID(),
            actor = body.name,
            position = new[] { body.transform.position.x, body.transform.position.y, body.transform.position.z },
            rotation = new[] { body.transform.rotation.x, body.transform.rotation.y, body.transform.rotation.z, body.transform.rotation.w },
            velocity = new[] { velocity.x, velocity.y, velocity.z },
            direction = body.Direction,
            kneeling = body.IsKneeling,
            weapon = body.ActiveWeapon ? body.ActiveWeapon.name : null,
            poses,
            audioMixerConfiguration = audioEventQueue?.TakeMixerConfiguration(world),
            audioMixerEvents = audioEventQueue?.DrainMixerEvents() ?? Array.Empty<object>(),
            droppedAudioMixerEvents = audioEventQueue?.DroppedMixerEvents ?? 0,
            physics = new
            {
                fixedSteps = lastFixedSteps,
                commands = physicsCommands.ToArray(),
                worldCommands = worldPhysicsCommands.ToArray()
            },
            casings = trackedCasingObjects.Where(item => item.Value).Select(item =>
            {
                var casingBody = item.Value.GetComponent<Rigidbody>();
                var casing = item.Value.GetComponent<Casing>();
                return casing && casingBody ? CasingState(item.Key, item.Value, casingBody, false) :
                    !casing && !casingBody ? CasingState(item.Key, item.Value, null, true) : null;
            }).Where(item => item != null).ToArray(),
            audioEvents,
            audioSourceStates = audioEventQueue?.ActiveSourceStates() ?? Array.Empty<object>(),
            droppedAudioEvents = audioEventQueue?.DroppedEvents ?? 0,
            particleEvents,
            droppedParticleEvents = (world.ParticleSystemDriver as BrowserParticleEventQueue)?.DroppedEvents ?? 0,
            controlSpeed = new {walkCap = lastTickWalkSpeedCap,sprintCap = lastTickSprintSpeedCap,movementMagnitude = pendingMovementMagnitude}
        };
    }
}

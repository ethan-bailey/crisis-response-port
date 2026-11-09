using System.Reflection;
using System.Text.Json;
using System.Linq;
using Crisis.Runtime.FullSource;
using SourceProjectile = Crisis.Runtime.FullSource.Projectile;
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
    readonly List<object> projectileSpawnEvidence=[];
    readonly List<object> projectileInteractionEvidence=[];
    readonly Dictionary<int, GameObject> trackedCasingObjects=[];
    CharacterBody? stagedProjectileContractFirstVictim;
    CharacterBody? stagedBrowserProjectileContractFirstVictim;
    CharacterBody? stagedBrowserProjectileContractSecondVictim;
    readonly FieldInfo accelerationField;
    readonly FieldInfo accelerationMagnitudeField;
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

        // Retain the original projectile's muzzle ray before its source Update
        // can hit and destroy it. This makes browser combat failures attributable
        // to the actual FullSource fire line rather than an empty post-hit pool.
        foreach (var weapon in world.Objects.OfType<WeaponBase>().Where(candidate => candidate))
        {
            weapon.onSpawnProjectile.AddListener(projectile =>
            {
                if (!ReferenceEquals(weapon.Owner, body)) return;
                var aim = controller.WeaponPointer.WorldPoint;
                projectileSpawnEvidence.Add(new
                {
                    time = Time.time,
                    id = projectile.Id,
                    owner = projectile.Owner?.name,
                    position = new[] { projectile.transform.position.x, projectile.transform.position.y, projectile.transform.position.z },
                    forward = new[] { projectile.transform.forward.x, projectile.transform.forward.y, projectile.transform.forward.z },
                    target = new[] { aim.x, aim.y, aim.z },
                    velocity = projectile.Velocity,
                    energy = projectile.Energy,
                    precision = projectile.FiredWithPrecision
                });
                projectile.onInteraction.AddListener(hit => RecordProjectileInteraction(hit, "material", null));
                projectile.onLimbInteraction.AddListener((hit, limb) => RecordProjectileInteraction(hit, "limb", limb ? limb.id.ToString() : null));
                if (projectileSpawnEvidence.Count > 32) projectileSpawnEvidence.RemoveAt(0);
            });
        }

        rigidbody = body.Rigidbody ? body.Rigidbody : throw new InvalidOperationException("FullSource player actor has no authored Rigidbody");
        accelerationField = typeof(CharacterBody).GetField("acceleration", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new MissingFieldException(typeof(CharacterBody).FullName, "acceleration");
        accelerationMagnitudeField = typeof(CharacterBody).GetField("accelerationMagnitude", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new MissingFieldException(typeof(CharacterBody).FullName, "accelerationMagnitude");
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

    void RecordProjectileInteraction(SourceProjectile projectile, string kind, string limb)
    {
        var impactPosition = projectile.transform.position;
        var nearbyBallisticColliders = world.Objects.OfType<Collider>()
            .Where(collider => collider && ((1 << collider.gameObject.layer) & Masks.Instance.BallisticTargets) != 0)
            .Select(collider => new
            {
                collider,
                distance = Vector3.Distance(collider.transform.position, impactPosition)
            })
            .OrderBy(candidate => candidate.distance)
            .Take(8)
            .Select(candidate =>
            {
                var collider = candidate.collider;
                return new
                {
                    name = collider.name,
                    gameObject = collider.gameObject.name,
                    layer = collider.gameObject.layer,
                    colliderType = collider.GetType().Name,
                    distance = candidate.distance,
                    position = new[] { collider.transform.position.x, collider.transform.position.y, collider.transform.position.z },
                    hasBallisticMaterial = collider.GetComponent<BallisticMaterial>() != null,
                    ownerBody = collider.GetComponentInParent<CharacterBody>()?.name,
                    ownerWeapon = collider.GetComponentInParent<WeaponBase>()?.name
                };
            })
            .ToArray();
        projectileInteractionEvidence.Add(new
        {
            time = Time.time,
            id = projectile.Id,
            kind,
            limb,
            owner = projectile.Owner?.name,
            firstHitCharacter = projectile.FirstHitCharacter?.name,
            position = new[] { projectile.transform.position.x, projectile.transform.position.y, projectile.transform.position.z },
            status = projectile.Status.ToString(),
            nearbyBallisticColliders
        });
        if (projectileInteractionEvidence.Count > 64) projectileInteractionEvidence.RemoveAt(0);
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
        // RuntimeWorld runs the source LateUpdate before the browser contact
        // solver. Refresh the source movement delta after contact so the next
        // CharacterBody.UpdateAnimation sees the same post-physics displacement
        // that Unity's next rendered update would observe.
        body.RefreshMovementAfterBrowserContact();
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

    /// <summary>
    /// Keep the original Mission camera and browser renderer on one projection
    /// so Unity WeaponPointer and Three.js interpret the same screen coordinates.
    /// </summary>
    public object ConfigureCameraFraming(float fieldOfView,float aspect)
    {
        if(!float.IsFinite(fieldOfView)||fieldOfView<1f||fieldOfView>150f)
            throw new ArgumentOutOfRangeException(nameof(fieldOfView));
        if(!float.IsFinite(aspect)||aspect<=0f)
            throw new ArgumentOutOfRangeException(nameof(aspect));
        world.MakeCurrent();
        var interior=Global.Context.Mission?.InteriorMode?.interiorCamera
            ??throw new InvalidOperationException("Original Mission has no InteriorCamera");
        var dynamicFovMax=typeof(InteriorCamera).GetField("dynamicFovMax",BindingFlags.Instance|BindingFlags.NonPublic)
            ??throw new MissingFieldException(typeof(InteriorCamera).FullName,"dynamicFovMax");
        dynamicFovMax.SetValue(interior,fieldOfView);
        interior.overrideFieldOfView=true;
        interior.fieldOfViewOverride=fieldOfView;
        // Center the full-source Operator and the forward Mission lane in the packaged wide frame.
        // The source InteriorCamera still owns orientation, smoothing, and subject tracking.
        interior.centerOffset=0.49f;
        interior.interiorCamera.aspect=aspect;
        interior.interiorCamera.fieldOfView=fieldOfView;
        return SourceCameraSnapshot();
    }

    object SourceCameraSnapshot()
    {
        var interior=Global.Context.Mission?.InteriorMode?.interiorCamera
            ??throw new InvalidOperationException("Original Mission has no InteriorCamera");
        var source=interior.interiorCamera??throw new InvalidOperationException("Original Mission InteriorCamera has no Camera");
        var position=source.transform.position;
        var rotation=source.transform.rotation;
        var screenWidth=0;
        var screenHeight=0;
        try { var resolution=Screen.currentResolution; screenWidth=resolution.width; screenHeight=resolution.height; }
        catch (NotSupportedException) { }
        return new
        {
            position=new[]{position.x,position.y,position.z},
            rotation=new[]{rotation.x,rotation.y,rotation.z,rotation.w},
            fov=source.fieldOfView,
            centerOffset=interior.centerOffset,
            near=source.nearClipPlane,
            far=source.farClipPlane,
            aspect=source.aspect,
            pixelWidth=source.pixelWidth,
            pixelHeight=source.pixelHeight,
            screenWidth,
            screenHeight
        };
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

    object? ActiveWeaponState()
    {
        var weapon = body.ActiveWeapon;
        if (!weapon || body.Equipment == null) return null;
        var pointer=controller.WeaponPointer;
        var pointerRayField=typeof(WeaponPointer).GetField("ray",BindingFlags.Instance|BindingFlags.NonPublic);
        var pointerHitField=typeof(WeaponPointer).GetField("raycastHit",BindingFlags.Instance|BindingFlags.NonPublic);
        var weaponHitField=typeof(WeaponBase).GetField("raycastHit",BindingFlags.Instance|BindingFlags.NonPublic);
        var pointerLocalPointField=typeof(WeaponPointer).GetField("localPoint",BindingFlags.Instance|BindingFlags.NonPublic);
        var sourceRay=pointerRayField?.GetValue(pointer) is Ray ray?ray:default;
        var sourceHit=pointerHitField?.GetValue(pointer) is RaycastHit hit?hit:default;
        var hitCollider=sourceHit.collider;
        var weaponBlockHit=weaponHitField?.GetValue(weapon) is RaycastHit blockHit?blockHit:default;
        var weaponBlockCollider=weaponBlockHit.collider;
        var mousePosition=Input.mousePosition;
        var interiorMode=Global.Context.Mission?.InteriorMode;
        var alternateCamera=interiorMode?.AlternateCamera;
        var inputCamera=alternateCamera!=null&&alternateCamera.enabled?alternateCamera.Camera:interiorMode?.interiorCamera?.interiorCamera;
        var inputCameraPosition=inputCamera?inputCamera.transform.position:Vector3.zero;
        var inputCameraRotation=inputCamera?inputCamera.transform.rotation:Quaternion.identity;
        var aimAlignment=pointer==null?180f:Vector3.Angle(pointer.WorldPoint-weapon.Muzzle.position,weapon.transform.right);
        var aimConverged=body.rightArm.OnTarget&&body.leftArm.OnTarget&&aimAlignment<=2f;
        var magazine = weapon.Magazine;
        var reserveMagazines = weapon.IsPrimary ? body.Equipment.PrimaryMagazines : body.Equipment.SecondaryMagazines;
        var reserve = reserveMagazines
            .Where(candidate => candidate != null && candidate.calibre == weapon.calibre && !object.ReferenceEquals(candidate, magazine))
            .Sum(candidate => candidate.roundCount);
        return new
        {
            weaponId = weapon.name,
            magazine = magazine?.roundCount ?? 0,
            magazineCapacity = weapon.ComponentMagazine?.capacity ?? magazine?.capacity ?? 0,
            chamberLive = weapon.ChamberStatus == WeaponBase.ChamberState.Live,
            fireMode = weapon.ActiveFireMode.ToString(),
            readyToFire = weapon.ReadyToFire,
            rightArmOnTarget = body.rightArm?.OnTarget ?? false,
            leftArmOnTarget = body.leftArm?.OnTarget ?? false,
            manipulatingEquipment = body.IsManipulatingEquipment,
            boltStatus = weapon.cyclingBolt?.Status.ToString(),
            boltLive = weapon.cyclingBolt?.Live ?? false,
            boltMoving = weapon.cyclingBolt?.IsMoving ?? false,
            boltMalfunctioned = weapon.cyclingBolt?.Malfunctioned ?? false,
            boltCarriesLiveRound = weapon.cyclingBolt?.CarryingLiveRound ?? false,
            aimAlignment,
            aimConverged,
            weaponAngle = weapon.Angle,
            weaponRight = new[] { weapon.transform.right.x, weapon.transform.right.y, weapon.transform.right.z },
            muzzleForward = new[] { weapon.Muzzle.forward.x, weapon.Muzzle.forward.y, weapon.Muzzle.forward.z },
            weaponAlignPoint = new[] { body.WeaponAlignPoint.x, body.WeaponAlignPoint.y, body.WeaponAlignPoint.z },
            weaponAimAlignment = body.WeaponAimAlignment(),
            weaponMode = body.weaponMode.ToString(),
            isLoweringWeapon = body.IsLoweringWeapon,
            weaponBlocked = weapon.IsBlocked,
            collisionClearance = weapon.CollisionClearance,
            weaponBlockedHit = weapon.IsBlocked && weaponBlockCollider ? new
            {
                name = weaponBlockCollider.name,
                layer = weaponBlockCollider.gameObject.layer,
                point = new[] { weaponBlockHit.point.x, weaponBlockHit.point.y, weaponBlockHit.point.z },
                distance = weaponBlockHit.distance,
                normal = new[] { weaponBlockHit.normal.x, weaponBlockHit.normal.y, weaponBlockHit.normal.z }
            } : null,
            sprinting = body.IsSprinting,
            usingPrecision = body.IsUsingPrecision,
            sourceAimPoint = pointer == null ? null : new[] { pointer.WorldPoint.x, pointer.WorldPoint.y, pointer.WorldPoint.z },
            sourcePointer = pointer == null ? null : new
            {
                mousePosition=new[]{mousePosition.x,mousePosition.y,mousePosition.z},
                rayOrigin=new[]{sourceRay.origin.x,sourceRay.origin.y,sourceRay.origin.z},
                rayDirection=new[]{sourceRay.direction.x,sourceRay.direction.y,sourceRay.direction.z},
                raycastHit=hitCollider?new { name=hitCollider.name,layer=hitCollider.gameObject.layer,point=new[]{sourceHit.point.x,sourceHit.point.y,sourceHit.point.z},distance=sourceHit.distance }:null,
                interactionMenuVisible=interiorMode?.UI?.InteractionMenu?.Visible??false,
                pointerVisible=pointer?.Visible??false,
                localPoint=pointerLocalPointField?.GetValue(pointer) is Vector3 local?new[]{local.x,local.y,local.z}:null,
                inputCamera= new { isAlternate=alternateCamera!=null&&alternateCamera.enabled,position=new[]{inputCameraPosition.x,inputCameraPosition.y,inputCameraPosition.z},rotation=new[]{inputCameraRotation.x,inputCameraRotation.y,inputCameraRotation.z,inputCameraRotation.w},fov=inputCamera?inputCamera.fieldOfView:0f,pixelWidth=inputCamera?inputCamera.pixelWidth:0,pixelHeight=inputCamera?inputCamera.pixelHeight:0 }
            },
            reserve,
            reloading = body.IsManipulatingEquipment
        };
    }

    public object Snapshot() => Snapshot([], []);

    /// <summary>Apply the original extra setting that hides authored Room exterior hierarchies.</summary>
    public object SetExteriorWallsHidden(bool hidden)
    {
        world.MakeCurrent();
        var extras = Global.Context?.Settings?.Extras
            ?? throw new InvalidOperationException("FullSource Room exterior visibility requires original Settings/Extras");
        extras.disableExteriorWalls = hidden;
        extras.onChanged.Invoke(extras);
        var rooms = world.Objects.OfType<Room>().Where(room => room).ToArray();
        var matchingRooms = rooms.Count(room => room.exteriorGeometry != null &&
            room.exteriorGeometry.gameObject.activeSelf == !hidden);
        return new
        {
            owner = "Global.Context.Settings.Extras.disableExteriorWalls",
            disableExteriorWalls = extras.disableExteriorWalls,
            matchingRoomExteriorHierarchies = matchingRooms,
            roomCount = rooms.Length
        };
    }

    static object? WoundMaskPixelSnapshot(Limb limb)
    {
        var renderer = limb.limbRenderer;
        if (renderer == null) return null;
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        var data = typeof(LimbRenderer).GetField("textureData", flags)?.GetValue(renderer);
        if (data == null) return null;
        var dataType = data.GetType();
        var skin = dataType.GetField("skinTexture", flags)?.GetValue(data) as Texture2D;
        var damage = dataType.GetField("staticDamage", flags)?.GetValue(data) as Texture2D;
        if (skin == null || damage == null || skin.width != damage.width || skin.height != damage.height) return null;

        var sourcePixels = skin.GetPixels();
        var damagePixels = damage.GetPixels();
        if (sourcePixels.Length != damagePixels.Length) return null;
        int changed = 0, minX = damage.width, minY = damage.height, maxX = -1, maxY = -1;
        const float epsilon = 0.001f;
        for (int i = 0; i < sourcePixels.Length; i++)
        {
            var source = sourcePixels[i];
            var actual = damagePixels[i];
            float originalGreen = source.a > 0f ? 0f : 1f;
            bool differs = MathF.Abs(actual.r) > epsilon ||
                MathF.Abs(actual.g - originalGreen) > epsilon ||
                MathF.Abs(actual.b) > epsilon ||
                MathF.Abs(actual.a - source.a) > epsilon;
            if (!differs) continue;
            changed++;
            int x = i % damage.width, y = i / damage.width;
            minX = Math.Min(minX, x); minY = Math.Min(minY, y);
            maxX = Math.Max(maxX, x); maxY = Math.Max(maxY, y);
        }
        return new
        {
            source = "LimbTextureData.staticDamage compared with its original skin-alpha initialization",
            width = damage.width,
            height = damage.height,
            changedPixels = changed,
            changedBounds = changed == 0 ? null : new[] { minX, minY, maxX, maxY },
            rendererDynamic = renderer.IsDynamic,
            dynamicDamageTargetCreated = renderer.debugDynamicDamage?.IsCreated() ?? false
        };
    }

    /// <summary>Read-only combat evidence for the packaged source projectile acceptance.</summary>
    public object CombatSnapshot()
    {
        world.MakeCurrent();
        var occupants = world.Objects.OfType<CharacterBody>()
            .Where(candidate => candidate && candidate.GetComponent<Occupant>() != null)
            .Select(victim =>
            {
                var limbs = world.Objects.OfType<Limb>()
                    .Where(limb => limb && ReferenceEquals(limb.Body, victim))
                    .OrderBy(limb => limb.id)
                    .Select(limb => new
                    {
                        limb = limb.id.ToString(),
                        name = limb.name,
                        position = new[] { limb.transform.position.x, limb.transform.position.y, limb.transform.position.z },
                        aimPoint = new[] { limb.AimPoint.x, limb.AimPoint.y, limb.AimPoint.z },
                        wounds = limb.LimbPhysiology?.wounds.Count ?? 0,
                        woundCavityVolume = limb.LimbPhysiology?.WoundCavityVolume ?? 0f,
                        damagedTissueVolume = limb.LimbPhysiology?.DamagedTissueVolume ?? 0f,
                        localPressure = limb.LimbPhysiology?.LocalPressure ?? 0f,
                        internalBloodVolume = limb.LimbPhysiology?.InternalBloodVolume ?? 0f,
                        internalHemorrhaging = limb.LimbPhysiology?.InternalHemorrhaging ?? 0f,
                        hemorrhaging = limb.LimbPhysiology?.Hemorrhaging ?? 0f,
                        woundMaskPixels = limb.id == LimbId.Torso ? WoundMaskPixelSnapshot(limb) : null
                    })
                    .ToArray();
                var lastHit = victim.LastHitLimb;
                return new
                {
                    body = victim.name,
                    position = new[] { victim.transform.position.x, victim.transform.position.y, victim.transform.position.z },
                    lastHitLimb = lastHit ? lastHit.id.ToString() : null,
                    alive = victim.Physiology?.Alive,
                    conscious = victim.Physiology?.Conscious,
                    shock = victim.Physiology?.Shock,
                    bloodVolume = victim.Physiology?.BloodVolume,
                    bloodOxygen = victim.Physiology?.BloodOxygen,
                    ragdoll = victim.IsRagdoll,
                    restrained = victim.IsRestrained,
                    physiology = victim.Physiology == null ? null : new
                    {
                        alive = victim.Physiology.Alive,
                        conscious = victim.Physiology.Conscious,
                        shock = victim.Physiology.Shock,
                        bloodVolume = victim.Physiology.BloodVolume,
                        bloodOxygen = victim.Physiology.BloodOxygen
                    },
                    limbs
                };
            })
            .ToArray();
        var projectiles = world.Objects.OfType<SourceProjectile>()
            .Where(projectile => projectile)
            .Select(projectile => new
            {
                id = projectile.Id,
                owner = projectile.Owner?.name,
                firstHitCharacter = projectile.FirstHitCharacter?.name,
                status = projectile.Status.ToString(),
                position = new[] { projectile.transform.position.x, projectile.transform.position.y, projectile.transform.position.z },
                velocity = projectile.Velocity,
                energy = projectile.Energy,
                spawnTime = projectile.SpawnTime
            })
            .ToArray();
        return new
        {
            observer = "FullSourceActorBridge",
            shooter = body.name,
            shotsFired = body.ShotsFired,
            sourceDeltaTime = UnityEngine.Time.deltaTime,
            mission = Global.Context?.Mission?.gameObject.name,
            missionOutcome = Global.Context?.Mission == null ? null : new
            {
                started = Global.Context.Mission.Started,
                ended = Global.Context.Mission.missionEnded,
                success = Global.Context.Mission.missionSuccess,
                occupantsAllRenderedSafe = Global.Context.Mission.Occupants?.AllRenderedSafe,
                occupantsAllRestrained = Global.Context.Mission.Occupants?.AllRestrained
            },
            occupants,
            projectiles,
            spawnedProjectiles = projectileSpawnEvidence.ToArray(),
            projectileInteractions = projectileInteractionEvidence.ToArray()
        };
    }

    /// <summary>Stage the original actors in the same clear lane used by MissionProjectileContracts.</summary>
    public object StageProjectileContractScenario(int shotNumber)
    {
        world.MakeCurrent();
        if(shotNumber is not (1 or 2))throw new ArgumentOutOfRangeException(nameof(shotNumber));
        var victims=world.Objects.OfType<CharacterBody>()
            .Where(candidate=>candidate&&candidate.GetComponent<Occupant>()!=null).ToArray();
        if(victims.Length<2)throw new InvalidOperationException($"The projectile contract scenario requires two live original Occupants; found {victims.Length}");

        CharacterBody target;
        if(shotNumber==1){
            target=victims.OrderBy(victim=>MathF.Abs(victim.transform.position.x-2f))
                .FirstOrDefault(victim=>MathF.Abs(victim.transform.position.x-2f)>6f)
                ??throw new InvalidOperationException("The projectile contract scenario has no Occupant beyond the authored roof lane");
            stagedProjectileContractFirstVictim=target;
            // Match the source contract's stationary X=4 kneeling shooter before each tap.
            // The preceding browser movement setup can leave velocity on the original actor;
            // the source contract starts this fixture from rest and keeps it staged at X=4.
            body.Rigidbody.velocity=Vector3.zero;
            body.transform.position=new Vector3(4f,body.transform.position.y,-0.7f);
            body.IsKneeling=true;
            int otherIndex=0;
            foreach(var otherVictim in victims.Where(victim=>!ReferenceEquals(victim,target)))
                otherVictim.transform.position=new Vector3(40f+otherIndex++*2f,otherVictim.transform.position.y,otherVictim.transform.position.z);
        }
        else{
            // FireOnce stages the same stationary kneeling shooter at X=4 for the second distinct Occupant too.
            body.Rigidbody.velocity=Vector3.zero;
            body.transform.position=new Vector3(4f,body.transform.position.y,-0.7f);
            body.IsKneeling=true;
            if(stagedProjectileContractFirstVictim==null)
                throw new InvalidOperationException("The second projectile contract target cannot be staged before the first");
            target=victims.FirstOrDefault(victim=>!ReferenceEquals(victim,stagedProjectileContractFirstVictim))
                ??throw new InvalidOperationException("The projectile contract scenario has no second Occupant");
            var firstPosition=stagedProjectileContractFirstVictim.transform.position;
            stagedProjectileContractFirstVictim.transform.position=new Vector3(40f,firstPosition.y,firstPosition.z);
            int otherIndex=0;
            foreach(var otherVictim in victims.Where(victim=>!ReferenceEquals(victim,stagedProjectileContractFirstVictim)&&!ReferenceEquals(victim,target)))
                otherVictim.transform.position=new Vector3(42f+otherIndex++*2f,otherVictim.transform.position.y,otherVictim.transform.position.z);
        }

        var targetPosition=target.transform.position;
        // Keep the original actors on the authored continuous X=4..7 floor strip.
        // The serialized floor boxes cover Z=-1..3, while the scene spawn Z=-1.5 falls off that platform.
        target.transform.position=new Vector3(7f,targetPosition.y+.75f,-0.7f);
        var sourceTarget=target.GetComponent<Target>()
            ??throw new InvalidOperationException($"Original Occupant {target.name} has no Target component");
        body.Blackboard.Target=sourceTarget;
        body.WeaponAimer.Mode=WeaponAimer.AimMode.Ready;
        world.Invoke(body.WeaponAimer,"Update");
        var torso=target.limbMap.TryGetValue(LimbId.Torso,out var targetTorso)?targetTorso
            :throw new InvalidOperationException($"Original Occupant {target.name} has no Torso limb");
        body.AimPoint=torso.AimPointWithExtents(body.RightUpperArm.transform.position);
        var torsoCollider=torso.GetComponent<Collider>()
            ??throw new InvalidOperationException($"Original Occupant {target.name} Torso has no serialized collider");
        var torsoBounds=torsoCollider.bounds;
        var torsoHitPointCandidates=new[]{
            body.AimPoint,
            torsoBounds.center,
            torsoBounds.center+Vector3.down*(torsoBounds.extents.y*.5f),
            torsoBounds.center+Vector3.down*(torsoBounds.extents.y*.85f)
        };
        var muzzle=body.ActiveWeapon?.Muzzle
            ??throw new InvalidOperationException($"Original shooter {body.name} has no active weapon muzzle");
        var muzzlePosition=muzzle.position;
        var aimQuadX=body.Position+body.Direction*20f;
        var aimPointCandidates=torsoHitPointCandidates.Select(torsoPoint=>{
            var muzzlePosition=muzzle.position;
            var horizontalSpan=torsoPoint.x-muzzlePosition.x;
            if(Mathf.Abs(horizontalSpan)<.01f)
                throw new InvalidOperationException("Torso hit point is too close to the muzzle for AimQuad projection");
            var distanceFactor=(aimQuadX-muzzlePosition.x)/horizontalSpan;
            var pointerPoint=new Vector3(
                aimQuadX,
                muzzlePosition.y+(torsoPoint.y-muzzlePosition.y)*distanceFactor,
                -1.5f);
            return new{
                pointerPoint=new[]{pointerPoint.x,pointerPoint.y,pointerPoint.z},
                torsoPoint=new[]{torsoPoint.x,torsoPoint.y,torsoPoint.z}
            };
        }).ToArray();
        return new{
            shotNumber,
            shooter=body.name,
            shooterPosition=new[]{body.transform.position.x,body.transform.position.y,body.transform.position.z},
            kneeling=body.IsKneeling,
            target=target.name,
            targetPosition=new[]{target.transform.position.x,target.transform.position.y,target.transform.position.z},
            aimedLimb=torso.id.ToString(),
            aimPoint=new[]{body.AimPoint.x,body.AimPoint.y,body.AimPoint.z},
            muzzlePosition=new[]{muzzlePosition.x,muzzlePosition.y,muzzlePosition.z},
            torsoColliderRadius=torsoBounds.extents.z,
            aimPointCandidates,
            sourceSnapshot=Snapshot()
        };
    }

    public object StageBrowserProjectileContractScenario(int shotNumber)
    {
        world.MakeCurrent();
        if(shotNumber is not (1 or 2))throw new ArgumentOutOfRangeException(nameof(shotNumber));
        var victims=world.Objects.OfType<CharacterBody>()
            .Where(candidate=>candidate&&candidate.GetComponent<Occupant>()!=null).ToArray();
        if(victims.Length<2)throw new InvalidOperationException($"The projectile contract scenario requires two original Occupants; found {victims.Length}");

        CharacterBody target;
        if(shotNumber==1){
            // AI lifecycle can remove a particular serialized Occupant before
            // the browser-only combat fixture begins. Select from the original
            // actors still owned by this Mission world, preferring the
            // previously proven visible Occupant when it is present.
            target=victims.FirstOrDefault(victim=>victim.name=="Occupant 5")??victims[0];
            stagedBrowserProjectileContractFirstVictim=target;
            stagedBrowserProjectileContractSecondVictim=victims.FirstOrDefault(victim=>!ReferenceEquals(victim,target))
                ??throw new InvalidOperationException("The browser projectile scenario requires a second live original Occupant");
            if(ReferenceEquals(stagedBrowserProjectileContractSecondVictim,target))
                throw new InvalidOperationException("The browser projectile scenario requires two distinct authored Occupants");
            // Stage beyond the authored Left section wall at X≈5; the original
            // WeaponBase muzzle-clearance ray correctly lowers a weapon held in
            // the wall at the source contract's X=4 staging point.
            body.transform.position=new Vector3(5.5f,0.94f,-0.7f);
            body.Rigidbody.velocity=Vector3.zero;
            body.IsSprinting=false;
            body.IsKneeling=true;
            int otherIndex=0;
            foreach(var otherVictim in victims.Where(victim=>!ReferenceEquals(victim,target)))
                otherVictim.transform.position=new Vector3(40f+otherIndex++*2f,otherVictim.transform.position.y,otherVictim.transform.position.z);
        }
        else{
            // Re-ground the source shooter for each distinct source-contract shot after browser contact integration.
            body.transform.position=new Vector3(5.5f,0.94f,-0.7f);
            body.Rigidbody.velocity=Vector3.zero;
            body.IsSprinting=false;
            body.IsKneeling=true;
            if(stagedBrowserProjectileContractFirstVictim==null||stagedBrowserProjectileContractSecondVictim==null)
                throw new InvalidOperationException("The second browser projectile target cannot be staged before the first");
            target=stagedBrowserProjectileContractSecondVictim;
            var firstPosition=stagedBrowserProjectileContractFirstVictim.transform.position;
            stagedBrowserProjectileContractFirstVictim.transform.position=new Vector3(40f,firstPosition.y,firstPosition.z);
            int otherIndex=0;
            foreach(var otherVictim in victims.Where(victim=>!ReferenceEquals(victim,stagedBrowserProjectileContractFirstVictim)&&!ReferenceEquals(victim,target)))
                otherVictim.transform.position=new Vector3(42f+otherIndex++*2f,otherVictim.transform.position.y,otherVictim.transform.position.z);
        }

        var targetPosition=target.transform.position;
        // Stage both original Occupants in the proven visible far lane at the grounded source actor depth.
        target.transform.position=new Vector3(17.74f,0.94f,-0.7f);
        // This is the original player actor: PlayerController and WeaponPointer
        // own its live aim. Do not attach the browser fixture's target to the
        // AI blackboard, which can let a BehaviorTree change the player's weapon mode.
        if(target.GetComponent<Target>()==null)
            throw new InvalidOperationException($"Original Occupant {target.name} has no Target component");
        body.WeaponAimer.Mode=WeaponAimer.AimMode.Ready;
        world.Invoke(body.WeaponAimer,"Update");
        var torso=target.limbMap.TryGetValue(LimbId.Torso,out var targetTorso)?targetTorso
            :throw new InvalidOperationException($"Original Occupant {target.name} has no Torso limb");
        body.AimPoint=torso.AimPointWithExtents(body.RightUpperArm.transform.position);
        var torsoCollider=torso.GetComponent<Collider>()
            ??throw new InvalidOperationException($"Original Occupant {target.name} Torso has no serialized collider");
        var torsoBounds=torsoCollider.bounds;
        // Aim at the original Torso limb transform center. AimPointWithExtents
        // can sit above the serialized capsule: the packaged trace fired from
        // that point and passed outside the Torso capsule into Abdomen.
        var torsoSourceAimPoint=torso.transform.position;
        // Center aim has alternated between Torso and Neck. A half-extent
        // inset reached a foot and the quarter inset was not converged for
        // Occupant 5, so use the intermediate point between those outcomes.
        body.AimPoint=shotNumber==1?torsoSourceAimPoint:
            torsoSourceAimPoint+Vector3.down*(torsoBounds.extents.y*.09375f);
        var torsoHitPointCandidates=new[]{
            body.AimPoint,
            torsoSourceAimPoint,
            torsoBounds.center,
            torsoBounds.center+Vector3.down*(torsoBounds.extents.y*.5f),
            torsoBounds.center+Vector3.down*(torsoBounds.extents.y*.85f)
        };
        var muzzle=body.ActiveWeapon?.Muzzle
            ??throw new InvalidOperationException($"Original shooter {body.name} has no active weapon muzzle");
        var muzzlePosition=muzzle.position;
        var aimQuadX=body.Position+body.Direction*20f;
        var aimPointCandidates=torsoHitPointCandidates.Select(torsoPoint=>{
            var muzzlePosition=muzzle.position;
            var horizontalSpan=torsoPoint.x-muzzlePosition.x;
            if(Mathf.Abs(horizontalSpan)<.01f)
                throw new InvalidOperationException("Torso hit point is too close to the muzzle for AimQuad projection");
            var distanceFactor=(aimQuadX-muzzlePosition.x)/horizontalSpan;
            var pointerPoint=new Vector3(
                aimQuadX,
                muzzlePosition.y+(torsoPoint.y-muzzlePosition.y)*distanceFactor,
                -1.5f);
            return new{
                pointerPoint=new[]{pointerPoint.x,pointerPoint.y,pointerPoint.z},
                torsoPoint=new[]{torsoPoint.x,torsoPoint.y,torsoPoint.z}
            };
        }).ToArray();
        return new{
            shotNumber,
            shooter=body.name,
            shooterPosition=new[]{body.transform.position.x,body.transform.position.y,body.transform.position.z},
            kneeling=body.IsKneeling,
            target=target.name,
            targetPosition=new[]{target.transform.position.x,target.transform.position.y,target.transform.position.z},
            aimedLimb=torso.id.ToString(),
            aimPoint=new[]{body.AimPoint.x,body.AimPoint.y,body.AimPoint.z},
            muzzlePosition=new[]{muzzlePosition.x,muzzlePosition.y,muzzlePosition.z},
            torsoColliderRadius=torsoBounds.extents.z,
            aimPointCandidates,
            sourceSnapshot=Snapshot()
        };
    }

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
        var sourceAnimator = body.GetComponent<Animator>()
            ?? throw new InvalidOperationException("FullSource player CharacterBody has no original Animator component");
        var mission = Global.Context.Mission;
        return new
        {
            owner = "FullSource.CharacterBody",
            sourceTime = Time.time,
            audioMixerTime = Time.realtimeSinceStartup,
            playerController = controller.GetInstanceID(),
            originalInput = actions.ToDictionary(pair => pair.Key, pair =>
            {
                var item = (InputKey)pair.Value.GetValue(input.Keys);
                return new
                {
                    Pressed = item.Pressed,
                    PressedLong = item.PressedLong,
                    Down = item.Down,
                    DownLong = item.DownLong,
                    Up = item.Up,
                    UpShort = item.UpShort,
                    UpLong = item.UpLong,
                    DownHandled = item.DownHandled,
                    UpHandled = item.UpHandled,
                    Enabled = item.Enabled,
                    axisValue = item.axisValue,
                    key = item.KeyName
                };
            }),
            actor = body.name,
            position = new[] { body.transform.position.x, body.transform.position.y, body.transform.position.z },
            rotation = new[] { body.transform.rotation.x, body.transform.rotation.y, body.transform.rotation.z, body.transform.rotation.w },
            velocity = new[] { velocity.x, velocity.y, velocity.z },
            direction = body.Direction,
            kneeling = body.IsKneeling,
            movementState = new
            {
                missionView = mission?.View.ToString(),
                missionPaused = mission?.Paused ?? false,
                playerControllerActive = controller.isActiveAndEnabled,
                bodyRagdoll = body.IsRagdoll,
                sprinting = body.IsSprinting,
                movementDelta = body.MovementDelta,
                sourceDeltaTime = Time.deltaTime,
                sourceMovingThreshold = Time.deltaTime * 0.1f,
                sourceFixedDeltaTime = Time.fixedDeltaTime,
                sourceIsMoving = body.IsMoving,
                sourceIsMovingBackwards = body.IsMovingBackwards,
                acceleration = (float)(accelerationField.GetValue(body) ?? 0f),
                accelerationMagnitude = (float)(accelerationMagnitudeField.GetValue(body) ?? 0f),
                sourceVelocity = velocity.x,
                walkSpeed = body.walkSpeed,
                sprintSpeed = body.sprintSpeed,
                fixedSteps = lastFixedSteps,
                emittedForceCommands = physicsCommands.Count
            },
            // CharacterBody.UpdateAnimation writes these on the source Animator
            // before LateUpdate updates MovementDelta and before the browser
            // contact step runs. Transport the source-owned values so the
            // renderer does not recompute them from a later physics snapshot.
            animationParameters = new
            {
                moving = sourceAnimator.GetBool("Moving"),
                backwards = sourceAnimator.GetBool("Backwards"),
                moveSpeed = sourceAnimator.GetFloat("MoveSpeed"),
                kneelRatio = sourceAnimator.GetFloat("KneelRatio"),
                crippledRatio = sourceAnimator.GetFloat("CrippledRatio")
            },
            weapon = body.ActiveWeapon ? body.ActiveWeapon.name : null,
            weaponState = ActiveWeaponState(),
            sourceCamera = SourceCameraSnapshot(),
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

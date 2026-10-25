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
    readonly Dictionary<string, FieldInfo> actions;
    HashSet<KeyCode> drivenKeys=[];

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
        world.Tick(elapsedSeconds);
        return Snapshot();
    }

    void RequirePlayablePlayerUI()
    {
        var mission = Global.Context.Mission;
        var interior = mission?.InteriorMode;
        var ui = interior?.UI;
        if (ui?.InteractionMenu == null)
            throw new NotSupportedException("FullSource actor stepping requires the recovered Mission InteriorMode UI and InteractionMenu; refusing to tick a partial player context");
    }

    public object Snapshot()
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
                s = new[] { scale.x, scale.y, scale.z }
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
            playerController = controller.GetInstanceID(),
            actor = body.name,
            position = new[] { body.transform.position.x, body.transform.position.y, body.transform.position.z },
            rotation = new[] { body.transform.rotation.x, body.transform.rotation.y, body.transform.rotation.z, body.transform.rotation.w },
            velocity = new[] { velocity.x, velocity.y, velocity.z },
            direction = body.Direction,
            kneeling = body.IsKneeling,
            weapon = body.ActiveWeapon ? body.ActiveWeapon.name : null,
            poses
        };
    }
}

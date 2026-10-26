using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Crisis.Runtime;
using UnityEngine;
using BDTask = BehaviorDesigner.Runtime.Tasks.Task;

public static class BehaviorDesignerContracts
{
    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    public static string Run(OriginalSourcePackage package)
    {
        var world = new RuntimeWorld();
        world.MakeCurrent();
        Resources.Configure(package, world);
        var reports = new List<object>();

        foreach (var actor in new[] { "Operator", "Occupant" })
        {
            var path = $"Assets/Resources/characters/{actor}.prefab";
            var prefab = SceneLoader.LoadPrefab(package, path, world, requireComplete: false);
            var trees = prefab.objects.Values.OfType<BehaviorTree>().Distinct<BehaviorTree>(ReferenceEqualityComparer.Instance).ToArray();
            if (trees.Length == 0)
                throw new InvalidOperationException($"Original {actor} prefab has no Behavior Designer BehaviorTree");

            var graphTasks = new HashSet<BDTask>(ReferenceEqualityComparer.Instance);
            var embeddedConditions = new List<ConditionalEvaluator>();
            foreach (var tree in trees)
            {
                var source = tree.GetBehaviorSource();
                source.Initialize(tree);
                source.CheckForSerialization(force: true);
                Visit(source.EntryTask);
                Visit(source.RootTask);
                if (source.DetachedTasks != null)
                    foreach (var detached in source.DetachedTasks)
                        Visit(detached);

                void Visit(BDTask task)
                {
                    if (task == null || !graphTasks.Add(task)) return;
                    if (task is ConditionalEvaluator conditional)
                    {
                        embeddedConditions.Add(conditional);
                        Visit(conditional.conditionalTask);
                    }
                    if (task is ParentTask parent && parent.Children != null)
                        foreach (var child in parent.Children) Visit(child);
                }
            }

            if (graphTasks.Count == 0)
                throw new InvalidOperationException($"Original {actor} BehaviorTree deserialized to an empty task graph");
            var unknownTasks = graphTasks.Where(task => task is UnknownTask or UnknownParentTask).ToArray();
            if (unknownTasks.Length > 0)
            {
                var details = unknownTasks.Select(task =>
                {
                    var taskData = trees[0].GetBehaviorSource().TaskData;
                    var typeName = task.ID >= 0 && task.ID < taskData.types.Count ? taskData.types[task.ID] : "<task index unavailable>";
                    return $"{typeName} (ID={task.ID}, label={task.FriendlyName})";
                });
                throw new InvalidOperationException($"Original {actor} BehaviorTree contains unknown task types: {string.Join("; ", details)}");
            }
            var missing = embeddedConditions.Where(task => task.conditionalTask == null)
                .Select(task => $"ID={task.ID}, label={task.FriendlyName}").ToArray();
            if (missing.Length > 0)
                throw new InvalidOperationException($"Original {actor} has {missing.Length}/{embeddedConditions.Count} ConditionalEvaluator nodes with missing embedded conditions: {string.Join("; ", missing)}");

            var taskPaths = new Dictionary<BDTask, string>(ReferenceEqualityComparer.Instance);
            foreach (var tree in trees)
            {
                var source = tree.GetBehaviorSource();
                AddPath(source.EntryTask, "Entry");
                AddPath(source.RootTask, "Root");
                if (source.DetachedTasks != null)
                    foreach (var detached in source.DetachedTasks) AddPath(detached, "Detached");

                void AddPath(BDTask task, string path)
                {
                    if (task == null || taskPaths.ContainsKey(task)) return;
                    taskPaths.Add(task, path);
                    if (task is ConditionalEvaluator conditional)
                        AddPath(conditional.conditionalTask, path + "/condition");
                    if (task is ParentTask parent && parent.Children != null)
                        for (int i = 0; i < parent.Children.Count; i++)
                            AddPath(parent.Children[i], $"{path}/{parent.FriendlyName}[{i}]");
                }
            }

            var magazineActions = graphTasks.OfType<CharacterBehavior.ChangeMagazine>().ToArray();
            var reloadRequests = graphTasks.OfType<CharacterBehavior.RequestedReload>().ToArray();
            var reloadTask = graphTasks.Where(task => task.GetType().FullName == "CharacterBehavior.Reload").ToArray();
            var aiWeaponSwitchConditions = graphTasks.OfType<CharacterBehavior.SwitchWeaponCondition>().ToArray();
            var aiWeaponSwitchActions = graphTasks.OfType<CharacterBehavior.SwitchWeapon>().ToArray();
            var aiWeaponSwitchSequences = graphTasks.OfType<Sequence>().Where(sequence =>
                sequence.Children != null &&
                sequence.Children.Any(child => child is CharacterBehavior.SwitchWeaponCondition) &&
                sequence.Children.Any(child => child is CharacterBehavior.SwitchWeapon)).ToArray();
            var requestReloadSequences = graphTasks.OfType<Sequence>().Where(sequence =>
                sequence.Children != null &&
                sequence.Children.Any(child => child is CharacterBehavior.RequestedReload) &&
                sequence.Children.Any(child => child is CharacterBehavior.ChangeMagazine)).ToArray();

            if (actor == "Operator")
            {
                Require(magazineActions.Length == 2,
                    $"Recovered Operator graph should serialize its two player/AI ChangeMagazine actions; found {magazineActions.Length}");
                Require(reloadRequests.Length == 1,
                    $"Recovered Operator graph should serialize one RequestedReload conditional; found {reloadRequests.Length}");
                Require(requestReloadSequences.Length == 1,
                    $"RequestedReload must share its authored Sequence with the player ChangeMagazine action; found {requestReloadSequences.Length} matching sequences");
                Require(!magazineActions.Any(action => !taskPaths.ContainsKey(action)),
                    "An Operator ChangeMagazine action is detached from Entry/Root and not reachable in the serialized behavior graph");
                Require(aiWeaponSwitchConditions.Length == 1 && aiWeaponSwitchActions.Length == 1 && aiWeaponSwitchSequences.Length == 1 &&
                        taskPaths.ContainsKey(aiWeaponSwitchConditions[0]) && taskPaths.ContainsKey(aiWeaponSwitchActions[0]),
                    $"Recovered Operator AI weapon-fallback route should have one reachable SwitchWeaponCondition → SwitchWeapon sibling Sequence; condition={aiWeaponSwitchConditions.Length}, action={aiWeaponSwitchActions.Length}, sequence={aiWeaponSwitchSequences.Length}");
            }

            reports.Add(new
            {
                actor,
                behaviorTrees = trees.Length,
                behaviorTreeComponentsEnabledInPrefab = trees.Count(tree => tree.enabled),
                graphTasks = graphTasks.Count,
                unknownTasks = 0,
                conditionalEvaluators = embeddedConditions.Count,
                embeddedConditionsResolved = embeddedConditions.Count(task => task.conditionalTask != null),
                operatorReloadGraph = actor == "Operator" ? new
                {
                    requestedReloadCount = reloadRequests.Length,
                    requestedReloadDirectSequenceCount = requestReloadSequences.Length,
                    changeMagazineActions = magazineActions.Select(action => new
                    {
                        action.ID,
                        action.FriendlyName,
                        path = taskPaths.GetValueOrDefault(action, "<unreachable>"),
                        action.isPlayer,
                        action.quick
                    }).ToArray(),
                    legacyReloadTaskCount = reloadTask.Length,
                    serializedTaskTypesContainCharacterBehaviorReload = graphTasks.Any(task => task.GetType().FullName == "CharacterBehavior.Reload")
                } : null,
                operatorAiWeaponFallback = actor == "Operator" ? new
                {
                    switchWeaponConditionCount = aiWeaponSwitchConditions.Length,
                    switchWeaponActionCount = aiWeaponSwitchActions.Length,
                    conditionActionSequenceCount = aiWeaponSwitchSequences.Length,
                    conditions = aiWeaponSwitchConditions.Select(task => new { task.ID, task.FriendlyName, path = taskPaths.GetValueOrDefault(task, "<unreachable>") }).ToArray(),
                    actions = aiWeaponSwitchActions.Select(task => new { task.ID, task.FriendlyName, path = taskPaths.GetValueOrDefault(task, "<unreachable>") }).ToArray()
                } : null
            });
        }

        return System.Text.Json.JsonSerializer.Serialize(new
        {
            status = "PASS",
            serialization = "legacy Mono string hashes for authored Behavior Designer v1.5.7–1.5.8 fields",
            actors = reports
        });
    }
}

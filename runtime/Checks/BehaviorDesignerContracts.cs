using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Crisis.Runtime;
using UnityEngine;
using BDTask = BehaviorDesigner.Runtime.Tasks.Task;

public static class BehaviorDesignerContracts
{
    public static string Run(string packageText)
    {
        var world = new RuntimeWorld();
        world.MakeCurrent();
        Resources.Configure(packageText, world);
        var reports = new List<object>();

        foreach (var actor in new[] { "Operator", "Occupant" })
        {
            var path = $"Assets/Resources/characters/{actor}.prefab";
            var prefab = SceneLoader.LoadPrefab(packageText, path, world, requireComplete: false);
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

            reports.Add(new
            {
                actor,
                behaviorTrees = trees.Length,
                graphTasks = graphTasks.Count,
                unknownTasks = 0,
                conditionalEvaluators = embeddedConditions.Count,
                embeddedConditionsResolved = embeddedConditions.Count(task => task.conditionalTask != null)
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

using Crisis.Runtime;
using System.Text.Json;

Console.WriteLine(RuntimeContracts.Run());
if(args.Length>0){
    var package=File.ReadAllText(args[0]);
    foreach(var path in new[]{"Assets/Scenes/Marksman range.unity","Assets/Scenes/Mission 01.unity"}){
        var loaded=SceneLoader.Load(package,path);
        if(loaded.unresolvedReferences.Count>0)throw new Exception("Unresolved supported fields");
        Console.WriteLine(JsonSerializer.Serialize(loaded.Report()));
    }
}

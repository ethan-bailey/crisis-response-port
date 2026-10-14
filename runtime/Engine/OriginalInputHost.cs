using System.Reflection;
using UnityEngine;

namespace Crisis.Runtime;

public sealed class OriginalInputHost {
    public readonly RuntimeWorld world;
    public readonly InputHandler handler;
    public InputMapping mapping=>handler.Keys;
    readonly MethodInfo updateKeys=typeof(InputHandler).GetMethod("UpdateKeys",BindingFlags.NonPublic|BindingFlags.Instance);
    public OriginalInputHost(RuntimeWorld world){this.world=world;handler=new GameObject("Original input handler",world).AddComponent<InputHandler>();handler.SetMapping(new InputMapping());}
    public void SetAction(string name,bool pressed){var field=typeof(InputMapping).GetField(name);if(field==null||field.FieldType!=typeof(InputKey))throw new ArgumentException($"Unknown original action {name}");var key=(InputKey)field.GetValue(mapping);if(key.IsAxis)throw new ArgumentException("Use SetAxis for scroll actions");Input.SetKey(key.key,pressed);}
    public void Tick(float seconds)=>world.Tick(seconds,()=>updateKeys.Invoke(handler,null));
    public object Snapshot()=>typeof(InputMapping).GetFields().Where(f=>f.FieldType==typeof(InputKey)).ToDictionary(f=>f.Name,f=>{
        var k=(InputKey)f.GetValue(mapping);return new {k.Pressed,k.PressedLong,k.Down,k.DownLong,k.Up,k.UpShort,k.UpLong,k.DownHandled,k.UpHandled,k.Enabled,k.axisValue,key=k.KeyName};
    });
}

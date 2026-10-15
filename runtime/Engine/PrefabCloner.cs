using System.Collections;
using System.Reflection;

namespace UnityEngine;

public static class PrefabCloner {
    public static T Clone<T>(T original,Transform parent=null,bool worldPositionStays=true,Vector3? position=null,Quaternion? rotation=null) where T:Object {
        if(!original)throw new ArgumentException("Cannot instantiate a null/destroyed original object");
        if(original is Texture2D texture)return (T)(Object)texture.CloneTexture(parent?.world??RuntimeWorld.Current);
        var root=original as GameObject??(original as Component)?.gameObject;
        if(root==null)throw new NotSupportedException($"Asset cloning for {original.GetType().Name} is not implemented");
        var world=parent?.world??RuntimeWorld.Current;
        var map=new Dictionary<Object,Object>(ReferenceEqualityComparer.Instance);
        using var deferred=world.DeferActivation();
        try {
        void Allocate(Transform source,Transform targetParent){
            var sourceGO=source.gameObject;
            var target=new GameObject(sourceGO.name,world,source is RectTransform){layer=sourceGO.layer,tag=sourceGO.tag,isStatic=sourceGO.isStatic};
            target.SetActive(sourceGO.activeSelf);map[sourceGO]=target;map[source]=target.transform;
            target.transform.SetParent(targetParent,false);
            target.transform.localPosition=source.localPosition;target.transform.localRotation=source.localRotation;target.transform.localScale=source.localScale;
            foreach(var component in sourceGO.components.Where(c=>c&&c is not Transform)){
                var copy=target.AddComponent(component.GetType());map[component]=copy;
                if(component is Behaviour b&&copy is Behaviour copied)copied.enabled=b.enabled;
            }
            foreach(var child in source.children)Allocate(child,target.transform);
        }
        Allocate(root.transform,null);
        object CopyValue(object value,Dictionary<object,object> values){
            if(value==null)return null;
            if(value is Object reference)return map.TryGetValue(reference,out var replacement)?replacement:reference;
            var type=value.GetType();
            if(type.IsPrimitive||type.IsEnum||type==typeof(string)||value is Vector2 or Vector3 or Quaternion or Keyframe)return value;
            if(values.TryGetValue(value,out var existing))return existing;
            if(value is AnimationCurve curve)return new AnimationCurve(curve.keys){clampOutside=curve.clampOutside,serializedPreInfinity=curve.serializedPreInfinity,serializedPostInfinity=curve.serializedPostInfinity};
            if(value is Array array){var output=Array.CreateInstance(type.GetElementType(),array.Length);values[value]=output;
                for(int i=0;i<array.Length;i++)output.SetValue(CopyValue(array.GetValue(i),values),i);return output;}
            if(value is IList list){var output=(IList)Activator.CreateInstance(type);values[value]=output;foreach(var item in list)output.Add(CopyValue(item,values));return output;}
            if(type.GetCustomAttribute<SerializableAttribute>()==null)throw new NotSupportedException($"Serialized clone value {type.FullName} is not implemented");
            var result=Activator.CreateInstance(type);values[value]=result;
            foreach(var field in SerializedFields(type))field.SetValue(result,CopyValue(field.GetValue(value),values));
            return result;
        }
        foreach(var pair in map){
            if(pair.Key is not Component source||source is Transform)continue;
            var values=new Dictionary<object,object>(ReferenceEqualityComparer.Instance);
            foreach(var field in SerializedFields(source.GetType()))field.SetValue(pair.Value,CopyValue(field.GetValue(source),values));
            if(source is BoxCollider box&&pair.Value is BoxCollider target){target.center=box.center;target.size=box.size;target.isTrigger=box.isTrigger;}
            if(source is Light light&&pair.Value is Light copy)copy.intensity=light.intensity;
        }
        var cloneRoot=(GameObject)map[root];cloneRoot.name=root.name+"(Clone)";
        if(worldPositionStays){cloneRoot.transform.position=root.transform.position;cloneRoot.transform.rotation=root.transform.rotation;}
        if(parent!=null)cloneRoot.transform.SetParent(parent,worldPositionStays);
        if(position.HasValue)cloneRoot.transform.position=position.Value;
        if(rotation.HasValue)cloneRoot.transform.rotation=rotation.Value;
        return (T)map[original];
        }catch{
            // No original Awake/OnEnable callback may observe a failed partial clone.
            if(map.TryGetValue(root,out var failed))world.DestroyNow(failed);
            throw;
        }
    }
    static IEnumerable<FieldInfo> SerializedFields(Type type){
        for(var t=type;t!=null&&t!=typeof(Object)&&t!=typeof(Component)&&t!=typeof(Behaviour)&&t!=typeof(MonoBehaviour);t=t.BaseType)
            foreach(var field in t.GetFields(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.DeclaredOnly))
                if(!field.IsStatic&&!field.IsInitOnly&&field.GetCustomAttribute<NonSerializedAttribute>()==null&&(field.IsPublic||field.GetCustomAttribute<SerializeField>()!=null))yield return field;
    }
}

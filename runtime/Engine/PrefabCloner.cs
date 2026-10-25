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
        var clonedBehaviours=new List<MonoBehaviour>();
        using var deferred=world.DeferActivation();
        try {
        void Allocate(Transform source,Transform targetParent){
            var sourceGO=source.gameObject;
            var target=new GameObject(sourceGO.name,world,source is RectTransform){layer=sourceGO.layer,tag=sourceGO.tag,isStatic=sourceGO.isStatic};
            CopySourceIdentity(sourceGO,target);
            target.SetActive(sourceGO.activeSelf);map[sourceGO]=target;map[source]=target.transform;
            target.transform.SetParent(targetParent,false);
            target.transform.localPosition=source.localPosition;target.transform.localRotation=source.localRotation;target.transform.localScale=source.localScale;
            if(source is RectTransform sourceRect&&target.transform is RectTransform targetRect){
                targetRect.anchorMin=sourceRect.anchorMin;targetRect.anchorMax=sourceRect.anchorMax;targetRect.anchoredPosition3D=sourceRect.anchoredPosition3D;
                targetRect.sizeDelta=sourceRect.sizeDelta;targetRect.pivot=sourceRect.pivot;
            }
            foreach(var component in sourceGO.components.Where(c=>c&&c is not Transform)){
                var copy=target.AddComponent(component.GetType());map[component]=copy;
                CopySourceIdentity(component,copy);
                foreach(var field in component.OriginalSerializedFields)
                    copy.OriginalSerializedFields[field.Key]=field.Value.Clone();
                if(copy is MonoBehaviour behaviour)clonedBehaviours.Add(behaviour);
                if(component is Behaviour b&&copy is Behaviour copied)copied.enabled=b.enabled;
            }
            foreach(var child in source.children)Allocate(child,target.transform);
        }
        Allocate(root.transform,null);
        bool IsUnityEventType(Type candidate){for(var current=candidate;current!=null;current=current.BaseType){if(current.FullName=="UnityEngine.Events.UnityEvent")return true;if(current.IsGenericType&&current.Namespace=="UnityEngine.Events"&&current.Name.StartsWith("UnityEvent",StringComparison.Ordinal))return true;}return false;}
        object CopyValue(object value,Dictionary<object,object> values){
            if(value==null)return null;
            if(value is Object reference)return map.TryGetValue(reference,out var replacement)?replacement:reference;
            var type=value.GetType();
            if(type.IsPrimitive||type.IsEnum||type==typeof(string)||value is Vector2 or Vector3 or Quaternion or Keyframe or Color)return value;
            if(values.TryGetValue(value,out var existing))return existing;
            if(value is AnimationCurve curve)return new AnimationCurve(curve.keys){preWrapMode=curve.preWrapMode,postWrapMode=curve.postWrapMode,clampOutside=curve.clampOutside,serializedPreInfinity=curve.serializedPreInfinity,serializedPostInfinity=curve.serializedPostInfinity,serializedVersion=curve.serializedVersion,serializedRotationOrder=curve.serializedRotationOrder};
            if(value is Array array){var output=Array.CreateInstance(type.GetElementType(),array.Length);values[value]=output;
                for(int i=0;i<array.Length;i++)output.SetValue(CopyValue(array.GetValue(i),values),i);return output;}
            if(value is IList list){var output=(IList)Activator.CreateInstance(type);values[value]=output;foreach(var item in list)output.Add(CopyValue(item,values));return output;}
            if(IsUnityEventType(type)){for(var candidate=type;candidate!=null&&candidate.FullName!="UnityEngine.Events.UnityEvent"&&!(candidate.IsGenericType&&candidate.Namespace=="UnityEngine.Events"&&candidate.Name.StartsWith("UnityEvent",StringComparison.Ordinal));candidate=candidate.BaseType){foreach(var field in candidate.GetFields(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.DeclaredOnly))if(!field.IsStatic)throw new NotSupportedException($"UnityEvent subclass state is not implemented: {type.FullName}.{field.Name}");}var eventCopy=Activator.CreateInstance(type,true);if(eventCopy==null)throw new NotSupportedException($"UnityEvent {type.FullName} has no constructible empty state");values[value]=eventCopy;return eventCopy;}if(type.IsValueType){var valueCopy=Activator.CreateInstance(type)??throw new NotSupportedException($"Value type {type.FullName} has no default state");values[value]=valueCopy;for(var candidate=type;candidate!=null&&candidate!=typeof(object);candidate=candidate.BaseType){foreach(var field in candidate.GetFields(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.DeclaredOnly)){if(field.IsStatic)continue;try{field.SetValue(valueCopy,CopyValue(field.GetValue(value),values));}catch(Exception error){throw new NotSupportedException($"Value type field clone is not implemented: {type.FullName}.{field.Name}",error);}}}return valueCopy;}if(type.FullName=="Crisis.Runtime.FullSource.CharacterPsychologyDriver"){var driverCopy=Activator.CreateInstance(type)??throw new NotSupportedException("CharacterPsychologyDriver has no source default constructor");values[value]=driverCopy;return driverCopy;}if(type.GetCustomAttribute<SerializableAttribute>()==null)throw new NotSupportedException($"Serialized clone value {type.FullName} is not implemented");
            object result;
            try{result=Activator.CreateInstance(type,true)!;}
            catch(MissingMethodException){result=System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(type);}
            values[value]=result;
            foreach(var field in SerializedFields(type))field.SetValue(result,CopyValue(field.GetValue(value),values));
            return result;
        }
        foreach(var pair in map){
            if(pair.Key is not Component source||source is Transform)continue;
            var values=new Dictionary<object,object>(ReferenceEqualityComparer.Instance);
            foreach(var field in SerializedFields(source.GetType()))field.SetValue(pair.Value,CopyValue(field.GetValue(source),values));
            if(source is BoxCollider box&&pair.Value is BoxCollider target){target.center=box.center;target.size=box.size;target.isTrigger=box.isTrigger;}
            if(source is Light light&&pair.Value is Light copy)copy.intensity=light.intensity;
            if(source is Canvas canvas&&pair.Value is Canvas canvasCopy)canvasCopy.scaleFactor=canvas.scaleFactor;
            if(source is ICloneReferenceState referenceState)referenceState.CopyCloneReferenceState((Component)pair.Value);
        }
        var cloneRoot=(GameObject)map[root];cloneRoot.name=root.name+"(Clone)";
        if(worldPositionStays){cloneRoot.transform.position=root.transform.position;cloneRoot.transform.rotation=root.transform.rotation;}
        if(parent!=null)cloneRoot.transform.SetParent(parent,worldPositionStays);
        if(position.HasValue)cloneRoot.transform.position=position.Value;
        if(rotation.HasValue)cloneRoot.transform.rotation=rotation.Value;
        // Unity's Instantiate completes Awake/OnEnable for an active clone before
        // returning, including when called from another component's Awake. The
        // world's normal activation sweep is reentrant-safe and therefore defers
        // newly created objects until the current callback returns; explicitly
        // initialize this fully mapped clone now to preserve Instantiate semantics.
        deferred.Dispose();
        if(world.Initialized)
            foreach(var behaviour in clonedBehaviours)
                world.Activate(behaviour);
        return (T)map[original];
        }catch{
            // No original Awake/OnEnable callback may observe a failed partial clone.
            if(map.TryGetValue(root,out var failed))world.DestroyNow(failed);
            throw;
        }
    }
    static void CopySourceIdentity(Object source,Object target){
        target.OriginalAssetGuid=source.OriginalAssetGuid;
        target.OriginalAssetFileID=source.OriginalAssetFileID;
        target.OriginalAssetPath=source.OriginalAssetPath;
        target.OriginalSerializedSha256=source.OriginalSerializedSha256;
    }
    static IEnumerable<FieldInfo> SerializedFields(Type type){
        for(var t=type;t!=null&&t!=typeof(Object)&&t!=typeof(Component)&&t!=typeof(Behaviour)&&t!=typeof(MonoBehaviour);t=t.BaseType)
            foreach(var field in t.GetFields(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.DeclaredOnly))
                if(!field.IsStatic&&!field.IsInitOnly&&field.GetCustomAttribute<NonSerializedAttribute>()==null&&!typeof(Delegate).IsAssignableFrom(field.FieldType)&&(field.IsPublic||field.GetCustomAttribute<SerializeField>()!=null))yield return field;
    }
}

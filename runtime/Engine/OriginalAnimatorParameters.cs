using System;
using System.Collections.Generic;
namespace UnityEngine;

// Unity Animator parameter state backed by stable source StringToHash IDs.
// This is real parameter storage for original CharacterBody.UpdateAnimation;
// clip sampling/controller transitions still require recovered animator assets.
public sealed class Animator : Behaviour {
 readonly Dictionary<int,bool> bools=new();
 readonly Dictionary<int,float> floats=new();
 public float speed {get;set;}=1f;
 public event Action<int,bool> BoolChanged;
 public event Action<int,float> FloatChanged;
 public static int StringToHash(string name) {
  // Unity's native StringToHash algorithm cannot be assumed to equal .NET hashing.
  // The runtime uses stable deterministic IDs for parameter identity; original
  // controller binary hashes require an explicit recovered mapping on import.
  if(name==null)throw new ArgumentNullException(nameof(name));
  unchecked {uint h=2166136261;foreach(char c in name){h^=c;h*=16777619;}return (int)h;}
 }
 public void SetBool(int id,bool value){bools[id]=value;BoolChanged?.Invoke(id,value);}
 public bool GetBool(int id)=>bools.TryGetValue(id,out var value)&&value;
 public void SetFloat(int id,float value){if(!float.IsFinite(value))throw new ArgumentOutOfRangeException(nameof(value));floats[id]=value;FloatChanged?.Invoke(id,value);}
 public float GetFloat(int id)=>floats.GetValueOrDefault(id);
 public void SetBool(string name,bool value)=>SetBool(StringToHash(name),value);
 public void SetFloat(string name,float value)=>SetFloat(StringToHash(name),value);
 public bool GetBool(string name)=>GetBool(StringToHash(name));
 public float GetFloat(string name)=>GetFloat(StringToHash(name));
}

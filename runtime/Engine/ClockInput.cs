using System.Collections;

namespace UnityEngine;

public static class Time {
    public static float timeScale {get;set;}=1;
    public static float fixedDeltaTime {get;set;}=.02f;
    public static float maximumDeltaTime {get;set;}=.33333334f;
    public static float time {get;private set;}
    public static float fixedTime {get;private set;}
    public static float deltaTime {get;private set;}
    public static float unscaledDeltaTime {get;private set;}
    public static float realtimeSinceStartup {get;private set;}
    public static int frameCount {get;private set;}
    static float accumulator,frameTime,frameDelta;
    internal static void Reset(){timeScale=1;fixedDeltaTime=.02f;maximumDeltaTime=.33333334f;time=fixedTime=deltaTime=unscaledDeltaTime=realtimeSinceStartup=accumulator=frameTime=frameDelta=0;frameCount=0;}
    internal static void BeginFrame(float elapsed){
        if(!float.IsFinite(elapsed)||elapsed<0)throw new ArgumentOutOfRangeException(nameof(elapsed));
        if(fixedDeltaTime<=0||timeScale<0)throw new InvalidOperationException("Invalid Unity time settings");
        unscaledDeltaTime=elapsed;realtimeSinceStartup+=elapsed;frameCount++;
        frameDelta=MathF.Min(elapsed*timeScale,maximumDeltaTime);frameTime+=frameDelta;accumulator+=frameDelta;
    }
    internal static bool BeginFixedStep(){if(accumulator+1e-7f<fixedDeltaTime)return false;accumulator-=fixedDeltaTime;fixedTime+=fixedDeltaTime;time=fixedTime;deltaTime=fixedDeltaTime;return true;}
    internal static void BeginUpdate(){time=frameTime;deltaTime=frameDelta;}
}
public static class Input {
    static readonly HashSet<KeyCode> held=[],previous=[],down=[],up=[];
    static readonly Dictionary<string,float> axes=[];
    public static Vector3 mousePosition {get;set;}
    public static bool anyKeyDown=>down.Count>0;
    public static bool GetKey(KeyCode key)=>held.Contains(key);
    public static bool GetKeyDown(KeyCode key)=>down.Contains(key);
    public static bool GetKeyUp(KeyCode key)=>up.Contains(key);
    public static float GetAxis(string name)=>axes.GetValueOrDefault(name);
    public static void SetKey(KeyCode key,bool pressed){if(pressed)held.Add(key);else held.Remove(key);}
    public static void SetAxis(string name,float value){if(!float.IsFinite(value))throw new ArgumentException("Non-finite input");axes[name]=value;}
    public static void ReleaseAll(){held.Clear();axes.Clear();}
    internal static void Reset(){ReleaseAll();previous.Clear();down.Clear();up.Clear();mousePosition=Vector3.zero;}
    internal static void BeginFrame(){down.Clear();up.Clear();down.UnionWith(held.Except(previous));up.UnionWith(previous.Except(held));}
    internal static void EndFrame(){previous.Clear();previous.UnionWith(held);axes.Clear();}
}
public sealed class WaitForSeconds(float seconds) {internal readonly float seconds=seconds;}
public sealed class WaitForSecondsRealtime(float seconds) {internal readonly float seconds=seconds;}
public sealed class CoroutineHandle {
    internal readonly MonoBehaviour owner;
    internal bool finished;
    readonly Stack<IEnumerator> stack=new();
    float wake;int frame;bool real;
    internal CoroutineHandle(MonoBehaviour owner,IEnumerator routine){this.owner=owner;stack.Push(routine);}
    internal void Advance(bool immediate=false){
        if(finished||!owner||!owner.gameObject.activeInHierarchy){finished=true;return;}
        if(!immediate&&(Time.frameCount<frame||(real?Time.realtimeSinceStartup:Time.time)+1e-7f<wake))return;
        while(stack.Count>0){var routine=stack.Peek();if(!routine.MoveNext()){stack.Pop();continue;}
            switch(routine.Current){
                case IEnumerator nested:stack.Push(nested);continue;
                case WaitForSeconds wait:real=false;wake=Time.time+wait.seconds;break;
                case WaitForSecondsRealtime wait:real=true;wake=Time.realtimeSinceStartup+wait.seconds;break;
                case null:real=false;wake=Time.time;break;
                default:throw new NotSupportedException($"Coroutine yield {routine.Current.GetType()} is not implemented");
            }frame=Time.frameCount+1;return;
        }finished=true;
    }
}

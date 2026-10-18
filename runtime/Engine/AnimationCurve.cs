namespace UnityEngine;

public struct Keyframe(float time,float value,float inTangent,float outTangent) {
    public float time=time,value=value,inTangent=inTangent,outTangent=outTangent;
    public int tangentMode;
}
public sealed class AnimationCurve {
    Keyframe[] values;
    public WrapMode preWrapMode {get;set;}=WrapMode.Default;
    public WrapMode postWrapMode {get;set;}=WrapMode.Default;
    public int AddKey(Keyframe key){
        if(values.Any(k=>k.time==key.time))return -1;
        values=values.Append(key).OrderBy(k=>k.time).ToArray();return Array.FindIndex(values,k=>k.time==key.time);
    }
    public Keyframe[] keys=>(Keyframe[])values.Clone();
    public int length=>values.Length;
    public bool clampOutside {get;init;}
    public int serializedPreInfinity {get;init;}
    public int serializedPostInfinity {get;init;}
    public AnimationCurve(params Keyframe[] keys){values=keys.OrderBy(k=>k.time).ToArray();
        for(int i=1;i<values.Length;i++)if(values[i].time<=values[i-1].time)throw new NotSupportedException("Duplicate curve key time");}
    public float Evaluate(float time){
        if(!float.IsFinite(time))throw new ArgumentOutOfRangeException(nameof(time));
        if(values.Length==0)return 0;
        if(values.Length==1)return values[0].value;
        var mode=time<values[0].time?preWrapMode:postWrapMode;
        if(time<values[0].time||time>values[^1].time){
            float first=values[0].time,last=values[^1].time,duration=last-first;
            if(mode is WrapMode.Once or WrapMode.ClampForever)time=Math.Clamp(time,first,last);
            else if(mode==WrapMode.Loop)time=first+((time-first)%duration+duration)%duration;
            else if(mode==WrapMode.PingPong){float phase=((time-first)%(duration*2)+duration*2)%(duration*2);time=first+(phase>duration?duration*2-phase:phase);}
        }
        // Retain infinity metadata without guessing the Unity native mapping.
        // Until it is verified, extrapolation is explicitly unsupported.
        if(clampOutside)time=Math.Clamp(time,values[0].time,values[^1].time);
        if(time<values[0].time||time>values[^1].time)throw new NotSupportedException("AnimationCurve extrapolation/wrap semantics are not parity-verified");
        if(time==values[^1].time)return values[^1].value;
        int index=0;while(index+1<values.Length&&values[index+1].time<=time)index++;
        var a=values[index];var b=values[index+1];
        if(time==a.time)return a.value;
        if(float.IsInfinity(a.outTangent)||float.IsInfinity(b.inTangent))return a.value;
        float width=b.time-a.time,t=(time-a.time)/width,t2=t*t,t3=t2*t;
        return (2*t3-3*t2+1)*a.value+(t3-2*t2+t)*a.outTangent*width+(-2*t3+3*t2)*b.value+(t3-t2)*b.inTangent*width;
    }
}


public enum WrapMode {Default=0,Once=1,Clamp=1,Loop=2,PingPong=4,ClampForever=8}

#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace UnityEngine;

/// <summary>Source-data-driven subset of Unity Animator transition evaluation; it does not sample clips or poses.</summary>
public sealed class AnimatorGraphDriver : IAnimatorDriver {
    readonly Dictionary<string,float> numbers=new(StringComparer.Ordinal);
    readonly HashSet<string> bools=new(StringComparer.Ordinal);
    readonly HashSet<string> triggers=new(StringComparer.Ordinal);
    readonly List<AnimatorGraphTransition> transitions;
    readonly Dictionary<string,float> stateDurations;
    float stateElapsed;

    public AnimatorGraphDriver(string initialState,IEnumerable<AnimatorGraphTransition> transitions,IReadOnlyDictionary<string,float>? stateDurations=null) {
        if(string.IsNullOrWhiteSpace(initialState))throw new ArgumentException("An initial Animator state is required.",nameof(initialState));
        CurrentState=initialState;
        this.transitions=transitions?.ToList()??throw new ArgumentNullException(nameof(transitions));
        if(this.transitions.Any(t=>t is null||string.IsNullOrWhiteSpace(t.SourceState)||string.IsNullOrWhiteSpace(t.DestinationState)||t.Conditions is null))throw new ArgumentException("Animator transitions must have source, destination, and condition data.",nameof(transitions));
        this.stateDurations=stateDurations is null?new Dictionary<string,float>(StringComparer.Ordinal):new Dictionary<string,float>(stateDurations,StringComparer.Ordinal);
        if(this.stateDurations.Any(p=>string.IsNullOrWhiteSpace(p.Key)||!float.IsFinite(p.Value)||p.Value<=0))throw new ArgumentException("State durations must be finite positive seconds.",nameof(stateDurations));
    }
    public string CurrentState { get; private set; }
    public float NormalizedTime => stateDurations.TryGetValue(CurrentState,out var duration)?stateElapsed/duration:stateElapsed;
    public event Action<string,string>? StateChanged;
    public void SetFloat(string name,float value)=>numbers[RequireName(name)]=value;
    public void SetInteger(string name,int value)=>numbers[RequireName(name)]=value;
    public void SetBool(string name,bool value) { name=RequireName(name);if(value)bools.Add(name);else bools.Remove(name); }
    public void SetTrigger(string name)=>triggers.Add(RequireName(name));
    public void ResetTrigger(string name)=>triggers.Remove(RequireName(name));
    static string RequireName(string name)=>!string.IsNullOrWhiteSpace(name)?name:throw new ArgumentException("Animator parameter name is required.",nameof(name));

    public void SetParameter(AnimatorParameter parameter) {
        switch(parameter.Type) {
            case AnimatorControllerParameterType.Float: SetFloat(parameter.Name,Convert.ToSingle(parameter.Value,CultureInfo.InvariantCulture));break;
            case AnimatorControllerParameterType.Int: SetInteger(parameter.Name,Convert.ToInt32(parameter.Value,CultureInfo.InvariantCulture));break;
            case AnimatorControllerParameterType.Bool: SetBool(parameter.Name,Convert.ToBoolean(parameter.Value,CultureInfo.InvariantCulture));break;
            case AnimatorControllerParameterType.Trigger: if(Convert.ToBoolean(parameter.Value,CultureInfo.InvariantCulture))SetTrigger(parameter.Name);else ResetTrigger(parameter.Name);break;
            default: throw new ArgumentOutOfRangeException(nameof(parameter),parameter.Type,"Unknown Animator parameter type.");
        }
    }
    public void Evaluate(float deltaTime,float speed) {
        if(!float.IsFinite(deltaTime)||deltaTime<0)throw new ArgumentOutOfRangeException(nameof(deltaTime));
        if(!float.IsFinite(speed))throw new ArgumentOutOfRangeException(nameof(speed));
        stateElapsed+=deltaTime*speed;
        var normalized=NormalizedTime;
        foreach(var transition in transitions) {
            if(!string.Equals(transition.SourceState,CurrentState,StringComparison.Ordinal))continue;
            if(transition.HasExitTime&&normalized<transition.ExitTime)continue;
            if(!transition.Conditions.All(ConditionIsMet))continue;
            var old=CurrentState;CurrentState=transition.DestinationState;stateElapsed=0;
            foreach(var condition in transition.Conditions)if(condition.Mode==1&&triggers.Contains(condition.Parameter))triggers.Remove(condition.Parameter);
            StateChanged?.Invoke(old,CurrentState);break;
        }
    }
    bool ConditionIsMet(AnimatorGraphCondition c) {
        numbers.TryGetValue(c.Parameter,out var value);
        return c.Mode switch {
            1=>triggers.Contains(c.Parameter)||bools.Contains(c.Parameter),
            2=>!triggers.Contains(c.Parameter)&&!bools.Contains(c.Parameter),
            3=>value>c.Threshold,
            4=>value<c.Threshold,
            6=>value==c.Threshold,
            7=>value!=c.Threshold,
            _=>false
        };
    }
}

/// <summary>Original Unity AnimatorConditionMode integer values.</summary>
public readonly record struct AnimatorGraphCondition(string Parameter,int Mode,float Threshold);
/// <summary>Directed state-machine edge decoded from an original controller asset.</summary>
public sealed record AnimatorGraphTransition(string SourceState,string DestinationState,IReadOnlyList<AnimatorGraphCondition> Conditions,bool HasExitTime=false,float ExitTime=0);

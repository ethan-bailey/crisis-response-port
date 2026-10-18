using FullSerializer;
using UnityEngine;
using Crisis.Runtime.FullSource;

static class SerializationContracts {
    public static int Run() {
        int count=0;
        void Check(bool ok,string label) { if(!ok)throw new Exception(label);count++; }
        T RoundTrip<T>(T original) {
            var serializer=new fsSerializer();
            serializer.TrySerialize(typeof(T),original,out var data).AssertSuccessWithoutWarnings();
            var json=fsJsonPrinter.CompressedJson(data);
            object result=null;
            serializer.TryDeserialize(fsJsonParser.Parse(json),typeof(T),ref result).AssertSuccessWithoutWarnings();
            return (T)result;
        }
        var graph=new Graph { Name="root" };graph.Next=graph;
        var restored=RoundTrip(graph);
        Check(restored.Name=="root"&&ReferenceEquals(restored,restored.Next),"original serializer restores cyclic object identity");
        var opted=RoundTrip(new OptIn { Saved=7,Ignored=99 });
        Check(opted.Saved==7&&opted.Ignored==0,"original fsProperty opt-in excludes runtime fields");
        var oldAudio=new SerializableAudioSettings_v1 { voiceVolume=-23, effectsVolume=-9 };
        var oldCopy=RoundTrip(oldAudio);
        var audio=new SerializableAudioSettings_v2(oldCopy);
        Check(audio.voiceVolume==-23&&audio.effectsVolume==-9&&audio.userInterfaceVolume==-12,"original audio version migration preserves channels and initializes the new channel");
        var migrationSerializer=new fsSerializer();
        migrationSerializer.TrySerialize(typeof(SerializableAudioSettings_v1),oldAudio,out var legacyData).AssertSuccessWithoutWarnings();
        object migrated=null;
        migrationSerializer.TryDeserialize(legacyData,typeof(SerializableAudioSettings_v2),ref migrated).AssertSuccessWithoutWarnings();
        Check(migrated is SerializableAudioSettings_v2 upgraded&&upgraded.voiceVolume==-23&&upgraded.userInterfaceVolume==-12,"original version metadata selects and executes the migration constructor");
        var entry=new SerializableBulletTimeEntry(true,0.15f,0.2f,0.5f,0.7f);
        var copy=RoundTrip(entry);
        Check(!entry.Different(copy),"original bullet-time settings round trip preserves every parameter");
        var curve=new AnimationCurve(new Keyframe(0,0,0,0),new Keyframe(1,1,0,0)) { preWrapMode=WrapMode.ClampForever, postWrapMode=WrapMode.Loop };
        var curveCopy=RoundTrip(curve);
        Check(curveCopy.keys.Length==2&&MathF.Abs(curveCopy.Evaluate(0.5f)-0.5f)<1e-5f&&curveCopy.postWrapMode==WrapMode.Loop,"original curve direct converter preserves keys, tangents and wrapping");
        var rect=RoundTrip(new Rect(3,5,7,11));
        Check(rect.x==3&&rect.y==5&&rect.width==7&&rect.height==11,"original rectangle direct converter preserves all four edges");
        var gradient=new Gradient();gradient.SetKeys([new(new(1,0,0,1),0),new(new(0,0,1,1),1)],[new(0,0),new(1,1)]);
        var gradientCopy=RoundTrip(gradient);var c=gradientCopy.Evaluate(0.25f);
        Check(MathF.Abs(c.r-0.75f)<1e-5f&&MathF.Abs(c.b-0.25f)<1e-5f&&MathF.Abs(c.a-0.25f)<1e-5f,"original gradient converter retains independent color and alpha keys");
        var callbacks=new CallbackRecord { Value=4 };
        var callbackCopy=RoundTrip(callbacks);
        Check(callbacks.Before==1&&callbackCopy.After==1&&callbackCopy.Value==4,"original serializer invokes engine callbacks around field serialization");
        var ev=new UnityEngine.Events.UnityEvent<int,int>();int sum=0;ev.AddListener((a,b)=>sum=a+b);ev.Invoke(3,8);
        Check(sum==11,"two-argument Unity event forwards both original values");
        ev.RemoveAllListeners();ev.Invoke(1,1);Check(sum==11,"typed event clearing removes prior listeners");
        var quality=new QualityProbe();QualitySettings.Bind(RuntimeWorld.Current,quality);
        QualityLevels.SetQualityLevel(QualityLevels.QualityLevel.Fastest);
        Check(quality.Values.Count==20&&quality.Get<int>("pixelLightCount")==2&&quality.Get<ShadowQuality>("shadows")==ShadowQuality.Disable,"complete original fastest quality preset reaches the rendering driver");
        Check(quality.Order[0]=="pixelLightCount"&&quality.Order[^1]=="asyncUploadBufferSize","original quality application order is preserved");
        QualityLevels.SetQualityLevel(QualityLevels.QualityLevel.Fantastic);
        Check(quality.Get<int>("pixelLightCount")==64&&quality.Get<ShadowQuality>("shadows")==ShadowQuality.All,"original high quality preset replaces renderer settings");
        return count;
    }
    public class Graph { public string Name; public Graph Next; }
    [fsObject(MemberSerialization=fsMemberSerialization.OptIn)]
    public class OptIn { [fsProperty] public int Saved; public int Ignored; }
    public class CallbackRecord:ISerializationCallbackReceiver {
        public int Value;
        [fsIgnore] public int Before,After;
        public void OnBeforeSerialize()=>Before++;
        public void OnAfterDeserialize()=>After++;
    }
    class QualityProbe:IQualitySettingsDriver {
        public Dictionary<string,object> Values=[];
        public List<string> Order=[];
        public T Get<T>(string name)=>(T)Values[name];
        public void Set<T>(string name,T value) {Values[name]=value;Order.Add(name);}
    }
}

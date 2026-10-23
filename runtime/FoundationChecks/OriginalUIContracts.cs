using UnityEngine;
using UnityEngine.UI;

static class OriginalUIContracts {
    public static int Run() {
        int count=0;
        void Check(bool value,string label) {if(!value)throw new Exception(label);count++;}
        var world=new RuntimeWorld();var go=new GameObject("original hit announcement",world);
        var original=go.AddComponent<Crisis.Runtime.FullSource.UIHitAnnouncement>();
        original.fill=new GameObject("fill",world).AddComponent<Text>();
        original.outline=new GameObject("outline",world).AddComponent<Text>();
        original.scale=new AnimationCurve(new Keyframe(0,1,0,0),new Keyframe(1,2,0,0));
        original.alpha=new AnimationCurve(new Keyframe(0,1,0,0),new Keyframe(1,0,0,0));
        original.Text="Original hit";
        Check(original.fill.text=="Original hit"&&original.outline.text=="Original hit","original setter updates fill and outline together");
        var display=new GraphicProbe();original.fill.Driver=display;
        Check(display.Text=="Original hit","presentation binding synchronizes the original text state");
        original.fill.color=new Color(.2f,.4f,.6f,.8f);
        var clonedText=UnityEngine.Object.Instantiate(original.fill);
        Check(clonedText.text=="Original hit"&&clonedText.color.a==.8f,"cloned original UI retains serialized text and tint");
        Check(clonedText.Driver==null,"prefab cloning does not copy native presentation bindings");
        clonedText.text="Clone";clonedText.color=Color.clear;
        Check(original.fill.text=="Original hit"&&original.fill.color.a==.8f,"cloned UI state is independent of its source component");
        var start=new Vector3(10,20,0);original.Initialize(start);world.Tick(.25f);
        Check(MathF.Abs(go.transform.localScale.x-1.15625f)<1e-5f,"original announcement evaluates the authored scale curve");
        Check(MathF.Abs(original.fill.color.a-.84375f)<1e-5f&&original.fill.color.a==original.outline.color.a,"original announcement applies the authored alpha to both graphics");
        Check(original.fill.color.r>=.3f&&original.fill.color.r<=1&&original.outline.color.r==1,"original announcement retains random fill tint and white outline");
        Check((go.transform.position-start).magnitude<=125.001f,"original announcement uses bounded randomized travel");
        Check(display.Color.a==original.fill.color.a,"original color update reaches the presentation driver");
        world.Tick(.25f);world.Tick(.25f);world.Tick(.25f);
        Check(go==null,"original announcement destroys itself at its authored lifetime");
        world=new RuntimeWorld();go=new GameObject("original audio player",world);
        var audio=go.AddComponent<AudioSource>();var sink=new AudioProbe();audio.Driver=sink;
        var player=go.AddComponent<Crisis.Runtime.FullSource.AudioPlayer>();
        AudioClip Clip(string name) {var clip=new AudioClip(name,.1f);clip.ReportLoadState(AudioDataLoadState.Loaded);return clip;}
        player.blip=Clip("blip");player.toggle=Clip("toggle");player.benchmarkSuccess=Clip("success");player.benchmarkFailure=Clip("failure");
        world.Tick(.26f);player.PlayBlip();player.PlayToggle();
        Check(sink.Played.SequenceEqual(new[]{player.blip}),"original UI audio throttle rejects immediate repetition");
        world.Tick(.2f);player.PlayToggle();Check(sink.Played.Count==1,"original throttle preserves the quiet interval");
        world.Tick(.06f);player.PlayToggle();Check(ReferenceEquals(sink.Played[^1],player.toggle),"original UI audio resumes after the throttle interval");
        player.PlayBenchmarkResult(true);player.PlayBenchmarkResult(false);
        Check(ReferenceEquals(sink.Played[^2],player.benchmarkSuccess)&&ReferenceEquals(sink.Played[^1],player.benchmarkFailure),"original result audio selects success and failure clips in source order");
        var slider=new GameObject("timer data",world).AddComponent<Slider>();var updates=new List<float>();slider.onValueChanged.AddListener(updates.Add);
        slider.value=1.5f;slider.value=1.8f;slider.value=-.5f;
        Check(updates.SequenceEqual(new[]{1f,0f}),"slider clamps fuse fractions and only notifies actual value changes");
        var recoveredUi=new GameObject("recovered settings UI",world);
        var toggle=recoveredUi.AddComponent<Toggle>();toggle.graphic=recoveredUi.AddComponent<Image>();toggle.toggleTransition=1;
        Check(recoveredUi.AddComponent<CanvasRenderer>()!=null&&recoveredUi.AddComponent<LayoutElement>()!=null&&
            recoveredUi.AddComponent<HorizontalLayoutGroup>()!=null&&recoveredUi.AddComponent<VerticalLayoutGroup>()!=null&&
            recoveredUi.AddComponent<ContentSizeFitter>()!=null&&recoveredUi.AddComponent<Mask>()!=null&&
            recoveredUi.AddComponent<Dropdown>()!=null&&recoveredUi.AddComponent<ScrollRect>()!=null&&
            recoveredUi.AddComponent<Scrollbar>()!=null&&toggle.graphic!=null,
            "recovered Unity 2017 settings prefab component identities are available");
        return count;
    }
    sealed class GraphicProbe:IGraphicDriver {
        public string Text;public Color Color;
        public void SetColor(Graphic graphic,Color value)=>Color=value;
        public void SetText(Text text,string value)=>Text=value;
    }
    sealed class AudioProbe:IAudioSourceDriver {
        public List<AudioClip> Played=[];
        public bool IsPlaying=>Played.Count>0;
        public void PlayOneShot(AudioClip clip,float volume,int priority)=>Played.Add(clip);
        public void Stop() { }
    }
}

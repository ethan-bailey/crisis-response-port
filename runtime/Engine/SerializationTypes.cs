namespace UnityEngine;

public interface ISerializationCallbackReceiver {
    void OnBeforeSerialize();
    void OnAfterDeserialize();
}

[AttributeUsage(AttributeTargets.Class)]
public sealed class CreateAssetMenuAttribute : Attribute {
    public string fileName, menuName;
    public int order;
}
public struct Bounds(Vector3 center, Vector3 size) {
    public Vector3 center=center, size=size;
    public Vector3 extents { get=>size*0.5f; set=>size=value*2; }
    public Vector3 min { get=>center-extents; set=>SetMinMax(value,max); }
    public Vector3 max { get=>center+extents; set=>SetMinMax(min,value); }
    public void SetMinMax(Vector3 min,Vector3 max) { size=max-min;center=(min+max)*0.5f; }
    public bool Contains(Vector3 point) { var lo=min;var hi=max;return point.x>=lo.x&&point.x<=hi.x&&point.y>=lo.y&&point.y<=hi.y&&point.z>=lo.z&&point.z<=hi.z; }
}

// Value/asset records used by the original FullSerializer direct converters.
// These do not implement UI rendering or any game settings decisions.
public enum TextAnchor { UpperLeft, UpperCenter, UpperRight, MiddleLeft, MiddleCenter, MiddleRight, LowerLeft, LowerCenter, LowerRight }
public enum TextClipping { Overflow, Clip }
public enum FontStyle { Normal, Bold, Italic, BoldAndItalic }
public enum ImagePosition { ImageLeft, ImageAbove, ImageOnly, TextOnly }
public class Font : Object { }
public class RectOffset {
    public int left, right, top, bottom;
    public int horizontal => left + right;
    public int vertical => top + bottom;
    public RectOffset() { }
    public RectOffset(int left, int right, int top, int bottom) {
        this.left=left; this.right=right; this.top=top; this.bottom=bottom;
    }
    public Rect Add(Rect rect) => new(rect.x-left, rect.y-top, rect.width+horizontal, rect.height+vertical);
    public Rect Remove(Rect rect) => new(rect.x+left, rect.y+top, rect.width-horizontal, rect.height-vertical);
}
public class GUIStyleState {
    public Texture2D background;
    public Color textColor = new(0,0,0,1);
}
public class GUIStyle {
    public GUIStyleState normal=new(), hover=new(), active=new(), focused=new();
    public GUIStyleState onNormal=new(), onHover=new(), onActive=new(), onFocused=new();
    public RectOffset border=new(), margin=new(), padding=new(), overflow=new();
    public Font font;
    public string name;
    public int fontSize;
    public FontStyle fontStyle;
    public TextAnchor alignment;
    public TextClipping clipping;
    public ImagePosition imagePosition;
    public Vector2 contentOffset;
    public float fixedHeight, fixedWidth;
    public bool richText, stretchHeight, stretchWidth, wordWrap;
}
public struct GradientAlphaKey(float alpha, float time) { public float alpha=alpha, time=time; }
public struct GradientColorKey(Color color, float time) { public Color color=color; public float time=time; }
public sealed class Gradient {
    GradientColorKey[] colors = [new(new(1,1,1,1),0),new(new(1,1,1,1),1)];
    GradientAlphaKey[] alphas = [new(1,0),new(1,1)];
    public GradientColorKey[] colorKeys {
        get => (GradientColorKey[])colors.Clone();
        set { ArgumentNullException.ThrowIfNull(value); if(value.Length<2)throw new ArgumentException("At least two gradient color keys are required"); colors=value.OrderBy(k=>k.time).ToArray(); }
    }
    public GradientAlphaKey[] alphaKeys {
        get => (GradientAlphaKey[])alphas.Clone();
        set { ArgumentNullException.ThrowIfNull(value); if(value.Length<2)throw new ArgumentException("At least two gradient alpha keys are required"); alphas=value.OrderBy(k=>k.time).ToArray(); }
    }
    public void SetKeys(GradientColorKey[] colorKeys, GradientAlphaKey[] alphaKeys) { this.colorKeys=colorKeys; this.alphaKeys=alphaKeys; }
    public Color Evaluate(float time) {
        time=Math.Clamp(time,0,1);
        int c=0,a=0;
        while(c<colors.Length-2 && time>colors[c+1].time)c++;
        while(a<alphas.Length-2 && time>alphas[a+1].time)a++;
        float ct=Fraction(time,colors[c].time,colors[c+1].time),at=Fraction(time,alphas[a].time,alphas[a+1].time);
        var x=colors[c].color;var y=colors[c+1].color;
        return new(x.r+(y.r-x.r)*ct,x.g+(y.g-x.g)*ct,x.b+(y.b-x.b)*ct,alphas[a].alpha+(alphas[a+1].alpha-alphas[a].alpha)*at);
    }
    static float Fraction(float t,float start,float end)=>end<=start?(t>=end?1:0):Math.Clamp((t-start)/(end-start),0,1);
}

// Unity serializes persistent event listeners through its native JSON service.
// Hosts must bind that service before the original UnityEvent converter runs.
// Ordinary game records use the recovered FullSerializer implementation.
public interface IUnityJsonDriver {
    string ToJson(object value, bool prettyPrint);
    object FromJson(string json, Type type);
}
public static class JsonUtility {
    static readonly System.Runtime.CompilerServices.ConditionalWeakTable<RuntimeWorld,IUnityJsonDriver> drivers=new();
    public static void Bind(RuntimeWorld world,IUnityJsonDriver driver) {
        ArgumentNullException.ThrowIfNull(world);ArgumentNullException.ThrowIfNull(driver);
        drivers.Remove(world);drivers.Add(world,driver);
    }
    static IUnityJsonDriver Driver => drivers.TryGetValue(RuntimeWorld.Current,out var driver)?driver:
        throw new NotSupportedException("Unity persistent-listener JSON driver is not bound");
    public static string ToJson(object value,bool prettyPrint=false)=>Driver.ToJson(value,prettyPrint);
    public static object FromJson(string json,Type type)=>Driver.FromJson(json,type);
    public static T FromJson<T>(string json)=>(T)FromJson(json,typeof(T));
}

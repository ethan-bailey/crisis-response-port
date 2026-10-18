namespace UnityEngine.UI;

// Serialized uGUI component state. A bound presentation driver consumes the
// original scripts' text/color updates; absent a driver, this is CPU state only.
public interface IGraphicDriver {
    void SetColor(Graphic graphic,Color color);
    void SetText(Text text,string value);
}
public class Graphic:MonoBehaviour {
    [SerializeField] Color tint=Color.white;
    IGraphicDriver driver;
    public Color color {get=>tint;set{tint=value;driver?.SetColor(this,value);}}
    public IGraphicDriver Driver {get=>driver;set{driver=value;if(driver!=null)Present();}}
    protected IGraphicDriver RequiredDriver=>driver??throw new NotSupportedException("uGUI presentation driver is not bound");
    public virtual void Present()=>RequiredDriver.SetColor(this,color);
}
public sealed class Text:Graphic {
    [SerializeField] string content=string.Empty;
    public string text {get=>content;set{content=value??string.Empty;Driver?.SetText(this,content);}}
    public override void Present() {base.Present();RequiredDriver.SetText(this,text);}
}
public sealed class Image:Graphic { }
public sealed class Slider:MonoBehaviour {
    [SerializeField] float current;
    public float minValue=0,maxValue=1;
    public bool wholeNumbers;
    public readonly UnityEngine.Events.UnityEvent<float> onValueChanged=new();
    [NonSerialized] public Action<float> OnVisualValueChanged;
    public float value {
        get=>current;
        set {
            float next=Math.Clamp(value,minValue,maxValue);
            if(wholeNumbers)next=MathF.Round(next);
            if(current==next)return;
            current=next;OnVisualValueChanged?.Invoke(current);onValueChanged.Invoke(current);
        }
    }
    public float normalizedValue {get=>maxValue==minValue?0:(current-minValue)/(maxValue-minValue);set=>this.value=minValue+(maxValue-minValue)*Math.Clamp(value,0,1);}
}

namespace UnityEngine.UI;

// Serialized uGUI component state. A bound presentation driver consumes the
// original scripts' text/color updates; absent a driver, this is CPU state only.
public interface IGraphicDriver {
    void SetColor(Graphic graphic,Color color);
    void SetText(Text text,string value);
}
public struct ColorBlock {
    public Color normalColor,highlightedColor,pressedColor,selectedColor,disabledColor;
    public float colorMultiplier,fadeDuration;
}
// Unity 2017.2 uGUI component identities used by the recovered original prefabs.
// Layout/render execution stays presentation-driver owned; these components retain
// the authored hierarchy and serialized identity without inventing browser layout.
public sealed class CanvasScaler:MonoBehaviour { }
public sealed class GraphicRaycaster:MonoBehaviour { }
public sealed class LayoutElement:MonoBehaviour { }
public sealed class HorizontalLayoutGroup:MonoBehaviour { }
public sealed class VerticalLayoutGroup:MonoBehaviour { }
public sealed class ContentSizeFitter:MonoBehaviour { }
public sealed class Mask:MonoBehaviour { }
public class Graphic:MonoBehaviour {
    [SerializeField] Color tint=Color.white;
    public UnityEngine.Material material {get;set;}
    IGraphicDriver driver;
    public Color color {get=>tint;set{tint=value;driver?.SetColor(this,value);}}
    public IGraphicDriver Driver {get=>driver;set{driver=value;if(driver!=null)Present();}}
    protected IGraphicDriver RequiredDriver=>driver??throw new NotSupportedException("uGUI presentation driver is not bound");
    public virtual void Present()=>RequiredDriver.SetColor(this,color);
}
public class Selectable:MonoBehaviour {
    public bool interactable=true;
    public Graphic targetGraphic;
    public ColorBlock colors;
}
public sealed class Button:Selectable {
    public readonly UnityEngine.Events.UnityEvent onClick=new();
}
public sealed class Toggle:Selectable {
    bool selected;
    public int toggleTransition;
    public Graphic graphic;
    public UnityEngine.Events.UnityEvent<bool> onValueChanged=new();
    public bool isOn {
        get=>selected;
        set {if(selected==value)return;selected=value;onValueChanged.Invoke(selected);}
    }
}
public sealed class Dropdown:Selectable {
    public sealed class OptionData {
        public string text;
        public OptionData() { }
        public OptionData(string value) { text=value??string.Empty; }
    }
    readonly List<OptionData> availableOptions=new();
    int current;
    public List<OptionData> options=>availableOptions;
    public readonly UnityEngine.Events.UnityEvent<int> onValueChanged=new();
    public Text captionText;
    public int value {
        get=>current;
        set {var next=availableOptions.Count==0?Math.Max(0,value):Math.Clamp(value,0,availableOptions.Count-1);if(current==next)return;current=next;onValueChanged.Invoke(current);}
    }
    public void ClearOptions(){availableOptions.Clear();current=0;}
    public void AddOptions(List<OptionData> items){if(items!=null)availableOptions.AddRange(items);}
    public void AddOptions(List<string> items){if(items!=null)availableOptions.AddRange(items.Select(value=>new OptionData(value)));}
}
public sealed class Scrollbar:Selectable {
    float current;
    public float size=1;
    public UnityEngine.Events.UnityEvent<float> onValueChanged=new();
    public float value {
        get=>current;
        set {var next=Math.Clamp(value,0f,1f);if(current==next)return;current=next;onValueChanged.Invoke(current);}
    }
}
public sealed class InputField:Selectable {
    string content=string.Empty;
    public readonly UnityEngine.Events.UnityEvent<string> onValueChanged=new();
    public readonly UnityEngine.Events.UnityEvent<string> onEndEdit=new();
    public int characterLimit;
    public bool readOnly;
    public void Select() { }
    public void ActivateInputField() { }
    public string text {
        get=>content;
        set {var next=value??string.Empty;if(characterLimit>0&&next.Length>characterLimit)next=next[..characterLimit];if(content==next)return;content=next;onValueChanged.Invoke(content);}
    }
}
public sealed class ScrollRect:MonoBehaviour {
    public RectTransform content;
    float vertical=1f;
    public float verticalNormalizedPosition {
        get=>vertical;
        set {if(!float.IsFinite(value))throw new ArgumentOutOfRangeException(nameof(value));vertical=Math.Clamp(value,0f,1f);}
    }
}
public sealed class RawImage:Graphic {
    public UnityEngine.Texture texture;
    public UnityEngine.Rect uvRect=new(0,0,1,1);
}
public sealed class Text:Graphic {
    [SerializeField] string content=string.Empty;
    public string text {get=>content;set{content=value??string.Empty;Driver?.SetText(this,content);}}
    public override void Present() {base.Present();RequiredDriver.SetText(this,text);}
}
public sealed class Image:Graphic { public Sprite sprite {get;set;} }
public sealed class Slider:Selectable {
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

namespace UnityEngine;

public sealed class Sprite:Object {
    public Texture texture {get;set;}
    public Rect rect {get;set;}
    public Vector2 pivot {get;set;}
}

public sealed class SpriteRenderer:Renderer {
    public Sprite sprite {get;set;}
    public Color color {get;set;}=Color.white;
    public bool flipX {get;set;}
    public bool flipY {get;set;}
    public new int sortingOrder {get;set;}
    public int drawMode {get;set;}
    public Vector2 size {get;set;}
    public float adaptiveModeThreshold {get;set;}
    public int spriteTileMode {get;set;}
    public int spriteSortPoint {get;set;}
    public int maskInteraction {get;set;}
}

public sealed class LineRenderer:Renderer {
    readonly List<Vector3> points=new();
    public bool useWorldSpace {get;set;}=true;
    public float startWidth {get;set;}=1f;
    public float endWidth {get;set;}=1f;
    public float widthMultiplier {get;set;}=1f;
    public Color startColor {get;set;}=Color.white;
    public Color endColor {get;set;}=Color.white;
    public int positionCount {
        get=>points.Count;
        set {if(value<0)throw new ArgumentOutOfRangeException(nameof(value));if(value<points.Count)points.RemoveRange(value,points.Count-value);else while(points.Count<value)points.Add(Vector3.zero);}
    }
    public void SetPosition(int index,Vector3 position)=>points[index]=position;
    public Vector3 GetPosition(int index)=>points[index];
    public void SetPositions(Vector3[] positions) {ArgumentNullException.ThrowIfNull(positions);points.Clear();points.AddRange(positions);}
    public int GetPositions(Vector3[] positions) {ArgumentNullException.ThrowIfNull(positions);int count=Math.Min(positions.Length,points.Count);for(int i=0;i<count;i++)positions[i]=points[i];return count;}
}

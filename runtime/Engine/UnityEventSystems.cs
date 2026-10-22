namespace UnityEngine.EventSystems;

public interface IEventSystemHandler { }
public interface IPointerClickHandler:IEventSystemHandler {void OnPointerClick(PointerEventData eventData);}
public interface IPointerDownHandler:IEventSystemHandler {void OnPointerDown(PointerEventData eventData);}
public interface IDragHandler:IEventSystemHandler {void OnDrag(PointerEventData eventData);}
public interface IPointerEnterHandler:IEventSystemHandler {void OnPointerEnter(PointerEventData eventData);}
public interface IPointerExitHandler:IEventSystemHandler {void OnPointerExit(PointerEventData eventData);}

public sealed class PointerEventData {
    public Vector2 position {get;set;}
    public Vector2 pressPosition {get;set;}
    public Vector2 delta {get;set;}
    public int pointerId {get;set;}
    public Camera pressEventCamera {get;set;}
}

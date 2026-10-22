namespace UnityEngine;

public static class RectTransformUtility {
    public static bool ScreenPointToWorldPointInRectangle(RectTransform rect,Vector2 screenPoint,Camera camera,out Vector3 worldPoint) {
        ArgumentNullException.ThrowIfNull(rect);
        if(camera==null) {
            worldPoint=rect.TransformPoint(new Vector3(screenPoint.x,screenPoint.y,0));
            return true;
        }
        var ray=camera.ScreenPointToRay(screenPoint);
        var normal=rect.forward;
        float denominator=Vector3.Dot(ray.direction,normal);
        if(MathF.Abs(denominator)<1e-7f){worldPoint=Vector3.zero;return false;}
        float distance=Vector3.Dot(rect.position-ray.origin,normal)/denominator;
        if(distance<0){worldPoint=Vector3.zero;return false;}
        worldPoint=ray.GetPoint(distance);
        return true;
    }
}

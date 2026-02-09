using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Unity.UI.Extensions
{
public static class CanvasExtensions
{
  private static Dictionary<Canvas, Vector3> CanvasScaleChached = new Dictionary<Canvas, Vector3>();

  public static Vector3 WorldToCanvasPosition(this Canvas canvas, Vector3 worldPosition, Camera camera)
  {
    if (camera == null)
      camera = Camera.main;

    var viewportPosition = camera.WorldToViewportPoint(worldPosition);
    return canvas.ViewportToCanvasPosition(viewportPosition);
  }

  public static Vector3 ScreenToCanvasPosition(this Canvas canvas, Vector3 screenPosition)
  {
    var viewportPosition = new Vector3(screenPosition.x / Screen.width,
    screenPosition.y / Screen.height,
    0);

    return canvas.ViewportToCanvasPosition(viewportPosition);
  }

  public static Vector3 ViewportToCanvasPosition(this Canvas canvas, Vector3 viewportPosition)
  {
    var centerBasedViewPortPosition = viewportPosition - new Vector3(0.5f, 0.5f, 0);

    if (!CanvasScaleChached.TryGetValue(canvas, out Vector3 scale))
    {
      var canvasRect = canvas.GetComponent<RectTransform>();
      scale = new Vector3(canvasRect.rect.width, canvasRect.rect.height);
      CanvasScaleChached[canvas] = scale;
    }

    return Vector3.Scale(centerBasedViewPortPosition, scale);
  }

  public static Vector3 CanvasToViewportPosition(this Canvas canvas, Vector3 canvasPosition)
  {
      var canvasRect = canvas.GetComponent<RectTransform>();
      var scale = canvasRect.sizeDelta;
      return Vector3.Scale(canvasPosition, new Vector3(1 / scale.x, 1/ scale.y)) + new Vector3(0.5f, 0.5f, 0);
  }

  public static Vector3 CanvasToWorldPosition(this Canvas canvas, Vector3 canvasPosition, Camera camera = null)
  {
    if (camera == null)
      camera = Camera.main;

    var viewportPos = canvas.CanvasToViewportPosition(canvasPosition);
    var ray = camera.ViewportPointToRay(viewportPos);
    var plane = new Plane(Vector3.up, Vector3.zero);
    if(!plane.Raycast(ray, out float hitDist))
      return Vector3.zero;

    return ray.GetPoint(hitDist);
  }
  
  public static Vector3 CanvasToWorldPosition(this Canvas canvas, Vector3 canvasPosition, float dist, Camera camera = null)
  {
      if (camera == null)
          camera = Camera.main;

      var viewportPos = canvas.CanvasToViewportPosition(canvasPosition);
      var ray = camera.ViewportPointToRay(viewportPos);
      var plane = new Plane(camera.transform.forward, camera.transform.position + camera.transform.forward * dist);
      if(!plane.Raycast(ray, out float hitDist))
          return Vector3.zero;

      return ray.GetPoint(hitDist);
  }
  
  public static Vector2 Clamp(this Rect rect, Vector2 origin, Vector3 size = default)
  {
    return new Vector2(
      Mathf.Clamp(origin.x, rect.xMin + size.x * 0.5f, rect.xMax - size.x * 0.5f),
      Mathf.Clamp(origin.y, rect.yMin + size.y * 0.5f, rect.yMax - size.y * 0.5f)
    );
  }
}
}

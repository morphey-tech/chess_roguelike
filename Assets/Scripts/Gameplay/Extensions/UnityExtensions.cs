#if !NO_UNITY
using UnityEngine;

namespace Project.Core
{
  public static class UnityExtensions
  {
    public static bool IsEmpty(this Vector3 self)
    {
      return float.IsNegativeInfinity(self.x) &&
             float.IsNegativeInfinity(self.y) &&
             float.IsNegativeInfinity(self.z);
    }

    public static Vector2 FromXZ(this Vector3 self)
    {
      return new Vector2(self.x, self.z);
    }

    public static Vector3 FromXY(this Vector2 self)
    {
      return new Vector3(self.x, 0, self.y);
    }

    public static Vector3 ToXZ(this Vector3 self)
    {
      return new Vector3(self.x, 0, self.z);
    }

    public static bool IsValid(this Vector3 v)
    {
      return !float.IsNaN(v.x) && !float.IsNaN(v.y) && !float.IsNaN(v.z);
    }

    public static bool IsEmpty(this Quaternion self)
    {
      return float.IsNegativeInfinity(self.x) &&
             float.IsNegativeInfinity(self.y) &&
             float.IsNegativeInfinity(self.z) &&
             float.IsNegativeInfinity(self.w);
    }
    public static GameObject CloneTpl(this GameObject tpl)
    {
      GameObject o = Object.Instantiate(tpl, tpl.transform.parent, true);
      o.transform.localPosition = tpl.transform.localPosition;
      o.transform.localRotation = tpl.transform.localRotation;
      o.transform.localScale = tpl.transform.localScale;
      return o;
    }

    public static T AddComponentOnce<T>(this Component self) where T : Component => self.gameObject.AddComponentOnce<T>();

    public static T AddComponentOnce<T>(this GameObject self) where T : Component
    {
      T c = self.GetComponent<T>();
      if(c == null)
        c = self.AddComponent<T>();
      return c;
    }

    public static T AsComponent<T>(this GameObject o) where T : Component
    {
      var res = o.GetComponent<T>();
      //VerifyComponent(o, res);
      return res;
    }

    
    public static T CastComponent<T>(this Component rawValue) where T : class
    {
      if(rawValue == null)
        return null;

      var result = rawValue as T;
      if(result != null)
        return result;

      result = rawValue.GetComponent<T>();
      if(result != null)
        return result;

      return null;
    }
    
    public static string GetFullPath(this GameObject o)
    {
      return o.transform.GetFullPath();
    }

    public static void SetLayerRecursive(this GameObject obj, int layer, int exceptLayer = -1)
    {
      if(obj.layer == exceptLayer)
        return;

      obj.layer = layer;

      for(int i = 0; i < obj.transform.childCount; ++i)
        SetLayerRecursive(obj.transform.GetChild(i).gameObject, layer, exceptLayer);
    }

    //NOTE: using Unity's built-in non-allocating Find
    public static Transform FindRecursive(this Transform current, string name, bool onlyActive = false)
    {
      bool activeCheck = !onlyActive || current.gameObject.activeInHierarchy;

      if(activeCheck && current.parent)
      {
        if(current.parent.Find(name) == current)
          return current;
      }
      //NOTE: switching to mem-allocating version only if there's no parent
      else if(activeCheck && current.name == name)
        return current;

      for(int i = 0; i < current.childCount; ++i)
      {
        var chld = current.GetChild(i);
        var tmp = chld.FindRecursive(name, onlyActive);
        if(tmp != null)
          return tmp;
      }
      return null;
    }

    public static GameObject GetParent(this GameObject o)
    {
      return o.transform.parent.gameObject;
    }

    public static string GetFullPath(this Transform t)
    {
      return t.GetPathWhile(current => current != null);
    }

    public static string GetPathUntilClone(this Transform t)
    {
      return t.GetPathWhile(current => !current.name.EndsWith("(Clone)"));
    }

    public delegate bool TransformPathBuilderCondition(Transform current);

    public static string GetPathWhile(this Transform t, TransformPathBuilderCondition condition)
    {
      string path = "";
      Transform tmp = t;
      while(condition(tmp))
      {
        path = tmp.gameObject.name + (path.Length > 0 ? ("/" + path) : "");
        tmp = tmp.parent;
      }
      return path;
    }

    public static bool BelongsTo(this GameObject o, GameObject other)
    {
      Transform current = o.transform;
      while(current != null)
      {
        if(current.gameObject == other)
          return true;
        current = current.parent;
      }
      return false;
    }

    public static float GetX(this GameObject go)
    {
      return go.transform.localPosition.x;
    }

    public static float GetY(this GameObject go)
    {
      return go.transform.localPosition.y;
    }

    public static void SetX(this GameObject go, float x)
    {
      Vector3 lp = go.transform.localPosition;
      lp.x = x;
      go.transform.localPosition = lp;
    }

    public static void SetY(this GameObject go, float y)
    {
      Vector3 lp = go.transform.localPosition;
      lp.y = y;
      go.transform.localPosition = lp;
    }

    public static void RestoreLocalSettingsToDefault(this Transform transform)
    {
      transform.localPosition = Vector3.zero;
      transform.localRotation = Quaternion.identity;
      transform.localScale = Vector3.one;
    }

    public static void InvertVectors(ref Vector3 vectorA, ref Vector3 vectorB)
    {
      var tempStartPos = vectorA;
      vectorA = vectorB;
      vectorB = tempStartPos;
    }

    public static bool HasParameters(this Animator anim, string name)
    {
      foreach(var p in anim.parameters)
        if(p.name == name)
          return true;
      return false;
    }

    public static Color AsColor(this int n)
    {
      uint val = (uint)n;

      // scramble the bits up using Robert Jenkins' 32 bit integer hash function
      val = (val+0x7ed55d16) + (val<<12);
      val = (val^0xc761c23c) ^ (val>>19);
      val = (val+0x165667b1) + (val<<5);
      val = (val+0xd3a2646c) ^ (val<<9);
      val = (val+0xfd7046c5) + (val<<3);
      val = (val^0xb55a4f09) ^ (val>>16);

      float r = (float)((val>>0) & 0xFF);
      float g = (float)((val>>8) & 0xFF);
      float b = (float)((val>>16) & 0xFF);

      float max = (float)Mathf.Max(Mathf.Max(r, g), b);
      float min = (float)Mathf.Min(Mathf.Min(r, g), b);
      float intensity = 0.75f;

      // Saturate and scale the color
      if(min == max)
      {
        return new Color(intensity, 0.0f, 0.0f, 1.0f);
      }
      else
      {
        float coef = (float)intensity/(max - min);
        return new Color(
          (r - min)*coef,
          (g - min)*coef,
          (b - min)*coef,
          1.0f
        );
      }
    }


  }

  public static class RectExtensions
  {
    public static Vector2 TopLeft(this Rect rect) { return new Vector2(rect.xMin, rect.yMin); }
    public static Vector2 TopRight(this Rect rect) { return new Vector2(rect.xMin + rect.width, rect.yMin); }
    public static Vector2 TopCenter(this Rect rect) { return new Vector2(rect.xMin + rect.width / 2f, rect.yMin); }
    public static Vector2 BottomCenter(this Rect rect) { return new Vector2(rect.xMin + rect.width / 2f, rect.yMin + rect.height); }
    public static Vector2 CenterLeft(this Rect rect) { return new Vector2(rect.xMin, rect.yMin + rect.height / 2f); }
    public static Vector2 CenterRight(this Rect rect) { return new Vector2(rect.xMin + rect.width, rect.yMin + rect.height / 2f); }

    public static Rect ScaleSizeBy(this Rect rect, float scale)
    {
      return rect.ScaleSizeBy(scale, rect.center);
    }

    public static Rect ScaleSizeBy(this Rect rect, float scale, Vector2 pivotPoint)
    {
      Rect result = rect;
      result.x -= pivotPoint.x;
      result.y -= pivotPoint.y;
      result.xMin *= scale;
      result.xMax *= scale;
      result.yMin *= scale;
      result.yMax *= scale;
      result.x += pivotPoint.x;
      result.y += pivotPoint.y;
      return result;
    }
    public static Rect ScaleSizeBy(this Rect rect, Vector2 scale)
    {
      return rect.ScaleSizeBy(scale, rect.center);
    }
    public static Rect ScaleSizeBy(this Rect rect, Vector2 scale, Vector2 pivotPoint)
    {
      Rect result = rect;
      result.x -= pivotPoint.x;
      result.y -= pivotPoint.y;
      result.xMin *= scale.x;
      result.xMax *= scale.x;
      result.yMin *= scale.y;
      result.yMax *= scale.y;
      result.x += pivotPoint.x;
      result.y += pivotPoint.y;
      return result;
    }
  }
}
#endif

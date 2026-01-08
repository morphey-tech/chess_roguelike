using UnityEngine;

namespace LiteUI.Common.Extensions
{
    public static class RectTransformExtension
    {
        public static bool IsPointInside(this RectTransform rectTransform, Vector2 screenPosition)
        {
            Vector2 localPosition = rectTransform.InverseTransformPoint(screenPosition);
            return rectTransform.rect.Contains(localPosition);
        }
    }
}

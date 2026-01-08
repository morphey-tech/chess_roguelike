using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace LiteUI.Common.Extensions
{
    [PublicAPI]
    public static class RawImageExtension
    {
        public static void ReleaseRenderTexture(this RawImage rawImage)
        {
            RenderTexture? targetTexture = rawImage.texture as RenderTexture;
            if (targetTexture == null) {
                return;
            }
            targetTexture.Release();
            rawImage.texture = null!;
        }
    }
}

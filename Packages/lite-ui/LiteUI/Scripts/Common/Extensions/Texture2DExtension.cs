using JetBrains.Annotations;
using UnityEngine;

namespace LiteUI.Common.Extensions
{
    [PublicAPI]
    public static class Texture2DExtension
    {
        public static Sprite CreateSprite(this Texture2D texture)
        {
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }
    }
}

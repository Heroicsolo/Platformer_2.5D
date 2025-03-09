using UnityEngine;

namespace HeroicEngine.Utils
{
    public static class SpriteUtils
    {
        /// <summary>
        /// This extension method creates Sprite from given texture.
        /// </summary>
        /// <param name="texture">Given texture</param>
        /// <returns>Sprite created from texture</returns>
        public static Sprite SpriteFromTexture (this Texture2D texture)
        {
            if (texture == null) return null;

            Rect rect = new(0, 0, texture.width, texture.height);
            Vector2 pivot = new(.5f, .5f);
            return Sprite.Create(texture, rect, pivot);
        }

        /// <summary>
        /// This extension method creates texture from given sprite.
        /// </summary>
        /// <param name="sprite">Given sprite</param>
        /// <returns>Texture created from sprite</returns>
        public static Texture2D TextureFromSprite(this Sprite sprite)
        {
            if (sprite.rect.width != sprite.texture.width)
            {
                Texture2D newText = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
                Color[] newColors = sprite.texture.GetPixels((int)sprite.textureRect.x,
                                                             (int)sprite.textureRect.y,
                                                             (int)sprite.textureRect.width,
                                                             (int)sprite.textureRect.height);
                newText.SetPixels(newColors);
                newText.Apply();
                return newText;
            }
            else
                return sprite.texture;
        }
    }
}

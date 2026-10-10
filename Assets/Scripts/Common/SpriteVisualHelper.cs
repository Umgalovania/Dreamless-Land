using UnityEngine;

namespace DreamlessLand.Common
{
    /// <summary>
    /// 用内置白纹理生成约 1×1 世界单位的占位精灵（4×4 纹理 / PPU 4），避免默认 PPU=100 导致几乎看不见。
    /// </summary>
    public static class SpriteVisualHelper
    {
        /// <summary>4×4 白图 + PPU=4 → 精灵约 1×1 世界单位。</summary>
        public static Sprite CreateUnitSquareSprite()
        {
            var t = Texture2D.whiteTexture;
            return Sprite.Create(
                t,
                new Rect(0f, 0f, t.width, t.height),
                new Vector2(0.5f, 0.5f),
                4f);
        }

        public static void EnsureOneUnitSquare(SpriteRenderer sr)
        {
            if (sr == null) return;
            var c = sr.color;
            sr.sprite = CreateUnitSquareSprite();
            sr.color = c;
        }
    }
}

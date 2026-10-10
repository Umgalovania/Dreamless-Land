using UnityEngine;

namespace DreamlessLand.Common
{
    /// <summary>
    /// 场景加载后，将「4×4 白纹理 + 极小世界尺寸」的占位精灵替换为 1×1 单位，避免 Game 视图几乎全空。
    /// </summary>
    public static class SpriteRuntimeFix
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AfterSceneLoad()
        {
            foreach (var sr in Object.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            {
                var s = sr.sprite;
                if (s == null) continue;

                var tex = s.texture;
                if (tex == null) continue;

                var e = s.bounds.extents;
                if (e.x >= 0.12f || e.y >= 0.12f) continue;

                if (tex.width <= 4 && tex.height <= 4)
                    SpriteVisualHelper.EnsureOneUnitSquare(sr);
            }
        }
    }
}

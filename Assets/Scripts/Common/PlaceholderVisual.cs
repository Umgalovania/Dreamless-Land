using UnityEngine;

namespace DreamlessLand.Common
{
    /// <summary>
    /// 占位视觉组件：在 Awake 时自动生成 1×1 单位方块精灵并着色。
    /// 解决编辑器中通过 Sprite.Create 创建的临时精灵在场景保存/加载后丢失的问题。
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlaceholderVisual : MonoBehaviour
    {
        [SerializeField] Color spriteColor = Color.white;

        void Awake()
        {
            var sr = GetComponent<SpriteRenderer>();
            sr.sprite = SpriteVisualHelper.CreateUnitSquareSprite();
            sr.color = spriteColor;
        }
    }
}

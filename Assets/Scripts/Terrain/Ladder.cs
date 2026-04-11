using UnityEngine;

namespace DreamlessLand.Terrain
{
    /// <summary>
    /// 梯子标记组件：依附于实体墙壁，玩家进入触发区域后可使用W/S攀爬。
    /// 实际攀爬逻辑由 PlayerController2D 处理。
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class Ladder : MonoBehaviour
    {
        void Awake()
        {
            var box = GetComponent<BoxCollider2D>();
            box.isTrigger = true;
        }
    }
}

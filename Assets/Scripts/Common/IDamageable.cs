using UnityEngine;

namespace DreamlessLand.Common
{
    /// <summary>
    /// 可被伤害的物体接口（玩家、敌人等）。
    /// </summary>
    public interface IDamageable
    {
        void TakeDamage(int amount, GameObject attacker);
    }
}

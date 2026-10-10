using UnityEngine;
using DreamlessLand.Common;

namespace DreamlessLand.Enemy
{
    /// <summary>
    /// 敌人接触伤害：与玩家碰撞时每隔一定时间造成1点伤害。
    /// 使用 OnCollisionStay2D + 冷却计时器，避免每帧扣血。
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class EnemyContactDamage : MonoBehaviour
    {
        [SerializeField] int contactDamage = 1;
        [SerializeField] float damageCooldown = 0.5f;

        float _nextDamageTime;

        void OnCollisionEnter2D(Collision2D collision)
        {
            TryDamagePlayer(collision.collider);
        }

        void OnCollisionStay2D(Collision2D collision)
        {
            if (Time.time >= _nextDamageTime)
                TryDamagePlayer(collision.collider);
        }

        void TryDamagePlayer(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            if (Time.time < _nextDamageTime) return;

            var unit = other.GetComponentInParent<Unit>();
            if (unit != null)
            {
                unit.TakeDamage(contactDamage, gameObject);
                _nextDamageTime = Time.time + damageCooldown;
                Debug.Log($"[Enemy] {gameObject.name} 对玩家造成 {contactDamage} 点接触伤害");
            }
        }
    }
}

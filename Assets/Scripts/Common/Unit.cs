using System;
using UnityEngine;

namespace DreamlessLand.Common
{
    /// <summary>
    /// 单位基类：管理血量与阵营，提供受伤与死亡的虚方法。
    /// 所有可受伤的游戏实体（玩家、敌人等）均应继承此类。
    /// </summary>
    public abstract class Unit : MonoBehaviour, IDamageable
    {
        [Header("单位属性")]
        [SerializeField] protected int maxHealth = 5;
        [SerializeField] protected Faction faction = Faction.Player;

        protected int currentHealth;

        public int MaxHealth => maxHealth;
        public int CurrentHealth => currentHealth;
        public Faction UnitFaction => faction;

        /// <summary>血量变化事件 (当前血量, 最大血量)</summary>
        public event Action<int, int> OnHealthChanged;
        /// <summary>死亡事件</summary>
        public event Action OnDied;

        protected virtual void Awake()
        {
            currentHealth = maxHealth;
        }

        public virtual void TakeDamage(int damage, GameObject attacker)
        {
            TakeDamage(damage);
        }

        public virtual void TakeDamage(int damage)
        {
            if (damage <= 0 || currentHealth <= 0) return;

            currentHealth -= damage;
            if (currentHealth < 0) currentHealth = 0;

            Debug.Log($"[Unit] {gameObject.name} 受到 {damage} 点伤害，剩余血量: {currentHealth}/{maxHealth}");
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            if (currentHealth <= 0)
                Die();
        }

        public virtual void Die()
        {
            Debug.Log($"[Unit] {gameObject.name} 死亡！");
            OnDied?.Invoke();
        }

        public void ResetHealth()
        {
            currentHealth = maxHealth;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }
    }
}

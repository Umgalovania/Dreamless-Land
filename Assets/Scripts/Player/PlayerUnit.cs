using System.Collections;
using UnityEngine;
using DreamlessLand.Common;

namespace DreamlessLand.Player
{
    /// <summary>
    /// 玩家单位：继承Unit，初始血量5。
    /// 受伤时闪烁反馈+击退，死亡时触发游戏结束。
    /// </summary>
    public class PlayerUnit : Unit
    {
        [Header("受伤反馈")]
        [SerializeField] float invincibleDuration = 1f;
        [SerializeField] float flashInterval = 0.1f;
        [SerializeField] float knockbackForce = 5f;

        SpriteRenderer _spriteRenderer;
        Rigidbody2D _rb;
        bool _isInvincible;
        Color _originalColor;

        protected override void Awake()
        {
            faction = Faction.Player;
            base.Awake();

            _spriteRenderer = GetComponent<SpriteRenderer>();
            _rb = GetComponent<Rigidbody2D>();
            if (_spriteRenderer != null)
                _originalColor = _spriteRenderer.color;
        }

        public override void TakeDamage(int damage)
        {
            if (_isInvincible || currentHealth <= 0) return;
            base.TakeDamage(damage);

            if (currentHealth > 0)
                StartCoroutine(InvincibilityFlash());
        }

        public override void TakeDamage(int damage, GameObject attacker)
        {
            if (_isInvincible || currentHealth <= 0) return;

            // 击退效果
            if (_rb != null && attacker != null)
            {
                Vector2 dir = ((Vector2)transform.position - (Vector2)attacker.transform.position).normalized;
                dir.y = 0.5f;
                _rb.linearVelocity = dir.normalized * knockbackForce;
            }

            TakeDamage(damage);
        }

        public override void Die()
        {
            base.Die();

            // 禁用玩家控制
            var controller = GetComponent<PlayerController2D>();
            if (controller != null) controller.enabled = false;
            var combat = GetComponent<PlayerCombat>();
            if (combat != null) combat.enabled = false;

            // 通知 GameManager
            if (GameManager.Instance != null)
                GameManager.Instance.TriggerGameOver();
        }

        IEnumerator InvincibilityFlash()
        {
            _isInvincible = true;
            float timer = 0f;
            bool visible = true;

            while (timer < invincibleDuration)
            {
                visible = !visible;
                if (_spriteRenderer != null)
                {
                    Color c = _originalColor;
                    c.a = visible ? 1f : 0.3f;
                    _spriteRenderer.color = c;
                }

                yield return new WaitForSeconds(flashInterval);
                timer += flashInterval;
            }

            if (_spriteRenderer != null)
                _spriteRenderer.color = _originalColor;

            _isInvincible = false;
        }

        public bool IsInvincible => _isInvincible;
    }
}

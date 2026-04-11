using System.Collections;
using UnityEngine;
using DreamlessLand.Common;

namespace DreamlessLand.Player
{
    /// <summary>
    /// 玩家近战攻击：J键攻击，前方扇形范围检测敌人。
    /// 攻击时显示半透明白色色块特效。
    /// </summary>
    public class PlayerCombat : MonoBehaviour
    {
        [Header("攻击参数")]
        [SerializeField] int attackDamage = 4;
        [SerializeField] float attackRange = 2f;
        [SerializeField] float attackAngle = 60f;
        [SerializeField] float attackCooldown = 0.4f;
        [SerializeField] LayerMask enemyLayers;

        PlayerController2D _controller;
        float _nextAttackTime;

        public int AttackDamage => attackDamage;

        void Awake()
        {
            _controller = GetComponent<PlayerController2D>();
        }

        void Update()
        {
            if (GameManager.Instance != null &&
                GameManager.Instance.CurrentState != GameState.Playing)
                return;

            if (Input.GetKeyDown(KeyCode.J) && Time.time >= _nextAttackTime)
            {
                PerformAttack();
                _nextAttackTime = Time.time + attackCooldown;
            }
        }

        void PerformAttack()
        {
            float face = _controller != null ? _controller.FacingSign : 1f;
            Vector2 origin = transform.position;
            Vector2 forward = new Vector2(face, 0f);

            Debug.Log("[Player] 攻击！");
            StartCoroutine(ShowAttackEffect(face));

            var hits = Physics2D.OverlapCircleAll(origin, attackRange, enemyLayers);
            foreach (var hit in hits)
            {
                if (hit == null || hit.transform == transform) continue;

                Vector2 toTarget = ((Vector2)hit.transform.position - origin).normalized;
                float angle = Vector2.Angle(forward, toTarget);
                if (angle > attackAngle * 0.5f) continue;

                var unit = hit.GetComponentInParent<Unit>();
                if (unit != null)
                {
                    unit.TakeDamage(attackDamage, gameObject);
                    Debug.Log($"[Player] 击中 {hit.gameObject.name}，造成 {attackDamage} 点伤害");
                }
            }
        }

        IEnumerator ShowAttackEffect(float face)
        {
            var go = new GameObject("AttackEffect");
            go.transform.position = transform.position + new Vector3(face * 1.0f, 0f, 0f);
            go.transform.localScale = new Vector3(2f, 2f, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteVisualHelper.CreateUnitSquareSprite();
            sr.color = new Color(1f, 1f, 1f, 0.4f);
            sr.sortingOrder = 5;

            float elapsed = 0f;
            const float duration = 0.12f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(0.4f, 0f, elapsed / duration);
                sr.color = new Color(1f, 1f, 1f, alpha);
                yield return null;
            }

            Destroy(go);
        }

        void OnDrawGizmosSelected()
        {
            float face = 1f;
            if (_controller != null) face = _controller.FacingSign;

            Vector2 origin = transform.position;
            Gizmos.color = new Color(1f, 0.3f, 0.2f, 0.6f);
            Gizmos.DrawWireSphere(origin, attackRange);

            float halfAngle = attackAngle * 0.5f * Mathf.Deg2Rad;
            float baseAngle = face > 0 ? 0f : Mathf.PI;
            Vector3 lineA = new Vector3(Mathf.Cos(baseAngle + halfAngle), Mathf.Sin(baseAngle + halfAngle), 0f) * attackRange;
            Vector3 lineB = new Vector3(Mathf.Cos(baseAngle - halfAngle), Mathf.Sin(baseAngle - halfAngle), 0f) * attackRange;
            Gizmos.DrawLine(origin, (Vector2)origin + (Vector2)lineA);
            Gizmos.DrawLine(origin, (Vector2)origin + (Vector2)lineB);
        }
    }
}

using System.Collections;
using UnityEngine;
using DreamlessLand.Common;

namespace DreamlessLand.Enemy
{
    /// <summary>
    /// 巡逻小兵AI：在出生点附近巡逻，发现玩家后追击并在近距离攻击。
    /// 检测逻辑：面向玩家且 10 单位内无地形阻挡，或玩家距离 3 单位以内直接感知。
    /// 需搭配 EnemyUnit 管理血量。
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemySoldier : MonoBehaviour
    {
        [Header("移动")]
        [SerializeField] float moveSpeed = 2.5f;
        [SerializeField] float patrolHalfWidth = 10f;

        [Header("检测")]
        [SerializeField] float detectionRange = 10f;
        [SerializeField] float hearingRange = 3f;
        [SerializeField] LayerMask terrainLayers;

        [Header("攻击")]
        [SerializeField] float attackRange = 1.5f;
        [SerializeField] int attackDamage = 1;
        [SerializeField] float attackWindup = 0.5f;
        [SerializeField] float attackRecovery = 1f;

        enum State { Patrol, Chase, AttackWindup, AttackRecovery }

        Rigidbody2D _rb;
        State _state = State.Patrol;
        float _direction = 1f;
        float _startX;
        Transform _player;
        float _stateTimer;
        float _chaseMemoryTimer;
        const float ChaseMemoryDuration = 2f;

        void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.freezeRotation = true;
        }

        void OnEnable()
        {
            _state = State.Patrol;
            _direction = 1f;
            _stateTimer = 0f;
            _chaseMemoryTimer = 0f;
            _startX = transform.position.x;
        }

        void Start()
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                _player = playerObj.transform;

            if (terrainLayers == 0)
            {
                int t = LayerMask.NameToLayer("Terrain");
                if (t >= 0) terrainLayers |= 1 << t;
            }
        }

        void FixedUpdate()
        {
            if (_player == null)
            {
                var go = GameObject.FindGameObjectWithTag("Player");
                if (go != null) _player = go.transform;
                return;
            }

            switch (_state)
            {
                case State.Patrol: UpdatePatrol(); break;
                case State.Chase: UpdateChase(); break;
                case State.AttackWindup: UpdateWindup(); break;
                case State.AttackRecovery: UpdateRecovery(); break;
            }
            UpdateFacing();
        }

        void UpdatePatrol()
        {
            float left = _startX - patrolHalfWidth;
            float right = _startX + patrolHalfWidth;

            _rb.linearVelocity = new Vector2(_direction * moveSpeed, _rb.linearVelocity.y);

            // 统一处理转向，每帧仅翻转一次
            bool shouldFlip = false;

            if (transform.position.x >= right && _direction > 0)
                shouldFlip = true;
            else if (transform.position.x <= left && _direction < 0)
                shouldFlip = true;

            if (!shouldFlip)
            {
                Vector2 edgeOrigin = (Vector2)transform.position + new Vector2(_direction * 0.5f, -0.2f);
                RaycastHit2D edgeHit = Physics2D.Raycast(edgeOrigin, Vector2.down, 2f, terrainLayers);
                if (edgeHit.collider == null)
                    shouldFlip = true;
            }

            if (!shouldFlip)
            {
                RaycastHit2D wallHit = Physics2D.Raycast(
                    transform.position, new Vector2(_direction, 0f), 0.6f, terrainLayers);
                if (wallHit.collider != null)
                    shouldFlip = true;
            }

            if (shouldFlip)
                _direction = -_direction;

            if (DetectPlayer())
            {
                _state = State.Chase;
                _chaseMemoryTimer = ChaseMemoryDuration;
                Debug.Log("[EnemySoldier] 发现玩家！开始追击");
            }
        }

        void UpdateChase()
        {
            if (_player == null || !_player.gameObject.activeInHierarchy)
            {
                _state = State.Patrol;
                return;
            }

            float hDist = Mathf.Abs(_player.position.x - transform.position.x);

            if (hDist <= attackRange)
            {
                _state = State.AttackWindup;
                _stateTimer = attackWindup;
                _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
                return;
            }

            float dir = Mathf.Sign(_player.position.x - transform.position.x);
            _rb.linearVelocity = new Vector2(dir * moveSpeed * 1.2f, _rb.linearVelocity.y);
            _direction = dir;

            if (DetectPlayer())
            {
                _chaseMemoryTimer = ChaseMemoryDuration;
            }
            else
            {
                _chaseMemoryTimer -= Time.fixedDeltaTime;
                if (_chaseMemoryTimer <= 0f)
                    _state = State.Patrol;
            }
        }

        void UpdateWindup()
        {
            _stateTimer -= Time.fixedDeltaTime;
            _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);

            if (_stateTimer <= 0f)
            {
                PerformAttack();
                _state = State.AttackRecovery;
                _stateTimer = attackRecovery;
            }
        }

        void UpdateRecovery()
        {
            _stateTimer -= Time.fixedDeltaTime;
            _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);

            if (_stateTimer <= 0f)
            {
                if (DetectPlayer())
                {
                    _state = State.Chase;
                    _chaseMemoryTimer = ChaseMemoryDuration;
                }
                else
                    _state = State.Patrol;
            }
        }

        void PerformAttack()
        {
            if (_player == null) return;

            StartCoroutine(ShowAttackEffect());

            float dist = Vector2.Distance(transform.position, _player.position);
            if (dist <= attackRange + 0.5f)
            {
                var unit = _player.GetComponent<Unit>();
                if (unit != null)
                    unit.TakeDamage(attackDamage, gameObject);
                Debug.Log("[EnemySoldier] 攻击命中！");
            }
        }

        /// <summary>
        /// 检测玩家：近距离（hearingRange）无视朝向和遮挡；
        /// 远距离需面向玩家且无地形阻挡。
        /// </summary>
        bool DetectPlayer()
        {
            if (_player == null || !_player.gameObject.activeInHierarchy) return false;

            float dist = Vector2.Distance(transform.position, _player.position);

            // 近距离直接感知
            if (dist <= hearingRange)
                return true;

            if (dist > detectionRange)
                return false;

            // 朝向检测
            float playerDir = Mathf.Sign(_player.position.x - transform.position.x);
            if (Mathf.Sign(_direction) != Mathf.Sign(playerDir))
                return false;

            // 视线检测：从稍高位置发射射线避免擦地
            Vector2 origin = (Vector2)transform.position + Vector2.up * 0.3f;
            Vector2 target = (Vector2)_player.position + Vector2.up * 0.3f;
            Vector2 dir = (target - origin).normalized;
            float rayDist = Vector2.Distance(origin, target);

            RaycastHit2D hit = Physics2D.Raycast(origin, dir, rayDist, terrainLayers);
            return hit.collider == null;
        }

        void UpdateFacing()
        {
            Vector3 s = transform.localScale;
            float mag = Mathf.Abs(s.x) > 0.01f ? Mathf.Abs(s.x) : 1f;
            s.x = mag * (_direction >= 0f ? 1f : -1f);
            transform.localScale = s;
        }

        IEnumerator ShowAttackEffect()
        {
            var go = new GameObject("SoldierAttackEffect");
            go.transform.position = transform.position + new Vector3(_direction * 0.7f, 0f, 0f);
            go.transform.localScale = new Vector3(1f, 1.2f, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteVisualHelper.CreateUnitSquareSprite();
            sr.color = new Color(1f, 1f, 1f, 0.35f);
            sr.sortingOrder = 5;

            float elapsed = 0f;
            const float duration = 0.15f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(0.35f, 0f, elapsed / duration);
                sr.color = new Color(1f, 1f, 1f, alpha);
                yield return null;
            }

            Destroy(go);
        }
    }
}

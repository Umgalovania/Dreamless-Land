using System.Collections;
using UnityEngine;

namespace DreamlessLand.Player
{
    /// <summary>
    /// 玩家控制器：A/D水平移动，K跳跃（仅地面），L闪避，U技能，
    /// S+K平台穿越，W/S梯子攀爬。
    /// 在梯子附近按S可穿过平台并抓住梯子。
    /// 使用向下 BoxCast 做地面检测，防止蹭墙刷新跳跃。
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CapsuleCollider2D))]
    public class PlayerController2D : MonoBehaviour
    {
        [Header("移动")]
        [SerializeField] float moveSpeed = 6.5f;

        [Header("跳跃")]
        [SerializeField] float jumpForce = 15.4f;
        [SerializeField] int maxJumpCount = 1;

        [Header("闪避")]
        [SerializeField] float dashSpeed = 15f;
        [SerializeField] float dashDuration = 0.15f;
        [SerializeField] float dashCooldown = 1f;

        [Header("地面检测")]
        [SerializeField] Transform groundCheck;
        [SerializeField] float groundCheckRadius = 0.18f;
        [SerializeField] float groundCheckWidth = 0.5f;
        [SerializeField] LayerMask groundLayers;
        [SerializeField] LayerMask platformLayerOnly;

        [Header("平台穿越")]
        [SerializeField] float platformIgnoreDuration = 0.4f;

        [Header("梯子")]
        [SerializeField] float climbSpeed = 4f;

        Rigidbody2D _rb;
        CapsuleCollider2D _capsule;
        int _jumpCount;
        bool _isDashing;
        float _nextDashTime;
        int _playerLayer;
        int _enemyLayer;
        int _platformLayer;
        bool _onLadder;
        bool _nearLadder;
        float _normalGravityScale = 3f;

        public float FacingSign { get; private set; } = 1f;
        public bool IsGrounded { get; private set; }
        public bool IsDashing => _isDashing;

        void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _capsule = GetComponent<CapsuleCollider2D>();
            _rb.freezeRotation = true;
            _rb.gravityScale = _normalGravityScale;

            _playerLayer = gameObject.layer;
            _enemyLayer = LayerMask.NameToLayer("Enemy");
            _platformLayer = LayerMask.NameToLayer("Platform");
        }

        void Update()
        {
            if (GameManager.Instance != null &&
                GameManager.Instance.CurrentState != GameState.Playing)
                return;

            if (_isDashing) return;

            float horizontal = Input.GetAxis("Horizontal");

            if (Mathf.Abs(horizontal) > 0.01f)
            {
                FacingSign = Mathf.Sign(horizontal);
                Vector3 s = transform.localScale;
                float mag = Mathf.Abs(s.x) > 0.01f ? Mathf.Abs(s.x) : 1f;
                s.x = mag * FacingSign;
                transform.localScale = s;
            }

            // 在梯子附近按S：穿过平台并抓住梯子
            if (_nearLadder && !_onLadder && Input.GetKey(KeyCode.S) && IsGroundedOnPlatform())
            {
                EnterLadder();
                return;
            }

            if (_onLadder) return;

            IsGrounded = CheckGrounded();
            if (IsGrounded)
                _jumpCount = 0;

            if (Input.GetKeyDown(KeyCode.K))
            {
                if (Input.GetKey(KeyCode.S) && IsGroundedOnPlatform())
                {
                    StartCoroutine(DropThroughPlatform());
                }
                else if (_jumpCount < maxJumpCount)
                {
                    Jump();
                }
            }

            if (Input.GetKeyDown(KeyCode.L) && Time.time >= _nextDashTime)
            {
                StartCoroutine(Dash());
            }

            if (Input.GetKeyDown(KeyCode.U))
            {
                UseSkill();
            }
        }

        void FixedUpdate()
        {
            if (_isDashing) return;

            float horizontal = Input.GetAxis("Horizontal");

            if (_onLadder)
            {
                float vertical = 0f;
                if (Input.GetKey(KeyCode.W)) vertical = 1f;
                if (Input.GetKey(KeyCode.S)) vertical = -1f;
                _rb.linearVelocity = new Vector2(horizontal * moveSpeed, vertical * climbSpeed);
                return;
            }

            _rb.linearVelocity = new Vector2(horizontal * moveSpeed, _rb.linearVelocity.y);
        }

        void Jump()
        {
            _jumpCount++;
            Vector2 v = _rb.linearVelocity;
            v.y = jumpForce;
            _rb.linearVelocity = v;
            Debug.Log($"[Player] 跳跃！(第{_jumpCount}段)");
        }

        IEnumerator Dash()
        {
            _isDashing = true;
            _nextDashTime = Time.time + dashCooldown;

            float originalGravity = _rb.gravityScale;
            _rb.gravityScale = 0f;
            _rb.linearVelocity = new Vector2(FacingSign * dashSpeed, 0f);

            if (_enemyLayer >= 0)
                Physics2D.IgnoreLayerCollision(_playerLayer, _enemyLayer, true);

            Debug.Log("[Player] 闪避！");
            yield return new WaitForSeconds(dashDuration);

            _rb.gravityScale = _onLadder ? 0f : originalGravity;
            _isDashing = false;

            if (_enemyLayer >= 0)
                Physics2D.IgnoreLayerCollision(_playerLayer, _enemyLayer, false);
        }

        /// <summary>
        /// 平台穿越：碰撞体级别忽略，绕过 PlatformEffector2D。
        /// </summary>
        IEnumerator DropThroughPlatform()
        {
            Vector2 point = GetGroundCheckPoint();
            Collider2D platformCol = Physics2D.OverlapCircle(
                point, groundCheckRadius + 0.3f, platformLayerOnly);
            if (platformCol == null) yield break;

            Physics2D.IgnoreCollision(_capsule, platformCol, true);

            Vector2 v = _rb.linearVelocity;
            v.y = -3f;
            _rb.linearVelocity = v;

            Debug.Log("[Player] 穿越平台下落！");
            yield return new WaitForSeconds(platformIgnoreDuration);

            if (_capsule != null && platformCol != null)
                Physics2D.IgnoreCollision(_capsule, platformCol, false);
        }

        void UseSkill()
        {
            Debug.Log("Skill Used");
        }

        // ---- 梯子 ----

        void EnterLadder()
        {
            _onLadder = true;
            _rb.gravityScale = 0f;
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, 0f);
            _jumpCount = maxJumpCount;

            if (_platformLayer >= 0)
                Physics2D.IgnoreLayerCollision(_playerLayer, _platformLayer, true);

            Debug.Log("[Player] 抓住梯子");
        }

        void ExitLadder()
        {
            _onLadder = false;
            _rb.gravityScale = _normalGravityScale;

            if (_platformLayer >= 0)
                Physics2D.IgnoreLayerCollision(_playerLayer, _platformLayer, false);

            Debug.Log("[Player] 离开梯子");
        }

        void OnTriggerEnter2D(Collider2D col)
        {
            if (col.GetComponent<DreamlessLand.Terrain.Ladder>() != null)
            {
                _nearLadder = true;

                // 如果玩家不在平台上，直接进入梯子模式；
                // 如果站在平台上，等待按S再进入
                if (!IsGroundedOnPlatform())
                    EnterLadder();
            }
        }

        void OnTriggerExit2D(Collider2D col)
        {
            if (col.GetComponent<DreamlessLand.Terrain.Ladder>() != null)
            {
                _nearLadder = false;

                if (_onLadder)
                    ExitLadder();
            }
        }

        // ---- 地面检测 ----

        /// <summary>
        /// 使用向下 BoxCast 检测地面，扁平检测框只朝下投射，
        /// 不会误检测到侧面的墙壁，从而防止蹭墙刷新跳跃次数。
        /// </summary>
        bool CheckGrounded()
        {
            Vector2 origin = GetGroundCheckPoint();
            RaycastHit2D hit = Physics2D.BoxCast(
                origin,
                new Vector2(groundCheckWidth, 0.02f),
                0f, Vector2.down, groundCheckRadius,
                groundLayers);
            return hit.collider != null;
        }

        bool IsGroundedOnPlatform()
        {
            Vector2 origin = GetGroundCheckPoint();
            RaycastHit2D hit = Physics2D.BoxCast(
                origin,
                new Vector2(groundCheckWidth, 0.02f),
                0f, Vector2.down, groundCheckRadius,
                platformLayerOnly);
            return hit.collider != null;
        }

        Vector2 GetGroundCheckPoint()
        {
            if (groundCheck != null)
                return groundCheck.position;

            if (_capsule != null)
            {
                Bounds b = _capsule.bounds;
                return new Vector2(b.center.x, b.min.y);
            }

            return transform.position;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Vector2 p = groundCheck != null
                ? (Vector2)groundCheck.position
                : (Vector2)transform.position;
            Gizmos.DrawWireCube(p + Vector2.down * groundCheckRadius * 0.5f,
                new Vector3(groundCheckWidth, groundCheckRadius, 0f));
        }
    }
}

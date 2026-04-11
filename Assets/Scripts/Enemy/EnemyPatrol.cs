using UnityEngine;

namespace DreamlessLand.Enemy
{
    /// <summary>
    /// 敌人巡逻AI：左右巡逻，射线检测墙壁/边缘掉头，支持 ResetPatrol 重置。
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyPatrol : MonoBehaviour
    {
        [Header("巡逻参数")]
        [SerializeField] float moveSpeed = 2f;
        [SerializeField] float patrolHalfWidth = 3f;

        [Header("射线检测")]
        [SerializeField] float wallCheckDistance = 0.5f;
        [SerializeField] float edgeCheckDistance = 1.5f;
        [SerializeField] LayerMask terrainLayers;

        Rigidbody2D _rb;
        float _startX;
        float _direction = 1f;
        Vector3 _spawnPosition;

        void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.bodyType = RigidbodyType2D.Kinematic;
            _rb.freezeRotation = true;
            _spawnPosition = transform.position;
        }

        void Start()
        {
            _startX = transform.position.x;

            if (terrainLayers == 0)
            {
                int terrain = LayerMask.NameToLayer("Terrain");
                int platform = LayerMask.NameToLayer("Platform");
                if (terrain >= 0) terrainLayers |= 1 << terrain;
                if (platform >= 0) terrainLayers |= 1 << platform;
            }
        }

        void FixedUpdate()
        {
            float left = _startX - patrolHalfWidth;
            float right = _startX + patrolHalfWidth;
            float nextX = _rb.position.x + _direction * moveSpeed * Time.fixedDeltaTime;

            Vector2 wallOrigin = (Vector2)transform.position;
            Vector2 wallDir = new Vector2(_direction, 0f);
            RaycastHit2D wallHit = Physics2D.Raycast(wallOrigin, wallDir, wallCheckDistance, terrainLayers);

            Vector2 edgeOrigin = (Vector2)transform.position + new Vector2(_direction * 0.5f, 0f);
            RaycastHit2D edgeHit = Physics2D.Raycast(edgeOrigin, Vector2.down, edgeCheckDistance, terrainLayers);

            bool shouldTurn = wallHit.collider != null
                || edgeHit.collider == null
                || nextX >= right || nextX <= left;

            if (shouldTurn)
            {
                _direction = -_direction;
                nextX = _rb.position.x + _direction * moveSpeed * Time.fixedDeltaTime;
            }

            nextX = Mathf.Clamp(nextX, left, right);
            _rb.MovePosition(new Vector2(nextX, _rb.position.y));

            Vector3 s = transform.localScale;
            float mag = Mathf.Abs(s.x) > 0.01f ? Mathf.Abs(s.x) : 1f;
            s.x = mag * (_direction >= 0f ? 1f : -1f);
            transform.localScale = s;
        }

        /// <summary>
        /// 重置巡逻状态：归位、方向重置、更新起始X。
        /// </summary>
        public void ResetPatrol()
        {
            transform.position = _spawnPosition;
            _startX = _spawnPosition.x;
            _direction = 1f;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Vector2 wallOrigin = transform.position;
            Gizmos.DrawLine(wallOrigin, wallOrigin + new Vector2(_direction * wallCheckDistance, 0f));

            Gizmos.color = Color.yellow;
            Vector2 edgeOrigin = (Vector2)transform.position + new Vector2(_direction * 0.5f, 0f);
            Gizmos.DrawLine(edgeOrigin, edgeOrigin + Vector2.down * edgeCheckDistance);

            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            float startX = Application.isPlaying ? _startX : transform.position.x;
            Gizmos.DrawLine(
                new Vector3(startX - patrolHalfWidth, transform.position.y, 0),
                new Vector3(startX + patrolHalfWidth, transform.position.y, 0));
        }
    }
}

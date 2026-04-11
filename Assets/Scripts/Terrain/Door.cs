using UnityEngine;

namespace DreamlessLand.Terrain
{
    /// <summary>
    /// 门机关：有开启/关闭两种状态。
    /// 关闭时碰撞体为实体阻挡通行；开启时碰撞体禁用，允许通过。
    /// 玩家靠近后按F键交互切换状态。
    /// </summary>
    public class Door : MonoBehaviour
    {
        [Header("门状态")]
        [SerializeField] bool isOpen = false;

        [Header("交互")]
        [SerializeField] float interactionRange = 2f;

        [Header("视觉")]
        [SerializeField] Color closedColor = new Color(0.2f, 0.4f, 1f, 1f);
        [SerializeField] Color openColor = new Color(0.2f, 0.4f, 1f, 0.3f);

        BoxCollider2D _doorCollider;
        SpriteRenderer _spriteRenderer;
        Transform _player;
        bool _playerInRange;

        // UI 提示引用（由 TestSceneBuilder 或外部设置）
        UnityEngine.UI.Text _promptText;

        void Awake()
        {
            _doorCollider = GetComponent<BoxCollider2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            UpdateDoorVisual();
        }

        void Start()
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                _player = playerObj.transform;

            // 查找场景中的交互提示 UI
            var promptGo = GameObject.Find("InteractionPrompt");
            if (promptGo != null)
                _promptText = promptGo.GetComponentInChildren<UnityEngine.UI.Text>();

            // 初始隐藏提示
            if (_promptText != null)
                _promptText.gameObject.SetActive(false);
        }

        void Update()
        {
            if (_player == null) return;

            float dist = Vector2.Distance(transform.position, _player.position);
            bool inRange = dist < interactionRange;

            if (inRange != _playerInRange)
            {
                _playerInRange = inRange;
                UpdatePrompt();
            }

            if (_playerInRange && Input.GetKeyDown(KeyCode.F))
            {
                ToggleDoor();
            }
        }

        void ToggleDoor()
        {
            isOpen = !isOpen;
            UpdateDoorVisual();
            Debug.Log($"[Door] 门已{(isOpen ? "开启" : "关闭")}");
        }

        void UpdateDoorVisual()
        {
            if (_doorCollider != null)
                _doorCollider.enabled = !isOpen;

            if (_spriteRenderer != null)
                _spriteRenderer.color = isOpen ? openColor : closedColor;
        }

        void UpdatePrompt()
        {
            if (_promptText != null)
            {
                _promptText.gameObject.SetActive(_playerInRange);
                if (_playerInRange)
                    _promptText.text = isOpen ? "按 F 关门" : "按 F 开门";
            }
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, interactionRange);
        }
    }
}

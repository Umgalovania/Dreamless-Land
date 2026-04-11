using UnityEngine;
using DreamlessLand.Player;

namespace DreamlessLand.Common
{
    public enum PickupEffect { None, RestoreHP, BoostATK }

    /// <summary>
    /// 掉落物/可拾取物品：玩家靠近按F拾取，暂停游戏并显示信息。
    /// 部分掉落物具有特殊效果（恢复血量、增加攻击力等）。
    /// </summary>
    public class Pickup : MonoBehaviour
    {
        [Header("物品信息")]
        [SerializeField] string itemName = "物品";
        [SerializeField][TextArea(3, 10)] string description = "";

        [Header("特殊效果")]
        [SerializeField] PickupEffect effect = PickupEffect.None;
        [SerializeField] int effectValue = 0;

        [Header("交互")]
        [SerializeField] float interactionRange = 1.5f;

        Transform _player;
        bool _playerInRange;
        UnityEngine.UI.Text _promptText;
        bool _pickedUp;

        public string ItemName => itemName;

        void Start()
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                _player = playerObj.transform;

            var promptGo = GameObject.Find("InteractionPrompt");
            if (promptGo != null)
                _promptText = promptGo.GetComponentInChildren<UnityEngine.UI.Text>();
        }

        void Update()
        {
            if (_pickedUp || _player == null) return;
            if (GameManager.Instance != null &&
                GameManager.Instance.CurrentState != GameState.Playing)
                return;

            float dist = Vector2.Distance(transform.position, _player.position);
            bool inRange = dist < interactionRange;

            if (inRange != _playerInRange)
            {
                _playerInRange = inRange;
                UpdatePrompt();
            }

            if (_playerInRange && Input.GetKeyDown(KeyCode.F))
                PickUp();
        }

        void PickUp()
        {
            _pickedUp = true;
            ApplyEffect();

            var pickupUI = FindFirstObjectByType<UI.PickupUI>(FindObjectsInactive.Include);
            if (pickupUI != null)
                pickupUI.Show(itemName, description);

            if (_promptText != null)
                _promptText.gameObject.SetActive(false);

            gameObject.SetActive(false);
            Debug.Log($"[Pickup] 拾取: {itemName}");
        }

        void ApplyEffect()
        {
            if (_player == null) return;

            switch (effect)
            {
                case PickupEffect.RestoreHP:
                    var unit = _player.GetComponent<PlayerUnit>();
                    if (unit != null)
                    {
                        unit.ResetHealth();
                        Debug.Log($"[Pickup] 恢复生命值");
                    }
                    break;

                case PickupEffect.BoostATK:
                    Debug.Log($"[Pickup] 攻击力提升 +{effectValue}（待实现）");
                    break;
            }
        }

        void UpdatePrompt()
        {
            if (_promptText != null)
            {
                _promptText.gameObject.SetActive(_playerInRange);
                if (_playerInRange)
                    _promptText.text = $"按 F 拾取 {itemName}";
            }
        }
    }
}

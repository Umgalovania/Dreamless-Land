using UnityEngine;
using DreamlessLand.Common;
using DreamlessLand.Player;
using DreamlessLand.Enemy;

namespace DreamlessLand.Terrain
{
    /// <summary>
    /// 休息点机关：玩家按F交互可回满血量、存档、刷新所有敌人。
    /// 用红色色块表示。
    /// </summary>
    public class RestPoint : MonoBehaviour
    {
        [SerializeField] float interactionRange = 2.5f;

        Transform _player;
        bool _playerInRange;
        UnityEngine.UI.Text _promptText;

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
            if (_player == null) return;

            float dist = Vector2.Distance(transform.position, _player.position);
            bool inRange = dist < interactionRange;

            if (inRange != _playerInRange)
            {
                _playerInRange = inRange;
                UpdatePrompt();
            }

            if (_playerInRange && Input.GetKeyDown(KeyCode.F))
                Rest();
        }

        void Rest()
        {
            var playerUnit = _player.GetComponent<PlayerUnit>();
            var playerCombat = _player.GetComponent<PlayerCombat>();

            // 回满血量
            if (playerUnit != null)
                playerUnit.ResetHealth();

            // 存档（保存玩家应出生的位置：休息点上方）
            int maxHp = playerUnit != null ? playerUnit.MaxHealth : 5;
            int atk = playerCombat != null ? playerCombat.AttackDamage : 4;
            Vector2 spawnPos = new Vector2(transform.position.x, transform.position.y + 1.5f);
            SaveSystem.Save(spawnPos, maxHp, atk);

            // 刷新所有敌人
            EnemyUnit.RefreshAllEnemies();

            Debug.Log("[RestPoint] 已休息、存档、刷新敌人！");
        }

        void UpdatePrompt()
        {
            if (_promptText != null)
            {
                _promptText.gameObject.SetActive(_playerInRange);
                if (_playerInRange)
                    _promptText.text = "按 F 休息并存档";
            }
        }
    }
}

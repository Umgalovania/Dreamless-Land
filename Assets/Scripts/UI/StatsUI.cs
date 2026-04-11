using UnityEngine;
using UnityEngine.UI;
using DreamlessLand.Player;

namespace DreamlessLand.UI
{
    /// <summary>
    /// 属性面板：按Tab键显示/隐藏，展示HP、攻击力、装备栏。
    /// </summary>
    public class StatsUI : MonoBehaviour
    {
        [SerializeField] GameObject statsPanel;
        [SerializeField] Text hpText;
        [SerializeField] Text attackText;
        [SerializeField] Text equipmentText;

        bool _isVisible;

        void Start()
        {
            if (statsPanel != null) statsPanel.SetActive(false);
            _isVisible = false;
        }

        public void Toggle()
        {
            _isVisible = !_isVisible;
            if (statsPanel != null)
                statsPanel.SetActive(_isVisible);
            if (_isVisible)
                Refresh();
        }

        public void Hide()
        {
            _isVisible = false;
            if (statsPanel != null)
                statsPanel.SetActive(false);
        }

        void Refresh()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;

            var unit = player.GetComponent<PlayerUnit>();
            var combat = player.GetComponent<PlayerCombat>();

            if (hpText != null && unit != null)
                hpText.text = $"HP:  {unit.CurrentHealth} / {unit.MaxHealth}";
            if (attackText != null && combat != null)
                attackText.text = $"攻击力:  {combat.AttackDamage}";
            if (equipmentText != null)
                equipmentText.text = "装备栏:  (空)";
        }
    }
}

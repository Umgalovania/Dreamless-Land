using UnityEngine;
using UnityEngine.UI;
using DreamlessLand.Player;

namespace DreamlessLand.UI
{
    /// <summary>
    /// 血量 UI：在 Canvas 上显示玩家当前血量（爱心图标）。
    /// </summary>
    public class HealthUI : MonoBehaviour
    {
        [SerializeField] Text healthText;

        PlayerUnit _player;

        void Start()
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                _player = playerObj.GetComponent<PlayerUnit>();
                if (_player != null)
                {
                    _player.OnHealthChanged += OnHealthChanged;
                    OnHealthChanged(_player.CurrentHealth, _player.MaxHealth);
                }
            }

            if (_player == null)
                Debug.LogWarning("[HealthUI] 未找到玩家单位 PlayerUnit");
        }

        void OnHealthChanged(int current, int max)
        {
            if (healthText == null) return;

            string hearts = "";
            for (int i = 0; i < max; i++)
                hearts += i < current ? "♥ " : "♡ ";
            healthText.text = hearts;
        }

        void OnDestroy()
        {
            if (_player != null)
                _player.OnHealthChanged -= OnHealthChanged;
        }
    }
}

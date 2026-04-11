using UnityEngine;
using UnityEngine.UI;

namespace DreamlessLand.UI
{
    /// <summary>
    /// 游戏结束界面：显示 GAME OVER + "再次尝试"/"返回主菜单" 按钮。
    /// </summary>
    public class GameOverUI : MonoBehaviour
    {
        [SerializeField] GameObject gameOverPanel;
        [SerializeField] Button retryButton;
        [SerializeField] Button menuButton;

        void Start()
        {
            if (gameOverPanel != null)
                gameOverPanel.SetActive(false);
            if (retryButton != null)
                retryButton.onClick.AddListener(OnRetry);
            if (menuButton != null)
                menuButton.onClick.AddListener(OnMenu);
        }

        public void Show()
        {
            if (gameOverPanel != null)
                gameOverPanel.SetActive(true);
            Debug.Log("[UI] 显示 Game Over");
        }

        void OnRetry()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.RetryFromSave();
        }

        void OnMenu()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.ReturnToMainMenu();
        }
    }
}

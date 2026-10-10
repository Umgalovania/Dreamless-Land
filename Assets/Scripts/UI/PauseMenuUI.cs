using UnityEngine;
using UnityEngine.UI;

namespace DreamlessLand.UI
{
    /// <summary>
    /// 暂停菜单：继续游戏、设置（暂同继续）、返回主菜单（含确认弹窗）。
    /// </summary>
    public class PauseMenuUI : MonoBehaviour
    {
        [SerializeField] GameObject pausePanel;
        [SerializeField] GameObject confirmPanel;
        [SerializeField] Button continueButton;
        [SerializeField] Button settingsButton;
        [SerializeField] Button returnMenuButton;
        [SerializeField] Button confirmYesButton;
        [SerializeField] Button confirmNoButton;

        void Start()
        {
            if (continueButton != null)
                continueButton.onClick.AddListener(OnContinue);
            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnContinue);
            if (returnMenuButton != null)
                returnMenuButton.onClick.AddListener(OnReturnMenu);
            if (confirmYesButton != null)
                confirmYesButton.onClick.AddListener(OnConfirmYes);
            if (confirmNoButton != null)
                confirmNoButton.onClick.AddListener(OnConfirmNo);

            if (pausePanel != null) pausePanel.SetActive(false);
            if (confirmPanel != null) confirmPanel.SetActive(false);
        }

        public void Show()
        {
            if (pausePanel != null) pausePanel.SetActive(true);
            if (confirmPanel != null) confirmPanel.SetActive(false);
        }

        public void Hide()
        {
            if (pausePanel != null) pausePanel.SetActive(false);
            if (confirmPanel != null) confirmPanel.SetActive(false);
        }

        void OnContinue()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.Resume();
        }

        void OnReturnMenu()
        {
            if (confirmPanel != null)
                confirmPanel.SetActive(true);
        }

        void OnConfirmYes()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.ReturnToMainMenu();
        }

        void OnConfirmNo()
        {
            if (confirmPanel != null)
                confirmPanel.SetActive(false);
        }
    }
}

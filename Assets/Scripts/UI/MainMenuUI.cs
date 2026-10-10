using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DreamlessLand.Common;

namespace DreamlessLand.UI
{
    /// <summary>
    /// 主菜单界面：新游戏、继续游戏、游戏说明、退出游戏。
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] Button newGameButton;
        [SerializeField] Button continueButton;
        [SerializeField] Button instructionsButton;
        [SerializeField] Button quitButton;
        [SerializeField] GameObject instructionsPanel;
        [SerializeField] Button closeInstructionsButton;

        void Start()
        {
            Time.timeScale = 1f;

            if (newGameButton != null)
                newGameButton.onClick.AddListener(OnNewGame);
            if (continueButton != null)
            {
                continueButton.onClick.AddListener(OnContinueGame);
                continueButton.interactable = SaveSystem.HasSave;
            }
            if (instructionsButton != null)
                instructionsButton.onClick.AddListener(OnShowInstructions);
            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuit);
            if (closeInstructionsButton != null)
                closeInstructionsButton.onClick.AddListener(OnCloseInstructions);

            if (instructionsPanel != null)
                instructionsPanel.SetActive(false);
        }

        void OnNewGame()
        {
            SaveSystem.DeleteSave();
            GameManager.ShouldLoadFromSave = false;
            SceneManager.LoadScene("Level1");
        }

        void OnContinueGame()
        {
            GameManager.ShouldLoadFromSave = true;
            SceneManager.LoadScene("Level1");
        }

        void OnShowInstructions()
        {
            if (instructionsPanel != null)
                instructionsPanel.SetActive(true);
        }

        void OnCloseInstructions()
        {
            if (instructionsPanel != null)
                instructionsPanel.SetActive(false);
        }

        void OnQuit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}

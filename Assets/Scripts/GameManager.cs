using UnityEngine;
using UnityEngine.SceneManagement;
using DreamlessLand.Common;
using DreamlessLand.UI;

namespace DreamlessLand
{
    public enum GameState { Playing, Paused, GameOver }

    /// <summary>
    /// 游戏管理器单例：管理游戏状态、暂停、场景切换、存档读取。
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        public static bool ShouldLoadFromSave { get; set; }

        [SerializeField] GameState currentState = GameState.Playing;

        PauseMenuUI _pauseUI;
        GameOverUI _gameOverUI;
        StatsUI _statsUI;
        PickupUI _pickupUI;

        public GameState CurrentState => currentState;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            Time.timeScale = 1f;
        }

        void Start()
        {
            currentState = GameState.Playing;

            _pauseUI = FindFirstObjectByType<PauseMenuUI>(FindObjectsInactive.Include);
            _gameOverUI = FindFirstObjectByType<GameOverUI>(FindObjectsInactive.Include);
            _statsUI = FindFirstObjectByType<StatsUI>(FindObjectsInactive.Include);
            _pickupUI = FindFirstObjectByType<PickupUI>(FindObjectsInactive.Include);

            if (ShouldLoadFromSave && SaveSystem.HasSave)
            {
                var data = SaveSystem.Load();
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    player.transform.position = new Vector3(
                        data.spawnPosition.x, data.spawnPosition.y, 0f);
                    Debug.Log($"[GameManager] 从存档加载，出生点: {data.spawnPosition}");
                }
                ShouldLoadFromSave = false;
            }

            Debug.Log("[GameManager] 游戏开始！");
        }

        void Update()
        {
            if (_pickupUI != null && _pickupUI.IsShowing) return;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (currentState == GameState.Playing)
                    Pause();
                else if (currentState == GameState.Paused)
                    Resume();
            }

            if (Input.GetKeyDown(KeyCode.Tab) && currentState == GameState.Playing)
            {
                if (_statsUI != null) _statsUI.Toggle();
            }
        }

        public void Pause()
        {
            currentState = GameState.Paused;
            Time.timeScale = 0f;
            if (_statsUI != null) _statsUI.Hide();
            if (_pauseUI != null) _pauseUI.Show();
            Debug.Log("[GameManager] 游戏暂停");
        }

        public void Resume()
        {
            currentState = GameState.Playing;
            Time.timeScale = 1f;
            if (_pauseUI != null) _pauseUI.Hide();
            Debug.Log("[GameManager] 继续游戏");
        }

        public void TriggerGameOver()
        {
            if (currentState == GameState.GameOver) return;
            currentState = GameState.GameOver;
            Time.timeScale = 0f;
            if (_statsUI != null) _statsUI.Hide();
            if (_gameOverUI != null) _gameOverUI.Show();
            Debug.Log("[GameManager] ===== GAME OVER =====");
        }

        public void RetryFromSave()
        {
            Time.timeScale = 1f;
            ShouldLoadFromSave = SaveSystem.HasSave;
            Instance = null;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void ReturnToMainMenu()
        {
            Time.timeScale = 1f;
            Instance = null;
            SceneManager.LoadScene("MainMenu");
        }
    }
}

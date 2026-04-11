using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DreamlessLand.UI
{
    /// <summary>
    /// 掉落物信息面板：拾取物品后暂停游戏并显示名称与描述，
    /// 0.5秒后按任意键关闭面板并恢复游戏。
    /// </summary>
    public class PickupUI : MonoBehaviour
    {
        [SerializeField] GameObject pickupPanel;
        [SerializeField] Text nameText;
        [SerializeField] Text descriptionText;
        [SerializeField] Text hintText;

        bool _waitingForKey;
        float _showTime;

        public bool IsShowing => _waitingForKey;

        void Start()
        {
            if (pickupPanel != null)
                pickupPanel.SetActive(false);
        }

        public void Show(string itemName, string desc)
        {
            if (pickupPanel != null) pickupPanel.SetActive(true);
            if (nameText != null) nameText.text = itemName;
            if (descriptionText != null) descriptionText.text = desc;
            if (hintText != null) hintText.text = "";

            _showTime = Time.unscaledTime;
            _waitingForKey = true;
            Time.timeScale = 0f;
        }

        void Update()
        {
            if (!_waitingForKey) return;

            float elapsed = Time.unscaledTime - _showTime;

            if (elapsed >= 0.5f)
            {
                if (hintText != null && string.IsNullOrEmpty(hintText.text))
                    hintText.text = "按任意键继续...";

                if (Input.anyKeyDown)
                    StartCoroutine(HideNextFrame());
            }
        }

        /// <summary>
        /// 延迟一帧再恢复游戏，防止按键事件传递给 GameManager 导致立即暂停。
        /// </summary>
        IEnumerator HideNextFrame()
        {
            _waitingForKey = false;
            yield return null;

            if (pickupPanel != null)
                pickupPanel.SetActive(false);
            Time.timeScale = 1f;
        }
    }
}

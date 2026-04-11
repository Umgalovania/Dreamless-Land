using UnityEngine;

namespace DreamlessLand.Common
{
    /// <summary>
    /// 存档数据结构。
    /// </summary>
    public struct SaveData
    {
        public Vector2 spawnPosition;
        public int maxHealth;
        public int attackPower;
    }

    /// <summary>
    /// 存档系统：使用 PlayerPrefs 保存/读取玩家在休息点的状态。
    /// </summary>
    public static class SaveSystem
    {
        const string KEY_EXISTS = "save_exists";
        const string KEY_SPAWN_X = "save_spawn_x";
        const string KEY_SPAWN_Y = "save_spawn_y";
        const string KEY_MAX_HP = "save_max_hp";
        const string KEY_ATK = "save_atk";

        public static bool HasSave => PlayerPrefs.GetInt(KEY_EXISTS, 0) == 1;

        public static void Save(Vector2 spawnPos, int maxHp, int atk)
        {
            PlayerPrefs.SetInt(KEY_EXISTS, 1);
            PlayerPrefs.SetFloat(KEY_SPAWN_X, spawnPos.x);
            PlayerPrefs.SetFloat(KEY_SPAWN_Y, spawnPos.y);
            PlayerPrefs.SetInt(KEY_MAX_HP, maxHp);
            PlayerPrefs.SetInt(KEY_ATK, atk);
            PlayerPrefs.Save();
            Debug.Log($"[SaveSystem] 已存档：位置({spawnPos.x:F1}, {spawnPos.y:F1}), HP={maxHp}, ATK={atk}");
        }

        public static SaveData Load()
        {
            return new SaveData
            {
                spawnPosition = new Vector2(
                    PlayerPrefs.GetFloat(KEY_SPAWN_X, -10f),
                    PlayerPrefs.GetFloat(KEY_SPAWN_Y, 2f)),
                maxHealth = PlayerPrefs.GetInt(KEY_MAX_HP, 5),
                attackPower = PlayerPrefs.GetInt(KEY_ATK, 4)
            };
        }

        public static void DeleteSave()
        {
            PlayerPrefs.DeleteKey(KEY_EXISTS);
            PlayerPrefs.DeleteKey(KEY_SPAWN_X);
            PlayerPrefs.DeleteKey(KEY_SPAWN_Y);
            PlayerPrefs.DeleteKey(KEY_MAX_HP);
            PlayerPrefs.DeleteKey(KEY_ATK);
            PlayerPrefs.Save();
            Debug.Log("[SaveSystem] 存档已删除");
        }
    }
}

using UnityEngine;
using DreamlessLand.Common;

namespace DreamlessLand.Enemy
{
    /// <summary>
    /// 敌人单位：死亡时隐藏（不销毁），支持通过休息点刷新复活。
    /// </summary>
    public class EnemyUnit : Unit
    {
        Vector3 _spawnPosition;

        protected override void Awake()
        {
            faction = Faction.Enemy;
            base.Awake();
            _spawnPosition = transform.position;
        }

        public override void Die()
        {
            base.Die();
            Debug.Log($"[Enemy] {gameObject.name} 被消灭！");
            gameObject.SetActive(false);
        }

        /// <summary>
        /// 刷新场景中所有敌人（含已隐藏的）：复活、回满血、归位。
        /// </summary>
        public static void RefreshAllEnemies()
        {
            var enemies = FindObjectsByType<EnemyUnit>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var e in enemies)
            {
                e.transform.position = e._spawnPosition;
                e.ResetHealth();
                e.gameObject.SetActive(true);

                var patrol = e.GetComponent<EnemyPatrol>();
                if (patrol != null) patrol.ResetPatrol();
            }
            Debug.Log($"[EnemyUnit] 已刷新 {enemies.Length} 个敌人");
        }
    }
}

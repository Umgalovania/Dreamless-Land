using UnityEngine;

namespace DreamlessLand.Player
{
    /// <summary>
    /// 摄像机跟随：平滑跟随目标（玩家），保持固定Z轴偏移。
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] Transform target;
        [SerializeField] Vector3 offset = new Vector3(0f, 2f, -10f);
        [SerializeField] float smoothSpeed = 8f;

        void LateUpdate()
        {
            if (target == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) target = player.transform;
                return;
            }

            Vector3 desired = new Vector3(
                target.position.x + offset.x,
                target.position.y + offset.y,
                offset.z);
            transform.position = Vector3.Lerp(transform.position, desired, smoothSpeed * Time.deltaTime);
        }
    }
}

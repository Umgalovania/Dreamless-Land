using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace DreamlessLand.Common
{
    /// <summary>
    /// URP 下主摄像机需要 UniversalAdditionalCameraData，否则 Game 视图可能全黑/无画面。
    /// </summary>
    public static class RenderingRuntimeFix
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AfterSceneLoad()
        {
            var cams = Camera.allCameras;
            if (cams == null || cams.Length == 0) return;

            foreach (var cam in cams)
            {
                if (cam == null) continue;
                if (cam.GetComponent<UniversalAdditionalCameraData>() == null)
                    cam.gameObject.AddComponent<UniversalAdditionalCameraData>();
            }
        }
    }
}

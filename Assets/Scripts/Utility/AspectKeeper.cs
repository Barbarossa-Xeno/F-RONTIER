// Reference: https://3dunity.org/game-create-lesson/clicker-game/mobile-adjustment/

using UnityEngine;

namespace FRONTIER.Utility
{
    /// <summary>
    /// カメラのアスペクト比を固定する。
    /// </summary>
    [ExecuteAlways, RequireComponent(typeof(Camera))]
    public class AspectKeeper : MonoBehaviour
    {
        #region フィールド

        [SerializeField] private Camera targetCamera;
        [SerializeField] private Vector2 aspect;

        #endregion

        #region MonoBehaviourメソッド

        void Start() => targetCamera = targetCamera ?? GetComponent<Camera>();

        void Update()
        {
            if (targetCamera == null) return;

            float screenAspect = Screen.width / (float)Screen.height;
            float targetAspect = aspect.x / aspect.y;
            float ratio = targetAspect / screenAspect;
            Rect viewportRect = new(0, 0, 1, 1);

            if (ratio < 1)
            {
                viewportRect.width = ratio;
                viewportRect.x = 0.5f - viewportRect.width * 0.5f;
            }
            else
            {
                viewportRect.height = 1 / ratio;
                viewportRect.y = 0.5f - viewportRect.height * 0.5f;
            }

            targetCamera.rect = viewportRect;
        }

        #endregion
    }
}

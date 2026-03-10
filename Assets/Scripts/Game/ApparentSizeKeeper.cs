// Reference : https://qiita.com/r-ngtm/items/e4df707d45b097999776

using UnityEngine;

namespace FRONTIER.Game
{
    [ExecuteAlways]
    public class ApparentSizeKeeper : MonoBehaviour
    {
        #region フィールド

        /// <summary>
        /// カメラからの距離が１のときのスケール
        /// </summary>
        private Vector3 baseScale;

        #endregion

        #region MonoBehaviourメソッド

        void Start() => baseScale = transform.localScale / GetDistance();

        void Update() => transform.localScale = baseScale * GetDistance();

        #endregion

        #region メソッド

        /// <summary>
        /// オブジェクトとカメラの間の距離を取得する。
        /// </summary>
        /// <returns>
        /// ベクトルの長さ
        /// </returns>
        private float GetDistance() => (transform.position - Camera.main.transform.position).magnitude;

        #endregion
    }
}

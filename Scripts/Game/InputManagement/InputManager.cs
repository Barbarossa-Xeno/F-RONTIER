using UnityEngine;
using UnityEngine.Events;

namespace FRONTIER.Game.InputManageMent
{
    /// <summary>
    /// 各レーンの入力を管理する。
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        #region フィールド

        /// <summary>
        /// レーンの光る速さ。
        /// </summary>
        [Range(0.1f, 2f)] public float lightSpeed = 0.1f;

        /// <summary>
        /// 各レーンが入力されたかどうかのフラグ。
        /// </summary>
        public bool[] inputFlag = new bool[6];

        /// <summary>
        /// 各レーンに入力があったときの時間。
        /// </summary>
        public float[] inputTime = new float[6];

        /// <summary>
        /// 各レーンに入力があったとき、発火するイベント。
        /// </summary>
        public UnityEvent<int, float>[] onInput = new UnityEvent<int, float>[6];

        #endregion
    }
}
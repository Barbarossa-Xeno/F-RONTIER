using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace FRONTIER.Game.Judgement
{
    /// <summary>
    /// 各レーンの入力を管理する。
    /// </summary>
    public class LaneManager : MonoBehaviour
    {
        #region フィールド

        /// <summary>
        /// レーンの光る速さ（インスペクタ調整用）。
        /// </summary>
        [SerializeField, Range(0.1f, 2f)] private float lightSpeed = 0.1f;

        /// <summary>
        /// レーンの光る速さ。
        /// </summary>
        public float LightSpeed => lightSpeed;

        /// <summary>
        /// 各レーンがタップされたか示すフラグ。
        /// </summary>
        /// <remarks>
        /// <see cref="Lane"/> からデータを渡すことのみの使用を想定
        /// </remarks> 
        public bool[] tappedLaneFlags = new bool[6];

        /// <summary>
        /// 各レーンがタップされたときの時間。
        /// </summary>
        /// /// <remarks>
        /// <see cref="Lane"/> からデータを渡すことのみの使用を想定
        /// </remarks> 
        public float[] tappedTime = new float[6];

        /// <summary>
        /// 各レーンに入力があったとき、発火するイベントのリスト。
        /// 各要素が各レーンに対応する。
        /// </summary>
        public List<UnityEvent<int, float>> TappedEvents { get; } = Enumerable.Range(0, 6)
            .Select(_ => new UnityEvent<int, float>())
            .ToList();
        #endregion
    }
}

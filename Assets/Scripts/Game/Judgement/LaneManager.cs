using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace FRONTIER.Game.Judgement
{
    /// <summary>
    /// 各レーンの入力を管理する。
    /// </summary>
    public class LaneManager : Utility.GameUtilityBase
    {
        #region フィールド

        /// <summary>
        /// ノーツが流れる、シーン上の6つのレーン（インスペクタ確認用）。
        /// </summary>
        [Header("階下の Lane を6つ登録"), SerializeField] private Lane[] lanes = new Lane[6];

        /// <summary>
        /// レーンの光る速さ（インスペクタ調整用）。
        /// </summary>
        [SerializeField, Range(0.1f, 2f)] private float lightSpeed = 0.1f;

        #endregion

        #region プロパティ

        /// <summary>
        /// ノーツが流れる、シーン上の6つのレーン。
        /// この配列のインデックスと、要素の <c>Lane</c> クラスの <c>laneIndex</c> フィールドは対応している必要がある。
        /// </summary>
        public Lane[] Lanes => lanes;

        #endregion

        #region MonoBehaviourメソッド

        void Awake()
        {
            foreach (var lane in lanes)
            {
                if (lane == null)
                {
                    Debug.LogError("未登録のレーンがあります。");
                    break;
                }

                lane.LightSpeed = lightSpeed;

                // レーンがタップされた時のSEを登録
                lane.TappedEvent += (index, time) => Manager.audios.seManager.Play(Audio.SEManager.SE.TapedLane);
            }
        }

        #endregion
    }
}

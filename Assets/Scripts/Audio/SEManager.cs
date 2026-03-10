using UnityEngine;
using FRONTIER.Utility;

namespace FRONTIER.Audio
{
    /// <summary>
    /// 効果音（SE）を再生するためのオーディオソースを管理する。
    /// </summary>
    public class SEManager : AudioManager
    {
        /// <summary>
        /// SEのデータ。
        /// </summary>
        private SEData seData;

        /// <summary>
        /// SEのリソースを取得する。
        /// </summary>
        private class SEData
        {
            private AudioClip selectedCell;
            private AudioClip woodBlockBeat;
            private AudioClip greatOrPerfect;
            private AudioClip goodOrBad;
            private AudioClip tapedLane;

            public SEData()
            {
                selectedCell = Resources.Load<AudioClip>("SE/select_cell");
                woodBlockBeat = Resources.Load<AudioClip>("SE/woodblock_1");
                greatOrPerfect = Resources.Load<AudioClip>("SE/heel_1");
                goodOrBad = Resources.Load<AudioClip>("SE/heel_2");
                tapedLane = Resources.Load<AudioClip>("SE/tap");
            }

            /// <summary>
            /// SEの名前をもとにオーディオクリップを渡す。
            /// </summary>
            /// <param name="name"></param>
            /// <returns></returns>
            public AudioClip NameToClip(SE name)
            {
                return name switch
                {
                    SE.SelectedCell => selectedCell,
                    SE.WoodBlockBeat => woodBlockBeat,
                    SE.GreatOrPerfect => greatOrPerfect,
                    SE.GoodOrBad => goodOrBad,
                    SE.TapedLane => tapedLane,
                    _ => null
                };
            }
        }

        public enum SE
        {
            /// <summary>
            /// 選曲メニューでセルを選択したときに再生する。
            /// </summary>
            SelectedCell,

            /// <summary>
            /// BPMに合わせた拍を演奏するときに再生する。
            /// </summary>
            WoodBlockBeat,

            /// <summary>
            /// ノーツの判定が「Perfect」または「Great」のときに再生する。
            /// </summary>
            GreatOrPerfect,

            /// <summary>
            /// ノーツの判定が「Good」または「Bad」のときに再生する。
            /// </summary>
            GoodOrBad,

            /// <summary>
            /// レーンをタップしたときに再生する。
            /// </summary>
            TapedLane
        }

        protected override sealed void Awake()
        {
            base.Awake();
            seData = new();
        }
        
        public override void Construct(Reference.Scene.GameScenes scene) => SetVolume(Save.SettingData.Instance.setting.seVolume);

        /// <summary>
        /// 指定したSEを再生する。
        /// </summary>
        /// <param name="name">再生するSE</param>
        public void Play(SE name) => Source.PlayOneShot(seData?.NameToClip(name));
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using FRONTIER.Audio;
using FRONTIER.Menu;
using FRONTIER.Save;
using FRONTIER.Utility;
using FadeTransition;


namespace FRONTIER
{
    /// <summary>
    /// ゲームを総括するクラス。
    /// </summary>
    public class GameManager : SingletonMonoBehaviour<GameManager>
    {
        #region フィールド

        /// <summary>
        /// 楽曲やSEの再生を管理する。
        /// </summary>
        public AudioManagers audios;

        /// <summary>
        /// 音楽の再生が開始したか。
        /// </summary>
        public bool start = false;

        /// <summary>
        /// 音楽の再生が開始した時間を記録する。判定時間の算出に用いる。
        /// </summary>
        public float startTime;

        /// <summary>
        /// プレイする楽曲の情報。
        /// </summary>
        public PlayInfo info;

        /// <summary>
        /// スコアのデータ。
        /// </summary>
        public ScoreData score = new();

        /// <summary>
        /// <see cref="SceneNavigator"/>と連動してシーンのロード時に発火させるイベント。
        /// </summary>
        [Header("SceneNavigaterと連動してシーンのロード時に発火させるイベント")] public SceneEvent scene;

        /// <summary>
        /// 「ゲーム」のプレイ状況。
        /// </summary>
        public GamePlayState gamePlayState = GamePlayState.None;

        #endregion

        #region クラス・列挙型

        /// <summary>
        /// 楽曲やSEの再生を管理する。
        /// </summary>
        [Serializable]
        public class AudioManagers
        {
            /// <summary>
            /// <see cref = "MusicManager"/>
            /// </summary>
            public MusicManager musicManager;

            /// <summary>
            /// <see cref = "SEManager"/>
            /// </summary>
            public SEManager seManager;
        }

        /// <summary>
        /// プレイする楽曲の情報を記録する。
        /// </summary>
        public class PlayInfo : SongInfo
        {
            /// <summary>
            /// ノーツの速度。
            /// </summary>
            public float NoteSpeed => SettingData.Instance.setting.noteSpeed * Reference.NOTE_SPEED_FACTOR;

            /// <summary>
            /// 判定ずらしの秒数。
            /// </summary>
            public float JudgingTiming => SettingData.Instance.setting.timing;

            /// <summary>
            /// 楽曲のBPM。
            /// </summary>
            public int Bpm { get; set; }

            /// <summary>
            /// オートプレイするか。
            /// </summary>
            public bool IsAutoPlay => MenuInfo.menuInfo.IsAutoPlay;

            /// <summary>
            /// MVを再生するか。
            /// </summary>
            public bool IsMV => MenuInfo.menuInfo.IsMV;

            public override int ID => MenuInfo.menuInfo.ID;
            public override string Name => MenuInfo.menuInfo.Name;
            public override string Artist => MenuInfo.menuInfo.Artist;
            public override string Works => MenuInfo.menuInfo.Works;
            public override Reference.DifficultyRank Difficulty => MenuInfo.menuInfo.Difficulty;
            public override string Level => MenuInfo.menuInfo.Level;
            public override Sprite Cover => MenuInfo.menuInfo.Cover;

        }

        /// <summary>
        /// 各シーンのロードを行うイベント。
        /// </summary>
        [Serializable]
        public class SceneEvent
        {
            /// <summary>
            /// タイトルがロードされたとき
            /// </summary>
            public UnityEvent title;

            /// <summary>
            /// メニューがロードされたとき
            /// </summary>
            public UnityEvent menu;

            /// <summary>
            /// ゲームがロードされたとき
            /// </summary>
            public UnityEvent game;

            /// <summary>
            /// リザルトがロードされたとき
            /// </summary>
            public UnityEvent result;
        }

        /// <summary>
        /// スコア情報を保存するクラス。
        /// </summary>
        [Serializable]
        public class ScoreData
        {
            /// <summary>
            /// コンボ数
            /// </summary>
            public int combo;

            /// <summary>
            /// 最大コンボ数
            /// </summary>
            public int maxCombo;

            /// <summary>
            /// 表示するスコア
            /// </summary>
            public int ScoreValue { get; private set; }

            /// <summary>
            /// 楽曲の総てのノーツをPerfect判定で叩いたと仮定したときの最大スコア
            /// </summary>
            /// <remarks>
            /// 楽曲の総ノーツ数 × Perfect判定のスコア
            /// </remarks>
            public float maxScoreValue;

            /// <summary>
            /// 判定ステータスに応じて加算していく、見かけ上のスコア
            /// </summary>
            public float apparentScoreValue;

            /// <summary>
            /// 各判定ステータスを獲得した回数を記録する
            /// </summary>
            public Dictionary<Reference.JudgementStatus, int> judgementStatus;

            public Reference.ClearRank clearRank;

            /// <summary>
            /// スコアの理論値
            /// </summary>
            public const int THEORETICAL_SCORE_VALUE = 1000000;

            /// <summary>
            /// スコアを計算する
            /// </summary>
            public void CalculateScore()
            {
                if (maxScoreValue == 0) return;

                ScoreValue = Mathf.RoundToInt(THEORETICAL_SCORE_VALUE * Mathf.Floor(apparentScoreValue / maxScoreValue * THEORETICAL_SCORE_VALUE) / THEORETICAL_SCORE_VALUE);
            }

            public ScoreData()
            {
                combo = 0;
                ScoreValue = 0;
                maxScoreValue = 0;
                apparentScoreValue = 0;
                clearRank = Reference.ClearRank.C;
                judgementStatus = new()
                {
                    {Reference.JudgementStatus.Perfect, 0},
                    {Reference.JudgementStatus.Great, 0},
                    {Reference.JudgementStatus.Good, 0},
                    {Reference.JudgementStatus.Bad, 0},
                    {Reference.JudgementStatus.Miss, 0}
                };
            }
        }

        public enum GamePlayState
        {
            Starting, Playing, Pausing, Finishing, None
        }

        #endregion

        #region メソッド

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void AbsoluteInit()
        {
            Application.targetFrameRate = 60;
            Time.timeScale = 1;
        }

        protected override void Init()
        {
            base.Init();
            DontDestroyOnLoad(gameObject);
        }

        [Banzan.Lib.Utility.EnumAction(typeof(Reference.Scene.GameScenes))]
        public override void Construct(int scene)
        {
            Reference.Scene.GameScenes _scene = (Reference.Scene.GameScenes)scene;
            // シーンナビゲータ側のイベントを初期化する
            SceneNavigator.instance.ResetEvent();
            switch (_scene)
            {
                case Reference.Scene.GameScenes.Menu:
                    SceneNavigator.instance.FadeOutFinished += () =>
                    {
                        instance.info = new();
                        instance.gamePlayState = GamePlayState.None;
                        if (SongSaveData.Instance.saves == null) { SongSaveData.Instance.Save(); }
                    };
                    SceneNavigator.instance.FadeOutFinished += SettingData.Instance.Load;
                    SceneNavigator.instance.FadeOutFinished += SongSaveData.Instance.Load;
                    SceneNavigator.instance.FadeOutFinished += () => instance.info = new();
                    SceneNavigator.instance.FadeInFinished += () => Menu.Background.FFT.OnAudioClipChanged?.Invoke();
                    break;

                case Reference.Scene.GameScenes.Game:
                    SceneNavigator.instance.FadeOutFinished += () =>
                    {
                        instance.score = new();
                        instance.gamePlayState = GamePlayState.Starting;
                        instance.start = false;
                        instance.startTime = 0;
                    };
                    break;

                case Reference.Scene.GameScenes.Result:
                    SceneNavigator.instance.FadeOutFinished += () =>
                    {
                        instance.start = false;
                        instance.gamePlayState = GamePlayState.Finishing;
                    };
                    break;
            }
            SceneNavigator.instance.FadeOutFinished += () => instance.audios.musicManager.Construct(_scene);
            SceneNavigator.instance.FadeOutFinished += () => instance.audios.seManager.Construct(_scene);

            SceneNavigator.instance.ChangeScene(Reference.Scene.ToString(_scene), _fadeTime: 1f);
        }

        #endregion
    }
}
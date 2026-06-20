using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using FRONTIER.Audio;
using FRONTIER.Menu;
using FRONTIER.Save;
using FRONTIER.Utility;
using FRONTIER.Utility.SceneTransition;

namespace FRONTIER
{
    /// <summary>
    /// ゲームを全体を管理するクラス。
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
        [Header("SceneNavigatorと連動してシーンのロード時に発火させるイベント")] public SceneEvent scene;

        /// <summary>
        /// 「ゲーム」のプレイ状況。
        /// </summary>
        public GameState gamePlayState = GameState.None;

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
        public class PlayInfo : SelectedSongInfo
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
            /// 譜面のオフセット。
            /// </summary>
            public float Offset { get; set; }

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
            /// 持続した最大コンボ数
            /// </summary>
            public int maxCombo;

            /// <summary>
            /// 譜面の最大コンボ数
            /// </summary>
            public int maxComboCount;

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
            public Dictionary<Reference.JudgementRank, int> judgementStatus;

            public Reference.ClearRank clearRank;

            /// <summary>
            /// スコアを計算する
            /// </summary>
            public void CalculateScore()
            {
                // 最大持続コンボ数を更新
                maxCombo = combo > maxCombo ? combo : maxCombo;

                if (maxScoreValue == 0)
                {
                    return;
                }
                ScoreValue = Mathf.RoundToInt(Reference.THEORETICAL_SCORE_VALUE * Mathf.Floor(apparentScoreValue / maxScoreValue * Reference.THEORETICAL_SCORE_VALUE) / Reference.THEORETICAL_SCORE_VALUE);
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
                    {Reference.JudgementRank.Perfect, 0},
                    {Reference.JudgementRank.Great, 0},
                    {Reference.JudgementRank.Good, 0},
                    {Reference.JudgementRank.Bad, 0},
                    {Reference.JudgementRank.Miss, 0}
                };
            }
        }

        /// <summary>
        /// Game シーンでの詳細なプレイ状況を表す。
        /// </summary>
        public enum GameState
        {
            /// <summary>
            /// Game シーンではない
            /// </summary>
            None,

            /// <summary>
            /// Game シーンに入ってから、音楽の再生が開始するまでの状態
            /// </summary>
            Starting,

            /// <summary>
            /// 音楽が再生されて、プレイしている状態
            /// </summary>
            Playing,

            /// <summary>
            /// 一時停止している状態
            /// </summary>
            Pausing,
            
            /// <summary>
            /// 終了処理に入り、Result シーンに遷移するまでの状態
            /// </summary>
            Finishing
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

        // このクラスは Singleton であるため、この初期化処理はアプリ起動時に一度だけ呼び出される。

        [Banzan.Lib.Utility.EnumAction(typeof(Reference.Scene.GameScenes))]
        public override void Construct(int scene)
        {
            Reference.Scene.GameScenes _scene = (Reference.Scene.GameScenes)scene;
            // シーンナビゲータ側のイベントを初期化する
            SceneNavigator.Instance.ResetEvent();
            switch (_scene)
            {
                case Reference.Scene.GameScenes.Menu:
                {
                    SceneNavigator.Instance.FadeOutFinished += () =>
                    {
                        Instance.info = new();
                        Instance.gamePlayState = GameState.None;
                    };
                    SceneNavigator.Instance.FadeOutFinished += SettingData.Instance.Load;
                    SceneNavigator.Instance.FadeOutFinished += PlayData.Instance.Load;
                    SceneNavigator.Instance.FadeOutFinished += () => Instance.info = new();
                    break;
                }
                case Reference.Scene.GameScenes.Game:
                {
                    SceneNavigator.Instance.FadeOutFinished += () =>
                    {
                        Instance.score = new();
                        Instance.gamePlayState = GameState.Starting;
                        Instance.start = false;
                        Instance.startTime = 0;
                    };
                    break;
                }
                case Reference.Scene.GameScenes.Result:
                {
                    SceneNavigator.Instance.FadeOutFinished += () =>
                    {
                        Instance.start = false;
                    };
                    break;
                }
            }

            SceneNavigator.Instance.FadeOutFinished += () => Instance.audios.musicManager.Construct(_scene);
            SceneNavigator.Instance.FadeOutFinished += () => Instance.audios.seManager.Construct(_scene);

            SceneNavigator.Instance.ChangeScene(Reference.Scene.ToString(_scene), fadeTime: 1f);
        }

        #endregion
    }
}

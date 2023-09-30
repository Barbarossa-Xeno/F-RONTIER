using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using FRONTIER.Menu;
using FRONTIER.Save;
using FRONTIER.Utility;
using FRONTIER.Utility.Development;
using FadeTransition;


namespace FRONTIER
{
    /// <summary>
    /// ゲームを総括するクラス。
    /// </summary>
    public class GameManager : SingletonMonoBehaviour<GameManager>
    {
        /// <summary>
        /// ノーツの速度。
        /// </summary>
        public float NoteSpeed { get; private set; }

        /// <summary>
        /// 判定ずらしの秒数。
        /// </summary>
        public float JudgingTiming { get; private set; }

        /// <summary>
        /// オートプレイが選択されたか。
        /// </summary>
        public bool AutoPlay { get; private set; }

        /// <summary>
        /// MVを再生するか。
        /// </summary>
        public bool Mv { get; private set; }

        /// <summary>
        /// 楽曲やSEの再生を管理するクラスたち。
        /// </summary>
        public AudioManagers audioManagers;

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
        [HideInInspector] public Info info;

        /// <summary>
        /// <see cref="SceneNavigator"/>と連動してシーンのロード時に発火させるイベント。
        /// </summary>
        [Header("SceneNavigaterと連動してシーンのロード時に発火させるイベント")] public SceneLoad sceneLoad;

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
        public class Info : SongInfo
        {
            public override int ID => MenuInfo.menuInfo.ID;
            public override string Name => MenuInfo.menuInfo.Name;
            public override string Artist => MenuInfo.menuInfo.Artist;
            public override string Works => MenuInfo.menuInfo.Works;
            public override Reference.DifficultyRank Difficulty => MenuInfo.menuInfo.Difficulty;
            public override string Level => MenuInfo.menuInfo.Level;
            public override Sprite Cover => MenuInfo.menuInfo.Cover;
            public int Bpm { get; set; }
        }

        /// <summary>
        /// 各シーンがロードされた際に発火するイベントをまとめたクラス。
        /// </summary>
        [Serializable]
        public class SceneLoad
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
            /// 表示するスコア
            /// </summary>
            public int Score { get; private set; }

            /// <summary>
            /// 楽曲の総てのノーツをPerfect判定で叩いたと仮定したときの最大スコア
            /// </summary>
            /// <remarks>
            /// 楽曲の総ノーツ数 × Perfect判定のスコア
            /// </remarks>
            public float maxScore;

            /// <summary>
            /// 判定ステータスに応じて加算していく、見かけ上のスコア
            /// </summary>
            public float apparentScore;

            /// <summary>
            /// スコアの理論値
            /// </summary>
            public const int THEORETICAL_SCORE = 1000000;

            public Dictionary<Reference.JudgementStatus, int> scoreCount;

            public void CalculateScore()
            {
                if (maxScore == 0) return;

                Score = Mathf.RoundToInt(THEORETICAL_SCORE * Mathf.Floor(apparentScore / maxScore * THEORETICAL_SCORE) / THEORETICAL_SCORE);
            }

            public ScoreData()
            {
                combo = 0;
                Score = 0;
                maxScore = 0;
                apparentScore = 0;
                scoreCount = new()
                {
                    {Reference.JudgementStatus.Perfect, 0},
                    {Reference.JudgementStatus.Great, 0},
                    {Reference.JudgementStatus.Good, 0},
                    {Reference.JudgementStatus.Bad, 0},
                    {Reference.JudgementStatus.Miss, 0}
                };
            }
        }

        public ScoreData scoreData = new();

        public Reference.Scene.GameScenes GameScene
        {
            get
            {
                if (SceneManager.GetActiveScene().buildIndex == 0) { return Reference.Scene.GameScenes.Title; }
                if (SceneManager.GetActiveScene().buildIndex == 1) { return Reference.Scene.GameScenes.Menu; }
                if (SceneManager.GetActiveScene().buildIndex == 2) { return Reference.Scene.GameScenes.Game; }
                if (SceneManager.GetActiveScene().buildIndex == 3) { return Reference.Scene.GameScenes.Result; }
                else { return 0; }
            }
        }

        [HideInInspector]
        public enum GamePlayState
        {
            Starting, Playing, Pausing, Finishing
        }

        public GamePlayState gamePlayState = GamePlayState.Starting;


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
            // シーンナビゲータ側のイベントを初期化する。
            SceneNavigator.instance.ResetEvent();
            switch (_scene)
            {
                case Reference.Scene.GameScenes.Menu:
                    SceneNavigator.instance.FadeOutFinished += () => instance.info = new();
                    SceneNavigator.instance.FadeOutFinished += () => instance.audioManagers.musicManager.Construct(_scene);
                    SceneNavigator.instance.FadeInFinished += () => Menu.Background.FFT.OnAudioClipChanged?.Invoke();
                    break;

                case Reference.Scene.GameScenes.Game:
                    SceneNavigator.instance.FadeOutFinished += InitializeFieldProperty;
                    SceneNavigator.instance.FadeOutFinished += () => instance.audioManagers.musicManager.Construct(_scene);
                    SceneNavigator.instance.FadeOutFinished += instance.audioManagers.musicManager.Construct;
                    break;

                case Reference.Scene.GameScenes.Result:
                    SceneNavigator.instance.FadeOutFinished += () =>
                    {
                        instance.start = false;
                        instance.gamePlayState = GamePlayState.Finishing;
                    };
                    break;

            }

            SceneNavigator.instance.ChangeScene(Reference.Scene.ToString(_scene), _fadeTime: 1f);
        }

        /// <summary>
        /// ゲームシーンがロードされたときに、フィールドやプロパティを初期化する。
        /// </summary>
        private void InitializeFieldProperty()
        {
            // プロパティ
            instance.NoteSpeed = SettingData.Instance.setting.noteSpeed * Reference.NOTE_SPEED_FACTOR;
            instance.JudgingTiming = SettingData.Instance.setting.timing;
            instance.AutoPlay = MenuInfo.menuInfo.autoPlay;
            instance.Mv = MenuInfo.menuInfo.mv;
            // フィールド
            instance.scoreData = new();
            instance.gamePlayState = GamePlayState.Starting;
            instance.start = false;
            instance.startTime = 0;
        }
    }
}
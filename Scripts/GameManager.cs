using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using FRONTIER.Game;
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
        /// 音楽を再生するオーディオソース。
        /// </summary>
        public AudioSource musicSource = null;

        /// <summary>
        /// 効果音を再生するオーディオソース。
        /// </summary>
        public AudioSource seSource = null;

        /// <summary>
        /// <see cref = "MusicManager"/>
        /// </summary>
        public MusicManager musicManager;

        /// <summary>
        /// <see cref = "SEManager"/>
        /// </summary>
        public SEManager seManager;

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
        /// プレイする楽曲の情報を記録する。
        /// </summary>
        public class Info : SongInfo
        {
            public override int ID => MenuInfo.menuInfo.ID;
            public override string Name => MenuInfo.menuInfo.Name;
            public override string Artist => MenuInfo.menuInfo.Artist;
            public override string Works => MenuInfo.menuInfo.Works;
            public override Reference.DifficultyEnum Difficulty => MenuInfo.menuInfo.Difficulty;
            public override string Level => MenuInfo.menuInfo.Level;
            public override Sprite Cover => MenuInfo.menuInfo.Cover;
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
        public class ScoreManager
        {
            public int combo;
            public int score;
            public float maxScore;
            public float ratioScore;
            public const int THEORETICALVALUE = 1000000;
            public Dictionary<string, int> scoreCount;
            public ScoreManager()
            {
                combo = 0;
                score = 0;
                maxScore = 0;
                ratioScore = 0;
                scoreCount = new Dictionary<string, int>()
            {
                {"perfect", 0}, {"great", 0}, {"good", 0}, {"bad", 0}, {"miss", 0}
            };
            }
        }

        public ScoreManager scoreManager = new();

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
            Idling, Starting, Playing, Pausing, Finishing, Inactiving
        }

        public GamePlayState gamePlayState = GamePlayState.Idling;


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
            AudioManagerInitialize();
        }

        void Update()
        {
            if (instance.start)
            {
                if (instance.gamePlayState == GamePlayState.Starting)
                {
                    instance.gamePlayState = GamePlayState.Playing;
                }
            }
            if (!instance.start && instance.gamePlayState == GameManager.GamePlayState.Finishing)
            {
                instance.gamePlayState = GamePlayState.Inactiving;
                SceneNavigator.instance.ResetEvent();
                SceneNavigator.instance.ChangeScene("Result", _fadeTime: 1.5f);
            }
            if (instance.gamePlayState == GamePlayState.Inactiving) { instance.start = false; }
        }

        [Banzan.Lib.Utility.EnumAction(typeof(Reference.Scene.GameScenes))]
        public override void OnSceneLoaded(int scene)
        {
            Reference.Scene.GameScenes _scene = (Reference.Scene.GameScenes)scene;
            // シーンナビゲータ側のイベントを初期化する。
            SceneNavigator.instance.ResetEvent();
            switch (_scene)
            {
                case Reference.Scene.GameScenes.Menu:
                    SceneNavigator.instance.FadeOutFinished += () => instance.info = new();
                    break;

                case Reference.Scene.GameScenes.Game:
                    SceneNavigator.instance.FadeOutFinished += InitializeFieldProperty;
                    SceneNavigator.instance.FadeOutFinished += () => instance.musicManager.OnSceneLoaded(_scene);
                    SceneNavigator.instance.FadeOutFinished += instance.musicManager.OnSceneLoaded;                /*
                instance.musicSource.Stop();
                instance.musicSource.loop = false;
                instance.seSource.Stop();
                instance.musicManager.OnSceneLoaded();*/
                    break;
            }

            SceneNavigator.instance.ChangeScene(Reference.Scene.ToString(_scene));
        }

        /// <summary>
        /// MusicManagerとSEManagerをシングルトンにする。
        /// </summary>
        private void AudioManagerInitialize()
        {
            if (musicSource == null)
            {
                GameObject ml = instance.transform.Find("MusicManager").gameObject;
                if (ml == null)
                {
                    ml = new GameObject("MusicManager");
                    ml.transform.SetParent(instance.gameObject.transform);
                    ml.AddComponent<AudioSource>();
                }
                musicSource = ml.GetComponent<AudioSource>();
                musicManager = ml.AddComponent<MusicManager>();
            }
            if (seSource == null)
            {
                GameObject sl = instance.transform.Find("SEManager").gameObject;
                if (sl == null)
                {
                    sl = new GameObject("SEManager");
                    sl.transform.SetParent(instance.gameObject.transform);
                    sl.AddComponent<AudioSource>();
                }
                seSource = sl.GetComponent<AudioSource>();
                seManager = sl.AddComponent<SEManager>();
            }
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
            instance.scoreManager = new ScoreManager();
            instance.gamePlayState = GamePlayState.Idling;
        }


        public void ScoreCalc()
        {
            scoreManager.score = Mathf.RoundToInt(ScoreManager.THEORETICALVALUE * Mathf.Floor(scoreManager.ratioScore / scoreManager.maxScore * ScoreManager.THEORETICALVALUE) / ScoreManager.THEORETICALVALUE);
        }
    }
}
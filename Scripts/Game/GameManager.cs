using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Game;
using Game.Utility;
using Game.Development;
using FadeTransition;

/// <summary>
/// ゲームを総括するクラス。
/// </summary>
public class GameManager : SingletonMonoBehaviour<GameManager>
{
    /// <summary>
    /// 読み込む楽曲のID。
    /// </summary>
    public int songID;
    /// <summary>
    /// 読み込む楽曲の名前。
    /// </summary>
    public string songName;
    /// <summary>
    /// 楽曲の難易度。
    /// </summary>
    public string difficulty;
    /// <summary>
    /// 難易度の番号。
    /// </summary>
    public int difficultyNumber;
    /// <summary>
    /// ノーツの速度。
    /// </summary>
    [SerializeField] public float noteSpeed;
    /// <summary>
    /// 判定ずらしの秒数。
    /// </summary>
    [SerializeField] public float judgingTiming;
    /// <summary>
    /// オートプレイが選択されたか。
    /// </summary>
    [SerializeField] public bool autoPlay;
    /// <summary>
    /// MVを再生するか。
    /// </summary>
    [SerializeField] public bool mv;
    /// <summary>
    /// 音楽を再生するオーディオソース。
    /// </summary>
    [SerializeField] public AudioSource musicSource = null;
    /// <summary>
    /// 効果音を再生するオーディオソース。
    /// </summary>
    [SerializeField] public AudioSource seSource = null;
    /// <summary>
    /// <see cref = "MusicManager"/>
    /// </summary>
    [SerializeField] public MusicManager musicManager;
    /// <summary>
    /// <see cref = "SEManager"/>
    /// </summary>
    [SerializeField] public SEManager seManager;
    /// <summary>
    /// 音楽の再生が開始したか。
    /// </summary>
    public bool start = false;
    /// <summary>
    /// 音楽の再生が開始した時間を記録する。判定時間の算出に用いる。
    /// </summary>
    public float startTime;
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
    public ScoreManager scoreManager = new ScoreManager();
    public SettingUtility.GameScenes gameScene
    {
        get
        {
            if (SceneManager.GetActiveScene().buildIndex == 0) { return SettingUtility.GameScenes.Title; }
            if (SceneManager.GetActiveScene().buildIndex == 1) { return SettingUtility.GameScenes.Menu; }
            if (SceneManager.GetActiveScene().buildIndex == 2) { return SettingUtility.GameScenes.Game; }
            if (SceneManager.GetActiveScene().buildIndex == 3) { return SettingUtility.GameScenes.Result; }
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
        AudioListenerInitialize();
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
            SceneNavigator.instance.ResetAction(1);
            SceneNavigator.instance.SceneChange("Result", _fadeTime: 1.5f);
        }
        if (instance.gamePlayState == GamePlayState.Inactiving) { instance.start = false; }
    }

    public override void OnSceneLoaded(SettingUtility.GameScenes scene)
    {
        switch (scene)
        {
            case SettingUtility.GameScenes.Game:
                instance.songID = MenuInfo.menuInfo.selectedSongID;
                instance.difficulty = MenuInfo.menuInfo.selectedDifficulty;
                instance.difficultyNumber = MenuInfo.menuInfo.selectedDifficultyNumber;
                instance.noteSpeed = Game.Save.Setting.setting.Save[0].noteSpeed * SettingUtility.NOTE_SPEED_FACTOR;
                instance.judgingTiming = Game.Save.Setting.setting.Save[0].timing;
                instance.autoPlay = MenuInfo.menuInfo.autoPlay;
                instance.mv = MenuInfo.menuInfo.mv;
                instance.scoreManager = new ScoreManager();
                instance.gamePlayState = GamePlayState.Idling;
                instance.musicSource.Stop();
                instance.musicSource.loop = false;
                instance.seSource.Stop();
                instance.musicManager.OnSceneLoaded(scene);
                break;
        }

    }

    /// <summary>
    /// MusicManagerとSEManagerをシングルトンにします。
    /// </summary>
    private void AudioListenerInitialize()
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

    public void ScoreCalc()
    {
        scoreManager.score = Mathf.RoundToInt(ScoreManager.THEORETICALVALUE * Mathf.Floor(scoreManager.ratioScore / scoreManager.maxScore * ScoreManager.THEORETICALVALUE) / ScoreManager.THEORETICALVALUE);
    }
}

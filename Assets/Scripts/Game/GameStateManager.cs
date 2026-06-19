using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using FRONTIER.Audio;
using FRONTIER.Utility;
using FRONTIER.Utility.SceneTransition;

namespace FRONTIER.Game
{
    public class GameStateManager : GameUtilityBase
    {
        #region フィールド

        /// <summary>
        /// プレイを開始する前に表示する画面。
        /// </summary>
        [SerializeField] private IntroductionBundle introductionBundle;

        /// <summary>
        /// ポーズ画面。
        /// </summary>
        [SerializeField] private PauseBundle pauseBundle;

        /// <summary>
        /// MV管理 (<see cref="MVManager"/> )
        /// </summary>
        [SerializeField] private MVManager mvManager;

        /// <summary>
        /// ゲームがポーズ中か。
        /// </summary>
        private bool isPaused = false;

        #endregion

        #region 構造体

        /// <summary>
        /// プレイを開始する直前に表示する画面の要素を集約したクラス。
        /// 基本全てがCanvasに表示するUI要素。非アクティブ化をAnimatorで制御する。
        /// </summary>
        [Serializable]
        private class IntroductionBundle
        {
            /// <summary>
            /// イントロアニメーションを制御するAnimator。
            /// </summary>
            [SerializeField] private Animator controlAnimator;

            /// <summary>
            /// 表示する背景のImage。
            /// </summary>
            [SerializeField] private Image background;

            /// <summary>
            /// ジャケット画像。
            /// </summary>
            [SerializeField] private Image cover;

            /// <summary>
            /// 曲名表示テキスト。
            /// </summary>
            [SerializeField] private TextMeshProUGUI name;

            /// <summary>
            /// 難易度表示テキスト。
            /// </summary>
            [SerializeField] private TextMeshProUGUI difficulty;

            /// <summary>
            /// レベル表示テキスト。
            /// </summary>
            [SerializeField] private TextMeshProUGUI level;

            /// <summary>
            /// カバー画像の背景。難易度に応じて色を変える。
            /// ヒエラルキー上ではカバー画像の親にあたるので、カバーサイズの基準となる。
            /// </summary>
            [SerializeField] private Image coverArea;

            /// <summary>
            /// レベル表示部分の背景。難易度に応じて色を変える。
            /// ヒエラルキー上ではレベルテキストの親にあたる。
            /// </summary>
            [SerializeField] private Image levelArea;

            /// <summary>
            /// イントロ画面を表示する。
            /// </summary>
            public void Open() => background.gameObject.SetActive(true);

            /// <summary>
            /// イントロ画面を終了する。
            /// </summary>
            public void Close() => controlAnimator.SetTrigger("Inactive");

            /// <summary>
            /// <see cref="background"/> のブラーマテリアルを設定する。
            /// </summary>
            public void SetBackground()
            {
                background.material.SetColor("_Color", new Color32(90, 90, 90, 255));
                background.material.SetFloat("_Blur", 70f);
            }

            /// <summary>
            /// プレイする曲の情報を取得して設定する。
            /// </summary>
            public void SetSongInfo()
            {
                cover.sprite = Manager.info.Cover;
                name.text = Manager.info.Name;
                difficulty.text = Manager.info.FromDifficulty(Manager.info.Difficulty).Item1;
                level.text = Manager.info.Level;
                coverArea.color = Manager.info.FromDifficulty(Manager.info.Difficulty).Item2;
                levelArea.color = coverArea.color;
            }
        }

        /// <summary>
        /// ポーズ機能に関わる要素を集約したクラス。
        /// Canvasに表示するUI要素。非アクティブ化をAnimatorで制御する。
        /// </summary>
        [Serializable]
        private class PauseBundle
        {
            /// <summary>
            /// ポーズ画面のパネル。有効化されると画面全体に表示される。
            /// </summary>
            [SerializeField] private GameObject panel;

            /// <summary>
            /// ポーズ画面を有効化するボタン。プレイ時に表示される。
            /// </summary>
            [SerializeField] private Button pauseButton;

            /// <summary>
            /// ポーズ画面を閉じるボタン。ポーズ画面に表示される。
            /// </summary>
            [SerializeField] private Button retireButton;

            /// <summary>
            /// ポーズ画面でゲームを続行するボタン。ポーズ画面に表示される。
            /// </summary>
            [SerializeField] private Button continueButton;

            /// <summary>
            /// ポーズ画面でゲームをリトライするボタン。ポーズ画面に表示される。
            /// </summary>
            [SerializeField] private Button retryButton;

            /// <summary>
            /// ゲームを続行する前のカウントダウンを表示するテキスト。ポーズ画面を抜けた直後、プレイ時に表示される。
            /// </summary>
            [SerializeField] private TextMeshProUGUI countDownText;

            /// <summary>
            /// ポーズ画面に表示するプレイ情報。曲名、難易度、レベル、スコアなどを表示する。
            /// </summary>
            [SerializeField] private PlayInfo playInfo;

            /// <summary>
            /// ポーズ画面を開く。
            /// </summary>
            public void Open()
            {
                playInfo.cover.sprite = Manager.info.Cover;
                playInfo.name.Text = Manager.info.Name;
                playInfo.level.text = Manager.info.Level;
                playInfo.score.text = $"{Manager.score.ScoreValue}";
                playInfo.bpm.text = $"{Manager.info.Bpm}";

                panel.SetActive(true);
            }

            /// <summary>
            /// ポーズ画面を閉じる。
            /// </summary>
            public void Close() => panel.SetActive(false);

            public void AddListenerToButtons(Action onPause, Action onContinue, Action onRetry, Action onRetire)
            {
                pauseButton.onClick.AddListener(onPause.Invoke);
                continueButton.onClick.AddListener(onContinue.Invoke);
                retryButton.onClick.AddListener(onRetry.Invoke);
                retireButton.onClick.AddListener(onRetire.Invoke);
            }

            /// <summary>
            /// ゲームをもう一度続行する前のカウントダウンをする。
            /// </summary>
            /// <param name="OnAfterCountDown">カウントダウンが終わった後に実行する処理</param>
            /// <returns></returns>
            public IEnumerator CountDown(Action OnAfterCountDown)
            {
                // １秒後に数字を減らす
                for (int i = 3; i > 0; i--)
                {
                    countDownText.GetComponent<Animator>().SetTrigger("Start");
                    countDownText.text = i.ToString();
                    yield return new WaitForSecondsRealtime(1f);
                    countDownText.text = "";
                }

                OnAfterCountDown?.Invoke();

                // コルーチンを破棄する
                yield break;
            }

            [Serializable]
            private class PlayInfo
            {
                public Image cover;
                public Utility.Asset.OverflowTextScroll name;
                public TextMeshProUGUI level;
                public TextMeshProUGUI score;
                public TextMeshProUGUI bpm;
            }
        }

        #endregion

        #region MonoBehaviourメソッド
        void Start()
        {
            // ポーズ画面をセットアップ
            pauseBundle.Close();
            pauseBundle.AddListenerToButtons(PauseGame, ContinueGame, RetryGame, RetireGame);

            // イントロ画面をセットアップ
            introductionBundle.Open();
            StartCoroutine(StartGame());

            // MVがある場合はセットアップ
            if (Manager.info.IsMV)
            {
                mvManager.Construct();
            }
            // MVが無い場合は mvManager.Player は null になるので、
            // その後のステート処理で null チェックが通らなければ自動的に再生しないことになる。
        }

        void Update()
        {
            // 音楽の再生が最後まで終わったら
            if (Manager.start
                && Manager.gamePlayState == GameManager.GameState.Playing
                && Time.time > Manager.audios.musicManager.Clip.length + Manager.startTime)
            {
                // 終了状態に移行
                Manager.gamePlayState = GameManager.GameState.Finishing;

                // リザルトシーンをロード。
                Manager.audios.musicManager.Stop();
                Manager.scene.result.Invoke();
            }
        }

        #endregion

        #region メソッド

        /// <summary>
        /// ゲームを開始する。
        /// </summary>
        /// <returns></returns>
        private IEnumerator StartGame()
        {
            // イントロの画面をセットアップ
            introductionBundle.SetBackground();
            introductionBundle.SetSongInfo();

            // 曲のBPMに合わせて拍のSEを４回鳴らす
            SetInterval(() => Manager.audios.seManager.Play(SEManager.SE.WoodBlockBeat), 60f / Manager.info.Bpm, 4);

            // 5秒待機して曲の情報画面を消す
            yield return new WaitForSeconds(5f);
            introductionBundle.Close();

            // 4秒待機してゲームを開始する
            yield return new WaitForSeconds(4f);
            Manager.start = true;
            Manager.startTime = Time.time;
            Manager.audios.musicManager.Play();

            // MVがある場合は再生する（null の場合は再生しない）
            mvManager.Player?.Play();
            Manager.gamePlayState = GameManager.GameState.Playing;
        }

        /// <summary>
        /// ゲームをポーズする。
        /// </summary>
        private void PauseGame()
        {
            if (!isPaused)
            {
                isPaused = true;
                Time.timeScale = 0;
                Manager.audios.musicManager.Pause();
                mvManager.Player?.Pause();
                pauseBundle.Open();
                Manager.gamePlayState = GameManager.GameState.Pausing;
            }
        }

        /// <summary>
        /// ゲームをやめる。
        /// </summary>
        private void RetireGame()
        {
            Manager.scene.menu.Invoke();
            Time.timeScale = 1f;
        }

        /// <summary>
        /// ゲームを続行する。
        /// </summary>
        private void ContinueGame()
        {
            pauseBundle.Close();

            StartCoroutine(pauseBundle.CountDown(() => 
                {
                    isPaused = false;
                    Time.timeScale = 1;
                    Manager.audios.musicManager.Source.Play();
                    mvManager.Player?.Play();
                    Manager.gamePlayState = GameManager.GameState.Playing;
                }
            ));
        }

        /// <summary>
        /// ゲームをリトライする。
        /// </summary>
        private void RetryGame()
        {
            Time.timeScale = 1;
            SceneNavigator.Instance.ChangeScene(SceneManager.GetActiveScene().name, 1.0f, isIgnoreTimeScale: true);
        }

        #endregion
    }
}

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;
using FadeTransition;
using FRONTIER.Audio;
using FRONTIER.Utility;
using System;

namespace FRONTIER.Game
{
    public class GameMenuManager : GameUtility
    {
        #region フィールド

        /// <summary>
        /// プレイを開始する前に表示する画面。
        /// </summary>
        [SerializeField] private IntroductionScreen introductionScreen;

        /// <summary>
        /// ポーズ画面。
        /// </summary>
        [SerializeField] private PauseMenu pauseMenu;

        /// <summary>
        /// <see cref="MVManager"/> 
        /// </summary>
        [SerializeField] private MVManager mvManager;

        /// <summary>
        /// ゲームがポーズ中か。
        /// </summary>
        private bool isPaused = false;

        #endregion

        #region 構造体

        /// <summary>
        /// プレイを開始する前に表示する画面の要素と挙動。
        /// </summary>
        [Serializable]
        private struct IntroductionScreen
        {
            #region フィールド

            /// <summary>
            /// イントロアニメーションのアニメーター。
            /// </summary>
            [SerializeField] private Animator animator;

            /// <summary>
            /// 背景を表示する。
            /// </summary>
            [SerializeField] private Image background;

            /// <summary>
            /// "BlurScreenWithColor" マテリアル。
            /// </summary>
            [Header("BlurScreenWithColorマテリアルをアタッチ"), SerializeField] private Material blurScreenWithColor;

            /// <summary>
            /// ジャケット画像表示。
            /// </summary>
            [SerializeField] private Image cover;

            /// <summary>
            /// 曲名表示。
            /// </summary>
            [SerializeField] private TextMeshProUGUI name;

            /// <summary>
            /// 難易度表示。
            /// </summary>
            [SerializeField] private TextMeshProUGUI difficulty;

            /// <summary>
            /// レベル表示。
            /// </summary>
            [SerializeField] private TextMeshProUGUI level;

            /// <summary>
            /// カバー画像の縁。
            /// </summary>
            [SerializeField] private Image coverBorder;

            /// <summary>
            /// レベル表示部分の背景。
            /// </summary>
            [SerializeField] private Image levelBorder;

            #endregion

            #region メソッド

            /// <summary>
            /// イントロ画面を表示する。
            /// </summary>
            public void Open() => background.gameObject.SetActive(true);

            /// <summary>
            /// イントロ画面を終了する。
            /// </summary>
            public void Close() => animator.SetTrigger("Inactive");

            /// <summary>
            /// <see cref="background"/> のブラーマテリアルを設定する。
            /// </summary>
            public void SetBackground()
            {
                background.gameObject.SetActive(true);
                Material material = new(blurScreenWithColor.shader);
                material.SetColor("_Color", new Color32(90, 90, 90, 255));
                material.SetFloat("_Blur", 70f);
                background.material = material;
            }

            /// <summary>
            /// プレイする曲の情報を取得して設定する。
            /// </summary>
            public void SetSongInfo()
            {
                cover.sprite = Manager.info.Cover;
                name.text = Manager.info.Name;
                difficulty.text = Manager.info.DifficultyTo(Manager.info.Difficulty).Item1;
                level.text = Manager.info.Level;
                coverBorder.color = Manager.info.DifficultyTo(Manager.info.Difficulty).Item2;
                levelBorder.color = coverBorder.color;
            }

            #endregion
        }

        /// <summary>
        /// ポーズ画面の要素と挙動。
        /// </summary>
        [Serializable]
        private struct PauseMenu
        {
            #region フィールド

            [SerializeField] private GameObject window;
            public Button pauseButton;
            public Button retireButton;
            public Button continueButton;
            public Button retryButton;
            [SerializeField] private TextMeshProUGUI countDownText;
            [SerializeField] private PlayInfo playInfo;

            #endregion

            #region メソッド

            /// <summary>
            /// ポーズ画面を開く。
            /// </summary>
            public void Open()
            {
                playInfo.UpdateInfo();
                window.SetActive(true);
            }

            /// <summary>
            /// ポーズ画面を閉じる。
            /// </summary>
            public void Close() => window.SetActive(false);

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

            #endregion

            #region 子構造体

            [Serializable]
            public struct PlayInfo
            {
                public Image cover;
                public Menu.OverflowTextScroll name;
                public TextMeshProUGUI level;
                public TextMeshProUGUI score;
                public TextMeshProUGUI bpm;

                /// <summary>
                /// 表示するプレイ情報を更新する。
                /// </summary>
                public void UpdateInfo()
                {
                    cover.sprite = Manager.info.Cover;
                    name.Text = Manager.info.Name;
                    level.text = Manager.info.Level;
                    score.text = $"{Manager.score.ScoreValue}";
                    bpm.text = $"{Manager.info.Bpm}";
                }
            }

            #endregion
        }

        #endregion

        #region MonoBehaviourメソッド
        void Start()
        {
            // ポーズ画面をセットアップ
            pauseMenu.Close();
            pauseMenu.pauseButton.onClick.AddListener(PauseGame);
            pauseMenu.continueButton.onClick.AddListener(ContinueGame);
            pauseMenu.retryButton.onClick.AddListener(RetryGame);
            pauseMenu.retireButton.onClick.AddListener(RetireGame);

            // イントロ画面をセットアップ
            introductionScreen.Open();
            StartCoroutine(StartGame());

            if (Manager.info.IsMV) { mvManager.Construct(); }
        }

        void Update()
        {
            // 音楽の再生が最後まで終わったら
            if (Manager.start
                && Manager.gamePlayState == GameManager.GamePlayState.Playing
                && Time.time > Manager.audios.musicManager.Source.clip.length + Manager.startTime)
            {
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
            introductionScreen.SetBackground();
            introductionScreen.SetSongInfo();

            // 曲のBPMに合わせて拍のSEを４回鳴らす
            SetInterval(() => Manager.audios.seManager.Play(SEManager.SE.WoodBlockBeat), 60f / Manager.info.Bpm, 4);

            // 5秒待機して曲の情報画面を消す
            yield return new WaitForSeconds(5f);
            introductionScreen.Close();

            // 4秒待機してゲームを開始する
            yield return new WaitForSeconds(4f);
            Manager.start = true;
            Manager.startTime = Time.time;
            Manager.audios.musicManager.Play();

            if (Manager.info.IsMV) { mvManager.Player.Play(); }

            Manager.gamePlayState = GameManager.GamePlayState.Playing;
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
                pauseMenu.Open();
                Manager.gamePlayState = GameManager.GamePlayState.Pausing;
            }
        }

        /// <summary>
        /// ゲームをリタイアする。
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
            pauseMenu.Close();
            StartCoroutine(pauseMenu.CountDown(
                () => 
                {
                    isPaused = false;
                    Time.timeScale = 1;
                    Manager.audios.musicManager.Source.Play();
                    if (Manager.info.IsMV) { mvManager.Player.Play(); }
                    Manager.gamePlayState = GameManager.GamePlayState.Playing;
                }
            ));
        }

        /// <summary>
        /// ゲームをリトライする。
        /// </summary>
        private void RetryGame()
        {
            Time.timeScale = 1;
            SceneNavigator.instance.ChangeScene(SceneManager.GetActiveScene().name, 1.0f, ignoreTimeScale: true);
        }

        #endregion
    }
}
//  SceneNavigator.cs
//  [Reference] http://kan-kikuchi.hatenablog.com/entry/SceneNavigator

using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace FRONTIER.Utility.SceneTransition
{
    /// <summary>シーン遷移の実行と総括的な管理・カスタイマイズを行うことができます。</summary>
    public class SceneNavigator : SingletonMonoBehaviour<SceneNavigator>
    {
        /* フィールド */
        /// <summary>
        /// 使用する<see cref = "CanvasFader"/>。
        /// </summary>
        [SerializeField] private CanvasFader canvasFader = null;

        /// <summary>
        /// フェードにかける時間。
        /// </summary>
        public const float FADE_TIME = 0.5f;

        private float fadeTime = FADE_TIME;
        /// <summary>
        /// フェード中か否かを示すフラグ。
        /// </summary>
        /// <returns>
        /// <see cref = "CanvasFader.IsFading"/>の<c>bool</c>値か、
        /// <see cref = "CanvasFader.Alpha"/>が0でないかどうか。
        /// </returns>
        public bool IsFading => canvasFader.IsFading || canvasFader.Alpha != 0;        

        public string CurrentSceneName { get; private set; } = "";

        public string BeforeSceneName { get; private set; } = "";

        public string NextSceneName { get; private set; } = "";

        /// <summary>
        /// フェードアウト後のイベント。
        /// </summary>
        public event Action FadeOutFinished = delegate { };

        /// <summary>
        /// フェードイン後のイベント。
        /// </summary>
        public event Action FadeInFinished = delegate { };

        /* メソッド */
        /// <summary>初期化します。</summary>
        /// <remarks>Awake時か、それ以前の初アクセス時の、どちらか一度しか実行されません。</remarks>
        protected override void Init()
        {
            base.Init();
            if (canvasFader == null)
            {
                Reset();
            }
            //*
            CurrentSceneName = SceneManager.GetActiveScene().name;

            DontDestroyOnLoad(gameObject);
            canvasFader.gameObject.SetActive(false);
        }
        /// <summary>コンポーネント追加時に自動で実行されるリセットメソッドです。</summary>
        /// <remarks>実機上やエディタ上では動作しません。</remarks>
        private void Reset()
        {
            gameObject.name = "SceneNavigator";

            // フェード用キャンバスのオブジェクトを作成
            GameObject fadeCanvas = new("FadeCanvas");
            fadeCanvas.transform.SetParent(transform);
            fadeCanvas.SetActive(false);

            // それに Canvas に必要なコンポーネント群をアタッチ、値の設定
            Canvas canvas = fadeCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;
            fadeCanvas.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            fadeCanvas.AddComponent<GraphicRaycaster>();
            canvasFader = fadeCanvas.AddComponent<CanvasFader>();
            canvasFader.Alpha = 0;

            // フェード用の Image 作成
            GameObject imageObject = new("Image");
            imageObject.transform.SetParent(fadeCanvas.transform, false);
            imageObject.AddComponent<Image>().color = Color.black;
            imageObject.GetComponent<RectTransform>().sizeDelta = new(2000, 2000);
        }

        /// <summary>シーン遷移を行います。</summary>
        public void ChangeScene(string sceneName, float fadeTime = FADE_TIME, bool isIgnoreTimeScale = false)
        {
            if(Instance.IsFading)
            {
                Debug.LogWarning("フェード中です！");
                return;
            }

            NextSceneName = sceneName;
            this.fadeTime = fadeTime;

            // フェードアウト処理
            canvasFader.gameObject.SetActive(true);
            canvasFader.Play(isFadeOut: false, duration: this.fadeTime, ignoreTimeScale: isIgnoreTimeScale, onFinished: OnFadeOutFinish);
        }

        private void OnFadeOutFinish()
        {
            FadeOutFinished?.Invoke();

            SceneManager.LoadScene(NextSceneName);
            
            BeforeSceneName = CurrentSceneName;
            CurrentSceneName = NextSceneName;

            // フェードイン処理
            canvasFader.gameObject.SetActive(true);
            canvasFader.Alpha = 1;
            canvasFader.Play(isFadeOut: true, duration: fadeTime, ignoreTimeScale: true, onFinished: OnFadeInFinish);

        }

        private void OnFadeInFinish()
        {
            canvasFader.gameObject.SetActive(false);
            FadeInFinished?.Invoke();
        }

        /// <summary>
        /// <see cref="FadeInFinished"/> と <see cref="FadeOutFinished"/> のイベントハンドラを初期化する。
        /// </summary>
        public void ResetEvent()
        {
            FadeInFinished = null;
            FadeOutFinished = null;
        }
    }
}

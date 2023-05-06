//  SceneNavigator.cs
//  [Reference] http://kan-kikuchi.hatenablog.com/entry/SceneNavigator

using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Game.Utility;

namespace FadeTransition
{
    ///<summary>シーン遷移の実行と総括的な管理・カスタイマイズを行うことができます。</summary>
    public class SceneNavigator : SingletonMonoBehaviour<SceneNavigator>
    {
        /* フィールド */
        ///<summary>使用する<see cref = "CanvasFader"/>。</summary>
        [SerializeField] private CanvasFader canvasFader = null;
        ///<summary>フェードにかける時間。</summary>
        public const float FADETIME = 0.5f;
        private float fadeTime = FADETIME;
        ///<summary>フェード中か否かを示すフラグ。</summary>
        ///<returns>
        ///<see cref = "CanvasFader.isFading"/>の真偽か、
        ///<see cref = "CanvasFader.alpha"/>が0でない条件の真偽のどちらかがtrueになればtrueを返します。
        ///</returns>
        public bool isFading
        {
            get
            {
                return canvasFader.isFading || canvasFader.alpha != 0;
            }
        }
        private string _currentSceneName = "";
        public string currentSceneName { get { return _currentSceneName; } }
        private string _beforeSceneName = "";
        public string beforeSceneName { get { return _beforeSceneName; } }
        private string _nextSceneName = "";
        public string nextSceneName { get { return _nextSceneName; } }
        ///<summary>フェードアウト後のイベント。</summary>
        public event Action FadeOutFinished = delegate { };
        ///<summary>フェードイン後のイベント。</summary>
        public event Action FadeInFinished = delegate { };

        /* メソッド */
        ///<summary>初期化します。</summary>
        ///<remarks>Awake時か、それ以前の初アクセス時の、どちらか一度しか実行されません。</remarks>
        protected override void Init()
        {
            base.Init();
            if (canvasFader == null) { Reset(); }
            //*
            _currentSceneName = SceneManager.GetActiveScene().name;

            DontDestroyOnLoad(gameObject);
            canvasFader.gameObject.SetActive(false);
        }
        ///<summary>コンポーネント追加時に自動で実行されるリセットメソッドです。</summary>
        ///<remarks>実機上やエディタ上では動作しません。</remarks>
        private void Reset()
        {
            gameObject.name = "SceneNavigator";

            //フェード用キャンバスを作成します。
            GameObject fadeCanvas = new GameObject("FadeCanvas");
            fadeCanvas.transform.SetParent(transform);
            fadeCanvas.SetActive(false);

            Canvas canvas = fadeCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;

            fadeCanvas.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            fadeCanvas.AddComponent<GraphicRaycaster>();
            canvasFader = fadeCanvas.AddComponent<CanvasFader>();
            canvasFader.alpha = 0;

            //フェード用の画像作成
            GameObject imageObject = new GameObject("Image");
            imageObject.transform.SetParent(fadeCanvas.transform, false);
            imageObject.AddComponent<Image>().color = Color.black;
            imageObject.GetComponent<RectTransform>().sizeDelta = new Vector2(2000, 2000);
        }
        ///<summary>シーン遷移を行います。</summary>
        public void SceneChange(string sceneName, float _fadeTime = FADETIME, bool ignoreTimeScale = false)
        {
            if(instance.isFading)
            {
                Debug.LogError("フェード中です！");
                return;
            }

            _nextSceneName = sceneName;
            fadeTime = _fadeTime;

            //フェードアウト処理
            canvasFader.gameObject.SetActive(true);
            canvasFader.Play(_isFadeOut: false, _duration: fadeTime, _ignoreTimeScale: ignoreTimeScale, _onFinished: OnFadeOutFinish);
        }

        private void OnFadeOutFinish()
        {
            FadeOutFinished();

            SceneManager.LoadScene(_nextSceneName);
            
            _beforeSceneName = _currentSceneName;
            _currentSceneName = _nextSceneName;

            //フェードイン処理
            canvasFader.gameObject.SetActive(true);
            canvasFader.alpha = 1;
            canvasFader.Play(_isFadeOut: true, _duration: fadeTime, _ignoreTimeScale: true, _onFinished: OnFadeInFinish);

        }

        private void OnFadeInFinish()
        {
            canvasFader.gameObject.SetActive(false);
            FadeInFinished();
        }

        public void ResetAction(int mode)
        {
            switch (mode)
            {
                case 0:
                    FadeInFinished = delegate { };
                    break;
                case 1: 
                    FadeOutFinished = delegate { };
                    break;
            }
        }
    }
}
//  CanvasFader.cs
//  [Reference] http://kan-kikuchi.hatenablog.com/entry/CanvasFader

using System;
using UnityEngine;

namespace FRONTIER.Utility.SceneTransition
{
    [RequireComponent(typeof(CanvasGroup))]

    /// <summary>キャンバスをフェードするクラスです。</summary>
    public class CanvasFader : MonoBehaviour
    {
        #region フィールド

        /// <summary>フェードにかける時間。</summary>
        [SerializeField] private float duration;

        /// <summary>タイムスケールを無視してフェードさせるかを示すフラグ。</summary>
        [SerializeField] private bool ignoreTimeScale = false;

        /// <summary>プロパティ調整用のフェードさせるキャンバス。</summary>
        private CanvasGroup canvasGroupEntity;

        /// <summary>フェード終了時のコールバック。</summary>
        private Action onFadeFinished = null;

        /// <summary>フェードの状態。</summary>
        private enum FadeState
        {
            None, FadeIn, FadeOut
        }

        private FadeState fadeState = FadeState.None;

        /// <summary>実際に使う方のフェードさせるキャンバス。</summary>
        private CanvasGroup MainCanvasGroup
        {
            get
            {
                if (canvasGroupEntity == null)
                {
                    canvasGroupEntity = GetComponent<CanvasGroup>();

                    if (canvasGroupEntity == null)
                    {
                        canvasGroupEntity = gameObject.AddComponent<CanvasGroup>();
                    }
                }

                return canvasGroupEntity;
            }
        }

        /// <summary>フェード中か否かを示すフラグ。</summary>
        public bool IsFading => fadeState != FadeState.None;

        /// <summary>フェードにかける時間。</summary>
        public float Duration => duration;
        /// <summary>キャンバスのアルファ値。</summary>
        public float Alpha
        {
            get => MainCanvasGroup.alpha;
            set => MainCanvasGroup.alpha = value;
        }

        #endregion

        /* メソッド */
        ///<summary>更新処理</summary>
        void Update()
        {
            if (!IsFading)
            {
                return;
            }
            // フェードの進行
            float fadeSpeed = 1f / duration;
            fadeSpeed *= ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
            Alpha += fadeSpeed * (fadeState == FadeState.FadeIn ? 1f : -1f);
            // Debug.Log($"{Alpha}, {fadeSpeed}, {Time.unscaledDeltaTime}, {Time.deltaTime}");

            // フェード終了判定
            // 0 < Alpha < 1 ならフェード中なので戻る
            if (Alpha > 0 && Alpha < 1)
            {
                return;
            }
            fadeState = FadeState.None;
            enabled = false;
            onFadeFinished?.Invoke();
        }

        /// <summary>必要ならばキャンバスを追加した上でフェードを開始させるメソッドです。</summary>
        /// <returns>新しい <see cref = "CanvasFader"/> コンポーネント</returns>
        public static CanvasFader Begin(GameObject target, bool isFadeOut, float duration, bool ignoreTimeScale = false, Action onFinished = null)
        {
            CanvasFader canvasFader = target.GetComponent<CanvasFader>();

            if (canvasFader == null)
            {
                canvasFader = target.AddComponent<CanvasFader>();
            }
            canvasFader.enabled = true;
            canvasFader.Play(isFadeOut, duration, ignoreTimeScale, onFinished);

            return canvasFader;
        }
        
        /// <summary>フェードを開始させるメソッドです。</summary>
        /// <remarks>静的ではないためスクリプトを再利用したい場合や、インスペクターで設定した<see cref = "CanvasFader"/>を使いたい場合に有効です。</remarks>
        public void Play(bool isFadeOut, float duration, bool ignoreTimeScale = false, Action onFinished = null)
        {
            enabled = true;
            Alpha = isFadeOut ? 1 : 0;
            fadeState = isFadeOut ? FadeState.FadeOut : FadeState.FadeIn;
            this.duration = duration;
            this.ignoreTimeScale = ignoreTimeScale;
            onFadeFinished = onFinished;
        }

        /// <summary>フェードを停止します。</summary>
        public void Stop()
        {
            fadeState = FadeState.None;
            enabled = true;
        }
    }
}

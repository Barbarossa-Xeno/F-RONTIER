//  CanvasFader.cs
//  [Reference] http://kan-kikuchi.hatenablog.com/entry/CanvasFader

using System;
using UnityEngine;

namespace FadeTransition
{
    //ゲームオブジェクトにアタッチ
    [RequireComponent(typeof(CanvasGroup))]

    ///<summary>キャンバスをフェードするクラスです。</>
    public class CanvasFader : MonoBehaviour
    {
        /* フィールド */
        ///<summary>プロパティ調整用のフェードさせるキャンバス。</summary>
        private CanvasGroup canvasGroupEntity;
        ///<summary>実際に使う方のフェードさせるキャンバス。</summary>
        private CanvasGroup canvasGroup
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
        ///<summary>フェードの状態。</summary>
        private enum FadeState
        {
            None, FadeIn, FadeOut
        }
        ///<summary>フェード中か否かを示すフラグ。</summary>
        public bool isFading
        {
            get { return fadeState != FadeState.None; }
        }
        private FadeState fadeState = FadeState.None;
        ///<summary>フェードにかける時間。</summary>
        [SerializeField] private float duration;
        ///<summary>フェードにかける時間。</summary>
        public float Duration { get { return duration; } }
        ///<summary>タイムスケールを無視してフェードさせるかを示すフラグ。</summary>
        [SerializeField] private bool ignoreTimeScale = false;
        ///<summary>フェード終了時のコールバック。</summary>
        private Action onFadeFinished = null;
        ///<summary>キャンバスのアルファ値。</summary>
        public float alpha
        {
            get { return canvasGroup.alpha; }
            set { canvasGroup.alpha = value; }
        }

        /* メソッド */
        ///<summary>更新処理</summary>
        void Update()
        {
            if (!isFading) { return; }

            float fadeSpeed = 1f / duration;
            fadeSpeed *= ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
            alpha += fadeSpeed * (fadeState == FadeState.FadeIn ? 1f : -1f);
            //Debug.Log($"{alpha}, {fadeSpeed}, {Time.unscaledDeltaTime}, {Time.deltaTime}");
            //フェード終了判定
            if (alpha > 0 && alpha < 1) { return; }
            fadeState = FadeState.None;
            this.enabled = false;
            if (onFadeFinished != null) { onFadeFinished(); }
        }
        ///<summary>必要ならばキャンバスを追加した上でフェードを開始させるメソッドです。</summary>
        ///<returns>新しいコンポーネント、<see cref = "CanvasFader"/></returns>
        public static CanvasFader Begin(GameObject target, bool _isFadeOut, float _duration, bool _ignoreTimeScale = false, Action _onFinished = null)
        {
            CanvasFader canvasFader = target.GetComponent<CanvasFader>();
            if (canvasFader == null)
            {
                canvasFader = target.AddComponent<CanvasFader>();
            }
            canvasFader.enabled = true;

            canvasFader.Play(_isFadeOut, _duration, _ignoreTimeScale, _onFinished);

            return canvasFader;
        }
        ///<summary>フェードを開始させるメソッドです。</summary>
        ///<remarks>静的ではないためスクリプトを再利用したい場合や、インスペクターで設定した<see cref = "CanvasFader"/>を使いたい場合に有効です。</remarks>
        public void Play(bool _isFadeOut, float _duration, bool _ignoreTimeScale = false, Action _onFinished = null)
        {
            this.enabled = true;
            alpha = _isFadeOut ? 1 : 0;
            fadeState = _isFadeOut ? FadeState.FadeOut : FadeState.FadeIn;
            duration = _duration;
            ignoreTimeScale = _ignoreTimeScale;
            onFadeFinished = _onFinished;
        }
        ///<summary>フェードを停止します。</summary>
        public void Stop()
        {
            fadeState = FadeState.None;
            this.enabled = true;
        }
    }
}

using System;
using System.Collections;
using UniRx;

namespace FRONTIER.Result
{
    /// <summary>
    /// リザルト画面に表示する各ウィンドウの要素や機能を持った抽象クラス。
    /// </summary>
    [Serializable]
    public abstract class ResultWindowElements
    {
        #region フィールド

        /// <summary>
        /// 現在サブスクライブしている処理。
        /// </summary>
        protected IDisposable currentSubscribing;

        /// <summary>
        /// アニメーションさせる時間。
        /// </summary>
        protected const float ANIMATION_DURATION = 2.5f;

        #endregion

        #region メソッド

        /// <summary>
        /// 初期化をする。
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// カウントアップを開始する。
        /// </summary>
        /// <param name="enumerator"><c>IEnumerator</c>で実装したコルーチンの処理</param>
        protected void PlayCountUp(Func<IEnumerator> enumerator) => currentSubscribing = Observable.FromCoroutine(enumerator).Subscribe();

        #endregion
    }

}

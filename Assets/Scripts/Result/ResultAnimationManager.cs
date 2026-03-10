using System;
using System.Linq;
using UnityEngine;
using UniRx;
using UniRx.Triggers;


namespace FRONTIER.Result
{
    /// <summary>
    /// リザルト画面のアニメーションを管理する。
    /// </summary>
    public class ResultAnimationManager : MonoBehaviour
    {
        #region フィールド

        /// <summary>
        /// 制御するアニメーター。
        /// </summary>
        [SerializeField] private Animator animator;

        /// <summary>
        /// スコアを表示するときに発火するイベント。
        /// </summary>
        public event Action OnPlayScore;

        /// <summary>
        /// コンボを表示するときに発火するイベント。
        /// </summary>
        public event Action OnPlayCombo;

        /// <summary>
        /// ハイスコアとの差を表示するときに発火するイベント。
        /// </summary>
        public event Action OnPlayDifference;

        #endregion

        #region クラス

        /// <summary>
        /// リザルト画面全体のアニメーションを制御する。
        /// </summary>
        public static class Controller
        {
            private static readonly int _Activate = Animator.StringToHash("Activate");
            private static readonly int _IsGotNewRecord = Animator.StringToHash("IsGotNewRecord");
            private static readonly int _IsGotFullCombo = Animator.StringToHash("IsGotFullCombo");
            private static readonly int _IsAutoPlay = Animator.StringToHash("IsAutoPlay");

            /// <summary>
            /// 制御するアニメーター。
            /// </summary>
            public static Animator animator;

            /// <summary>
            /// アニメーションを開始する。
            /// </summary>
            public static void Activate() => animator.SetTrigger(_Activate);

            /// <summary>
            /// 新記録を獲得した際のアニメーションを開始する。
            /// </summary>
            /// <param name="value"></param>
            public static void IsGotNewRecord(bool value) => animator.SetBool(_IsGotNewRecord, value);

            /// <summary>
            /// フルコンボまたはオールパーフェクトを獲得した際のアニメーションを開始する。
            /// </summary>
            /// <param name="value"></param>
            public static void IsGotFullCombo(bool value) => animator.SetBool(_IsGotFullCombo, value);

            /// <summary>
            /// オートプレイかどうか。
            /// </summary>
            /// <param name="value"></param>
            public static void IsAutoPlay(bool value) => animator.SetBool(_IsAutoPlay, value);

            /// <summary>
            /// 再生中のアニメーションをスキップする。
            /// </summary>
            public static void Skip() => animator.Play(animator.GetCurrentAnimatorStateInfo(0).fullPathHash, 0, 1);

            /// <summary>
            /// リザルト画面のアニメーションが一通り終わった時に処理を実行する。
            /// </summary>
            /// <param name="component">
            /// ゲームオブジェクトまたはそれにアタッチされたコンポーネント<br/>
            /// ※購読を取り消す際には何らかのゲームオブジェクトのデストロイが必要
            /// </param>
            /// <param name="actions">実行したい処理</param>
            public static void OnAnimatorFinished(Component component, params Action[] actions)
            {
                animator.GetBehaviour<ObservableStateMachineTrigger>()
                    .OnStateEnterAsObservable()
                    .Where(anime => anime.StateInfo.IsName("End"))
                    .Subscribe(x => actions?.ToList()?.ForEach(action => action.Invoke()))
                    .AddTo(component);
            }
        }

        #endregion

        #region メソッド

        // 制御するアニメーターを渡す
        void Awake() => Controller.animator = animator;

        // 以下のメソッドはアニメーションクリップのイベントに登録する
        private void PlayScore() => OnPlayScore?.Invoke();

        private void PlayCombo() => OnPlayCombo?.Invoke();

        private void PlayDifference() => OnPlayDifference?.Invoke();

        #endregion
    }
}

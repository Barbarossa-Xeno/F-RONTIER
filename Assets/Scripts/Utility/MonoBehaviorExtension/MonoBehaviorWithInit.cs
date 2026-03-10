using UnityEngine;

namespace FRONTIER.Utility
{
    /// <summary>初期化メソッドを実装したMonoBehaviour。</summary>
    /// <remarks>継承元：<see cref = "GameUtility"/></remarks>
    public class MonoBehaviorWithInit : GameUtility
    {
        /// <summary>初期化したか否かのフラグ。</summary>
        /// <remarks>※初期化は一度しか実行されないようにする設計。</remarks>
        private bool isInitialized = false;

        /// <summary>必要ならば初期化を行うメソッド。</summary>
        public void IfInit()
        {
            if (isInitialized) { return; }
            Init();
            isInitialized = true;
        }

        /// <summary>初期化メソッド。</summary>
        protected virtual void Init() { }

        /// <summary><c>Awake()</c> 実行タイミングで実行されるメソッド。</summary>
        /// <remarks>※継承先で <c>sealed override</c> として実装する。</remarks>
        protected virtual void Awake() { }
    }
}

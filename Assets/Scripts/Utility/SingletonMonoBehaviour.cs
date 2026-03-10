//  SingletonMonoBehaviour.cs
//  [Reference] http://kan-kikuchi.hatenablog.com/entry/SingletonMonoBehaviour

using UnityEngine;

namespace FRONTIER.Utility
{
    /// <summary>
    /// シングルトンなMonoBehaviorを実装するクラス。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SingletonMonoBehaviour<T> : MonoBehaviorWithInit where T : MonoBehaviorWithInit
    {
        /* フィールド */
        ///<summary>このクラスのインスタンス。</summary>
        private static T _instance;
        ///<summary>外部から参照するインスタンス。</summary>
        ///<remarks>ゲッターを使って予め初期化したインスタンスを保持します。</remarks>
        public static T instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = (T)FindObjectOfType(typeof(T));
                    if (_instance == null)
                    {
                        Debug.LogError($"{typeof(T)}は定義されていないか、無効な型です。");
                    }
                    else
                    {
                        _instance.IfInit();
                    }
                }
                return _instance;
            }
        }

        /* メソッド */
        protected sealed override void Awake()
        {
            if (this == instance) { return; }
            Debug.LogError($"{typeof(T)}は重複して存在しています。");
        }
    }

    //* 初期化 *//

    ///<summary>初期化メソッドを実装したMonoBehaviour。</summary>
    ///<remarks>継承元：<see cref = "GameUtility"/></remarks>
    public class MonoBehaviorWithInit : GameUtility
    {
        ///<summary>初期化したか否かのフラグ。</summary>
        ///<remarks>※初期化は一度しか実行されないようにする設計。</remarks>
        private bool isInitialized = false;
        ///<summary>必要ならば初期化を行うメソッド。</summary>
        public void IfInit()
        {
            if (isInitialized) { return; }
            Init();
            isInitialized = true;
        }
        ///<summary>初期化メソッド。</summary>
        protected virtual void Init() { }
        ///<summary>Awake実行タイミングで実行されるメソッド。</summary>
        ///<remarks>※継承先で [sealed override] として実装する。</remarks>
        protected virtual void Awake() { }
    }
}

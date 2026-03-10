//  SingletonMonoBehaviour.cs
//  [Reference] http://kan-kikuchi.hatenablog.com/entry/SingletonMonoBehaviour

using UnityEngine;

namespace FRONTIER.Utility
{
    /// <summary>
    /// シングルトンなMonoBehaviorを実装するクラス。
    /// </summary>
    /// <typeparam name="T"><see cref="MonoBehaviorWithInit"/></typeparam>
    /// <remarks>MonoBehavior 拡張</remarks> /// 
    public class SingletonMonoBehaviour<T> : MonoBehaviorWithInit where T : MonoBehaviorWithInit
    {
        /// <summary>このクラスのインスタンス。</summary>
        private static T _instance;

        /// <summary>外部から参照するインスタンス。</summary>
        /// <remarks>ゲッターを使って予め初期化したインスタンスを保持します。</remarks>
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<T>();
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

        protected sealed override void Awake()
        {
            if (this == Instance) return;
            Debug.LogError($"{typeof(T)}は重複して存在しています。");
        }
    }
}

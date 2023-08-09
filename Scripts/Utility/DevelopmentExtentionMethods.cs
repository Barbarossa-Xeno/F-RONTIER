using UnityEngine;
using System.Collections;

namespace Game.Development
{
    ///<summary>エディタ上のデバッグをカスタマイズしたクラスです。</summary>
    public static class DevelopmentExtentionMethods
    {
        ///<summary>Unityエディター上でしかログを吐かないメソッド。</summary>
        public static void Log(object message)
        {
            #if UNITY_EDITOR
            Debug.Log(message);
            #endif
        }
        /// <summary>
        /// Unityエディター上でしかログを吐かないメソッド。
        /// </summary>
        /// <remarks>"message: value"の形で表示する</remarks>
        /// <param name="value"></param>
        /// <param name="message">メッセージを吐く時のフォーマット</param>
        public static void LogValue(this object value, string message = "")
        {
            #if UNITY_EDITOR
            Debug.Log(message + ": " + $"{value}");
            #endif
        }
        public static void LogValue<Collections>(this Collections values, string message = "") where Collections : ICollection, IEnumerable, IList
        {
            #if UNITY_EDITOR
            for (int i = 0; i < values.Count; i++) { values[i].LogValue(message + $"[{i}]"); }
            #endif
        }
        public static void Pause()
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPaused = true;
            #endif
        }
        public static void Pause(object message)
        {
            Log(message);
            Pause();
        }
        ///<summary>ゲームオブジェクトのマテリアルをデフォルトに変更します。</summary>
        public static void ResetMaterial(this GameObject target)
        {
            if(target == null) { return; }
            target.GetComponent<Renderer>().material = default;
        }
    }
}

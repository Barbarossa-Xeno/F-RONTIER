using UnityEngine;

namespace Game.Development
{
    ///<summary>エディタ上のデバッグをカスタマイズしたクラスです。</summary>
    public static class EditorCustom
    {
        ///<summary>Unityエディター上でしかログを吐かないメソッド。</summary>
        public static void Log(object message)
        {
            #if UNITY_EDITOR
            Debug.Log(message);
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
    }
    public static class DebugCustom
    {
        ///<summary>ゲームオブジェクトのマテリアルをデフォルトに変更します。</summary>
        public static void ResetMaterial(this GameObject target)
        {
            if(target == null) { return; }
            target.GetComponent<Renderer>().material = default;
        }
    }
}

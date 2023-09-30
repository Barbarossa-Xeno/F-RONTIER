using UnityEngine;
using System;
using System.Collections;

namespace FRONTIER.Utility
{
    ///<summary>ゲーム内の処理で頻繁に使用するメソッドをまとめた抽象クラス。</summary>
    public abstract class UtilityClass : MonoBehaviour
    {
        
        protected static GameManager Manager => GameManager.instance;

        ///<summary>シーンがロードされた時などに実行するクラスの初期化処理。</summary>
        ///<param name = "scene">現在のシーン。</param>
        public virtual void Construct() { }

        ///<summary>シーンがロードされた時に実行するクラスの初期化処理。</summary>
        ///<param name = "scene">現在のシーン。</param>
        [Banzan.Lib.Utility.EnumAction(typeof(Reference.Scene.GameScenes))]
        public virtual void Construct(int scene) { }

        ///<summary>シーンがロードされた時に実行するクラスの初期化処理。</summary>
        ///<param name = "scene">現在のシーン。</param>
        public virtual void Construct(Reference.Scene.GameScenes scene) { }

        ///<summary>シーンがアンロードされた時に実行する処理。</summary>
        ///<param name = "scene">現在のシーン。</param>
        public virtual void Destruct() { }

        ///<summary>シーンがアンロードされた時に実行する処理。</summary>
        ///<param name = "scene">現在のシーン。</param>
        public virtual void Destruct(Reference.Scene.GameScenes scene) { }
        
        ///<summary>レーン番号に応じてノーツのX座標を設定します。</summary>
        ///<param name = "laneIndex">レーン番号。</param>
        ///<param name = "useSplitLane">レーン数を細分するか。</param>
        protected virtual float SwitchNoteLane(int laneIndex, bool useSplitLane = false)
        {
            float x = 0;
            if (!useSplitLane)
            {
                switch (laneIndex)
                {
                    case 0:
                        x = -5f;
                        break;
                    case 1:
                        x = -3f;
                        break;
                    case 2:
                        x = -1f;
                        break;
                    case 3:
                        x = 1f;
                        break;
                    case 4:
                        x = 3f;
                        break;
                    case 5:
                        x = 5f;
                        break;
                }
            }
            return x;
        }

        /// <summary>
        /// 一定時間毎に処理を繰り返したい関数を指定する。
        /// </summary>
        /// <param name="callback">処理を繰り返したい関数</param>
        /// <param name="time">繰り返す秒間隔</param>
        protected IEnumerator SetFunction(Action callback, float time)
        {
            while (true)
            {
                callback();
                yield return new WaitForSeconds(time);   
            }
        }

        /// <summary>
        /// 一定時間ごとに一定回数処理を繰り返したい関数を指定する。
        /// </summary>
        /// <param name="callback">処理を繰り返したい関数<</param>
        /// <param name="time">繰り返す秒間隔</param>
        /// <param name="count">繰り返す回数</param>
        /// <returns></returns>
        protected IEnumerator SetFunction(Action callback, float time, int count)
        {
            for (int i = 0; i < count; i++)
            {
                callback();
                yield return new WaitForSeconds(time);
                continue;
            }
        }

        protected IEnumerator Wait(float time)
        {
            yield return new WaitForSeconds(time);
        }

        /// <summary>
        /// 一定時間毎に関数処理を繰り返すコルーチンを実行する。
        /// </summary>
        /// <remarks>Startメソッドに処理を書く。</remarks>
        /// <param name="callback">処理を繰り返したい関数。Action型デリゲートを使うか、ラムダ式を使って指定する。</param>
        /// <param name="time">繰り返す秒間隔。</param>
        /// <returns>このコルーチンのインスタンス</returns>
        protected Coroutine SetInterval(Action callback, float time) => StartCoroutine(SetFunction(callback, time));

        /// <summary>
        /// 一定時間毎に関数処理を繰り返すコルーチンを実行する。
        /// </summary>
        /// <remarks>Startメソッドに処理を書く。</remarks>
        /// <param name="callback">処理を繰り返したい関数</param>
        /// <param name="time">繰り返す秒間隔</param>
        /// <param name="count">繰り返す回数</param>
        /// <returns>このコルーチンのインスタンス</returns>
        protected Coroutine SetInterval(Action callback, float time, int count) => StartCoroutine(SetFunction(callback, time, count));
    }
}
using UnityEngine;
using System;
using System.Collections;

namespace Game.Utility
{
    ///<summary>ゲーム内の処理で頻繁に使用するメソッドをまとめた抽象クラス。</summary>
    public abstract class UtilityBase : MonoBehaviour
    {
        ///<summary>シーンがロードされた時に実行する処理。</summary>
        ///<remarks>デリゲートのコールバックに追加すると便利です。</remarks>
        ///<param name = "scene">現在のシーン。</param>
        public virtual void OnSceneLoaded(SettingUtility.GameScenes scene) { }
        ///<summary>シーンがアンロードされた時に実行する処理。</summary>
        ///<remarks>デリゲートのコールバックに追加すると便利です。</remarks>
        ///<param name = "scene">現在のシーン。</param>
        public virtual void OnSceneUnLoaded(SettingUtility.GameScenes scene) { }
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
        /// <param name="callback">処理を繰り返したい関数。Action型デリゲートを使うか、ラムダ式を使って指定する。</param>
        /// <param name="time">繰り返す秒間隔。</param>
        protected IEnumerator SetFunction(Action callback, float time)
        {
            while (true)
            {
                yield return new WaitForSeconds(time);
                callback();
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
        protected Coroutine SetInterval(Action callback, float time)
        {
            return StartCoroutine(SetFunction(callback, time));
        }

        /// <summary>
        /// 1フレーム毎に関数処理を繰り返すコルーチンを実行する。
        /// </summary>
        /// <remarks>Startメソッドに処理を書く。</remarks>
        /// <param name="callback">処理を繰り返したい関数。Action型デリゲートを使うか、ラムダ式を使って指定する。</param>
        /// <returns>このコルーチンのインスタンス</returns>
        protected Coroutine SetInterval(Action callback)
        {
            IEnumerator SetFunction()
            {
                while (true)
                {
                    yield return null;
                    callback();
                }
            }
            return StartCoroutine(SetFunction());
        }
    }
}
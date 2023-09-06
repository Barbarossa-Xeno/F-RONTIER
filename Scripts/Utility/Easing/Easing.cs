using System;
using UnityEngine;

/*
 * - Reference -
 * イージング関数チートシート (https://easings.net)
 */

namespace Game.Utility.Easing
{
    /// <summary>
    /// イージング関数をまとめたクラス。
    /// </summary>
    public static class EasingExtensions
    {
        /// <summary>
        /// 入力された値に対してInOutSineなイージングを行う。
        /// </summary>
        /// <param name="x">入力値。</param>
        /// <param name="t">周期。大きいほどイージングの変化が遅くなる。</param>
        /// <returns>計算後の値を返す</returns>
        public static float EaseInOutSine(this float x, float t = 1f) => (Mathf.Sin((Mathf.PI * x - (Mathf.PI / 2)) / t) + 1) / 2;

        /// <summary>
        /// 入力された値に対してInOutCircなイージングを行う。
        /// </summary>
        /// <param name="x">入力値</param>
        /// <returns>計算後の値を返す</returns>
        public static float EaseInOutCirc(this float x)
        {
            return x < 0.5
                ? (1 - Mathf.Sqrt(1 - (float)Math.Pow(2 * x, 2))) / 2
                : (Mathf.Sqrt(1 - (float)Math.Pow(-2 * x + 2, 2)) + 1) / 2;
        }

        /// <summary>
        /// 入力された値に対してOutQuintなイージングを行う。
        /// </summary>
        /// <param name="x">入力値</param>
        /// <returns>計算後の値を返す</returns>
        public static float EaseOutQuint(this float x) => 1 - Mathf.Pow(1 - x, 5);
    
    }
}
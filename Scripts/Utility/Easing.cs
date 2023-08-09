using UnityEngine;

namespace Game.Utility.Easing
{
    public static class EasingExtensions
    {
        /// <summary>
        /// 入力された値に対してInOutSineなイージングを行う。
        /// </summary>
        /// <param name="x">入力値。</param>
        /// <param name="t">周期。大きいほどイージングの変化が遅くなる。</param>
        /// <returns></returns>
        public static float EaseInOutSine(this float x, float t = 1f)
        {
            return (Mathf.Sin((Mathf.PI * x - (Mathf.PI / 2)) / t) + 1) / 2;
        }
    }
}
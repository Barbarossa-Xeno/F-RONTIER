using UnityEngine;
using System;
using System.Collections;

namespace Game.Utility
{
    /// <summary>
    /// うまく使えそうなメソッド集
    /// </summary>
    public static class UtilityMethod
    {
        /// <summary>
        /// LineRendererの点に座標を設定する際その頂点数を予め書き換えたうえで設定する。
        /// </summary>
        /// <param name="lineRenderer">対象のLineRenderer。</param>
        /// <param name="positions">設定する座標。</param>
        public static void SetPositionsWithCount(this LineRenderer lineRenderer, Vector3[] positions)
        {
            lineRenderer.positionCount = positions.Length;
            lineRenderer.SetPositions(positions);
        }

        /// <summary>
        /// テンポラリー変数と元の変数を比較して値の更新の必要性の確認をしたいときにつかう。
        /// </summary>
        /// <typeparam name="T">比較したい変数のデータ型。</typeparam>
        /// <param name="temp">テンポラリー（一時保存用の）変数。</param>
        /// <param name="original">元（普段使いしている）の変数。</param>
        /// <returns><c>temp</c>と<c>original</c>が等しいかどうか。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static bool IsValueChanged<T>(ref T temp, in T original)
        {
            // tempがnullだった場合tempを強制上書き
            if (temp == null) 
            {
                temp = original;
                return true;
            }
            // tempとoriginalが異なっていたら、tempを更新してtrueを返す
            if (!temp.Equals(original))
            {
                temp = original;
                return true;
            }
            // 同じだったらfalseを返す
            else { return false; }
        }
    }
}
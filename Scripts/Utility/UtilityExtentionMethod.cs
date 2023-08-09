using UnityEngine;
using System;
using System.Collections;

namespace Game.Utility.ExtentionMethod
{
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
    }
}
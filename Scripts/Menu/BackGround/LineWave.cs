using UnityEngine;
using FRONTIER.Utility;
using FRONTIER.Utility.Easing;

namespace FRONTIER.Menu.Background
{/// <summary>
 /// ラインレンダラーを使って波状のビジュアライザーをつくる。
 /// </summary>
    public class LineWave : UtilityClass
    {
        [SerializeField] private Wave wave;

        /// <summary>
        /// LineRendererで描画するオーディオスペクトラム。
        /// </summary>
        public LineRenderer line;

        /// <summary>
        /// 振幅
        /// </summary>
        public float amplitude;

        /// <summary>
        /// 周期
        /// </summary>
        public float period;

        public float lineWidth;

        [SerializeField] private FPSLimit.TargetFPS targetFPS;

        [SerializeField] private Material material = default;

        private class FPSLimit
        {
            public static float FPSToSecond(TargetFPS fps)
            {
                switch (fps)
                {
                    case TargetFPS._30:
                        return 0.0333f;
                    case TargetFPS._40:
                        return 0.0248f;
                    case TargetFPS._45:
                        return 0.0222f;
                    case TargetFPS._50:
                        return 0.0199f;
                    case TargetFPS._60:
                        return 0.0166f;
                    default: break;
                }
                return 0f;
            }

            public enum TargetFPS
            {
                _30, _40, _45, _50, _60
            }
        }

        void Awake()
        {
            line = GetComponent<LineRenderer>();
            line.loop = false;
            line.useWorldSpace = false;
            line.startWidth = lineWidth;
            line.endWidth = lineWidth;
            line.material = material;
            line.startColor = Color.black;
            line.endColor = Color.black;
        }

        void Start()
        {
            // コルーチンでラインレンダラーを使ったスペクトラムの更新を一定秒間隔にすることで、処理落ちを防ぎ、波の高さの急激な変化を控えさせる
            // 0.028s(28ms)がだいたい40fpsくらい
            SetInterval(() => SetLineWave(line, wave.GenerateSpectrumBezierCurve(wave.activeWaveVertices, 100)), FPSLimit.FPSToSecond(targetFPS));
        }

        /// <summary>
        /// ベジェ曲線化したスペクトルをラインレンダラーに適用して波形のように動かす。
        /// </summary>
        /// <param name="line">動かしたいラインレンダラー。</param>
        /// <param name="curveVertices">ラインレンダラーに適用させる点、曲線の点。</param>
        public void SetLineWave(LineRenderer line, Vector3[] curveVertices)
        {
            // 点たちの真ん中のX座標を抽出して、それを基準にラインを中央ぞろえにする。
            float pivotX = curveVertices[curveVertices.Length / 2 - 1].x;
            for (int i = 0; i < curveVertices.Length; i++)
            {
                curveVertices[i].x -= pivotX;
                //イージング関数を用いて、激しい動きを抑制する。
                curveVertices[i].y = curveVertices[i].y.EaseInOutSine(period);
            }
            line.positionCount = curveVertices.Length;
            line.SetPositionsWithCount(curveVertices);
        }
    }
}

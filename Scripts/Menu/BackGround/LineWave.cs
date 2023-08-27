using UnityEngine;
using Game.Utility;
using Game.Utility.Easing;

namespace Game.Menu.Background
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

        [SerializeField] private Material material = default;

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
            SetInterval(() => SetLineWave(line, wave.GenerateSpectrumBezierCurve(wave.activeWaveVertices, 100)), 0.028f);
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

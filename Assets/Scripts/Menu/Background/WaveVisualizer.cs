using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using FRONTIER.Utility;
using FRONTIER.Utility.Easing;

namespace FRONTIER.Menu.Background
{
    /// <summary>
    /// LineRendererを使って波状のオーディオビジュアライザを生成するクラス。
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class WaveVisualizer : AudioVisualizer
    {
        /// <summary>
        /// LineRenderer の頂点の間を補間するときの分割数。
        /// 大きいほど頂点分布が滑らかになる
        /// </summary>
        [Header("線形補間の分割数: 大きいほど滑らか"), SerializeField, Min(1)]
        private int lerpCount = 10; 

        /// <summary>
        /// 波形変化を滑らかにするためのイージングの周期。
        /// </summary>
        [SerializeField] private float easingPeriod = 5f;

        private LineRenderer lineRenderer;

        /// <summary>
        /// LineRenderer の頂点の座標を格納する配列。
        /// </summary>
        private Vector3[] positions;

        protected override void Start()
        {
            base.Start();

            // 点どうしの間隔
            float distance = size.x / band.Length;

            // 頂点数は、一旦バンドの数と同数で初期化
            positions = new Vector3[band.Length];
            
            // x座標を等間隔に配置して初期化
            for (int i = 0; i < band.Length; i++)
            {
                positions[i] = new Vector3(distance * i, 0f, 0f);
            }

            // LineRendererの初期化
            lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.SetPositionsWithCount(positions);
        }

        void OnValidate()
        {
            Start();
        }

        void Update()
        {
            // オーディオが流れていると反映される
            UpdatePositions();
        }

        /// <summary>
        /// 頂点座標を更新する。
        /// </summary>
        private void UpdatePositions()
        {
            for (int i = 0; i < positions.Length; i++)
            {
                // y座標をスペクトルの値に応じて変化させる + イージング
                positions[i].y = (fft.SpectrumData[samplingIndexes[i]] * amplitude + size.y).EaseInOutSine(easingPeriod);
            }

            // ベジェ曲線化した頂点座標を格納する
            List<Vector3> curves = new();

            // 始点、終点、制御点を抽出して、ベジェ曲線の座標を求めていく
            Vector3 start = positions[0], end = positions[^1];
            Vector3[] controls = positions[1..^1];

            // 線形補間を用いてベジェ曲線の座標を求める
            // 一辺の分割回数分繰り返す
            for (int i = 0; i < lerpCount; i++)
            {
                // 計算用のベクトルを初期化する
                List<Vector3> v = new();

                // forループ処理で使うカウンタ変数をここで初期化する。
                int j = 0, k = 0, l = 0, m = 0;

                // 分割数との比により媒介変数tを計算する。
                float t = i / (float)lerpCount;

                // -- Phase1 : V1 ~ Vnまでを求める。V1 ~ Vn ベクトルの個数としてはn個 -- //
                // リストの最初に、始点と最初の制御点との球面線形補間を追加する。V1とおく。
                v.Insert(0, Vector3.Lerp(start, controls[0], t));

                // 制御点どうしで、線形補間する。
                // ベクトル V2~V(n-1) を<制御点どうし>の線形補間により求める。V2 ~ V[n-1]までのベクトルの個数としてはn-2個
                for (j = 1; j <= controls.Length - 1; j++)
                {
                    v.Insert(j, Vector3.Lerp(controls[j - 1], controls[j], t));
                }

                // リストの最後に、最後の制御点と終点との球面線形補間を追加する。Vnとおく。
                v.Add(Vector3.Lerp(controls[controls.Length - 1], end, t));

                // -- Phase2 : V[n+1] ~ V[(n/2) * (n+1)]までを求める。V[n+1] ~ V[n/2) * (n+1)]ベクトルの個数としては (n^2-n-2)/2 + 1 個 -- //
                // ベクトル V[n+1] ~ V[(n/2) * (n+1)] を、互いに線形補間して求める。
                for (k = 0; k < controls.Length; k++)
                {
                    for (m = 0; m < controls.Length - (k + 1); l++, m++)
                    {
                        v.Add(Vector3.Lerp(v[l], v[l + 1], t));
                    }
                }

                // リストの最後のベクトルが、算出したベジェ曲線の座標
                curves.Add(v[^1]);
            }

            // LineRenderer にベジェ曲線化した頂点座標を適用する
            lineRenderer.SetPositionsWithCount(curves.ToArray());
        }
    }
}

using UnityEngine;
using System.Collections.Generic;
using FRONTIER.Utility;
using FRONTIER.Utility.Mesh;
using FRONTIER.Utility.Development;

namespace FRONTIER.Menu.Background
{
    /// <summary>
    /// 流れている音楽のスペクトルをもとに波形のメッシュを生成して動かすクラス。
    /// </summary>
    [RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(FFT))]
    public class SpectrumVisualMesh : GameUtilityBase
    {
        #region フィールド

        /// <summary>
        /// 波のメッシュを生成するときの三角形ポリゴンの数。偶数にすること。
        /// </summary>
        [SerializeField] private int trianglesCount = 30;

        /// <summary>
        /// 波のメッシュを生成するときの三角形ポリゴンの横幅。
        /// </summary>
        [SerializeField] private float waveWidth = 2f;

        /// <summary>
        /// 波のメッシュを生成するときの三角形ポリゴンの縦幅。
        /// </summary>
        [SerializeField] private float waveHeight = 1f;

        /// <summary>
        /// メッシュに適用させる波の振幅。
        /// </summary>
        [SerializeField] private float waveAmplitude = 200f;

        /// <summary>
        /// スペクトルデータを適用させない四角形の範囲。
        /// </summary>
        [SerializeField] private int waveOffset;

        /// <summary>
        /// 波のメッシュの生成に利用する MeshFilter。
        /// </summary>
        private MeshFilter meshFilter;

        /// <summary>
        /// 周波数情報を取得する。
        /// </summary>
        private FFT spectrum = default;

        /// <summary>
        /// 波形として表示する周波数帯域で30種類の周波数。
        /// </summary>
        private readonly int[] FrequencyBand = new int[30]
        {
            25, 32, 40, 50, 63, 80, 100, 125, 160, 200, 250, 315, 400, 500, 630, 800, 1000, 1250, 1600, 2000, 2500, 3150, 4000, 5000, 6300, 8000, 10000, 12500, 16000, 20000
        };

        /// <summary>
        /// サンプリングしたスペクトル情報のインデックスの値を格納する。
        /// </summary>
        private int[] samplingIndexes;

        /// <summary>
        /// オフセット (<see cref="waveOffset"/>) も加味した三角形ポリゴンの数。
        /// 最終的に生成するポリゴンの数となる。
        /// </summary>
        private int TotalTrianglesCount => trianglesCount + waveOffset;

        /// <summary>
        /// オーディオスペクトラムとして生成する波形のメッシュ。
        /// </summary>
        private Mesh waveMesh;

        /// <summary>
        /// 波形メッシュのパラメータ。
        /// </summary>
        private FilterParameters waveMeshParams;

        /// <summary>
        /// Y座標が変化する <see cref="waveMesh"/> の頂点のみ抽出したもの
        /// </summary>
        public Vector3[] activeWaveVertices;

        #endregion

        #region MonoBehaviour メソッド

        void OnValidate()
        {
            meshFilter = GetComponent<MeshFilter>();
            spectrum = GetComponent<FFT>();
            GenerateWaveMesh();
        }

        void Awake()
        {
            meshFilter = GetComponent<MeshFilter>();
            spectrum = GetComponent<FFT>();
        }

        void Start()
        {
            GenerateWaveMesh();
        }

        void Update()
        {
            UpdateWaveMesh();
        }

        #endregion

        #region メソッド

        /// <summary>
        /// 波形のメッシュを生成する。
        /// </summary>
        private void GenerateWaveMesh()
        {
            // バンドからサンプル周波数の配列で使えるインデックスを取得してくる
            samplingIndexes = GetIndicesOfBand(AudioSettings.outputSampleRate, FrequencyBand, spectrum.Resolution);

            // メッシュ用のパラメータ設定
            waveMeshParams.vertices = new Vector3[8 * (TotalTrianglesCount / 2)];
            waveMeshParams.triangles = new int[3 * TotalTrianglesCount];

            // 実際に可動する頂点の数は、メッシュの頂点の4分の1
            activeWaveVertices = new Vector3[waveMeshParams.vertices.Length / 4];

            // スペクトルをもとに複数の頂点を動かせるように、メッシュの頂点を増やす
            waveMesh = AmplifyVerticesWaveMesh();
            
            meshFilter.mesh = waveMesh;
        }

        /// <summary>
        /// 頂点を増幅させた新しいメッシュを作る。
        /// </summary>
        private Mesh AmplifyVerticesWaveMesh()
        {
            // できあがる1つの四角形の横幅を1としたときの三角形ポリゴンの対辺の幅。（分割係数的な）
            float partitionFactor = TotalTrianglesCount / 2f;

            // 各頂点の幅。
            float pointWidth = waveWidth / partitionFactor;

            // 左側から基準の四角形を作成していく
            // 左下
            waveMeshParams.vertices[0] = new Vector3(0, 0, 0);

            // 左上
            waveMeshParams.vertices[1] = new Vector3(0, waveHeight, 0);

            // 右下
            waveMeshParams.vertices[2] = new Vector3(pointWidth, 0, 0);

            // 右上
            waveMeshParams.vertices[3] = new Vector3(pointWidth, waveHeight, 0);

            // 頂点を増やす処理をする
            for (int i = 4; i < waveMeshParams.vertices.Length; i++)
            {
                // 左下
                if (i % 4 == 0)
                {
                    waveMeshParams.vertices[i] = new Vector3(waveMeshParams.vertices[i - 4].x + pointWidth, 0, 0);
                }
                // 左上
                if ((i - 1) % 4 == 0)
                {
                    waveMeshParams.vertices[i] = new Vector3(waveMeshParams.vertices[i - 4].x + pointWidth, waveHeight, 0);
                }
                // 右下
                if ((i - 2) % 4 == 0)
                {
                    waveMeshParams.vertices[i] = new Vector3(waveMeshParams.vertices[i - 4].x + pointWidth, 0, 0);
                }
                // 右上
                if ((i - 3) % 4 == 0)
                {
                    waveMeshParams.vertices[i] = new Vector3(waveMeshParams.vertices[i - 4].x + pointWidth, waveHeight, 0);
                }
            }

            // 三角形のインデックスを決める
            for (int i = 0, j = 0; i < waveMeshParams.triangles.Length; i += 6, j += 4)
            {
                /* 左下 - 左上 - 右下、右上 - 右下 - 左上 の順にポリゴンを作る。*/
                waveMeshParams.triangles[i] = j;
                waveMeshParams.triangles[i + 1] = j + 1;
                waveMeshParams.triangles[i + 2] = j + 2;
                waveMeshParams.triangles[i + 3] = j + 3;
                waveMeshParams.triangles[i + 4] = j + 2;
                waveMeshParams.triangles[i + 5] = j + 1;
            }

            return waveMeshParams.CreateMesh();
        }

        /// <summary>
        /// スペクトルをメッシュの大きさとして適用する。
        /// </summary>
        /// <param name="mesh">Y座標を変えるメッシュ。</param>
        /// <param name="vertices">メッシュの頂点。</param>
        private void UpdateWaveMesh()
        {
            //上辺の頂点だけ動かす
            for (int i = 0, j = 0; i < samplingIndexes.Length; i += 2, j++)
            {
                waveMeshParams.vertices[2 * i + 1 + waveOffset].y = spectrum.SpectrumData[samplingIndexes[2 * j]] * waveAmplitude + waveHeight;
                waveMeshParams.vertices[2 * i + 3 + waveOffset].y = spectrum.SpectrumData[samplingIndexes[2 * j]] * waveAmplitude + waveHeight;

                activeWaveVertices[2 * j] = waveMeshParams.vertices[2 * i + 1 + waveOffset];
                activeWaveVertices[2 * j + 1] = waveMeshParams.vertices[2 * i + 3 + waveOffset];
            }
            waveMesh.SetVertices(waveMeshParams.vertices);
        }



        /// <summary>
        /// 周波数帯域（バンド）に対応した元のスペクトルデータ (<see cref="FFT.spectrumData"/>) のインデックスを返す。
        /// </summary>
        /// <param name="samplingFrequency">サンプリングレート（周波数）</param>
        /// <param name="band">バンド</param>
        /// <param name="resolution">フーリエ変換（FFT）の解像度。</param>
        /// <returns>インデックスを格納した配列。</returns>
        private int[] GetIndicesOfBand(int samplingFrequency, int[] band, int resolution)
        {
            // インデックスの配列
            int[] indexes = new int[band.Length];

            for (int i = 0; i < indexes.Length; i++)
            {
                // 公式からインデックスの式に変形したもの
                // https://tech.mobilefactory.jp/entry/2020/12/21/140000
                indexes[i] = Mathf.RoundToInt(2f * (band[i] * resolution / (float)samplingFrequency));
            }

            return indexes;
        }

        /// <summary>
        /// オーディオスペクトラムとする波形のメッシュ(<see cref = "waveMesh"/>)から波形状のベジェ曲線を生成する。
        /// </summary>
        /// <param name="split">ベジェ曲線の分割数。</param>
        /// <returns>生成したN次ベジェ曲線の座標の配列。</returns>
        public Vector3[] GenerateSpectrumBezierCurve(int split)
        {
            var vertices = activeWaveVertices;

            // あえて可動部全ての頂点を参照しないことで、全体のスペクトラムの変化の平均をとるかたちにする。
            // そうして波形を安定させる試み。
            
            // ベジェ曲線となる座標を収めるリスト
            List<Vector3> curve = new List<Vector3>();

            // ベジェ曲線の始点（最初は使わずに１番目を始点にしてみる）
            Vector3 start = vertices[1];

            // ベジェ曲線の終点（終点はそのまま最後）
            Vector3 end = vertices[vertices.Length - 1];

            // ベジェ曲線の制御点のリスト
            List<Vector3> controls = new List<Vector3>();

            // 始点より後の奇数番号の頂点だけ制御点に選び取る
            for (int i = 3; i < vertices.Length - 2; i += 2)
            {
                controls.Add(vertices[i]);
            }

            // 球面線形補間に使うためのベクトルのリスト
            List<Vector3> v = new List<Vector3>();

            // 球面線形補間 Slerp を用いてベジェ曲線の座標を求める。
            // 一辺の分割回数分繰り返す
            for (int i = 0; i < split; i++)
            {
                // ベクトルを初期化する
                v.Clear();

                // forループ処理で使うカウンタ変数をここで初期化する。
                int j = 0, k = 0, l = 0, m = 0;

                // 分割数との比により媒介変数tを計算する。
                float t = (float)i / (float)split;

                // -- Phase1 : V1 ~ Vnまでを求める。V1 ~ Vn ベクトルの個数としてはn個 -- //
                // リストの最初に、始点と最初の制御点との球面線形補間を追加する。V1とおく。
                v.Insert(0, Vector3.Slerp(start, controls[0], t));

                // 制御点どうしで、線形補間する。
                // ベクトル V2~V(n-1) を<制御点どうし>の線形補間により求める。V2 ~ V[n-1]までのベクトルの個数としてはn-2個
                for (j = 1; j <= controls.Count - 1; j++)
                {
                    v.Insert(j, Vector3.Slerp(controls[j - 1], controls[j], t));
                }

                // リストの最後に、最後の制御点と終点との球面線形補間を追加する。Vnとおく。
                v.Add(Vector3.Slerp(controls[controls.Count - 1], end, t));

                // -- Phase2 : V[n+1] ~ V[(n/2) * (n+1)]までを求める。V[n+1] ~ V[n/2) * (n+1)]ベクトルの個数としては (n^2-n-2)/2 + 1 個 -- //
                // ベクトル V[n+1] ~ V[(n/2) * (n+1)] を、互いに線形補間して求める。
                for (k = 0; k < controls.Count; k++)
                {
                    for (m = 0; m < controls.Count - (k + 1); l++, m++)
                    {
                        v.Add(Vector3.Slerp(v[l], v[l + 1], t));
                    }
                }

                // リストの最後のベクトルが算出されたベジェ曲線の座標
                curve.Add(v[^1]);
            }

            return curve.ToArray();
        }

        #endregion
    }
}

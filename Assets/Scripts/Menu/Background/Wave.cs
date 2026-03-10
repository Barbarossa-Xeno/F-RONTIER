using UnityEngine;
using System.Collections.Generic;
using FRONTIER.Utility;
using FRONTIER.Utility.Development;

namespace FRONTIER.Menu.Background
{
    public class Wave : GameUtility
    {
        /* --フィールド-- */
        /// <summary>
        /// 波のメッシュのメッシュフィルター。
        /// </summary>
        [SerializeField] private MeshFilter waveMeshFilter;

        /// <summary>
        /// 周波数情報を取得する。
        /// </summary>
        [SerializeField] private FFT spectrum = default;

        /// <summary>
        /// 波形として表示する周波数帯域で30種類の周波数。
        /// </summary>
        private readonly int[] frequencyBand = new int[30]
        {
        25, 32, 40, 50, 63, 80, 100, 125, 160, 200, 250, 315, 400, 500, 630, 800, 1000, 1250, 1600, 2000, 2500, 3150, 4000, 5000, 6300, 8000, 10000, 12500, 16000, 20000
        };

        /// <summary>
        /// サンプリングしたスペクトル情報のインデックスの値を格納する。
        /// </summary>
        private int[] samplingIndices;

        /// <summary>
        /// フーリエ変換の解像度。
        /// </summary>
        private int resolution;
        /// <summary>
        /// 波のメッシュを生成するときの三角形ポリゴンの数。偶数にすること。
        /// </summary>
        [SerializeField] private int numberOfTriangles = 30;

        /// <summary>
        /// オフセット<see cref = "waveOffset"/>も加味した三角形ポリゴンの数。
        /// </summary>
        private int trianglesNum;

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
        /// 周波数波形を適用させない四角形の範囲。
        /// </summary>
        [SerializeField] private int waveOffset;

        /// <summary>
        /// オーディオスペクトラムとして生成する波形のメッシュ。
        /// </summary>
        private Mesh waveMesh;

        /// <summary>
        /// 生成する波形のメッシュの情報を保存する構造体。
        /// </summary>
        private struct WaveMeshProperties
        {
            /// <summary>
            /// 頂点。
            /// </summary>
            public Vector3[] vertices;

            /// <summary>
            /// UV座標。
            /// </summary>
            public Vector2[] uvs;

            /// <summary>
            /// 三角形ポリゴンのインデックス。
            /// </summary>
            public int[] triangles;
        }

        /// <summary>
        /// 生成する波形のメッシュの情報を保存する構造体のインスタンス。
        /// </summary>
        private WaveMeshProperties waveMeshProp = new WaveMeshProperties();

        /// <summary>
        /// Ｙ座標が変化している<see cref = "waveMesh"/>の頂点のみ抽出したもの
        /// </summary>
        public Vector3[] activeWaveVertices;

        /* --MonoBehaviour メソッド-- */
        void Start()
        {
            // 初期化 //
            // フーリエ変換の解像度を取得してくる
            resolution = (int)spectrum.FFT_resolution;
            // バンドからサンプル周波数の配列で使えるインデックスを取得してくる
            samplingIndices = GetIndicesOfBand(AudioSettings.outputSampleRate, frequencyBand, resolution);
            // 最終的に生成するポリゴンの数は、オフセットも含めてこうなる
            trianglesNum = numberOfTriangles + waveOffset;
            // メッシュ用のパラメータ設定
            waveMeshProp.vertices = new Vector3[8 * (trianglesNum / 2)];
            waveMeshProp.triangles = new int[3 * trianglesNum];
            // スペクトルをもとに複数の頂点を動かせるように、メッシュの頂点を増やす
            waveMesh = AmplifyVerticesWaveMesh(waveMeshProp.vertices, waveMeshProp.triangles, trianglesNum);
            waveMeshFilter.mesh = waveMesh;
            // 実際に可動する頂点の数は、メッシュの頂点の4分の1
            activeWaveVertices = new Vector3[waveMeshProp.vertices.Length / 4];
        }

        void Update()
        {
            SetWaveMesh(waveMesh, waveMeshProp.vertices);
        }

        /* --メソッド-- */
        /// <summary>
        /// 頂点を増幅させた新しいメッシュを作る。
        /// </summary>
        /// <param name="vertice">頂点を格納する配列。</param>
        /// <param name="triangle">三角形ポリゴンのインデックスを指定する配列。</param>
        /// <param name="number">三角形ポリゴンの数。</param>
        /// <returns></returns>
        private Mesh AmplifyVerticesWaveMesh(Vector3[] vertice, int[] triangle, int number)
        {
            //逆数を出力するローカル関数
            static float Inverse(float number) { return 1f / number; }
            //新たに作るメッシュ
            Mesh mesh = new Mesh();
            // できあがる１つの四角形の横幅を１としたときの三角形ポリゴンの対辺の幅。（分割係数的な）
            float partitionNumber = number / 2f;
            // 各頂点の幅。
            float pointWidth = Inverse(partitionNumber) * waveWidth;

            /* 左側から基準の四角形を作成していく */
            //左下
            vertice[0] = new Vector3(0, 0, 0);
            //左上
            vertice[1] = new Vector3(0, waveHeight, 0);
            //右下
            vertice[2] = new Vector3(pointWidth, 0, 0);
            //右上
            vertice[3] = new Vector3(pointWidth, waveHeight, 0);

            //頂点を増やす処理をする
            for (int i = 4; i < vertice.Length; i++)
            {
                //左下
                if (i % 4 == 0) { vertice[i] = new Vector3(vertice[i - 4].x + pointWidth, 0, 0); }
                //左上
                if ((i - 1) % 4 == 0) { vertice[i] = new Vector3(vertice[i - 4].x + pointWidth, waveHeight, 0); }
                //右下
                if ((i - 2) % 4 == 0) { vertice[i] = new Vector3(vertice[i - 4].x + pointWidth, 0, 0); }
                //右上
                if ((i - 3) % 4 == 0) { vertice[i] = new Vector3(vertice[i - 4].x + pointWidth, waveHeight, 0); }
            }

            //三角形のインデックスを決める
            for (int i = 0, j = 0; i < triangle.Length; i += 6, j += 4)
            {
                /* 左下 - 左上 - 右下、右上 - 右下 - 左上 の順にポリゴンを作る。*/
                triangle[i] = j;
                triangle[i + 1] = j + 1;
                triangle[i + 2] = j + 2;
                triangle[i + 3] = j + 3;
                triangle[i + 4] = j + 2;
                triangle[i + 5] = j + 1;
            }

            mesh.vertices = vertice;
            mesh.triangles = triangle;
            mesh.RecalculateNormals();

            return mesh;
        }

        /// <summary>
        /// スペクトルをメッシュの大きさとして適用する。
        /// </summary>
        /// <param name="mesh">Y座標を変えるメッシュ。</param>
        /// <param name="vertices">メッシュの頂点。</param>
        private void SetWaveMesh(Mesh mesh, Vector3[] vertices)
        {
            //上辺の頂点だけ動かす
            for (int i = 0, j = 0; i < samplingIndices.Length; i += 2, j++)
            {
                vertices[2 * i + 1 + waveOffset].y = spectrum.spectrumData[samplingIndices[2 * j]] * waveAmplitude + waveHeight;
                vertices[2 * i + 3 + waveOffset].y = spectrum.spectrumData[samplingIndices[2 * j]] * waveAmplitude + waveHeight;

                activeWaveVertices[2 * j] = vertices[2 * i + 1 + waveOffset];
                activeWaveVertices[2 * j + 1] = vertices[2 * i + 3 + waveOffset];
            }
            mesh.SetVertices(vertices);
        }



        /// <summary>
        /// 周波数帯域（バンド）に対応した元のスペクトルデータ(<see cref = "FFT.spectrumData"/>)のインデックスを返す。
        /// </summary>
        /// <param name="samplingFrequency">サンプリングレート（周波数）</param>
        /// <param name="band">バンド</param>
        /// <param name="resolution">フーリエ変換（FFT）の解像度。</param>
        /// <returns>インデックスを格納した配列。</returns>
        private int[] GetIndicesOfBand(int samplingFrequency, int[] band, int resolution)
        {
            // インデックスの配列（indexの複数形ってindicesらしいね）
            int[] indices = new int[band.Length];
            for (int i = 0; i < indices.Length; i++)
            {
                // 公式からインデックスの式に変形したもの
                // https://tech.mobilefactory.jp/entry/2020/12/21/140000
                indices[i] = Mathf.RoundToInt(2f * (band[i] * resolution / (float)samplingFrequency));
            }
            return indices;
        }

        /// <summary>
        /// オーディオスペクトラムとする波形のメッシュ(<see cref = "waveMesh"/>)から波形状のベジェ曲線を生成する。
        /// </summary>
        /// <param name="vertices">使用するスペクトラムの可動部の頂点。</param>
        /// <param name="split">ベジェ曲線の分割数。</param>
        /// <returns>生成したN次ベジェ曲線の座標の配列。</returns>
        public Vector3[] GenerateSpectrumBezierCurve(Vector3[] vertices, int split)
        {
            // -- あえて可動部全ての頂点を参照しないことで、全体のスペクトラムの変化の平均をとるかたちにする。そうして波形を安定させる -- //
            // ベジェ曲線となる座標を収めるリスト
            List<Vector3> curve = new List<Vector3>();
            // ベジェ曲線の始点（最初は使わずに１番目を始点にしてみる）
            Vector3 start = vertices[1];
            // ベジェ曲線の終点（終点はそのまま最後）
            Vector3 end = vertices[vertices.Length - 1];
            // ベジェ曲線の制御点のリスト
            List<Vector3> controls = new List<Vector3>();
            // 始点より後の奇数番号の頂点だけ制御点に選び取る
            for (int i = 3; i < vertices.Length - 2; i += 2) { controls.Add(vertices[i]); }
            // 球面線形補間に使うためのベクトルのリスト
            List<Vector3> v = new List<Vector3>();
            // -- 球面線形補間 <Slerp> -- //
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

    }
}
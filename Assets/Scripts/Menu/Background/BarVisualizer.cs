using UnityEngine;
using FRONTIER.Utility.Mesh;

namespace FRONTIER.Menu.Background
{
    /// <summary>
    /// 棒グラフ状のオーディオビジュアライザを生成するクラス。
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class BarVisualizer : AudioVisualizer
    {
        /// <summary>
        /// メッシュの生成に利用する。
        /// </summary>
        private MeshFilter meshFilter;

        /// <summary>
        /// オーディオスペクトラムとして生成するメッシュ。
        /// </summary>
        private Mesh mesh;

        /// <summary>
        /// メッシュ生成時のパラメータ。
        /// </summary>
        private FilterParameters meshParams;

        protected override void Start()
        {
            base.Start();

            // ポリゴン数 = バンドの数
            // 棒として扱いやすくするために、四角ポリゴンの数（＝三角ポリゴンの数 / 2）を基準にメッシュを構成する
            // つまりband.Length / 2 が四角形の数に相当
            meshParams.vertices = new Vector3[4 * band.Length];
            meshParams.triangles = new int[6 * band.Length];
            CreateBar();

            meshFilter = gameObject.GetComponent<MeshFilter>();
            meshFilter.mesh = mesh;
        }

        void OnValidate()
        {
            Start();
        }

        void Update()
        {
            // オーディオが流れていると反映される
            UpdateBar();
        }

        /// <summary>
        /// 棒グラフを作成する。
        /// </summary>
        private void CreateBar()
        {
            // 1つの棒の幅
            float barWidth = size.x / band.Length;

            // 左側から基準の四角形を作成していく
            // 左下
            meshParams.vertices[0] = new Vector3(0, 0, 0);

            // 左上
            meshParams.vertices[1] = new Vector3(0, size.y, 0);

            // 右下
            meshParams.vertices[2] = new Vector3(barWidth, 0, 0);

            // 右上
            meshParams.vertices[3] = new Vector3(barWidth, size.y, 0);

            // さっき作った四角形を基準にして
            // 右側に向けて四角形とその頂点を増やしていく
            for (int i = 4; i < meshParams.vertices.Length; i++)
            {
                // 左下
                if (i % 4 == 0)
                {
                    meshParams.vertices[i] = new Vector3(meshParams.vertices[i - 4].x + barWidth, 0, 0);
                }
                // 左上
                if ((i - 1) % 4 == 0)
                {
                    meshParams.vertices[i] = new Vector3(meshParams.vertices[i - 4].x + barWidth, size.y, 0);
                }
                // 右下
                if ((i - 2) % 4 == 0)
                {
                    meshParams.vertices[i] = new Vector3(meshParams.vertices[i - 4].x + barWidth, 0, 0);
                }
                // 右上
                if ((i - 3) % 4 == 0)
                {
                    meshParams.vertices[i] = new Vector3(meshParams.vertices[i - 4].x + barWidth, size.y, 0);
                }
            }

            // 三角形のインデックスを決める
            // 四角形1つの中に2つの三角形の頂点が合計6点あるので、1ループで6点ずつ決めていく
            // また、1ループで四角形1つ分の頂点4点を操作する
            for (int i = 0, j = 0; i < meshParams.triangles.Length; i += 6, j += 4)
            {
                /* (左下 - 左上 - 右下)、(右上 - 右下 - 左上) の順にポリゴンを作る。*/
                meshParams.triangles[i] = j;
                meshParams.triangles[i + 1] = j + 1;
                meshParams.triangles[i + 2] = j + 2;
                meshParams.triangles[i + 3] = j + 3;
                meshParams.triangles[i + 4] = j + 2;
                meshParams.triangles[i + 5] = j + 1;
            }

            mesh = meshParams.CreateMesh();
        }

        /// <summary>
        /// オーディオスペクトラムの変化を棒グラフに適用する。
        /// </summary>
        private void UpdateBar()
        {
            // 上辺の頂点だけ動かす
            for (int i = 0, j = 0; (i < meshParams.vertices.Length && j < samplingIndexes.Length); i += 2, j++)
            {
                // +1 は常に棒の左上頂点
                meshParams.vertices[2 * i + 1].y = fft.SpectrumData[samplingIndexes[j]] * amplitude + size.y;
                // +3 は常に棒の右上頂点
                meshParams.vertices[2 * i + 3].y = fft.SpectrumData[samplingIndexes[j]] * amplitude + size.y;
                // 表示されるスペクトルは実質元のバンドの半分の数になる
            }
            mesh.SetVertices(meshParams.vertices);
        }
    }
}

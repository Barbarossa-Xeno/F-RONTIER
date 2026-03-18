using UnityEngine;
using FRONTIER.Utility;
using FRONTIER.Utility.Mesh;

namespace FRONTIER.Menu.Window
{
    /// <summary>
    /// メニューウィンドウの背景をメッシュでかたどる。
    /// </summary>
    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
    public class WindowMesh : WindowBorder
    {
        #region フィールド

        /// <summary>
        /// 使用するメッシュレンダラー。
        /// </summary>
        [SerializeField] private MeshRenderer meshRenderer;

        /// <summary>
        /// 使用するメッシュフィルター。
        /// </summary>
        [SerializeField] private MeshFilter meshFilter;

        /// <summary>
        /// 生成する背景メッシュ。
        /// </summary>
        private Mesh backgroundMesh;

        /// <summary>
        /// 生成する背景メッシュのプロパティ。
        /// </summary>
        private FilterParameters backgroundMeshParams;

        #endregion

        #region MonoBehaviourメソッド

        void OnValidate() => Initialize();

        void Start() => Initialize();

        #endregion

        #region オーバライドメソッド

        public override void Initialize()
        {
            InitializeEvents?.Invoke();
            GenerateMesh();
            SetMesh();
        }

        public override void RescaleObject()
        {
            transform.localScale = new(1, canvasSize.y / parentRect.height, 1);
        }

        public override void SetColorTrigger(Reference.DifficultyRank difficulty)
        {
            // 背景の色を変えるためのローカル関数
            // ブラー背景には、元々のRGBを1.4で割ったものを適用すると外見が元々の色に程近くなる
            static Color32 FixColorForBlur(Color32 color)
            {
                return new
                (
                    r: (byte)((float)color.r / 1.4f), 
                    g: (byte)((float)color.g / 1.4f), 
                    b: (byte)((float)color.b / 1.4f), 
                    a: 255
                );
            }
            
            meshRenderer.material.SetColor("_Color", FixColorForBlur(MenuInfo.menuInfo.FromDifficulty(difficulty).Item2));
        }

        #endregion

        #region メソッド

        /// <summary>
        /// メッシュを生成する。
        /// </summary>
        private void GenerateMesh()
        {
            // メッシュとその設定のインスタンスを作成
            backgroundMesh = new();

            // 背景メッシュの周りの頂点の数
            // 曲線 (BorderCurve) の点の数 + 右端の頂点の数（右上と右下）
            int borderVerticesCount = curveLocalPositions.Length + 2;

            backgroundMeshParams = new()
            {
                // +1 は、メッシュの中心点を追加するため
                vertices = new Vector3[borderVerticesCount + 1],

                // ポリゴンはメッシュの中心点と、メッシュ周りの各点を結んで作る
                // 周りの頂点の数の分だけ三角形ができるので、3倍する
                triangles = new int[borderVerticesCount * 3]
            };

            // 端の頂点の座標（左上と左下は使わないけど）
            Vector3 topLeft, bottomLeft, topRight, bottomRight;

            // 端の頂点の座標を指定
            topLeft = curveLocalPositions[0];
            bottomLeft = curveLocalPositions[^1];
            float rootWidth = parentRect.width / parentRect.parentWidthRatio;
            topRight = new(parentRect.width + rootWidth * ((1 - parentRect.parentWidthRatio) - parentRect.parentWidthRatio), topLeft.y);
            bottomRight = new(parentRect.width + rootWidth * ((1 - parentRect.parentWidthRatio) - parentRect.parentWidthRatio), -topLeft.y);

            // 頂点の座標を指定するためのループ
            for (int i = 0; i < backgroundMeshParams.vertices.Length; i++)
            {
                // 先頭: 中心点
                if (i == 0)
                {
                    // 曲線を描画するときに少し大きく描画しているため、中心点の高さもそれに依存する
                    backgroundMeshParams.vertices[i] = new(origin.x, topLeft.y / 2);
                }
                // 中間 (1 ~ curveLocalPositions.Length (= vertices.Length - 3)): 曲線の点
                else if (i <= curveLocalPositions.Length)
                {
                    backgroundMeshParams.vertices[i] = curveLocalPositions[i - 1];
                }
                // 末尾1つ前: 右下
                else if (i == backgroundMeshParams.vertices.Length - 2)
                {
                    backgroundMeshParams.vertices[i] = bottomRight;
                }
                // 末尾: 右上
                else
                {
                    backgroundMeshParams.vertices[i] = topRight;
                }
            }

            // ローカル座標からメッシュの位置を補正する
            for (int i = 0; i < backgroundMeshParams.vertices.Length; i++)
            {
                // 全頂点のx座標を、メッシュの横幅の半分だけマイナス方向（左向き）にずらして補正する
                // （つまり、中心点をローカル座標の原点にする操作）
                // curveLocalPositionsは、一番左端が0になっているので、
                // 右端のローカルX座標はメッシュの横幅に相当する
                backgroundMeshParams.vertices[i] -= new Vector3(topRight.x / 2, 0);
            }

            // インデックスの適用
            for (int k = 0, l = -1; k < backgroundMeshParams.triangles.Length; k += 3, l += 2)
            {
                backgroundMeshParams.triangles[k] = 0;
                backgroundMeshParams.triangles[k + 1] = (k + 1) - l == backgroundMeshParams.vertices.Length ? 1 : (k + 1) - l;
                backgroundMeshParams.triangles[k + 2] = (k + 1) - l - 1;
            }
        }

        /// <summary>
        /// メッシュを適用する。
        /// </summary>
        private void SetMesh()
        {
            backgroundMesh.vertices = backgroundMeshParams.vertices;
            backgroundMesh.triangles = backgroundMeshParams.triangles;
            backgroundMesh.RecalculateNormals();

            meshFilter.mesh = backgroundMesh;
        }

        /// <summary>
        /// メッシュの頂点（<see cref="backMeshProp.vertices"/> ）の値を補正する。
        /// </summary>
        /// <param name="meshWidth">メッシュの横幅</param>
        private void CorrectVertexPositions(float meshWidth)
        {
            for (int i = 0; i < backgroundMeshParams.vertices.Length; i++)
            {
                // 全頂点のx座標を、メッシュの横幅の半分だけマイナス方向（左向き）にずらす
                backgroundMeshParams.vertices[i] -= new Vector3(meshWidth / 2, 0);
            }
        }

        /// <summary>
        /// ブラー係数を調整する。
        /// </summary>
        // インスペクタから自身の Initialize Events に登録
        public void SetBlurFactor()
        {
            // スマホでブラー処理がとても重かったので、スマホではブラーを軽くする

            #if UNITY_EDITOR || UNITY_STANDALONE_WIN

            meshRenderer.material.SetFloat("_Blur", 100f);

            #elif UNITY_ANDROID

            meshRenderer.material.SetFloat("_Blur", 25f);

            #endif
        }
        
        #endregion
    }
}

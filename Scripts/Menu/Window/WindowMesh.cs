using UnityEngine;

namespace Game.Menu.Window
{/// <summary>
 /// メニューウィンドウの背景をメッシュでかたどる。
 /// </summary>
    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
    public class WindowMesh : WindowBorder
    {
        /// <summary>
        /// 使用するレンダラー。
        /// </summary>
        [SerializeField] private MeshRenderer meshRenderer;

        /// <summary>
        /// 使用するフィルター。
        /// </summary>
        [SerializeField] private MeshFilter meshFilter;

        /// <summary>
        /// 生成するメッシュ。
        /// </summary>
        private Mesh backMesh;

        /// <summary>
        /// 生成するメッシュのプロパティ。
        /// </summary>
        private BackMeshPropaties backMeshProp;

        /// <summary>
        /// メッシュにプロパティを適用するためのクラス。
        /// </summary>
        private class BackMeshPropaties
        {
            public Vector3[] vertices;

            public Vector2[] uvs;

            public int[] triangles;

            public BackMeshPropaties(int verticesCount)
            {
                vertices = new Vector3[verticesCount + 1];
                triangles = new int[verticesCount * 3];
            }
        }

        /* Monobehavior イベント */
        void OnValidate() => Initialize();

        /* 継承メソッド */
        public override void Initialize()
        {
            GenerateMesh();
            SetMesh();
        }

        public override void ReScaleObject()
        {
            transform.localScale = new(1, canvasSize.y / parentRect.height, 1);
        }

        /* メソッド */
        /// <summary>
        /// メッシュを生成する。
        /// </summary>
        private void GenerateMesh()
        {
            // メッシュとその設定のインスタンスを作成
            backMesh = new();
            backMeshProp = new(curveLocalPositions.Length + 2);
            // メッシュ生成
            CalculateMesh();
        }

        /// <summary>
        /// 曲線に合わせたメッシュを計算して生成する。
        /// </summary>
        private void CalculateMesh()
        {
            //端の頂点の座標（左上と左下は使わないけど）
            Vector3 topLeft, bottomLeft, topRight, bottomRight;
            // カウンタ変数
            int j = 0;

            // 端の頂点の座標を指定
            topLeft = curveLocalPositions[0];
            bottomLeft = curveLocalPositions[^1];
            float rootWidth = parentRect.width / parentRect.widthToParent;
            topRight = new(parentRect.width + rootWidth * ((1 - parentRect.widthToParent) - parentRect.widthToParent), topLeft.y);
            bottomRight = new(parentRect.width + rootWidth * ((1 - parentRect.widthToParent) - parentRect.widthToParent), -topLeft.y);

            // メッシュの中心となる基準点を設定（曲線を描画するときに、画面領域よりも少し大きく描画しているので、基準点の高さもそれに依存する）
            backMeshProp.vertices[0] = new(origin.x, topLeft.y / 2);

            // 曲線のローカル座標を参照してメッシュの各頂点を指定
            for (j = 1; j <= curveLocalPositions.Length; j++) { backMeshProp.vertices[j] = curveLocalPositions[j - 1]; }

            // 頂点の最後の２つの要素は右下と右上
            backMeshProp.vertices[j] = bottomRight;
            backMeshProp.vertices[j + 1] = topRight;
            CorrectVerticePositions(topRight.x);

            // インデックスの適用
            for (int k = 0, l = -1; k < backMeshProp.triangles.Length; k += 3, l += 2)
            {
                backMeshProp.triangles[k] = 0;
                backMeshProp.triangles[k + 1] = (k + 1) - l == backMeshProp.vertices.Length ? 1 : (k + 1) - l;
                backMeshProp.triangles[k + 2] = (k + 1) - l - 1;
            }
        }

        /// <summary>
        /// メッシュを適用する。
        /// </summary>
        private void SetMesh()
        {
            backMesh.vertices = backMeshProp.vertices;
            backMesh.triangles = backMeshProp.triangles;
            backMesh.RecalculateNormals();

            meshFilter.mesh = backMesh;
        }

        /// <summary>
        /// メッシュの頂点（<see cref="backMeshProp.vertices"/> ）の値を補正する。
        /// </summary>
        /// <param name="meshWidth">メッシュの横幅</param>
        private void CorrectVerticePositions(float meshWidth)
        {
            for (int i = 0; i < backMeshProp.vertices.Length; i++)
            {
                // 全頂点のx座標を、メッシュの横幅の半分だけマイナス方向（左向き）にずらす
                backMeshProp.vertices[i] -= new Vector3(meshWidth / 2, 0);
            }
        }
    }
}
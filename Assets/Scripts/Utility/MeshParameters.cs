using UnityEngine;

namespace FRONTIER.Utility.Mesh
{
    /// <summary>
    /// コライダー用のメッシュを生成するためのパラメータをまとめたクラス。
    /// </summary>
    public struct ColliderParameters
    {
        /// <summary>
        /// コライダーの頂点座標。
        /// </summary>
        public Vector3[] vertices;

        /// <summary>
        /// 三角ポリゴンの頂点インデックス。
        /// </summary>
        public int[] triangles;

        /// <summary>
        /// パラメータからメッシュを生成して法線を計算する。
        /// </summary>
        /// <returns>パラメータが指定された新しいメッシュ</returns>
        public readonly UnityEngine.Mesh CreateMesh()
        {
            UnityEngine.Mesh mesh = new()
            {
                vertices = this.vertices,
                triangles = this.triangles
            };
            mesh.RecalculateNormals();
            return mesh;
        }
    }

    /// <summary>
    /// MeshFilter 用のメッシュを生成するためのパラメータをまとめたクラス。
    /// </summary>
    public struct FilterParameters
    {
        /// <summary>
        /// メッシュの頂点座標。
        /// </summary>
        public Vector3[] vertices;

        /// <summary>
        /// 三角ポリゴンの頂点インデックス。
        /// </summary>
        public int[] triangles;

        /// <summary>
        /// テクスチャを貼るときのUV座標。
        /// </summary>
        public Vector2[] uvs;

        /// <summary>
        /// パラメータからメッシュを生成して法線を計算する。
        /// </summary>
        /// <returns>パラメータが指定された新しいメッシュ</returns>
        public readonly UnityEngine.Mesh CreateMesh()
        {
            UnityEngine.Mesh mesh = new()
            {
                vertices = this.vertices,
                triangles = this.triangles,
                uv = this.uvs
            };
            mesh.RecalculateNormals();
            return mesh;
        }
    }
}

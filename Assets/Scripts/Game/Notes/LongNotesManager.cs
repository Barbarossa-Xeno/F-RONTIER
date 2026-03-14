using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FRONTIER.Utility;

namespace FRONTIER.Game.Notes
{
    // このクラスでの ribbon はロングノーツ間に生成する「帯」状のメッシュのことを指します。

    /// <summary>
    /// ロングノーツの生成と管理を行うクラス。
    /// </summary>
    [System.Serializable]
    public partial class LongNotesManager : NotesManagerBase<List<int>, List<float>, List<LongNote>>
    {
        #region フィールド

        /// <summary>
        /// 中間点を有するロングノーツ（節）のプレハブ。
        /// </summary>
        [SerializeField] private GameObject intermediateNotePrefab;

        /// <summary>
        /// 中間点を持たないロングノーツ（節）のプレハブ。
        /// </summary>
        [SerializeField] private GameObject directNotePrefab;

        /// <summary>
        /// <see cref = "NotesManager"/>
        /// </summary>
        [SerializeField] private NotesManager notesGenerator;

        /// <summary>
        /// 各ロングノーツごとの中間点の数をインデックス順に格納するリスト。
        /// </summary>
        public List<int> intermediateNotesCounts = new();

        /// <summary>
        /// 生成したロングノーツの中間点を管理しておく辞書。
        /// </summary>
        /// <remarks>
        /// Keyにロングノーツのインデックス、Valueにそのインデックスのロングノーツが含む中間ノーツのリストを代入して管理する。
        /// </remarks>
        public Dictionary<int, List<LongNote>> intermediateNotes = new();

        /// <summary>
        /// 生成したロングノーツの帯を格納したリスト。
        /// </summary>
        public List<LongNote> ribbons = new();

        /// <summary>
        /// ロングノーツの帯に適用するマテリアル。
        /// </summary>
        [SerializeField] private RibbonMaterials ribbonMaterials = new();

        /// <summary>
        /// 生成するロングノーツの帯のゲームオブジェクト。
        /// 曲線型で、多数の分割メッシュから作られているような帯に対しては
        /// 常にこのオブジェクトが親になるように設定する。
        /// </summary>
        /// <remarks>
        /// 子に多数の分割メッシュがある場合、1まとまりで最後の分割メッシュが入力されたときに <c><see cref="CombineFragmentMesh"/></c> や
        //  <c><see cref="ReprintTexture"/><c/> によってメッシュとUVを適用しなおす。
        /// </remarks>
        private LongNote ribbon;

        #endregion

        #region 定数

        /// <summary>
        /// ロングノーツの太さ。
        /// </summary>
        private const float RIBBON_WIDTH = 2.0f;

        /// <summary>
        /// ロングノーツの高さ。
        /// </summary>
        private const float RIBBON_HEIGHT = 0.001f;

        /// <summary>
        /// ロングノーツを生成するときに頂点の生成位置をレーンに合わせるための差分。
        /// </summary>
        private const float LANE_GAP = -3f;

        /// <summary>
        /// ノーツの奥行き。
        /// </summary>
        private const float NOTE_DEPTH = 0.6f;

        /// <summary>
        /// 曲線ロングノーツを生成するときのデフォルトの分割数。
        /// </summary>
        private const int SPLIT_SIZE = 10;

        /// <summary>
        /// ロングノーツのコライダーの横幅。
        /// </summary>
        private const float COLLIDER_WIDTH = 0.5f;

        #endregion

        #region 構造体・列挙型

        /// <summary>
        /// ロングノーツの帯のメッシュに適用する色々なマテリアルを保持する。
        /// </summary>
        [System.Serializable]
        private struct RibbonMaterials
        {
            /// <summary>
            /// 中間点無しのもの。
            /// </summary>
            public Material direct;

            /// <summary>
            /// 中間点有りのもの。
            /// </summary>
            public Material intermediate;

            /// <summary>
            /// レンダリングモードがFadeのマテリアル。（そうであったらなんでもいい）
            /// </summary>
            public Material fadeMaterial;

            /// <summary>
            /// 中間点有りのものの、帯中央に描画する中間線のマテリアル。
            /// </summary>
            public Material line;
        }

        /// <summary>
        /// ロングノーツの帯のメッシュ生成に用いる要素。
        /// </summary>
        private struct MeshFilterParameters
        {
            /// <summary>
            /// メッシュの頂点座標。
            /// </summary>
            public Vector3[] vertices;

            /// <summary>
            /// 三角ポリゴンの頂点の数。
            /// </summary>
            public int[] triangles;

            /// <summary>
            /// テクスチャのUV座標。
            /// </summary>
            public Vector2[] uvs;

            /// <summary>
            /// パラメータからメッシュを生成して法線を計算する。
            /// </summary>
            /// <returns>パラメータを指定した新しいメッシュ</returns>
            public readonly Mesh CreateMesh()
            {
                Mesh mesh = new()
                {
                    vertices = this.vertices,
                    triangles = this.triangles,
                    uv = this.uvs
                };
                mesh.RecalculateNormals();
                return mesh;
            }
        }

        /// <summary>
        /// ロングノーツにコライダーを設定するためのパラメータ。
        /// </summary>
        private struct MeshColliderParameters
        {
            /// <summary>
            /// メッシュの頂点座標。
            /// </summary>
            public Vector3[] vertices;

            /// <summary>
            /// 三角ポリゴンの頂点の数。
            /// </summary>
            public int[] triangles;

            /// <summary>
            /// パラメータからメッシュを生成して法線を計算する。
            /// </summary>
            /// <returns>パラメータを指定した新しいメッシュ</returns>
            public readonly Mesh CreateMesh()
            {
                Mesh mesh = new()
                {
                    vertices = this.vertices,
                    triangles = this.triangles
                };
                mesh.RecalculateNormals();
                return mesh;
            }
        }

        #endregion

        #region オーバーライドメソッド

        // NotesGenerator 側から実行される！
        public override void GenerateNotes()
        {
            // JSONの譜面データからロングノーツの情報を取得したとき、それらの情報は、ロングノーツ1まとまり単位でリストに格納する仕様にしている
            // このメソッドは、そんな各ロングノーツ1まとまりを生成するタイミングで使用するため、
            // 随時データが追加されていく laneNumbers の1次元目のカウントが分かれば、今どのロングノーツを生成すれば良いかが分かる（※のように）
            int shouldGenerateNoteIndex = laneIndexes.Count - 1;

            // 算出するX座標とZ座標
            float x, z;

            // 実際に Instantiate するノーツ
            GameObject instance;

            // 仮で入れておく中間点のリスト
            List<LongNote> t_intermediates = new();

            // 現在計算中のロングノーツのインデックスに対応した情報をリストたちから参照するようにする（※）
            for (int i = shouldGenerateNoteIndex; i < laneIndexes.Count; i++)
            {
                t_intermediates.Clear();

                // ロングノーツひとまとまりに対応した二次元配列を使って座標を計算する
                // 1次元目はロングノーツ1まとまとまりとして全体の中でのインデックス、2次元目はそのロングノーツ中のノーツのインデックス（始点、中間点、終点）
                // ノーツ間をつなぐ帯の生成に前後の座標が必要なためこの形で記録しておくと参照しやすい
                float[,] zRecords = new float[laneIndexes.Count, laneIndexes[i].Count];

                // ロングノーツ内のノーツの数の分だけループ
                for (int j = 0; j < laneIndexes[i].Count; j++)
                {
                    // ノーツの座標を求める
                    x = GetLaneX(laneIndexes[i][j]);
                    z = reachedTimes[i][j] * PlayInfo.NoteSpeed + Reference.noteOrigin.z;
                    zRecords[i, j] = z;

                    // ロングノーツの種類とオブジェクトとしてつけておく名前を、ノーツの種類と中間点の有無から決める
                    var (longNoteType, objectName) = (Reference.NoteType)types[i] switch
                    {
                        Reference.NoteType.LinearLong when intermediateNotesCounts[i] == 0 => (Reference.LongNoteType.DirectLinear, $"LongNote (DirectLinear) -{i}"),
                        Reference.NoteType.CurvedLong when intermediateNotesCounts[i] == 0 => (Reference.LongNoteType.DirectCurved, $"LongNote (DirectCurved) -{i}"),
                        Reference.NoteType.LinearLong when intermediateNotesCounts[i] != 0 => (Reference.LongNoteType.IntermediateLinear, $"LongNote (IntermediateLinear) -{i}"),
                        Reference.NoteType.CurvedLong when intermediateNotesCounts[i] != 0 => (Reference.LongNoteType.IntermediateCurved, $"LongNote (IntermediateCurved) -{i}"),
                        _ => (Reference.LongNoteType.None, $"LongNote (None) -{i}")
                    };

                    // ノーツの種類と中間点の有無から、生成するノーツのプレハブを決める
                    instance = intermediateNotesCounts[i] == 0
                        ? Instantiate(directNotePrefab, new(x, Reference.specialNoteOrigin.y, z), Quaternion.identity, instanceParent)
                        : Instantiate(intermediateNotePrefab, new(x, Reference.specialNoteOrigin.y, z), Quaternion.identity, instanceParent);

                    // 値の適用
                    instance.name = objectName;
                    LongNote note = instance.GetComponent<LongNote>();
                    note.Type = (Reference.NoteType)types[i];
                    note.Index = i;
                    note.ReachedTime = reachedTimes[i][j];
                    note.LaneIndex = laneIndexes[i][j];

                    // オートプレイの時は通常ノーツと同じ括りにするために、NotesGenerator のほうに全部入れる
                    // 通常プレイの時は、1番最初のノーツだけ入れる（判定の仕組みによる）
                    if (PlayInfo.IsAutoPlay || !PlayInfo.IsAutoPlay && j == 0)
                    {
                        notesGenerator.instances.Add(note);
                    }
                    // ループ回数（中間点の数）で処理する部分
                    if (j == 0)
                    {
                        // 1番最初のノーツは始点のノーツ
                        note.Part = Reference.LongNotePart.Start;
                    }
                    else
                    {
                        // 1番最初以降のノーツは中間点のノーツとして管理
                        t_intermediates.Add(note);
                        note.IsIntermediate = true;

                        // 中間点がある場合
                        if (intermediateNotesCounts[i] != 0)
                        {
                            // 分割メッシュ単位でリボンを作る
                            CreateRibbon
                            (
                                laneIndexes[i][j - 1],
                                zRecords[i, j - 1] + NOTE_DEPTH / 2,
                                laneIndexes[i][j],
                                zRecords[i, j] - NOTE_DEPTH / 2,
                                longNoteType,
                                i
                            );
                        }

                        // 最後のノーツ
                        if (j == laneIndexes[i].Count - 1)
                        {
                            // 今までの中間点のノーツをまとめて管理するリストに入れる
                            intermediateNotes.Add(i, t_intermediates);

                            // 終点のノーツに設定
                            note.Part = Reference.LongNotePart.End;

                            // 中間点がない場合
                            if (intermediateNotesCounts[i] == 0)
                            {
                                // 最後に帯をつくる
                                var (start, end) = laneIndexes[i].FirstAndLast();
                                CreateRibbon
                                (
                                    start,
                                    zRecords[i, 0] + NOTE_DEPTH / 2,
                                    end,
                                    zRecords[i, j] - NOTE_DEPTH / 2,
                                    longNoteType,
                                    i
                                );
                            }
                        }
                        else
                        {
                            // それ以外はただの中間点とマーク
                            note.Part = Reference.LongNotePart.Intermediate;
                        }
                    }
                }
            }
        }

        public override void SortNotes()
        {
            SetRibbonTransform();

            // インデックスを降順にソートし直したり、リストの中身を求め直す

            // このクラスで管理するのはロングノーツの始点以外
            instances = intermediateNotes.Values.ToList();
            instances.Reverse();
            foreach (var notes in instances)
            {
                notes.Reverse();
                
                for (int i = 0; i < notes.Count; i++)
                {
                    notes[i].NoteIndex = i;
                }
            }

            ribbons.Reverse();
            foreach (var ribbon in ribbons)
            {
                if (ribbon == null)
                {
                    continue;
                }

                foreach (var note in instances[ribbon.Index])
                {
                    ribbon.Pressed += isPressed => note.IsPressed = isPressed;
                    note.ReachedLine += () => Debug.Log(note);
                }
                ribbon.Pressed += isPressed => instances[ribbon.Index].ForEach(note => note.IsPressed = isPressed);

                // レイヤー付与
                ribbon.gameObject.SetLayerSelfChildren(LayerMask.NameToLayer("LongNoteRibbon"));
            }
        }

        public override bool DeleteNote(List<LongNote> target)
        {
            
            throw new System.NotImplementedException();
        }

        #endregion
    }
}

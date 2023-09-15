/* 名前空間 */
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
//自作名前空間
using FRONTIER.Utility;

namespace FRONTIER.Game.NotesManagement
{
    /// <summary>
    /// ロングノーツ生成を行うクラス。
    /// </summary>
    [System.Serializable]
    public class LongNotesGenerator : NotesManager<List<int>, List<float>, List<GameObject>>
    {
        #region フィールド

        /// <summary>
        /// <see cref = "NotesGenerator"/>
        /// </summary>
        [SerializeField] private NotesGenerator notesGenerator;

        /// <summary>
        /// 各ロングノーツごとの中間点の数をインデックス順に格納するリスト。
        /// </summary>
        public List<int> innerNotesCounts = new();

        /// <summary>
        /// ロングノーツの中間点の辞書。
        /// </summary>
        /// <remarks>
        /// Keyにロングノーツのインデックス、Valueにそのインデックスのロングノーツが含む中間ノーツのリストを代入して管理する。
        /// </remarks>
        public Dictionary<int, List<GameObject>> innerNotes = new();

        /// <summary>
        /// ロングノーツ線のリスト。
        /// </summary>
        public List<GameObject> longNoteMeshList = new();

        /// <summary>
        /// ロングノーツ線に紐づいた<see cref = "LongNotes"/>コンポーネントのリスト。
        /// </summary>
        /// <remarks>
        /// ロングノーツの長押し判定をEventSystemでとってくれる。
        /// </remarks>
        public List<LongNotes> longNotesList = new();

        /// <summary>
        /// ロングノーツ線に適用するマテリアル。
        /// </summary>
        [SerializeField] private MeshMaterials meshMaterials = new();

        /// <summary>
        /// 生成するロングノーツ線のゲームオブジェクト。
        /// </summary>
        private GameObject longNote;

        #endregion

        #region 定数

        /// <summary>
        /// ロングノーツの太さ。
        /// </summary>
        private const float LANE_WIDTH = 2.0f;

        /// 
        /// <summary>ロングノーツの高さ。
        /// </summary>
        private const float LANE_HEIGHT = 0.001f;

        /// <summary>
        /// ロングノーツを生成するときに頂点の生成位置をレーンに合わせるための差分。
        /// </summary>
        private const float LANE_GAP = -3f;

        /// <summary>
        /// 曲線ロングノーツを生成するときの、基底の分割数。
        /// </summary>
        private const int SPLIT_SIZE = 10;

        /// <summary>
        /// ロングノーツのコライダーの横幅。
        /// </summary>
        private const float COLLIDER_WIDTH = 0.5f;

        #endregion

        #region 構造体・列挙型

        /// <summary>
        /// ロングノーツ線のメッシュに適用する色々なマテリアル。
        /// </summary>
        [System.Serializable]
        private struct MeshMaterials
        {
            /// <summary>
            /// 中間点無し。
            /// </summary>
            public Material noInner;

            /// <summary>
            /// 中間点有り。
            /// </summary>
            public Material anyInner;

            /// <summary>
            /// レンダリングモードがFadeのマテリアル。（そうであったらなんでもいい）
            /// </summary>
            public Material fadeMaterial;

            /// <summary>
            /// 中間点有りノーツに描画する中間線のマテリアル。
            /// </summary>
            public Material line;
        }

        /// <summary>
        /// ロングノーツ線のメッシュ生成に用いる要素。
        /// </summary>
        private struct MeshParameters
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
        }

        #endregion

        #region メソッド

        /// <summary>
        /// ロングノーツ線のメッシュを生成します。
        /// </summary>
        /// <param name = "startPosition">ロングノーツの始点座標。</param>
        /// <param name = "endPosition">ロングノーツの終点座標。</param>
        /// <param name = "noteLine">ロングノーツ線のゲームオブジェクト。</param>
        /// <param name = "type">ロングノーツの種類。<br/>中間点無しなら0、中間点有りなら1。</param>
        /// <param name = "curve">曲線を作る点の座標。</param>
        /// <param name = "split">曲線型ロングノーツの分割数。</param>
        /// <param name = "isLast">分割したロングノーツの生成時、その対象が最後の分割ノーツだったときtrueを指定する。</param>
        private void GenerateLongNoteMesh(Vector3 startPosition, Vector3 endPosition, GameObject noteLine, Reference.LongNoteType type, Vector3[] curve = null, int split = 10, bool isLast = false)
        {
            // 生成するメッシュ本体
            Mesh mesh = new();

            // メッシュの描画に必要な要素の設定
            MeshParameters meshParameters = new();
            meshParameters.vertices = new Vector3[4 * 6];
            meshParameters.triangles = new int[6 * 6];
            meshParameters.uvs = new Vector2[4 * 6];
            MeshColliderParameters meshColliderParameters = new();
            meshColliderParameters.vertices = new Vector3[4 * 6];
            meshColliderParameters.triangles = new int[6 * 6];

            #region メッシュの頂点とUV座標、コライダーの座標を計算

            // 面①（ワールド原点からZ座標を正の方向に見たときの、上底 => Y方向）
            meshParameters.vertices[0] = startPosition + new Vector3(-LANE_WIDTH / 2, LANE_HEIGHT / 2, 0);     //始点の左端 = 左下
            meshParameters.vertices[1] = startPosition + new Vector3(LANE_WIDTH / 2, LANE_HEIGHT / 2, 0);      //始点の右端 = 右下
            meshParameters.vertices[2] = endPosition + new Vector3(-LANE_WIDTH / 2, LANE_HEIGHT / 2, 0);       //終点の左端 = 左上
            meshParameters.vertices[3] = endPosition + new Vector3(LANE_WIDTH / 2, LANE_HEIGHT / 2, 0);        //終点の右端 = 右上
            meshParameters.triangles[0] = 0;
            meshParameters.triangles[1] = 2;
            meshParameters.triangles[2] = 1;
            meshParameters.triangles[3] = 3;
            meshParameters.triangles[4] = 1;
            meshParameters.triangles[5] = 2;
            meshParameters.uvs[0] = new Vector2(0, 0);
            meshParameters.uvs[1] = new Vector2(1, 0);
            meshParameters.uvs[2] = new Vector2(0, 1);
            meshParameters.uvs[3] = new Vector2(1, 1);
            meshColliderParameters.vertices[0] = new Vector3(-COLLIDER_WIDTH, 0, 0) + meshParameters.vertices[0];
            meshColliderParameters.vertices[1] = new Vector3(COLLIDER_WIDTH, 0, 0) + meshParameters.vertices[1];
            meshColliderParameters.vertices[2] = new Vector3(-COLLIDER_WIDTH, 0, 0) + meshParameters.vertices[2];
            meshColliderParameters.vertices[3] = new Vector3(COLLIDER_WIDTH, 0, 0) + meshParameters.vertices[3];

            // 面②（前側面 => -Z方向）
            meshParameters.vertices[4] = startPosition + new Vector3(-LANE_WIDTH / 2, -LANE_HEIGHT / 2, 0);    //左下
            meshParameters.vertices[5] = startPosition + new Vector3(LANE_WIDTH / 2, -LANE_HEIGHT / 2, 0);     //右下
            meshParameters.vertices[6] = startPosition + new Vector3(-LANE_WIDTH / 2, LANE_HEIGHT / 2, 0);     //左上
            meshParameters.vertices[7] = startPosition + new Vector3(LANE_WIDTH / 2, LANE_HEIGHT / 2, 0);      //右上
            meshParameters.triangles[6] = 4;
            meshParameters.triangles[7] = 6;
            meshParameters.triangles[8] = 5;
            meshParameters.triangles[9] = 7;
            meshParameters.triangles[10] = 5;
            meshParameters.triangles[11] = 6;
            meshParameters.uvs[4] = new Vector2(0, 0);
            meshParameters.uvs[5] = new Vector2(1, 0);
            meshParameters.uvs[6] = new Vector2(0, 1);
            meshParameters.uvs[7] = new Vector2(1, 1);
            meshColliderParameters.vertices[4] = new Vector3(-COLLIDER_WIDTH, 0, 0) + meshParameters.vertices[4];
            meshColliderParameters.vertices[5] = new Vector3(COLLIDER_WIDTH, 0, 0) + meshParameters.vertices[5];
            meshColliderParameters.vertices[6] = new Vector3(-COLLIDER_WIDTH, 0, 0) + meshParameters.vertices[6];
            meshColliderParameters.vertices[7] = new Vector3(COLLIDER_WIDTH, 0, 0) + meshParameters.vertices[7];

            // 面③（左側面 => -X方向）
            meshParameters.vertices[8] = endPosition + new Vector3(-LANE_WIDTH / 2, -LANE_HEIGHT / 2, 0);      //左下
            meshParameters.vertices[9] = startPosition + new Vector3(-LANE_WIDTH / 2, -LANE_HEIGHT / 2, 0);    //右下
            meshParameters.vertices[10] = endPosition + new Vector3(-LANE_WIDTH / 2, LANE_HEIGHT / 2, 0);      //左上
            meshParameters.vertices[11] = startPosition + new Vector3(-LANE_WIDTH / 2, LANE_HEIGHT / 2, 0);    //右上
            meshParameters.triangles[12] = 8;
            meshParameters.triangles[13] = 10;
            meshParameters.triangles[14] = 9;
            meshParameters.triangles[15] = 11;
            meshParameters.triangles[16] = 9;
            meshParameters.triangles[17] = 10;
            meshParameters.uvs[8] = new Vector2(0, 0);
            meshParameters.uvs[9] = new Vector2(1, 0);
            meshParameters.uvs[10] = new Vector2(0, 1);
            meshParameters.uvs[11] = new Vector2(1, 1);
            meshColliderParameters.vertices[8] = new Vector3(-COLLIDER_WIDTH, 0, 0) + meshParameters.vertices[8];
            meshColliderParameters.vertices[9] = new Vector3(-COLLIDER_WIDTH, 0, 0) + meshParameters.vertices[9];
            meshColliderParameters.vertices[10] = new Vector3(-COLLIDER_WIDTH, 0, 0) + meshParameters.vertices[10];
            meshColliderParameters.vertices[11] = new Vector3(-COLLIDER_WIDTH, 0, 0) + meshParameters.vertices[11];

            // 面④（右側面 => X方向）
            meshParameters.vertices[12] = startPosition + new Vector3(LANE_WIDTH / 2, -LANE_HEIGHT / 2, 0);    //左下
            meshParameters.vertices[13] = endPosition + new Vector3(LANE_WIDTH / 2, -LANE_HEIGHT / 2, 0);      //右下
            meshParameters.vertices[14] = startPosition + new Vector3(LANE_WIDTH / 2, LANE_HEIGHT / 2, 0);     //左上
            meshParameters.vertices[15] = endPosition + new Vector3(LANE_WIDTH / 2, LANE_HEIGHT / 2, 0);       //右上
            meshParameters.triangles[18] = 12;
            meshParameters.triangles[19] = 14;
            meshParameters.triangles[20] = 13;
            meshParameters.triangles[21] = 15;
            meshParameters.triangles[22] = 13;
            meshParameters.triangles[23] = 14;
            meshParameters.uvs[12] = new Vector2(0, 0);
            meshParameters.uvs[13] = new Vector2(1, 0);
            meshParameters.uvs[14] = new Vector2(0, 1);
            meshParameters.uvs[15] = new Vector2(1, 1);
            meshColliderParameters.vertices[12] = new Vector3(COLLIDER_WIDTH, 0, 0) + meshParameters.vertices[12];
            meshColliderParameters.vertices[13] = new Vector3(COLLIDER_WIDTH, 0, 0) + meshParameters.vertices[13];
            meshColliderParameters.vertices[14] = new Vector3(COLLIDER_WIDTH, 0, 0) + meshParameters.vertices[14];
            meshColliderParameters.vertices[15] = new Vector3(COLLIDER_WIDTH, 0, 0) + meshParameters.vertices[15];

            //面⑤（後側面 => Z方向）
            meshParameters.vertices[16] = endPosition + new Vector3(-LANE_WIDTH / 2, -LANE_HEIGHT / 2, 0);     //左下
            meshParameters.vertices[17] = endPosition + new Vector3(LANE_WIDTH / 2, -LANE_HEIGHT / 2, 0);      //右下
            meshParameters.vertices[18] = endPosition + new Vector3(-LANE_WIDTH / 2, LANE_HEIGHT / 2, 0);      //左上
            meshParameters.vertices[19] = endPosition + new Vector3(LANE_WIDTH / 2, LANE_HEIGHT / 2, 0);       //右上
            meshParameters.triangles[24] = 16;
            meshParameters.triangles[25] = 17;
            meshParameters.triangles[26] = 18;
            meshParameters.triangles[27] = 19;
            meshParameters.triangles[28] = 18;
            meshParameters.triangles[29] = 17;
            meshParameters.uvs[16] = new Vector2(0, 0);
            meshParameters.uvs[17] = new Vector2(1, 0);
            meshParameters.uvs[18] = new Vector2(0, 1);
            meshParameters.uvs[19] = new Vector2(1, 1);
            meshColliderParameters.vertices[16] = new Vector3(-COLLIDER_WIDTH, 0, 0) + meshParameters.vertices[16];
            meshColliderParameters.vertices[17] = new Vector3(COLLIDER_WIDTH, 0, 0) + meshParameters.vertices[17];
            meshColliderParameters.vertices[18] = new Vector3(-COLLIDER_WIDTH, 0, 0) + meshParameters.vertices[18];
            meshColliderParameters.vertices[19] = new Vector3(COLLIDER_WIDTH, 0, 0) + meshParameters.vertices[19];

            //面⑥（下底 => -Y方向）
            meshParameters.vertices[20] = startPosition + new Vector3(-LANE_WIDTH / 2, -LANE_HEIGHT / 2, 0);   //始点の左端
            meshParameters.vertices[21] = startPosition + new Vector3(LANE_WIDTH / 2, -LANE_HEIGHT / 2, 0);    //始点の右端
            meshParameters.vertices[22] = endPosition + new Vector3(-LANE_WIDTH / 2, -LANE_HEIGHT / 2, 0);     //終点の左端
            meshParameters.vertices[23] = endPosition + new Vector3(LANE_WIDTH / 2, -LANE_HEIGHT / 2, 0);      //終点の右端
            meshParameters.triangles[30] = 20;
            meshParameters.triangles[31] = 21;
            meshParameters.triangles[32] = 22;
            meshParameters.triangles[33] = 23;
            meshParameters.triangles[34] = 22;
            meshParameters.triangles[35] = 21;
            meshParameters.uvs[20] = new Vector2(0, 0);
            meshParameters.uvs[21] = new Vector2(1, 0);
            meshParameters.uvs[22] = new Vector2(0, 1);
            meshParameters.uvs[23] = new Vector2(1, 1);
            meshColliderParameters.vertices[20] = new Vector3(-COLLIDER_WIDTH, 0, 0) + meshParameters.vertices[20];
            meshColliderParameters.vertices[21] = new Vector3(COLLIDER_WIDTH, 0, 0) + meshParameters.vertices[21];
            meshColliderParameters.vertices[22] = new Vector3(-COLLIDER_WIDTH, 0, 0) + meshParameters.vertices[22];
            meshColliderParameters.vertices[23] = new Vector3(COLLIDER_WIDTH, 0, 0) + meshParameters.vertices[23];

            meshColliderParameters.triangles = meshParameters.triangles;

            #endregion

            // 頂点、三角ポリゴンのインデックス、UVを指定し、法線(normal vector)を計算
            mesh = SetMeshParameters(meshParameters.vertices, meshParameters.triangles, meshParameters.uvs);

            // 各コンポーネントにメッシュを渡す
            noteLine.GetComponent<MeshFilter>().mesh = mesh;
            noteLine.GetComponent<MeshCollider>().sharedMesh = SetMeshParameters(meshColliderParameters.vertices, meshColliderParameters.triangles);

            // コライダーを覆う場合はconvexにチェックを入れる
            // noteLine.GetComponent<MeshCollider>().convex = true;

            // 種類によって処理を分ける。
            switch (type)
            {
                case Reference.LongNoteType.NoInnerLinear:
                    noteLine.GetComponent<MeshRenderer>().material = meshMaterials.noInner;
                    break;

                case Reference.LongNoteType.AnyInnerLinear:
                    // ラインレンダラーで中心線を描画する。
                    Vector3[] linePositions = new Vector3[2] { startPosition, endPosition };
                    LineRenderer lineRenderer = longNote.AddComponent<LineRenderer>();
                    lineRenderer.SetPositions(linePositions);
                    lineRenderer.startWidth = lineRenderer.endWidth = 0.1f;
                    lineRenderer.material = meshMaterials.line;
                    lineRenderer.useWorldSpace = false;
                    noteLine.GetComponent<MeshRenderer>().material = meshMaterials.anyInner;
                    noteLine.transform.SetParent(longNote.transform);
                    break;

                case Reference.LongNoteType.NoInnerCurve:
                    // そのロングノーツのまとまりにおいて、最後となるノーツの断片を受け取ったら
                    if (isLast)
                    {
                        // メッシュの結合とテクスチャの再貼付
                        CombineFragmentMesh(longNote);
                        ReprintTexure(longNote, split, Reference.LongNoteType.NoInnerCurve);
                    }
                    break;

                case Reference.LongNoteType.AnyInnerCurve:
                    // そのロングノーツのまとまりにおいて、最後となるノーツの断片を受け取ったら
                    // 曲線の座標を収めた配列がnullでないことを確認して
                    if (isLast && curve != null)
                    {
                        // メッシュの結合とテクスチャの再貼付
                        CombineFragmentMesh(longNote);
                        ReprintTexure(longNote, split, Reference.LongNoteType.AnyInnerCurve);

                        // ラインレンダラーをコンポーネントに追加して中心線を描画する
                        LineRenderer _lineRenderer = longNote.AddComponent<LineRenderer>();
                        _lineRenderer.positionCount = curve.Length;
                        _lineRenderer.widthMultiplier = 0.1f;
                        _lineRenderer.SetPositions(curve);
                        _lineRenderer.useWorldSpace = false;
                        _lineRenderer.material = meshMaterials.line;
                        noteLine.transform.SetParent(longNote.transform);
                    }
                    break;
            }
        }

        /// <summary>
        /// ロングノーツの線を生成するためのパラメーターを計算して設定する。
        /// </summary>
        ///<param name = "startLane">ロングノーツの始点が置かれるレーン番号。</param>
        ///<param name = "startZ">ロングノーツの始点のZ座標。</param>
        ///<param name = "endLane">ロングノーツの終点が置かれるレーン番号。</param>
        ///<param name = "endZ">ロングノーツの終点のZ座標。</param>
        ///<param name = "type">ロングノーツの種類。<br/>中間点無しなら0、中間点有りなら1。</param>
        ///<param name = "index">譜面上で何番目のロングノーツか。</param>
        private void SetNotesLine(int startLane, float startZ, int endLane, float endZ, Reference.LongNoteType type, int index)
        {
            // ロングノーツの種類が大きく分けて直線型あるいは曲線型か、中間点ありかなしかを分類するローカル関数
            static (Reference.NoteType, bool) SwitchLongNoteInnerType(Reference.LongNoteType type)
            {
                Reference.NoteType baseNoteType = default;
                switch (type)
                {
                    case Reference.LongNoteType.NoInnerLinear:
                    case Reference.LongNoteType.AnyInnerLinear:
                        baseNoteType = Reference.NoteType.LongLinear;
                        break;
                    case Reference.LongNoteType.NoInnerCurve:
                    case Reference.LongNoteType.AnyInnerCurve:
                        baseNoteType = Reference.NoteType.LongCurve;
                        break;
                }

                bool flag = false;
                switch (type)
                {
                    case Reference.LongNoteType.AnyInnerLinear:
                    case Reference.LongNoteType.AnyInnerCurve:
                        flag = true;
                        break;
                    case Reference.LongNoteType.NoInnerLinear:
                    case Reference.LongNoteType.NoInnerCurve:
                        flag = false;
                        break;
                }

                return (baseNoteType, flag);
            }

            (Reference.NoteType, bool) categorizedType = SwitchLongNoteInnerType(type);

            // ロングノーツ線にコンポーネントを付与
            longNote = new GameObject($"LongNoteMesh-{index}");
            longNote.AddComponent<MeshRenderer>();
            longNote.AddComponent<MeshFilter>();
            longNote.AddComponent<MeshCollider>();
            longNote.AddComponent<LongNotes>().SetInfo(categorizedType.Item1, index, Reference.LongNoteStatus.Mesh, categorizedType.Item2);
            longNote.tag = "LongNoteMesh";
            longNote.layer = LayerMask.NameToLayer("LongNoteMesh");

            longNoteMeshList.Add(longNote);

            // レーン番号からX座標を求め、パラメーターを元にノーツの始点と終点、曲線型では制御点も計算
            Vector3 startPositon = new(LANE_GAP * LANE_WIDTH + startLane * LANE_WIDTH + LANE_WIDTH / 2, Reference.SpecialNotesPosition.y, startZ);
            Vector3 endPosition = new(LANE_GAP * LANE_WIDTH + endLane * LANE_WIDTH + LANE_WIDTH / 2, Reference.SpecialNotesPosition.y, endZ);
            Vector3 controlPositon;

            // 曲線型の場合
            if (type == Reference.LongNoteType.NoInnerCurve || type == Reference.LongNoteType.AnyInnerCurve)
            {
                // 制御点の計算
                controlPositon = new(LANE_GAP * LANE_WIDTH + endLane * LANE_WIDTH + LANE_WIDTH / 2, Reference.SpecialNotesPosition.y, (startZ + endZ) / 2);
                //int variableSplit = SPLIT_SIZE * 3;

                // ベジェ曲線から曲線上の点を計算する。
                Vector3[] curvePoints = CalculateBezierCurves(startPositon, controlPositon, endPosition, splitSize: SPLIT_SIZE);
                
                // 曲線を構成するオブジェクトの断片をつくる
                GameObject[] fragments = new GameObject[curvePoints.Length - 1];
                for (int i = 0; i < fragments.Length; i++)
                {
                    fragments[i] = new GameObject("SplittedLongNoteMesh");
                    fragments[i].AddComponent<MeshFilter>();
                    fragments[i].AddComponent<MeshRenderer>();
                    fragments[i].AddComponent<MeshCollider>();

                    // ロングノーツ１まとまりを、1つのゲームオブジェクトとして扱うために、ロングノーツ線オブジェクトの子とする
                    fragments[i].transform.SetParent(longNote.transform);
                }

                // 断片を生成する
                for (int i = 0; i < curvePoints.Length - 1; i++)
                {
                    // ロングノーツ１まとまりにおいて、最後の断片
                    if (i == curvePoints.Length - 2) { GenerateLongNoteMesh(curvePoints[i], curvePoints[i + 1], fragments[i], type, split: SPLIT_SIZE, curve: curvePoints, isLast: true); }
                    else { GenerateLongNoteMesh(curvePoints[i], curvePoints[i + 1], fragments[i], type, split: SPLIT_SIZE, curve: curvePoints); }
                }
            }
            // 直線型の場合（曲線の座標も渡さない）
            else { GenerateLongNoteMesh(startPositon, endPosition, longNote, type); }
        }

        /// <summary>
        /// 各ロングノーツの点の座標を計算して設定する。
        /// </summary>
        public override void GenerateNotes()
        {
            // JSONの譜面データからロングノーツの情報を取得したとき、それらの情報は、ロングノーツ１まとまり単位でリストに格納する仕様にしている
            // このメソッドは、そんな各ロングノーツ１まとまりを生成するタイミングで使用するため、
            // 随時情報が追加されていくlaneNumbersの１次元目のカウントが分かれば、今どのロングノーツを生成すれば良いかが分かる（※のように）
            int calculatingListIndex = laneNumbers.Count - 1;
            
            // 算出するX座標とZ座標
            float positionX = 0;
            float positionZ = 0;

            // Instanciateするノー
            GameObject note;

            // 中間点のリスト
            List<GameObject> inners = new();

            // 現在計算中のロングノーツのインデックスに対応した情報をリストたちから参照するようにする（※）
            for (int i = calculatingListIndex; i < laneNumbers.Count; i++)
            {
                // ロングノーツひとまとまりに対応した二次元配列を使って座標を計算する
                float[,] _positionX = new float[laneNumbers.Count, laneNumbers[i].Count];
                float[,] _positionZ = new float[laneNumbers.Count, laneNumbers[i].Count];
                inners.Clear();
                for (int j = 0; j < laneNumbers[i].Count; j++)
                {
                    positionX = SwitchNoteLane(laneNumbers[i][j]);
                    positionZ = notesTimes[i][j] * GameManager.instance.NoteSpeed + Reference.Origin.z;
                    _positionX[i, j] = positionX;
                    _positionZ[i, j] = positionZ;

                    // 直線型で中間点のないノーツ
                    if (innerNotesCounts[i] == 0 && notesTypes[i] == 2)
                    {
                        note = Instantiate(notesGenerator.notePrefabs.longOnly, new(positionX, Reference.SpecialNotesPosition.y, positionZ), Quaternion.identity, noteObjectParent);
                        LongNotes prop = note.GetComponent<LongNotes>();

                        if (GameManager.instance.AutoPlay || (!GameManager.instance.AutoPlay && j == 0)) { notesGenerator.notesObjects.Add(note); }
                        if ((/*!GameManager.instance.AutoPlay &&*/ j > 0))
                        {
                            inners.Add(note);
                            if (j == laneNumbers[i].Count - 1) { innerNotes.Add(i, inners); }
                        }

                        if (j == 0) { prop.status = Reference.LongNoteStatus.Start; }
                        else if (j == laneNumbers[i].Count - 1) { prop.status = Reference.LongNoteStatus.End; }
                        else { prop.status = Reference.LongNoteStatus.Inner; }

                        prop.type = Reference.NoteType.LongLinear;
                        prop.index = i;

                        if (_positionZ[i, _positionZ.GetLength(1) - 1] > 0)
                        {
                            SetNotesLine(GetStartAndEndElements(laneNumbers[i])[0], _positionZ[i, 0], GetStartAndEndElements(laneNumbers[i])[1], _positionZ[i, _positionZ.GetLength(1) - 1], Reference.LongNoteType.NoInnerLinear, i);
                        }
                    }
                    // 曲線型で中間点のないノーツ
                    else if (innerNotesCounts[i] == 0 && notesTypes[i] == 3)
                    {
                        note = Instantiate(notesGenerator.notePrefabs.longOnly, new(positionX, Reference.SpecialNotesPosition.y, positionZ), Quaternion.identity, noteObjectParent);
                        LongNotes prop = note.GetComponent<LongNotes>();

                        if (GameManager.instance.AutoPlay || (!GameManager.instance.AutoPlay && j == 0)) { notesGenerator.notesObjects.Add(note); }
                        if ((/*!GameManager.instance.AutoPlay &&*/ j > 0))
                        {
                            inners.Add(note);
                            if (j == laneNumbers[i].Count - 1) { innerNotes.Add(i, inners); }
                        }

                        if (j == 0) { prop.status = Reference.LongNoteStatus.Start; }
                        else if (j == laneNumbers[i].Count - 1) { prop.status = Reference.LongNoteStatus.End; }
                        else { prop.status = Reference.LongNoteStatus.Inner; }

                        prop.type = Reference.NoteType.LongCurve;
                        prop.index = i;

                        if (_positionZ[i, _positionZ.GetLength(1) - 1] > 0)
                        {
                            SetNotesLine(GetStartAndEndElements(laneNumbers[i])[0], _positionZ[i, 0], GetStartAndEndElements(laneNumbers[i])[1], _positionZ[i, _positionZ.GetLength(1) - 1], Reference.LongNoteType.NoInnerCurve, i);
                        }
                    }
                    // 直線型で中間点のあるノーツ
                    else if (innerNotesCounts[i] != 0 && notesTypes[i] == 2)
                    {
                        note = Instantiate(notesGenerator.notePrefabs.longAny, new(positionX, Reference.SpecialNotesPosition.y, positionZ), Quaternion.identity, noteObjectParent);
                        LongNotes prop = note.GetComponent<LongNotes>();

                        if (GameManager.instance.AutoPlay || (!GameManager.instance.AutoPlay && j == 0)) { notesGenerator.notesObjects.Add(note); }
                        if ((/*!GameManager.instance.AutoPlay &&*/ j > 0))
                        {
                            inners.Add(note);
                            if (j == laneNumbers[i].Count - 1) { innerNotes.Add(i, inners); }
                        }

                        if (j == 0) { prop.status = Reference.LongNoteStatus.Start; }
                        else if (j == laneNumbers[i].Count - 1) { prop.status = Reference.LongNoteStatus.End; }
                        else { prop.status = Reference.LongNoteStatus.Inner; }

                        prop.type = Reference.NoteType.LongLinear;
                        prop.index = i;

                        if (j > 0) { SetNotesLine(laneNumbers[i][j - 1], _positionZ[i, j - 1], laneNumbers[i][j], _positionZ[i, j], Reference.LongNoteType.AnyInnerLinear, i); }
                    }
                    // 曲線型で中間点のあるノーツ
                    else if (innerNotesCounts[i] != 0 && notesTypes[i] == 3)
                    {
                        note = Instantiate(notesGenerator.notePrefabs.longAny, new(positionX, Reference.SpecialNotesPosition.y, positionZ), Quaternion.identity, noteObjectParent);
                        LongNotes prop = note.GetComponent<LongNotes>();

                        if (GameManager.instance.AutoPlay || (!GameManager.instance.AutoPlay && j == 0)) { notesGenerator.notesObjects.Add(note); }
                        if ((/*!GameManager.instance.AutoPlay &&*/ j > 0))
                        {
                            inners.Add(note);
                            if (j == laneNumbers[i].Count - 1) { innerNotes.Add(i, inners); }
                        }

                        if (j == 0) { prop.status = Reference.LongNoteStatus.Start; }
                        else if (j == laneNumbers[i].Count - 1) { prop.status = Reference.LongNoteStatus.End; }
                        else { prop.status = Reference.LongNoteStatus.Inner; }

                        prop.type = Reference.NoteType.LongCurve;
                        prop.index = i;
                        if (j > 0) { SetNotesLine(laneNumbers[i][j - 1], _positionZ[i, j - 1], laneNumbers[i][j], _positionZ[i, j], Reference.LongNoteType.AnyInnerCurve, i); }
                    }
                }
            }
        }

        /// <summary>
        /// 曲線型ロングノーツのメッシュ座標をベジェ曲線として計算する。
        /// </summary>
        /// <remarks>
        /// https://iconcreator.hatenablog.com/entry/2021/09/13/190000
        /// </remarks>
        /// <param name = "startPosition">ロングノーツの始点。</param>
        /// <param name = "controlPosition">ロングノーツを曲げる制御点。</param>
        /// <param name = "endPosition">ロングノーツの終点。</param>
        /// <param name = "splitSize">曲線生成時の分割数。値が多い程滑らかな曲線になります。</param>
        /// <returns>
        /// 曲線をつくる点の座標。
        /// </returns>
        private Vector3[] CalculateBezierCurves(Vector3 startPositon, Vector3 controlPositon, Vector3 endPositon, int splitSize = 10)
        {
            // ベジェ曲線の点を分割するときに値を変化させる比
            float t = 0;

            // 曲線上の点の座標を格納する配列
            Vector3[] curvePoints = new Vector3[splitSize + 1];

            // 曲線上の点を求める
            for (int i = 0; i <= splitSize; i++)
            {
                // 現在の分割回数と、最終的な分割数を割ったものが比であるtの値
                t = (float)i / splitSize;

                // 分割した点a, bを線形補間によって繰り返し計算する
                Vector3 a = Vector3.Lerp(startPositon, controlPositon, t);
                Vector3 b = Vector3.Lerp(controlPositon, endPositon, t);

                // 2つの分割点をもとにさらに分割する
                curvePoints[i] = Vector3.Lerp(a, b, t);
            }

            return curvePoints;
        }

        /// <summary>
        /// 曲線型ロングノーツを構成するメッシュの断片を一つのメッシュとして合成し、親オブジェクト（ロングノーツ１まとまり）のメッシュとして置き換える。
        /// </summary>
        /// <remarks>
        /// コライダーは合成しない（できない）
        /// </remarks>
        /// <param name = "parent">
        /// メッシュを合成するときに使う親オブジェクト（ロングノーツ１まとまりに相当）
        /// </param>
        private void CombineFragmentMesh(GameObject parent)
        {
            // 断片のメッシュを全取得
            List<MeshFilter> meshFilters = new();
            meshFilters = parent.GetComponentsInChildren<MeshFilter>().Where(fragmentFilter => fragmentFilter.gameObject != parent).ToList();

            // メッシュを合成する
            CombineInstance[] combineInstances = new CombineInstance[meshFilters.Count];
            for (int i = 0; i < meshFilters.Count; i++)
            {
                combineInstances[i].mesh = meshFilters[i].sharedMesh;
                combineInstances[i].transform = meshFilters[i].transform.localToWorldMatrix;
            }

            // 合成したメッシュを生成する
            parent.GetComponent<MeshFilter>().mesh.CombineMeshes(combineInstances);

            // 結合後の子オブジェクト（断片）の処理
            // マテリアルで透明にしておくにとどめる(Destroyしたらコライダーが結合できていないままなので当たり判定が消える)
            meshFilters.ForEach(fragment => fragment.gameObject.GetComponent<MeshRenderer>().material = meshMaterials.fadeMaterial);
        }

        /// <summary>
        /// 結合した後のロングノーツのメッシュのUV座標を再計算してテクスチャを貼り直す。
        /// </summary>
        /// <param name = "longNoteObject">結合させたメッシュを適用したオブジェクト。</param>
        /// <param name = "split">分割数。</param>
        private void ReprintTexure(GameObject longNoteObject, float split, Reference.LongNoteType type)
        {
            // 結合後のメッシュの頂点数を取得する
            int verticesLength = longNoteObject.GetComponent<MeshFilter>().mesh.vertices.Length;

            // UVを再計算する
            Vector2[] uvs = new Vector2[verticesLength];
            for (int i = 0; i < uvs.Length; i++)
            {
                /* 以下：メッシュの上底（Y面）のUVを設定する。 */
                //四角形左下の絞り込み。
                if (i == 0 || (i % 4 == 0 && i % 24 == 0))
                {
                    uvs[i] = new Vector2(0, Mathf.Abs(i / 24 / (float)split));
                    continue;
                }
                //四角形右下の絞り込み。
                else if (i == 1 || ((i - 1) % 4 == 0 && (i - 1) % 24 == 0))
                {
                    uvs[i] = new Vector2(1, Mathf.Abs((i - 1) / 24 / (float)split));
                    continue;
                }
                //四角形左上の絞り込み。
                else if (i == 2 || ((i - 2) % 4 == 0 && (i - 2) % 24 == 0))
                {
                    uvs[i] = new Vector2(0, Mathf.Abs(i / 24 / (float)split + 1f / (float)split));
                    continue;
                }
                //四角形右上の絞り込み。
                else if (i == 3 || ((i - 3) % 4 == 0 && (i - 3) % 24 == 0))
                {
                    uvs[i] = new Vector2(1, Mathf.Abs((i - 1) / 24 / (float)split + 1f / (float)split));
                    continue;
                }
                /* 以下：メッシュの上底以外のUVを設定する。見ようにも見えない部分なので適当。 */
                else if (i % 4 == 0)
                {
                    uvs[i] = new Vector2(0, 0);
                    continue;
                }
                else if ((i - 1) % 4 == 0)
                {
                    uvs[i] = new Vector2(1, 0);
                    continue;
                }
                else if ((i - 2) % 4 == 0)
                {
                    uvs[i] = new Vector2(0, 1);
                    continue;
                }
                else if (((i - 3) % 4 == 0))
                {
                    uvs[i] = new Vector2(1, 1);
                    continue;
                }
            }

            // UVを適用
            longNoteObject.GetComponent<MeshFilter>().mesh.uv = uvs;

            // テクスチャを再貼付
            switch (type)
            {
                case Reference.LongNoteType.NoInnerCurve:
                    longNoteObject.GetComponent<MeshRenderer>().material = meshMaterials.noInner;
                    break;
                case Reference.LongNoteType.AnyInnerCurve:
                    longNoteObject.GetComponent<MeshRenderer>().material = meshMaterials.anyInner;
                    break;
            }
        }

        /// <summary>
        /// 中間点有りのロングノーツの生成時、中間点どうしの間で各々生成されてしまうロングノーツの断片を、
        /// 譜面のロングノーツの順番ごとに1つの親オブジェクトに関連付けて、ロングノーツ１まとまりとする。
        /// </summary>
        private void SetLongNoteTransform()
        {
            if (longNoteMeshList.Count == 0) { return; }

            // 各ロングノーツに設定された、流れてくる順番を入れるリストをつくり、各Ｌノーツから参照する
            List<int> meshIndexList = longNoteMeshList.Select(mesh => mesh.GetComponent<LongNotes>().index).ToList();

            //ロングノーツのまとまりの個数を取得するために、Ｌノーツの最後のインデックスを取得する。(これは0から始まるインデックス番号なので実際はこれに+1した個数)
            int maxNumberOfLongNotes = meshIndexList.Max();

            // リストの中から重複しているインデックスとその重複数を抽出する。
            //　1次元目：重複しているインデックス　2次元目：その重複数
            Dictionary<int, int> duplicatesIndex = meshIndexList.GroupBy(x => x).Where(x => x.Count() > 1).ToDictionary(x => x.Key, y => y.Count());
            // リストの中から1つしかないインデックスと実際に格納されたリストのなかでのインデックス番号を抽出する。
            //　1次元目：1つしかないインデックス　2次元目：meshIndexList中でのそのインデックス番号
            Dictionary<int, int> uniquesIndex = meshIndexList.GroupBy(x => x).Where(x => x.Count() == 1).ToDictionary(x => x.Key, y => meshIndexList.IndexOf(y.Key));

            // 新たにつくる親オブジェクトをひとまとめにするLノーツ(インデックスの重複があるＬノーツ)分だけ確保する。
            GameObject[] parents = new GameObject[duplicatesIndex.Count];

            // 親オブジェクトはインデックスをその名前に入れて生成
            parents = duplicatesIndex.Select(item => new GameObject($"LongNoteMesh-{item.Key}")).ToArray();

            // 複数あるLノーツのインデックスを記録した配列
            int[] duplicateIndexKeys = duplicatesIndex.Keys.ToArray();

            // インデックスが重複した中間点有りLノーツの欠片を入れ子構造にして、インデックスごとにひとまとまりのオブジェクトができるよう調整
            for (int j = 0; j < duplicateIndexKeys.Length; j++)
            {
                // 親にはコンポーネントを新しく設定する
                parents[j].AddComponent<LongNotes>().SetInfo(Reference.NoteType.LongLinear, duplicateIndexKeys[j], Reference.LongNoteStatus.Mesh, true);
                parents[j].AddComponent<MeshCollider>();
                for (int k = 0; k < meshIndexList.Count; k++)
                {
                    if (longNoteMeshList[k].GetComponent<LongNotes>().index == duplicateIndexKeys[j])
                    {
                        // 入れ子にする欠片の方はコンポーネントを削除する
                        longNoteMeshList[k].transform.SetParent(parents[j].transform);
                        // 入れ子にした断片がさらに子オブジェクトを持っているようならそれは曲線型
                        if (longNoteMeshList[k].transform.childCount > 0)
                        {
                            parents[j].GetComponent<LongNotes>().SetInfo(Reference.NoteType.LongCurve, duplicateIndexKeys[j], Reference.LongNoteStatus.Mesh, true);
                        }
                        Destroy(longNoteMeshList[k].GetComponent<Notes>());
                        Destroy(longNoteMeshList[k].GetComponent<LongNotes>());
                    }
                }
            }

            // 新しいリストは順番で代入できるように一旦配列で作成
            GameObject[] newMeshArray = new GameObject[maxNumberOfLongNotes + 1];

            // 1つしかないLノーツインデックスをもつメッシュの配列たち
            // 元のリストの順番におけるインデックス番号
            int[] uniqueValues = uniquesIndex.Values.ToArray();

            // 実際のLノーツインデックス
            int[] uniqueKeys = uniquesIndex.Keys.ToArray();

            //新しいリスト（配列）に、Lノーツインデックス順にメッシュを追加。
            for (int l = 0, n = 0, m = 0; l < longNoteMeshList.Count; l++)
            {
                try
                {
                    if (l == duplicateIndexKeys[n] && n < duplicateIndexKeys.Length)
                    {
                        newMeshArray[l] = parents[n];
                        if (n < duplicateIndexKeys.Length - 1) { n++; }
                    }
                }
                catch (System.IndexOutOfRangeException) { }

                try
                {
                    if (l == uniqueValues[m])
                    {
                        newMeshArray[uniqueKeys[m]] = longNoteMeshList[uniqueValues[m]];
                        if (m < uniqueValues.Length - 1) { m++; }
                    }
                }
                catch (System.IndexOutOfRangeException) { }
            }

            // 新しいリストへ変更を反映する
            longNoteMeshList.Clear();
            longNoteMeshList = newMeshArray.ToList();
            /*
            longNoteMeshList.Reverse();
            foreach (GameObject item in longNoteMeshList)
            {
                item.SetLayerSelfChildren(LayerMask.NameToLayer("LongNoteMesh"));
                longNotesList.Add(item.GetComponent<LongNotes>());
            }
            notesObjects = innerNotes.Values.ToList();
            notesObjects.Reverse();
            foreach (var l in notesObjects) { l.Reverse(); }*/
        }

        public override void NotesSort()
        {
            SetLongNoteTransform();

            // インデックスを降順にソートし直したり、リストの中身を求め直す
            longNoteMeshList.Reverse();
            foreach (var item in longNoteMeshList)
            {
                item.SetLayerSelfChildren(LayerMask.NameToLayer("LongNoteMesh"));
                longNotesList.Add(item.GetComponent<LongNotes>());
            }
            notesObjects = innerNotes.Values.ToList();
            notesObjects.Reverse();
            notesObjects.ForEach(notes => notes.Reverse());
        }

        /// <summary>
        /// メッシュにポリゴンの頂点と、そのインデックスを指定して法線を計算する。
        /// </summary>
        /// <param name="vertices">頂点</param>
        /// <param name="triangles">頂点のインデックス</param>
        /// <returns>パラメータを指定した新しいメッシュ</returns>
        private Mesh SetMeshParameters(Vector3[] vertices, int[] triangles)
        {
            Mesh mesh = new()
            {
                vertices = vertices,
                triangles = triangles
            };
            mesh.RecalculateNormals();
            return mesh;
        }

        /// <summary>
        /// メッシュにポリゴンの頂点と、そのインデックスと、UV座標を指定して法線を計算する。
        /// </summary>
        /// <param name="vertices">頂点</param>
        /// <param name="triangles">頂点のインデックス</param>
        /// <param name="uvs">UV座標</param>
        /// <returns>パラメータを指定した新しいメッシュ</returns>
        private Mesh SetMeshParameters(Vector3[] vertices, int[] triangles, Vector2[] uvs)
        {
            Mesh mesh = new()
            {
                vertices = vertices,
                triangles = triangles,
                uv = uvs
            };
            mesh.RecalculateNormals();
            return mesh;
        }

        /// <summary>
        /// リストの最初と最後の要素を抽出する。
        /// </summary>
        /// <remarks>
        /// ロングノーツの始点と終点のパラメーターを抽出するときに使う。
        /// </remarks>
        /// <typeparam name = "T">いずれかの型。</typeparam>
        /// <param name = "list">いずれかの型のリスト。</param>
        /// <returns>
        /// いずれかの型のリストの最初、最終要素の二つを含んだ配列。
        /// </returns>
        private T[] GetStartAndEndElements<T>(List<T> list)
        {
            //返す配列。
            T[] startAndEnd = new T[2];
            //始点と終点を代入。
            startAndEnd[0] = list[0];
            startAndEnd[1] = list[list.Count - 1];
            return startAndEnd;
        }

        #endregion
    }
}
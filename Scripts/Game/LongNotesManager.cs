/* 名前空間 */
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
//自作名前空間
using Game.Utility;

///<summary>ロングノーツ生成を行うクラスです。</summary>
public class LongNotesManager : UtilityBase
{
    /* フィールド */
    ///<summary>レーン番号を格納するリスト。</summary>
    ///<remarks>ロングノーツのリストの構造上、ロングノーツのまとまりの生成順数を1次元目とし、ロングノーツ内のノーツの情報を2次元目に格納するリストになっています。</remarks>
    public List<List<int>> laneNum = new List<List<int>>();
    ///<summary>ノーツの到達時間を格納するリスト。</summary>
    ///<remarks>ロングノーツのリストの構造上、ロングノーツのまとまりの生成順数を1次元目とし、ロングノーツ内のノーツの情報を2次元目に格納するリストになっています。</remarks>
    public List<List<float>> notesTime = new List<List<float>>();
    ///<summary>各ロングノーツごとの種類をインデックス順に格納するリスト。</summary>
    public List<int> notesType = new List<int>();
    ///<summary>各ロングノーツごとの中間点の数をインデックス順に格納するリスト。</summary>
    public List<int> innnerNotesNum = new List<int>();
    ///<summary>ロングノーツの中間点の辞書。</summary>
    ///<remarks>KeyにLノーツインデックス、ValueにそのインデックスのLノーツが含む中間ノーツのリストを設定する。</remarks>
    public Dictionary<int, List<GameObject>> innerNotes = new Dictionary<int, List<GameObject>>();
    ///<summary>各ロングノーツごとの中間点の二次元リスト。</summary>
    ///<remarks><see cref = "innerNotes"/>のValueを変換して使う。</remarks>
    public List<List<GameObject>> innerNotesList = new List<List<GameObject>>();
    ///<summary>ロングノーツの太さ。</summary>
    private const float LANE_WIDTH = 2.0f;
    ///<summary>ロングノーツの高さ。</summary>
    private const float LANE_HEIGHT = 0.001f;
    ///<summary>ロングノーツを生成するときに頂点の生成位置をレーンに合わせるための差分。</summary>
    private const float LANEGAP = -3f;
    private const int SPLIT_SIZE = 10;
    ///<summary><see cref = "NotesManager"/>.</summary>
    [SerializeField] private NotesManager notesManager;
    ///<summary>生成するロングノーツ線のゲームオブジェクト。</summary>
    private GameObject longNote;
    ///<summary>ロングノーツ線のリスト。</summary>
    public List<GameObject> longNoteMeshList = new List<GameObject>();
    ///<summary>ロングノーツ線に紐づいた<see cref = "LongNotes"/>コンポーネントのリスト。</summary>
    ///<remarks>ロングノーツの長押し判定をEventSystemでとってくれる。</remarks>
    public List<LongNotes> longNotesList = new List<LongNotes>();
    ///<summary>ロングノーツ線のメッシュに適用する色々なマテリアル。</summary>
    [System.Serializable]
    private struct MeshMaterials
    {
        ///<summary>中間点無し。</summary>
        [SerializeField] public Material noInner;
        ///<summary>中間点有り。</summary>
        [SerializeField] public Material anyInner;
        ///<summary>レンダリングモードがFadeのマテリアル。</summary>
        [SerializeField] public Material fadeRendering;
        ///<summary>中間点有りノーツに描画する中間線。</summary>
        [SerializeField] public Material line;
    }
    ///<summary>ロングノーツ線にアタッチするマテリアル。</summary>
    [SerializeField] private MeshMaterials meshMaterials = new MeshMaterials();
    ///<summary>ロングノーツ線のメッシュ生成に用いる要素。</summary>
    private struct MeshParameters
    {
        ///<summary>メッシュの頂点座標。</summary>
        public Vector3[] vertices;
        ///<summary>三角ポリゴンの頂点の数。</summary>
        public int[] triangles;
        ///<summary>テクスチャのUV座標。</summary>
        public Vector2[] uvs;
    }
    private enum LongNoteType
    {
        NoInnerLinear, AnyInnerLinear, NoInnerCurve, AnyInnerCurve
    }

    //void Start() => CombineLongNoteTransform();

    ///<summary>ロングノーツ線のメッシュを生成します。</summary>
    ///<param name = "startPosition">ロングノーツの始点座標。</param>
    ///<param name = "endPosition">ロングノーツの終点座標。</param>
    ///<param name = "noteLine">ロングノーツ線のゲームオブジェクト。</param>
    ///<param name = "type">ロングノーツの種類。<br/>中間点無しなら0、中間点有りなら1。</param>
    ///<param name = "curve">曲線を作る点の座標。</param>
    ///<param name = "split">曲線型ロングノーツの分割数。</param>
    ///<param name = "isLast">分割したロングノーツの生成時、その対象が最後の分割ノーツだったときtrueを指定する。</param>
    private void Generate(Vector3 startPosition, Vector3 endPosition, GameObject noteLine, int type, Vector3[] curve = null, int split = 10, bool isLast = false)
    {
        //生成するメッシュ本体。
        Mesh mesh = new Mesh();
        //メッシュの描画に必要な要素の設定。
        MeshParameters meshParameters = new MeshParameters();
        meshParameters.vertices = new Vector3[4 * 6];
        meshParameters.triangles = new int[6 * 6];
        meshParameters.uvs = new Vector2[4 * 6];

        //面①（ワールド原点からZ座標を正の方向に見たときの、上底 => Y方向）
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
        //面②（前側面 => -Z方向）
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
        //面③（左側面 => -X方向）
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
        //面④（右側面 => X方向）
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

        //頂点、三角ポリゴンのインデックス、UVを指定。
        mesh.vertices = meshParameters.vertices;
        mesh.triangles = meshParameters.triangles;
        mesh.uv = meshParameters.uvs;
        //法線(normal vector)を計算。
        mesh.RecalculateNormals();
        //各コンポーネントにメッシュを渡す。
        noteLine.GetComponent<MeshFilter>().mesh = mesh;
        noteLine.GetComponent<MeshCollider>().sharedMesh = mesh;
        //コライダーを覆う。
        noteLine.GetComponent<MeshCollider>().convex = true;
        //種類によって処理を分ける。
        switch (type)
        {
            case (int)LongNoteType.NoInnerLinear:
                //直線型で中間点無し。
                noteLine.GetComponent<MeshRenderer>().material = meshMaterials.noInner;
                break;
            case (int)LongNoteType.AnyInnerLinear:
                //直線型で中間点有り。
                //ラインレンダラーで中心線を描画する。
                Vector3[] linePositions = new Vector3[2] { startPosition, endPosition };
                LineRenderer lineRenderer = longNote.AddComponent<LineRenderer>();
                lineRenderer.SetPositions(linePositions);
                lineRenderer.startWidth = lineRenderer.endWidth = 0.1f;
                lineRenderer.material = meshMaterials.line;
                lineRenderer.useWorldSpace = false;
                noteLine.GetComponent<MeshRenderer>().material = meshMaterials.anyInner;
                noteLine.transform.SetParent(longNote.transform);
                break;
            case (int)LongNoteType.NoInnerCurve:
                //曲線型で中間点無し。
                if (isLast)
                //最後のノーツ断片を受け取ったら。
                {
                    //メッシュの結合とテクスチャの再貼付。
                    CombineMeshInChildren(longNote);
                    ReprintTexure(longNote, split, (int)LongNoteType.NoInnerCurve);
                }
                break;
            case (int)LongNoteType.AnyInnerCurve:
                //曲線型で中間点有り。
                if (isLast && curve != null)
                {
                    //メッシュの結合とテクスチャの再貼付。
                    CombineMeshInChildren(longNote);
                    ReprintTexure(longNote, split, (int)LongNoteType.AnyInnerCurve);
                    //ラインレンダラーをコンポーネントに追加して中心線を描画する。
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
    ///<summary>ロングノーツの線を生成するためのパラメーターを設定します。</summary>
    ///<param name = "startLane">ロングノーツの始点が置かれるレーン番号。</param>
    ///<param name = "startZ">ロングノーツの始点のZ座標。</param>
    ///<param name = "endLane">ロングノーツの終点が置かれるレーン番号。</param>
    ///<param name = "endZ">ロングノーツの終点のZ座標。</param>
    ///<param name = "type">ロングノーツの種類。<br/>中間点無しなら0、中間点有りなら1。</param>
    ///<param name = "index">譜面上で何番目のロングノーツか。</param>
    private void SetNotesLine(int startLane, float startZ, int endLane, float endZ, int type, int index)
    {
        //ロングノーツ線にコンポーネントを付与。
        longNote = new GameObject("LongNoteMesh");
        longNote.AddComponent<MeshRenderer>();
        longNote.AddComponent<MeshFilter>();
        longNote.AddComponent<MeshCollider>();
        longNote.AddComponent<Notes>();
        longNote.AddComponent<LongNotes>();
        longNote.tag = "LongNoteMesh";
        longNote.layer = LayerMask.NameToLayer("LongNoteMesh");
        longNote.GetComponent<Notes>().longNote = new Notes.LongNote();
        longNote.GetComponent<Notes>().longNote.status = SettingUtility.LongNoteStatus.Mesh;
        longNote.GetComponent<Notes>().longNote.index = index;
        longNote.GetComponent<Notes>().longNote.isInner = SwitchLongNoteInnerType(type);
        longNoteMeshList.Add(longNote);
        //レーン番号からX座標を求め、パラメーターを元にノーツの始点、終点、曲線型では制御点を計算。
        Vector3 startPositon = new Vector3(LANEGAP * LANE_WIDTH + startLane * LANE_WIDTH + LANE_WIDTH / 2, SettingUtility.specialNotesPosition.y, startZ);
        Vector3 endPosition = new Vector3(LANEGAP * LANE_WIDTH + endLane * LANE_WIDTH + LANE_WIDTH / 2, SettingUtility.specialNotesPosition.y, endZ);
        Vector3 controlPositon;

        //曲線型であった場合。
        if (type == (int)LongNoteType.NoInnerCurve || type == (int)LongNoteType.AnyInnerCurve)
        {
            //制御点の計算。
            controlPositon = new Vector3(LANEGAP * LANE_WIDTH + endLane * LANE_WIDTH + LANE_WIDTH / 2, SettingUtility.specialNotesPosition.y, (startZ + endZ) / 2);
            //int variableSplit = SPLIT_SIZE * 3;
            //ベジェ曲線から曲線上の点を計算する。
            Vector3[] curvePoints = SetBezierCurves(startPositon, controlPositon, endPosition, splitSize: SPLIT_SIZE);
            //曲線を構成するメッシュの断片をつくる。
            GameObject[] parts = new GameObject[curvePoints.Length - 1];
            for (int i = 0; i < parts.Length; i++)
            {
                parts[i] = new GameObject("SplittedLongNoteMesh");
                parts[i].AddComponent<MeshFilter>();
                parts[i].AddComponent<MeshRenderer>();
                parts[i].AddComponent<MeshCollider>();
                //1つのゲームオブジェクトとして扱うために、ロングノーツ線オブジェクトの子として関連付けておく。
                parts[i].transform.SetParent(longNote.transform);
            }
            //断片を生成する。
            for (int i = 0; i < curvePoints.Length - 1; i++)
            {
                if (i == curvePoints.Length - 2)
                {
                    //最後の断片。
                    Generate(curvePoints[i], curvePoints[i + 1], parts[i], type, split: SPLIT_SIZE, curve: curvePoints, isLast: true);
                }
                else
                {
                    Generate(curvePoints[i], curvePoints[i + 1], parts[i], type, split: SPLIT_SIZE, curve: curvePoints);
                }
            }
        }
        else
        {
            //メッシュの生成に値を渡す。
            Generate(startPositon, endPosition, longNote, type);
        }
    }
    ///<summary>各ロングノーツの点の座標を計算します。</summary>
    ///<param name = "calculatingListIndex"><see cref = "NotesManager.LoadJSON(string)"/>でロングノーツを取得する際に使っているリストの要素数から1引いたものを代入する。
    ///<br/><see cref = "NotesManager"/>でJSON構造に基づいてロングノーツを取得したとき、それらのパラメーターはひとまとまりのロングノーツごとにリストに格納する。それをこのクラスの<see cref = "laneNum"/>や<see cref = "notesTime"/>に
    ///     二次元リストの形でさらに格納させるが、このメソッドは、前述の「ひとまとまり」ごとに呼び出されるためリストの参照の仕方を工夫する必要がある。
    ///<br/>そこでこのパラメーターを用い、<see cref = "NotesManager"/>で計算中のロングノーツのまとまりにあわせてリストを参照し座標計算の手間を減らしている。
    ///</param>
    public void SetPosition(int calculatingListIndex)
    {
        //算出するX座標とZ座標。
        float positionX = 0;
        float positionZ = 0;
        //Instanciateするノーツ。
        GameObject note = new GameObject();
        //中間点のリスト。
        List<GameObject> inners = new List<GameObject>();
        for (int i = calculatingListIndex; i < laneNum.Count; i++)
        {
            //ロングノーツひとまとまりに対応した二次元配列を使って座標を計算する。
            float[,] _positionX = new float[laneNum.Count, laneNum[i].Count];
            float[,] _positionZ = new float[laneNum.Count, laneNum[i].Count];
            inners.Clear();
            for (int j = 0; j < laneNum[i].Count; j++)
            {
                positionX = SwitchNoteLane(laneNum[i][j]);
                positionZ = notesTime[i][j] * GameManager.instance.noteSpeed + SettingUtility.origin.z;
                _positionX[i, j] = positionX;
                _positionZ[i, j] = positionZ;
                //直線型で中間点のないノーツ。
                if (innnerNotesNum[i] == 0 && notesType[i] == 2)
                {
                    note = Instantiate(notesManager.noteObject.longOnly, new Vector3(positionX, SettingUtility.specialNotesPosition.y, positionZ), Quaternion.identity);
                    Notes prop = note.GetComponent<Notes>();

                    if (GameManager.instance.autoPlay || (!GameManager.instance.autoPlay && j == 0)) { notesManager.notesObjects.Add(note); }
                    if ((/*!GameManager.instance.AutoPlay &&*/ j > 0))
                    {
                        inners.Add(note);
                        if (j == laneNum[i].Count - 1) { innerNotes.Add(i, inners); }
                    }

                    if (j == 0) { prop.longNote.status = SettingUtility.LongNoteStatus.Start; }
                    else if (j == laneNum[i].Count - 1) { prop.longNote.status = SettingUtility.LongNoteStatus.End; }
                    else { prop.longNote.status = SettingUtility.LongNoteStatus.Inner; }

                    prop.type = SettingUtility.NoteType.LongLinear;
                    prop.longNote.index = i;

                    if (_positionZ[i, _positionZ.GetLength(1) - 1] > 0)
                    {
                        SetNotesLine(GetStartAndEndElements(laneNum[i])[0], _positionZ[i, 0], GetStartAndEndElements(laneNum[i])[1], _positionZ[i, _positionZ.GetLength(1) - 1], (int)LongNoteType.NoInnerLinear, i);
                    }
                }
                //曲線型で中間点のないノーツ。
                else if (innnerNotesNum[i] == 0 && notesType[i] == 3)
                {
                    note = Instantiate(notesManager.noteObject.longOnly, new Vector3(positionX, SettingUtility.specialNotesPosition.y, positionZ), Quaternion.identity);
                    Notes prop = note.GetComponent<Notes>();

                    if (GameManager.instance.autoPlay || (!GameManager.instance.autoPlay && j == 0)) { notesManager.notesObjects.Add(note); }
                    if ((/*!GameManager.instance.AutoPlay &&*/ j > 0))
                    {
                        inners.Add(note);
                        if (j == laneNum[i].Count - 1) { innerNotes.Add(i, inners); }
                    }

                    if (j == 0) { prop.longNote.status = SettingUtility.LongNoteStatus.Start; }
                    else if (j == laneNum[i].Count - 1) { prop.longNote.status = SettingUtility.LongNoteStatus.End; }
                    else { prop.longNote.status = SettingUtility.LongNoteStatus.Inner; }

                    prop.type = SettingUtility.NoteType.LongCurve;
                    prop.longNote.index = i;

                    if (_positionZ[i, _positionZ.GetLength(1) - 1] > 0)
                    {
                        SetNotesLine(GetStartAndEndElements(laneNum[i])[0], _positionZ[i, 0], GetStartAndEndElements(laneNum[i])[1], _positionZ[i, _positionZ.GetLength(1) - 1], (int)LongNoteType.NoInnerCurve, i);
                    }
                }
                //直線型で中間点のあるノーツ。
                else if (innnerNotesNum[i] != 0 && notesType[i] == 2)
                {
                    note = Instantiate(notesManager.noteObject.longAny, new Vector3(positionX, SettingUtility.specialNotesPosition.y, positionZ), Quaternion.identity);
                    Notes prop = note.GetComponent<Notes>();

                    if (GameManager.instance.autoPlay || (!GameManager.instance.autoPlay && j == 0)) { notesManager.notesObjects.Add(note); }
                    if ((/*!GameManager.instance.AutoPlay &&*/ j > 0))
                    {
                        inners.Add(note);
                        if (j == laneNum[i].Count - 1) { innerNotes.Add(i, inners); }
                    }

                    if (j == 0) { prop.longNote.status = SettingUtility.LongNoteStatus.Start; }
                    else if (j == laneNum[i].Count - 1) { prop.longNote.status = SettingUtility.LongNoteStatus.End; }
                    else { prop.longNote.status = SettingUtility.LongNoteStatus.Inner; }

                    prop.type = SettingUtility.NoteType.LongLinear;
                    prop.longNote.index = i;

                    if (j > 0)
                    {
                        SetNotesLine(laneNum[i][j - 1], _positionZ[i, j - 1], laneNum[i][j], _positionZ[i, j], (int)LongNoteType.AnyInnerLinear, i);
                    }
                }
                //曲線型で中間点のあるノーツ。
                else if (innnerNotesNum[i] != 0 && notesType[i] == 3)
                {
                    note = Instantiate(notesManager.noteObject.longAny, new Vector3(positionX, SettingUtility.specialNotesPosition.y, positionZ), Quaternion.identity);
                    Notes prop = note.GetComponent<Notes>();

                    if (GameManager.instance.autoPlay || (!GameManager.instance.autoPlay && j == 0)) { notesManager.notesObjects.Add(note); }
                    if ((/*!GameManager.instance.AutoPlay &&*/ j > 0))
                    {
                        inners.Add(note);
                        if (j == laneNum[i].Count - 1) { innerNotes.Add(i, inners); }
                    }

                    if (j == 0) { prop.longNote.status = SettingUtility.LongNoteStatus.Start; }
                    else if (j == laneNum[i].Count - 1) { prop.longNote.status = SettingUtility.LongNoteStatus.End; }
                    else { prop.longNote.status = SettingUtility.LongNoteStatus.Inner; }

                    prop.type = SettingUtility.NoteType.LongCurve;
                    prop.longNote.index = i;
                    if (j > 0)
                    {
                        SetNotesLine(laneNum[i][j - 1], _positionZ[i, j - 1], laneNum[i][j], _positionZ[i, j], (int)LongNoteType.AnyInnerCurve, i);
                    }
                }
            }
        }
    }
    ///<summary>曲線型ロングノーツの元となるベジェ曲線を計算します。</summary>
    ///<remarks>ベジェ曲線の数学的な方法に関しては色々なサイトを参考にしてください。</remarks>
    ///<param name = "startPosition">ロングノーツの始点。</param>
    ///<param name = "controlPosition">ロングノーツを曲げる制御点。</param>
    ///<param name = "endPosition">ロングノーツの終点。</param>
    ///<param name = "splitSize">曲線生成時の分割数。値が多い程滑らかな曲線になります。</param>
    ///<returns>曲線上の点座標。</returns>
    private Vector3[] SetBezierCurves(Vector3 startPositon, Vector3 controlPositon, Vector3 endPositon, int splitSize = 10)
    {
        //ベジェ曲線の点を分割するときに値を変化させる比。
        float t = 0;
        //曲線上の天座標を格納する配列。
        Vector3[] curvePoints = new Vector3[splitSize + 1];
        //曲線上の点を求める。
        for (int i = 0; i <= splitSize; i++)
        {
            //現在の分割回数と、最終的な分割数を割ったものが比のt。
            t = (float)i / splitSize;
            //分割した点a, bを線形補間によって求め繰り返す。
            Vector3 a = Vector3.Lerp(startPositon, controlPositon, t);
            Vector3 b = Vector3.Lerp(controlPositon, endPositon, t);
            //2つの分割点をもとにさらに分割する。
            curvePoints[i] = Vector3.Lerp(a, b, t);
        }
        return curvePoints;
    }
    ///<summary>曲線として構成されるロングノーツの断片を一つのメッシュとして合成し、親オブジェクトのメッシュとして置き換えます。</summary>
    ///<remarks>現在コライダーは合成できません。</remarks>
    ///<param name = "parent">メッシュを合成するときに使う親オブジェクト。既にノーツの断片を子としているオブジェクトである必要があります。</param>
    private void CombineMeshInChildren(GameObject parent)
    {
        //断片のメッシュを全取得。
        List<MeshFilter> meshFilters = new List<MeshFilter>();
        foreach (var mesh in parent.GetComponentsInChildren<MeshFilter>())
        {
            if (mesh.gameObject == parent) { continue; }
            meshFilters.Add(mesh);
        }
        //メッシュを合成する。
        CombineInstance[] combineInstances = new CombineInstance[meshFilters.Count];
        for (int i = 0; i < meshFilters.Count; i++)
        {
            combineInstances[i].mesh = meshFilters[i].sharedMesh;
            combineInstances[i].transform = meshFilters[i].transform.localToWorldMatrix;
        }
        //合成したメッシュを生成する。
        parent.GetComponent<MeshFilter>().mesh.CombineMeshes(combineInstances);
        //結合後の子オブジェクト（断片）の処理。
        foreach (var obj in meshFilters)
        {
            //マテリアルで透明にしておく。
            obj.gameObject.GetComponent<MeshRenderer>().material = meshMaterials.fadeRendering;
            //Destroy(obj.gameObject);
        }

    }
    ///<summary>結合した後のロングノーツのメッシュのUV座標を再計算してテクスチャを貼り直します。</summary>
    ///<param name = "longNoteObject">結合させたメッシュを適用したオブジェクト。</param>
    ///<param name = "split">分割数。</param>
    private void ReprintTexure(GameObject longNoteObject, float split, int type)
    {
        //結合後のメッシュの頂点数を取得する。
        int verticesLength = longNoteObject.GetComponent<MeshFilter>().mesh.vertices.Length;
        //UVを再計算する。
        Vector2[] uvs = new Vector2[verticesLength];
        for (int i = 0; i < uvs.Length; i++)
        {
            /* 以下：メッシュの上底（Y面）のUVを設定する。 */
            //四角形左下の絞り込み。
            if (i == 0 || (i % 4 == 0 && i % 24 == 0))
            {
                uvs[i] = new Vector2(0, Mathf.Abs((i / 24) / (float)split));
                continue;
            }
            //四角形右下の絞り込み。
            else if (i == 1 || ((i - 1) % 4 == 0 && (i - 1) % 24 == 0))
            {
                uvs[i] = new Vector2(1, Mathf.Abs(((i - 1) / 24) / (float)split));
                continue;
            }
            //四角形左上の絞り込み。
            else if (i == 2 || ((i - 2) % 4 == 0 && (i - 2) % 24 == 0))
            {
                uvs[i] = new Vector2(0, Mathf.Abs((i / 24) / (float)split + 1f / (float)split));
                continue;
            }
            //四角形右上の絞り込み。
            else if (i == 3 || ((i - 3) % 4 == 0 && (i - 3) % 24 == 0))
            {
                uvs[i] = new Vector2(1, Mathf.Abs(((i - 1) / 24) / (float)split + 1f / (float)split));
                continue;
            }
            /* 以下：メッシュの上底以外のUVを設定する。見ようにも見えない部分なので適当。 */
            else if ((i % 4 == 0))
            {
                uvs[i] = new Vector2(0, 0);
                continue;
            }
            else if (((i - 1) % 4 == 0))
            {
                uvs[i] = new Vector2(1, 0);
                continue;
            }
            else if (((i - 2) % 4 == 0))
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
        //UVを適用。
        longNoteObject.GetComponent<MeshFilter>().mesh.uv = uvs;
        //テクスチャを再貼付。
        switch (type)
        {
            case (int)LongNoteType.NoInnerCurve:
                longNoteObject.GetComponent<MeshRenderer>().material = meshMaterials.noInner;
                break;
            case (int)LongNoteType.AnyInnerCurve:
                longNoteObject.GetComponent<MeshRenderer>().material = meshMaterials.anyInner;
                break;
        }

    }


    ///<summary>中間点有りのロングノーツの生成時、中間点どうしの間で各々生成されてしまうロングノーツの断片を、譜面のロングノーツの順番ごとに1つの親オブジェクトに関連付けます。</summary>
    ///<remarks>ロングノーツをひとまとまりとして管理しやすくします。</remarks>
    public void CombineLongNoteTransform()
    {
        if (longNoteMeshList.Count == 0) { return; }
        //各ロングノーツに設定された順番を入れるリスト。
        List<int> meshIndexList = new List<int>();
        foreach (GameObject elem in longNoteMeshList)
        {
            meshIndexList.Add(elem.GetComponent<Notes>().longNote.index);
        }
        //ロングノーツのまとまりの個数を取得する。(インデックス番号なので実際はこれに+1した個数)
        int maxIndex = meshIndexList.Max();

        //リストの中から重複しているインデックスとその重複数を抽出する。
        Dictionary<int, int> duplicates = meshIndexList.GroupBy(x => x).Where(x => x.Count() > 1).ToDictionary(x => x.Key, y => y.Count());
        //リストの中から1つしかないインデックスと実際に格納されたリストのなかでのインデックス番号を抽出する。
        Dictionary<int, int> uniques = meshIndexList.GroupBy(x => x).Where(x => x.Count() == 1).ToDictionary(x => x.Key, y => meshIndexList.IndexOf(y.Key));

        //新たにつくる親オブジェクトをひとまとめにするLノーツ分だけ確保する。
        GameObject[] parents = new GameObject[duplicates.Count];
        //カウンタ。
        int i = 0;
        foreach (var item in duplicates)
        {
            //親オブジェクトにはインデックスの名前を入れて生成。
            parents[i] = new GameObject($"LongNoteMesh_{item.Key}");
            i++;
        }
        //複数あるLノーツのインデックスの配列
        int[] duplicateKeys = duplicates.Keys.ToArray();
        //重複した中間点有りLノーツインデックスの欠片を入れ子構造にして順番ごとにひとまとまりのオブジェクトができるよう調整。
        for (int j = 0; j < duplicateKeys.Length; j++)
        {
            //親にはコンポーネントを新しく設定する。
            parents[j].AddComponent<LongNotes>();
            Notes notesProp = parents[j].AddComponent<Notes>();
            parents[j].AddComponent<MeshCollider>();
            notesProp.longNote = new Notes.LongNote();
            notesProp.longNote.status = SettingUtility.LongNoteStatus.Mesh;
            notesProp.longNote.index = duplicateKeys[j];
            notesProp.longNote.isInner = true;
            for (int k = 0; k < meshIndexList.Count; k++)
            {
                if (longNoteMeshList[k].GetComponent<Notes>().longNote.index == duplicateKeys[j])
                {
                    //入れ子にする欠片の方はコンポーネントを削除する。
                    longNoteMeshList[k].transform.SetParent(parents[j].transform);
                    Destroy(longNoteMeshList[k].GetComponent<Notes>());
                    Destroy(longNoteMeshList[k].GetComponent<LongNotes>());
                }
            }
        }

        //新しいリストは順番で代入できるように一旦配列で作成。
        GameObject[] newMeshArray = new GameObject[maxIndex + 1];
        //1つしかないLノーツインデックスをもつメッシュの配列たち。
        //元のリストの順番におけるインデックス番号。
        int[] uniqueValues = uniques.Values.ToArray();
        //実際のLノーツインデックス。
        int[] uniqueKeys = uniques.Keys.ToArray();

        //新しいリスト（配列）に、Lノーツインデックス順にメッシュを追加。
        for (int l = 0, n = 0, m = 0; l < longNoteMeshList.Count; l++)
        {
            try
            {
                if (l == duplicateKeys[n] && n < duplicateKeys.Length)
                {
                    newMeshArray[l] = parents[n];
                    if (n < duplicateKeys.Length - 1) { n++; }
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
        //新しいリストへ変更を反映する。
        longNoteMeshList.Clear();
        longNoteMeshList = newMeshArray.ToList();
        longNoteMeshList.Reverse();
        foreach (GameObject item in longNoteMeshList)
        {
            item.SetLayerSelfChildren(LayerMask.NameToLayer("LongNoteMesh"));
            longNotesList.Add(item.GetComponent<LongNotes>());
        }
        innerNotesList = innerNotes.Values.ToList();
        innerNotesList.Reverse();
        foreach(var l in innerNotesList) { l.Reverse(); }
    }

    ///<summary>リストの最初と最後の要素を抽出します。</summary>
    ///<remarks>ロングノーツの始点と終点のパラメーターを抽出するときに使います。</remarks>
    ///<typeparam name = "T">いずれかの型。</typeparam>
    ///<param name = "list">いずれかの型のリスト。</param>
    ///<returns>いずれかの型のリストの最初、最終要素の二つを含んだ配列。</returns>
    private T[] GetStartAndEndElements<T>(List<T> list)
    {
        //返す配列。
        T[] startAndEnd = new T[2];
        //始点と終点を代入。
        startAndEnd[0] = list[0];
        startAndEnd[1] = list[list.Count - 1];
        return startAndEnd;
    }

    private bool SwitchLongNoteInnerType(int type)
    {
        bool flag = false;
        switch (type)
        {
            case (int)LongNoteType.AnyInnerLinear:
            case (int)LongNoteType.AnyInnerCurve:
                flag = true;
                break;
            case (int)LongNoteType.NoInnerLinear:
            case (int)LongNoteType.NoInnerCurve:
                flag = false;
                break;
        }
        return flag;
    }

    protected override float SwitchNoteLane(int laneIndex, bool useSplitLane = false)
    {
        return base.SwitchNoteLane(laneIndex, useSplitLane);
    }
}

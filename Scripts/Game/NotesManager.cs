using System.Collections.Generic;
using UnityEngine;
using Game.Utility;

public class NotesManager : UtilityClass
{
    /* フィールド */
    ///<summary>楽曲に含まれる総ノーツ数。</summary>
    public int numberOfNotes;
    ///<summary>楽曲の固有ID。実行時<see cref ="GameManager.songID"></see>から参照させます。</summary>
    private int songID { get { return GameManager.instance.songID; } }
    ///<summary>楽曲の名前。</summary><remarks>注：デバッグ時のみの使用に限る。</remarks>
    private string songName;
    ///<summary>それぞれのノーツが流れるレーン番号を格納するリスト。ノーツのX座標の振り分けに使われます。</summary>
    public List<int> laneNum = new List<int>();
    ///<summary>それぞれのノーツの種類を格納するリスト。通常ノーツ、ロングノーツの仕分けに使われます。</summary>
    public List<int> notesType = new List<int>();
    ///<summary>それぞれのノーツが判定線に接触する時間を格納するリスト。ノーツのZ座標の算出、判定、オブジェクトの並び替えに使われます。</summary>
    public List<float> notesTime = new List<float>();
    ///<summary>Instantiateしたそれぞれのノーツのオブジェクトを格納するリスト。</summary>
    public List<GameObject> notesObjects = new List<GameObject>();
    ///<summary>ノーツスピード。<see cref = "GameManager.noteSpeed"/>から参照させます。</summary>
    private float noteSpeed;
    [System.Serializable]
    public struct NoteObject
    {
        ///<summary>通常ノーツのプレハブ。</summary>
        [SerializeField] public GameObject normal;
        ///<summary>中間点を有する直線型ロングノーツのプレハブ。</summary>
        [SerializeField] public GameObject longAny;
        ///<summary>中間点を持たない直線型ロングノーツのプレハブ。</summary>
        [SerializeField] public GameObject longOnly;
    }
    ///<summary>ノーツのプレハブを格納する構造体のインスタンス。</summary>
    public NoteObject noteObject = new NoteObject();
    ///<summary>ロングノーツを管理するスクリプト。</summary>
    private LongNotesManager longNotesManager;
    ///<summary>難易度ランクを記載した辞書。</summary><remarks>注：デバッグ時のみの使用に限る。</remarks>
    private readonly List<string> Difficulty = new List<string> { "NORMAL", "HARD", "EXPERT", "MASTER" };
    ///<summary>難易度数値。</summary><remarks>注：デバッグ時のみの使用に限る。</remarks>
    [SerializeField] private int DifficultyNumber;
    private const float noteStartingLag = 0f;

    /* 組み込みメソッド */
    private void Awake()
    {
        numberOfNotes = 0;
        noteSpeed = GameManager.instance.noteSpeed;
        longNotesManager = this.GetComponent<LongNotesManager>();
        Game.Utility.Development.DevelopmentExtentionMethods.LogEditor(GameManager.instance.difficulty);
        LoadJSON($"Data/{songID}/{GameManager.instance.difficulty}");
    }

    /* メソッド */
    ///<summary>JSON形式の譜面データを読み込んでノーツを生成します。</summary>
    ///<param name = "name">JSONファイルのパス。</param>
    private void LoadJSON(string name)
    {
        //JSONの読込
        //Resourcesフォルダーから指定したコンポーネントと名前のプレハブを取得する。
        string path = Resources.Load<TextAsset>(name).ToString();
        //指定したJSONファイルから構造を丸ごと取得しクラスに代入する。
        LoadData inputJson = JsonUtility.FromJson<LoadData>(path);
        //値の代入と受け渡し。
        numberOfNotes = inputJson.notes.Length;
        GameManager.instance.musicManager.bpm = inputJson.BPM;
        //読み込んだデータからノーツを生成する。
        for (int i = 0; i < inputJson.notes.Length; i++)
        {    //ノーツの数だけ繰り返す。[ループ①]とする。
            //もし、ノーツのタイプが「2」=>ロングノーツであった時。
            if (inputJson.notes[i].type == (int)Reference.NoteType.LongLinear || inputJson.notes[i].type == (int)Reference.NoteType.LongCurve)
            {
                //ロングノーツのプロパティ（レーン番号や到達時間等）だけを格納するリスト。最終的にLongNotesManagerに送ったりする。
                List<float> longNotesTime = new List<float>();
                List<int> longNoteLaneNum = new List<int>();
                //リスト初期化
                if (longNotesTime != null && longNotesTime.Count > 0)
                {
                    longNotesTime.Clear();
                }
                if (longNoteLaneNum != null && longNoteLaneNum.Count > 0)
                {
                    longNoteLaneNum.Clear();
                }
                //最初に一度だけ、ロングノーツの始点の計算を行う（始点の計算だけこのループ①で行うのはJSONの構造が原因）
                float _secPerBeat = 60f / inputJson.BPM;    //1拍あたりの秒数。
                float _minDistance = _secPerBeat / (float)inputJson.notes[i].LPB;   //ノーツの間隔が最小のときの位置。”小節位置”と呼ぶらしい。
                float _noteTime = (float)inputJson.notes[i].num * _minDistance + noteStartingLag;     //小節位置に与えられた小節の通し番号を乗算して実際の再生時間を算出する。
                //各リストへの追加
                //Lノーツの始点はマニュアル・オートプレイに拘わらずリストに追加する。（始点はちゃんと時間で判定するため。）
                notesTime.Add(_noteTime);
                laneNum.Add(inputJson.notes[i].block);
                notesType.Add(inputJson.notes[i].type);
                //ロングノーツ専用のリストにも追加
                longNotesTime.Add(_noteTime);
                longNoteLaneNum.Add(inputJson.notes[i].block);

                for (int j = 0; j < inputJson.notes[i].notes.Length; j++)
                {   //ロングノーツの中間点の数だけ繰り返す。[ループ②]とする。
                    //ループ②内の処理は①と変わりありません。
                    float __secPerBeat = 60f / (float)inputJson.BPM;
                    float __minDistance = __secPerBeat / (float)inputJson.notes[i].notes[j].LPB;
                    float __noteTime = (float)inputJson.notes[i].notes[j].num * __minDistance + noteStartingLag;

                    if (GameManager.instance.autoPlay)
                    {
                        notesTime.Add(__noteTime);
                        laneNum.Add(inputJson.notes[i].notes[j].block);
                        notesType.Add(inputJson.notes[i].notes[j].type);
                    }
                    longNotesTime.Add(__noteTime);
                    longNoteLaneNum.Add(inputJson.notes[i].notes[j].block);
                }
                //始点・終点以外にいくつかの中間点が存在するノーツは生成時に処理を分岐させたい。そこで、中間点の数を格納するリストを生成側のLongNotesManagerに作成しておく。
                //notes[i].notesの長さから1引くと中間点の数になる（終点を除外している。）
                longNotesManager.innnerNotesNum.Add(inputJson.notes[i].notes.Length - 1);
                longNotesManager.notesType.Add(inputJson.notes[i].type);
                //各リストへの追加
                longNotesManager.notesTime.Add(longNotesTime);
                longNotesManager.laneNum.Add(longNoteLaneNum);
                //ノーツ総数に追加
                numberOfNotes += inputJson.notes[i].notes.Length;
                //メソッドに引数を渡す。
                longNotesManager.SetPosition(longNotesManager.laneNum.Count - 1);
            }
            //ノーツのタイプが通常ノーツであった時。
            if (inputJson.notes[i].type == (int)Reference.NoteType.Normal)
            {
                float secPerBeat = 60f / (float)inputJson.BPM;
                float minDistance = secPerBeat / (float)inputJson.notes[i].LPB;
                float noteTime = (float)inputJson.notes[i].num * minDistance + noteStartingLag;

                notesTime.Add(noteTime);
                laneNum.Add(inputJson.notes[i].block);
                notesType.Add(inputJson.notes[i].type);
                //座標計算。
                float positionX = SwitchNoteLane(inputJson.notes[i].block);  //X座標の振り分け。
                float positionZ = notesTime[notesTime.Count - 1] * noteSpeed + Reference.Origin.z;    //Z座標の算出。
                //ノーツをオブジェクトとして生成する。
                notesObjects.Add(Instantiate(noteObject.normal, new Vector3(positionX, Reference.Origin.y, positionZ), Quaternion.identity));
                //プロパティを渡す。
                notesObjects[notesObjects.Count - 1].GetComponent<Notes>().type = Reference.NoteType.Normal;
                notesObjects[notesObjects.Count - 1].name = $"Note_{i}";
            }
        }
        //Linqを使ってノーツの到達時間を降順にソートする。
        notesTime.Reverse();
        //ノーツのオブジェクトを到達順に整理したいが、Linqを使ったソートではできないため、オブジェクトのZ座標を利用する。
        List<float> objSort = new List<float>();
        for (int i = 0; i < notesObjects.Count; i++)
        {
            objSort.Add(notesObjects[i].transform.position.z);
        }
        //ノーツオブジェクトのソート。
        NotesSort(notesObjects, objSort, false);
        //ノーツレーンのソート。
        NotesSort(laneNum, objSort, false);
        //ノーツタイプのソート。
        NotesSort(notesType, objSort, false);
        //ロングノーツの整理。
        longNotesManager.CombineLongNoteTransform();
        //最大スコアの計算。
        GameManager.instance.scoreManager.maxScore = numberOfNotes * 5;
    }

    ///<summary>ノーツのZ座標からノーツオブジェクトのリスト（<see cref ="notesObjects"/>）をソートします。</summary>
    ///<remarks>両リストは、同じインデックスに同じ対象のノーツの情報を格納しているので、バブルソートで得られる交換時のインデックス番号を使って内部の値を交換しています。</remarks>
    ///<returns>ソートされたノーツのオブジェクト。</returns>
    ///<param name = "targetList">整列する前のノーツのオブジェクトのリスト。</param> <param name = "baseList">Z座標を格納したリスト。</param> <param name = "isAscending">trueなら昇順、falseなら降順に並べ替えます。</param>
    private List<T> NotesSort<T>(List<T> targetList, List<float> baseList, bool isAscending)
    {
        float tmp = 0;
        //リストは変更が参照されて引数に設定した元のリストまで変わるので実体コピーすること。
        List<float> clone = new List<float>(baseList);
        T _tmp = default;
        if (isAscending)
        {
            for (int i = 0; i < clone.Count; i++)
            {
                for (int j = clone.Count - 1; j > i; j--)
                {
                    if (clone[j - 1] > clone[j])
                    {
                        tmp = clone[j - 1];
                        _tmp = targetList[j - 1];
                        clone[j - 1] = clone[j];
                        targetList[j - 1] = targetList[j];
                        clone[j] = tmp;
                        targetList[j] = _tmp;
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < clone.Count; i++)
            {
                for (int j = clone.Count - 1; j > i; j--)
                {
                    if (clone[j - 1] < clone[j])
                    {
                        tmp = clone[j - 1];
                        _tmp = targetList[j - 1];
                        clone[j - 1] = clone[j];
                        targetList[j - 1] = targetList[j];
                        clone[j] = tmp;
                        targetList[j] = _tmp;
                    }
                }
            }
        }
        return targetList;
    }

    protected override float SwitchNoteLane(int laneIndex, bool useSplitLane = false)
    {
        return base.SwitchNoteLane(laneIndex, useSplitLane);
    }
}

/* クラス（シリアライズ） */
///<summary>譜面JSONファイルの全構造をコピーしたクラスです。</summary>
[System.Serializable]
public class LoadData
{
    public string name;
    public int maxBlock;
    public int BPM;
    public int offset;
    public LoadNote[] notes;
}
///<summary>譜面JSONファイルのノーツの情報が格納された配列部分をコピーしたクラスです。</summary>
[System.Serializable]
public class LoadNote
{
    public int type;
    public int num;
    public int block;
    public int LPB;
    public LoadLongNote[] notes;
}
///<summary>譜面JSONファイルのロングノーツの情報が格納された配列部分をコピーしたクラスです。</summary>
[System.Serializable]
public class LoadLongNote
{
    public int type;
    public int num;
    public int block;
    public int LPB;
}
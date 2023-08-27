/* 名前空間 */
using System.Collections.Generic;
using UnityEngine;
using TMPro;
//自作名前空間
using Game.Utility;
using Game.Utility.Development;

///<summary>ノーツの判定をします。</summary>
public class Judge : UtilityClass
{
    ///<summary>生成する判定ステータスのオブジェクトプール。</summary>
    [SerializeField] private ScoreObjectPool scoreObjectPool;
    ///<summary>生成される判定ステータスのオブジェクト。</summary>
    private GameObject scoreObject { get; set; }
    ///<summary><see cref = "scoreObject"/>に対する親オブジェクト。</summary>
    [SerializeField] private GameObject scoreObjectParent;
    ///<summary><see cref = "NotesManager"/></summary>
    [SerializeField] private NotesManager notesManager;
    ///<summary><see cref = "LongNotesManager"/></summary>
    [SerializeField] private LongNotesManager longNotesManager;
    ///<summary><see cref = "TapManager"/></summary>
    [SerializeField] private TapManager tapManager;
    ///<summary>判定の対象になるノーツ。</summary>
    private GameObject targetNote;
    ///<summary>判定の対象になるノーツの情報 (<see cref = "Notes"/>の情報)。</summary>
    private Notes targetNoteProperty;
    ///<summary>タップされた時間を保存する。</summary>
    private float[] tapTime = new float[6];
    ///<summary>タップされたレーンにあるノーツを収める。</summary>
    List<GameObject> laneNotes = new List<GameObject>();
    ///<summary>タップされたレーンにあるノーツのZ座標を収める。</summary>
    List<float> laneNotesPosZ = new List<float>();
    ///<summary>コンボ数を表示するテキスト。</summary>
    [SerializeField] private TextMeshProUGUI ComboText;
    ///<summary>スコアを表示するテキスト。</summary>
    [SerializeField] private TextMeshProUGUI ScoreText;
    ///<summary>ノーツにヒットした時のSE。</summary>
    private AudioClip[] hitSE = new AudioClip[2];
    ///<summary>判定ステータスの基準。</summary>
    private Dictionary<string, float> JudgementTiming = new Dictionary<string, float>()
    {
        {"perfect", 0.08f}, {"great", 0.12f}, {"good", 0.25f}, {"bad", 0.4f}, {"miss", 0.6f}
    };
    ///<summary>ノーツが譜面上に残っているか。</summary>
    ///<remarks>ノーツリストのカウントを参照している。</remarks>
    private bool isNotes { get { return (notesManager.notesObjects.Count > 0 ? true : false); } }
    ///<summary>ロングノーツが譜面上に存在するか。</summary>
    ///<remarks>ノーツリストのカウントを参照している。</remarks>
    private bool isLongNotes { get { return (longNotesManager.longNotesList.Count > 0 ? true : false); } }
    ///<summary><see cref = "NotesManager.laneNum"/>のカウント</summary>
    private int laneNumCount { get { return notesManager.laneNum.Count; } }
    ///<summary><see cref = "NotesManager.notesType"/>のカウント</summary>
    private int notesTypeCount { get { return notesManager.notesType.Count; } }
    ///<summary><see cref = "NotesManager.notesTime"/>のカウント</summary>
    private int notesTimeCount { get { return notesManager.notesTime.Count; } }
    ///<summary><see cref = "NotesManager.notesObjects"/>のカウント</summary>
    private int notesObjectsCount { get { return notesManager.notesObjects.Count; } }
    ///<summary><see cref = "LongNotesManager.longNotesList"/>のカウント</summary>
    private int longNotesCount { get { return longNotesManager.longNotesList.Count; } }
    ///<summary><see cref = "LongNotesManager.innerNotesList"/>のカウント</summary>
    private int innerNotesListCount { get { return longNotesManager.innerNotesList.Count; } }

    /* メソッド */
    /* -- MonoBehavior -- */
    void Start()
    {
        //great以上のときのSE。
        hitSE[0] = (AudioClip)Resources.Load(Reference.ResourcesPath.NOTE_SINGLE_GREAT_SE_PATH);
        //good以下の時のSE。
        hitSE[1] = (AudioClip)Resources.Load(Reference.ResourcesPath.NOTE_SINGLE_GOOD_SE_PATH);
    }

    void Update()
    {
        if (GameManager.instance.start && !GameManager.instance.autoPlay)
        {   //ゲームが始まったら
            /*try
            {*/
            ManualPlay(0);
            ManualPlay(1);
            ManualPlay(2);
            ManualPlay(3);
            ManualPlay(4);
            ManualPlay(5);
            /*
            }
            catch (System.ArgumentOutOfRangeException) { }*/
        }
        if (GameManager.instance.start && GameManager.instance.autoPlay)
        {   //オートプレイが選択されている場合
            try { AutoPlay(); }
            catch (System.ArgumentOutOfRangeException) { }
        }
    }

    ///<summary>ノーツが叩かれたかどうかを取得し、判定する基本のメソッド。</summary>
    ///<param name = "laneIndex">レーンのインデックス。</param>
    private void ManualPlay(int laneIndex)
    {
        //ロングノーツが存在するとき。
        if (isLongNotes)
        {
            if (longNotesCount > 0 && innerNotesListCount > 0)
            {
                if (longNotesManager.longNotesList[longNotesCount - 1].isPressed) { GetTargetLongNotes(longNotesCount - 1, longNotesManager.longNotesList[longNotesCount - 1].isPressed); }
                else { GetTargetLongNotes(longNotesCount - 1, longNotesManager.longNotesList[longNotesCount - 1].isPressed); }
                //同時押し対応。
                if (longNotesCount > 1)
                {
                    if (longNotesManager.longNotesList[longNotesCount - 2].isPressed) { GetTargetLongNotes(longNotesCount - 2, longNotesManager.longNotesList[longNotesCount - 2].isPressed); }
                    else { GetTargetLongNotes(longNotesCount - 2, longNotesManager.longNotesList[longNotesCount - 2].isPressed); }
                    /*
                    if (longNotesManager.longNotesList[longNotesCount - 3].isPressed) { GetTargetLongNotes(longNotesCount - 3, longNotesManager.longNotesList[longNotesCount - 3].isPressed); }
                    else { GetTargetLongNotes(longNotesCount - 3, longNotesManager.longNotesList[longNotesCount - 3].isPressed); }*/
                }
            }
        }
        //ノーツが存在しなくなったらやめる。
        if (!isNotes) { return; }
        //レーンが押下されていて、現在のタップ時間が過去のタップ時間と等しくなければ。
        if (tapManager.tapFlag[laneIndex] && tapTime[laneIndex] != tapManager.tapTime[laneIndex])
        {
            //タップされた時間を保存する。
            tapTime[laneIndex] = tapManager.tapTime[laneIndex];
            //押されたレーンを流れる、判定線に最も近いノーツを取得する。
            GetTargetNotes(laneIndex);
            //ターゲットノーツが取得できていないときは処理を終わる。
            if (targetNote == null) { return; }

            //ターゲットノーツのZ座標と判定線との距離の差が5より小さいときに判定の対象とする。
            if (Mathf.Abs(targetNote.transform.position.z - Reference.Origin.z) < 5f)
            {
                //ターゲットが通常ノーツのとき。
                if (targetNoteProperty.type == Reference.NoteType.Normal)
                {
                    //ノーツ時間の降順リストの最後尾に登録されたインデックスと、取得したインデックスが同じ且つ、ターゲットノーツのX座標とインデックスに紐づくX座標が同じならば
                    if (notesManager.laneNum[laneNumCount - 1] == laneIndex)
                    {
                        DevelopmentExtentionMethods.LogEditor($"{Mathf.Abs(tapManager.tapTime[laneIndex] - (notesManager.notesTime[notesTimeCount - 1] + GameManager.instance.startTime))}, timetime:{tapManager.tapTime[laneIndex]}, notetime:{notesManager.notesTime[notesTimeCount - 1]}, 0, {targetNote.name}");
                        Judgement(Mathf.Abs(tapManager.tapTime[laneIndex] - (notesManager.notesTime[notesTimeCount - 1] + GameManager.instance.startTime)), notesTimeCount - 1);   //ノーツ（light）を叩いた時間と、本来ノーツを叩くべき時間とスタート時間の和との差をメソッドに送る
                        return;
                    }
                    try
                    {   //同時押しなどの融通を効かせるために、最後尾より前のノーツ時間も参照するようにする。
                        if (notesManager.laneNum[laneNumCount - 2] == laneIndex)
                        {
                            DevelopmentExtentionMethods.LogEditor($"{Mathf.Abs(tapManager.tapTime[laneIndex] - (notesManager.notesTime[notesTimeCount - 2] + GameManager.instance.startTime))}, timetime:{tapManager.tapTime[laneIndex]}, notetime:{notesManager.notesTime[notesTimeCount - 2]}, 1, {targetNote.name}");
                            Judgement(Mathf.Abs(tapManager.tapTime[laneIndex] - (notesManager.notesTime[notesTimeCount - 2] + GameManager.instance.startTime)), notesTimeCount - 2);
                            return;
                        }
                        if (notesManager.laneNum[laneNumCount - 3] == laneIndex)
                        {
                            DevelopmentExtentionMethods.LogEditor($"{Mathf.Abs(tapManager.tapTime[laneIndex] - (notesManager.notesTime[notesTimeCount - 3] + GameManager.instance.startTime))}, {tapManager.tapTime[laneIndex]}, 2");
                            Judgement(Mathf.Abs(tapManager.tapTime[laneIndex] - (notesManager.notesTime[notesTimeCount - 3] + GameManager.instance.startTime)), notesTimeCount - 3);
                            return;
                        }
                        if (notesManager.laneNum[laneNumCount - 4] == laneIndex)
                        {
                            DevelopmentExtentionMethods.LogEditor($"{Mathf.Abs(tapManager.tapTime[laneIndex] - (notesManager.notesTime[notesTimeCount - 4] + GameManager.instance.startTime))}, {tapManager.tapTime[laneIndex]}, 3");
                            Judgement(Mathf.Abs(tapManager.tapTime[laneIndex] - (notesManager.notesTime[notesTimeCount - 4] + GameManager.instance.startTime)), notesTimeCount - 4);
                            return;
                        }
                    }
                    catch (System.ArgumentOutOfRangeException) { }
                }
                //ターゲットがロングノーツのとき。
                if (targetNoteProperty.type == Reference.NoteType.LongLinear || targetNoteProperty.type == Reference.NoteType.LongCurve)
                {
                    //ターゲットがロングノーツの始点であれば。
                    if (targetNoteProperty.longNote.status == Reference.LongNoteStatus.Start)
                    {
                        //ノーツ時間の降順リストの最後尾に登録されたインデックスと、取得したインデックスが同じ且つ、ターゲットノーツのX座標とインデックスに紐づくX座標が同じならば
                        if (notesManager.laneNum[laneNumCount - 1] == laneIndex)
                        {
                            DevelopmentExtentionMethods.LogEditor($"{Mathf.Abs(tapManager.tapTime[laneIndex] - (notesManager.notesTime[notesTimeCount - 1] + GameManager.instance.startTime))}, timetime:{tapManager.tapTime[laneIndex]}, notetime:{notesManager.notesTime[notesTimeCount - 1]}, start:{GameManager.instance.startTime}");
                            Judgement(Mathf.Abs(tapManager.tapTime[laneIndex] - (notesManager.notesTime[notesTimeCount - 1] + GameManager.instance.startTime)), notesTimeCount - 1);   //ノーツ（light）を叩いた時間と、本来ノーツを叩くべき時間とスタート時間の和との差をメソッドに送る
                            return;
                        }
                        try
                        {   //同時押しなどの融通を効かせるために、最後尾より前のノーツ時間も参照するようにする。
                            if (notesManager.laneNum[laneNumCount - 2] == laneIndex)
                            {
                                DevelopmentExtentionMethods.LogEditor($"{Mathf.Abs(tapManager.tapTime[laneIndex] - (notesManager.notesTime[notesTimeCount - 2] + GameManager.instance.startTime))}, {tapManager.tapTime[laneIndex]}");
                                Judgement(Mathf.Abs(tapManager.tapTime[laneIndex] - (notesManager.notesTime[notesTimeCount - 2] + GameManager.instance.startTime)), notesTimeCount - 2);
                                return;
                            }
                            if (notesManager.laneNum[laneNumCount - 3] == laneIndex)
                            {
                                DevelopmentExtentionMethods.LogEditor($"{Mathf.Abs(tapManager.tapTime[laneIndex] - (notesManager.notesTime[notesTimeCount - 3] + GameManager.instance.startTime))}, {tapManager.tapTime[laneIndex]}");
                                Judgement(Mathf.Abs(tapManager.tapTime[laneIndex] - (notesManager.notesTime[notesTimeCount - 3] + GameManager.instance.startTime)), notesTimeCount - 3);
                                return;
                            }
                            if (notesManager.laneNum[laneNumCount - 4] == laneIndex)
                            {
                                DevelopmentExtentionMethods.LogEditor($"{Mathf.Abs(tapManager.tapTime[laneIndex] - (notesManager.notesTime[notesTimeCount - 4] + GameManager.instance.startTime))}, {tapManager.tapTime[laneIndex]}");
                                Judgement(Mathf.Abs(tapManager.tapTime[laneIndex] - (notesManager.notesTime[notesTimeCount - 4] + GameManager.instance.startTime)), notesTimeCount - 4);
                                return;
                            }
                        }
                        catch (System.ArgumentOutOfRangeException) { }
                    }
                }
            }
        }
        //レーンが押下されなかった時。
        else
        {   //直近で押しておかなければならなかったノーツがZ:4.6を下回った時。
            if (notesManager.notesObjects[notesObjectsCount - 1].transform.position.z < 3.5f && notesManager.notesObjects[notesObjectsCount - 1].transform.position.x == SwitchNoteLane(laneIndex) && notesManager.notesObjects[notesObjectsCount - 1].activeSelf)
            {
                ScoreMessage(Reference.JudgementStatus.Miss);
                GameManager.instance.scoreManager.combo = 0;
                DeleteNote(notesTimeCount - 1, mode: 2);
            }
        }
    }

    ///<summary>ターゲットノーツを特定します。</summary>
    ///<param name = "laneIndex">レーン番号</param>
    private void GetTargetNotes(int laneIndex)
    {
        //あるレーンを流れるノーツオブジェクトが入るリストの初期化。
        laneNotes.Clear();
        //あるレーンを流れるノーツのz座標を記録するリストの初期化。
        laneNotesPosZ.Clear();
        //全ノーツのリストの中から絞り込む。
        for (int i = 0; i < notesManager.notesObjects.Count; i++)
        {
            //押されたレーンに一致するノーツを特定する。
            if (notesManager.notesObjects[i].transform.position.x == SwitchNoteLane(laneIndex))
            {
                //そのうちのノーツのZ座標と判定線のZ座標との差が5より小さい且つ、差が-1より大きいものを特定する。
                if (((notesManager.notesObjects[i].transform.position.z - 7.3f) < 5f) && ((notesManager.notesObjects[i].transform.position.z - 7.3f) > -1f))
                {
                    //リストに特定したノーツを追加する。
                    laneNotes.Add(notesManager.notesObjects[i]);
                }
                //当てはまらなかったら以外だったら除外してfor文の始めに戻る。
                else { continue; }
            }
        }
        if (laneNotes.Count == 0) { return; }
        //最接近しているノーツは抽出したノーツリストのうち、z座標が最も小さいものをさらに抽出
        float reference = laneNotes[0].transform.position.z;
        int targetIndex = 0;
        for (int i = 0; i < laneNotes.Count; i++)
        {
            //ノーツリストの一番初めの要素よりもZ座標が小さいものがあるか確認する。
            if (laneNotes[i].transform.position.z < reference)
            {
                //あれば、参照値を変更し、その番号を記憶する。
                reference = laneNotes[i].transform.position.z;
                targetIndex = i;
            }
        }
        //抽出できたものをターゲットノーツとする。
        targetNote = laneNotes[targetIndex];
        targetNoteProperty = targetNote.GetComponent<Notes>();
    }

    ///<summary>押されるロングノーツを特定する。</summary>
    ///<param name = "longNoteListIndex">判定するLノーツのリストのインデックス。</param>
    ///<param name = "isPressed">ロングノーツが押されているか。</param>
    private void GetTargetLongNotes(int longNoteListIndex, bool isPressed)
    {
        int key = 0;
        if (longNoteListIndex == longNotesCount - 1) { key = innerNotesListCount - 1; }
        else if (longNoteListIndex == longNotesCount - 2) { key = innerNotesListCount - 2; }
        else if (longNoteListIndex == longNotesCount - 3) { key = innerNotesListCount - 3; }
        //押されるノーツの情報を取得する。
        Notes noteInfo = longNotesManager.longNoteMeshList[longNoteListIndex].GetComponent<Notes>();
        //次に押されるロングノーツの中間点の有無と、Ｌノーツのリストが空でないことを確認する。
        if (noteInfo.longNote.isInner && innerNotesListCount > 0 && longNotesManager.innerNotesList[key].Count > 0)
        {
            //直近のLノーツの中間点が判定線付近まで達した時。
            if (longNotesManager.innerNotesList[key][longNotesManager.innerNotesList[key].Count - 1].transform.position.z <= Reference.Origin.z + 1f)
            {
                //中間点のノーツ情報も取得の準備をする。
                Notes innerNoteInfo;
                //そのノーツの順番が参照するリストのノーツの順番に等しければ。
                if (noteInfo.longNote.index == longNotesManager.innerNotesList[key][longNotesManager.innerNotesList[key].Count - 1].GetComponent<Notes>().longNote.index)
                {
                    //押されていれば。
                    if (isPressed)
                    {
                        GameManager.instance.seSource.PlayOneShot(hitSE[0]);
                        ScoreMessage(Reference.JudgementStatus.Perfect);
                        GameManager.instance.scoreManager.ratioScore += (int)Reference.JudgementStatusScore.Perfect;
                        GameManager.instance.scoreManager.scoreCount["perfect"]++;
                        GameManager.instance.scoreManager.combo++;
                        GameManager.instance.ScoreCalc();
                        ScoreText.SetText("{0}", GameManager.instance.scoreManager.score);
                        ComboText.SetText("{0}", GameManager.instance.scoreManager.combo);
                        innerNoteInfo = longNotesManager.innerNotesList[key][longNotesManager.innerNotesList[key].Count - 1].GetComponent<Notes>();
                        longNotesManager.innerNotesList[key][longNotesManager.innerNotesList[key].Count - 1].SetActive(false);
                        longNotesManager.innerNotesList[key].RemoveAt(longNotesManager.innerNotesList[key].Count - 1);                        
                        if (innerNoteInfo.longNote.status == Reference.LongNoteStatus.End)
                        {
                            longNotesManager.longNoteMeshList[longNoteListIndex].SetActive(false);
                            longNotesManager.longNotesList.RemoveAt(longNoteListIndex);
                            longNotesManager.longNoteMeshList.RemoveAt(longNoteListIndex);
                            longNotesManager.innerNotesList.RemoveAt(key);
                        }
                    }
                    //押されていなければ。
                    else
                    {
                        ScoreMessage(Reference.JudgementStatus.Miss);
                        GameManager.instance.scoreManager.scoreCount["miss"]++;
                        GameManager.instance.scoreManager.combo = 0;
                        ComboText.SetText("{0}", GameManager.instance.scoreManager.combo);
                        innerNoteInfo = longNotesManager.innerNotesList[key][longNotesManager.innerNotesList[key].Count - 1].GetComponent<Notes>();
                        longNotesManager.innerNotesList[key][longNotesManager.innerNotesList[key].Count - 1].SetActive(false);
                        longNotesManager.innerNotesList[key].RemoveAt(longNotesManager.innerNotesList[key].Count - 1);
                        if (innerNoteInfo.longNote.status == Reference.LongNoteStatus.End)
                        {
                            longNotesManager.longNoteMeshList[longNoteListIndex].SetActive(false);
                            longNotesManager.longNotesList.RemoveAt(longNoteListIndex);
                            longNotesManager.longNoteMeshList.RemoveAt(longNoteListIndex);
                            longNotesManager.innerNotesList.RemoveAt(key);
                        }
                    }
                }

            }
        }
        else if (!noteInfo.longNote.isInner)
        {
            //直近のLノーツの中間点が判定線付近まで達した時。
            if (longNotesManager.innerNotesList[key][longNotesManager.innerNotesList[key].Count - 1].transform.position.z <= Reference.Origin.z + 1f)
            {
                Notes endNote;
                //そのノーツの順番が参照するリストのノーツの順番に等しければ。
                if (noteInfo.longNote.index == longNotesManager.innerNotesList[key][longNotesManager.innerNotesList[key].Count - 1].GetComponent<Notes>().longNote.index)
                {
                    //押されていれば。
                    if (isPressed)
                    {
                        GameManager.instance.seSource.PlayOneShot(hitSE[0]);
                        ScoreMessage(Reference.JudgementStatus.Perfect);
                        GameManager.instance.scoreManager.ratioScore += (int)Reference.JudgementStatusScore.Perfect;
                        GameManager.instance.scoreManager.scoreCount["perfect"]++;
                        GameManager.instance.scoreManager.combo++;
                        GameManager.instance.ScoreCalc();
                        ScoreText.SetText("{0}", GameManager.instance.scoreManager.score);
                        ComboText.SetText("{0}", GameManager.instance.scoreManager.combo);
                        endNote = longNotesManager.innerNotesList[key][0].GetComponent<Notes>();
                        longNotesManager.innerNotesList[key][0].SetActive(false);
                        longNotesManager.innerNotesList[key].RemoveAt(0);
                        DevelopmentExtentionMethods.LogEditor(endNote.longNote.status);
                        longNotesManager.longNoteMeshList[longNoteListIndex].SetActive(false);
                        longNotesManager.longNotesList.RemoveAt(longNoteListIndex);
                        longNotesManager.longNoteMeshList.RemoveAt(longNoteListIndex);
                        longNotesManager.innerNotesList.RemoveAt(key);
                    }
                    //押されていなければ。
                    else
                    {
                        ScoreMessage(Reference.JudgementStatus.Miss);
                        GameManager.instance.scoreManager.scoreCount["miss"]++;
                        GameManager.instance.scoreManager.combo = 0;
                        ComboText.SetText("{0}", GameManager.instance.scoreManager.combo);
                        endNote = longNotesManager.innerNotesList[key][0].GetComponent<Notes>();
                        longNotesManager.innerNotesList[key][0].SetActive(false);
                        longNotesManager.innerNotesList[key].RemoveAt(0);
                        longNotesManager.longNoteMeshList[longNoteListIndex].SetActive(false);
                        longNotesManager.longNotesList.RemoveAt(longNoteListIndex);
                        longNotesManager.longNoteMeshList.RemoveAt(longNoteListIndex);
                        longNotesManager.innerNotesList.RemoveAt(key);
                    }
                }
            }
        }
        else return;
    }

    ///<summary>判定をする。</summary>
    ///<param name = "timeLag">実際にノーツが押された時間と押されるべき時間とのラグ。</param>
    ///<param name = "targetIndex">判定対象になったノーツの、リスト内のインデックス。</param>
    private void Judgement(float timeLag, int targetIndex)
    {
        //ターゲットノーツがリストに存在するか確認する。。
        if (notesManager.notesObjects.Contains(targetNote))
        {
            //ラグを判定幅に照応させて判定する。
            if (timeLag <= JudgementTiming["perfect"])
            {
                GameManager.instance.seSource.PlayOneShot(hitSE[0]);
                ScoreMessage(Reference.JudgementStatus.Perfect);
                GameManager.instance.scoreManager.ratioScore += (int)Reference.JudgementStatusScore.Perfect;
                GameManager.instance.scoreManager.scoreCount["perfect"]++;
                GameManager.instance.scoreManager.combo++;
                DeleteNote(targetIndex);
            }
            else if (timeLag <= JudgementTiming["great"])
            {
                GameManager.instance.seSource.PlayOneShot(hitSE[0]);
                ScoreMessage(Reference.JudgementStatus.Great);
                GameManager.instance.scoreManager.ratioScore += (int)Reference.JudgementStatusScore.Great;
                GameManager.instance.scoreManager.scoreCount["great"]++;
                GameManager.instance.scoreManager.combo++;
                DeleteNote(targetIndex);
            }
            else if (timeLag <= JudgementTiming["good"])
            {
                GameManager.instance.seSource.PlayOneShot(hitSE[1]);
                ScoreMessage(Reference.JudgementStatus.Good);
                GameManager.instance.scoreManager.ratioScore += (int)Reference.JudgementStatusScore.Good;
                GameManager.instance.scoreManager.scoreCount["good"]++;
                GameManager.instance.scoreManager.combo++;
                DeleteNote(targetIndex);
                //*絶対精度良くないから、Goodまではコンボ許容しないと俺が怒るぜ
            }
            else if (timeLag <= JudgementTiming["bad"])
            {
                GameManager.instance.seSource.PlayOneShot(hitSE[1]);
                ScoreMessage(Reference.JudgementStatus.Bad);
                GameManager.instance.scoreManager.ratioScore += (int)Reference.JudgementStatusScore.Bad;
                GameManager.instance.scoreManager.scoreCount["bad"]++;
                GameManager.instance.scoreManager.combo = 0;
                DeleteNote(targetIndex);
            }
        }
    }

    ///<summary>オートプレイ時の判定を行う。</summary>
    private void Judgement(int noteNumber)
    {
        GameManager.instance.seSource.PlayOneShot(hitSE[0]);
        ScoreMessage(Reference.JudgementStatus.Perfect);
        GameManager.instance.scoreManager.ratioScore += (int)Reference.JudgementStatusScore.Perfect;
        GameManager.instance.scoreManager.scoreCount["perfect"]++;
        GameManager.instance.scoreManager.combo++;
        DeleteNote(noteNumber, mode: 1);
    }

    ///<summary>判定が終わったノーツを消す。</summary>
    ///<param name = "targetIndex">判定対象になったノーツの、リスト内のインデックス。</param>
    ///<param name = "mode">0 => 通常用。(デフォルト)<br/>1 => オートプレイ専用。<br/>2 => ミスした時に呼び出す用。</param>
    private void DeleteNote(int targetIndex, int mode = 0)
    {
        if (mode == 0)
        {
            targetNote.SetActive(false);
            int index = notesManager.notesObjects.IndexOf(targetNote);
            notesManager.notesTime.RemoveAt(index);
            notesManager.laneNum.RemoveAt(index);
            notesManager.notesType.RemoveAt(index);
            notesManager.notesObjects.RemoveAt(index);
            GameManager.instance.ScoreCalc();
            ScoreText.SetText("{0}", GameManager.instance.scoreManager.score);
            ComboText.SetText("{0}", GameManager.instance.scoreManager.combo);
        }
        else if (mode == 1)
        {
            notesManager.notesObjects[targetIndex].SetActive(notesManager.notesObjects[targetIndex].activeSelf ? false : false);

            notesManager.notesTime.RemoveAt(targetIndex);
            notesManager.laneNum.RemoveAt(targetIndex);
            notesManager.notesType.RemoveAt(targetIndex);
            notesManager.notesObjects.RemoveAt(targetIndex);
            GameManager.instance.ScoreCalc();
            ScoreText.SetText("{0}", GameManager.instance.scoreManager.score);
            ComboText.SetText("{0}", GameManager.instance.scoreManager.combo);
        }
        else if (mode == 2)
        {
            notesManager.notesObjects[targetIndex].SetActive(notesManager.notesObjects[targetIndex].activeSelf ? false : false);
            notesManager.notesTime.RemoveAt(targetIndex);
            notesManager.laneNum.RemoveAt(targetIndex);
            notesManager.notesType.RemoveAt(targetIndex);
            notesManager.notesObjects.RemoveAt(targetIndex);
            GameManager.instance.scoreManager.scoreCount["miss"]++;
            GameManager.instance.scoreManager.combo = 0;
            ComboText.SetText("{0}", GameManager.instance.scoreManager.combo);
        }
        else return;
    }

    ///<summary>判定ステータスを画面上に表示する。</summary>
    ///<remarks>オブジェクトプール(<see cref = "ScoreObjectPool"/>)を利用する。</remarks>
    private void ScoreMessage(Reference.JudgementStatus status)
    {
        switch (status)
        {
            case Reference.JudgementStatus.Perfect:
                scoreObject = scoreObjectPool.perfect.Get();
                break;
            case Reference.JudgementStatus.Great:
                scoreObject = scoreObjectPool.great.Get();
                break;
            case Reference.JudgementStatus.Good:
                scoreObject = scoreObjectPool.good.Get();
                break;
            case Reference.JudgementStatus.Bad:
                scoreObject = scoreObjectPool.bad.Get();
                break;
            case Reference.JudgementStatus.Miss:
                scoreObject = scoreObjectPool.miss.Get();
                break;
        }
        scoreObject.transform.SetParent(scoreObjectParent.transform);
        scoreObject.transform.position = new Vector3(0, 0, 0);
        scoreObject.transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    ///<summary>オートプレイ時の自動判定。</summary>
    private void AutoPlay()
    {
        if (Mathf.Abs(notesManager.notesObjects[notesObjectsCount - 1].transform.position.z - 7.3f) < 1.0f) { Judgement(notesObjectsCount - 1); }
        if (Mathf.Abs(notesManager.notesObjects[notesObjectsCount - 2].transform.position.z - 7.3f) < 1.0f) { Judgement(notesObjectsCount - 2); }
        if (Mathf.Abs(notesManager.notesObjects[notesObjectsCount - 3].transform.position.z - 7.3f) < 1.0f) { Judgement(notesObjectsCount - 3); }
        if (Mathf.Abs(notesManager.notesObjects[notesObjectsCount - 4].transform.position.z - 7.3f) < 1.0f) { Judgement(notesObjectsCount - 4); }
        if (Mathf.Abs(notesManager.notesObjects[notesObjectsCount - 5].transform.position.z - 7.3f) < 1.0f) { Judgement(notesObjectsCount - 5); }
        if (Mathf.Abs(notesManager.notesObjects[notesObjectsCount - 6].transform.position.z - 7.3f) < 1.0f) { Judgement(notesObjectsCount - 6); }
        if (Mathf.Abs(notesManager.notesObjects[notesObjectsCount - 7].transform.position.z - 7.3f) < 1.0f) { Judgement(notesObjectsCount - 7); }
        if (Mathf.Abs(notesManager.notesObjects[notesObjectsCount - 8].transform.position.z - 7.3f) < 1.0f) { Judgement(notesObjectsCount - 8); }
        if (Mathf.Abs(notesManager.notesObjects[notesObjectsCount - 9].transform.position.z - 7.3f) < 1.0f) { Judgement(notesObjectsCount - 9); }
    }
}
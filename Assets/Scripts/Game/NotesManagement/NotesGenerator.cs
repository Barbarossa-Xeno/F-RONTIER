using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FRONTIER.Utility;

namespace FRONTIER.Game.NotesManagement
{
    /// <summary>
    /// ノーツ生成を行うクラス。
    /// </summary>
    public class NotesGenerator : NotesManager<int, float, GameObject>
    {
        #region フィールド

        /// <summary>
        /// 楽曲に含まれる総ノーツ数。
        /// </summary>
        public int notesCount;

        /// <summary>
        /// 通常ノーツのプレハブ。
        /// </summary>
        [SerializeField] private GameObject notePrefab;

        /// <summary>
        /// ロングノーツを管理するスクリプト。
        /// </summary>
        [SerializeField] private LongNotesGenerator longNotesGenerator;

        /// <summary>
        /// 譜面データ。
        /// </summary>
        [SerializeField] private NotePatternData data;

        #endregion

        #region MonoBehaviorメソッド

        void Awake()
        {
            data = base.LoadNotePattern(PlayInfo.ID, PlayInfo.DifficultyTo(PlayInfo.Difficulty).Item1);
            GenerateNotes();
            // 最大スコアの計算
            Manager.score.maxScoreValue = notesCount * Reference.JudgementStatusScore.PERFECT;
            Manager.score.maxComboCount = notesCount;

            // 譜面データからわかることをプレイ状況に反映
            PlayInfo.Bpm = data.BPM;
            PlayInfo.Offset = data.offset / 50000f;
        }

        #endregion

        #region メソッド

        public override void GenerateNotes()
        {
            notesCount = data.notes.Length;
            
            // 読み込んだデータからノーツを生成する
            // ノーツの数だけループさせる
            // [ループ①]
            for (int i = 0; i < data.notes.Length; i++)
            {
                // もし、ノーツのタイプが「2」または「3」=> いずれかのロングノーツであった時
                if (data.notes[i].type == (int)Reference.NoteType.LinearLong
                    || data.notes[i].type == (int)Reference.NoteType.CurvedLong)
                {
                    // ロングノーツのプロパティ（レーン番号や到達時間等）だけを格納するリスト
                    List<float> longNoteTimes = new();
                    List<int> longNoteLaneNumbers = new();

                    // ノーツの到達時間（再生時間）
                    float noteTime = CalculateNoteTime(data.notes[i].LPB, data.notes[i].num);

                    // 各リストへの追加
                    // Lノーツの始点はマニュアル・オートプレイに拘わらず、通常ノーツのリストへ追加する（始点は到達時間で判定するため）
                    notesTimes.Add(noteTime);
                    laneIndexes.Add(data.notes[i].block);
                    notesTypes.Add(data.notes[i].type);

                    // ロングノーツ専用のリストにも追加
                    longNoteTimes.Add(noteTime);
                    longNoteLaneNumbers.Add(data.notes[i].block);

                    // ロングノーツ１まとまりに存在する中間点の数だけループさせる
                    // [ループ②]
                    for (int j = 0; j < data.notes[i].notes.Length; j++)
                    {
                        float _noteTime = CalculateNoteTime(data.notes[i].notes[j].LPB, data.notes[i].notes[j].num);

                        // ゲームをオートでプレイするときは、中間点ノーツの情報も通常ノーツのリストに追加する
                        if (PlayInfo.IsAutoPlay)
                        {
                            notesTimes.Add(_noteTime);
                            laneIndexes.Add(data.notes[i].notes[j].block);
                            notesTypes.Add(data.notes[i].notes[j].type);
                        }

                        longNoteTimes.Add(_noteTime);
                        longNoteLaneNumbers.Add(data.notes[i].notes[j].block);
                    }

                    // 始点・終点以外にいくつかの中間点が存在するノーツは生成時に処理を分岐させたい
                    // そこで、中間点の数を格納するリストを生成側の LongNotesGenerator に作成しておく
                    // notes[i].notes の長さから1引くと中間点の数になる（終点を除外している。）
                    longNotesGenerator.intermediateNotesCounts.Add(data.notes[i].notes.Length - 1);
                    longNotesGenerator.notesTypes.Add(data.notes[i].type);

                    // 各リストへの追加
                    longNotesGenerator.notesTimes.Add(longNoteTimes);
                    longNotesGenerator.laneIndexes.Add(longNoteLaneNumbers);

                    // ノーツ総数に追加
                    notesCount += data.notes[i].notes.Length;

                    // ロングノーツを生成
                    longNotesGenerator.GenerateNotes();
                }
                // ノーツのタイプが通常ノーツであった時
                if (data.notes[i].type == (int)Reference.NoteType.Normal)
                {
                    notesTimes.Add(CalculateNoteTime(data.notes[i].LPB, data.notes[i].num));
                    laneIndexes.Add(data.notes[i].block);
                    notesTypes.Add(data.notes[i].type);

                    // 座標計算
                    // X座標の振り分け
                    float x = GetLaneX(data.notes[i].block);

                    // Z座標の算出
                    float z = notesTimes[^1] * PlayInfo.NoteSpeed + Reference.noteOrigin.z;

                    // ノーツをゲームオブジェクトとして生成する
                    noteInstances.Add(Instantiate(notePrefab, new(x, Reference.noteOrigin.y, z), Quaternion.identity, noteInstanceParent));

                    // プロパティを渡す
                    noteInstances[^1].GetComponent<Note>().Type = Reference.NoteType.Normal;
                    noteInstances[^1].GetComponent<Note>().index = i;
                    noteInstances[^1].name = $"Note-{i}";
                }
            }

            // Linqを使ってノーツの到達時間を降順にソートする
            notesTimes.Reverse();

            SortNotes();

            // 各ノーツにリスト内でのインデックスの情報を渡す
            for (int i = 0; i < noteInstances.Count; i++)
            {
                Note info = noteInstances[i].GetComponent<Note>() ?? noteInstances[i].GetComponent<LongNote>();
                info.noteIndex = i;
            }

            // ロングノーツの整理
            longNotesGenerator.SortNotes();
        }

        /// <summary>
        /// ノーツの判定線への到達時間を求める。
        /// </summary>
        /// <param name="lpb">そのノーツの小説位置</param>
        /// <param name="index">そのノーツのインデックス</param>
        /// <returns>到達時間</returns>
        private float CalculateNoteTime(int lpb, int index)
        {
            // 1拍あたりの秒数
            float secPerBeat = 60f / data.BPM;
            // ノーツの間隔が最小のときの位置（小節位置）
            float minDistance = secPerBeat / lpb;
            // ノーツの判定線への到達時間
            // 小節位置に与えられた小節の通し番号（インデックス）を乗算して実際の再生時間を算出する
            float noteTime = index * minDistance + PlayInfo.JudgingTiming;

            return noteTime + data.offset / 50000f;
        }

        public override void SortNotes()
        {
            // Z座標の降順（＝到達順）でソートしたインデックスを取得し、各リストに適用する
            // OrderbyDescending が遅延評価のため、ToList でインスタンスを生成して確定させる
            var orderedByReachingIndexes = Enumerable.Range(0, noteInstances.Count)
                .OrderByDescending(i => noteInstances[i].transform.position.z).ToList();

            var sortedInstances = orderedByReachingIndexes.Select(i => noteInstances[i]).ToList();
            var sortedLanes     = orderedByReachingIndexes.Select(i => laneIndexes[i]).ToList();
            var sortedTypes     = orderedByReachingIndexes.Select(i => notesTypes[i]).ToList();

            noteInstances.Clear();
            laneIndexes.Clear();
            notesTypes.Clear();

            // 指定しなおし
            noteInstances.AddRange(sortedInstances);
            laneIndexes.AddRange(sortedLanes);
            notesTypes.AddRange(sortedTypes);
        }

        #endregion
    }
}

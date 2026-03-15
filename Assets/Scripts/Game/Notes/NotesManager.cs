using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FRONTIER.Utility;

namespace FRONTIER.Game.Notes
{
    /// <summary>
    /// ノーツの生成と管理を行うクラス。
    /// </summary>
    [RequireComponent(typeof(LongNotesManager))]
    public class NotesManager : NotesManagerBase<int, float, Note>
    {
        #region フィールド

        /// <summary>
        /// 楽曲に含まれる総ノーツ数。
        /// </summary>
        [SerializeField] private int notesCount;

        /// <summary>
        /// 通常ノーツのプレハブ。
        /// </summary>
        [SerializeField] private GameObject notePrefab;

        /// <summary>
        /// ロングノーツを管理するスクリプト。
        /// </summary>
        [SerializeField] private LongNotesManager longNotesGenerator;

        /// <summary>
        /// 譜面データ。
        /// </summary>
        [SerializeField] private NotePatternData patternData;

        #endregion

        #region MonoBehaviorメソッド

        void Awake()
        {
            patternData = LoadNotePattern(PlayInfo.ID, PlayInfo.FromDifficulty(PlayInfo.Difficulty).Item1);

            GenerateNotes();

            // 最大スコアの計算
            Manager.score.maxScoreValue = notesCount * Reference.JudgementRankValues.PERFECT;
            Manager.score.maxComboCount = notesCount;

            // 譜面データからわかることをプレイ状況に反映
            PlayInfo.Bpm = patternData.BPM;
            PlayInfo.Offset = patternData.offset / 50000f;
        }

        #endregion

        #region メソッド

        public override void GenerateNotes()
        {
            // ロングノーツがあれば随時加算される
            notesCount = patternData.notes.Length;
            
            // 読み込んだデータからノーツを生成する
            // ノーツの数だけループさせる
            for (int i = 0; i < patternData.notes.Length; i++)
            {
                // もし、ノーツのタイプが「2」または「3」=> いずれかのロングノーツであった時
                if (patternData.notes[i].type == (int)Reference.NoteType.LinearLong
                    || patternData.notes[i].type == (int)Reference.NoteType.CurvedLong)
                {
                    // ロングノーツのプロパティ（レーン番号や到達時間等）だけを格納するリスト
                    List<float> longNoteReachedTimes = new();
                    List<int> longNoteLaneIndexes = new();

                    // ロングノーツ専用のリストに追加
                    longNoteReachedTimes.Add(CalculateReachedTime(patternData.notes[i].LPB, patternData.notes[i].num));
                    longNoteLaneIndexes.Add(patternData.notes[i].block);

                    // ロングノーツ１まとまりに存在する中間点の数だけループさせる
                    for (int j = 0; j < patternData.notes[i].notes.Length; j++)
                    {
                        longNoteReachedTimes.Add(CalculateReachedTime(patternData.notes[i].notes[j].LPB, patternData.notes[i].notes[j].num));
                        longNoteLaneIndexes.Add(patternData.notes[i].notes[j].block);

                        // タイプをロングノーツ専用のリストに追加
                        longNotesGenerator.AddToTypes((Reference.NoteType)patternData.notes[i].type);
                    }

                    // 始点・終点以外にいくつかの中間点が存在するノーツは生成時に処理を分岐させたい
                    // そこで、中間点の数を格納するリストを生成側の LongNotesGenerator に作成しておく
                    // notes[i].notes の長さから1引くと中間点の数になる（終点を除外している。）
                    longNotesGenerator.intermediateNotesCounts.Add(patternData.notes[i].notes.Length - 1);
                    longNotesGenerator.AddToTypes((Reference.NoteType)patternData.notes[i].type);

                    // 各リストへの追加
                    longNotesGenerator.AddToReachedTimes(longNoteReachedTimes);
                    longNotesGenerator.AddToLaneIndexes(longNoteLaneIndexes);

                    // ノーツ総数に追加
                    notesCount += patternData.notes[i].notes.Length;

                    // ロングノーツを生成
                    longNotesGenerator.GenerateNotes();
                }
                // ノーツのタイプが通常ノーツであった時
                else if (patternData.notes[i].type == (int)Reference.NoteType.Normal)
                {
                    reachedTimes.Add(CalculateReachedTime(patternData.notes[i].LPB, patternData.notes[i].num));
                    laneIndexes.Add(patternData.notes[i].block);
                    types.Add((Reference.NoteType)patternData.notes[i].type);

                    // 座標計算
                    // X座標の振り分け
                    float x = GetLaneX(patternData.notes[i].block);

                    // Z座標の算出
                    float z = reachedTimes[^1] * PlayInfo.NoteSpeed + Reference.noteOrigin.z;

                    // ノーツをゲームオブジェクトとして生成する
                    GameObject instance = Instantiate(notePrefab, new(x, Reference.noteOrigin.y, z), Quaternion.identity, instanceParent);

                    // そのコンポーネントを追加
                    notes.Add(instance.GetComponent<Note>());

                    // プロパティを渡す
                    notes[^1].Type = Reference.NoteType.Normal;
                    notes[^1].ArrivalOrder = i;
                    notes[^1].ReachedTime = reachedTimes[^1];
                    notes[^1].LaneIndex = laneIndexes[^1];
                    notes[^1].name = $"Note-{i}";
                }
            }

            SortNotes();

            // ロングノーツの整理
            longNotesGenerator.SortNotes();
        }

        public override void SortNotes()
        {
            // ノーツの到達時間を降順にソートする
            // 早く着くものから順に入れたので、すべて逆順にすればいい
            reachedTimes.Reverse();
            laneIndexes.Reverse();
            types.Reverse();
            notes.Reverse();
        }

        /// <summary>
        /// ノーツの判定線への到達時間を求める。
        /// </summary>
        /// <param name="lpb">そのノーツの小説位置</param>
        /// <param name="index">そのノーツのインデックス</param>
        /// <returns>到達時間</returns>
        private float CalculateReachedTime(int lpb, int index)
        {
            // 1拍あたりの秒数
            float secPerBeat = 60f / patternData.BPM;
            // ノーツの間隔が最小のときの位置（小節位置）
            float minDistance = secPerBeat / lpb;
            // ノーツの判定線への到達時間
            // 小節位置に与えられた小節の通し番号（インデックス）を乗算して実際の再生時間を算出する
            float noteTime = index * minDistance + PlayInfo.JudgingTiming;

            return noteTime + patternData.offset / 50000f;
        }

        #endregion
    }
}

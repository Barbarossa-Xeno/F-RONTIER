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
        /// ノーツのプレハブ。
        /// </summary>
        public NotePrefabs notePrefabs = new();

        /// <summary>
        /// ロングノーツを管理するスクリプト。
        /// </summary>
        [SerializeField] private LongNotesGenerator longNotesGenerator;

        /// <summary>
        /// 譜面データ。
        /// </summary>
        [SerializeField] private NotePatternData data;

        /// <summary>
        /// ノーツの情報を格納する各リスト（<see cref = "notesObjects"/>など）をノーツの降る順番に合わせて
        /// ソートするためにソートの基底にするリスト
        /// </summary>
        private List<float> notePositionZBase;

        #endregion

        #region 構造体

        [System.Serializable]
        public struct NotePrefabs
        {
            /// <summary>
            /// 通常ノーツのプレハブ。
            /// </summary>
            public GameObject normal;

            /// <summary>
            /// 中間点を有する直線型ロングノーツのプレハブ。
            /// </summary>
            public GameObject longAny;

            /// <summary>
            /// 中間点を持たない直線型ロングノーツのプレハブ。
            /// </summary>
            public GameObject longOnly;
        }

        #endregion

        #region Monobehaviorメソッド

        void Awake()
        {
            notesCount = 0;
            data = base.LoadNotePattern(Manager.info.ID, Manager.info.DifficultyTo(Manager.info.Difficulty).Item1);
            GenerateNotes();
            //最大スコアの計算
            GameManager.instance.scoreManager.maxScore = notesCount * 5;
        }

        #endregion

        #region メソッド

        public override void GenerateNotes()
        {
            // 値の代入と受け渡し
            notesCount = data.notes.Length;
            GameManager.instance.musicManager.bpm = data.BPM;

            // 読み込んだデータからノーツを生成する
            // ノーツの数だけループさせる
            // [ループ①]
            for (int i = 0; i < data.notes.Length; i++)
            {
                //もし、ノーツのタイプが「2」または「３」=> いずれかのロングノーツであった時
                if (data.notes[i].type == (int)Reference.NoteType.LongLinear || data.notes[i].type == (int)Reference.NoteType.LongCurve)
                {
                    // ロングノーツのプロパティ（レーン番号や到達時間等）だけを格納するリスト
                    List<float> longNoteTimes = new();
                    List<int> longNoteLaneNumbers = new();

                    // ノーツの到達時間（再生時間）
                    float noteTime = CalculateNoteTime(data.notes[i].LPB, data.notes[i].num);

                    // 各リストへの追加
                    // Lノーツの始点はマニュアル・オートプレイに拘わらず、通常ノーツのリストへ追加する（始点は到達時間で判定するため）
                    notesTimes.Add(noteTime);
                    laneNumbers.Add(data.notes[i].block);
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
                        if (Manager.AutoPlay)
                        {
                            notesTimes.Add(_noteTime);
                            laneNumbers.Add(data.notes[i].notes[j].block);
                            notesTypes.Add(data.notes[i].notes[j].type);
                        }

                        longNoteTimes.Add(_noteTime);
                        longNoteLaneNumbers.Add(data.notes[i].notes[j].block);
                    }

                    // 始点・終点以外にいくつかの中間点が存在するノーツは生成時に処理を分岐させたい
                    // そこで、中間点の数を格納するリストを生成側のLongNotesGeneratorに作成しておく
                    // notes[i].notesの長さから1引くと中間点の数になる（終点を除外している。）
                    longNotesGenerator.innerNotesCounts.Add(data.notes[i].notes.Length - 1);
                    longNotesGenerator.notesTypes.Add(data.notes[i].type);

                    // 各リストへの追加
                    longNotesGenerator.notesTimes.Add(longNoteTimes);
                    longNotesGenerator.laneNumbers.Add(longNoteLaneNumbers);

                    // ノーツ総数に追加
                    notesCount += data.notes[i].notes.Length;

                    // ロングノーツを生成
                    longNotesGenerator.GenerateNotes();
                }
                // ノーツのタイプが通常ノーツであった時
                if (data.notes[i].type == (int)Reference.NoteType.Normal)
                {
                    notesTimes.Add(CalculateNoteTime(data.notes[i].LPB, data.notes[i].num));
                    laneNumbers.Add(data.notes[i].block);
                    notesTypes.Add(data.notes[i].type);

                    // 座標計算
                    // X座標の振り分け
                    float positionX = SwitchNoteLane(data.notes[i].block);

                    // Z座標の算出
                    float positionZ = notesTimes[^1] * Manager.NoteSpeed + Reference.noteOrigin.z;

                    // ノーツをゲームオブジェクトとして生成する
                    notesObjects.Add(Instantiate(notePrefabs.normal, new(positionX, Reference.noteOrigin.y, positionZ), Quaternion.identity, noteObjectParent));

                    // プロパティを渡す
                    notesObjects[^1].GetComponent<Notes>().type = Reference.NoteType.Normal;
                    notesObjects[^1].GetComponent<Notes>().index = i;
                    notesObjects[^1].name = $"Note_{i}";
                }
            }

            // Linqを使ってノーツの到達時間を降順にソートする
            notesTimes.Reverse();

            // ノーツのオブジェクトを到達順に整理したいが、Linqを使ったソートでは方法が思いつかないため、オブジェクトのZ座標を利用する
            notePositionZBase = notesObjects.Select(note => note.transform.position.z).ToList();
            NotesSort();

            // 各ノーツにリスト内でのインデックスの情報を渡す
            for (int i = 0; i < notesObjects.Count; i++)
            {
                Notes info = notesObjects[i].GetComponent<Notes>() ?? notesObjects[i].GetComponent<LongNotes>();
                info.indexOfList = i;
            }

            // ロングノーツの整理
            longNotesGenerator.NotesSort();
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
            float secPerBeat = 60f / (float)data.BPM;
            // ノーツの間隔が最小のときの位置（小節位置）
            float minDistance = secPerBeat / (float)lpb;
            // ノーツの判定線への到達時間
            // 小節位置に与えられた小節の通し番号（インデックス）を乗算して実際の再生時間を算出する
            float noteTime = (float)index * minDistance + Manager.JudgingTiming;

            return noteTime;
        }

        /// <summary>
        /// ノーツのZ座標をもとに、ノーツの情報のリストをノーツが流れてくる順番通りにソートする。
        /// </summary>
        /// <remarks>
        /// 両リストは、同じインデックスに同じ対象のノーツの情報を格納しているので、
        /// バブルソートで交換する時にそのインデックス番号を使って、対象リストの値を交換できる
        /// </remarks>
        /// <returns>ソート済みリスト</returns>
        /// <param name = "targetList">整列する前のノーツのオブジェクトのリスト。</param>
        /// <param name = "baseList">Z座標を格納したリスト</param>
        /// <param name = "isAscending"><c>true</c>なら昇順、<c>false</c>なら降順</param>
        private List<T> NotesSort<T>(List<T> targetList, bool isAscending)
        {
            float tmp = 0;
            T target_tmp = default;

            // リストは変更が参照されて引数に設定した元のリストまで変わるので実体コピーすること。
            List<float> clone = new(notePositionZBase);

            if (isAscending)
            {
                for (int i = 0; i < clone.Count; i++)
                {
                    for (int j = clone.Count - 1; j > i; j--)
                    {
                        if (clone[j - 1] > clone[j])
                        {
                            tmp = clone[j - 1];
                            target_tmp = targetList[j - 1];
                            clone[j - 1] = clone[j];
                            targetList[j - 1] = targetList[j];
                            clone[j] = tmp;
                            targetList[j] = target_tmp;
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
                            target_tmp = targetList[j - 1];
                            clone[j - 1] = clone[j];
                            targetList[j - 1] = targetList[j];
                            clone[j] = tmp;
                            targetList[j] = target_tmp;
                        }
                    }
                }
            }
            return targetList;
        }

        public override void NotesSort()
        {
            // ノーツオブジェクトのソート
            NotesSort(notesObjects, false);
            // ノーツレーンのソート
            NotesSort(laneNumbers, false);
            // ノーツタイプのソート
            NotesSort(notesTypes, false);
        }

        #endregion
    }
}
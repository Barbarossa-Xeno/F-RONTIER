using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using FRONTIER.Game.NotesManagement;
using FRONTIER.Game.InputManagement;
using FRONTIER.Audio;
using FRONTIER.Utility;
using static FRONTIER.Utility.Reference;

namespace FRONTIER.Game
{
    /// <summary>
    /// ノーツの判定をする。
    /// </summary>
    public class JudgementManager : GameUtility
    {
        #region フィールド

        /// <summary>
        /// スコア表示に関する様々な情報。
        /// </summary>
        [SerializeField] private Score score;

        /// <summary>
        /// <see cref = "NotesGenerator"/>
        /// </summary>
        [SerializeField] private NotesGenerator notesGenerator;

        /// <summary>
        /// <see cref = "LongNotesGenerator"/>
        /// </summary>
        [SerializeField] private LongNotesGenerator longNotesGenerator;

        /// <summary>
        /// <see cref = "InputManager"/>
        /// </summary>
        [SerializeField] private InputManager inputManager;

        /// <summary>
        /// ノーツが削除されたタイミングで発火するイベント。
        /// </summary>
        [Header("ノーツが削除されたタイミングで発火するイベントを登録する"), SerializeField] private UnityEvent OnNoteDeleted;

        /// <summary>
        /// 判定の対象とするノーツ。
        /// </summary>
        private TargetNote target = new();

        /// <summary>
        /// <c>Item1</c> => タップされたレーンにあり、条件によって絞り込まれたノーツ
        /// <c>Item2</c> => <c>Item1</c>のインデックス。
        /// </summary>
        private List<List<GameObject>> eachLanesNotes = new(6)
        {
            new(), new(), new(), new(), new(), new()
        };

        /// <summary>
        /// 判定ステータスの基準。
        /// </summary>
        private static readonly Dictionary<JudgementStatus, float> judgementTime = new()
        {
            { JudgementStatus.Perfect, 0.08f },
            { JudgementStatus.Great, 0.12f },
            { JudgementStatus.Good, 0.25f },
            { JudgementStatus.Bad, 0.4f },
            { JudgementStatus.Miss, 0.6f }
        };

        #endregion

        #region クラス・構造体

        /// <summary>
        /// スコア表示を管理する。
        /// </summary>
        [Serializable]
        private struct Score
        {
            /// <summary>
            /// 判定ステータス表示のオブジェクトプール。
            /// </summary>
            public ScoreObjectPool objectPool;

            /// <summary>
            /// 生成される判定ステータスのオブジェクト。
            /// </summary>
            public GameObject Object { get; set; }

            /// <summary>
            /// <see cref="Object"/>を配置するときの親オブジェクト。
            /// </summary>
            public Transform parent;
        }

        /// <summary>
        /// 判定の対象とするノーツを記録する。
        /// </summary>
        private class TargetNote
        {
            /// <summary>
            /// 判定の対象とするノーツのオブジェクト。
            /// </summary>
            public GameObject note;

            /// <summary>
            /// 判定の対象とするノーツの情報。
            /// </summary>
            public Notes info;
        }

        #endregion

        #region MonoBehaviorメソッド

        void Start()
        {

            // タップしたときのイベントを登録する
            inputManager.onInput.ToList().ForEach(tapEvent => tapEvent.AddListener((index, time) => JudgeNote(index, time)));

            // ノーツが判定線を越えたときのイベントを登録する
            notesGenerator.notesObjects.ForEach
            (
                note =>
                {
                    // 通常時
                    if (!Manager.info.IsAutoPlay)
                    { 
                        Notes info = note.GetComponent<Notes>() ?? note.GetComponent<LongNotes>();
                        // 判定線を超過して画面の外に出たらミスにする
                        info.OnReachedJudgement += () => DeleteNote(targetIndex: info.indexOfList, isMissed: true);
                    }
                    // オート時
                    else
                    {
                        // 判定線あたりでノーツをPerfect判定する
                        Notes info = note.GetComponent<Notes>() ?? note.GetComponent<LongNotes>();
                        // ノーツがロングノーツだったら、始点・中間点・終点のノーツだけイベントを登録するようにする
                        if (info.type == NoteType.LongLinear || info.type == NoteType.LongCurve)
                        {
                            // ダウンキャスト
                            LongNotes _info = info as LongNotes;
                            if (!(_info.status == LongNoteStatus.Mesh || _info.status == LongNoteStatus.None))
                            {
                                _info.OnReachedJudgement += () => DeleteNote(_info.indexOfList, isAuto: true);
                            }
                        }
                        // 通常ノーツのときは関係なくイベントを登録
                        else
                        {
                            info.OnReachedJudgement += () => DeleteNote(info.indexOfList, isAuto: true);
                        }
                    }
                }
            );

            // ロングノーツの判定のイベントを登録する
            longNotesGenerator.longNoteMeshList.Select(line => line.GetComponent<LongNotes>())
                                               .ToList().ForEach
                                               (
                                                    info =>
                                                    info.OnPressedUpdate += isOn =>
                                                    JudgeLongNote(isOn, info.isInner, info.index)
                                               );
        }

        #endregion

        #region メソッド

        /// <summary>
        /// ターゲットにするノーツを取得した後に判定処理を行う。
        /// </summary>
        /// <param name="laneIndex"></param>
        /// <param name="tapTime"></param>
        public void JudgeNote(int laneIndex, float tapTime)
        {
            #region ローカルメソッド

            // ノーツの現在位置と判定線位置との差を求める
            static float GetNotePositionDifference(float notePosition) => notePosition - noteOrigin.z;

            // タップされたレーンと押された時間をもとにノーツの判定をする
            /// <param name="laneIndex">タップされたレーン</param>
            /// <param name="tapTime">タップされた時間</param>
            void Judge(int laneIndex, float tapTime)
            {
                // インデックスがオーバーしたときのことを考えて、例外はキャッチだけする
                try
                {
                    if (laneIndex == notesGenerator.laneNumbers[^1])
                    {
                        JudgeStatus(CalculateLag(tapTime, notesGenerator.notesTimes[^1]));
                    }
                    else if (laneIndex == notesGenerator.laneNumbers[^2])
                    {
                        JudgeStatus(CalculateLag(tapTime, notesGenerator.notesTimes[^2]));
                    }
                    else if (laneIndex == notesGenerator.laneNumbers[^3])
                    {
                        JudgeStatus(CalculateLag(tapTime, notesGenerator.notesTimes[^3]));
                    }
                    else if (laneIndex == notesGenerator.laneNumbers[^4])
                    {
                        JudgeStatus(CalculateLag(tapTime, notesGenerator.notesTimes[^4]));
                    }
                }
                catch (ArgumentOutOfRangeException) { }
            }

            #endregion

            // そのレーンを流れるノーツオブジェクトが入るリストの初期化
            eachLanesNotes[laneIndex].Clear();

            // そのレーンを流れるノーツの中で一番近そうなノーツを何個か取得
            foreach (GameObject note in notesGenerator.notesObjects)
            {
                if (note.transform.position.x == SwitchNoteLane(laneIndex))
                {
                    float differenceZ = GetNotePositionDifference(note.transform.position.z);
                    if (differenceZ > -1.5f && differenceZ < 5f)
                    {
                        eachLanesNotes[laneIndex].Add(note);
                    }
                }
            }

            // 該当したノーツが１つもないようなら処理を抜ける
            if (eachLanesNotes[laneIndex].Count == 0) { return; }

            // 最接近しているノーツは抽出したノーツリストのうち、
            // そのz座標と判定線との距離の差が最も小さいものを１つ抽出
            // 参照値をリストの一番初めの要素のz座標で初期化
            float reference = eachLanesNotes[laneIndex][0].transform.position.z;
            int targetIndex = 0;

            for (int i = 0; i < eachLanesNotes[laneIndex].Count; i++)
            {
                // 参照値よりもZ座標の差が小さいものがあるか確認する
                if (eachLanesNotes[laneIndex][i].transform.position.z - noteOrigin.z < reference - noteOrigin.z)
                {
                    // あれば、参照値を変更し、その番号を記憶する
                    reference = eachLanesNotes[laneIndex][i].transform.position.z;
                    targetIndex = i;
                }
            }
            // 抽出できたものをターゲットノーツとする
            target.note = eachLanesNotes[laneIndex][targetIndex];
            target.info = target.note.GetComponent<Notes>() ?? target.note.GetComponent<LongNotes>();

            // 便宜上、ノーツの種類のよって処理を分ける
            if (target.info.type == NoteType.Normal)
            {
                Judge(laneIndex, tapTime);
            }
            else if (target.info.type == NoteType.LongLinear || target.info.type == NoteType.LongCurve)
            {
                Judge(laneIndex, tapTime);
            }
        }

        /// <summary>
        /// ロングノーツが押されているかに応じて、その中間点のノーツを判定する
        /// </summary>
        /// <param name="isPressed">ロングノーツが押されているか</param>
        /// <param name="isInner">ロングノーツが終点以外の中間点を持っているか</param>
        /// <param name="longNoteIndex">ロングノーツに割り振られた順番</param>
        private void JudgeLongNote(bool isPressed, bool isInner, int longNoteIndex)
        {
            // カウンタ変数をメソッドスコープで宣言して、メソッド内で使いまわす
            int i;

            // 指定されたロングノーツの順番に照応するロングノーツラインを探す
            for (i = 1; i <= longNotesGenerator.notesObjects.Count; i++)
            {
                // インデックスが同じコンポーネントがあったら、そのインデックスを i にコピー
                if (longNotesGenerator.longNotesList[^i].index == longNoteIndex) { break; }
            }

            // 中間点を持つ場合 -> 中間点と終点のチェック
            if (isInner)
            {
                // 中間ノーツがロングノーツのまとまりごとに収まっているリストにおいて、
                // i番目のロングノーツの中間ノーツのリストの、一番最後の要素が最も近い中間ノーツになる
                // その中間ノーツが判定線まで近づいたとき (押下判定が外れてしまうタイミングを考慮して、余分に距離を見積もる)
                if (longNotesGenerator.notesObjects[^i][^1].transform.position.z <= noteOrigin.z + 1f)
                {
                    // その中間ノーツが終点の場合
                    if (longNotesGenerator.notesObjects[^i][^1].GetComponent<LongNotes>().status == LongNoteStatus.End)
                    {
                        DeleteNote(longNotesGenerator.notesObjects, ^i, isPressed);
                    }
                    // その中間ノーツが中間点の場合
                    else if (longNotesGenerator.notesObjects[^i][^1].GetComponent<LongNotes>().status == LongNoteStatus.Inner)
                    {
                        DeleteNote(longNotesGenerator.notesObjects, ^i, isPressed, ^1);
                    }
                }
            }
            // 中間点を持たない場合 -> 終点のみのチェック
            else
            {
                // 説明略（中間点を持つ場合の処理と同じ）
                if (longNotesGenerator.notesObjects[^i][^1].transform.position.z <= noteOrigin.z + 1f)
                {
                    if (longNotesGenerator.notesObjects[^i][^1].GetComponent<LongNotes>().status == LongNoteStatus.End)
                    {
                        DeleteNote(longNotesGenerator.notesObjects, ^i, isPressed);
                    }
                }
            }
        }

        /// <summary>
        /// ノーツが押されたときのラグに合わせて、判定をする。
        /// </summary>
        /// <param name = "timeLag">実際にノーツが押された時間と押されるべき時間とのラグ。</param>
        private void JudgeStatus(float timeLag)
        {
            // ターゲットノーツがリストに存在するか確認する
            if (notesGenerator.notesObjects.Contains(target.note))
            {
                // ラグを判定幅に照応させて判定する
                if (timeLag <= judgementTime[JudgementStatus.Perfect])
                {
                    Manager.audios.seManager.Play(SEManager.SE.GreatOrPerfect);
                    ShowScoreStatus(JudgementStatus.Perfect);
                    Manager.score.apparentScoreValue += JudgementStatusScore.PERFECT;
                    Manager.score.judgementStatus[JudgementStatus.Perfect]++;
                    Manager.score.combo++;
                }
                else if (timeLag <= judgementTime[JudgementStatus.Great])
                {
                    Manager.audios.seManager.Play(SEManager.SE.GreatOrPerfect);
                    ShowScoreStatus(JudgementStatus.Great);
                    Manager.score.apparentScoreValue += JudgementStatusScore.GREAT;
                    Manager.score.judgementStatus[JudgementStatus.Great]++;
                    Manager.score.combo++;
                }
                else if (timeLag <= judgementTime[JudgementStatus.Good])
                {
                    Manager.audios.seManager.Play(SEManager.SE.GoodOrBad);
                    ShowScoreStatus(JudgementStatus.Good);
                    Manager.score.apparentScoreValue += JudgementStatusScore.GOOD;
                    Manager.score.judgementStatus[JudgementStatus.Good]++;
                    Manager.score.combo++;
                    // 絶対精度良くないから、Goodまではコンボ許容しないと俺が怒るぜ
                }
                else if (timeLag <= judgementTime[JudgementStatus.Bad])
                {
                    Manager.audios.seManager.Play(SEManager.SE.GoodOrBad);
                    ShowScoreStatus(JudgementStatus.Bad);
                    Manager.score.apparentScoreValue += JudgementStatusScore.BAD;
                    Manager.score.judgementStatus[JudgementStatus.Bad]++;
                    Manager.score.combo = 0;
                }

                // スコア計算
                Manager.score.CalculateScore();

                // ノーツを消す
                DeleteNote();
            }
        }

        /// <summary>
        /// 判定が終わったターゲットノーツ <see cref="target"/> を削除する。
        /// </summary>
        private void DeleteNote()
        {
            target.note.SetActive(false);
            int index = notesGenerator.notesObjects.IndexOf(target.note);
            notesGenerator.notesTimes.RemoveAt(index);
            notesGenerator.laneNumbers.RemoveAt(index);
            notesGenerator.notesTypes.RemoveAt(index);
            notesGenerator.notesObjects.RemoveAt(index);

            OnNoteDeleted?.Invoke();
        }

        /// <summary>
        /// オートプレイの時や、判定線を超過してミス判定になった時にノーツを消す。
        /// </summary>
        /// <param name = "targetIndex">
        /// 判定対象になったノーツの、リスト内のインデックス。<br/>
        /// ※オートプレイ時にインデックスを指定した際は、メソッド内で自動的に後ろから数えた時のインデックス（<c>System.Index</c>）に変換する
        /// </param>
        /// <param name="isAuto">オートプレイの判定の時、<c>true</c>を指定する。</param>
        /// <param name="isMissed">ノーツのミス判定をとるとき、<c>true</c>を指定する。</param>
        private void DeleteNote(int targetIndex, bool isAuto = false, bool isMissed = false)
        {
            if (isAuto)
            {
                // 同じノーツを二度判定することがないように、ノーツのアクティブ状態を確認する
                if (notesGenerator.notesObjects[targetIndex].activeSelf)
                {
                    // ノーツをリストから削除せずに形だけ消す
                    notesGenerator.notesObjects[targetIndex].SetActive(false);
                }
                else return; 

                Manager.audios.seManager.Play(SEManager.SE.GreatOrPerfect);

                // スコア計算
                ShowScoreStatus(JudgementStatus.Perfect);
                Manager.score.apparentScoreValue += JudgementStatusScore.PERFECT;
                Manager.score.judgementStatus[JudgementStatus.Perfect]++;
                Manager.score.combo++;
                Manager.score.CalculateScore();
            }
            else if (isMissed)
            {
                // 同タイミングで判定線を通過するノーツは、インデックスが被って上手くリストから削除できないことがあるので
                // リストのカウントを超えないようにtargetIndexを予め調整する
                if (targetIndex <= notesGenerator.notesObjects.Count)
                {
                    int i = 0;
                    while (true)
                    {
                        if (targetIndex - i < notesGenerator.notesObjects.Count) { break; }
                        else { i++; }
                    }
                    targetIndex -= i;
                }

                // ノーツをリストから削除
                notesGenerator.notesObjects[targetIndex].SetActive(notesGenerator.notesObjects[targetIndex].activeSelf ? false : false);
                notesGenerator.notesTimes.RemoveAt(targetIndex);
                notesGenerator.laneNumbers.RemoveAt(targetIndex);
                notesGenerator.notesTypes.RemoveAt(targetIndex);
                notesGenerator.notesObjects.RemoveAt(targetIndex);

                // スコア計算
                ShowScoreStatus(JudgementStatus.Miss);
                Manager.score.judgementStatus[JudgementStatus.Miss]++;
                Manager.score.combo = 0;
            }

            OnNoteDeleted?.Invoke();
        }

        /// <summary>
        /// 判定線を超過したロングノーツの中間点・終点を削除する。
        /// </summary>
        /// <param name="innerNotesList">ロングノーツの中間点が格納されたリスト</param>
        /// <param name="targetLongNoteListIndex">ターゲットにするロングノーツのリストでのインデックス</param>
        /// <param name="isPressed">ロングノーツのラインが押されているか</param>
        /// <param name="targetIndex">消す中間点のインデックス</param>
        private void DeleteNote(List<List<GameObject>> innerNotesList, Index targetLongNoteListIndex, bool isPressed, Index targetIndex = default)
        {
            // 押下の有無を判別
            if (isPressed)
            {
                // 押されていたまま判定線を超過したら、Perfectで判定をとる
                Manager.audios.seManager.Play(SEManager.SE.GreatOrPerfect);
                ShowScoreStatus(JudgementStatus.Perfect);
                Manager.score.apparentScoreValue += JudgementStatusScore.PERFECT;
                Manager.score.judgementStatus[JudgementStatus.Perfect]++;
                Manager.score.combo++;
                Manager.score.CalculateScore();
            }
            else
            {
                // 押されてなかったらミス
                ShowScoreStatus(JudgementStatus.Miss);
                Manager.score.judgementStatus[JudgementStatus.Miss]++;
                Manager.score.combo = 0;
            }

            // ターゲットとする中間点（終点以外）のインデックスの指定があったとき
            if (targetIndex.Value > 0)
            {
                // 中間点を隠してリストから消す
                innerNotesList[targetLongNoteListIndex][targetIndex].SetActive(false);
                innerNotesList[targetLongNoteListIndex].RemoveAt(targetIndex);
            }
            // 終点を消すとき
            else
            {
                innerNotesList[targetLongNoteListIndex][0].SetActive(false);
                innerNotesList[targetLongNoteListIndex].RemoveAt(0);
                innerNotesList.RemoveAt(targetLongNoteListIndex);
                // ロングノーツラインのリストからも消す
                longNotesGenerator.longNoteMeshList[targetLongNoteListIndex].SetActive(false);
                longNotesGenerator.longNoteMeshList.RemoveAt(targetLongNoteListIndex);
                longNotesGenerator.longNotesList.RemoveAt(targetLongNoteListIndex);
            }

            OnNoteDeleted?.Invoke();
        }

        /// <summary>
        /// 判定ステータスを画面上に表示する。
        /// </summary>
        /// <remarks>
        /// オブジェクトプール(<see cref = "ScoreObjectPool"/>)を利用する
        /// </remarks>
        private void ShowScoreStatus(JudgementStatus status)
        {
            switch (status)
            {
                case JudgementStatus.Perfect:
                    score.Object = score.objectPool.perfect.Get();
                    break;
                case JudgementStatus.Great:
                    score.Object = score.objectPool.great.Get();
                    break;
                case JudgementStatus.Good:
                    score.Object = score.objectPool.good.Get();
                    break;
                case JudgementStatus.Bad:
                    score.Object = score.objectPool.bad.Get();
                    break;
                case JudgementStatus.Miss:
                    score.Object = score.objectPool.miss.Get();
                    break;
            }
            score.Object.transform.SetParent(score.parent);
            score.Object.transform.position = new(0, 0, 0);
            score.Object.transform.rotation = Quaternion.Euler(60, 0, 0);
        }

        /// <summary>
        /// 実際にノーツがタップされた時間と、本来ノーツをタップすべき時間との差を求める。
        /// </summary>
        /// <param name="tapTime">タップエリアをタップした時間</param>
        /// <param name="noteTime">本来ノーツをタップすべき時間</param>
        /// <returns>タイムラグ</returns>
        private float CalculateLag(float tapTime, float noteTime) => Mathf.Abs(Manager.startTime + noteTime - tapTime);

        #endregion
    }
}
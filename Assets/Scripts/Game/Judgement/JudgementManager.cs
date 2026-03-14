using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using FRONTIER.Game.Notes;
using FRONTIER.Game.Judgement;
using FRONTIER.Audio;
using FRONTIER.Utility;
using static FRONTIER.Utility.Reference;

namespace FRONTIER.Game.Judgement
{
    /// <summary>
    /// ノーツの判定をする。
    /// </summary>
    public class JudgementManager : GameUtilityBase
    {
        #region フィールド

        /// <summary>
        /// <see cref = "NotesManager"/>
        /// </summary>
        [SerializeField] private NotesManager notesGenerator;

        /// <summary>
        /// <see cref = "LongNotesManager"/>
        /// </summary>
        [SerializeField] private LongNotesManager longNotesGenerator;

        /// <summary>
        /// <see cref = "LaneManager"/>
        /// </summary>
        [SerializeField] private LaneManager laneManager;

        /// <summary>
        /// 
        /// </summary>
        [SerializeField] private ScoreManager scoreManager;

        /// <summary>
        /// ノーツが判定されたタイミングで発火するイベント。
        /// </summary>
        [Header("ノーツが判定されたタイミングで発火するイベントを登録する"), SerializeField] private UnityEvent<Note> noteJudged;

        /// <summary>
        /// 判定の対象とするノーツ。
        /// </summary>
        [SerializeField] private Note target;

        /// <summary>
        /// 1次元目にレーン番号、2次元目にそのレーンを流れるノーツを格納するリスト。
        /// </summary>
        private readonly List<List<Note>> EachLanesNotes = Enumerable.Range(0, 6).Select(_ => new List<Note>()).ToList();

        #endregion

        #region MonoBehaviorメソッド

        void Start()
        {
            if (!Manager.info.IsAutoPlay)
            {
                // タップしたときのイベントを登録する
                Array.ForEach(laneManager.Lanes, lane => lane.TappedEvent += (index, time) => JudgeNote(index, time));
            }

            // ノーツが判定線を越えたときのイベントを登録する
            notesGenerator.instances.ForEach
            (
                note =>
                {
                    // 通常時
                    if (!Manager.info.IsAutoPlay)
                    {
                        // 判定線を超過して画面の外に出たらミスにする
                        note.PassedOverLine += () =>
                        {
                            int currentNoteIndex = notesGenerator.instances.IndexOf(note);
                            // ノーツをリストから削除
                            notesGenerator.instances[currentNoteIndex].gameObject.SetActive(false);
                            notesGenerator.reachedTimes.RemoveAt(currentNoteIndex);
                            notesGenerator.laneIndexes.RemoveAt(currentNoteIndex);
                            notesGenerator.types.RemoveAt(currentNoteIndex);
                            notesGenerator.instances.RemoveAt(currentNoteIndex);

                            // スコア計算
                            // ShowScoreStatus(JudgementRank.Miss);
                            // Manager.score.judgementStatus[JudgementRank.Miss]++;
                            // Manager.score.combo = 0;
                        };
                        note.PassedOverLine += () =>
                        {
                            scoreManager.Calculate(JudgementRank.Miss);
                            scoreManager.ShowScoreStatus(JudgementRank.Miss);
                            scoreManager.Reflect();
                        };
                        
                        // FIXME: エラーが出るかも
                        // ロングノーツの判定のイベントを登録する
                        // longNotesGenerator.ribbons
                        //     .Select(line => line.GetComponent<LongNote>())
                        //     .ToList().ForEach
                        //     (
                        //         note => note.OnPressedUpdate += isOn => JudgeLongNote(isOn, note.IsIntermediate, note.NoteIndex)
                        //     );
                    }
                    // オート時
                    else
                    {
                        // 判定線あたりでノーツをPerfect判定する
                        // note.ReachedLineEvent += () => DeleteNote(note.NoteIndex, isAuto: true);
                        // TODO: 仮の削除処理、これで isAuto: true は必要ないしUpdateもいらない
                        note.ReachedLine += () =>
                        {
                            // ノーツをリストから削除せずに形だけ消す
                            note.gameObject.SetActive(false);

                            Manager.audios.seManager.Play(SEManager.SE.GreatOrPerfect);

                            // スコア計算
                            // ShowScoreStatus(JudgementRank.Perfect);
                            // Manager.score.apparentScoreValue += JudgementRankValues.PERFECT;
                            // Manager.score.judgementStatus[JudgementRank.Perfect]++;
                            // Manager.score.combo++;
                            // Manager.score.CalculateScore();
                        };
                        note.ReachedLine += () =>
                        {
                            scoreManager.Calculate(JudgementRank.Perfect);
                            scoreManager.ShowScoreStatus(JudgementRank.Perfect);
                            scoreManager.Reflect(); 
                        };
                    }
                }
            );

            longNotesGenerator.instances.ForEach(longNotes => 
            {
                if (!Manager.info.IsAutoPlay)
                {
                    longNotes.ForEach(longNote => 
                    {
                        longNote.ReachedLine += () => 
                        {
                            // longNotesGenerator.instances[]
                        };
                    });
                }
            });
        }

        // void Update()
        // {
        //     if (Manager.info.IsAutoPlay)
        //     {
        //         try { AutoPlay(); }
        //         catch (ArgumentOutOfRangeException) { }
        //     }
        // }

        #endregion

        #region メソッド

        /// <summary>
        /// ターゲットにするノーツを取得した後に判定処理を行う。
        /// </summary>
        /// <param name="laneIndex"></param>
        /// <param name="tapTime"></param>
        private void JudgeNote(int laneIndex, float tapTime)
        {
            // そのレーンを流れるノーツの中で直近のを20個くらい調べる
            int searchCount = notesGenerator.instances.Count > 20 ? 20 : notesGenerator.instances.Count;

            // 一番小さいラグとその時のインデックスを記録
            float bestLag = float.MaxValue;
            int bestIndex = -1;

            // 同じレーンのノーツを後ろから探し
            // タップされた時間とノーツの到達時間のラグを計算して、最も小さいラグのノーツをターゲットにする
            for (int i = 1; i <= searchCount; i++)
            {
                if (notesGenerator.instances[^i].LaneIndex == laneIndex)
                {
                    float lag = CalculateLag(tapTime, notesGenerator.reachedTimes[^i]);
                    if (lag <= JudgementRankLagThresholds.BAD && lag < bestLag)
                    {
                        bestLag = lag;
                        bestIndex = i;
                    }
                }
            }

            // ターゲットノーツをセットする。見つからなかったときは null
            target = bestIndex != -1 ? notesGenerator.instances[^bestIndex] : null;

            Evaluate(bestLag);
        }

        /// <summary>
        /// ロングノーツが押されているかに応じて、その中間点のノーツを判定する
        /// </summary>
        /// <param name="isPressed">ロングノーツが押されているか</param>
        /// <param name="isIntermediate">ロングノーツが終点以外の中間点を持っているか</param>
        /// <param name="longNoteIndex">ロングノーツに割り振られた順番</param>
        private void JudgeLongNote(bool isPressed, bool isIntermediate, int longNoteIndex)
        {
            // カウンタ変数をメソッドスコープで宣言して、メソッド内で使いまわす
            int i;

            // 指定されたロングノーツの順番に照応するロングノーツラインを探す
            for (i = 1; i <= longNotesGenerator.instances.Count; i++)
            {
                // インデックスが同じコンポーネントがあったら、そのインデックスを i にコピー
                if (longNotesGenerator.ribbons[^i].Index == longNoteIndex)
                {
                    break;
                }
            }

            // 中間点を持つ場合 -> 中間点と終点のチェック
            if (isIntermediate)
            {
                // 中間ノーツがロングノーツのまとまりごとに収まっているリストにおいて、
                // i番目のロングノーツの中間ノーツのリストの、一番最後の要素が最も近い中間ノーツになる
                // その中間ノーツが判定線まで近づいたとき (押下判定が外れてしまうタイミングを考慮して、余分に距離を見積もる)
                if (longNotesGenerator.instances[^i][^1].transform.position.z <= noteOrigin.z + 0.5f)
                {
                    // その中間ノーツが終点の場合
                    if (longNotesGenerator.instances[^i][^1].Part == LongNotePart.End)
                    {
                        DeleteNote(longNotesGenerator.instances, ^i, isPressed);
                    }
                    // その中間ノーツが中間点の場合
                    else if (longNotesGenerator.instances[^i][^1].Part == LongNotePart.Intermediate)
                    {
                        DeleteNote(longNotesGenerator.instances, ^i, isPressed, ^1);
                    }
                }
            }
            // 中間点を持たない場合 -> 終点のみのチェック
            else
            {
                // 説明略（中間点を持つ場合の処理と同じ）
                if (longNotesGenerator.instances[^i][^1].transform.position.z <= noteOrigin.z + 0.5f)
                {
                    if (longNotesGenerator.instances[^i][^1].Part == LongNotePart.End)
                    {
                        DeleteNote(longNotesGenerator.instances, ^i, isPressed);
                    }
                }
            }
        }

        /// <summary>
        /// ノーツが押されたときのラグに合わせて、判定ランクを評価する。
        /// </summary>
        /// <param name="timeLag">実際にノーツが押された時間と押されるべき時間とのラグ。</param>
        private void Evaluate(float timeLag)
        {
            JudgementRank scoreRank = timeLag switch
            {
                // 0.05s 以内: Perfect
                <= JudgementRankLagThresholds.PERFECT => JudgementRank.Perfect,
                // 0.1s 以内: Great
                <= JudgementRankLagThresholds.GREAT => JudgementRank.Great,
                // 0.25s 以内: Good
                <= JudgementRankLagThresholds.GOOD => JudgementRank.Good,
                // 0.4s 以内: Bad
                <= JudgementRankLagThresholds.BAD => JudgementRank.Bad,
                // その他: Miss
                _ => JudgementRank.Miss
            };
            
            if (scoreRank == JudgementRank.Miss)
            {
                return;
            }

            Manager.audios.seManager.Play(scoreRank switch
            {
                JudgementRank.Perfect or JudgementRank.Great => SEManager.SE.GreatOrPerfect,
                _ => SEManager.SE.GoodOrBad,
            });

            // ターゲットノーツがリストに存在するか確認する
            if (notesGenerator.instances.Contains(target))
            {
                // // ラグを判定幅に照応させて判定する
                // if (timeLag <= JudgementThreshold[JudgementRank.Perfect])
                // {
                    
                //     ShowScoreStatus(JudgementRank.Perfect);
                    
                //     Manager.score.combo++;
                // }
                // else if (timeLag <= JudgementThreshold[JudgementRank.Great])
                // {
                //     Manager.audios.seManager.Play(SEManager.SE.GreatOrPerfect);
                //     ShowScoreStatus(JudgementRank.Great);
                //     Manager.score.apparentScoreValue += JudgementRankValues.GREAT;
                //     Manager.score.judgementStatus[JudgementRank.Great]++;
                //     Manager.score.combo++;
                // }
                // else if (timeLag <= JudgementThreshold[JudgementRank.Good])
                // {
                //     Manager.audios.seManager.Play(SEManager.SE.GoodOrBad);
                //     ShowScoreStatus(JudgementRank.Good);
                //     Manager.score.apparentScoreValue += JudgementRankValues.GOOD;
                //     Manager.score.judgementStatus[JudgementRank.Good]++;
                //     Manager.score.combo++;
                //     // 精度の問題もあり、Goodまではコンボ許容することにする
                // }
                // else if (timeLag <= JudgementThreshold[JudgementRank.Bad])
                // {
                //     Manager.audios.seManager.Play(SEManager.SE.GoodOrBad);
                //     ShowScoreStatus(JudgementRank.Bad);
                //     Manager.score.apparentScoreValue += JudgementRankValues.BAD;
                //     Manager.score.judgementStatus[JudgementRank.Bad]++;
                //     Manager.score.combo = 0;
                // }

                

                // ノーツを消す
                DeleteNote();
            }
            else
            {
                Debug.LogWarning("ターゲットノーツがリストに存在しません");
            }
        }

        /// <summary>
        /// 判定が終わったターゲットノーツ <see cref="target"/> を削除する。
        /// </summary>
        private void DeleteNote()
        {
            target.gameObject.SetActive(false);
            int index = notesGenerator.instances.IndexOf(target);
            notesGenerator.reachedTimes.RemoveAt(index);
            notesGenerator.laneIndexes.RemoveAt(index);
            notesGenerator.types.RemoveAt(index);
            notesGenerator.instances.RemoveAt(index);

            // FIXME: 仮設定
            noteJudged?.Invoke(target);
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
        // private void DeleteNote(int targetIndex, bool isAuto = false, bool isMissed = false)
        // {
        //     // if (isAuto)
        //     // {
        //     //     // 同じノーツを二度判定することがないように、ノーツのアクティブ状態を確認する
        //     //     if (notesGenerator.instances[targetIndex].gameObject.activeSelf)
        //     //     {
        //     //         // ノーツをリストから削除せずに形だけ消す
        //     //         notesGenerator.instances[targetIndex].gameObject.SetActive(false);

        //     //         Manager.audios.seManager.Play(SEManager.SE.GreatOrPerfect);

        //     //         // スコア計算
        //     //         ShowScoreStatus(JudgementRank.Perfect);
        //     //         Manager.score.apparentScoreValue += JudgementRankValues.PERFECT;
        //     //         Manager.score.judgementStatus[JudgementRank.Perfect]++;
        //     //         Manager.score.combo++;
        //     //         Manager.score.CalculateScore();
        //     //     }
        //     //     else
        //     //     {
        //     //         return;
        //     //     }
        //     // }
        //     // else 
        //     if (isMissed)
        //     {
        //         // 同タイミングで判定線を通過するノーツは、インデックスが被って上手くリストから削除できないことがあるので
        //         // リストのカウントを超えないようにtargetIndexを予め調整する
        //         if (targetIndex <= notesGenerator.instances.Count)
        //         {
        //             int i = 0;
        //             while (true)
        //             {
        //                 if (targetIndex - i < notesGenerator.instances.Count)
        //                 {
        //                     break;
        //                 }
        //                 else
        //                 {
        //                     i++;
        //                 }
        //             }
        //             targetIndex -= i;
        //         }

        //         // ノーツをリストから削除
        //         notesGenerator.instances[targetIndex].gameObject.SetActive(false);
        //         notesGenerator.reachedTimes.RemoveAt(targetIndex);
        //         notesGenerator.laneIndexes.RemoveAt(targetIndex);
        //         notesGenerator.types.RemoveAt(targetIndex);
        //         notesGenerator.instances.RemoveAt(targetIndex);

        //         // スコア計算
        //         ShowScoreStatus(JudgementRank.Miss);
        //         Manager.score.judgementStatus[JudgementRank.Miss]++;
        //         Manager.score.combo = 0;
        //     }
    
        //     // FIXME: 仮設定
        //     // noteJudged?.Invoke(notesGenerator.instances[targetIndex]);
        // }

        /// <summary>
        /// 判定線を超過したロングノーツの中間点・終点を削除する。
        /// </summary>
        /// <param name="intermediateNotesList">ロングノーツの中間点が格納されたリスト</param>
        /// <param name="targetLongNoteListIndex">ターゲットにするロングノーツのリストでのインデックス</param>
        /// <param name="isPressed">ロングノーツのラインが押されているか</param>
        /// <param name="targetIndex">消す中間点のインデックス</param>
        private void DeleteNote(List<List<LongNote>> intermediateNotesList, Index targetLongNoteListIndex, bool isPressed, Index targetIndex = default)
        {
            // 押下の有無を判別
            if (isPressed)
            {
                // 押されていたまま判定線を超過したら、Perfectで判定をとる
                Manager.audios.seManager.Play(SEManager.SE.GreatOrPerfect);
                // ShowScoreStatus(JudgementRank.Perfect);
                Manager.score.apparentScoreValue += JudgementRankValues.PERFECT;
                Manager.score.judgementStatus[JudgementRank.Perfect]++;
                Manager.score.combo++;
                Manager.score.CalculateScore();
            }
            else
            {
                // 押されてなかったらミス
                // ShowScoreStatus(JudgementRank.Miss);
                Manager.score.judgementStatus[JudgementRank.Miss]++;
                Manager.score.combo = 0;
            }

            // ターゲットとする中間点（終点以外）のインデックスの指定があったとき
            if (targetIndex.Value > 0)
            {
                // 中間点を隠してリストから消す
                intermediateNotesList[targetLongNoteListIndex][targetIndex].gameObject.SetActive(false);
                intermediateNotesList[targetLongNoteListIndex].RemoveAt(targetIndex);
            }
            // 終点を消すとき
            else
            {
                intermediateNotesList[targetLongNoteListIndex][0].gameObject.SetActive(false);
                intermediateNotesList[targetLongNoteListIndex].RemoveAt(0);
                intermediateNotesList.RemoveAt(targetLongNoteListIndex);
                // ロングノーツラインのリストからも消す
                longNotesGenerator.ribbons[targetLongNoteListIndex].gameObject.SetActive(false);
                longNotesGenerator.ribbons.RemoveAt(targetLongNoteListIndex);
            }

            // FIXME: 仮設定
            // noteJudged?.Invoke(longNotesGenerator.ribbons[targetLongNoteListIndex]);
        }

        /// <summary>
        /// 判定ステータスを画面上に表示する。
        /// </summary>
        /// <remarks>
        /// オブジェクトプール(<see cref = "JudgementEffectPool"/>)を利用する
        /// </remarks>
        // private void ShowScoreStatus(JudgementRank status)
        // {
        //     switch (status)
        //     {
        //         case JudgementRank.Perfect:
        //             score.Object = score.objectPool.perfect.Get();
        //             break;
        //         case JudgementRank.Great:
        //             score.Object = score.objectPool.great.Get();
        //             break;
        //         case JudgementRank.Good:
        //             score.Object = score.objectPool.good.Get();
        //             break;
        //         case JudgementRank.Bad:
        //             score.Object = score.objectPool.bad.Get();
        //             break;
        //         case JudgementRank.Miss:
        //             score.Object = score.objectPool.miss.Get();
        //             break;
        //     }
        //     score.Object.transform.SetParent(score.parent);
        //     score.Object.transform.position = new(0, 0, 0);
        //     score.Object.transform.rotation = Quaternion.Euler(0, 0, 0);
        // }

        /// <summary>
        /// 実際にノーツがタップされた時間と、本来ノーツをタップすべき時間との差を求める。
        /// </summary>
        /// <param name="tapTime">タップエリアをタップした時間</param>
        /// <param name="noteTime">本来ノーツをタップすべき時間</param>
        /// <returns>タイムラグ</returns>
        private float CalculateLag(float tapTime, float noteTime) => Mathf.Abs(Manager.startTime + noteTime - tapTime);

        /// <summary>
        /// オートプレイ時の自動判定。
        /// （仮）
        /// </summary>
        private void AutoPlay()
        {
            void Judgement(Index index)
            {
                Debug.LogWarning($"ノーツの{notesGenerator.instances[index].Index}が判定線を超過しました。");
                // ノーツをリストから削除
                notesGenerator.instances[index].gameObject.SetActive(false);
                notesGenerator.reachedTimes.RemoveAt(index);
                notesGenerator.laneIndexes.RemoveAt(index);
                notesGenerator.types.RemoveAt(index);
                notesGenerator.instances.RemoveAt(index);

                // スコア計算
                Manager.audios.seManager.Play(SEManager.SE.GreatOrPerfect);
                // ShowScoreStatus(JudgementRank.Perfect);
                Manager.score.apparentScoreValue += JudgementRankValues.PERFECT;
                Manager.score.judgementStatus[JudgementRank.Perfect]++;
                Manager.score.combo++;
                Manager.score.CalculateScore();

                // FIXME: 仮設定
                // noteJudged?.Invoke(notesGenerator.instances[index]);
            }

            if (Mathf.Abs(notesGenerator.instances[^1].transform.position.z - 7.3f) < 1.0f)
            {
                Judgement(^1);
            }
            if (Mathf.Abs(notesGenerator.instances[^2].transform.position.z - 7.3f) < 1.0f)
            {
                Judgement(^2);
            }
            if (Mathf.Abs(notesGenerator.instances[^3].transform.position.z - 7.3f) < 1.0f)
            {
                Judgement(^3);
            }
            if (Mathf.Abs(notesGenerator.instances[^4].transform.position.z - 7.3f) < 1.0f)
            {
                Judgement(^4);
            }
            if (Mathf.Abs(notesGenerator.instances[^5].transform.position.z - 7.3f) < 1.0f)
            {
                Judgement(^5);
            }
            if (Mathf.Abs(notesGenerator.instances[^6].transform.position.z - 7.3f) < 1.0f)
            {
                Judgement(^6);
            }
            if (Mathf.Abs(notesGenerator.instances[^7].transform.position.z - 7.3f) < 1.0f)
            {
                Judgement(^7);
            }
            if (Mathf.Abs(notesGenerator.instances[^8].transform.position.z - 7.3f) < 1.0f)
            {
                Judgement(^8);
            }
            if (Mathf.Abs(notesGenerator.instances[^9].transform.position.z - 7.3f) < 1.0f)
            {
                Judgement(^9);
            }
        }
        #endregion
    }
}

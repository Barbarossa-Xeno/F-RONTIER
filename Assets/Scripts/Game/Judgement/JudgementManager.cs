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
        [SerializeField] private NotesManager notesManager;

        /// <summary>
        /// <see cref = "LongNotesManager"/>
        /// </summary>
        [SerializeField] private LongNotesManager longNotesManager;

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
        [SerializeField] private JudgementResult judgementResult;

        /// <summary>
        /// 判定をする際にターゲットの候補として調べる最大ノーツ数。
        /// </summary>
        private const int MAX_SEARCH_NOTES_COUNT = 20;

        #endregion

        #region クラス

        /// <summary>
        /// 判定結果を格納するクラス。
        /// </summary>
        [Serializable]
        private class JudgementResult
        {
            public Note target;
            public float timeLag;

            public JudgementResult(Note target, float timeLag)
            {
                this.target = target;
                this.timeLag = timeLag;
            }
        }

        #endregion

        void Start()
        {
            if (!Manager.info.IsAutoPlay)
            {
                #region 通常プレイ時の初期化

                // タップしたときのイベントを登録する
                Array.ForEach(laneManager.Lanes, lane => lane.TappedEvent += (index, time) => 
                {
                    // ターゲットが見つかれば先に進む、なければ戻る
                    if (!JudgeNote(index, time))
                    {
                        return;
                    }

                    // ターゲットがロングノーツか通常ノーツかを見て、削除が成功したかをチェックする
                    if (judgementResult.target is LongNote target
                        ? longNotesManager.DeleteNote(target)
                        : notesManager.DeleteNote(judgementResult.target))
                    {
                        // スコア計算など
                        scoreManager.Evaluate(judgementResult.timeLag);
                    }
                    else
                    {
                        Debug.LogWarning($"ターゲットノーツがリストに存在しません: {judgementResult.target}");
                    }
                });

                // ロングノーツの中間点、終点ノーツはタップ判定の対象外
                // 帯が押されている状態で判定線に到達したときに Perfect 判定
                longNotesManager.Instances
                .Where(note => note.IsIntermediate)
                .ToList()
                .ForEach(note => note.ReachedLine += () =>
                {
                    if (note.IsPressed)
                    {
                        if (longNotesManager.DeleteNote(note))
                        {
                            // スコア計算
                            scoreManager.Evaluate(JudgementRankLagThresholds.PERFECT);
                        }
                        else
                        {
                            Debug.LogWarning($"ターゲットノーツがリストに存在しません: {note}");
                        }
                    }
                });
                
                // 判定線を超過したときのミス判定をすべてのノーツに登録する
                notesManager.Instances.ForEach(note => note.PassedOverLine += () =>
                {
                    if (notesManager.DeleteNote(note))
                    {
                        scoreManager.Evaluate(JudgementRankLagThresholds.MISS);
                    }
                    else
                    {
                        Debug.LogWarning($"ターゲットノーツがリストに存在しません: {note}");
                    }
                });

                longNotesManager.Instances.ForEach(note => note.PassedOverLine += () => 
                {
                    if (longNotesManager.DeleteNote(note))
                    {
                        scoreManager.Evaluate(JudgementRankLagThresholds.MISS);
                    }
                    else
                    {
                        Debug.LogWarning($"ターゲットノーツがリストに存在しません: {note}");
                    }
                });

                #endregion
            }
            else
            {
                #region オートプレイ時の初期化

                // 判定線に到達したときの Perfect 判定を登録する
                notesManager.Instances.ForEach(note => note.ReachedLine += () =>
                {
                    if (notesManager.DeleteNote(note))
                    {
                    // スコア計算
                        scoreManager.Evaluate(JudgementRankLagThresholds.PERFECT);
                    }
                    else
                    {
                        Debug.LogWarning($"ターゲットノーツがリストに存在しません: {note}");
                    }
                });

                longNotesManager.Instances.ForEach(note => note.ReachedLine += () =>
                {
                    if (longNotesManager.DeleteNote(note))
                    {
                        // スコア計算
                        scoreManager.Evaluate(JudgementRankLagThresholds.PERFECT);
                    }
                    else
                    {
                        Debug.LogWarning($"ターゲットノーツがリストに存在しません: {note}");
                    }
                });
            }

            #endregion
        }

        #region メソッド

        /// <summary>
        /// ターゲットにするノーツを取得した後に判定処理を行う。
        /// </summary>
        /// <param name="laneIndex"></param>
        /// <param name="tapTime"></param>
        /// <returns>ターゲットノーツが見つかったか</returns>
        private bool JudgeNote(int laneIndex, float tapTime)
        {
            // ゲームプレイ中でなければ判定しない
            if (Manager.gamePlayState != GameManager.GamePlayState.Playing)
            {
                return false;
            }

            // まずは通常ノーツから調べていく
            // そのレーンを流れるノーツの中で直近のを20個くらい調べる
            int searchCount = notesManager.Instances.Count > MAX_SEARCH_NOTES_COUNT ? MAX_SEARCH_NOTES_COUNT : notesManager.Instances.Count;

            // 一番小さいラグとその時のインデックスを記録
            float bestLag = float.MaxValue;
            Note target = null;

            // 同じレーンのノーツを後ろから探し
            // タップされた時間とノーツの到達時間のラグを計算して、最も小さいラグのノーツをターゲットにする
            for (int i = 1; i <= searchCount; i++)
            {
                if (notesManager.Instances[^i].LaneIndex == laneIndex)
                {
                    float lag = CalculateLag(tapTime, notesManager.Instances[^i].ReachedTime);
                    if (lag <= JudgementRankLagThresholds.BAD && lag < bestLag)
                    {
                        bestLag = lag;
                        target = notesManager.Instances[^i];
                    }
                }
            }

            // 次はロングノーツ
            searchCount = longNotesManager.Instances.Count > MAX_SEARCH_NOTES_COUNT ? MAX_SEARCH_NOTES_COUNT : longNotesManager.Instances.Count;

            for (int i = 1; i <= searchCount; i++)
            {
                if (longNotesManager.Instances[^i].LaneIndex == laneIndex
                    // 中間点、終点ノーツは判定の対象外
                    && !longNotesManager.Instances[^i].IsIntermediate)
                {
                    float lag = CalculateLag(tapTime, longNotesManager.Instances[^i].ReachedTime);
                    if (lag <= JudgementRankLagThresholds.BAD && lag < bestLag)
                    {
                        bestLag = lag;
                        target = longNotesManager.Instances[^i];
                    }
                }
            }

            // ターゲットノーツをセットする。見つからなかったときは null
            judgementResult = target != null ? new(target, bestLag) : null;

            return judgementResult != null;
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

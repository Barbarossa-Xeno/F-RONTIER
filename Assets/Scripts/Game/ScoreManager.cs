using UnityEngine;
using UnityEngine.UI;
using FRONTIER.Utility;
using TMPro;
using static FRONTIER.Utility.Reference;

namespace FRONTIER.Game
{
    /// <summary>
    /// スコアを計算し、ゲーム中のUIにスコアを表示する。
    /// </summary>
    [RequireComponent(typeof(JudgementEffectPool))]
    public class ScoreManager : GameUtilityBase
    {
        #region フィールド

        /// <summary>
        /// スコアの進捗を示すゲージ。
        /// </summary>
        [SerializeField] private Slider scoreGauge;

        /// <summary>
        /// スコア数値を表示する。
        /// </summary>
        [SerializeField] private TextMeshProUGUI score;

        /// <summary>
        /// スコアランクのTMP。
        /// </summary>
        [SerializeField] private ScoreRankTexts scoreRankTexts;

        /// <summary>
        /// コンボ数を表示する。
        /// </summary>
        [SerializeField] private TextMeshProUGUI combo;

        [SerializeField] private JudgementRank currentRank;

        private JudgementEffectPool judgementEffect;

        #endregion

        #region 構造体

        /// <summary>
        /// ゲージの上に現在到達しているクリアランクを表示する。
        /// </summary>
        [System.Serializable]
        private struct ScoreRankTexts
        {
            public TextMeshProUGUI s;
            public TextMeshProUGUI a;
            public TextMeshProUGUI b;
            public TextMeshProUGUI c;
        }

        #endregion

        #region MonoBehaviourメソッド

        void Start()
        {
            judgementEffect = GetComponent<JudgementEffectPool>();
            scoreGauge.value = 0;
            score.SetText("0000000");
            combo.SetText("0");
        }

        #endregion

        #region メソッド

        /// <summary>
        /// ノーツが押されたときのラグにより、判定ランクを評価する。
        /// また、スコア計算、エフェクト表示やUIへの反映も行う。
        /// </summary>
        /// <param name="timeLag">実際にノーツが押された時間と押されるべき時間とのラグ。</param>
        public void Evaluate(float timeLag)
        {
            currentRank = timeLag switch
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

            Calculate();
            ShowEffect();
            Reflect();
        }

        /// <summary>
        /// 直近の判定評価でスコアを計算して、値を更新する。
        /// </summary>
        private void Calculate()
        {
            switch (currentRank)
            {
                case JudgementRank.Perfect:
                {
                    Manager.score.apparentScoreValue += JudgementRankValues.PERFECT;
                    Manager.score.judgementStatus[JudgementRank.Perfect]++;
                    Manager.score.combo++;
                    break;
                }
                case JudgementRank.Great:
                {
                    Manager.score.apparentScoreValue += JudgementRankValues.GREAT;
                    Manager.score.judgementStatus[JudgementRank.Great]++;
                    Manager.score.combo++;
                    break;
                }
                case JudgementRank.Good:
                {
                    Manager.score.apparentScoreValue += JudgementRankValues.GOOD;
                    Manager.score.judgementStatus[JudgementRank.Good]++;
                    Manager.score.combo++;
                    break;
                }
                case JudgementRank.Bad:
                {
                    Manager.score.apparentScoreValue += JudgementRankValues.BAD;
                    Manager.score.judgementStatus[JudgementRank.Bad]++;
                    Manager.score.combo = 0;
                    break;
                }
                case JudgementRank.Miss:
                {
                    Manager.score.judgementStatus[JudgementRank.Miss]++;
                    Manager.score.combo = 0;
                    break;
                }
                default:
                {
                    break;
                }
            }

            // スコア計算
            Manager.score.CalculateScore();
        }

        /// <summary>
        /// 直近の判定評価を画面上に表示する。
        /// </summary>
        /// <remarks>
        /// オブジェクトプール(<see cref = "JudgementEffectPool"/>)を利用する
        /// </remarks>
        private void ShowEffect()
        {
            switch (currentRank)
            {
                case JudgementRank.Perfect:
                {
                    judgementEffect.Perfect.Get();
                    break;
                }
                case JudgementRank.Great:
                {
                    judgementEffect.Great.Get();
                    break;
                }
                case JudgementRank.Good:
                {
                    judgementEffect.Good.Get();
                    break;
                }
                case JudgementRank.Bad:
                {
                    judgementEffect.Bad.Get();
                    break;
                }
                case JudgementRank.Miss:
                {
                    judgementEffect.Miss.Get();
                    break;
                }
            }
        }

        /// <summary>
        /// スコアをアップデートする。
        /// </summary>
        // UnityEventから発火する
        private void Reflect()
        {
            // Miss の場合はSE鳴らしたりスコアを加算したりしないので、ここで処理を終える
            if (currentRank == JudgementRank.Miss)
            {
                return;
            }

            Manager.audios.seManager.Play(currentRank switch
            {
                JudgementRank.Perfect or JudgementRank.Great => Audio.SEManager.SE.GreatOrPerfect,
                _ => Audio.SEManager.SE.GoodOrBad,
            });

            scoreGauge.value = (float)Manager.score.ScoreValue / (float)Reference.THEORETICAL_SCORE_VALUE;
            score.SetText($"{Manager.score.ScoreValue}");
            combo.SetText($"{Manager.score.combo}");

            if (Manager.score.ScoreValue >= Reference.ClearRankThresholds.C)
            {
                scoreRankTexts.c.color = Reference.DifficultyValues.Colors.Lite;
                scoreRankTexts.c.outlineColor = new(255, 255, 255, 255);
                Manager.score.clearRank = Reference.ClearRank.C;
                if (Manager.score.ScoreValue >= Reference.ClearRankThresholds.C_PLUS)
                {
                    Manager.score.clearRank = Reference.ClearRank.C_Plus;
                }
            }
            if (Manager.score.ScoreValue >= Reference.ClearRankThresholds.B)
            {
                scoreRankTexts.b.color = Reference.DifficultyValues.Colors.Hard;
                scoreRankTexts.b.outlineColor = new(255, 255, 255, 255);
                Manager.score.clearRank = Reference.ClearRank.B;
                if (Manager.score.ScoreValue >= Reference.ClearRankThresholds.C_PLUS)
                {
                    Manager.score.clearRank = Reference.ClearRank.B_Plus;
                }
            }
            if (Manager.score.ScoreValue >= Reference.ClearRankThresholds.A)
            {
                scoreRankTexts.a.color = Reference.DifficultyValues.Colors.Ecstasy;
                scoreRankTexts.a.outlineColor = new(255, 255, 255, 255);
                Manager.score.clearRank = Reference.ClearRank.A;
                if (Manager.score.ScoreValue >= Reference.ClearRankThresholds.C_PLUS)
                {
                    Manager.score.clearRank = Reference.ClearRank.A_Plus;
                }
            }
            if (Manager.score.ScoreValue >= Reference.ClearRankThresholds.S)
            {
                scoreRankTexts.s.color = Reference.DifficultyValues.Colors.Restricted;
                scoreRankTexts.s.outlineColor = new(255, 255, 255, 255);
                Manager.score.clearRank = Reference.ClearRank.S;
                if (Manager.score.ScoreValue >= Reference.ClearRankThresholds.C_PLUS)
                {
                    Manager.score.clearRank = Reference.ClearRank.S_Plus;
                }
            }
        }

        #endregion
    }
}

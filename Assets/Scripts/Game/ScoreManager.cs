using UnityEngine;
using UnityEngine.UI;
using FRONTIER.Utility;
using TMPro;

namespace FRONTIER.Game
{
    /// <summary>
    /// ゲーム中のUIにスコアを表示する。
    /// </summary>
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
            scoreGauge.value = 0;
            score.SetText("0000000");
            combo.SetText("0");
        }

        #endregion

        #region メソッド

        /// <summary>
        /// スコアをアップデートする。
        /// </summary>
        // UnityEventから発火する
        public void UpdateScore()
        {
            scoreGauge.value = (float)Manager.score.ScoreValue / (float)GameManager.ScoreData.THEORETICAL_SCORE_VALUE;
            score.SetText($"{Manager.score.ScoreValue}");
            combo.SetText($"{Manager.score.combo}");

            if (Manager.score.ScoreValue >= Reference.ClearRankThresholds.C)
            {
                scoreRankTexts.c.color = Reference.DifficultyUtilities.Colors.Lite;
                scoreRankTexts.c.outlineColor = new(255, 255, 255, 255);
                Manager.score.clearRank = Reference.ClearRank.C;
                if (Manager.score.ScoreValue >= Reference.ClearRankThresholds.C_PLUS)
                {
                    Manager.score.clearRank = Reference.ClearRank.C_Plus;
                }
            }
            if (Manager.score.ScoreValue >= Reference.ClearRankThresholds.B)
            {
                scoreRankTexts.b.color = Reference.DifficultyUtilities.Colors.Hard;
                scoreRankTexts.b.outlineColor = new(255, 255, 255, 255);
                Manager.score.clearRank = Reference.ClearRank.B;
                if (Manager.score.ScoreValue >= Reference.ClearRankThresholds.C_PLUS)
                {
                    Manager.score.clearRank = Reference.ClearRank.B_Plus;
                }
            }
            if (Manager.score.ScoreValue >= Reference.ClearRankThresholds.A)
            {
                scoreRankTexts.a.color = Reference.DifficultyUtilities.Colors.Ecstasy;
                scoreRankTexts.a.outlineColor = new(255, 255, 255, 255);
                Manager.score.clearRank = Reference.ClearRank.A;
                if (Manager.score.ScoreValue >= Reference.ClearRankThresholds.C_PLUS)
                {
                    Manager.score.clearRank = Reference.ClearRank.A_Plus;
                }
            }
            if (Manager.score.ScoreValue >= Reference.ClearRankThresholds.S)
            {
                scoreRankTexts.s.color = Reference.DifficultyUtilities.Colors.Restricted;
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

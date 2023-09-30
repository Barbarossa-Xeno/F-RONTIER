using UnityEngine;
using UnityEngine.UI;
using FRONTIER.Utility;
using TMPro;

namespace FRONTIER.Game
{
    /// <summary>
    /// ゲーム中のUIにスコアを表示する。
    /// </summary>
    public class ScoreManager : MonoBehaviour
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
            scoreGauge.value = (float)GameManager.instance.scoreData.Score / (float)GameManager.ScoreData.THEORETICAL_SCORE;
            score.SetText($"{GameManager.instance.scoreData.Score}");
            combo.SetText($"{GameManager.instance.scoreData.combo}");

            if (GameManager.instance.scoreData.Score >= Reference.ClearRankBorder.C)
            {
                scoreRankTexts.c.color = new(152f / 255f, 94f / 255f, 39f / 255f, 255f / 255f);
                scoreRankTexts.c.outlineColor = new(255, 255, 255, 255);
            }
            if (GameManager.instance.scoreData.Score >= Reference.ClearRankBorder.B)
            {
                scoreRankTexts.b.color = new Color(135f / 255f, 135f / 255f, 135f / 255f, 255f / 255f);
                scoreRankTexts.b.outlineColor = new(255, 255, 255, 255);
            }
            if (GameManager.instance.scoreData.Score >= Reference.ClearRankBorder.A)
            {
                scoreRankTexts.a.color = new Color(173f / 255f, 146f / 255f, 34f / 255f, 255f / 255f);
                scoreRankTexts.a.outlineColor = new(255, 255, 255, 255);
            }
            if (GameManager.instance.scoreData.Score >= Reference.ClearRankBorder.S)
            {
                scoreRankTexts.s.color = new Color(150f / 255f, 206f / 255f, 199f / 255f, 255f / 255f);
                scoreRankTexts.s.outlineColor = new(255, 255, 255, 255);
            }
        }

        #endregion
    }
}
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FRONTIER.Save;
using FRONTIER.Utility;

namespace FRONTIER.Result
{
    public class ResultWindows : GameUtility
    {
        #region フィールド

        /// <summary>
        /// リザルト画面のアニメーション。
        /// </summary>
        [SerializeField] private ResultAnimationManager resultAnimationManager;

        /// <summary>
        /// スコア表示部分の要素。
        /// </summary>
        [SerializeField] private Score score;

        /// <summary>
        /// 曲情報表示部分の要素。
        /// </summary>
        [SerializeField] private SongInfo songInfo;

        /// <summary>
        /// コンボ等表示部分の要素。
        /// </summary>
        [SerializeField] private Combo combo;

        /// <summary>
        /// フッター
        /// </summary>
        [SerializeField] private Footer footer;

        #endregion

        #region クラス

        /// <summary>
        /// スコアを表示する。
        /// </summary>
        [Serializable]
        private class Score : ResultWindowElements
        {
            #region フィールド・プロパティ

            /// <summary>
            /// クリアランクの表示。
            /// </summary>
            [SerializeField] private Image rank;

            /// <summary>
            /// スコアのゲージ。
            /// </summary>
            [SerializeField] private Slider gauge;

            /// <summary>
            /// スコア表示。
            /// </summary>
            [SerializeField] private TextMeshProUGUI value;

            /// <summary>
            /// ハイスコアとの差の表示。
            /// </summary>
            [SerializeField] private TextMeshProUGUI differenceFromHigh;

            /// <summary>
            /// 記録されたハイスコアの表示。
            /// </summary>
            [SerializeField] private TextMeshProUGUI high;

            /// <summary>
            /// ランクの画像。
            /// </summary>
            [SerializeField] private RankSprites rankSprites;

            /// <summary>
            /// 獲得スコアとハイスコアとの差。
            /// </summary>
            private int difference;

            /// <summary>
            /// 新記録かどうか。
            /// </summary>
            public bool IsGotNewRecord { get; private set; } = false;

            #endregion

            [Serializable]
            private class RankSprites
            {
                [SerializeField] private Sprite S_Plus;
                [SerializeField] private Sprite S;
                [SerializeField] private Sprite A_Plus;
                [SerializeField] private Sprite A;
                [SerializeField] private Sprite B_Plus;
                [SerializeField] private Sprite B;
                [SerializeField] private Sprite C_Plus;
                [SerializeField] private Sprite C;

                public Sprite RankToSprite(Reference.ClearRank rank)
                {
                    return rank switch
                    {
                        Reference.ClearRank.C => C,
                        Reference.ClearRank.C_Plus => C_Plus,
                        Reference.ClearRank.B => B,
                        Reference.ClearRank.B_Plus => B_Plus,
                        Reference.ClearRank.A => A,
                        Reference.ClearRank.A_Plus => A_Plus,
                        Reference.ClearRank.S => S,
                        Reference.ClearRank.S_Plus => S_Plus,
                        _ => null
                    };
                }
            }

            #region メソッド

            public override void Initialize()
            {
                int highScore = SongSaveData.Instance.Explore(Manager.info.ID).DifficultyTo(Manager.info.Difficulty).highScore;
                difference = Manager.score.ScoreValue - highScore;
                IsGotNewRecord = difference > 0;

                value.text = "0";
                high.text = $"{highScore}";
                differenceFromHigh.text = "0";

                rank.sprite = rankSprites.RankToSprite(Manager.score.clearRank);
            }

            /// <summary>
            /// 獲得スコアをカウントアップする。
            /// </summary>
            /// <remarks>
            /// https://media.colorfulpalette.co.jp/n/n18a11e05e883
            /// </remarks>
            private IEnumerator CountUpScores()
            {
                // 値
                float val = 0;
                // 経過時間
                float elapsedTime = 0;
                // 経過時間とアニメーション時間の比で正規化した時間
                float normalizedTime = 0;
                // ゲージの終端の値
                float gaugeFinalValue = (float)Manager.score.ScoreValue / (float)GameManager.ScoreData.THEORETICAL_SCORE_VALUE;

                // アニメーション時間に到達するまで繰り返す
                while (elapsedTime < ANIMATION_DURATION)
                {
                    // 1フレーム待つ
                    yield return null;

                    // 現在の経過時間を計算する
                    elapsedTime += Time.deltaTime;
                    normalizedTime = elapsedTime / ANIMATION_DURATION;

                    // 線形補間で現在の値を計算する
                    val = Mathf.Lerp(0, gaugeFinalValue, normalizedTime);

                    // ゲージとスコア値に適用する
                    gauge.value = val;
                    value.text = $"{(int)(val * GameManager.ScoreData.THEORETICAL_SCORE_VALUE)}";
                }
            }

            /// <summary>
            /// ハイスコアとの差をカウントアップする。
            /// </summary>
            /// <remarks>
            /// https://media.colorfulpalette.co.jp/n/n18a11e05e883
            /// </remarks>
            private IEnumerator CountUpDifference()
            {
                // 値
                float val = 0;
                // 経過時間
                float elapsedTime = 0;
                // 経過時間とアニメーション時間の比で正規化した時間
                float normalizedTime = 0;
                // 最終的な値
                float finalValue = difference;

                // アニメーション時間に到達するまで繰り返す
                while (elapsedTime < ANIMATION_DURATION)
                {
                    // 1フレーム待つ
                    yield return null;

                    // 現在の経過時間を計算する
                    elapsedTime += Time.deltaTime;
                    normalizedTime = elapsedTime / ANIMATION_DURATION;

                    // 線形補間で現在の値を計算する
                    val = Mathf.Lerp(0, finalValue, normalizedTime);

                    // 差分値に適用する
                    differenceFromHigh.text = $"{(int)(val):+#;-#;}";
                }
            }

            /// <summary>
            /// スコアとスコアゲージをアニメーションする。
            /// </summary>
            public void AnimateScores() => PlayCountUp(CountUpScores);

            /// <summary>
            /// ハイスコアとの差の数値をアニメーションする。
            /// </summary>
            public void AnimateDifferenceScore() => PlayCountUp(CountUpDifference);

            #endregion
        }

        /// <summary>
        /// プレイした楽曲の情報を表示する。
        /// </summary>
        [Serializable]
        private class SongInfo : ResultWindowElements
        {
            #region フィールド

            /// <summary>
            /// 楽曲のカバーアート。
            /// </summary>
            [SerializeField] private Image cover;

            /// <summary>
            /// 楽曲名。
            /// </summary>
            [SerializeField] private Utility.Asset.OverflowTextScroll songName;

            /// <summary>
            /// 難易度。
            /// </summary>
            [SerializeField] private TextMeshProUGUI difficulty;

            /// <summary>
            /// <see cref="difficulty"/>の背景。 
            /// </summary>
            public Image difficultyBackground;

            #endregion

            public override void Initialize()
            {
                cover.sprite = Manager.info.Cover;
                songName.Text = Manager.info.Name;
                (difficulty.text, difficultyBackground.color) = Manager.info.DifficultyTo(Manager.info.Difficulty);
            }
        }

        /// <summary>
        /// コンボ数や判定ステータスなどを表示する。
        /// </summary>
        [Serializable]
        private class Combo : ResultWindowElements
        {
            #region フィールド・プロパティ

            /// <summary>
            /// コンボ数の表示。
            /// </summary>
            [SerializeField] private TextMeshProUGUI count;

            /// <summary>
            /// 最大コンボ数の表示。
            /// </summary>
            [SerializeField] private TextMeshProUGUI max;

            /// <summary>
            /// PERFECT判定になった回数。
            /// </summary>
            [SerializeField] private TextMeshProUGUI perfect;

            /// <summary>
            /// GREAT判定になった回数。
            /// </summary>
            [SerializeField] private TextMeshProUGUI great;

            /// <summary>
            /// GOOD判定になった回数。
            /// </summary>
            [SerializeField] private TextMeshProUGUI good;

            /// <summary>
            /// BAD判定になった回数。
            /// </summary>
            [SerializeField] private TextMeshProUGUI bad;

            /// <summary>
            /// MISS判定になった回数。
            /// </summary>
            [SerializeField] private TextMeshProUGUI miss;

            /// <summary>
            /// 実績（フルコンボまたはオールパーフェクト）の表示。
            /// </summary>
            [SerializeField] private Image achivement;

            /// <summary>
            /// フルコンボ
            /// </summary>
            [SerializeField] private Sprite fullCombo;

            /// <summary>
            /// オールパーフェクト
            /// </summary>
            [SerializeField] private Sprite allPerfect;

            /// <summary>
            /// 最大コンボ数を更新したか
            /// </summary>
            public bool IsGotNewRecordOfCombo { get; private set; } = false;

            /// <summary>
            /// フルコンボしたか
            /// </summary>
            public bool IsGotFullCombo { get; private set; } = false;

            /// <summary>
            /// オールパーフェクトしたか
            /// </summary>
            public bool IsGotAllPerfect { get; private set; } = false;

            #endregion

            #region メソッド

            public override void Initialize()
            {
                count.text = "0";
                max.text = $"/{Manager.score.maxComboCount}";
                perfect.text = $"{Manager.score.judgementStatus[Reference.JudgementStatus.Perfect]}";
                great.text = $"{Manager.score.judgementStatus[Reference.JudgementStatus.Great]}";
                good.text = $"{Manager.score.judgementStatus[Reference.JudgementStatus.Good]}";
                bad.text = $"{Manager.score.judgementStatus[Reference.JudgementStatus.Bad]}";
                miss.text = $"{Manager.score.judgementStatus[Reference.JudgementStatus.Miss]}";

                int highCombo = SongSaveData.Instance.Explore(Manager.info.ID).DifficultyTo(Manager.info.Difficulty).highCombo;
                IsGotNewRecordOfCombo = Manager.score.maxCombo > highCombo;

                // フルコンボしているか確認する
                if (Manager.score.combo == Manager.score.maxComboCount)
                {
                    achivement.sprite = fullCombo;
                    IsGotFullCombo = true;
                    if (Manager.score.judgementStatus[Reference.JudgementStatus.Perfect] == Manager.score.maxComboCount)
                    {
                        achivement.sprite = allPerfect;
                        IsGotAllPerfect = true;
                    }
                }
            }

            /// <summary>
            /// 達成コンボをカウントアップする。
            /// </summary>
            /// <remarks>
            /// https://media.colorfulpalette.co.jp/n/n18a11e05e883
            /// </remarks>
            private IEnumerator CountUpCombo()
            {
                // 値
                float val = 0;
                // 経過時間
                float elapsedTime = 0;
                // 経過時間とアニメーション時間の比で正規化した時間
                float normalizedTime = 0;
                // 最終的な値
                float finalValue = Manager.score.combo;

                // アニメーション時間に到達するまで繰り返す
                while (elapsedTime < ANIMATION_DURATION)
                {
                    // 1フレーム待つ
                    yield return null;

                    // 経過時間を計算する
                    elapsedTime += Time.deltaTime;
                    normalizedTime = elapsedTime / ANIMATION_DURATION;

                    // 線形補間で現在の値を計算する
                    val = Mathf.Lerp(0, finalValue, normalizedTime);

                    // コンボ数に適用
                    count.text = $"{(int)(val)}";
                }
            }

            /// <summary>
            /// コンボ数をアニメーションする。
            /// </summary>
            public void AnimateCombo() => PlayCountUp(CountUpCombo);

            #endregion
        }

        /// <summary>
        /// フッターの要素。
        /// </summary>
        [Serializable]
        private class Footer : ResultWindowElements
        {
            [SerializeField] private TextMeshProUGUI text;

            #region メソッド

            public override void Initialize() => text.color = new(1f, 1f, 1f, 0);

            /// <summary>
            /// テキストを点滅させる。
            /// </summary>
            /// <returns></returns>
            private IEnumerator BlinkText()
            {
                float alphaAngle = 0;
                yield return new WaitForSeconds(0.5f);

                while (true)
                {
                    yield return new WaitForSeconds(0.05f);

                    // sin関数で透明度を0~1の範囲で変化させる
                    alphaAngle = alphaAngle < Mathf.PI ? alphaAngle + 0.05f : 0f;
                    text.color = new(1f, 1f, 1f, Mathf.Sin(alphaAngle));
                }
            }
        
            public void StartToBlink() => PlayCountUp(BlinkText);

            #endregion
        }
        
        #endregion

        #region メソッド

        public override void Construct()
        {
            // 各要素の初期化
            score.Initialize();
            combo.Initialize();
            songInfo.Initialize();
            footer.Initialize();

            // アニメーションの登録
            resultAnimationManager.OnPlayScore += score.AnimateScores;
            resultAnimationManager.OnPlayCombo += combo.AnimateCombo;
            resultAnimationManager.OnPlayDifference += score.AnimateDifferenceScore;

            // アニメーションの遷移設定
            ResultAnimationManager.Controller.Activate();
            ResultAnimationManager.Controller.IsGotNewRecord(score.IsGotNewRecord);
            ResultAnimationManager.Controller.IsGotFullCombo(combo.IsGotFullCombo);
            ResultAnimationManager.Controller.IsAutoPlay(Manager.info.IsAutoPlay);
            ResultAnimationManager.Controller.OnAnimatorFinished(this, footer.StartToBlink, UpdateSaveData, SongSaveData.Instance.Save);
        }

        /// <summary>
        /// 新記録が更新された場合、楽曲のセーブデータを上書きする。
        /// </summary>
        private void UpdateSaveData()
        {
            // プレイデータを参照してスコアの更新があった場合にデータを更新する
            SongSaveData.Instance
                .Explore(Manager.info.ID)
                .DifficultyTo(Manager.info.Difficulty)
                .Overwrite
                (
                    score: score.IsGotNewRecord ? Manager.score.ScoreValue : -1,
                    combo : combo.IsGotNewRecordOfCombo ? Manager.score.maxCombo : -1,
                    rank: score.IsGotNewRecord ? Manager.score.clearRank.ToString() : null,
                    isGotfullCombo: combo.IsGotFullCombo,
                    isGotAllPerfect: combo.IsGotAllPerfect
                );
        }

        #endregion
    }
}

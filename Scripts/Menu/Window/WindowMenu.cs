using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FRONTIER.Save;
using FRONTIER.Utility;
using FRONTIER.Utility.Asset;

namespace FRONTIER.Menu.Window
{
    /// <summary>
    /// 選曲画面のウィンドウを管理する。
    /// </summary>
    public class WindowMenu : MonoBehaviour, IMenu
    {
        #region フィールド

        /// <summary>
        /// ウィンドウの背景。
        /// </summary>
        [SerializeField] private WindowMesh windowMesh;

        /// <summary>
        /// ウィンドウの境界線。
        /// </summary>
        [SerializeField] private WindowBorderCurve windowBorderCurve;

        /// <summary>
        /// 設定ウィンドウ。
        /// </summary>
        [SerializeField] Setting.SettingWindowManager settingWindowManager;

        /// <summary>
        /// レベル・難易度の表示部分。
        /// </summary>
        [SerializeField] private LevelAndDifficulty levelAndDifficulty;

        /// <summary>
        /// 曲情報の表示部分。
        /// </summary>
        [SerializeField] private Song song;

        /// <summary>
        /// カバー画像とアチーブメントの表示部分。
        /// </summary>
        [SerializeField] private CoverAndAchivement coverAndAchivement;

        /// <summary>
        /// 設定ボタンやスタートボタン。
        /// </summary>
        [SerializeField] private Buttons buttons;

        #endregion

        #region クラス

        [Serializable]
        private class LevelAndDifficulty
        {
            public TextMeshProUGUI level = default;
            public TextMeshProUGUI Difficulty = default;
        }

        [Serializable]
        private class Song
        {
            public OverflowTextScroll name;
            public OverflowTextScroll artist;
        }

        [Serializable]
        private class CoverAndAchivement
        {
            public Image cover;
            public Image clearRank;
            public TextMeshProUGUI highScore;
            public TextMeshProUGUI maxCombo;
            public CanvasGroup fullComboAchivement;
            public CanvasGroup allPerfectAchivement;
            public Sprites sprites;

            [Serializable]
            public class Sprites
            {
                [SerializeField] private Sprite S_Plus;
                [SerializeField] private Sprite S;
                [SerializeField] private Sprite A_Plus;
                [SerializeField] private Sprite A;
                [SerializeField] private Sprite B_Plus;
                [SerializeField] private Sprite B;
                [SerializeField] private Sprite C_Plus;
                [SerializeField] private Sprite C;
                [SerializeField] private Sprite NoData;

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
                        Reference.ClearRank.NoData => NoData,
                        _ => null
                    };
                }
            }
        }

        [Serializable]
        private class Buttons
        {
            /// <summary>
            /// 設定ボタン。
            /// </summary>
            public Button setting;

            /// <summary>
            /// スタートボタン。
            /// </summary>
            public Button start;

            /// <summary>
            /// MVを再生するかどうか選択するボタン。
            /// </summary>
            public ToggleSwitch mv;

            /// <summary>
            /// オートプレイを選択するボタン。
            /// </summary>
            public ToggleSwitch auto;
        }

        #endregion

        #region MonoBehaviourメソッド

        void Start()
        {
            // 最初に１度実行して背景に色を適用させる
            OnDifficultyChanged(MenuInfo.menuInfo.Difficulty);

            buttons.setting.onClick.AddListener(settingWindowManager.Open);
            buttons.start.onClick.AddListener(GameManager.instance.scene.game.Invoke);
            buttons.mv.OnToggleChanged += isOn => MenuInfo.menuInfo.IsMV = isOn;
            buttons.auto.OnToggleChanged += isOn => MenuInfo.menuInfo.IsAutoPlay = isOn;
        }

        #endregion

        #region 実装メソッド

        public void OnSongSelected(int id)
        {
            levelAndDifficulty.level.text = MenuInfo.menuInfo.Level;

            song.name.Text = MenuInfo.menuInfo.Name;
            song.artist.Text = MenuInfo.menuInfo.Artist;

            // アチーブメントウィンドウの更新
            coverAndAchivement.cover.sprite = MenuInfo.menuInfo.Cover;
            var data = SongSaveData.Instance.Explore(MenuInfo.menuInfo.ID).DifficultyTo(MenuInfo.menuInfo.Difficulty);
            coverAndAchivement.clearRank.sprite = coverAndAchivement.sprites.RankToSprite
                (data.highRank != null ? Enum.Parse<Reference.ClearRank>(data.highRank) : Reference.ClearRank.NoData);
            coverAndAchivement.highScore.text = $"{data.highScore}";
            coverAndAchivement.maxCombo.text = $"{data.highCombo}";
            coverAndAchivement.fullComboAchivement.alpha = data.fullCombo ? 1 : 0;
            coverAndAchivement.allPerfectAchivement.alpha = data.allPerfect ? 1 : 0;
        }

        public void OnDifficultyChanged(int difficulty) => OnDifficultyChanged((Reference.DifficultyRank)difficulty);

        public void OnDifficultyChanged(Reference.DifficultyRank difficulty)
        {
            // ウィンドウの背景の更新
            windowMesh.SetColorTrigger(difficulty);
            windowBorderCurve.SetColorTrigger(difficulty);
            // メニューウィンドウの更新
            levelAndDifficulty.level.text = MenuInfo.menuInfo.Level;
            levelAndDifficulty.Difficulty.text = MenuInfo.menuInfo.DifficultyTo(difficulty).Item1;

            // アチーブメントウィンドウの更新
            var data = SongSaveData.Instance.Explore(MenuInfo.menuInfo.ID).DifficultyTo(difficulty);
            coverAndAchivement.clearRank.sprite = coverAndAchivement.sprites.RankToSprite
                (data.highRank != null ? Enum.Parse<Reference.ClearRank>(data.highRank) : Reference.ClearRank.NoData);
            coverAndAchivement.highScore.text = $"{data.highScore}";
            coverAndAchivement.maxCombo.text = $"{data.highCombo}";
            coverAndAchivement.fullComboAchivement.alpha = data.fullCombo ? 1 : 0;
            coverAndAchivement.allPerfectAchivement.alpha = data.allPerfect ? 1 : 0;
        }

        #endregion

        #region 実装しないメソッド

        public void OnSortOptionChanged(int option) { }
        public void OnSortOrderChanged(int order) { }
        public void OnSortOptionChanged(IMenu.Sort.Option option) { }
        public void OnSortOrderChanged(IMenu.Sort.Order order) { }

        #endregion
    }
}

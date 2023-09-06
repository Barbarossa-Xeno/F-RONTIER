using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Game.Utility;

namespace Game.Menu.Window
{
    /// <summary>
    /// 選曲画面のウィンドウを管理する。
    /// </summary>
    public class WindowMenu : MonoBehaviour, IMenu
    {
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


        [Serializable]
        private class LevelAndDifficulty
        {
            [SerializeField] public TextMeshProUGUI level = default;
            [SerializeField] public TextMeshProUGUI Difficulty = default;
        }

        [Serializable]
        private class Song
        {
            [SerializeField] public OverflowTextScroll name;
            [SerializeField] public OverflowTextScroll artist;
        }

        [Serializable]
        private class CoverAndAchivement
        {
            [SerializeField] public Image cover;
            [SerializeField] public Image clearRank;
            [SerializeField] public TextMeshProUGUI hiScore;
            [SerializeField] public TextMeshProUGUI maxCombo;
            [SerializeField] public Image fullComboAchivement;
            [SerializeField] public Image allPerfectAchivement;
        }

        [Serializable]
        private class Buttons
        {
            /// <summary>
            /// 設定ボタン。
            /// </summary>
            [SerializeField] public Button setting;

            /// <summary>
            /// スタートボタン。
            /// </summary>
            [SerializeField] public Button start;
        }

        void Start()
        {
            // 最初に１度実行して背景に色を適用させる
            OnDifficultyChanged();

            buttons.setting.onClick.AddListener(settingWindowManager.Open);
        }

        public void OnSongSelected()
        {
            levelAndDifficulty.level.text = MenuInfo.menuInfo.Level;
            
            song.name.Text = MenuInfo.menuInfo.Name;
            song.artist.Text = MenuInfo.menuInfo.Artist;

            coverAndAchivement.cover.sprite = MenuInfo.menuInfo.Cover;
        }

        public void OnDifficultyChanged()
        {
            // ウィンドウの背景の更新
            windowMesh.SetColorTrigger(MenuInfo.menuInfo.Difficulty);
            windowBorderCurve.SetColorTrigger(MenuInfo.menuInfo.Difficulty);
            // メニューウィンドウの更新
            levelAndDifficulty.level.text = MenuInfo.menuInfo.Level;
            levelAndDifficulty.Difficulty.text = MenuInfo.menuInfo.DifficultyTo().Item1;
        }
    }
}

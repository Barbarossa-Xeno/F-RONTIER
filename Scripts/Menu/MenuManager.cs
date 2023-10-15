using System;
using UnityEngine;
using UnityEngine.Events;
using FancyScrollView.FRONTIER;
using FRONTIER.Save;
using FRONTIER.Utility;

namespace FRONTIER.Menu
{
    /// <summary>
    /// メニュー画面の総括的な管理を行う。
    /// </summary>
    public class MenuManager : MonoBehaviour, IMenu
    {
        #region フィールド

        /// <summary>
        /// 難易度を変えるスライダー。
        /// </summary>
        [SerializeField] private DifficultySlider difficultySlider;

        /// <summary>
        /// 曲のハイライトを入れる配列。
        /// </summary>
        private AudioClip[] songHighlights;

        /// <summary>
        /// 楽曲データを表示するときのソートを管理するインスタンス。
        /// </summary>
        public Events events = new();

        #endregion

        #region クラス

        /// <summary>
        /// 楽曲データ更新のイベントを管理する。
        /// </summary>
        [Serializable]
        public class Events
        {
            /// <summary>
            /// 難易度が更新されたときに発火するイベント。
            /// </summary>
            [Header("難易度が更新されたときに発火するイベント")] public UnityEvent<int> OnDifficultyChanged;

            /// <summary>
            /// ソートの基準が変更された時に発火するイベント。
            /// </summary>
            [Header("ソートの基準が変更された時に発火するイベント")] public UnityEvent<int> OnSortOptionChanged;

            /// <summary>
            /// ソート順が変更されたときに発火するイベント。
            /// </summary>
            [Header("ソート順が変更されたときに発火するイベント")] public UnityEvent<int> OnSortOrderChanged;        
        }

        #endregion

        #region MonoBehaviourメソッド

        void Awake()
        {
            LoadData();
            difficultySlider.AddListener(events.OnDifficultyChanged);
        }

        #endregion

        #region メソッド

        /// <summary>
        /// リソースフォルダーからファイルを読み込む。
        /// </summary>
        private void LoadData()
        {
            NotificationData.Instance.Load();
            SongData.Instance.Load();
            LoadAudio();
        }

        /// <summary>
        /// メニューで流れる楽曲のハイライトのオーディオクリップを読み込む。
        /// </summary>
        private void LoadAudio()
        {
            songHighlights = new AudioClip[SongData.Instance.songs.Length];
            for (int i = 0; i < songHighlights.Length; i++) { songHighlights[i] = Resources.Load<AudioClip>($"Data/{i}/highlight"); }
        }

        private int id_tmp = -1;
        /// <summary>
        /// ハイライトを再生する。
        /// </summary>
        /// <param name="id">曲のID</param>
        private void PlayHighLight(int id)
        {
            if (id_tmp == id) { return; }
            id_tmp = id;
            GameManager.instance.audios.musicManager.Source.clip = songHighlights[id];
            GameManager.instance.audios.musicManager.Source.Play();
        }

        #endregion

        #region 実装メソッド

        public void OnSongSelected(int id) => PlayHighLight(id);

        public void OnDifficultyChanged(int difficulty) => OnDifficultyChanged((Reference.DifficultyRank)difficulty);

        public void OnSortOptionChanged(int option) => OnSortOptionChanged((IMenu.Sort.Option)option);

        public void OnSortOrderChanged(int order) => OnSortOrderChanged((IMenu.Sort.Order)order);

        public void OnDifficultyChanged(Reference.DifficultyRank difficulty)
        {
            // メニュー全体の難易度の更新
            MenuInfo.menuInfo.Update(difficulty);
        }

        public void OnSortOptionChanged(IMenu.Sort.Option option) => MenuInfo.menuInfo.SortOption = option;

        public void OnSortOrderChanged(IMenu.Sort.Order order) => MenuInfo.menuInfo.SortOrder = order;

        #endregion
    }
}
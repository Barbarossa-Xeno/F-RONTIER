using System;
using System.IO;
using UnityEngine;
using FancyScrollView.FRONTIER;
using UnityEngine.Scripting;
using Game.Save;
using Game.Utility.Development;
using Game.Utility;

namespace Game.Menu
{
    ///<summary>
    ///メニュー画面の総括的な管理を行う。
    ///</summary>
    public class MenuManager : MonoBehaviour, IMenu
    {
        /* フィールド */

        /// <summary>
        /// <see cref = "ScrollManager"/>
        /// </summary>
        [SerializeField] private ScrollManager scrollManager;

        /// <summary>
        /// <see cref = "DifficultySlider"/>
        /// </summary>
        [SerializeField] private DifficultySlider slider;

        /// <summary>
        /// <see cref = "Window.WindowMenu"/>
        /// </summary>
        public Window.WindowMenu windowMenu;

        /// <summary>
        /// 曲のハイライトを入れる配列。
        /// </summary>
        private AudioClip[] songHighlights;

        /// <summary>
        /// 難易度が更新されたときに発火させるアクション。
        /// </summary>
        public Action OnDifficultyChangedAction { get; set; }

        /// <summary>
        /// 楽曲データを表示するときのソートを管理するインスタンス。
        /// </summary>
        public SongSort songSort = new();

        /// <summary>
        /// 楽曲データを表示するときのソートを管理する。
        /// </summary>
        public class SongSort
        {
            /// <summary>
            /// ソートの基準が変更された時に発火するイベント。
            /// </summary>
            public Action<IMenu.SortOption> OnSortOptionChanged { get; set; }

            /// <summary>
            /// ソート順が変更されたときに発火するイベント。
            /// </summary>
            public Action<IMenu.SortOrder> OnSortOrderChanged { get; set; }

            public SongSort()
            {
                // コンストラクタで、ソートが変わった時に値を反映させるイベントを登録させる
                OnSortOptionChanged += (option) => MenuInfo.menuInfo.SortOption = option;
                OnSortOrderChanged += (order) => MenuInfo.menuInfo.SortOrder = order;
            }
        }


        void Awake()
        {
            GarbageCollector.GCMode = GarbageCollector.Mode.Enabled;
            //初期化処理。後にセーブデータに対応する予定
            LoadData();

            // イベントを登録
            OnDifficultyChangedAction += OnDifficultyChanged;
            OnDifficultyChangedAction += windowMenu.OnDifficultyChanged;
            OnDifficultyChangedAction += scrollManager.OnDifficultyChanged;

            slider.OnDifficultyChanged(OnDifficultyChangedAction);
        }

        /// <summary>
        /// リソースフォルダーからファイルを読み込む。
        /// </summary>
        private void LoadData()
        {
            SettingData.Instance.Load();
            NotificationData.Instance.Load();
            SongData.Instance.Load();
            LoadAudio();
        }

        /// <summary>
        /// 設定ファイルを保存します。
        /// </summary>
        public void SaveSetting(string parentPath)
        {
            string serialedSaveData = JsonUtility.ToJson(SettingData.Instance, true);
            StreamWriter streamWriter = new StreamWriter(parentPath + "/Setting.json");
            streamWriter.Write(serialedSaveData);
            streamWriter.Flush();
            streamWriter.Close();
            DevelopmentExtentionMethods.LogEditor($"{parentPath}への保存に成功しました。");
        }

        /// <summary>
        /// メニューで流れる楽曲のハイライトのオーディオクリップを読み込む。
        /// </summary>
        private void LoadAudio()
        {
            songHighlights = new AudioClip[SongData.Instance.songs.Length];
            for (int i = 0; i < songHighlights.Length; i++) { songHighlights[i] = Resources.Load<AudioClip>($"Data/{i}/highlight"); }
        }

        public void OnSongSelected(ItemData itemData)
        {
            PlayHighLight(itemData.id);
            MenuInfoUpdate(itemData);
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
            GameManager.instance.musicSource.clip = songHighlights[id];
            GameManager.instance.musicSource.Play();
        }

        /// <summary>
        /// メニューで選択された曲情報を更新する。
        /// </summary>
        /// <param name="itemDatas">セルのデータ</param>
        /// <param name="index">曲のインデックス</param>
        private void MenuInfoUpdate(ItemData itemData)
        {
            MenuInfo.menuInfo.ID = itemData.id;
            MenuInfo.menuInfo.Name = itemData.name;
            MenuInfo.menuInfo.Artist = itemData.artist;
            MenuInfo.menuInfo.Level = itemData.ChangeLevel(MenuInfo.menuInfo.Difficulty);
            MenuInfo.menuInfo.DifficultyColor = MenuInfo.menuInfo.DifficultyTo().Item2;
            MenuInfo.menuInfo.Cover = Resources.Load<Sprite>($"Data/{itemData.id}/cover");
        }

        public void OnDifficultyChanged()
        {
            // メニュー全体の難易度の更新
            MenuInfo.menuInfo.Difficulty = (Reference.DifficultyEnum)Enum.ToObject(typeof(Reference.DifficultyEnum), slider.SliderValue);
            // 難易度に応じてアイテムデータのレベルも更新する
            MenuInfo.menuInfo.Level = scrollManager.ItemDatas[MenuInfo.menuInfo.indexInMenu].ChangeLevel(MenuInfo.menuInfo.Difficulty);
        }

        /// <summary>
        /// ソート時、以前選択していた曲が持っていたIDを通じて、メニュー内のセルのインデックスを参照する。
        /// </summary>
        /// <param name="datas">項目</param>
        /// <returns>以前選択されていた曲のソート後のインデックス位置</returns>
        public int GetIndexInMenu(in ItemData[] datas, out int index)
        {
            int _index = 0;
            for (int i = 0; i < datas.Length; i++)
            {
                if (datas[i].id == MenuInfo.menuInfo.ID)
                {
                    _index = datas[i].cellIndex;
                    break;
                }
            }
            index = _index;
            return _index;
        }
    }
}
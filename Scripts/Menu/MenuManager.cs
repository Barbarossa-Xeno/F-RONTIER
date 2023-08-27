using System;
using System.IO;
using UnityEngine;
using FancyScrollView.FRONTIER;
using UnityEngine.Scripting;
using Game.Menu.Save;
using Game.Utility.Development;
using Game.Utility;

namespace Game.Menu
{
    ///<summary>
    ///メニュー画面の総括的な管理を行う。
    ///</summary>
    public class MenuManager : MonoBehaviour
    {
        /* フィールド */

        /// <summary>
        /// <see cref = "ScrollManager"/>
        /// </summary>
        [SerializeField] private ScrollManager scrollManager;

        /// <summary>
        /// <see cref = "SongData"/>
        /// </summary>
        public SongData songData;

        ///<summary>現在選択中の難易度ランク。</summary>
        private Reference.DifficultyEnum difficulty;

        private AudioClip[] songHighlights;


        void Awake()
        {
            GarbageCollector.GCMode = GarbageCollector.Mode.Enabled;
            //初期化処理。後にセーブデータに対応する予定
            LoadData();
            difficulty = MenuInfo.menuInfo.difficulty != 0
                ? MenuInfo.menuInfo.difficulty
                : Utility.Reference.DifficultyEnum.Lite;
            scrollManager.difficulty = difficulty;
        }

        void Update()
        {

            //GameManager.instance.musicSource.volume = (float)SettingData.instance.save[0].volumeMusic / 10f;
            //GameManager.instance.seSource.volume = (float)SettingData.instance.save[0].volumeSE / 10f;
        }

        /// <summary>
        /// リソースフォルダーからファイルを読み込む。
        /// </summary>
        private void LoadData()
        {
            LoadSetting(Application.persistentDataPath);
            songData = LoadSongs();
            LoadAudio();
        }

        /// <summary>
        /// 楽曲のデータをロードする。
        /// </summary>
        /// <returns>楽曲データ</returns>
        private SongData LoadSongs()
        {
            return JsonUtility.FromJson<SongData>(Resources.Load<TextAsset>("SongData").ToString());
        }

        ///<summary>設定ファイルとお知らせを記載したファイルを読み込みます。</summary>
        public void LoadSetting(string parentPath)
        {
            if (File.Exists(parentPath + "/Setting.json"))
            {
                StreamReader streamReader = new StreamReader(parentPath + "/Setting.json");
                string loadData = streamReader.ReadToEnd();
                streamReader.Close();
                SettingData.instance = JsonUtility.FromJson<SettingData>(loadData);
                DevelopmentExtentionMethods.LogEditor($"{Application.persistentDataPath}からの読込に成功しました。");
            }
            else
            {
                SettingData.instance = JsonUtility.FromJson<SettingData>(Resources.Load<TextAsset>("Setting").ToString());
                DevelopmentExtentionMethods.LogValue($"{Application.persistentDataPath}からの読込に失敗したかファイルが存在しないためResourceフォルダから読込を完了しました。");
            }
            Notification.instance = JsonUtility.FromJson<Notification>(Resources.Load<TextAsset>("Notification").ToString());
        }

        ///<summary>設定ファイルを保存します。</summary>
        public void SaveSetting(string parentPath)
        {
            string serialedSaveData = JsonUtility.ToJson(SettingData.instance, true);
            StreamWriter streamWriter = new StreamWriter(parentPath + "/Setting.json");
            streamWriter.Write(serialedSaveData);
            streamWriter.Flush();
            streamWriter.Close();
            DevelopmentExtentionMethods.LogEditor($"{parentPath}への保存に成功しました。");
        }

        private void LoadAudio()
        {
            songHighlights = new AudioClip[songData.songs.Length];
            for (int i = 0; i < songHighlights.Length; i++) { songHighlights[i] = Resources.Load<AudioClip>($"Data/{i}/highlight"); }
        }

        public void PlayHighLight(int index)
        {
            GameManager.instance.musicSource.clip = songHighlights[index];
            GameManager.instance.musicSource.Play();
        }

        public void MenuInfoUpdate(ItemData[] itemDatas, int index)
        {
            MenuInfo.menuInfo.indexInMenu = index;
            MenuInfo.menuInfo.id = itemDatas[index].SongID;
            MenuInfo.menuInfo.name = itemDatas[index].Title;
            MenuInfo.menuInfo.level = itemDatas[index].Level;
            MenuInfo.menuInfo.DifficultyColor = MenuInfo.menuInfo.DifficultyTo().Item2;
            MenuInfo.menuInfo.cover = Resources.Load<Sprite>($"Data/{index}/cover");
        }
    }
}
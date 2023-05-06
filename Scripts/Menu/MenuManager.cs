using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using FancyScrollView.SongSelect;
using Game;
using Game.Save;
using Game.Utility;
using UnityEngine.Scripting;

///<summary>メニュー画面の総括的な管理を行います。</summary>
public class MenuManager : MonoBehaviour
{
    /* フィールド */
    ///<summary>クラス<see cref = "ScrollManager"/>の変数。</summary>
    [SerializeField] ScrollManager scrollManager;
    ///<summary>クラス<see cref = "SongList"/>の変数。</summary>
    private SongList songList;
    ///<summary>難易度を上げるボタン。</summary>
    [SerializeField] Button PlusButton = default;
    ///<summary>難易度を下げるボタン。</summary>
    [SerializeField] Button MinusButton = default;
    ///<summary>現在選択中の難易度ランク。</summary>
    private int rank;


    void Awake()
    {
        GarbageCollector.GCMode = GarbageCollector.Mode.Enabled;
        //初期化処理。後にセーブデータに対応する予定
        LoadJSON();
        rank = MenuInfo.menuInfo.selectedDifficultyNumber != 0
            ? MenuInfo.menuInfo.selectedDifficultyNumber
            : (int)SettingUtility.DifficultyRank.normal;
        scrollManager.difficulty = rank;
    }

    void Update()
    {
        //三項演算子「変数 = 条件式 ? trueの時の処理 : falseの時の処理」を使って簡略化しています。
        PlusButton.interactable = rank > (int)SettingUtility.DifficultyRank.master ? false : true;
        MinusButton.interactable = rank < (int)SettingUtility.DifficultyRank.normal ? false : true;

        GameManager.instance.musicSource.volume = (float)Setting.setting.Save[0].volumeMusic / 10f;
        GameManager.instance.seSource.volume = (float)Setting.setting.Save[0].volumeSE / 10f;
    }

    ///<summary>
    ///楽曲リストをロードします。
    ///</summary>
    private void LoadJSON()
    {
        LoadSetting(Application.persistentDataPath);
        songList = JsonUtility.FromJson<SongList>(Resources.Load<TextAsset>("SongList").ToString());
        scrollManager.AmountOfElementsRange = songList.songs.Length;
        scrollManager.songList = songList;
    }
    ///<summary>設定ファイルとお知らせを記載したファイルを読み込みます。</summary>
    public void LoadSetting(string parentPath)
    {
        if (File.Exists(parentPath + "/Setting.json"))
        {
            StreamReader streamReader = new StreamReader(parentPath + "/Setting.json");
            string loadData = streamReader.ReadToEnd();
            streamReader.Close();
            Setting.setting = JsonUtility.FromJson<Setting>(loadData);
#if UNITY_EDITOR
            Debug.Log($"{Application.persistentDataPath}からの読込に成功しました。");
#endif
        }
        else
        {
            Setting.setting = JsonUtility.FromJson<Setting>(Resources.Load<TextAsset>("Setting").ToString());
#if UNITY_EDITOR
            Debug.Log($"{Application.persistentDataPath}からの読込に失敗したかファイルが存在しないためResourceフォルダから読込を完了しました。");
#endif
        }
        Notification.notification = JsonUtility.FromJson<Notification>(Resources.Load<TextAsset>("Notification").ToString());
    }
    ///<summary>設定ファイルを保存します。</summary>
    public void SaveSetting(string parentPath)
    {
        string serialedSaveData = JsonUtility.ToJson(Setting.setting, true);
        StreamWriter streamWriter = new StreamWriter(parentPath + "/Setting.json");
        streamWriter.Write(serialedSaveData);
        streamWriter.Flush();
        streamWriter.Close();
#if UNITY_EDITOR
        Debug.Log($"{parentPath}への保存に成功しました。");
#endif
    }


    ///<summary>ボタンが押されたとき、変数<see cref = "rank"/>をインクリメントして<see cref = "ScrollManager"/>に渡し、難易度を更新します。</summary>
    public void Plus()
    {
        rank++;
        if (rank > (int)SettingUtility.DifficultyRank.master)
        {
            rank = (int)SettingUtility.DifficultyRank.master;
            PlusButton.interactable = false;
        }
        else
        {
            PlusButton.interactable = true;
        }
        scrollManager.difficulty = rank;
        MenuInfo.menuInfo.selectedDifficultyNumber = rank;
    }
    ///<summary>ボタンが押されたとき、変数<see cref = "rank"/>をデクリメントして<see cref = "ScrollManager"/>に渡し、難易度を更新します。</summary>
    public void Minus()
    {
        rank--;
        if (rank < (int)SettingUtility.DifficultyRank.normal)
        {
            rank = (int)SettingUtility.DifficultyRank.normal;
            MinusButton.interactable = false;
        }
        else
        {
            MinusButton.interactable = true;
        }
        scrollManager.difficulty = rank;
        MenuInfo.menuInfo.selectedDifficultyNumber = rank;
    }
}

///<summary>
///楽曲のデータが記載されたJSONファイルを読み込んで情報を保存するクラスです。
///</summary>
[Serializable]
public class SongList
{
    public Songs[] songs;
}
[Serializable]
public class Songs
{
    public string title;
    public string artist;
    public string works;
    public int songID;
    public Level[] level;
}
[Serializable]
public class Level
{
    public string normal;
    public string hard;
    public string expert;
    public string master;
}
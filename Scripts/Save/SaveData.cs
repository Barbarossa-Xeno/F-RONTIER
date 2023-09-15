using UnityEngine;
using System.IO;

namespace FRONTIER.Save
{
    /// <summary>
    /// JSONファイルで保存されたセーブデータを読み込んだり書き込んだりする。
    /// </summary>
    /// <typeparam name="T">このクラスの継承先</typeparam>
    public class SaveData<T> where T : SaveData<T>, new()
    {
        /// <summary>
        /// このクラスの情報を保持する静的インスタンス。
        /// </summary>
        public static T Instance { get; set; } = new();

        /// <summary>
        /// 扱うデータの種類を定義したクラス。
        /// </summary>
        protected static class DataMode
        {
            public const string SONG = "SongData";
            public const string SETTING = "Setting";
            public const string NOTIFICATION = "Notification";
        }

        public virtual void Load() { }

        public virtual void Save() { } 

        /// <summary>
        /// データをゲーム内のResoucesフォルダーから読みこむ。
        /// </summary>
        /// <param name="dataMode">読み込むデータ</param>
        protected void Load(string dataMode) => Instance = JsonUtility.FromJson<T>(Resources.Load<TextAsset>(dataMode).ToString());

        /// <summary>
        /// データをゲーム内ディレクトリの特定のパスから読み込む。
        /// </summary>
        /// <param name="dataMode">読み込むデータ</param>
        /// <param name="dataPath">読み込み先のパス</param>
        protected void Load(string dataMode, string dataPath)
        {
            // 特定の場所に読み込みたいファイルが存在する場合
            if (File.Exists(dataPath + $"/{dataMode}.json"))
            {
                StreamReader streamReader = new(dataPath + $"/{dataMode}.json");
                string loadData = streamReader.ReadToEnd();
                streamReader.Close();
                Instance = JsonUtility.FromJson<T>(loadData);

            }
            // 無かったらResoucesファイルにあったデフォルトのファイルを読み込む
            else { Instance = JsonUtility.FromJson<T>(Resources.Load<TextAsset>("Setting").ToString());}
        }

        /// <summary>
        /// データをゲーム内ディレクトリの特定のファイルへ保存する。
        /// </summary>
        /// <param name="dataMode">保存するデータ</param>
        /// <param name="dataPath">保存したいファイルがあるパス</param>
        protected void Save(string dataMode, string dataPath)
        {
            string serialedData = JsonUtility.ToJson(SettingData.Instance, true);
            StreamWriter streamWriter = new(dataPath + $"/{dataMode}.json");
            streamWriter.Write(serialedData);
            streamWriter.Flush();
            streamWriter.Close();
        }
    }


}
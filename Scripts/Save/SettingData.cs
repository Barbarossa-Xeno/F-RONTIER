using UnityEngine;

namespace Game.Save
{
    /// <summary>
    /// 設定データを保持するクラス。
    /// </summary>
    [System.Serializable]
    public class SettingData : SaveData<SettingData>
    {
        /// <summary>
        /// 保存された設定のデータ。
        /// </summary>
        public Setting setting;
        
        [System.Serializable]
        public class Setting
        {
            public float noteSpeed;
            public float timing;
            public bool mirror;
            public float musicVolume;
            public float seVolume;
        }

        /// <summary>
        /// 設定のデータを読み込む。
        /// </summary>
        public override void Load() => base.Load(DataMode.SETTING, Application.persistentDataPath);

        /// <summary>
        /// 設定のデータを書き込む。
        /// </summary>
        public override void Save() => base.Save(DataMode.SETTING, Application.persistentDataPath);
    }
}


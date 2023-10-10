using System.Collections;
using UnityEngine;
using FRONTIER.Utility;

namespace FRONTIER.Audio
{
    /// <summary>
    /// 楽曲の再生をするためのオーディオソースを管理する。
    /// </summary>
    public class MusicManager : AudioManager
    {
        #region フィールド

        /// <summary>
        /// 再生する楽曲。
        /// </summary>
        private AudioClip song;

        #endregion

        #region オーバーライドメソッド

        public override sealed void Construct(Reference.Scene.GameScenes scenes)
        {
            Source.Stop();
            SetVolume(Save.SettingData.Instance.setting.musicVolume);

            switch (scenes)
            {
                case Reference.Scene.GameScenes.Menu:
                    Source.loop = true;
                    break;
                case Reference.Scene.GameScenes.Game:
                    Source.loop = false;
                    song = Resources.Load<AudioClip>($"Data/{GameManager.instance.info.ID}/song");
                    Source.clip = song;
                    break;
            }
        }

        #endregion
    }
}
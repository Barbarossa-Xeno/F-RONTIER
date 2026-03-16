using System.Collections;
using UnityEngine;
using FRONTIER.Utility;

namespace FRONTIER.Audio
{
    /// <summary>
    /// 楽曲などの AudioClip の再生を管理する。
    /// </summary>
    public class MusicManager : AudioManager
    {
        /// <summary>
        /// 再生中の AudioClip が変わったときに発火するイベント。
        /// </summary>
        public event System.Action<AudioClip> ClipChanged;

        /// <summary>
        /// 楽曲などの再生する AudioClip。
        /// </summary>
        public AudioClip Clip
        {
            get => Source.clip;
            set
            {
                Source.clip = value;

                // セッター伝手に発火
                ClipChanged?.Invoke(value);
            }
        }

        public override sealed void Construct(Reference.Scene.GameScenes scenes)
        {
            Source.Stop();
            SetVolume(Save.SettingData.Instance.setting.musicVolume);

            // シーンに応じてオーディオの設定を変える
            // ゲーム中の楽曲のロードはここで行う
            switch (scenes)
            {
                case Reference.Scene.GameScenes.Menu:
                {
                    Source.loop = true;
                    return;
                }
                case Reference.Scene.GameScenes.Game:
                {
                    Source.loop = false;
                    Clip = Resources.Load<AudioClip>($"Data/{GameManager.Instance.info.ID}/song");
                    return;
                }
            }
        }
    }
}

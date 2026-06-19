using UnityEngine;
using UnityEngine.Video;

namespace FRONTIER.Game
{
    /// <summary>
    /// MVを使用する設定が有効な時、背景でMVを再生するクラス
    /// </summary>
    [RequireComponent(typeof(VideoPlayer))]
    public class MVManager : Utility.GameUtilityBase
    {
        public VideoPlayer Player { get; private set; }

        public override void Construct()
        {
            Player = GetComponent<VideoPlayer>();
            Player.source = VideoSource.VideoClip;
            Player.clip = Resources.Load<VideoClip>($"Data/{GameManager.Instance.info.ID}/mv");
        }
    }
}

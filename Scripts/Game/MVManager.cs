using UnityEngine;
using UnityEngine.Video;

namespace FRONTIER.Game
{
    public class MVManager : Utility.GameUtility
    {
        public VideoPlayer Player { get; private set; }

        public override void Construct()
        {
            Player = GetComponent<VideoPlayer>();
            Player.source = VideoSource.VideoClip;
            Player.clip = Resources.Load<VideoClip>($"Data/{GameManager.instance.info.ID}/mv");
        }
    }
}

using UnityEngine;
using UnityEngine.Video;

namespace FRONTIER.Game
{
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

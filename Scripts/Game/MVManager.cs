using UnityEngine;
using UnityEngine.Video;

namespace FRONTIER.Game
{
    public class MVManager : MonoBehaviour
    {
        private VideoClip videoClip;
        private VideoPlayer videoPlayer;
        private MusicManager musicManager;
        private bool isCalledOnce = false;

        // Start is called before the first frame update
        void Start()
        {
            videoPlayer = this.GetComponent<VideoPlayer>();
            videoClip = (VideoClip)Resources.Load($"Data/{GameManager.instance.info.ID}/mv");
            videoPlayer.source = VideoSource.VideoClip;
            videoPlayer.clip = videoClip;

            isCalledOnce = false;
        }

        // Update is called once per frame
        void Update()
        {
            if (!isCalledOnce && GameManager.instance.musicManager.musicPlayed && GameManager.instance.gamePlayState == GameManager.GamePlayState.Playing && GameManager.instance.Mv)
            {
                videoPlayer.Play();
                isCalledOnce = true;
            }
            if (!GameManager.instance.musicManager.musicPlayed && GameManager.instance.gamePlayState == GameManager.GamePlayState.Finishing)
            {
                videoPlayer.Stop();
            }
        }
    }
}

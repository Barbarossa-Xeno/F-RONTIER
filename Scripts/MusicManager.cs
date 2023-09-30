using System.Collections;
using UnityEngine;
using FRONTIER.Utility;

namespace FRONTIER
{
    public class MusicManager : UtilityClass
    {
        /// <summary>
        /// 楽曲を再生するオーディオソース。
        /// </summary>
        public AudioSource Source { get; private set; }

        /// <summary>
        /// 再生する楽曲。
        /// </summary>
        private AudioClip song;

        public bool IsPlaying => Source.isPlaying;

        void Awake() => Source = GetComponent<AudioSource>();

        public override void Construct() => Construct(Reference.Scene.GameScenes.Game);

        public override void Construct(Reference.Scene.GameScenes scenes)
        {
            Source.Stop();

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

        private IEnumerator EndingRoutine()
        {
            yield return new WaitForSeconds(3f);
            GameManager.instance.start = false;
        }

        public void Play() => Source.Play();

        public void Pause() => Source.Pause();

        public void Stop() => Source.Stop();
    }
}
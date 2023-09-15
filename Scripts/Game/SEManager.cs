using UnityEngine;
using FRONTIER.Utility;

namespace FRONTIER.Game
{
    public class SEManager : UtilityClass
    {
        public AudioSource audioSource;

        void Awake() => audioSource = GetComponent<AudioSource>();

        public override void OnSceneLoaded()
        {
            audioSource.Stop();
        }
    }
}
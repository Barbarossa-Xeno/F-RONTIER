using UnityEngine;
using FRONTIER.Utility;

namespace FRONTIER
{
    public class SEManager : UtilityClass
    {
        public AudioSource Source { get; private set; }

        void Awake() => Source = GetComponent<AudioSource>();
    }
}
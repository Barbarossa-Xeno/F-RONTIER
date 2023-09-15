using System.Linq;
using UnityEngine;

namespace FRONTIER.Menu.Background
{
    public class FFT : MonoBehaviour
    {
        /// <summary>
        /// オーディオを取得するAudioSource。
        /// </summary>
        /// <remarks>
        /// 規定は<see cref = "GameManager.musicSource"/>。
        /// </remarks>
        public AudioSource audioSource = default;
        /// <summary>
        /// フーリエ変換の解像度。
        /// </summary>
        [SerializeField] public FFTResolution FFT_resolution = FFTResolution._1024;
        /// <summary>
        /// フーリエ変換の窓関数。
        /// </summary>
        [SerializeField] private FFTWindow FFT_window = FFTWindow.Triangle;
        /// <summary>
        /// 取得する音声情報。
        /// </summary>
        private float[] audioData;
        /// <summary>
        /// 取得する音声の周波数情報。
        /// </summary>
        public float[] spectrumData;
        /// <summary>
        /// 再生されているオーディオが変わったかどうか。
        /// </summary>
        /// <value>変わったならtrue、変わっていなければfalseを返す。</value>
        private bool IsAudioClipChange
        {
            get { return _audioClip != audioSource.clip; }
        }
        /// <summary>
        /// 再生されたオーディオクリップを保存しておく。
        /// </summary>
        private static AudioClip _audioClip = null;
        /// <summary>
        /// フーリエ変換の解像度選択。
        /// </summary>
        public enum FFTResolution
        {
            _8192 = 8192, _4096 = 4096, _2048 = 2048, _1024 = 1024, _512 = 512, _256 = 256, _128 = 128
        }

        void Start()
        {
            audioSource = GameManager.instance.musicSource;
            spectrumData = new float[(int)FFT_resolution];
        }

        void FixedUpdate()
        {
            if (audioSource == null) return;
            //オーディオクリップが変更された時に音声情報を一新する。
            if (IsAudioClipChange)
            {
                _audioClip = audioSource.clip;
                audioData = new float[_audioClip.channels * _audioClip.samples];
                audioSource.clip.GetData(audioData, 0);
            }

            if (audioSource.isPlaying && audioSource.timeSamples < audioData.Length)
            {
                audioSource.GetSpectrumData(spectrumData, 0, FFT_window);
            }
            else { spectrumData = Enumerable.Repeat<float>(0, (int)FFT_resolution).ToArray(); }
        }
    }
}
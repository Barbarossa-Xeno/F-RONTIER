using System;
using System.Linq;
using UnityEngine;

namespace FRONTIER.Menu.Background
{
    /// <summary>
    /// 高速フーリエ変換を行い、再生中の音声の周波数を解析する。
    /// </summary>
    public class FFT : MonoBehaviour
    {
        /// <summary>
        /// FFTの解像度。
        /// </summary>
        [SerializeField] private FFTResolution resolution = FFTResolution._1024;

        /// <summary>
        /// FFTの窓関数。
        /// </summary>
        [SerializeField] private FFTWindow window = FFTWindow.Triangle;

        /// <summary>
        /// オーディオを取得する AudioSource。
        /// </summary>
        /// <remarks>
        /// デフォルトは<see cref = "GameManager.audios.musicManager.Source"/>
        /// </remarks>
        [Header("デフォルトではGameManagerから読み込むためデバッグ用途以外で設定の必要なし"), SerializeField]
        private AudioSource audioSource;

        /// <summary>
        /// 取得する音声情報。
        /// </summary>
        private float[] audioData;

        /// <summary>
        /// 取得する音声の周波数情報。
        /// </summary>
        [SerializeField] private float[] spectrumData;

        /// <summary>
        /// FFTの解像度。
        /// </summary>
        public int Resolution => (int)resolution;

        public float[] SpectrumData => spectrumData;

        /// <summary>
        /// インスペクタからFFTの解像度を選択できるようにするための列挙型。
        /// </summary>
        public enum FFTResolution
        {
            _8192 = 8192, _4096 = 4096, _2048 = 2048, _1024 = 1024, _512 = 512, _256 = 256, _128 = 128
        }

        void OnValidate()
        {
            spectrumData = new float[Resolution];
        }

        void Awake()
        {
            // オーディオソースの取得
            if (audioSource == null)
            {
                audioSource = GameManager.Instance.audios.musicManager.Source;

                // AudioClip の変更検知は GameManager に依存するので
                // デバッグ目的で AudioSource を設定したときは別方法で検知する必要がある
                GameManager.Instance.audios.musicManager.ClipChanged += UpdateAudioData;
            }
            
            spectrumData = new float[Resolution];
        }

        void FixedUpdate()
        {
            // 音声情報が取得できなかったらやめる
            if (audioSource == null || audioData == null)
            {
                return;
            }

            // オーディオが流れているとき
            if (audioSource.isPlaying)
            {
                audioSource.GetSpectrumData(spectrumData, 0, window);
            }
            else
            {
                // 0埋め
                Array.Clear(spectrumData, 0, spectrumData.Length);
            }
        }

        void OnDestroy()
        {
            // イベントの購読解除
            GameManager.Instance.audios.musicManager.ClipChanged -= UpdateAudioData;
        }

        /// <summary>
        /// オーディオクリップが変わったときにスペクトルデータを更新する。
        /// </summary>
        public void UpdateAudioData(AudioClip clip)
        {
            audioData = new float[clip.channels * clip.samples];
            clip.GetData(audioData, 0);
        }
    }
}

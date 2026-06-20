using UnityEngine;
using System.Collections.Generic;
using FRONTIER.Utility;

namespace FRONTIER.Menu.Background
{
    /// <summary>
    /// オーディオスペクトラムをビジュアライズするクラス。
    /// </summary>
    [RequireComponent(typeof(FFT))]
    public class AudioVisualizer : MonoBehaviour
    {
        /// <summary>
        /// FFTデータを取得するためのコンポーネント
        /// </summary>
        [SerializeField] protected FFT fft;

        /// <summary>
        /// 表示サイズ
        /// </summary>
        [SerializeField] protected Vector2 size;

        /// <summary>
        /// 振幅
        /// </summary>
        [SerializeField, Min(0f)] protected float amplitude;

        /// <summary>
        /// 波形として表示する周波数帯域
        /// デフォルトで30種類の周波数
        /// </summary>
        /// <remarks>
        /// あくまで既定値。インスペクタの設定から変更されていることがある
        /// </remarks>
        [SerializeField] private int[] frequencyBand = new int[30]
        {
            25, 32, 40, 50, 63, 80, 100, 125, 160, 200, 250, 315, 400, 500, 630, 800, 1000, 1250, 1600, 2000, 2500, 3150, 4000, 5000, 6300, 8000, 10000, 12500, 16000, 20000
        };

        /// <summary>
        /// サンプリングしたスペクトル情報のインデックスの値を格納する
        /// </summary>
        protected int[] samplingIndexes;

        /// <summary>
        /// 周波数帯域
        /// </summary>
        protected int[] band => frequencyBand;

        protected virtual void Start()
        {
            fft = GetComponent<FFT>();

            // サンプリングするインデックスを取得する
            samplingIndexes = GetIndexesOfBand(AudioSettings.outputSampleRate, frequencyBand, fft.Resolution);
        }

        /// <summary>
        /// 周波数帯域（バンド）に対応した元のスペクトルデータ (<see cref="FFT.spectrumData"/>) のインデックスを返す。
        /// </summary>
        /// <param name="samplingFrequency">サンプリングレート（周波数）</param>
        /// <param name="band">バンド</param>
        /// <param name="resolution">フーリエ変換（FFT）の解像度。</param>
        /// <returns>インデックスを格納した配列。</returns>
        protected int[] GetIndexesOfBand(int samplingFrequency, int[] band, int resolution)
        {
            // インデックスの配列
            int[] indexes = new int[band.Length];

            for (int i = 0; i < indexes.Length; i++)
            {
                // 公式からインデックスの式に変形したもの
                // https://tech.mobilefactory.jp/entry/2020/12/21/140000
                indexes[i] = Mathf.RoundToInt(2f * (band[i] * resolution / (float)samplingFrequency));
            }

            return indexes;
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;
using AudioSpectrumVisualize;

/// <summary>
/// オーディオビジュアライザーをつくるクラス。
/// </summary>
public class AudioVisualizer : MonoBehaviour
{
    /// <summary>
    /// <see cref = "AudioSpectrum"/>
    /// </summary>
    [SerializeField] private AudioSpectrum audioSpectrum;
    /// <summary>
    /// スペクトラムの波形とする複数のオブジェクト。
    /// </summary>
    [Header("Planeの3Dオブジェクトをアタッチする")]
    [SerializeField] private List<Transform> waves = default;
    /// <summary>
    /// 波形の振幅のスケール。
    /// </summary>
    [SerializeField] private float waveScale = 10f;
    /// <summary>
    /// １つの波形オブジェクトの横幅。
    /// </summary>
    [SerializeField] private float waveWidthScale = 0.1f;
    /// <summary>
    /// ビジュアライザーの基準位置。
    /// </summary>
    [SerializeField] private Vector3 spectrumOrigin = default;
    /// <summary>
    /// １つの波形オブジェクトどうしの間隔。
    /// </summary>
    [SerializeField] private float spectrumDistance = 2f;
    /// <summary>
    /// 波形オブジェクトの個数。
    /// </summary>
    /// <value>自身の子オブジェクトの数。</value>
    private int waveLength { get { return this.gameObject.GetComponentsInChildren<Transform>().Length - 1; } }
    /// <summary>
    /// 増減する前の波形オブジェクトの数を保持する。
    /// </summary>
    public static int _waveLength = 0;
    /// <summary>
    /// 波形オブジェクトの中心を記録する構造体。
    /// </summary>
    private struct WavePositionOfCenter
    {
        /// <summary>
        /// 波形オブジェクト<see cref = "waves"/>の個数が偶数(= 2n)の場合、その数の半分の値(= n)。
        /// </summary>
        /// <remarks>
        /// 波形オブジェクト<see cref = "waves"/>の個数が奇数(= 2n+1)の場合、その数を２で割って四捨五入した値(= n+0.5を少数第一位で四捨五入 => n+1)。
        /// </remarks>
        public float minPosition;
        /// <summary>
        /// 波形オブジェクト<see cref = "waves"/>の個数が偶数(= 2n)の場合、<see cref = "WavePositionOfCenter.minPosition"/>に＋１した値(= n+1)。
        /// </summary>
        /// <remarks>
        /// 波形オブジェクト<see cref = "waves"/>の個数が奇数(= 2n+1)の場合、この値は定めない（= 0）。
        /// </remarks>
        public float maxPosition;
    }

    //インスペクターの変更に対応する。
    void OnValidate()
    {
        if (waves.Count == 0) { return; }
        AutoWavesPosition(spectrumOrigin, spectrumDistance, onValidateMode: true);
    }

    //初期化
    void Start()
    {
        if (waves.Count == 0) { GetWaveObjects(); }
        AutoWavesPosition(spectrumOrigin, spectrumDistance);
    }

    void Update()
    {
        for (int i = 0; i < waves.Count; i++)
        {
            //波形オブジェクトを取得して、１つずつ波形情報を適用していく。
            Transform wave = waves[i];
            Vector3 localScale = new Vector3();
            //波形オブジェクトの数が変化した場合に対応する。
            try 
            { 
                localScale = wave.localScale;
                localScale.z = audioSpectrum.Levels[i] * waveScale;
                wave.localScale = localScale;
            }
            catch (MissingReferenceException) { waves.RemoveAt(waves.Count - 1); }            
        }
        AutoWavesPosition(spectrumOrigin, spectrumDistance);
    }

    /// <summary>
    /// 波形オブジェクトを自らの子オブジェクトから取得する。
    /// </summary>
    /// <returns>
    /// <see cref = "waves"/>の長さ。
    /// </returns>
    private int GetWaveObjects()
    {
        Transform[] childrenWaves = this.gameObject.GetComponentsInChildren<Transform>();
        waves = new List<Transform>(childrenWaves.Length - 1);
        Debug.Log(childrenWaves.Length);
        for (int i = 0; i < childrenWaves.Length; i++)
        {
            if (childrenWaves[i] == this.transform) { continue; }
            waves.Add(childrenWaves[i]);
        }
        return waves.Count;
    }

    /// <summary>
    /// 波形オブジェクトの位置を自動調整する。
    /// </summary>
    /// <param name="pivot">ビジュアライザーの基準位置。</param>
    /// <param name="distance">波形オブジェクトどうしの間隔。</param>
    /// <param name="onValidateMode">インスペクター上の実行かどうか。（<see cref = "OnValidate"/>内で実行するときtrueにする。）</param>
    private void AutoWavesPosition(Vector3 pivot, float distance, bool onValidateMode = false)
    {
        /* ローカル関数 */
        // 波形オブジェクトの位置をその個数をもとに計算する。
        // 引数１：wavePosition => 波形のオブジェクト数が偶数個か奇数個かによって変える基準位置。
        // 引数２：isOddNum => 波形オブジェクト数が奇数かどうか。
        void WavesTransform(WavePositionOfCenter wavePosition, bool isOddNum = true)
        {
            //波形オブジェクトの横幅。
            float waveWidth = 0;

            for (int i = 0; i < waves.Count; i++)
            {
                waves[i].transform.localScale = new Vector3(waveWidthScale, 1, waveScale);
                waveWidth = waves[i].GetComponent<Renderer>().bounds.size.x;
            }

            //偶数個(2n)の場合。
            if (!isOddNum)
            {
                //n個目のオブジェクトを計算。
                waves[(int)wavePosition.minPosition - 1].transform.position = new Vector3((pivot.x - distance - waveWidth / 2f) / 2f, pivot.y, pivot.z);
                //n+1個目のオブジェクトを計算。
                waves[(int)wavePosition.maxPosition - 1].transform.position = new Vector3((pivot.x + distance + waveWidth / 2f) / 2f, pivot.y, pivot.z);
                //1個目からn-1個目までのオブジェクトを計算。
                for (int i = 0; i < (int)wavePosition.minPosition - 1; i++)
                {
                    waves[i].transform.position = new Vector3((waves[(int)wavePosition.minPosition - 1].transform.position.x + ((int)wavePosition.minPosition - 1 - i) * (-distance - waveWidth / 2)), pivot.y, pivot.z);
                }
                //n+2個目から2n個までのオブジェクトを計算。
                for (int i = (int)wavePosition.maxPosition; i < waves.Count; i++)
                {
                    waves[i].transform.position = new Vector3((waves[(int)wavePosition.maxPosition - 1].transform.position.x - ((int)wavePosition.maxPosition - 1 - i) * (distance + waveWidth / 2)), pivot.y, pivot.z);
                }
            }
            //奇数個(2n+1)の場合。
            else
            {
                //n+1個目のオブジェクトを計算。
                waves[(int)wavePosition.minPosition - 1].transform.position = pivot;
                //1個目からn個目までのオブジェクトを計算。
                for (int i = 0; i < (int)wavePosition.minPosition - 1; i++)
                {
                    waves[i].transform.position = new Vector3((waves[(int)wavePosition.minPosition - 1].transform.position.x + ((int)wavePosition.minPosition - 1 - i) * (-distance - waveWidth / 2)), pivot.y, pivot.z);
                }
                //n+2個目から2n+1個目までのオブジェクトを計算。
                for (int i = (int)wavePosition.minPosition; i < waves.Count; i++)
                {
                    waves[i].transform.position = new Vector3((waves[(int)wavePosition.minPosition - 1].transform.position.x - ((int)wavePosition.minPosition - 1 - i) * (distance + waveWidth / 2)), pivot.y, pivot.z);
                }
            }
        }

        //波形オブジェクトが偶数個、奇数個かで中心位置を調整する。
        WavePositionOfCenter centerWave = new WavePositionOfCenter();

        if (waves.Count == 0) { return; }
        
        if (_waveLength != waveLength)
        {  
            _waveLength = GetWaveObjects();
        }
        else if (_waveLength == waveLength && !onValidateMode) { return; }

        if ((float)waves.Count % 2f == 0)
        {
            centerWave.minPosition = (float)waves.Count / 2f;
            centerWave.maxPosition = centerWave.minPosition + 1f;
            WavesTransform(centerWave, isOddNum: false);
        }
        else
        {
            centerWave.minPosition = (float)Math.Round(((float)waves.Count / 2f), 0, MidpointRounding.AwayFromZero);
            centerWave.maxPosition = 0;
            WavesTransform(centerWave, isOddNum: true);
        }
    }
}

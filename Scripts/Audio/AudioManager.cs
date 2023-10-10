using UnityEngine;
using FRONTIER.Utility;

namespace FRONTIER.Audio
{
    /// <summary>
    /// オーディオの再生を管理するための抽象クラス。
    /// </summary>
    public abstract class AudioManager : GameUtility
    {
        #region プロパティ

        /// <summary>
        /// 再生するためのオーディオソース。
        /// </summary>
        public AudioSource Source { get; protected set; }

        #endregion

        #region MonoBehaviourメソッド

        protected virtual void Awake() => Source = GetComponent<AudioSource>();

        #endregion

        #region メソッド

        public abstract override void Construct(Reference.Scene.GameScenes scene);

        /// <summary>
        /// オーディオを再生する。
        /// </summary>
        public virtual void Play() => Source?.Play();

        /// <summary>
        /// オーディオを一時停止する。
        /// </summary>
        public virtual void Pause() => Source?.Pause();

        /// <summary>
        /// オーディオを停止する。
        /// </summary>
        public virtual void Stop() => Source?.Stop();

        /// <summary>
        /// オーディオソースの音量を変更する。
        /// </summary>
        /// <param name="volume">変更後の音量</param>
        public virtual void SetVolume(float volume) => Source.volume = Mathf.Clamp01(volume);

        #endregion
    }
}

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using FRONTIER.Utility;
using FancyScrollView.FRONTIER;


namespace FRONTIER.Menu
{
    /// <summary>
    /// 難易度を変更するためのスライダー。
    /// </summary>
    public class DifficultySlider : MonoBehaviour
    {
        /// <summary>
        /// スクロールの状態を取得する。
        /// </summary>
        [SerializeField] private ScrollCondition scrollCondition;

        /// <summary>
        /// スライダー本体。
        /// </summary>
        [SerializeField] private Slider slider;

        /// <summary>
        /// 選択可能な最大難易度。
        /// </summary>
        [SerializeField] private Reference.DifficultyRank maxDifficulty;

        /// <summary>
        /// スライダーのハンドルをカスタマイズする要素。
        /// </summary>
        [Header("スライダーのハンドルをカスタマイズ"), SerializeField] private SliderHandle sliderHandle;

        /// <summary>
        /// スライダーのハンドル部分。
        /// </summary>
        [Serializable]
        private class SliderHandle
        {
            public RectTransform handle;
            public RectTransform handleShadow;
            public RectTransform floatCursor;
        }

        /// <summary>
        /// スライダーの値。
        /// </summary>
        public int SliderValue => (int)slider.value;

        /// <summary>
        /// 外部クラスのメソッドやイベントを<see cref = "slider"/>の値変更時のイベントに登録させるためのメソッド。
        /// </summary>
        /// <param name="callBack">難易度が変更されたときに発火したいイベント</param>
        public void AddListener(UnityEvent<int> callBack)
            => slider.onValueChanged.AddListener((value) => callBack?.Invoke((int)value));

        void Start()
        {
            Initialize();
        }

        void Update()
        {
            SliderActivate(scrollCondition.scrollState);
            HandleFollowSlider();
        }

        /// <summary>
        /// スライダーを初期化する。
        /// </summary>
        private void Initialize()
        {
            slider.minValue = (int)Reference.DifficultyRank.Lite;
            slider.maxValue = (int)maxDifficulty;
            slider.wholeNumbers = true;
            slider.value = (int)MenuInfo.menuInfo.Difficulty;
        }

        /// <summary>
        /// スライダーの表示を切り替える。
        /// </summary>
        /// <param name="state">スクロールビューのスクロール状態。</param>
        private void SliderActivate(ScrollCondition.ScrollState state)
        {
            switch (state)
            {
                case ScrollCondition.ScrollState.Scrolling:
                    slider.gameObject.SetActive(false);
                    break;
                case ScrollCondition.ScrollState.Selecting:
                    slider.gameObject.SetActive(true);
                    break;
            }
        }

        /// <summary>
        /// スライダーのハンドルをスライダーの動きに追従させる。
        /// </summary>
        private void HandleFollowSlider()
        {
            sliderHandle.handleShadow.anchorMin = new Vector2(sliderHandle.handle.anchorMin.x, 0);
            sliderHandle.handleShadow.anchorMax = new Vector2(sliderHandle.handle.anchorMax.x, 0);
            sliderHandle.floatCursor.anchorMin = new Vector2(sliderHandle.handle.anchorMin.x, 0);
            sliderHandle.floatCursor.anchorMax = new Vector2(sliderHandle.handle.anchorMax.x, 0);

            sliderHandle.handleShadow.anchoredPosition = new Vector2(0, 10);
            sliderHandle.floatCursor.anchoredPosition = new Vector2(0, 10);
        }
    }
}
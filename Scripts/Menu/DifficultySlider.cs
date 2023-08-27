using UnityEngine;
using UnityEngine.UI;
using System;
using Game.Utility;
using FancyScrollView.FRONTIER;

namespace Game.Menu
{
    public class DifficultySlider : MonoBehaviour
    {
        [SerializeField] private ScrollCondition scrollCondition;
        [SerializeField] private Slider slider;
        [Tooltip("スライダーのハンドルをカスタマイズ")][SerializeField] private SliderHandle sliderHandle;

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

        void Start()
        {
            SliderInit();
        }

        void Update()
        {
            SliderActivate(scrollCondition.scrollState);
            HandleFollowSlider();
        }

        /// <summary>
        /// スライダーを初期化する。
        /// </summary>
        private void SliderInit()
        {
            slider.minValue = (int)Reference.DifficultyEnum.Lite;
            slider.maxValue = (int)Reference.DifficultyEnum.Restricted;
            slider.wholeNumbers = true;
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
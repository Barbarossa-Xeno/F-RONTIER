using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using FRONTIER.Save;
using FRONTIER.Utility.Asset;

namespace FRONTIER.Menu.Window.Setting
{
    /// <summary>
    /// 各設定項目の値を設定する。
    /// </summary>
    public class SettingValues : MonoBehaviour
    {
        #region フィールド

        /// <summary>
        /// このオブジェクトで設定する要素。
        /// </summary>
        [Header("このオブジェクトで設定する要素"), SerializeField] private SettingItems element = SettingItems.None;

        /// <summary>
        /// 設定の仕方がどのタイプか。
        /// </summary>
        [SerializeField] private ValueType valueType = ValueType.Float;

        // TODO: エディター拡張で、elementの値に応じて表示内容を変える
        /// <summary>
        /// 直接値を指定する設定項目。
        /// </summary>
        [Header("直接値を指定する設定項目"), SerializeField] private FloatTypeValue floatTypeValue;

        /// <summary>
        /// スライダーで値を指定する設定項目。
        /// </summary>
        [Header("スライダーで値を指定する設定項目"), SerializeField] private SliderTypeValue sliderTypeValue;

        /// <summary>
        /// トグルスイッチで値を指定する設定項目。
        /// </summary>
        [Header("トグルスイッチで値を指定する設定項目"), SerializeField] private BoolTypeValue boolTypeValue;

        #endregion

        #region 列挙型・クラス

        /// <summary>
        /// その設定項目の値の設定の仕方を、どう行うか表す。
        /// </summary>
        private enum ValueType
        {
            /// <summary>
            /// ボタンをクリックすることによって一定小数だけ値を調整するタイプの設定項目。
            /// </summary>
            Float, 
            /// <summary>
            /// スライダーによって値を範囲設定するタイプの設定項目。
            /// </summary>
            Slider, 
            /// <summary>
            /// スイッチやトグルによって値を決める設定項目。
            /// </summary>
            Bool
        }

        /// <summary>
        /// 数値（小数）によって直接値を設定する項目のUIセット。
        /// </summary>
        [Serializable]
        private class FloatTypeValue : AnyTypeValue
        {
            [SerializeField] private TextMeshProUGUI value;
            [Tooltip("値を-1.0するボタン"), SerializeField] private Button largeMinusButton;
            [Tooltip("値を-0.1するボタン"), SerializeField] private Button smallMinusButton;
            [Tooltip("値を+0.1するボタン"), SerializeField] private Button smallPlusButton;
            [Tooltip("値を+1.0するボタン"), SerializeField] private Button largePlusButton;

            public static class IncrementValue
            {
                public const float LARGE_PLUS = 1.0f;
                public const float SMALL_PLUS = 0.1f;
                public const float SMALL_MINUS = -0.1f;
                public const float LARGE_MINUS = -1.0f;
            }

            public override void Init(SettingItems element)
            {
                ValueInit(element);
                largeMinusButton.onClick.AddListener(() => SelectAction(element, IncrementValue.LARGE_MINUS));
                smallMinusButton.onClick.AddListener(() => SelectAction(element, IncrementValue.SMALL_MINUS));
                smallPlusButton.onClick.AddListener(() => SelectAction(element, IncrementValue.SMALL_PLUS));
                largePlusButton.onClick.AddListener(() => SelectAction(element, IncrementValue.LARGE_PLUS));
            }

            protected override void ValueInit(SettingItems element)
            {
                switch (element)
                {
                    case SettingItems.NoteSpeed:
                    {
                        value.text = $"{SettingData.Instance.setting.noteSpeed}";
                        break;
                    }
                    case SettingItems.JudgementTiming:
                    {
                        value.text = $"{SettingData.Instance.setting.timing}";
                        break;
                    }
                    case SettingItems.LaneWall:
                    {
                        // TODO: 未実装
                        // value.text = $"{SettingData.Instance.setting.}";
                        break;
                    }
                }
            }

            private void UpdateValue(ref float targetValue, float increment, SettingItems element)
            {
                UpdateValue(ref targetValue, increment);

                switch (element)
                {
                    case SettingItems.NoteSpeed:
                    {
                        // 上限と下限に来たら値を固定する
                        if (targetValue <= 1.0f)
                        {
                            targetValue = 1.0f;
                            return;
                        }
                        else if(targetValue >= 14.9f)
                        {
                            targetValue = 14.9f;
                            return;
                        }
                        break;
                    }
                    case SettingItems.JudgementTiming:
                    {
                        if (targetValue <= -2.0f)
                        {
                            targetValue = -2.0f;
                            return;
                        }
                        else if(targetValue >= 2.0f)
                        {
                            targetValue = 2.0f;
                            return;
                        }
                        break;
                    }
                    case SettingItems.LaneWall:
                    {
                        if (targetValue <= -5.0f)
                        {
                            targetValue = -5.0f;
                            return;
                        }
                        else if(targetValue >= 5.0f)
                        {
                            targetValue = 5.0f;
                            return;
                        }
                        break;
                    }
                }

                ShowValue(targetValue);
            }

            protected override void ShowValue(in float targetValue) => value.text = $"{targetValue}";

            protected override void UpdateValue(ref float targetValue, float increment)
            {
                targetValue += increment;
                ShowValue(targetValue);
            }

            protected override void SelectAction(SettingItems element, float increment)
            {
                switch (element)
                {
                    case SettingItems.NoteSpeed:
                    {
                        UpdateValue(ref SettingData.Instance.setting.noteSpeed, increment, element);
                        break;
                    }
                    case SettingItems.JudgementTiming:
                    {
                        UpdateValue(ref SettingData.Instance.setting.timing, increment, element);
                        break;
                    }
                    case SettingItems.LaneWall:
                    {
                        // UpdateValue(ref SettingData.Instance.setting.laneWall, increment, element);
                        break;
                    }
                    default: break;
                }
                return;
            }
        }

        /// <summary>
        /// スライダーに応じて値を設定する項目のUIセット。
        /// </summary>
        [Serializable]
        private class SliderTypeValue : AnyTypeValue
        {
            [SerializeField] private TextMeshProUGUI value;
            [SerializeField] private Slider slider;

            public override void Init(SettingItems element)
            {
                ValueInit(element);
                slider.onValueChanged.AddListener((sliderValue) => SelectAction(element, sliderValue));
            }

            protected override void ValueInit(SettingItems element)
            {
                switch (element)
                {
                    case SettingItems.MusicVolume:
                    {
                        value.text = $"{SettingData.Instance.setting.musicVolume}";
                        slider.value = SettingData.Instance.setting.musicVolume;
                        break;
                    }
                    case SettingItems.SEVolume:
                    {
                        value.text = $"{SettingData.Instance.setting.seVolume}";
                        slider.value = SettingData.Instance.setting.seVolume;
                        break;
                    }
                }
            }

            protected override void ShowValue(in float targetValue) => value.text = $"{targetValue}";

            protected override void UpdateValue(ref float targetValue, float increment)
            {
                targetValue = increment;
                ShowValue(targetValue);
            }

            protected override void SelectAction(SettingItems element, float increment)
            {
                switch (element)
                {
                    case SettingItems.MusicVolume:
                    {
                        UpdateValue(ref SettingData.Instance.setting.musicVolume, increment);
                        GameManager.Instance.audios.musicManager.SetVolume(SettingData.Instance.setting.musicVolume);
                        break;
                    }
                    case SettingItems.SEVolume:
                    {
                        UpdateValue(ref SettingData.Instance.setting.seVolume, increment);
                        GameManager.Instance.audios.musicManager.SetVolume(SettingData.Instance.setting.seVolume);
                        break;
                    }
                    default : break;
                }
                return;
            }
        }

        /// <summary>
        /// トグルスイッチで値を設定する項目のUIセット。
        /// </summary>
        [Serializable]
        private class BoolTypeValue : AnyTypeValue
        {
            [SerializeField] public ToggleSwitch toggle;

            public override void Init(SettingItems element)
            {
                toggle.OnToggleChanged += (value) => SettingData.Instance.setting.mirror = value;
            }

            protected override void ValueInit(SettingItems element)
            {
                switch (element)
                {
                    case SettingItems.MirrorNotes:
                        toggle.IsOn = SettingData.Instance.setting.mirror;
                        break;
                }
            }

            protected override void ShowValue(in float targetValue) { }

            protected override void UpdateValue(ref float targetValue, float increment) { }

            protected override void SelectAction(SettingItems element, float increment)
            {
                switch (element)
                {
                    case SettingItems.MirrorNotes:
                        UpdateValue(ref SettingData.Instance.setting.mirror, increment);
                        break;
                    default : break;
                }
                return;
            }
        }

        #endregion

        #region MonoBehaviourメソッド

        void Start()
        {
            // 自分が設定することになっている項目に応じて初期化
            if (valueType == ValueType.Float)
            {
                floatTypeValue.Init(element);
            }
            else if (valueType == ValueType.Slider)
            {
                sliderTypeValue.Init(element);
            }
            else if (valueType == ValueType.Bool)
            {
                boolTypeValue.Init(element);
            }
        }

        #endregion
    }
}

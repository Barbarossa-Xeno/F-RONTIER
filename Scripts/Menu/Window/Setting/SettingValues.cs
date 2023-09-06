using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Game.Save;

namespace Game.Menu.Window.Setting
{
    public class SettingValues : MonoBehaviour
    {
        /// <summary>
        /// このオブジェクトで設定する要素。
        /// </summary>
        [Header("このオブジェクトで設定する要素"), SerializeField] private Element element = Element.None;

        /// <summary>
        /// 設定の仕方がどのタイプか。
        /// </summary>
        [SerializeField] private ValueType valueType = ValueType.Float;

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

        // エディター拡張したいけどめんどいからやらない
        /// <summary>
        /// 直接値を指定する設定項目。
        /// </summary>
        [Header("直接値を指定する設定項目")] public FloatTypeValue floatTypeValue;

        /// <summary>
        /// スライダーで値を指定する設定項目。
        /// </summary>
        [Header("スライダーで値を指定する設定項目")] public SliderTypeValue sliderTypeValue;

        /// <summary>
        /// トグルスイッチで値を指定する設定項目。
        /// </summary>
        [Header("トグルスイッチで値を指定する設定項目")] public BoolTypeValue boolTypeValue;

        public enum Element
        {
            NoteSpeed, JudgementTiming, LaneWall, MirrorNotes, MusicVolume, SEVolume, None
        }

        [Serializable]
        public class FloatTypeValue : BaceTypeValue
        {
            [SerializeField] public TextMeshProUGUI value;
            [Tooltip("値を-1.0するボタン"), SerializeField] public Button LargeMinusButton;
            [Tooltip("値を-0.1するボタン"), SerializeField] public Button SmallMinusButton;
            [Tooltip("値を+0.1するボタン"), SerializeField] public Button SmallPlusButton;
            [Tooltip("値を+1.0するボタン"), SerializeField] public Button LargePlusButton;

            public static class IncrementValue
            {
                public const float LARGE_PLUS = 1.0f;
                public const float SMALL_PLUS = 0.1f;
                public const float SMALL_MINUS = -0.1f;
                public const float LARGE_MINUS = -1.0f;
            }

            public override void Init(Element element)
            {
                ValueInit(element);
                LargeMinusButton.onClick.AddListener(() => SelectActionFromElement(element, IncrementValue.LARGE_MINUS));
                SmallMinusButton.onClick.AddListener(() => SelectActionFromElement(element, IncrementValue.SMALL_MINUS));
                SmallPlusButton.onClick.AddListener(() => SelectActionFromElement(element, IncrementValue.SMALL_PLUS));
                LargePlusButton.onClick.AddListener(() => SelectActionFromElement(element, IncrementValue.LARGE_PLUS));
            }

            protected override void ValueInit(Element element)
            {
                switch (element)
                {
                    case Element.NoteSpeed:
                        value.text = $"{SettingData.Instance.setting.noteSpeed}";
                        break;
                    case Element.JudgementTiming:
                        value.text = $"{SettingData.Instance.setting.timing}";
                        break;
                    case Element.LaneWall:
                        //value.text = $"{SettingData.Instance.setting.}";
                        break;
                }
            }

            private void UpdateValue(ref float targetValue, float increment, Element element)
            {
                UpdateValue(ref targetValue, increment);

                switch (element)
                {
                    case Element.NoteSpeed:
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
                    case Element.JudgementTiming:
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
                    case Element.LaneWall:
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

                ShowValue(targetValue);
            }

            protected override void ShowValue(in float targetValue) => value.text = $"{targetValue}";

            protected override void UpdateValue(ref float targetValue, float increment)
            {
                targetValue += increment;
                ShowValue(targetValue);
            }

            protected override void SelectActionFromElement(Element element, float increment)
            {
                switch (element)
                {
                    case Element.NoteSpeed:
                        UpdateValue(ref SettingData.Instance.setting.noteSpeed, increment, element);
                        break;
                    case Element.JudgementTiming:
                        UpdateValue(ref SettingData.Instance.setting.timing, increment, element);
                        break;
                    case Element.LaneWall:
                        // UpdateValue(ref SettingData.Instance.Data.noteSpeed, increment);
                        break;
                    default: break;
                }
                return;
            }
        }

        [Serializable]
        public class SliderTypeValue : BaceTypeValue
        {
            [SerializeField] public TextMeshProUGUI value;
            [SerializeField] public Slider slider;

            public override void Init(Element element)
            {
                ValueInit(element);
                slider.onValueChanged.AddListener((sliderValue) => SelectActionFromElement(element, sliderValue));
            }

            protected override void ValueInit(Element element)
            {
                switch (element)
                {
                    case Element.MusicVolume:
                        value.text = $"{SettingData.Instance.setting.musicVolume}";
                        slider.value = SettingData.Instance.setting.musicVolume;
                        break;
                    case Element.SEVolume:
                        value.text = $"{SettingData.Instance.setting.seVolume}";
                        slider.value = SettingData.Instance.setting.seVolume;
                        break;
                }
            }

            protected override void ShowValue(in float targetValue) => value.text = $"{targetValue}";

            protected override void UpdateValue(ref float targetValue, float increment)
            {
                targetValue = increment;
                ShowValue(targetValue);
            }

            protected override void SelectActionFromElement(Element element, float increment)
            {
                switch (element)
                {
                    case Element.MusicVolume:
                        UpdateValue(ref SettingData.Instance.setting.musicVolume, increment);
                        break;
                    case Element.SEVolume:
                        UpdateValue(ref SettingData.Instance.setting.seVolume, increment);
                        break;
                    default : break;
                }
                return;
            }
        }

        [Serializable]
        public class BoolTypeValue : BaceTypeValue
        {
            [SerializeField] public ToggleSwitch toggle;

            public override void Init(Element element)
            {
                toggle.OnToggleChanged += (value) => SettingData.Instance.setting.mirror = value;
            }

            protected override void ValueInit(Element element)
            {
                switch (element)
                {
                    case Element.MirrorNotes:
                        toggle.IsOn = SettingData.Instance.setting.mirror;
                        break;
                }
            }

            protected override void ShowValue(in float targetValue) { }

            protected override void UpdateValue(ref float targetValue, float increment) { }

            protected override void SelectActionFromElement(Element element, float increment)
            {
                switch (element)
                {
                    case Element.MirrorNotes:
                        UpdateValue(ref SettingData.Instance.setting.mirror, increment);
                        break;
                    default : break;
                }
                return;
            }
        }

        /// <summary>
        /// 各設定項目の値を決定するための基底抽象クラス。
        /// </summary>
        public abstract class BaceTypeValue
        {
            /// <summary>
            /// 項目を初期化する。
            /// </summary>
            /// <param name="element">そのオブジェクトで何の項目を設定するか。</param>
            public abstract void Init(Element element);

            /// <summary>
            /// 設定の値を初期化する。
            /// </summary>
            /// <param name="element">そのオブジェクトで何の項目を設定するか。</param>
            protected abstract void ValueInit(Element element);

            /// <summary>
            /// 値を表示する。
            /// </summary>
            /// <param name="targetValue">設定された値</param>
            protected abstract void ShowValue(in float targetValue);

            /// <summary>
            /// 設定値を更新する。
            /// </summary>
            /// <param name="targetValue">設定値</param>
            /// <param name="increment">設定値に反映させる変化量（増減分）またはそのものの値</param>
            protected abstract void UpdateValue(ref float targetValue, float increment);

            /// <summary>
            /// 設定値がintの場合floatにキャストして反映する。
            /// </summary>
            /// <param name="targetValue">設定値</param>
            /// <param name="increment">設定値に反映させる変化量（増減分）またはそのものの値</param>
            protected virtual void UpdateValue(ref int targetValue, float increment)
            {
                float casted = (float)targetValue;
                UpdateValue(ref casted, increment);
                targetValue = (int)casted;
            }

            /// <summary>
            /// 設定値が<c>bool</c>の場合、入力された値を０か１にして反映させる。
            /// </summary>
            /// <param name="targetValue">設定値</param>
            /// <param name="increment">設定値に反映させる変化量（増減分）またはそのものの値</param>
            protected virtual void UpdateValue(ref bool targetValue, float increment)
            {
                float clamped = Mathf.Clamp01(increment);
                targetValue = clamped == 0 ? false : clamped == 1 ? true : false;
            }

            /// <summary>
            /// 設定する項目に合わせて実行するメソッドやアクションを選択させる。
            /// </summary>
            /// <param name="element">設定する項目</param>
            /// <param name="increment">設定値に反映させる変化量（増減分）またはそのものの値</param>
            protected abstract void SelectActionFromElement(Element element, float increment);
        }

        void Start()
        {
            if (valueType == ValueType.Float) { floatTypeValue.Init(element); }
            else if (valueType == ValueType.Slider) { sliderTypeValue.Init(element); }
            else if (valueType == ValueType.Bool) { boolTypeValue.Init(element); }
        }
    }
}
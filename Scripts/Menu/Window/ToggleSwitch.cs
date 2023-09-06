using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Game.Utility.Easing;
using System;

namespace Game.Menu.Window
{
    public class ToggleSwitch : MonoBehaviour, IPointerClickHandler
    {
        /// <summary>
        /// 背景画像。
        /// </summary>
        [SerializeField] private BackgroundImage backgroundImage;

        [System.Serializable]
        private struct BackgroundImage
        {
            /// <summary>
            /// オンになった時背景を埋める。
            /// </summary>
            [SerializeField] public Sprite fill;

            /// <summary>
            /// オフになった時枠線になる。
            /// </summary>
            [SerializeField] public Sprite border;
        }

        private ImageComponent image;

        /// <summary>
        /// ハンドルと背景の<c>Image</c>コンポーネント
        /// </summary>
        private struct ImageComponent
        {
            public Image bg;
            public Image handle;
        }
        
        /// <summary>
        /// 背景のトランスフォーム。
        /// </summary>
        [SerializeField] private RectTransform background = default;

        /// <summary>
        /// ハンドルのトランスフォーム。
        /// </summary>
        [SerializeField] private RectTransform handle = default;

        /// <summary>
        /// トグルが変化するスピード。
        /// </summary>
        private const float HANDLE_SPEED = 2.5f;

        /// <summary>
        /// 現在のトグルの状態と、初期値の設定。
        /// </summary>
        [SerializeField] private bool isOn;

        /// <summary>
        /// 現在のトグルの状態。
        /// </summary>
        public bool IsOn { get => isOn; set => isOn = value; }

        public Func<bool, bool> OnToggleChanged { get; set; }

        void OnEnable()
        {
            image.bg = background.GetComponent<Image>();
            image.handle = handle.GetComponent<Image>();
        }

        void Update()
        {
            MoveHandle();
            ChangeBackground(IsOn);
        }

        /// <summary>
        /// ハンドルを動かす。
        /// </summary>
        private void MoveHandle()
        {
            if (handle.anchorMax.x >= 1 || handle.anchorMin.x >= 1)
            {
                handle.anchorMax = new Vector2(1, handle.anchorMax.y);
                handle.anchorMin = new Vector2(1, handle.anchorMin.y);
            }
            else if (handle.anchorMax.x <= 0 || handle.anchorMin.x <= 0)
            {
                handle.anchorMax = new Vector2(0, handle.anchorMax.y);
                handle.anchorMin = new Vector2(0, handle.anchorMin.y);
            }

            if (IsOn && handle.anchorMax.x < 1)
            {
                if (handle.anchorMax.x <= 1 || handle.anchorMin.x <= 1)
                {
                    handle.anchorMax += new Vector2(EasingExtensions.EaseOutQuint(HANDLE_SPEED * Time.deltaTime), 0);
                    handle.anchorMin += new Vector2(EasingExtensions.EaseOutQuint(HANDLE_SPEED * Time.deltaTime), 0);
                }

            }
            else if (!IsOn && handle.anchorMax.x > 0)
            {
                if (handle.anchorMax.x >= 0 || handle.anchorMin.x >= 0)
                {
                    handle.anchorMax -= new Vector2(EasingExtensions.EaseOutQuint(HANDLE_SPEED * Time.deltaTime), 0);
                    handle.anchorMin -= new Vector2(EasingExtensions.EaseOutQuint(HANDLE_SPEED * Time.deltaTime), 0);
                }
            }
        }
        private void ChangeBackground(bool isOn)
        {
            image.bg.sprite = isOn ? backgroundImage.fill : backgroundImage.border;
            image.handle.color = isOn ? Color.white : new Color32(50, 50, 50, 255);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            IsOn = !IsOn;
            OnToggleChanged?.Invoke(isOn);
        }
    }
}

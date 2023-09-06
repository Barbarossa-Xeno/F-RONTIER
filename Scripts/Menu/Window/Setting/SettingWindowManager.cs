using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Menu.Window.Setting
{
    /// <summary>
    /// 設定画面（ウィンドウ）の挙動を管理する。
    /// </summary>
    public class SettingWindowManager : MonoBehaviour
    {
        /// <summary>
        /// 設定画面の中に表示するウィンドウの項目。
        /// </summary>
        [Tooltip("ゲーム設定・オーディオ設定・システム設定")] [SerializeField] private Window window;

        [Serializable]
        private struct Window
        {
            [SerializeField] public GameObject game;
            [SerializeField] public GameObject audio;
            [SerializeField] public GameObject system;
        }

        /// <summary>
        /// ウィンドウの閉じるボタン。
        /// </summary>
        [SerializeField] private Button closeButton;

        /// <summary>
        /// アイテムバーが押されたとき発火するアクション。
        /// </summary>
        public Action<Category> OnClickBar;

        /// <summary>
        /// 現在選択されているカテゴリ。
        /// </summary>
        public Category currentCategory = Category.None;

        /// <summary>
        /// アニメーターコントローラー
        /// </summary>
        private Animator animator;

        private static class AnimatorHash
        {
            public static readonly int _openWindow = Animator.StringToHash("_OpenWindow");
            public static readonly int _closeWindow = Animator.StringToHash("_CloseWindow");
        }

        /// <summary>
        /// 設定項目のカテゴリー。
        /// </summary>
        public enum Category { None, Game, Audio, System }

        /// <summary>
        /// 設定画面が有効（表示されている）かどうか。
        /// </summary>
        public bool Enable { private get => gameObject.activeSelf; set => gameObject.SetActive(value); }

        
        void Awake()
        {
            OnClickBar += OpenEachWindows;
            OnClickBar += (_category) => currentCategory = _category;
            OpenEachWindows(Category.Game);
            animator = animator ?? GetComponent<Animator>();
            closeButton.onClick.AddListener(Close);
        }

        void OnValidate() => animator = animator ?? GetComponent<Animator>();

        /// <summary>
        /// 各項目が選択されたときにウィンドウを展開する。
        /// </summary>
        /// <param name="category">項目カテゴリ</param>
        private void OpenEachWindows(Category category)
        {
            switch (category)
            {
                case Category.Game:
                    window.game.SetActive(true);
                    window.audio.SetActive(false);
                    window.system.SetActive(false);
                    break;
                case Category.Audio:
                    window.game.SetActive(false);
                    window.audio.SetActive(true);
                    window.system.SetActive(false);
                    break;
                case Category.System:
                    window.game.SetActive(false);
                    window.audio.SetActive(false);
                    window.system.SetActive(true);
                    break;
            }
        }

        /// <summary>
        /// 設定ウィンドウを開く。
        /// </summary>
        public void Open()
        {
            animator.SetTrigger(AnimatorHash._openWindow);
        }

        /// <summary>
        /// 設定ウィンドウを閉じる。
        /// </summary>
        public void Close()
        {
            animator.SetTrigger(AnimatorHash._closeWindow);
        }
    }
}

using System;
using UnityEngine;
using UnityEngine.UI;
using FRONTIER.Save;

namespace FRONTIER.Menu.Window.Setting
{
    /// <summary>
    /// 設定画面（ウィンドウ）の挙動を管理する。
    /// </summary>
    public class SettingWindowManager : MonoBehaviour
    {
        #region フィールド

        /// <summary>
        /// 設定画面の中に表示するウィンドウの項目。
        /// </summary>
        [Header("ゲーム設定・オーディオ設定・システム設定"), SerializeField] private Window window;

        /// <summary>
        /// ウィンドウを閉じるボタン。
        /// </summary>
        [SerializeField] private Button closeButton;

        /// <summary>
        /// 現在選択されているカテゴリ。
        /// </summary>
        public Category currentCategory = Category.None;

        /// <summary>
        /// アニメーターコントローラー
        /// </summary>
        private Animator animator;

        #endregion

        #region プロパティ

        /// <summary>
        /// アイテムバーが押されたとき発火するイベント。
        /// </summary>
        public Action<Category> OnClickBar { get; private set; }

        /// <summary>
        /// 設定画面が有効（表示されている）かどうか。
        /// </summary>
        public bool Enable { private get => gameObject.activeSelf; set => gameObject.SetActive(value); }

        #endregion

        #region 構造体・クラス・列挙型

        [Serializable]
        private struct Window
        {
            /// <summary>
            /// ゲーム設定のウィンドウ。
            /// </summary>
            [SerializeField] public GameObject game;

            /// <summary>
            /// オーディオ設定のウィンドウ。
            /// </summary>
            [SerializeField] public GameObject audio;

            /// <summary>
            /// システム設定のウィンドウ。
            /// </summary>
            [SerializeField] public GameObject system;
        }

        /// <summary>
        /// アニメーターのトリガーのハッシュ値。
        /// </summary>
        private static class AnimatorHash
        {
            public static readonly int _openWindow = Animator.StringToHash("_OpenWindow");
            public static readonly int _closeWindow = Animator.StringToHash("_CloseWindow");
        }

        /// <summary>
        /// 設定項目のカテゴリー。
        /// </summary>
        public enum Category { None, Game, Audio, System }

        #endregion

        #region MonoBehaviourメソッド
        
        void Awake()
        {
            OnClickBar += OpenEachCategories;
            OnClickBar += (_category) => currentCategory = _category;
            OpenEachCategories(Category.Game);
            animator = animator ?? GetComponent<Animator>();
            closeButton.onClick.AddListener(Close);
        }

        void OnValidate() => animator = animator ?? GetComponent<Animator>();

        #endregion

        #region メソッド

        /// <summary>
        /// 各項目が選択されたときにウィンドウを展開する。
        /// </summary>
        /// <param name="category">項目カテゴリ</param>
        private void OpenEachCategories(Category category)
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
            SettingData.Instance.Load();
            animator.SetTrigger(AnimatorHash._openWindow);
        }

        /// <summary>
        /// 設定ウィンドウを閉じる。
        /// </summary>
        public void Close()
        {
            SettingData.Instance.Save();
            animator.SetTrigger(AnimatorHash._closeWindow);
        }

        #endregion
    }
}

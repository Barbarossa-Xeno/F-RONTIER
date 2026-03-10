using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

namespace FRONTIER.Menu
{
    /// <summary>
    /// サイドメニューを管理する。
    /// </summary>
    public class SideMenuManager : MonoBehaviour
    {
        /// <summary>
        /// <see cref="MenuManager"/>
        /// </summary>
        [SerializeField] private MenuManager menuManager;

        /// <summary>
        /// 設定画面の管理スクリプト
        /// </summary>
        [SerializeField] private Window.Setting.SettingWindowManager settingWindowManager;

        [SerializeField] private UnityEvent<int> OnSortOptionChanged;

        [SerializeField] private UnityEvent<int> OnSortOrderChanged;

        /// <summary>
        /// サイドメニューを開閉するボタン。
        /// </summary>
        [SerializeField] private SideMenuButton sideMenuButton;

        /// <summary>
        /// サイドメニューに表示するもの。
        /// </summary>
        [SerializeField] private SideMenuItems sideMenuItems;

        /// <summary>
        /// サイドメニュー上でソートを切り替えたときに情報を表示するもの。
        /// </summary>
        [SerializeField] private SortConditionView sortConditionView;

        /// <summary>
        /// サイドメニューが開かれているか。
        /// </summary>
        private bool isOpen = false;
        
        private Animator sideMenuAnimator;
        private Image sideMenuButtonImage;

        [Serializable]
        private struct SideMenuItems
        {
            public Button sortButton;
            public Button sortOrderButton;
            public Button notificationButton;
            public Button settingButton;
        }

        [Serializable]
        private struct SideMenuButton
        {
            public Button button;
            public Sprite disabled;
            public Sprite enabled;
        }

        [Serializable]
        private class SortConditionView
        {
            /// <summary>
            /// 現在のソートの状態を表示するテキスト
            /// </summary>
            [SerializeField] private TextMeshProUGUI sortOptionText;

            /// <summary>
            /// 現在のソート順の状態を表示するテキスト
            /// </summary>
            [SerializeField] private TextMeshProUGUI sortOrderText;

            public const string SORT_OPTION_INTRO_TEXT = "ソート：";
            public const string SORT_ORDER_INTRO_TEXT = "ソート順：";

            /// <summary>
            /// ソートオプションの変更をテキストに反映する
            /// </summary>
            /// <param name="sortOption">現在のソートのオプション</param>
            public void UpdateSortCondition(IMenu.Sort.Option sortOption) => sortOptionText.text = SORT_OPTION_INTRO_TEXT + EnumToString(sortOption);

            /// <summary>
            /// ソートの並び順の変更をテキストに反映する
            /// </summary>
            /// <param name="sortOrder">現在のソートの並び順</param>
            public void UpdateSortCondition(IMenu.Sort.Order sortOrder) => sortOrderText.text = SORT_ORDER_INTRO_TEXT + EnumToString(sortOrder);

            private static string EnumToString(IMenu.Sort.Option sortOption)
            {
                string message = "";

                switch (sortOption)
                {
                    case IMenu.Sort.Option.ID:
                        message = "ID";
                        break;
                    case IMenu.Sort.Option.Name:
                        message = "名前";
                        break;
                    case IMenu.Sort.Option.Genre:
                        message = "ジャンル";
                        break;
                    case IMenu.Sort.Option.Level:
                        message = "レベル";
                        break;
                }

                return message;
            }

            private static string EnumToString(IMenu.Sort.Order sortOrder)
            {
                string message = "";

                switch (sortOrder)
                {
                    case IMenu.Sort.Order.Ascending:
                        message = "昇順";
                        break;
                    case IMenu.Sort.Order.Descending:
                        message = "降順";
                        break;
                }

                return message;
            }
        }
        
        private static class AnimatorHash
        {
            public static readonly int Open = Animator.StringToHash("Open");
            public static readonly int Close = Animator.StringToHash("Close");
        }

        void OnEnable()
        {
            sideMenuAnimator = GetComponent<Animator>();
            sideMenuButtonImage = sideMenuButton.button.GetComponent<Image>();
            sideMenuButtonImage.sprite = sideMenuButton.disabled;

            sideMenuButton.button.onClick.AddListener(() => isOpen = !isOpen);
            sideMenuButton.button.onClick.AddListener(() => ActWindow(isOpen));

            sideMenuItems.sortButton.onClick.AddListener(ChangeSortOption);
            sideMenuItems.sortOrderButton.onClick.AddListener(ChangeSortOrder);
            sideMenuItems.settingButton.onClick.AddListener(OpenSetting);
        }

        /// <summary>
        /// サイドメニューのウィンドウを開閉する。
        /// </summary>
        /// <param name="trigger">サイドメニューが開かれる状態か。</param>
        private void ActWindow(bool trigger)
        {
            switch (trigger)
            {
                case true:
                    Open();
                    sideMenuButtonImage.sprite = sideMenuButton.enabled;
                    sideMenuButtonImage.color = Color.white;
                    // これはハンバーガーアイコンを非表示にするための
                    sideMenuButtonImage.transform.GetChild(0).GetComponent<Image>().enabled = false;
                    break;
                case false:
                    Close();
                    sideMenuButtonImage.sprite = sideMenuButton.disabled;
                    sideMenuButtonImage.color = Color.black;
                    sideMenuButtonImage.transform.GetChild(0).GetComponent<Image>().enabled = true;
                    break;
            }
        }

        private void Open() => sideMenuAnimator.SetTrigger(AnimatorHash.Open);

        private void Close() => sideMenuAnimator.SetTrigger(AnimatorHash.Close);

        /// <summary>
        /// ソートのオプションを変更する。
        /// </summary>
        private void ChangeSortOption()
        {
            // SortOptionのEnumとintを相互変換して、ボタンが押された毎に値を変えていく
            int enumNum = (int)MenuInfo.menuInfo.SortOption + 1;
            
            if (enumNum < IMenu.Sort.OptionCount) { menuManager.events.OnSortOptionChanged?.Invoke(enumNum); }
            else { menuManager.events.OnSortOptionChanged?.Invoke((int)IMenu.Sort.Option.ID); }

            Debug.Log(MenuInfo.menuInfo.SortOption);
            sortConditionView.UpdateSortCondition(MenuInfo.menuInfo.SortOption);
        }

        /// <summary>
        /// ソートの並び順を変更する。
        /// </summary>
        private void ChangeSortOrder()
        {
            int enumNum = (int)MenuInfo.menuInfo.SortOrder + 1;

            if (enumNum < IMenu.Sort.OrderCount) { menuManager.events.OnSortOrderChanged?.Invoke(enumNum); }
            else { menuManager.events.OnSortOrderChanged?.Invoke((int)IMenu.Sort.Order.Ascending); }

            sortConditionView.UpdateSortCondition(MenuInfo.menuInfo.SortOrder);
        }

        /// <summary>
        /// サイドメニューを閉じて、設定ウィンドウを開く。
        /// </summary>
        private void OpenSetting()
        {
            isOpen = false;
            ActWindow(false);
            settingWindowManager.Open();
        }
    }
}

/*
 * FancyScrollView (https://github.com/setchi/FancyScrollView)
 * Copyright (c) 2020 setchi
 * Licensed under MIT (https://github.com/setchi/FancyScrollView/blob/master/LICENSE)
 * Modified by @roots.eji for "F-RONTIER"
 */

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FRONTIER;
using FRONTIER.Menu;
using FRONTIER.Utility;

namespace FancyScrollView.FRONTIER
{
    /// <summary>
    /// セル バージョン２
    /// </summary>
    class Cell2 : FancyCell<ItemData, Context>, IMenu
    {
        /// <summary>
        /// セルのアニメーションに用いるアニメーター。
        /// </summary>
        [SerializeField] private Animator animator = default;

        /// <summary>
        /// 楽曲のカバー画像。
        /// </summary>
        [SerializeField] private Image cover = default;

        /// <summary>
        /// 曲名。（長い場合スクロール表示する）
        /// </summary>
        [SerializeField] private OverflowTextScroll songName = default;

        /// <summary>
        /// ソートの基準として使っている情報を表示するテキスト。
        /// </summary>
        [SerializeField] private TextMeshProUGUI sortInfo = default;

        /// <summary>
        /// 曲のジャンルを表示するときに設定する項目。
        /// </summary>
        [SerializeField] private Genre genre;

        [System.Serializable]
        private struct Genre
        {
            public GameObject parent;
            public Image background;
            public TextMeshProUGUI genreName;
        }

        // GameObject が非アクティブになると Animator がリセットされてしまうため
        // 現在位置を保持しておいて OnEnable のタイミングで現在位置を再設定します
        float currentPosition = 0;

        /// <summary>
        /// <c>ItemData</c>のテンポラリー変数。セルに表示する情報は自身が保持する。
        /// </summary>
        public ItemData itemData_tmp = null;

        /// <summary>
        /// アニメーターのパラメーターのハッシュ値を保存
        /// </summary>
        private static class AnimatorHash
        {
            public static readonly int scroll = Animator.StringToHash("scroll");
        }
        
        void Update()
        {
            // 屈辱ながら、Updateで実行させるしかなかった...
            UpdateSortInfo();
        }

        void OnEnable()
        {
            UpdatePosition(currentPosition);
        }

        public override void UpdateContent(ItemData itemData)
        {
            // 受け渡されたItemDataの情報がテンポラリーと異なっていたらセルの情報を更新する
            if (UtilityMethod.IsValueChanged(ref itemData_tmp, itemData))
            {
                cover.sprite = Resources.Load<Sprite>($"Data/{itemData.id}/cover");
                songName.Text = itemData.name;
            }
            UpdateSortInfo(MenuInfo.menuInfo.SortOption);
        }

        public override void UpdatePosition(float position)
        {
            currentPosition = position;

            if (animator.isActiveAndEnabled)
            {
                animator.Play(AnimatorHash.scroll, -1, position);
            }

            animator.speed = 0;
        }

        /// <summary>
        /// 現在選択中のソートの基準に応じてセルの表示を変更する。
        /// </summary>
        /// <param name="sortOption">ソートの基準</param>
        private void UpdateSortInfo(IMenu.SortOption sortOption)
        {
            switch (sortOption)
            {
                case IMenu.SortOption.ID:
                case IMenu.SortOption.Genre:
                case IMenu.SortOption.Level:
                    sortInfo.SetText(itemData_tmp.level);
                    break;
                case IMenu.SortOption.Name:
                    sortInfo.SetText(songName.Text.ToCharArray()[0].ToString());
                    break;
            }
            switch (sortOption)
            {
                case IMenu.SortOption.Genre:
                    genre.parent.SetActive(true);
                    switch (itemData_tmp.genre)
                    {
                        case "F":
                            genre.background.color = new Color32(238, 39, 55, 255);
                            genre.genreName.text = "F";
                            break;
                        case "ANIME":
                            genre.background.color = new Color32(229, 179, 73, 255);
                            genre.genreName.text = "ANIME";
                            break;
                        case "GAME":
                            genre.background.color = new Color32(65, 105, 225, 255);
                            genre.genreName.text = "GAME";
                            break;
                    }
                    break;
                default:
                    genre.parent.SetActive(false);
                    break;
            }
        }

        /// <summary>
        /// 現在選択中のソートの基準と難易度に応じてセルの表示を変更する。
        /// </summary>
        private void UpdateSortInfo()
        {
            switch (MenuInfo.menuInfo.SortOption)
            {
                case IMenu.SortOption.ID:
                case IMenu.SortOption.Genre:
                case IMenu.SortOption.Level:
                    sortInfo.SetText(itemData_tmp.level);
                    break;
            }
        }
    }
}



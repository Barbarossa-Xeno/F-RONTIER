/*
 * FancyScrollView (https://github.com/setchi/FancyScrollView)
 * Copyright (c) 2020 setchi
 * Licensed under MIT (https://github.com/setchi/FancyScrollView/blob/master/LICENSE)
 */

using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Game.Menu;
using Game.Save;
using Game.Utility;
using System;

namespace FancyScrollView.FRONTIER
{
    /// <summary>
    /// スクロールビューの内容の取得や、スクロール時の挙動を設定する。
    /// </summary>
    public class ScrollManager : MonoBehaviour
    {
        /// <summary>
        /// スクロールビュー
        /// </summary>
        [SerializeField] ScrollView scrollView = default;

        /// <summary>
        /// １つ次のセルへ移動させるボタン。
        /// </summary>
        [SerializeField] Button prevCellButton = default;

        /// <summary>
        /// １つ前のセルへ移動させるボタン。
        /// </summary>
        [SerializeField] Button nextCellButton = default;

        /// <summary>
        /// 選択されたセルの概要を表示するテキスト。
        /// </summary>
        [SerializeField] Text selectedItemInfo = default;

        /// <summary>
        /// <see cref="MenuManager"/>
        /// </summary>
        [SerializeField] private MenuManager menuManager = default;

        /// <summary>
        /// セルが格納するすべてのデータ。
        /// </summary>
        [SerializeField] private ItemData[] itemDatas;

        /// <summary>
        /// セルが格納するすべてのデータ。
        /// </summary>
        public ItemData[] ItemDatas
        {
            get => itemDatas;
            private set => itemDatas = value;
        }

        [SerializeField] private IMenu.SortOption sortOption = IMenu.SortOption.ID;
        [SerializeField] private IMenu.SortOrder sortOrder = IMenu.SortOrder.Ascending;

        void Start()
        {
            if (prevCellButton != null) prevCellButton.onClick?.AddListener(scrollView.SelectPrevCell);
            if (nextCellButton != null) nextCellButton.onClick?.AddListener(scrollView.SelectNextCell);

            // セルが選択されたときのイベントを登録する
            scrollView.OnSelectionChanged(
                index =>
                {
                    MenuInfo.menuInfo.indexInMenu = index;
                    menuManager.OnSongSelected(ItemDatas[index]);
                    menuManager.windowMenu.OnSongSelected();
                }
            );
            
            // アイテムデータの全取得と反映
            ItemDatas = GetItemData();
            scrollView.UpdateData(ItemDatas);

            scrollView.SelectCell(MenuInfo.menuInfo.indexInMenu);
        }

        void OnValidate()
        {
            SortItemData(sortOption, sortOrder);
        }

        void Update()
        {
            //Update内にもコンストラクタと同じ処理を書くことで難易度の変更に対応しました。
            //itemDatas = GetItemData();
            //scrollView.UpdateData(itemDatas);
        }

        /// <summary>
        /// 曲数だけの<see cref="ItemData"/>を全取得する。
        /// </summary>
        /// <returns><c>ItemData</c>の配列</returns>
        public ItemData[] GetItemData() => Enumerable.Range(0, SongData.Instance.songs.Length).Select(i => new ItemData(SongData.Instance, i, MenuInfo.menuInfo.Difficulty)).ToArray();

        /// <summary>
        /// セルに表示する<see cref="itemDatas"/>をソートする。
        /// </summary>
        /// <param name="sortOption"></param>
        /// <param name="sortOrder"></param>
        public void SortItemData(IMenu.SortOption sortOption, IMenu.SortOrder sortOrder = IMenu.SortOrder.Ascending)
        {
            switch (sortOption)
            {
                case IMenu.SortOption.ID:
                    if (sortOrder == IMenu.SortOrder.Ascending) { Array.Sort(ItemDatas, (a, b) => a.id - b.id); }
                    else if (sortOrder == IMenu.SortOrder.Descending) { Array.Sort(ItemDatas, (a, b) => b.id - a.id); }
                    break;

                case IMenu.SortOption.Name:
                    if (sortOrder == IMenu.SortOrder.Ascending) { Array.Sort(ItemDatas, (a, b) => a.name.CompareTo(b.name)); }
                    else if (sortOrder == IMenu.SortOrder.Descending) { Array.Sort(ItemDatas, (a, b) => b.name.CompareTo(a.name)); }
                    break;

                case IMenu.SortOption.Genre:
                    if (sortOrder == IMenu.SortOrder.Ascending) { Array.Sort(ItemDatas, (a, b) => a.genre.CompareTo(b.genre)); }
                    else if (sortOrder == IMenu.SortOrder.Descending) { Array.Sort(ItemDatas, (a, b) => b.genre.CompareTo(a.genre)); }
                    break;

                case IMenu.SortOption.Level:
                    if (sortOrder == IMenu.SortOrder.Ascending) { Array.Sort(ItemDatas, (a, b) => a.level.CompareTo(b.level)); }
                    else if (sortOrder == IMenu.SortOrder.Descending) { Array.Sort(ItemDatas, (a, b) => b.level.CompareTo(a.level)); }
                    break;
            }

            for (int i = 0; i < ItemDatas.Length; i++) { ItemDatas[i].cellIndex = i; }

            scrollView.UpdateData(ItemDatas);
            scrollView.SelectCell(menuManager.GetIndexInMenu(ItemDatas, out MenuInfo.menuInfo.indexInMenu));
        }
    }
}

/*
 * FancyScrollView (https://github.com/setchi/FancyScrollView)
 * Copyright (c) 2020 setchi
 * Licensed under MIT (https://github.com/setchi/FancyScrollView/blob/master/LICENSE)
 */

using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using FRONTIER.Menu;
using FRONTIER.Save;
using FRONTIER.Utility;

namespace FancyScrollView.FRONTIER
{
    /// <summary>
    /// スクロールビューの内容の取得や、スクロール時の挙動を設定する。
    /// </summary>
    public class ScrollManager : MonoBehaviour, IMenu
    {
        #region フィールド

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
        /// あるセルが選択されて、選択中の曲が変更されたときに発火するイベントを登録する。
        /// </summary>
        [Header("選択された曲が変わった時に発火するイベントを登録"), SerializeField] private UnityEvent<int> OnCellSelected;

        /// <summary>
        /// セルが格納するすべてのデータ。
        /// </summary>
        public ItemData[] ItemDatas { get; private set; }
        
        #endregion

        #region MonoBehaviourメソッド

        void Start()
        {
            if (prevCellButton != null) prevCellButton.onClick?.AddListener(scrollView.SelectPrevCell);
            if (nextCellButton != null) nextCellButton.onClick?.AddListener(scrollView.SelectNextCell);

            // セルが選択されたときのイベントを登録する
            scrollView.OnSelectionChanged(
                index =>
                {
                    MenuInfo.menuInfo.indexInMenu = index;
                    MenuInfo.menuInfo.Update
                    (
                        id: ItemDatas[index].id,
                        name: ItemDatas[index].name,
                        artist: ItemDatas[index].artist,
                        level: ItemDatas[index].ChangeLevel(MenuInfo.menuInfo.Difficulty)
                    );
                    OnSongSelected(ItemDatas[index].id);
                }
            );
            
            // アイテムデータの全取得と反映
            ItemDatas = GetItemData();

            // アイテムの個数（楽曲数）に応じて、無限スクロールにするかを切り替える
            if (ItemDatas.Length < 4)
            {
                scrollView.Loop = false;
                scrollView.Scroller.MovementType = MovementType.Elastic;
            }
            else
            {
                scrollView.Loop = true;
                scrollView.Scroller.MovementType = MovementType.Unrestricted;
            }
            scrollView.UpdateData(ItemDatas);
            scrollView.SelectCell(MenuInfo.menuInfo.indexInMenu);
            UpdateCellsInfo(MenuInfo.menuInfo.SortOption);
        }

        #endregion

        #region メソッド

        /// <summary>
        /// 曲数だけの<see cref="ItemData"/>を全取得する。
        /// </summary>
        /// <returns><c>ItemData</c>の配列</returns>
        private ItemData[] GetItemData() => Enumerable.Range(0, SongData.Instance.songs.Length).Select(i => new ItemData(SongData.Instance, i, MenuInfo.menuInfo.Difficulty)).ToArray();

        /// <summary>
        /// セルに表示する<see cref="itemDatas"/>をソートする。
        /// </summary>
        /// <param name="sortOption">現在選択されているソートの基準</param>
        /// <param name="sortOrder">現在選択されているソートの並び順</param>
        private void SortItemData(IMenu.Sort.Option sortOption, IMenu.Sort.Order sortOrder)
        {
            switch (sortOption)
            {
                case IMenu.Sort.Option.ID:
                    if (sortOrder == IMenu.Sort.Order.Ascending) { Array.Sort(ItemDatas, (a, b) => a.id - b.id); }
                    else if (sortOrder == IMenu.Sort.Order.Descending) { Array.Sort(ItemDatas, (a, b) => b.id - a.id); }
                    break;

                case IMenu.Sort.Option.Name:
                    if (sortOrder == IMenu.Sort.Order.Ascending) { Array.Sort(ItemDatas, (a, b) => a.name.CompareTo(b.name)); }
                    else if (sortOrder == IMenu.Sort.Order.Descending) { Array.Sort(ItemDatas, (a, b) => b.name.CompareTo(a.name)); }
                    break;

                case IMenu.Sort.Option.Genre:
                    if (sortOrder == IMenu.Sort.Order.Ascending) { Array.Sort(ItemDatas, (a, b) => a.genre.CompareTo(b.genre)); }
                    else if (sortOrder == IMenu.Sort.Order.Descending) { Array.Sort(ItemDatas, (a, b) => b.genre.CompareTo(a.genre)); }
                    break;

                case IMenu.Sort.Option.Level:
                    if (sortOrder == IMenu.Sort.Order.Ascending) { Array.Sort(ItemDatas, (a, b) => a.ChangeLevel(MenuInfo.menuInfo.Difficulty).CompareTo(b.ChangeLevel(MenuInfo.menuInfo.Difficulty))); }
                    else if (sortOrder == IMenu.Sort.Order.Descending) { Array.Sort(ItemDatas, (a, b) => b.level.CompareTo(a.level)); }
                    break;
            }

            for (int i = 0; i < ItemDatas.Length; i++) { ItemDatas[i].cellIndex = i; }

            scrollView.UpdateData(ItemDatas);
            scrollView.SelectCell(GetIndexInMenu(out MenuInfo.menuInfo.indexInMenu));
        }

        /// <summary>
        /// 全てのセルへ更新された難易度を適用する。
        /// </summary>
        /// <param name="difficulty"></param>
        private void UpdateCellsInfo(Reference.DifficultyRank difficulty) => scrollView.Cells.Select(fancyCell => fancyCell as Cell2).ToList().ForEach(cell => cell.OnDifficultyChanged(difficulty));

        /// <summary>
        /// 全てのセルへ更新されたソートオプションを適用する。
        /// </summary>
        /// <param name="option"></param>
        private void UpdateCellsInfo(IMenu.Sort.Option option) => scrollView.Cells.Select(fancyCell => fancyCell as Cell2).ToList().ForEach(cell => cell.OnSortOptionChanged(option));

        /// <summary>
        /// ソート時、以前選択していた曲が持っていたIDを通じて、メニュー内のセルのインデックスを参照する。
        /// </summary>
        /// <param name="index">ソート後に変更されたメニュー内のセルのインデックス</param>
        /// <returns>以前選択されていた曲のソート後のインデックス位置</returns>
        public int GetIndexInMenu(out int index)
        {
            int _index = 0;
            for (int i = 0; i < ItemDatas.Length; i++)
            {
                if (ItemDatas[i].id == MenuInfo.menuInfo.ID)
                {
                    _index = ItemDatas[i].cellIndex;
                    break;
                }
            }
            index = _index;
            return _index;
        }

        #endregion

        #region 実装メソッド

        public void OnSongSelected(int id) => OnCellSelected?.Invoke(id);

        public void OnDifficultyChanged(int difficulty) => OnDifficultyChanged((Reference.DifficultyRank)difficulty);

        public void OnSortOptionChanged(int option) => OnSortOptionChanged((IMenu.Sort.Option)option);

        public void OnSortOrderChanged(int order) => OnSortOrderChanged((IMenu.Sort.Order)order);

        public void OnDifficultyChanged(Reference.DifficultyRank difficulty)
        {
            ItemDatas.Select(itemData => itemData.ChangeLevel(difficulty));
            MenuInfo.menuInfo.Update(ItemDatas[MenuInfo.menuInfo.indexInMenu].ChangeLevel(difficulty));
            UpdateCellsInfo(difficulty);
            UpdateCellsInfo(MenuInfo.menuInfo.SortOption);
            SortItemData(MenuInfo.menuInfo.SortOption, MenuInfo.menuInfo.SortOrder);
        }

        public void OnSortOptionChanged(IMenu.Sort.Option option)
        {
            SortItemData(option, MenuInfo.menuInfo.SortOrder);
            UpdateCellsInfo(option);
        }

        public void OnSortOrderChanged(IMenu.Sort.Order order) => SortItemData(MenuInfo.menuInfo.SortOption, order);

        #endregion
    }
}

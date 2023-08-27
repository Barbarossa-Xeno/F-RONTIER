/*
 * FancyScrollView (https://github.com/setchi/FancyScrollView)
 * Copyright (c) 2020 setchi
 * Licensed under MIT (https://github.com/setchi/FancyScrollView/blob/master/LICENSE)
 */

using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Game.Menu;
using Game.Menu.Save;
using Game.Utility;

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

        [HideInInspector] public Reference.DifficultyEnum difficulty;
        /// </summary>
        private ItemData[] itemDatas;

        void Start()
        {
            if (prevCellButton != null) prevCellButton.onClick?.AddListener(scrollView.SelectPrevCell);
            if (nextCellButton != null) nextCellButton.onClick?.AddListener(scrollView.SelectNextCell);
            
            // セルが選択されたときのイベントを登録する
            scrollView.OnSelectionChanged(
                index =>
                {
                    menuManager.PlayHighLight(index);
                    menuManager.MenuInfoUpdate(itemDatas, index);
                }
            );
            
            // アイテムデータの全取得と反映
            itemDatas = GetItemData();
            scrollView.UpdateData(itemDatas);

            scrollView.SelectCell(MenuInfo.menuInfo.indexInMenu);
        }

        void Update()
        {
            //Update内にもコンストラクタと同じ処理を書くことで難易度の変更に対応しました。
            itemDatas = GetItemData();

            scrollView.UpdateData(itemDatas);
        }

        /// <summary>
        /// 曲数だけの<see cref="ItemData"/>を全取得する。
        /// </summary>
        /// <returns><c>ItemData</c>の配列</returns>
        private ItemData[] GetItemData() => Enumerable.Range(0, menuManager.songData.songs.Length).Select(i => new ItemData(menuManager.songData, i, difficulty)).ToArray();

    }
}

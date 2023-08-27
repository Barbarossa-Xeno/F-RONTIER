/*
 * FancyScrollView (https://github.com/setchi/FancyScrollView)
 * Copyright (c) 2020 setchi
 * Licensed under MIT (https://github.com/setchi/FancyScrollView/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using UnityEngine;
using EasingCore;

namespace FancyScrollView.FRONTIER
{
    class ScrollView : FancyScrollView<ItemData, Context>
    {
        [SerializeField] Scroller scroller = default;
        [SerializeField] GameObject cellPrefab = default;
        /// <summary>
        /// スクロールビュー全体のスケール。
        /// </summary>
        [SerializeField] Vector3 cellContainerScale = new Vector3();
        
        private Vector3 viewScale { set { base.cellContainer.localScale = value; } }

        /// <summary>
        /// 選択中のセルが変更されたときに発火されるイベント
        /// </summary>
        private event Action<int> OnSelectionChangedAction;

        protected override GameObject CellPrefab => cellPrefab;

        public bool Holding => scroller.hold;
        public bool Scrolling => scroller.scrolling;
        public bool Dragging => scroller.dragging;

        protected override void Initialize()
        {
            base.Initialize();

            Context.OnCellClicked = SelectCell;

            scroller.OnValueChanged(UpdatePosition);
            scroller.OnSelectionChanged(UpdateSelection);
        }

        void UpdateSelection(int index)
        {
            if (Context.SelectedIndex == index)
            {
                return;
            }

            Context.SelectedIndex = index;
            Refresh();

            OnSelectionChangedAction?.Invoke(index);
        }

        public void UpdateData(IList<ItemData> items)
        {
            UpdateContents(items);
            scroller.SetTotalCount(items.Count);
        }

        /// <summary>
        /// 選択中のセルが変わった時のアクションをイベントに登録する
        /// </summary>
        /// <param name="callback">イベントで発火させる関数</param>
        public void OnSelectionChanged(Action<int> callback)
        {
            OnSelectionChangedAction += callback;
        }

        public void SelectNextCell()
        {
            SelectCell(Context.SelectedIndex + 1);
        }

        public void SelectPrevCell()
        {
            SelectCell(Context.SelectedIndex - 1);
        }

        public void SelectCell(int index)
        {
            if (index < 0 || index >= ItemsSource.Count || index == Context.SelectedIndex)
            {
                return;
            }

            UpdateSelection(index);
            scroller.ScrollTo(index, 0.35f, Ease.OutCubic);
        }

        void Update() { viewScale = cellContainerScale; }
    }
}

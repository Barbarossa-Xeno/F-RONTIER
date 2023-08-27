/*
 * FancyScrollView (https://github.com/setchi/FancyScrollView)
 * Copyright (c) 2020 setchi
 * Licensed under MIT (https://github.com/setchi/FancyScrollView/blob/master/LICENSE)
 * Modified by @roots.eji for "F-RONTIER"
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Menu;
using Game.Utility;

namespace FancyScrollView.FRONTIER
{
    class Cell2 : FancyCell<ItemData, Context>
    {
        /// <summary>
        /// セルのアニメーションに用いるアニメーター。
        /// </summary>
        [SerializeField] private Animator animator = default;

        //ソートクラス実装しよう [SerializeField] private 

        /// <summary>
        /// 楽曲のカバー画像。
        /// </summary>
        [SerializeField] private Image cover = default;

        /// <summary>
        /// 曲名。（長い場合スクロール表示する）
        /// </summary>
        [SerializeField] private OverflowTextScroll songName = default;

        /// <summary>
        /// セル全体の透明度を管轄する。
        /// </summary>
        [SerializeField] private CanvasGroup canvasGroup = default;

        // GameObject が非アクティブになると Animator がリセットされてしまうため
        // 現在位置を保持しておいて OnEnable のタイミングで現在位置を再設定します
        float currentPosition = 0;

        /// <summary>
        /// <c>ItemData</c>のテンポラリー変数
        /// </summary>
        public ItemData itemData_tmp = null;

        /// <summary>
        /// セルが可視状態かどうか。<see cref="canvasGroup"/>のアルファでチェックする
        /// </summary>
        private bool CellVisible { get { return canvasGroup.alpha >= 0.2f; } }

        /// <summary>
        /// アニメーターのパラメーターのハッシュ値を保存
        /// </summary>
        private static class AnimatorHash
        {
            public static readonly int scroll = Animator.StringToHash("scroll");
        }

        void OnEnable()
        {
            UpdatePosition(currentPosition);
        }

        public override void Initialize() { }

        public override void UpdateContent(ItemData itemData)
        {
            // 受け渡されたItemDataの情報がテンポラリーと異なっていたらセルの情報を更新する
            if (UtilityMethod.IsValueChanged(ref itemData_tmp, itemData))
            {
                cover.sprite = Resources.Load<Sprite>($"Data/{itemData.SongID}/cover");
                songName.Text = itemData.Title;

                bool selected = Context.SelectedIndex == Index;

                if (selected || CellVisible)
                {
                    //songName.Init();
                }
            }
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
    }
}



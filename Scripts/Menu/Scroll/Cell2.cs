using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CLSrollProject;

namespace FancyScrollView.SongSelect
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

        [SerializeField] private CLScroll songName = default;

        [SerializeField] private CanvasGroup canvasGroup = default;

        [SerializeField] private CLScrollTextInitialize clScrollList = default;

        // GameObject が非アクティブになると Animator がリセットされてしまうため
        // 現在位置を保持しておいて OnEnable のタイミングで現在位置を再設定します
        float currentPosition = 0;
        
        private bool cellVisible { get { return canvasGroup.alpha >= 0.8f; } }

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
            cover.sprite = Resources.Load<Sprite>($"Data/{itemData.SongID}/cover");
            songName.textArea.text = itemData.Title;

            bool selected = Context.SelectedIndex == Index;

            if (selected || cellVisible)
            {
                clScrollList.UpdateTextCondition();
            }
            else
            {
                clScrollList.UpdateTextCondition(false);
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



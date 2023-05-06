/*
 * FancyScrollView (https://github.com/setchi/FancyScrollView)
 * Copyright (c) 2020 setchi
 * Licensed under MIT (https://github.com/setchi/FancyScrollView/blob/master/LICENSE)
 */

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using CLSrollProject;

namespace FancyScrollView.SongSelect
{
    class Cell : FancyCell<ItemData, Context>
    {
        [SerializeField] Animator animator = default;
        [SerializeField] Text title = default;
        [SerializeField] Text artist = default; 
        [SerializeField] Text works = default;
        [SerializeField] TextMeshProUGUI level = default;
        [SerializeField] TextMeshProUGUI diffcultyText = default;
        [SerializeField] Image image = default;
        [SerializeField] Image BackGround = default;
        [SerializeField] Image CoverImage = default;
        [SerializeField] Button button = default;
        [SerializeField] PressCellActions pressCellActions;
        [SerializeField] private Animator cellParent;
        [SerializeField] CLScroll title_clscroll;
        [SerializeField] CLScroll artist_clscroll;
        [SerializeField] CLScroll works_clscroll;
        private Sprite coverSrc;

        static class AnimatorHash
        {
            public static readonly int Scroll = Animator.StringToHash("scroll");
        }

        public override void Initialize()
        {
            button.onClick.AddListener(() => Context.OnCellClicked?.Invoke(Index));
        }

        public override void UpdateContent(ItemData itemData)
        {
            title.text = itemData.Title;
            artist.text = itemData.Artist;
            works.text = itemData.Works;
            level.text = itemData.Level;
            LoadImage(itemData.SongID);

            var selected = Context.SelectedIndex == Index;
            image.color = selected
                ? new Color(image.color.r, image.color.g, image.color.b, 1f)
                : new Color(image.color.r, image.color.g, image.color.b, 0.6f);
            if(selected){
                this.GetComponent<CLScrollTextInitialize>().UpdateTextCondition();
            }
            else{
                this.GetComponent<CLScrollTextInitialize>().UpdateTextCondition(false);
            }
            UpdateContentByDifficulty(itemData.Difficulty);
        }

        ///<summary>
        ///セルのコンテンツを難易度に応じて変更させます。難易度に基づいて、
        ///<see cref = "BackGround"></see>のカラーと、<see cref = "diffcultyText"></see>のテキストを変更します。
        ///</summary>
        ///<param name = "difficulty">現在選択中の難易度ランク。</param>
        private void UpdateContentByDifficulty(int difficulty){
            switch(difficulty){
                case 0:
                BackGround.color = Game.MenuInfo.menuInfo.selectedImageColor = new Color32(76, 199, 255, 255);                
                diffcultyText.text = Game.MenuInfo.menuInfo.selectedDifficulty = "NORMAL";
                break;
                case 1:
                image.color = Game.MenuInfo.menuInfo.selectedImageColor = new Color32(255, 162, 76, 255);
                diffcultyText.text = Game.MenuInfo.menuInfo.selectedDifficulty = "HARD";
                break;
                case 2:
                image.color = Game.MenuInfo.menuInfo.selectedImageColor = new Color32(255, 76, 89, 255);
                diffcultyText.text = Game.MenuInfo.menuInfo.selectedDifficulty = "EXPERT";
                break;
                case 3:
                image.color = Game.MenuInfo.menuInfo.selectedImageColor = new Color32(140, 76, 255, 255);
                diffcultyText.text = Game.MenuInfo.menuInfo.selectedDifficulty = "MASTER";
                break;
                default: return;
            }
        }

        public override void UpdatePosition(float position)
        {
            currentPosition = position;

            if (animator.isActiveAndEnabled)
            {
                animator.Play(AnimatorHash.Scroll, -1, position);
            }

            animator.speed = 0;
        }

        // GameObject が非アクティブになると Animator がリセットされてしまうため
        // 現在位置を保持しておいて OnEnable のタイミングで現在位置を再設定します
        float currentPosition = 0;

        void OnEnable(){
            UpdatePosition(currentPosition);
        }

        void Start(){
            pressCellActions.modalWindow = GameObject.Find("Canvas").GetComponent<ModalWindow>();
            cellParent = GameObject.Find("ScrollView").GetComponent<Animator>();
        }
        
        ///<param name = "id">楽曲の固有ID。</param>
        ///<summary>楽曲IDを元にカバー画像を取得します。</summary>
        private void LoadImage(int id){
            coverSrc = (Sprite)Resources.Load<Sprite>($"Data/{id}/cover");
            CoverImage.sprite = coverSrc;
        }

        ///<summary>セルが押されたときに実行するメソッド。</summary><remarks>ButtonコンポーネントのOnClickに設定します。</remarks>
        public void CellPressed(){
            cellParent.SetBool("isActive", true);
            pressCellActions.modalWindow.CheckWindowOpened();
        }
        ///<summary>確認ウィンドウなどを閉じたときに<see cref = "CellPressed"></see>で実行されたフェードアウトのアニメーションを取り消してセルを再表示するときに使います。</summary>
        public void CellReturn(){
            cellParent = GameObject.Find("ScrollView").GetComponent<Animator>();
            cellParent.SetBool("isActive", false);
        }
    }

    [System.Serializable]
    class PressCellActions{
        [SerializeField] public ModalWindow modalWindow;
    }
}
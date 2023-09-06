using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Menu.Window.Setting
{
    public class SettingItemBar : MonoBehaviour, IWindow
    {
        /// <summary>
        /// そのボタンで展開する設定の項目（カテゴリ）を選ぶ。
        /// </summary>
        [Header("このボタンが開く設定項目を選択")] public SettingWindowManager.Category category;

        /// <summary>
        /// <see cref = "SettingWindowManager"/>
        /// </summary>
        [SerializeField] private SettingWindowManager settingWindowManager;

        /// <summary>
        /// アイコンを表示する場所。
        /// </summary>
        [SerializeField] private Image icon;
        
        /// <summary>
        /// ボタンが選択されていないとき表示するアイコン。
        /// </summary>
        [Header("選択されていないとき表示するアイコン")][SerializeField] private Sprite outlineIcon;

        /// <summary>
        /// ボタンが選択されているとき表示するアイコン。
        /// </summary>
        [Header("選択されているとき表示するアイコン")][SerializeField] private Sprite fillIcon;


        void Awake() => Init();

        /// <summary>
        /// ※「ゲーム設定」は一番初めに選択されてほしいのでメソッドにしただけ
        /// </summary>
        private void Init()
        {
            if (category == SettingWindowManager.Category.Game)
            {
                icon.sprite = fillIcon;
                icon.GetComponent<RectTransform>().localScale = new(1.1f, 1.1f, 1f);
                GetComponent<Image>().enabled = true;
            }
        }

        /// <summary>
        /// ボタンが押されたとき実行するメソッド。
        /// </summary>
        public void ClickBar()
        {
            settingWindowManager.OnClickBar.Invoke(category);
        }

        /// <summary>
        /// 選択された項目変数のテンポラリー
        /// </summary>
        SettingWindowManager.Category category_tmp;
        
        public void OnChangeButtonState(ButtonState state)
        {
            switch (state)
            {
                // 項目が選択されているとき
                case ButtonState.Selected:
                    icon.sprite = fillIcon;
                    GetComponent<Image>().enabled = true;
                    category_tmp = settingWindowManager.currentCategory;
                    break;
                // 項目が選択されていないとき
                case ButtonState.Unselected:
                    // テンポラリーと現在の選択カテゴリが同じなら表示を変えない
                    if (category_tmp == settingWindowManager.currentCategory) { return; }         
                    icon.sprite = outlineIcon;
                    GetComponent<Image>().enabled = false;
                    break;
            }
        }
    }
}

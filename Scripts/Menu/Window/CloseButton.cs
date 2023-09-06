using UnityEngine;
using UnityEngine.UI;

namespace Game.Menu.Window
{
    /// <summary>
    /// クローズボタン用のスクリプト。
    /// </summary>
    [RequireComponent(typeof(Button), typeof(Image))]
    public class CloseButton : MonoBehaviour, IWindow
    {
        [Header("選択されていないとき表示するアイコン")] [SerializeField] private Sprite unselectedIcon;
        [Header("選択されているとき表示するアイコン")] [SerializeField] private Sprite selectedIcon;
        private Image image;

        void Awake()
        {
            image = image ?? GetComponent<Image>();
            image.sprite = unselectedIcon;
        }

        void OnValidate() => image = image ?? GetComponent<Image>();

        public void OnChangeButtonState(ButtonState state)
        {
            switch (state)
            {
                case ButtonState.Selected:
                    image.sprite = selectedIcon;
                    break;
                case ButtonState.Unselected:
                    image.sprite = unselectedIcon;
                    break;
            }
        }
    }
}

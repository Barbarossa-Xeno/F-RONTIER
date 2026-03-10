using UnityEngine;

namespace FancyScrollView.FRONTIER
{
    /// <summary>
    /// スクロールビューのスクロールの状態を取得する。
    /// </summary>
    public class ScrollCondition : MonoBehaviour
    {
        /// <summary>
        /// <see cref ="ScrollView"/>から情報をとるためのインスタンス
        /// </summary>
        [SerializeField] private ScrollView scrollView;
        
        /// <summary>
        /// スクロールの状態
        /// </summary>
        public ScrollState scrollState = default;
        
        /// <summary>
        /// セルが選択されているか
        /// </summary>
        public bool isSelected = true;
        
        /// <summary>
        /// セルのインデックス番号のテンポラリー
        /// </summary>
        private static int index_tmp = default;
        
        /// <summary>
        /// セルが選択されているかどうかのテンポラリー
        /// </summary>
        private static bool isSelected_tmp = true;
        
        /// <summary>
        /// スクロールの挙動
        /// </summary>
        public enum ScrollState
        {
            Selecting, Scrolling
        }

        void Start()
        {
            scrollView.OnSelectionChanged
            (
                (x) => 
                {
                    if (index_tmp != x)
                    {
                        index_tmp = x;
                        isSelected_tmp = true;
                    }
                    else
                    {
                        isSelected_tmp = false;
                    }
                }
            );
        }

        void Update()
        {
            // スクロールビューがホールドされているか、スクロールされているか、ドラッグされていれば
            if (scrollView.Holding || scrollView.Scrolling || scrollView.Dragging)
            {
                // スクロール中の判定
                scrollState = ScrollState.Scrolling;
                isSelected = false;
            }
            // スクロールビューがホールドされていないか、スクロールされていないか、ドラッグされていないか、かつインデックス番号の更新が一瞬でもあった場合
            else if ((!scrollView.Holding || !scrollView.Scrolling || !scrollView.Dragging) && isSelected_tmp)
            {
                // セルを選択中の判定
                scrollState = ScrollState.Selecting;
                isSelected = true;
            }
        }
    }
}
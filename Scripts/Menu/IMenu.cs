using FancyScrollView.FRONTIER;

namespace Game.Menu
{
    /// <summary>
    /// メニューの処理で実装するインターフェイス
    /// </summary>
    public interface IMenu
    {
        /// <summary>
        /// 曲が選択されたときのメソッド。
        /// </summary>
        public void OnSongSelected() { }

        /// <summary>
        /// 曲が選択されたときのメソッド。(インデックス指定あり)
        /// </summary>
        public void OnSongSelected(int index) { }

        /// <summary>
        /// 曲が選択されたときのメソッド。(アイテムデータの情報指定あり)
        /// </summary>
        public void OnSongSelected(ItemData itemData) { }

        /// <summary>
        /// 難易度の変更があった時のメソッド。
        /// </summary>
        public void OnDifficultyChanged() { }

        public enum SortOption
        {
            ID, Name, Genre, Level
        }
        public enum SortOrder { Ascending, Descending }
    }
}
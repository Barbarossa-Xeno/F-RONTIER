using System;
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

        /// <summary>
        /// 楽曲のソートを何を基準に行うか。
        /// </summary>
        public enum SortOption
        {
            ID, Name, Genre, Level
        }

        /// <summary>
        /// <see cref="SortOption"/>の項目数。
        /// </summary>
        public int SortOptionCount => Enum.GetNames(typeof(SortOption)).Length;

        /// <summary>
        /// 楽曲のソート順。
        /// </summary>
        public enum SortOrder { Ascending, Descending }

        /// <summary>
        /// <see cref="SortOption"/>の項目数。 
        /// </summary>
        public int SortOrderCount => Enum.GetNames(typeof(SortOrder)).Length;
    }
}
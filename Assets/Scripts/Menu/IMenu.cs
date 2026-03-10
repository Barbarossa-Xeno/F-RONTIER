using System;
using FRONTIER.Utility;

namespace FRONTIER.Menu
{
    /// <summary>
    /// メニューの処理で実装するインターフェイス
    /// </summary>
    public interface IMenu
    {
        /// <summary>
        /// ソート基準
        /// </summary>
        public static class Sort
        {
            /// <summary>
            /// <see cref="Option"/>の項目数。
            /// </summary>
            public static int OptionCount => Enum.GetNames(typeof(Option)).Length;

            /// <summary>
            /// <see cref="Order"/>の項目数。 
            /// </summary>
            public static int OrderCount => Enum.GetNames(typeof(Order)).Length;

            /// <summary>
            /// 楽曲のソートを何を基準に行うか。
            /// </summary>
            public enum Option
            {
                ID, Name, Genre, Level
            }

            /// <summary>
            /// 楽曲のソート順。
            /// </summary>
            public enum Order { Ascending, Descending }
        }

        /// <summary>
        /// 曲が選択されたとき実行する処理。
        /// </summary>
        /// <param name="id">曲のID</param>
        public void OnSongSelected(int id);
        
        /// <summary>
        /// 難易度が変更されたとき実行するメソッド。<br/>
        /// - インスペクターから<see cref="MenuManager.Events.OnDifficultyChanged"/>
        /// へアタッチするための指定用メソッド<br/>
        /// - <b>実際の処理はこの関数のオーバーロードを使用して実装することを推奨</b>
        /// </summary>
        /// <example>
        /// <code>
        /// public void OnDifficultyChanged(int difficulty) => OnDifficultyChanged((DifficultyRank)difficulty);
        /// public void OnDifficulyChanged(DifficultyRank difficuty) { Write any routines here. };
        /// </code>
        /// </example>
        /// <param name="difficulty">難易度を数値で指定</param>
        public void OnDifficultyChanged(int difficulty);

        /// <summary>
        /// 難易度が変更された時実行する処理。
        /// </summary>
        /// <param name="difficulty">難易度</param>
        public void OnDifficultyChanged(Reference.DifficultyRank difficulty);

        /// <summary>
        /// ソート基準のオプションが変更されたとき実行するメソッド。<br/>
        /// - インスペクターから<see cref="MenuManager.Events.OnSortOptionChanged"/>
        /// へアタッチするための指定用メソッド<br/>
        /// - <b>実際の処理はこの関数のオーバーロードを使用して実装することを推奨</b>
        /// </summary>
        /// <example>
        /// <code>
        /// public void OnSortOptionChanged(int sortOprion) => OnSortOptionChanged((Sort.Option)option);
        /// public void OnSortOptionChanged(Sort.Option option) { Write any routines here. };
        /// </code>
        /// </example>
        /// <param name="option">オプションを数値で指定</param>
        public void OnSortOptionChanged(int option);

        /// <summary>
        /// ソート基準のオプションが変更されたとき実行する処理。
        /// </summary>
        /// <param name="option">ソートオプション</param>
        public void OnSortOptionChanged(Sort.Option option);

        /// <summary>
        /// ソート順が変更されたとき実行するメソッド。<br/>
        /// - インスペクターから<see cref="MenuManager.Events.OnSortOrderChanged"/>
        /// へアタッチするための指定用メソッド<br/>
        /// - <b>実際の処理はこの関数のオーバーロードを使用して実装することを推奨</b>
        /// </summary>
        /// <example>
        /// <code>
        /// public void OnSortOrderChanged(int order) => OnSortOrderChanged((Sort.Order)order);
        /// public void OnSortOrderChanged(Sort.Order order) { Write any routines here. };
        /// </code>
        /// </example>
        /// <param name="order">オーダー設定を数値で指定</param>
        public void OnSortOrderChanged(int order);

        /// <summary>
        /// ソート順が変更されたとき実行する処理。
        /// </summary>
        /// <param name="order">昇順か降順か</param>
        public void OnSortOrderChanged(Sort.Order order);
    }
}
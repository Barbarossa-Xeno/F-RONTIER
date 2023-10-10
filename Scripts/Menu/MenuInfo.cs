using UnityEngine;
using FRONTIER.Utility;

namespace FRONTIER.Menu
{
    /// <summary>
    /// メニューで選択されている様々な情報を保存する。
    /// </summary>
    [System.Serializable]
    public class MenuInfo : SongInfo
    {
        /// <summary>
        /// 外部からアクセスするための静的インスタンス。
        /// </summary>
        public static MenuInfo menuInfo = new();

        /// <summary>
        /// メニューで選択されていた楽曲が表示されていたセルのインデックスを保持する。
        /// </summary>
        public int indexInMenu;

        /// <summary>
        /// ソート順を決める。
        /// </summary>
        public IMenu.SortOption SortOption { get; set; }

        /// <summary>
        /// ソートが昇順か降順かを決める。
        /// </summary>
        public IMenu.SortOrder SortOrder { get; set; }

        /// <summary>
        /// オートプレイで開始するかどうか。
        /// </summary>
        public virtual bool IsAutoPlay { get; set; }

        /// <summary>
        /// プレイ時にMVを再生するかどうか。
        /// </summary>
        public virtual bool IsMV { get; set; }

        /// <summary>
        /// 継承メソッドでインスタンス（<see cref="menuInfo"/>）の難易度を参照する場合の省略用オーバーロード
        /// </summary>
        /// <returns></returns>
        public (string, Color32) DifficultyTo()
        {
            return DifficultyTo(menuInfo.Difficulty);
        }
    }
}
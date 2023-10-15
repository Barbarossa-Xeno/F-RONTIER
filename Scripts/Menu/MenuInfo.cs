using UnityEngine;
using FRONTIER.Utility;

namespace FRONTIER.Menu
{
    /// <summary>
    /// メニューで選択されている様々な情報を保存する。
    /// </summary>
    [System.Serializable]
    public class MenuInfo : SongInfoManager
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
        public IMenu.Sort.Option SortOption { get; set; }

        /// <summary>
        /// ソートが昇順か降順かを決める。
        /// </summary>
        public IMenu.Sort.Order SortOrder { get; set; }

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

        /// <summary>
        /// ID、曲名、アーティスト名、楽曲レベルを更新する。
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="artist"></param>
        /// <param name="level"></param>
        public void Update(int id, string name, string artist, string level)
        {
            ID = id;
            Name = name;
            Artist = artist;
            Level = level;
            Cover = Resources.Load<Sprite>($"Data/{id}/cover");
        }

        public void Update(Reference.DifficultyRank difficulty) => Difficulty = difficulty;

        public void Update(string level) => Level = level;
    }
}
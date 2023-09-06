using UnityEngine;
using Game.Utility;

namespace Game.Menu
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
        /// 難易度の色（のちのち廃止）
        /// </summary>
        public Color32 DifficultyColor;

        /// <summary>
        /// メニューで選択されていた楽曲が表示されていたセルのインデックスを保持する。
        /// </summary>
        public int indexInMenu;

        public bool autoPlay;
        public bool mv;

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
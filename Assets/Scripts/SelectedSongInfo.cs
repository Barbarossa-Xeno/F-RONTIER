using UnityEngine;
using static FRONTIER.Utility.Reference;

namespace FRONTIER
{
    // TODO: 別で SongData 等も存在しているため、このクラスの役割や命名については要検討

    /// <summary>
    /// 選択された曲の情報を保持するためのクラス。
    /// </summary>
    public class SelectedSongInfo
    {
        /// <summary>
        /// 曲のID。
        /// </summary>
        public virtual int ID { get; set; }

        /// <summary>
        /// 曲名。
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// アーティスト。
        /// </summary>
        public virtual string Artist { get; set; }

        /// <summary>
        /// 作品名。
        /// </summary>
        public virtual string Works { get; set; }

        /// <summary>
        /// 選択中の難易度。
        /// </summary>
        public virtual DifficultyRank Difficulty { get; set; }

        /// <summary>
        /// その曲のレベル。
        /// </summary>
        public virtual string Level { get; set; }

        /// <summary>
        /// その曲のカバー画像。
        /// </summary>
        public virtual Sprite Cover { get; set; }

        /// <summary>
        /// 難易度からその難易度のテキストとイメージカラーを取得する。
        /// </summary>
        /// <param name="difficulty">難易度</param>
        /// <returns><c>Item1</c>: <c>string</c>, <c>Item2</c>: <c>Color32</c> 型になるタプル</returns>
        public (string, Color32) FromDifficulty(DifficultyRank difficulty)
        {            
            return difficulty switch
            {
                DifficultyRank.Lite => (DifficultyValues.LITE, DifficultyValues.Colors.Lite),
                DifficultyRank.Hard => (DifficultyValues.HARD, DifficultyValues.Colors.Hard),
                DifficultyRank.Ecstasy => (DifficultyValues.ECSTASY, DifficultyValues.Colors.Ecstasy),
                DifficultyRank.Restricted => (DifficultyValues.RESTRICTED, DifficultyValues.Colors.Restricted),
                _ => ("", Color.white)
            };
        }
    }
}

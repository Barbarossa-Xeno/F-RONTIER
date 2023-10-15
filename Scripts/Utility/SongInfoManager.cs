using UnityEngine;

namespace FRONTIER.Utility
{
    /// <summary>
    /// 選択された曲の情報を保持するためのクラス。
    /// </summary>
    public class SongInfoManager
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
        public virtual Reference.DifficultyRank Difficulty { get; set; }

        /// <summary>
        /// その曲のレベル。
        /// </summary>
        public virtual string Level { get; set; }

        /// <summary>
        /// その曲のカバー画像。
        /// </summary>
        public virtual Sprite Cover { get; set; }

        /// <summary>
        /// 難易度（<see cref="difficulty"/>）からその難易度の文字列とイメージカラーへ変換する。
        /// </summary>
        /// <param name="_difficulty">難易度</param>
        /// <returns><c>Item1</c> => <c>string</c><br/><c>Item2</c> => <c>Color32</c></returns>
        public (string, Color32) DifficultyTo(Reference.DifficultyRank _difficulty)
        {
            (string, Color32) instance = new();

            switch (_difficulty)
            {
                case Reference.DifficultyRank.Lite:
                    instance.Item1 = Reference.DifficultyUtilities.LITE;
                    instance.Item2 = Reference.DifficultyUtilities.Colors.Lite;
                    break;
                case Reference.DifficultyRank.Hard:
                    instance.Item1 = Reference.DifficultyUtilities.HARD;
                    instance.Item2 = Reference.DifficultyUtilities.Colors.Hard;
                    break;
                case Reference.DifficultyRank.Ecstasy:
                    instance.Item1 = Reference.DifficultyUtilities.ECSTASY;
                    instance.Item2 = Reference.DifficultyUtilities.Colors.Ecstasy;
                    break;
                case Reference.DifficultyRank.Restricted:
                    instance.Item1 = Reference.DifficultyUtilities.RESTRICTED;
                    instance.Item2 = Reference.DifficultyUtilities.Colors.Restricted;
                    break;
                default: return ("", Color.white);
            }
            
            return instance;
        }
    }
}
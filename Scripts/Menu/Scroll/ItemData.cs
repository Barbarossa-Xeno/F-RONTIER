/*
 * FancyScrollView (https://github.com/setchi/FancyScrollView)
 * Copyright (c) 2020 setchi
 * Licensed under MIT (https://github.com/setchi/FancyScrollView/blob/master/LICENSE)
 */

using FRONTIER.Utility;
using FRONTIER.Save;
using UnityEngine;

namespace FancyScrollView.FRONTIER
{
    /// <summary>
    /// セルに表示する項目のデータ。
    /// </summary>
    [System.Serializable]
    public class ItemData
    {
        /// <summary>
        /// 曲名
        /// </summary>
        public string name;

        /// <summary>
        /// アーティスト名
        /// </summary>
        public string artist;

        /// <summary>
        /// 作品名
        /// </summary>
        public string works;

        /// <summary>
        /// 曲のID
        /// </summary>
        public int id;

        /// <summary>
        /// レベル
        /// </summary>
        public string level;

        /// <summary>
        /// ジャンル
        /// </summary>
        public string genre;

        /// <summary>
        /// 難易度
        /// </summary>
        public Reference.DifficultyRank difficulty;

        /// <summary>
        /// セルでのインデックス
        /// </summary>
        public int cellIndex;

        /// <summary>
        /// 難易度に対応したレベルたち
        /// </summary>
        private SongData.Songs.Level levelCollection;


        /// <summary>
        /// 曲の情報を受け取ってセルに反映させるアイテム（項目）の情報を作成する。
        /// </summary>
        /// <param name = "index">セルの番号。</param>
        /// <param name = "data">曲の情報。</param>
        /// <param name = "difficulty">難易度。</param>
        public ItemData(SongData data, int index, Reference.DifficultyRank difficulty)
        {
            name = data.songs[index].name;
            artist = data.songs[index].artist;
            works = data.songs[index].works;
            id = data.songs[index].id;
            genre = data.songs[index].genre;

            this.difficulty = difficulty;
            cellIndex = index;
            levelCollection = data.songs[index].level;

            ChangeLevel(difficulty);
        }

        /// <summary>
        /// <see cref="level"/>を選択中の難易度に合わせて変更させる。
        /// </summary>
        /// <param name="difficulty"></param>
        public string ChangeLevel(Reference.DifficultyRank difficulty)
        {
            switch (difficulty)
            {
                case Reference.DifficultyRank.Lite:
                    level = levelCollection.lite;
                    break;
                case Reference.DifficultyRank.Hard:
                    level = levelCollection.hard;
                    break;
                case Reference.DifficultyRank.Ecstasy:
                    level = levelCollection.ecstacy;
                    break;
                case Reference.DifficultyRank.Restricted:
                    level = levelCollection.restricted;
                    break;
            }
            return level;
        }
    }
}

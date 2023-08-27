/*
 * FancyScrollView (https://github.com/setchi/FancyScrollView)
 * Copyright (c) 2020 setchi
 * Licensed under MIT (https://github.com/setchi/FancyScrollView/blob/master/LICENSE)
 */

using Game.Utility;
using Game.Menu.Save;

namespace FancyScrollView.FRONTIER
{
    [System.Serializable]
    public class ItemData
    {
        public string Message { get; }
        public string Title;
        public string Artist;
        public string Works;
        public string Level;
        public Reference.DifficultyEnum Difficulty;
        public int SongID;

        ///<param name = "index">セルの番号。</param>
        ///<param name = "data">出力させたい情報を入れたクラス。</param>
        ///<param name = "difficulty">難易度数値の指定。</param>
        ///<summary>
        ///配列の文字列を受け取ってそれぞれをコンストラクタで振り分けます。
        ///</summary>
        public ItemData(SongData data, int index, Reference.DifficultyEnum difficulty)
        {
            Title = data.songs[index].name;
            Artist = data.songs[index].artist;
            Works = data.songs[index].works;
            SongID = data.songs[index].id;
            Difficulty = difficulty;
            switch(difficulty){
                case Reference.DifficultyEnum.Lite:
                Level = data.songs[index].level[0].lite;
                break;
                case Reference.DifficultyEnum.Hard:
                Level = data.songs[index].level[0].hard;
                break;
                case Reference.DifficultyEnum.Ecstasy:
                Level = data.songs[index].level[0].ecstacy;
                break;
                case Reference.DifficultyEnum.Restricted:
                Level = data.songs[index].level[0].restricted;
                break;
                default: return;
            }
        }
    }
}

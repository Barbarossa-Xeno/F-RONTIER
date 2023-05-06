/*
 * FancyScrollView (https://github.com/setchi/FancyScrollView)
 * Copyright (c) 2020 setchi
 * Licensed under MIT (https://github.com/setchi/FancyScrollView/blob/master/LICENSE)
 */

 using UnityEngine;

namespace FancyScrollView.SongSelect
{
    [System.Serializable]
    class ItemData
    {
        public string Message { get; }
        public string Title;
        public string Artist;
        public string Works;
        public string Level;
        public int Difficulty;
        public int SongID;

        ///<param name = "index">セルの番号。</param>
        ///<param name = "message">出力させたい情報を入れたクラス。</param>
        ///<param name = "difficulty">難易度数値の指定。</param>
        ///<summary>
        ///配列の文字列を受け取ってそれぞれをコンストラクタで振り分けます。
        ///</summary>
        public ItemData(SongList message, int index, int diffculty)
        {
            Title = message.songs[index].title;
            Artist = message.songs[index].artist;
            Works = message.songs[index].works;
            SongID = message.songs[index].songID;
            Difficulty = diffculty;
            switch(diffculty){
                case 0:
                Level = message.songs[index].level[0].normal;
                break;
                case 1:
                Level = message.songs[index].level[0].hard;
                break;
                case 2:
                Level = message.songs[index].level[0].expert;
                break;
                case 3:
                Level = message.songs[index].level[0].master;
                break;
                default: return;
            }
        }
    }
}

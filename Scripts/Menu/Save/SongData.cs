using System;

namespace Game.Menu.Save
{
    /// <summary>
    /// 楽曲のデータが記載されたJSONファイルを読み込んで情報を保存するクラス。
    /// </summary>
    [Serializable]
    public class SongData
    {
        /// <summary>
        /// 実装される曲の全ての情報。
        /// </summary>
        public Songs[] songs;

        /// <summary>
        /// 曲の情報。
        /// </summary>
        [Serializable]
        public class Songs
        {
            public string name;
            public string artist;
            public string works;
            public int id;
            public Level[] level;

            /// <summary>
            /// レベル
            /// </summary>
            [Serializable]
            public class Level
            {
                public string lite;
                public string hard;
                public string ecstacy;
                public string restricted;
            }
        }

    }

}
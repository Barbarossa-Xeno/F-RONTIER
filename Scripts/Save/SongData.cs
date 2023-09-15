using System;

namespace FRONTIER.Save
{
    /// <summary>
    /// 楽曲のデータを読み込んで情報を保存する。
    /// </summary>
    [Serializable]
    public class SongData : SaveData<SongData>
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
            public Level level;
            public string genre;

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

        /// <summary>
        /// 楽曲のデータを読み込む。
        /// </summary>
        public override void Load() => base.Load(DataMode.SONG);
    }

}
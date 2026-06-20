using System;

namespace FRONTIER.Save
{
    /// <summary>
    /// 楽曲のデータを読み込んで情報を保存する。
    /// </summary>
    [Serializable]
    public class SongData : JSONResource<SongData>
    {
        /// <summary>
        /// 実装される曲の全ての情報。
        /// </summary>
        public SongRecord[] songs;

        /// <summary>
        /// 曲の情報。
        /// </summary>
        [Serializable]
        public class SongRecord
        {
            public string name;
            public string artist;
            public string works;
            public int id;
            public LevelRecord level;
            public string genre;

            /// <summary>
            /// レベル
            /// </summary>
            [Serializable]
            public class LevelRecord
            {
                public string lite;
                public string heavy;
                public string vivid;
                public string beyond;
            }
        }

        public override void Load() => base.Load(DataMode.SONG_DATA);

        public override void Save() => throw new Exception("セーブ機能はありません");
    }

}

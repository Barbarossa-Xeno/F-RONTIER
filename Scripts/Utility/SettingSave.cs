using System;

namespace Game.Save{
    [Serializable]
    public class Setting{
        public static Setting setting = new Setting();
        public DEFAULT[] Default;
        public SAVE[] Save;
        [Serializable]
        public class DEFAULT{
            public float noteSpeed;
            public float timing;
            public bool mirror;
            public int volumeMusic;
            public int volumeSE;
            public int diffculty;
            public int indexPosition;
        }
        [Serializable]
        public class SAVE{
            public float noteSpeed;
            public float timing;
            public bool mirror;
            public int volumeMusic;
            public int volumeSE;
            public int diffculty;
            public int indexPosition;
        }   
    }
}


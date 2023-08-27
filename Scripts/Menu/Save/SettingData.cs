using System;

namespace Game.Menu.Save
{
    [Serializable]
    public class SettingData
    {
        public static SettingData instance = new();
        public Default[] _default;
        public Save[] save;
        [Serializable]
        public class Default
        {
            public float noteSpeed;
            public float timing;
            public bool mirror;
            public int volumeMusic;
            public int volumeSE;
            public int diffculty;
            public int indexPosition;
        }
        [Serializable]
        public class Save
        {
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


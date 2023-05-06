using UnityEngine;

namespace Game{
    [System.Serializable]
    public class MenuInfo{
        public static MenuInfo menuInfo = new MenuInfo();
        public int selectedSongID;
        public string selectedSongTitle;
        public string selectedDifficulty;
        public int selectedDifficultyNumber;
        public string selectedSongLevel;
        public Color32 selectedImageColor;
        public int selectedSongIndexInMenu;
        public Sprite selectedSongCover;
        public bool autoPlay;
        public bool mv;

        public string DifficultyValueToRank(int diffculty){
            selectedDifficultyNumber = diffculty;
            string rank = "";
            switch(diffculty){
                case 0:
                rank = "NORMAL";
                break;
                case 1:
                rank = "HARD";
                break;
                case 2:
                rank = "EXPERT";
                break;
                case 3:
                rank = "MASTER";
                break;
            }
            return rank;
        }

        public Color32 DifficultyToImageColor(int diffculty){
            Color32 color = new Color32();
            switch(diffculty){
                case (int)Game.Utility.SettingUtility.DifficultyRank.normal:     //normal
                color = new Color32(76, 199, 255, 255);
                break;
                case (int)Game.Utility.SettingUtility.DifficultyRank.hard:     //hard
                color = new Color32(255, 162, 76, 255);
                break;
                case (int)Game.Utility.SettingUtility.DifficultyRank.expert:     //expert
                color = new Color32(255, 76, 89, 255);
                break;
                case (int)Game.Utility.SettingUtility.DifficultyRank.master:     //master
                color = new Color32(140, 76, 255, 255);
                break;
            }
            return color;
        }
    }
}
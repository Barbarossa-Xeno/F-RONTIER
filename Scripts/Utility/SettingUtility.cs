using UnityEngine;

namespace Game.Utility
{
    ///<summary>ゲームの要素やユーティリティの情報を持ちます。</summary>
    public static class SettingUtility
    {
        ///<summary>ノーツの種類。</summary>
        public enum NoteType
        {
            Normal = 1,
            LongLinear = 2,
            LongCurve = 3
        }
        ///<summary>ロングノーツのステータス。</summary>
        public enum LongNoteStatus
        {
            Start, Inner, End, Mesh, None
        }
        ///<summary>難易度のランク。</summary>
        public enum DifficultyRank
        {
            normal, hard, expert, master
        }
        ///<summary>判定のステータス。</summary>
        public enum JudgementStatus
        {
            perfect, great, good, bad, miss
        }
        ///<summary>判定のステータスに応じて加算されるスコア。</summary>
        public enum JudgementStatusScore : int
        {
            perfect = 5, great = 3, good = 2, bad = 1, miss = 0
        }
        ///<summary>リザルトスコアのランク。</summary>
        public enum ScoreRank
        {
            S_plus, S, A_plus, A, B_plus, B, C_plus, C
        }
        ///<summary>ランクのボーダー。</summary>
        public enum RankBorder : int
        {
            S_plus = 950000,
            S = 900000,
            A_plus = 800000,
            A = 700000,
            B_plus = 600000,
            B = 500000,
            C_plus = 400000,
            C = 300000
        }
        ///<summary>ノーツを設定する基準と判定線の位置。</summary>
        public static Vector3 origin { get { return new Vector3(0, 0.05f, 7.3f); } }
        ///<summary>特殊ノーツのY座標。</summary>
        public static Vector3 specialNotesPosition { get { return new Vector3(0, 0.1f, 0f); } }
        ///<summary>親オブジェクトと子オブジェクトの全てにレイヤー変更を適用する拡張メソッド。</summary>
        public static void SetLayerSelfChildren(this GameObject self, int layer)
        {
            self.layer = layer;
            //再帰で子オブジェクトにもレイヤーを適用。
            foreach (Transform child in self.transform) { SetLayerSelfChildren(child.gameObject, layer); }
        }
        ///<summary>このプロジェクト内に存在するシーン。</summary>
        public enum GameScenes
        {
            Title, Menu, Game, Result
        }
        /// <summary> 使用するリソースのパス。 </summary>
        public struct ResourcesPath
        {
            public const string TAP_SE_PATH = "SE/tap";
            public const string NOTE_SINGLE_GREAT_SE_PATH = "SE/heel_1";
            public const string NOTE_SINGLE_GOOD_SE_PATH = "SE/heel_2";
        }
        public const float NOTE_SPEED_FACTOR = 2.5f;
    }
}

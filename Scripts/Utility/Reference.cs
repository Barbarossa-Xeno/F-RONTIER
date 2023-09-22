using UnityEngine;

namespace FRONTIER.Utility
{
    /// <summary>
    /// ゲームの自体の設定でよく参照できる情報をまとめたクラス。
    /// </summary>
    public static class Reference
    {
        /// <summary>
        /// ノーツの種類。
        /// </summary>
        public enum NoteType
        {
            Normal = 1,
            LongLinear = 2,
            LongCurve = 3
        }

        /// <summary>
        /// ロングノーツの種類。
        /// </summary>
        public enum LongNoteType
        {
            None,
            /// <summary>
            /// 中間点なし直線型
            /// </summary>
            NoInnerLinear,

            /// <summary>
            /// 中間点あり直線型
            /// </summary>
            AnyInnerLinear,

            /// <summary>
            /// 中間点なし曲線型
            /// </summary>
            NoInnerCurve,

            /// <summary>
            /// 中間点あり曲線型
            /// </summary>
            AnyInnerCurve
        }

        /// <summary>
        /// ロングノーツのステータス。
        /// </summary>
        public enum LongNoteStatus
        {
            Start, Inner, End, Mesh, None
        }
        ///<summary>難易度のランク。</summary>
        public enum DifficultyEnum
        {
            Lite, Hard, Ecstasy, Restricted
        }
        /// <summary>
        /// 難易度のランクの名前とイメージカラー。
        /// </summary>
        public static class DifficultyUtilities
        {
            public const string LITE = "LITE";
            public const string HARD = "HARD";
            public const string ECSTASY = "ECSTASY";
            public const string RESTRICTED = "RESTRICTED";

            /// <summary>
            /// 各難易度に対応したイメージカラー。
            /// </summary>
            public readonly struct Colors
            {
                public static readonly Color32 Lite = new(76, 199, 255, 255);
                public static readonly Color32 Hard = new(255, 157, 13, 255);
                public static readonly Color32 Ecstasy = new(254, 101, 205, 255);
                public static readonly Color32 Restricted = new(238, 39, 55, 255);
            }
        }
        ///<summary>判定のステータス。</summary>
        public enum JudgementStatus
        {
            Perfect, Great, Good, Bad, Miss
        }
        /// <summary>
        /// 判定のステータスに応じて加算されるスコア。
        /// </summary>
        public static class JudgementStatusScore
        {
            public const int PERFECT = 5;
            public const int GREAT = 3;
            public const int GOOD = 2;
            public const int BAD = 1;
            public const int MISS = 0;
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
        public static readonly Vector3 noteOrigin = new(0, 0.05f, 7.3f);
        ///<summary>特殊ノーツのY座標。</summary>
        public static Vector3 SpecialNotesPosition { get { return new Vector3(0, 0.1f, 0f); } }
        ///<summary>親オブジェクトと子オブジェクトの全てにレイヤー変更を適用する拡張メソッド。</summary>
        public static void SetLayerSelfChildren(this GameObject self, int layer)
        {
            self.layer = layer;
            //再帰で子オブジェクトにもレイヤーを適用。
            foreach (Transform child in self.transform) { SetLayerSelfChildren(child.gameObject, layer); }
        }

        public static class Scene
        {
            /// <summary>
            /// このプロジェクト内に存在するシーン。
            /// </summary>
            public enum GameScenes
            {
                Title, Menu, Game, Result
            }

            /// <summary>
            /// 列挙型<see cref="GameScenes"/>を文字列に変換する。
            /// </summary>
            /// <param name="gameScenes">シーン</param>
            /// <returns>シーン名</returns>
            public static string ToString(GameScenes gameScenes)
            {
                string name = "";
                switch (gameScenes)
                {
                    case GameScenes.Title:
                        name = "Title";
                        break;
                    case GameScenes.Menu:
                        name = "Menu";
                        break;
                    case GameScenes.Game:
                        name = "Game";
                        break;
                    case GameScenes.Result:
                        name = "Result";
                        break;
                }
                return name;
            }
            
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

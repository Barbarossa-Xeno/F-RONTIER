using UnityEngine;

namespace FRONTIER.Utility
{
    /// <summary>
    /// ゲームの自体の設定でよく参照できる情報をまとめたクラス。
    /// </summary>
    public static class Reference
    {
        #region 種類・カテゴリ分け

        /// <summary>
        /// ゲーム中のシーン。
        /// </summary>
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
        
        /// <summary>
        /// 難易度のランク。
        /// </summary>
        public enum DifficultyRank
        {
            Lite, Hard, Ecstasy, Restricted
        }

        /// <summary>
        /// 判定のステータス。
        /// </summary>
        public enum JudgementStatus
        {
            Perfect, Great, Good, Bad, Miss
        }
        
        /// <summary>
        /// クリアランク。
        /// </summary>
        public enum ClearRank
        {
            S_Plus, S, A_Plus, A, B_Plus, B, C_Plus, C, NoData
        }

        #endregion

        #region 定数・参照値

        /// <summary>
        /// ノーツを設定する基準と判定線の位置。
        /// </summary>
        public static readonly Vector3 noteOrigin = new(0, 0.0f, 7.3f);

        /// <summary>
        /// 特殊ノーツの基準座標。
        /// </summary>
        public static readonly Vector3 specialNoteOrigin = new (0, 0.05f, 0f);

        public const float NOTE_SPEED_FACTOR = 2.5f;

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

        /// <summary>
        /// ランクのボーダー。
        /// </summary>
        public static class ClearRankBorder
        {
            public const int S_PLUS = 950000;
            public const int S = 900000;
            public const int A_PLUS = 800000;
            public const int A = 700000;
            public const int B_PLUS = 600000;
            public const int B = 500000;
            public const int C_PLUS = 400000;
            public const int C = 300000;
        }
        
        #endregion

        #region メソッド

        /// <summary>
        /// 親オブジェクトと子オブジェクトの全てにレイヤー変更を適用する拡張メソッド。
        /// </summary>
        public static void SetLayerSelfChildren(this GameObject self, int layer)
        {
            self.layer = layer;
            // 再帰で子オブジェクトにもレイヤーを適用
            foreach (Transform child in self.transform) { SetLayerSelfChildren(child.gameObject, layer); }
        }

        #endregion
    }
}

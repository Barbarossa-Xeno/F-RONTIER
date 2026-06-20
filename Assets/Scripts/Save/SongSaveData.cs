using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static FRONTIER.Utility.Reference;

namespace FRONTIER.Save
{
    /// <summary>
    /// 楽曲のプレイデータを保持する。
    /// </summary>
    public class SongSaveData : SaveManager<SongSaveData>
    {
        #region フィールド

        /// <summary>
        /// 楽曲ごとに保存するプレイデータ。
        /// </summary>
        public SongSave[] saves;

        #endregion

        #region クラス

        /// <summary>
        /// 楽曲のプレイデータ。
        /// </summary>
        [System.Serializable]
        public class SongSave
        {
            public int id;

            /// <summary>
            /// 難易度「LITE」のセーブデータ
            /// </summary>
            public Record lite;

            /// <summary>
            /// 難易度「HEAVY」のセーブデータ
            /// </summary>
            public Record heavy;

            /// <summary>
            /// 難易度「VIVID」のセーブデータ
            /// </summary>
            public Record vivid;

            /// <summary>
            /// 難易度「BEYOND」のセーブデータ
            /// </summary>
            public Record beyond;

            /// <summary>
            /// 難易度ごとに記録を保持する。
            /// </summary>
            [System.Serializable]
            public class Record
            {
                public int highScore;
                public int highCombo;
                public string highRank = ClearRank.NoData.ToString();
                public bool fullCombo;
                public bool allPerfect;

                /// <summary>
                /// 記録を上書きする。
                /// </summary>
                /// <param name="score">到達したハイスコア</param>
                /// <param name="rank">到達したハイランク</param>
                /// <param name="fullCombo">フルコンボしたか</param>
                /// <param name="allPerfect">オールパーフェクトしたか</param>
                public void Overwrite(int score = -1, int combo = -1, string rank = null, bool fullCombo = false, bool allPerfect = false)
                {
                    // スコアが上がらなくてもフルコンボをするなどの場合があるため
                    // 初期値（ふつうでは取り得ない値）との比較で以てフィールドを上書きする
                    highScore = score != -1 ? score : highScore;
                    highCombo = combo != -1 ? combo : highCombo;
                    highRank = rank ?? highRank;
                    this.fullCombo = fullCombo;
                    this.allPerfect = allPerfect;
                }
            }

            /// <summary>
            /// 指定された難易度に記録されたデータを返す。
            /// </summary>
            /// <param name="difficulty">難易度</param>
            /// <returns>難易度ごとのセーブデータ、存在しない場合は新規作成したデータ</returns>
            public Record DifficultyTo(DifficultyRank difficulty)
            {
                return difficulty switch
                {
                    DifficultyRank.Lite => lite ?? ConstructNewData(DifficultyRank.Lite),
                    DifficultyRank.Heavy => heavy ?? ConstructNewData(DifficultyRank.Heavy),
                    DifficultyRank.Vivid => vivid ?? ConstructNewData(DifficultyRank.Vivid),
                    DifficultyRank.Beyond => beyond ?? ConstructNewData(DifficultyRank.Beyond),
                    _ => null
                };
            }

            /// <summary>
            /// 各難易度の新しいデータを作る。
            /// </summary>
            /// <param name="difficulty">難易度</param>
            /// <returns>指定した難易度の新しいセーブデータ</returns>
            private Record ConstructNewData(DifficultyRank difficulty)
            {
                switch (difficulty)
                {
                    case DifficultyRank.Lite:
                    {
                        lite = new();
                        return lite;
                    }
                    case DifficultyRank.Heavy:
                    {
                        heavy = new();
                        return heavy;
                    }
                    case DifficultyRank.Vivid:
                    {
                        vivid = new();
                        return vivid;
                    }
                    case DifficultyRank.Beyond:
                    {
                        beyond = new();
                        return beyond;
                    }
                    default:
                    {
                        return new();
                    }
                }
            }

            /// <summary>
            /// インスタンスがない場合、指定されたIDを登録したデータをつくる。
            /// </summary>
            /// <param name="id">ID</param>
            public SongSave(int id)
            {
                this.id = id;
            }
        }

        #endregion

        #region メソッド

        /// <summary>
        /// セーブデータの項目を増やす。
        /// </summary>
        /// <param name="addition">追加するセーブデータ</param>
        private SongSave Add(SongSave addition)
        {
            // 現時点でのセーブデータをリストにコピーする
            List<SongSave> data = saves.ToList();
            
            // 追加データをリストに追加する
            data.Add(addition);

            // IDについて昇順にデータを並び替える
            data.Sort((a, b) => a.id - b.id);

            // セーブデータに反映する
            saves = data.ToArray();

            return data[data.IndexOf(addition)];
        }

        /// <summary>
        /// 楽曲IDからセーブデータを検索する。
        /// </summary>
        /// <param name="id">楽曲ID</param>
        /// <returns>
        /// その楽曲のデータ、存在しなかった場合は新規作成したデータ
        /// </returns>
        public SongSave Explore(int id)
        {
            // 指定されたIDがセーブデータにあるか確認する
            // どのみちIDにつき１つしかデータがないので、初めに見つけたものを返す
            // データがなかったらとりあえず新しく作る
            return saves.FirstOrDefault(save => save.id == id) ?? Add(new(id));            
        }

        public override void Load()
        {
            // ファイルがあったとき
            if (File.Exists($"{Application.persistentDataPath}/Save.json"))
            {
                StreamReader streamReader = new($"{Application.persistentDataPath}/Save.json");
                string data = streamReader.ReadToEnd();
                streamReader.Close();
                Instance = JsonUtility.FromJson<SongSaveData>(data);
            }
            // ファイルがなかったとき、ファイルを作って読み込む（再帰）
            else
            {
                Save();
                Load();    
            }
        }

        public override void Save()
        {
            string json = JsonUtility.ToJson(Instance, true);
            StreamWriter streamWriter = new($"{Application.persistentDataPath}/Save.json");
            streamWriter.Write(json);
            streamWriter.Flush();
            streamWriter.Close();
        }

        #endregion
    }
}

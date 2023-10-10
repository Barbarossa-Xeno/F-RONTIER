using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FRONTIER.Utility;

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
            /// 難易度「HARD」のセーブデータ
            /// </summary>
            public Record hard;

            /// <summary>
            /// 難易度「ECSTASY」のセーブデータ
            /// </summary>
            public Record ecstasy;

            /// <summary>
            /// 難易度「RESTRICTED」のセーブデータ
            /// </summary>
            public Record restricted;

            /// <summary>
            /// 難易度ごとに記録を保持する。
            /// </summary>
            [System.Serializable]
            public class Record
            {
                public int highScore;
                public string highRank;
                public bool fullCombo;
                public bool allPerfect;

                /// <summary>
                /// 記録を上書きする。
                /// </summary>
                /// <param name="score">到達したハイスコア</param>
                /// <param name="rank">到達したハイランク</param>
                /// <param name="isGotfullCombo">フルコンボしたか</param>
                /// <param name="isGotAllPerfect">オールパーフェクトしたか</param>
                public void Overwrite(int score = -1, string rank = null, bool isGotfullCombo = false, bool isGotAllPerfect = false)
                {
                    // スコアが上がらなくてもフルコンボをするなどの場合があるため
                    // 初期値（ふつうでは取り得ない値）との比較で以てフィールドを上書きする
                    highScore = score != -1 ? score : highScore;
                    highRank = rank ?? highRank;
                    fullCombo = isGotfullCombo;
                    allPerfect = isGotAllPerfect;
                }
            }

            /// <summary>
            /// 指定された難易度に記録されたデータを返す。
            /// </summary>
            /// <param name="difficulty">難易度</param>
            /// <returns>難易度ごとのセーブデータ、存在しない場合は新規作成したデータ</returns>
            public Record DifficultyTo(Reference.DifficultyRank difficulty)
            {
                return difficulty switch
                {
                    Reference.DifficultyRank.Lite => lite ?? new(),
                    Reference.DifficultyRank.Hard => hard ?? new(),
                    Reference.DifficultyRank.Ecstasy => ecstasy ?? new(),
                    Reference.DifficultyRank.Restricted => restricted ?? new(),
                    _ => null
                };
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
            List<SongSave> datas = saves.ToList();
            
            // 追加データをリストに追加する
            datas.Add(addition);

            // IDについて昇順にデータを並び替える
            datas.Sort((a, b) => a.id - b.id);

            // セーブデータに反映する
            saves = datas.ToArray();

            return datas[datas.IndexOf(addition)];
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
            // Whereを使って探索するのでIEnumerableで返ってくるが
            // どのみちIDにつき１つしかデータがないので、配列の先頭要素を返す
            var datas = saves.Where(save => save.id == id);
            var data = datas.Count() > 0 ? datas.ElementAt(0) : null;

            // データがなかったらとりあえず新しく作る
            return data ?? Add(new(id));            
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
            // ファイルがなかったとき、データの配列を１つ確保する
            else { Instance.saves = new SongSave[1]; }
        }

        public override void Save()
        {
            string serialedData = JsonUtility.ToJson(Instance, true);
            StreamWriter streamWriter = new($"{Application.persistentDataPath}/Save.json");
            streamWriter.Write(serialedData);
            streamWriter.Close();
        }

        #endregion
    }
}
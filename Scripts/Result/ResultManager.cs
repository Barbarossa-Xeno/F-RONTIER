using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FadeTransition;
using Game.Utility;
using Game.Development;

///<summary>リザルトの管理を行うクラスです。</summary>
public class ResultManager : MonoBehaviour
{
    /* フィールド */
    ///<summary>スコアを表示するオブジェクトをまとめたクラス。</summary>
    [System.Serializable]
    private class ScoreElements
    {
        ///<summary>スコアを数値で表示する。</summary>
        public TextMeshProUGUI scoreText = default;
        ///<summary>ハイスコアとの差分を表示する。</summary>
        public TextMeshProUGUI differenceHighScore = default;
        ///<summary>フルコンボのときの演出。</summary>
        public GameObject fullCombo = default;
        ///<summary>オールパーフェクトのときの演出。</summary>
        public GameObject allPerfect = default;
        ///<summary>ニューレコードを記録した時の演出。</summary>
        public GameObject newRecord = default;
    }
    ///<summary>詳細なスコア（判定ステータス）を表示するオブジェクトをまとめたクラス。</summary>
    [System.Serializable]
    private class DetailElements
    {
        public TextMeshProUGUI perfect;
        public TextMeshProUGUI great;
        public TextMeshProUGUI good;
        public TextMeshProUGUI bad;
        public TextMeshProUGUI miss;
    }
    ///<summary>楽曲に所縁のあるキャラクターを表示させるためのオブジェクトをまとめたクラス。</summary>
    [System.Serializable]
    private class CharacterElements
    {
        ///<summary>キャラクターを表示する。</summary>
        public Image character;
        ///<summary>キャラの台詞。</summary>
        public TextMeshProUGUI dialog;
    }
    ///<summary>スコアを保存するクラス。</summary>
    [System.Serializable]
    private class SongSaveData
    {
        public Song song;

        [System.Serializable]
        public class Song
        {
            public int highScore;
            public int[] highStatus;
            public string highRank;
            public bool fc;
            public bool ap;
        }
    }
    [SerializeField] private ScoreElements scoreElements;
    ///<summary>ランクのロゴをセットするイメージ。</summary>
    [SerializeField] private Image rankLogoField;
    [SerializeField] private DetailElements detailElements;
    [SerializeField] private CharacterElements characterElements;
    ///<summary>スライドインさせるマスク。</summary>
    [SerializeField] private GameObject mask;
    ///<summary>選曲画面へ戻るボタン。</summary>
    [SerializeField] private Button continueButton;
    ///<summary>楽曲のID。</summary>
    private int songID;
    ///<summary>スコア。</summary>
    private int score;
    ///<summary>ランク。</summary>
    private string rank;
    ///<summary>判定ステータスの結果と総数を格納する辞書。</summary>
    private Dictionary<string, int> detailValueCount = new Dictionary<string, int>();
    ///<summary>ランクのスプライト。</summary>
    private Sprite rankSprite;
    ///<summary>キャラのスプライト。</summary>
    private Sprite charaSprite;
    ///<summary><see cref = "SongSaveData"/>のインスタンス。</summary>
    private SongSaveData songSaveData = new SongSaveData();
    ///<summary>アチーブメントの達成状況。</summary>
    private enum ComboAchivement
    {
        None, FullCombo, AllPerfect
    }
    ///<summary>アチーブメントの現在の達成状況。</summary>
    private ComboAchivement comboAchivement = ComboAchivement.None;

    /* メソッド */

    void Awake()
    {
        //マスクをアクティブにする。
        mask.SetActive(true);
        //リザルトの値を受け取る。
        songID = GameManager.instance.songID;
        score = GameManager.instance.scoreManager.score;
        DevelopmentExtentionMethods.Log($"{score}, {GameManager.instance.scoreManager.score}");
        detailValueCount = GameManager.instance.scoreManager.scoreCount;
        //アクティブの設定。
        scoreElements.fullCombo.gameObject.SetActive(scoreElements.fullCombo.gameObject.activeSelf ? false : false);
        scoreElements.allPerfect.gameObject.SetActive(scoreElements.allPerfect.gameObject.activeSelf ? false : false);
        scoreElements.newRecord.gameObject.SetActive(scoreElements.newRecord.gameObject.activeSelf ? false : false);
    }

    void Start()
    {
        //詳細ゾーンのテキストをセット。
        scoreElements.scoreText.SetText("{0}", score);
        detailElements.perfect.SetText("{0}", detailValueCount["perfect"]);
        detailElements.great.SetText("{0}", detailValueCount["great"]);
        detailElements.good.SetText("{0}", detailValueCount["good"]);
        detailElements.bad.SetText("{0}", detailValueCount["bad"]);
        detailElements.miss.SetText("{0}", detailValueCount["miss"]);
        //ランクに応じたスプライトを取得してセット。
        rank = SelectRank(score);
        rankSprite = (Sprite)Resources.Load<Sprite>($"Images/Result/rank_{rank}");
        rankLogoField.sprite = rankSprite;
        //楽曲のリザルトデータを読み込み、アチーブメントに基づいた演出をする。
        comboAchivement = CompareRecord(songSaveData);
        if(comboAchivement == ComboAchivement.FullCombo || comboAchivement == ComboAchivement.AllPerfect)
        {
            StartCoroutine(Achived(scoreElements.fullCombo));
            if(comboAchivement == ComboAchivement.AllPerfect)
            {
                StartCoroutine(Achived(scoreElements.allPerfect));
            }
        }
        //アニメーションの再生と、データのセーブ。
        this.GetComponent<Animator>().SetTrigger("Start");
        SaveRecord(songSaveData);
    }
    
    ///<summary>楽曲のハイスコアを読み込み、直前のプレイデータと比較します。スコア記録がない場合は、新規で作成します。</summary>
    ///<returns>コンボアチーブメントの状況。</returns>
    ///<param name = "data">セーブデータを読み込ませるクラスインスタンス。</param>
    private ComboAchivement CompareRecord(SongSaveData data)
    {
        ComboAchivement state = 0;
        data = null;
        if(!Directory.Exists($"{Application.persistentDataPath}/songData"))
        {
            Directory.CreateDirectory($"{Application.persistentDataPath}/songData");
        }

        if(File.Exists($"{Application.persistentDataPath}/songData/{songID}.json"))
        {
            StreamReader streamReader = new StreamReader($"{Application.persistentDataPath}/songData/{songID}.json");
            string loadData = streamReader.ReadToEnd();
            streamReader.Close();
            data = JsonUtility.FromJson<SongSaveData>(loadData);
            DevelopmentExtentionMethods.Log("楽曲セーブデータの読込に成功しました。");
        }
        else { DevelopmentExtentionMethods.Log("データが見つからなかったので新規データを保存します。"); }

        if(detailValueCount["bad"] == 0 && detailValueCount["miss"] == 0)
        {
            state = ComboAchivement.FullCombo;
            if(detailValueCount["great"] == 0 && detailValueCount["good"] == 0)
            {
                state = ComboAchivement.AllPerfect;
            }
        }
        //データを上書きする場合
        if(data != null)
        {
            int difference = Mathf.Abs(score - data.song.highScore);
            if(difference >= 0) { scoreElements.differenceHighScore.SetText($"+{difference}"); }
            else { scoreElements.differenceHighScore.SetText($"-{difference}"); }

            if(score > data.song.highScore)
            {
                scoreElements.newRecord.SetActive(true);
                data.song.highScore = score;
                data.song.highRank = rank;
                data.song.highStatus = detailValueCount.Values.ToArray();
                if(data.song.fc == false && state == ComboAchivement.FullCombo) { data.song.fc = true; }
                if(data.song.ap == false && state == ComboAchivement.AllPerfect) { data.song.ap = true; }
            }
        }
        //新しくデータを作る場合
        else
        {
            scoreElements.differenceHighScore.SetText($"+{score}");
            scoreElements.newRecord.SetActive(true);
            data = new SongSaveData();
            data.song = new SongSaveData.Song();
            data.song.highScore = score;
            data.song.highRank = rank;
            data.song.highStatus = detailValueCount.Values.ToArray();
            data.song.fc = ((state == ComboAchivement.FullCombo) || (state == ComboAchivement.AllPerfect)) ? true : false;
            data.song.ap = state == ComboAchivement.AllPerfect ? true : false;
            songSaveData = data;
        }
        return state;
    }
    ///<summary>プレイ記録の上書きをします。</summary>
    ///<typeparam name = "Data">保存するクラス。</typeparam>
    ///<param name = "data">クラスのインスタンス。</param>
    private void SaveRecord<Data>(Data data) where Data : class
    {
        string serialedSaveData = JsonUtility.ToJson(data, true);
        StreamWriter streamWriter = new StreamWriter($"{Application.persistentDataPath}/songData/{songID}.json");
        streamWriter.Write(serialedSaveData);
        streamWriter.Flush();
        streamWriter.Close();
        DevelopmentExtentionMethods.Log($"{songID}.jsonを保存しました。");
    }
    ///<summary>獲得したアチーブメントに応じてゲームオブジェクトの演出をします。</summary>
    ///<param name = "gameObject">演出（アクティブ）させるオブジェクト。</param>
    private IEnumerator Achived(GameObject gameObject)
    {
        yield return new WaitForSeconds(0.5f);
        gameObject.SetActive(true);
    }
    ///<summary>スコアに応じてランク評価を振り分けます。</summary>
    private string SelectRank(int _score)
    {
        if (_score >= (int)SettingUtility.RankBorder.S_plus) { return "s+"; }
        else if (_score >= (int)SettingUtility.RankBorder.S && _score < (int)SettingUtility.RankBorder.S_plus) { return "s"; }
        else if (_score >= (int)SettingUtility.RankBorder.A_plus && _score < (int)SettingUtility.RankBorder.S) { return "a+"; }
        else if (_score >= (int)SettingUtility.RankBorder.A && _score < (int)SettingUtility.RankBorder.A_plus) { return "a"; }
        else if (_score >= (int)SettingUtility.RankBorder.B_plus && _score < (int)SettingUtility.RankBorder.A) { return "b+"; }
        else if (_score >= (int)SettingUtility.RankBorder.B && _score < (int)SettingUtility.RankBorder.B_plus) { return "b"; }
        else if (_score >= (int)SettingUtility.RankBorder.C_plus && _score < (int)SettingUtility.RankBorder.B) { return "c+"; }
        else if (_score < (int)SettingUtility.RankBorder.C_plus) { return "c"; }
        else { return ""; }
    }

    public void ContinueButtonPressed()
    {
        SceneNavigator.instance.SceneChange("Menu", 1.5f);
    }
}



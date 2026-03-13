using System;
using UnityEngine;
using FRONTIER.Utility;

namespace FRONTIER.Game.Notes
{
    /// <summary>
    /// ノーツの情報を保持し、ノーツを動かすためのクラス。ノーツ1つにつきこのクラス1つ。
    /// </summary>
    public class Note : GameUtilityBase
    {
        #region フィールド

        /// <summary>
        /// このノーツの種類（インスペクタ確認用）。
        /// </summary>
        [SerializeField] private Reference.NoteType type;

        /// <summary>
        /// このノーツが流れてくる順番。
        /// </summary>
        [SerializeField] private int index;

        /// <summary>
        /// このノーツが含まれているリスト (<see cref="NotesManagerBase.instances"/> ) でのインデックス。
        /// 最終的に、先に判定線に到達する方が番号が大きくなるような番号付けがされている。
        /// </summary>
        [SerializeField] private int noteIndex;

        /// <summary>
        /// このノーツが到達する時間。
        /// </summary>
        [SerializeField] private float reachedTime;

        /// <summary>
        /// このノーツが配置されているレーンのインデックス。
        /// </summary>
        [SerializeField] private int laneIndex;

        /// <summary>
        /// このノーツが判定ラインを超過したときに発火するイベント。<br/>
        /// </summary>
        /// <remarks>
        /// <see cref="GameManager.PlayInfo.IsAutoPlay"/> が
        /// <b><c>true</c> の時: 判定線から少し離れた位置 (<see cref="Reference.missJudgementPosition.z"/>) で発火</b><br/>
        /// <b><c>false</c> の時: ノーツの判定線到達時間が経過すると発火</b><br/>
        /// </remarks>
        public event Action<Note> ReachedLineEvent;

        /// <summary>
        /// 判定線を超過したか。
        /// </summary>
        [SerializeField] protected bool isReachedLine = false;

        #endregion

        #region プロパティ

        /// <summary>
        /// このノーツの種類。
        /// </summary>
        public Reference.NoteType Type
        {
            get => type;
            set => type = value;
        }

        /// <summary>
        /// このノーツが流れてくる順番。
        /// </summary>
        public int Index
        {
            get => index;
            set => index = value;
        }

        /// <summary>
        /// このノーツが含まれているリスト (<see cref="NotesManagerBase.instances"/>) でのインデックス。
        /// 最終的に、先に判定線に到達する方が番号が大きくなるような番号付けがされている。
        /// </summary>
        public int NoteIndex
        {
            get => noteIndex;
            set => noteIndex = value;
        }

        /// <summary>
        /// このノーツが到達する時間。
        /// </summary>
        public float ReachedTime
        {
            get => reachedTime;
            set => reachedTime = value;
        }

        /// <summary>
        /// このノーツが配置されているレーンのインデックス。
        /// </summary>
        public int LaneIndex
        {
            get => laneIndex;
            set => laneIndex = value;
        }

        #endregion

        #region MonoBehaviorメソッド

        // MonoBehaviorメソッドはオーバーライドできるようにする
        protected virtual void Start() { }

        protected virtual void Update()
        {
            // ゲームプレイ中に実行される
            if (Manager.gamePlayState == GameManager.GamePlayState.Playing)
            {
                // Z座標を移動させる
                transform.position -= new Vector3(0, 0, Manager.info.NoteSpeed) * Time.deltaTime;

                // 判定線を超過したかチェック
                if (!isReachedLine
                    // 通常プレイ: 画面外
                    && ((!Manager.info.IsAutoPlay && transform.position.z <= Reference.missJudgementPosition.z)
                        // オートプレイ: 到達時間で判定することで、ノーツの速さごとに毎フレームの変位が異なって実際の判定とギャップができるのを防ぐ
                        || (Manager.info.IsAutoPlay && Time.time - Manager.startTime >= reachedTime)))
                {
                    isReachedLine = true;
                    ReachedLineEvent?.Invoke(this);
                }
            }
        }

        #endregion

        #region メソッド

        /// <summary>
        /// 各プロパティに値を設定する。
        /// </summary>
        /// <param name="type">ノーツの種類</param>
        /// <param name="index">順番</param>
        public void SetProperties(Reference.NoteType type, int index)
        {
            this.Type = type;
            this.index = index;
        }

        #endregion
    }
}

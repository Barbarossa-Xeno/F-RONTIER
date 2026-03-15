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
        /// ノーツの種類ごとにカウントされる。
        /// </summary>
        [SerializeField] protected Reference.NoteType type;

        /// <summary>
        /// このノーツが流れてくる順番。
        /// </summary>
        [SerializeField] protected int arrivalOrder;

        /// <summary>
        /// このノーツが含まれているリスト (<see cref="NotesManagerBase.instances"/> ) でのインデックス。
        /// 最終的に、先に判定線に到達する方が番号が大きくなるような番号付けがされている。
        /// </summary>
        [SerializeField] protected int noteIndex;

        /// <summary>
        /// このノーツが到達する時間。
        /// </summary>
        [SerializeField] protected float reachedTime;

        /// <summary>
        /// このノーツが配置されているレーンのインデックス。
        /// </summary>
        [SerializeField] protected int laneIndex;

        /// <summary>
        /// このノーツが判定線ちょうどに到達したときに発火するイベント。<br/>
        /// </summary>
        /// <remarks>
        /// ノーツの判定線到達時間が経過すると発火
        /// </remarks>
        public event Action ReachedLine;

        /// <summary>
        /// このノーツが判定線を超過したときに発火するイベント。<br/>
        /// </summary>
        /// <remarks>
        /// 判定線から少し離れた位置 (<see cref="Reference.missJudgementPosition.z"/>) で発火
        /// </remarks>

        public event Action PassedOverLine;

        /// <summary>
        /// 判定線に到達したか。
        /// </summary>
        [SerializeField] protected bool isReachedLine = false;

        /// <summary>
        /// 判定線を超過したか。
        /// </summary>
        [SerializeField] protected bool isPassedOverLine = false;

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
        /// ノーツの種類ごとにカウントされるので通常ノーツとロングノーツで別々の順番になる。
        /// </summary>
        public int ArrivalOrder
        {
            get => arrivalOrder;
            set => arrivalOrder = value;
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

                OnReachedLine();
                OnPassedOverLine();
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
            this.arrivalOrder = index;
        }

        /// <summary>
        /// ノーツが判定線を超過したときの処理。イベント <see cref="ReachedLine"/> を発火させる。
        /// </summary>
        protected virtual void OnReachedLine()
        {
            // このノーツの到達時間に達したかチェック
            // 到達時間で判定することで、ノーツの速さごとに毎フレームの変位が異なって実際の判定とギャップができるのを防ぐ
            if (!isReachedLine && Time.time - Manager.startTime >= reachedTime)
            {
                isReachedLine = true;
                ReachedLine?.Invoke();
            }
        }

        protected virtual void OnPassedOverLine()
        {
             // 画面外に出ていったかチェック
            if (!isPassedOverLine && transform.position.z <= Reference.missJudgementPosition.z)
            {
                isPassedOverLine = true;
                PassedOverLine?.Invoke();
            }
        }

        /// <summary>
        /// 子クラスで <see cref="ReachedLine"/> を発火させるときに呼び出すメソッド。
        /// </summary>
        protected virtual void InvokeReachedLine() => ReachedLine?.Invoke();

        #endregion
    }
}

using System;
using UnityEngine;
using FRONTIER.Utility;

namespace FRONTIER.Game.NotesManagement
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
        public int index;

        /// <summary>
        /// このノーツが含まれているリスト (<see cref="NotesManager.notesObjects"/> ) でのインデックス。生成順
        /// </summary>
        public int noteIndex;

        /// <summary>
        /// このノーツが判定ラインを超過したときに発火するイベント。
        /// </summary>
        public event Action ReachedLineEvent;

        /// <summary>
        /// 判定線を超過したか。
        /// </summary>
        protected bool isReachedLine = false;

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
                if (!isReachedLine &&
                    // 通常プレイ: 画面外
                    ((!Manager.info.IsAutoPlay && transform.position.z <= Reference.noteOrigin.z - 3.5f) ||
                    // オートプレイ: 判定線通過
                     (Manager.info.IsAutoPlay && transform.position.z <= Reference.noteOrigin.z)))
                {
                    isReachedLine = true;
                    ReachedLineEvent?.Invoke();
                }
            }
        }

        #endregion

        #region メソッド

        /// <summary>
        /// 情報フィールドに値を設定する。
        /// </summary>
        /// <param name="type">ノーツの種類</param>
        /// <param name="index">順番</param>
        public void SetInfo(Reference.NoteType type, int index)
        {
            this.Type = type;
            this.index = index;
        }

        #endregion
    }
}

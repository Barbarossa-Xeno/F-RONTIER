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
        /// このノーツの種類。
        /// </summary>
        [SerializeField] private Reference.NoteType type;

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
        /// このノーツが含まれているリストでのインデックス。
        /// </summary>
        /// <remarks>
        /// リスト => <see cref="NotesManager.notesObjects"/> 
        /// </remarks>
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
                transform.position -= new Vector3(0, 0, Manager.info.NoteSpeed) * Time.deltaTime;

                // 通常プレイ時、画面外に行くとミス判定
                if (!Manager.info.IsAutoPlay && transform.position.z <= Reference.noteOrigin.z - 3.5f && !isReachedLine)
                {
                    isReachedLine = true;
                    ReachedLineEvent?.Invoke();
                }
                // オートプレイ時、判定線通過で判定
                else if (Manager.info.IsAutoPlay && transform.position.z <= Reference.noteOrigin.z && !isReachedLine)
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

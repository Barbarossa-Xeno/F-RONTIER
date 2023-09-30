using System;
using UnityEngine;
using FRONTIER.Utility;

namespace FRONTIER.Game.NotesManagement
{
    /// <summary>
    /// ノーツの情報を保持し、ノーツを動かす。
    /// </summary>
    public class Notes : MonoBehaviour
    {
        #region フィールド

        /// <summary>
        /// このノーツの種類。
        /// </summary>
        public Reference.NoteType type;

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
        public int indexOfList;

        /// <summary>
        /// このノーツが判定ラインを超過したときに発火するイベント。
        /// </summary>
        public event Action OnReachedJudgement;

        /// <summary>
        /// 判定線を超過したか。
        /// </summary>
        protected bool IsReachedJudgement = false;

        #endregion

        #region MonoBehaviorメソッド

        // MonoBehaviorメソッドはオーバーライドできるようにする
        protected virtual void Start() { }

        protected virtual void Update()
        {
            // ゲームプレイ中に実行される
            if (GameManager.instance.gamePlayState == GameManager.GamePlayState.Playing)
            {
                transform.position -= new Vector3(0, 0, GameManager.instance.NoteSpeed) * Time.deltaTime;

                // 通常プレイ時、画面外に行くとミス判定
                if (!GameManager.instance.AutoPlay && transform.position.z <= Reference.noteOrigin.z - 3.5f && !IsReachedJudgement)
                {
                    IsReachedJudgement = true;
                    OnReachedJudgement?.Invoke();
                }
                // オートプレイ時、判定線通過で判定
                else if (GameManager.instance.AutoPlay && transform.position.z <= Reference.noteOrigin.z && !IsReachedJudgement)
                {
                    IsReachedJudgement = true;
                    OnReachedJudgement?.Invoke();
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
            this.type = type;
            this.index = index;
        }

        #endregion
    }
}
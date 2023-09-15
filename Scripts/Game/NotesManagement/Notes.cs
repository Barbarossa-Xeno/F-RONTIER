using System.Collections;
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

        #endregion

        #region MonoBehaviorメソッド

        void Update()
        {
            if (GameManager.instance.gamePlayState == GameManager.GamePlayState.Playing) { transform.position -= new Vector3(0, 0, GameManager.instance.NoteSpeed) * Time.deltaTime; }
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
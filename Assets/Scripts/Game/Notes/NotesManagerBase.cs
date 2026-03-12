using System;
using System.Collections.Generic;
using FRONTIER.Utility;
using UnityEngine;

namespace FRONTIER.Game.Notes
{
    /// <summary>
    /// ノーツを管理するための抽象クラス。
    /// </summary>
    /// <typeparam name="TLaneIndexes">
    /// リスト <c>laneIndexes</c> の型。基本は<c>int</c><br/>
    /// 通常ノーツを収めるなら1次元を、ロングノーツを収めるなら2次元のリストになるようにする
    /// </typeparam>
    /// <typeparam name="TReachedTimes">
    /// リスト <c>reachedTimes</c> の型。基本は<c>float</c><br/>
    /// 通常ノーツを収めるなら1次元を、ロングノーツを収めるなら2次元のリストになるようにする
    /// </typeparam>
    /// <typeparam name="TInstances">
    /// リスト <c>instances</c> の型。基本は<c><see cref="Note"/></c><br/>
    /// 通常ノーツを収めるなら1次元を、ロングノーツを収めるなら2次元のリストになるようにする
    /// </typeparam>
    [Serializable]
    public abstract class NotesManagerBase<TLaneIndexes, TReachedTimes, TInstances> : GameUtilityBase
    {
        #region フィールド

        /// <summary>
        /// ゲームオブジェクトとして生成したノーツの親オブジェクトとする。
        /// </summary>
        [SerializeField] protected Transform instanceParent;

        /// <summary>
        /// 各ノーツが流れるレーン番号を格納する。
        /// </summary>
        /// <remarks>
        /// ノーマルノーツの場合：<c>int</c>型リスト。順にレーン番号を格納<br/>
        /// ロングノーツの場合：<c>List(int)</c>型リスト（二次元リスト）。ロングノーツ１まとまり毎にその中間点ノーツのレーン番号を格納
        /// </remarks>
        public List<TLaneIndexes> laneIndexes = new();

        /// <summary>
        /// 各ノーツの楽曲が判定線に接触する時間を格納する。
        /// </summary>
        /// <remarks>
        /// ノーマルノーツの場合：<c>float</c>型リスト。順に時間を格納<br/>
        /// ロングノーツの場合：<c>List(float)</c>型リスト（二次元リスト）。ロングノーツ１まとまり毎にその中間点ノーツの時間を格納
        /// </remarks>
        public List<TReachedTimes> reachedTimes = new();

        /// <summary>
        /// 各ノーツの種類を <see cref="Reference.NoteType"/> を <c>int</c> に変換した値で格納する。（インスペクタ確認用）
        /// 種類の仕分けに利用する
        /// </summary>
        public List<int> types = new();

        /// <summary>
        /// 各ノーツを<c>GameObject</c>として生成したインスタンスを格納する。
        /// </summary>
        /// <remarks>
        /// ノーマルノーツの場合：<c>GameObject</c>型リスト。順にオブジェクトを格納<br/>
        /// ロングノーツの場合：<c>List(GameObject)</c>型リスト（二次元リスト）。ロングノーツ１まとまり毎にその中間点ノーツのオブジェクトを格納
        /// </remarks>
        public List<TInstances> instances = new();

        #endregion

        #region プロパティ

        /// <summary>
        /// ゲームの準備に際して必要な情報。
        /// </summary>
        public GameManager.PlayInfo PlayInfo => Manager.info;

        #endregion

        #region メソッド

        /// <summary>
        /// プレイする楽曲の譜面データをロードする。
        /// </summary>
        /// <returns></returns>
        protected NotePatternData LoadNotePattern(int id, string difficulty)
            => JsonUtility.FromJson<NotePatternData>(Resources.Load<TextAsset>($"Data/{id}/{difficulty}").ToString());

        /// <summary>
        /// ノーツを生成する。
        /// </summary>
        public abstract void GenerateNotes();

        /// <summary>
        /// ノーツのリスト等をソート・整理する。
        /// </summary>
        public abstract void SortNotes();

        public abstract bool DeleteNote(TInstances target);

        #endregion

        #region クラス

        [Serializable]
        protected class NotePatternData
        {
            public string name;
            public int maxBlock;
            public int BPM;
            public int offset;
            public NoteData[] notes;

            [Serializable]
            public class NoteData
            {
                public int type;
                public int num;
                public int block;
                public int LPB;
                public LongNoteData[] notes;

                [Serializable]
                public class LongNoteData
                {
                    public int type;
                    public int num;
                    public int block;
                    public int LPB;
                }
            }
        }

        #endregion
    }
}

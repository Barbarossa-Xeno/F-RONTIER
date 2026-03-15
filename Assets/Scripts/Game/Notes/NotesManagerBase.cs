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
    /// <typeparam name="TNote">
    /// リスト <c>instances</c> に収めるノーツの型。<br/>
    /// 通常ノーツを収めるなら<c><see cref="Note"/></c>を、ロングノーツを収めるなら<c><see cref="LongNote"/></c>にする
    /// </typeparam>
    [Serializable]
    public abstract class NotesManagerBase<TLaneIndexes, TReachedTimes, TNote> : GameUtilityBase
        where TNote : Note
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
        [SerializeField] protected List<TLaneIndexes> laneIndexes = new();

        /// <summary>
        /// 各ノーツの楽曲が判定線に接触する時間を格納する。
        /// </summary>
        /// <remarks>
        /// ノーマルノーツの場合：<c>float</c>型リスト。順に時間を格納<br/>
        /// ロングノーツの場合：<c>List(float)</c>型リスト（二次元リスト）。ロングノーツ１まとまり毎にその中間点ノーツの時間を格納
        /// </remarks>
        [SerializeField] protected List<TReachedTimes> reachedTimes = new();

        /// <summary>
        /// 各ノーツの種類を <see cref="Reference.NoteType"/> を <c>int</c> に変換した値で格納する。（インスペクタ確認用）
        /// 種類の仕分けに利用する
        /// </summary>
        [SerializeField] protected List<Reference.NoteType> types = new();

        /// <summary>
        /// 各ノーツを<c>GameObject</c>として生成し、<c>TNote</c>を付与したインスタンスを格納する。
        /// </summary>
        /// <remarks>
        /// 最終的にこのクラスで生成したノーツが到達時間の降順に格納されるようにする。
        /// </remarks>
        [SerializeField] protected List<TNote> instances = new();

        #endregion

        #region プロパティ

        /// <summary>
        /// 各ノーツを<c>GameObject</c>として生成し、<c>TNote</c>を付与したインスタンスを格納する。
        /// </summary>
        /// <remarks>
        /// 最終的にこのクラスで生成したノーツが到達時間の降順に格納されるようにする。
        /// </remarks>
        public List<TNote> Instances => instances;

        /// <summary>
        /// ゲームの準備に際して必要な情報。
        /// </summary>
        protected GameManager.PlayInfo PlayInfo => Manager.info;

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

        /// <summary>
        /// 指定されたノーツを削除する。
        /// 削除に成功した場合は<c>true</c>、失敗した場合は<c>false</c>を返す。<br/>
        /// </summary>
        /// <remarks>
        /// - GameObject は非アクティブ化<br/>
        /// - TNote型のインスタンスは関連情報を各リストから削除
        /// </remarks>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool DeleteNote(TNote target)
        {
            if (instances.Contains(target))
            {
                target.gameObject.SetActive(false);

                // Miss 等の理由でリストから削除したタイミングが前後する場合があるので
                // 現在のインデックスを取得するのが安全
                int index = instances.IndexOf(target);
                
                instances.RemoveAt(index);

                return true;
            }
            else
            {
                return false;
            }
        }

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

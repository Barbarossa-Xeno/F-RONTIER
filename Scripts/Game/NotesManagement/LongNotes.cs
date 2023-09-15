using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using FRONTIER.Utility;

namespace FRONTIER.Game.NotesManagement
{
    /// <summary>
    /// ロングノーツの情報を保持し、押下の判定を制御する。
    /// </summary>
    public class LongNotes : Notes, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        #region フィールド

        /// <summary>
        /// ロングノーツとしてのステータス。
        /// </summary>
        public Reference.LongNoteStatus status = default;

        /// <summary>
        /// ロングノーツの種類。
        /// </summary>
        public Reference.LongNoteType longNoteType = default;

        /// <summary>
        /// このロングノーツが中間点を持つか。
        /// </summary>
        public bool isInner;

        /// <summary>
        /// ロングノーツが押下されているか。
        /// </summary>
        public bool isPressed = false;

        /// <summary>
        /// ロングノーツが押下されているときに発火するイベント。
        /// </summary>
        private event Action<bool> OnPressed;

        /// <summary>
        /// 直線型ロングノーツのマテリアル。（直線なので一個だけ）
        /// </summary>
        private MeshRenderer linearTypeLongNoteMesh;

        /// <summary>
        /// 曲線型ロングノーツで必要なコンポーネントなどを取得するインスタンス。
        /// </summary>
        private CurveTypeComponent curveTypeComponent;

        #endregion

        #region クラス

        /// <summary>
        /// 曲線型ロングノーツのコンポーネントなど。
        /// </summary>
        private class CurveTypeComponent
        {
            /// <summary>
            /// ロングノーツの分割された断片のゲームオブジェクト。
            /// </summary>
            private GameObject[] fragmentLongNotes;

            /// <summary>
            /// 曲線型ロングノーツのマテリアル（曲線なので複数）
            /// </summary>
            public List<MeshRenderer> longNoteMeshes;
            
            /// <summary>
            /// 曲線型ロングノーツの中心
            /// </summary>
            public List<LineRenderer> longNoteLines;

            public CurveTypeComponent(Transform longNotesTransform, bool isInner)
            {
                fragmentLongNotes = new GameObject[longNotesTransform.childCount];
                longNoteMeshes = new(longNotesTransform.childCount);
                longNoteLines = isInner ? new(longNotesTransform.childCount) : null;
                GetComponents(longNotesTransform);
            }

            /// <summary>
            /// 各配列に取得したコンポーネントを代入する。
            /// </summary>
            /// <param name="parent">親のトランスフォーム（ロングノーツ１まとまり）</param>
            private void GetComponents(Transform parent)
            {
                fragmentLongNotes = Enumerable.Range(0, fragmentLongNotes.Length).Select(i => parent.GetChild(i).gameObject).ToArray();
                longNoteMeshes = Enumerable.Range(0, longNoteMeshes.Capacity).Select(i => fragmentLongNotes[i].GetComponent<MeshRenderer>()).ToList();
                longNoteLines = longNoteLines != null
                                ? Enumerable.Range(0, longNoteLines.Capacity).Select(i => fragmentLongNotes[i].GetComponent<LineRenderer>()).ToList()
                                : null;
            }
        }

        private static class ShaderParameter
        {
            /// <summary>
            /// シェーダーのプロパティID
            /// </summary>
            public static readonly int _isPressed = Shader.PropertyToID("_isPressed");

            /// <summary>
            /// bool型をint型の数値に対応させて返す。
            /// </summary>
            /// <param name="flag">真偽値</param>
            /// <returns>
            /// <c>true</c> => 1<br/>
            /// <c>false</c> => 0
            /// </returns>
            public static int SetFlag(bool flag) => flag ? 1 : 0;
        }

        #endregion

        #region MonoBehaviorメソッド

        void Start()
        {
            // イベントの登録
            OnPressed += isOn => isPressed = isOn;

            if (longNoteType == Reference.LongNoteType.NoInnerLinear || longNoteType == Reference.LongNoteType.NoInnerCurve)
            {
                // 中間点がないLノーツの場合、自分のレンダラーを取得してマテリアルを設定する
                linearTypeLongNoteMesh = GetComponent<MeshRenderer>();
                OnPressed += isOn => linearTypeLongNoteMesh.material.SetFloat(ShaderParameter._isPressed, ShaderParameter.SetFlag(isOn));
            }
            else if (longNoteType == Reference.LongNoteType.AnyInnerLinear || longNoteType == Reference.LongNoteType.AnyInnerCurve)
            {
                // 中間点があるLノーツの場合、各種コンポーネントを取得してマテリアルを設定する
                curveTypeComponent = new(transform, isInner);
                OnPressed += isOn =>
                {
                    curveTypeComponent.longNoteMeshes.ForEach(fragment => fragment.material.SetFloat(ShaderParameter._isPressed, ShaderParameter.SetFlag(isOn)));
                    curveTypeComponent.longNoteLines.ForEach(centerLine => centerLine.material.SetFloat(ShaderParameter._isPressed, ShaderParameter.SetFlag(isOn)));
                };
            }

            // オート時はずっと押された判定にする
            if (GameManager.instance.AutoPlay) { OnPressed.Invoke(true); }
        }

        #endregion

        #region メソッド

        /// <summary>
        /// ロングノーツが押下されたときに行う処理。
        /// </summary>
        /// <param name = "flag">押下の有無のフラグ。</param>
        private void Pressing(bool flag)
        {
            if (!GameManager.instance.AutoPlay) { OnPressed.Invoke(flag); }
            else { OnPressed.Invoke(true); }
        }

        /// <summary>
        /// ロングノーツの情報フィールドを設定する。
        /// </summary>
        /// <param name="noteType">ノーツの種類</param>
        /// <param name="index">順番</param>
        /// <param name="status">ロングノーツのステータス</param>
        /// <param name="isInner">中間点があるか</param>
        public void SetInfo(Reference.NoteType noteType, int index, Reference.LongNoteStatus status, bool isInner)
        {
            SetInfo(noteType, index);
            this.status = status;
            this.isInner = isInner;
            if (isInner)
            {
                if (noteType == Reference.NoteType.LongLinear) { longNoteType = Reference.LongNoteType.AnyInnerLinear; }
                else if (noteType == Reference.NoteType.LongCurve) { longNoteType = Reference.LongNoteType.AnyInnerCurve; }
            }
            else
            {
                if (noteType == Reference.NoteType.LongLinear) { longNoteType = Reference.LongNoteType.NoInnerLinear; }
                else if (noteType == Reference.NoteType.LongCurve) { longNoteType = Reference.LongNoteType.NoInnerCurve; }
            }
        }

        public void OnPointerDown(PointerEventData pointerDownEvent) => Pressing(true);
        public void OnPointerEnter(PointerEventData pointerEnterEvent) => Pressing(true);
        public void OnPointerUp(PointerEventData pointerUpEvent) => Pressing(false);
        public void OnPointerExit(PointerEventData pointerExitEvent) => Pressing(false);

        #endregion
    }
}
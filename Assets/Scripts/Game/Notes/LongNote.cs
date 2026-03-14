using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using FRONTIER.Utility;

namespace FRONTIER.Game.Notes
{
    /// <summary>
    /// ロングノーツの情報を保持し、押下の判定を制御する。
    /// </summary>
    public class LongNote : Note, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        #region フィールド

        /// <summary>
        /// ロングノーツのどの部分にあたるか。
        /// </summary>
        [SerializeField] private Reference.LongNotePart part = default;

        /// <summary>
        /// ロングノーツの種類。
        /// </summary>
        [SerializeField] private Reference.LongNoteType longNoteType = default;

        /// <summary>
        /// このロングノーツが中間点であるか。
        /// </summary>
        [SerializeField] private bool isIntermediate;

        /// <summary>
        /// ロングノーツが押下されているか。
        /// </summary>
        [SerializeField] private bool isPressed = false;

        /// <summary>
        /// ロングノーツが押下されているときに発火するイベント。
        /// </summary>
        public event Action<bool> Pressed;

        /// <summary>
        /// ロングノーツの押下の状態に応じて、毎フレーム発火するイベント。
        /// </summary>
        public event Action<bool> OnPressedUpdate;

        /// <summary>
        /// 直線型ロングノーツのマテリアル。（直線なので一個だけ）
        /// </summary>
        private MeshRenderer linearTypeLongNoteMesh;

        /// <summary>
        /// 曲線型ロングノーツで必要なコンポーネントなどを取得するインスタンス。
        /// </summary>
        private CurveTypeComponent curveTypeComponent;

        #endregion

        #region プロパティ

        /// <summary>
        /// ロングノーツのどの部分にあたるか。
        /// </summary>
        public Reference.LongNotePart Part
        {
            get => part;
            set => part = value;
        }

        /// <summary>
        /// ロングノーツの種類。
        /// </summary>
        public Reference.LongNoteType LongNoteType
        {
            get => longNoteType;
            set => longNoteType = value;
        }

        /// <summary>
        /// このロングノーツが中間点であるか。
        /// </summary>
        public bool IsIntermediate
        {
            get => isIntermediate;
            set => isIntermediate = value;
        }

        /// <summary>
        /// ロングノーツが押下されているか。
        /// </summary>
        public bool IsPressed
        {
            get => isPressed;
            set => isPressed = value;
        }

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

        protected sealed override void Start()
        {
            // イベントの登録
            Pressed += isOn => isPressed = isOn;

            // TODO: この辺びみょい
            if (longNoteType == Reference.LongNoteType.DirectLinear || longNoteType == Reference.LongNoteType.DirectCurved)
            {
                // 中間点がないLノーツの場合、自分のレンダラーを取得してマテリアルを設定する
                linearTypeLongNoteMesh = GetComponent<MeshRenderer>();
                Pressed += isOn => linearTypeLongNoteMesh.material.SetFloat(ShaderParameter._isPressed, ShaderParameter.SetFlag(isOn));
            }
            else if (longNoteType == Reference.LongNoteType.IntermediateLinear || longNoteType == Reference.LongNoteType.IntermediateCurved)
            {
                // 中間点があるLノーツの場合、各種コンポーネントを取得してマテリアルを設定する
                curveTypeComponent = new(transform, isIntermediate);
                Pressed += isOn =>
                {
                    curveTypeComponent.longNoteMeshes.ForEach(fragment => fragment.material.SetFloat(ShaderParameter._isPressed, ShaderParameter.SetFlag(isOn)));
                    curveTypeComponent.longNoteLines.ForEach(centerLine => centerLine.material.SetFloat(ShaderParameter._isPressed, ShaderParameter.SetFlag(isOn)));
                };
            }

            // オート時はずっと押された判定になるよう、フラグを固定する
            if (Manager.info.IsAutoPlay)
            {
                Pressed.Invoke(true);
            }
        }

        protected sealed override void Update()
        {
            base.Update();
        }

        #endregion

        #region メソッド

        protected override void OnReachedLine()
        {
            // 中間点と終点のみ、押されているかつ到達時間に達したときに到達イベントを発火させる
            if (part is Reference.LongNotePart.Intermediate or Reference.LongNotePart.End)
            {
                if (!isReachedLine
                    // オートプレイの時は isPressed = true で固定なので、オートプレイ時の条件も内包
                    && isPressed
                    && Time.time - Manager.startTime >= reachedTime)
                {
                    isReachedLine = true;
                    InvokeReachedLine();
                }
            }
            // 始点は通常ノーツと同じ条件と処理
            else
            {
                base.OnReachedLine();
            }                
        }

        /// <summary>
        /// ロングノーツが押下されたときに行う処理。
        /// </summary>
        /// <remarks>
        /// マニュアルプレイ時のみ
        /// </remarks>
        /// <param name="flag">押下の有無のフラグ。</param>
        private void OnPressed(bool flag)
        {
            if (!Manager.info.IsAutoPlay)
            {
                Pressed.Invoke(flag);
            }
        }

        /// <summary>
        /// ロングノーツが押下されているかどうかを毎フレーム監視してメソッド及びイベントを処理する。
        /// </summary>
        private void Pressing()
        {
            // ゲーム中に押されていた時
            if (isPressed && GameManager.Instance.gamePlayState == GameManager.GamePlayState.Playing)
            {
                OnPressedUpdate?.Invoke(true);
            }
            // ゲーム中に押されていない時
            else if (!isPressed && GameManager.Instance.gamePlayState == GameManager.GamePlayState.Playing)
            {
                OnPressedUpdate?.Invoke(false);
            }
        }

        /// <summary>
        /// ロングノーツの情報フィールドを設定する。
        /// </summary>
        /// <param name="noteType">ノーツの種類</param>
        /// <param name="index">順番</param>
        /// <param name="status">ロングノーツのステータス</param>
        /// <param name="isInner">中間点があるか</param>
        public void SetProperties(Reference.NoteType noteType, int index, Reference.LongNotePart status, bool isInner)
        {
            SetProperties(noteType, index);
            this.part = status;
            this.isIntermediate = isInner;
            if (isInner)
            {
                if (noteType == Reference.NoteType.LinearLong)
                {
                    longNoteType = Reference.LongNoteType.IntermediateLinear;
                }
                else if (noteType == Reference.NoteType.CurvedLong)
                {
                    longNoteType = Reference.LongNoteType.IntermediateCurved;
                }
            }
            else
            {
                if (noteType == Reference.NoteType.LinearLong)
                {
                    longNoteType = Reference.LongNoteType.DirectLinear;
                }
                else if (noteType == Reference.NoteType.CurvedLong)
                {
                    longNoteType = Reference.LongNoteType.DirectCurved;
                }
            }
        }

        public void OnPointerDown(PointerEventData eventData) => OnPressed(true);
        public void OnPointerEnter(PointerEventData eventData) => OnPressed(true);
        public void OnPointerUp(PointerEventData eventData) => OnPressed(false);
        public void OnPointerExit(PointerEventData eventData) => OnPressed(false);

        #endregion
    }
}

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
    public class LongNote : Note, IPointerEnterHandler, IPointerExitHandler
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
        /// ロングノーツ1まとまりの中でのインデックス。始点が0で、そこから順に番号が振られる。
        /// </summary>
        [SerializeField] private int longNoteIndex;

        /// <summary>
        /// このロングノーツが中間点（及び終点）であるか。
        /// 自身が帯の場合は、自身を含めたロングノーツ1まとまりが中間点を持つかどうかを示す。
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
        public event Action<bool> Pressing;

        /// <summary>
        /// 自身がロングノーツの帯であるときに使用されるマテリアル。
        /// </summary>
        private RibbonMaterial ribbonMaterial;

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
        /// ロングノーツ1まとまりの中でのインデックス。始点が0で、そこから順に番号が振られる。
        /// </summary>
        public int LongNoteIndex
        {
            get => longNoteIndex;
            set => longNoteIndex = value;
        }

        /// <summary>
        /// このロングノーツが中間点（及び終点）であるか。
        /// 自身が帯の場合は、自身を含めたロングノーツ1まとまりが中間点を持つかどうかを示す。
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
        private class RibbonMaterial
        {
            /// <summary>
            /// 曲線型ロングノーツのマテリアル（曲線なので複数）
            /// </summary>
            public Material[] ribbons = null;
            
            /// <summary>
            /// 曲線型ロングノーツの中心
            /// </summary>
            public Material[] centerLines = null;

            /// <summary>
            /// ロングノーツの分割された断片のゲームオブジェクト。
            /// </summary>
            private Transform[] fragments = null;

            /// <summary>
            /// 中間点を持っているか。
            /// </summary>
            private bool hasIntermediate;

            /// <summary>
            /// 使用するシェーダーのパラメータID
            /// </summary>
            private static readonly int _isPressed = Shader.PropertyToID("_isPressed");

            public RibbonMaterial(LongNote longNote, bool hasIntermediate)
            {
                this.hasIntermediate = hasIntermediate;

                // 中間点がある場合
                if (hasIntermediate)
                {
                    // longNote (Ribbon) の子オブジェクトにあるはずの分割された断片を取得
                    fragments = Enumerable.Range(0, longNote.transform.childCount).Select(longNote.transform.GetChild).ToArray();
                    ribbons = new Material[longNote.transform.childCount];
                    centerLines = hasIntermediate ? new Material[longNote.transform.childCount] : null;
                    
                    // 帯の MeshRenderer からマテリアルを取り出す
                    for (int i = 0; i < ribbons.Length; i++)
                    {
                        ribbons[i] = fragments[i].GetComponent<MeshRenderer>().material;
                    }

                    // 中心線の LineRenderer からマテリアルを取り出す
                    for (int i = 0; i < centerLines.Length; i++)
                    {
                        centerLines[i] = fragments[i].GetComponent<LineRenderer>().material;
                    }
                }
                // 中間点がない場合
                else
                {
                    // longNote 自身が帯のオブジェクトになっているはずなので、その MeshRenderer からマテリアルを取り出す
                    ribbons = new Material[1] { longNote.GetComponent<MeshRenderer>().material };
                }
            }

            /// <summary>
            /// ロングノーツの押下の状態を、帯と中心線のマテリアルに反映させる。
            /// </summary>
            /// <param name="isPressed">押下されているかどうか</param>
            public void SetIsPressed(bool isPressed)
            {
                // 中間点がある場合は帯と中心線の両方すべてに、ない場合は帯のみに反映させる
                if (hasIntermediate)
                {
                    Array.ForEach(ribbons, material => material.SetFloat(_isPressed, isPressed ? 1 : 0));
                    Array.ForEach(centerLines, line => line.SetFloat(_isPressed, isPressed ? 1 : 0));
                }
                else
                {
                    ribbons[0].SetFloat(_isPressed, isPressed ? 1 : 0);
                }
            }
        }

        #endregion

        #region MonoBehaviorメソッド

        protected sealed override void Start()
        {
            // イベントの登録
            Pressed += isPressed => this.isPressed = isPressed;

            if (part != Reference.LongNotePart.Ribbon)
            {
                return;
            }
            // 以降の設定は、自身が帯であるときにのみ行う

            // 帯の場合はマテリアルの設定を行う
            ribbonMaterial = new(this, hasIntermediate: isIntermediate);
            Pressed += ribbonMaterial.SetIsPressed;

            // オート時はずっと押された判定になるよう、フラグを固定する
            if (Manager.info.IsAutoPlay)
            {
                Pressed.Invoke(true);
            }
        }

        protected sealed override void Update()
        {
            base.Update();
            // TODO: 将来的にロングノーツの押下の状態に応じたエフェクトなどを実装する際は、ここで Pressing イベントを発火させる
        }

        #endregion

        #region メソッド

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

        // TODO: 将来の機能追加に使えるかもしれない
        /// <summary>
        /// ロングノーツが押下されているかどうかを毎フレーム監視してメソッド及びイベントを処理する。
        /// </summary>
        private void OnPressing()
        {
            // ゲーム中に押されていた時
            if (isPressed && GameManager.Instance.gamePlayState == GameManager.GamePlayState.Playing)
            {
                Pressing?.Invoke(true);
            }
            // ゲーム中に押されていない時
            else if (!isPressed && GameManager.Instance.gamePlayState == GameManager.GamePlayState.Playing)
            {
                Pressing?.Invoke(false);
            }
        }

        /// <summary>
        /// ロングノーツの情報フィールドを設定する。
        /// </summary>
        /// <param name="noteType">ノーツの種類</param>
        /// <param name="index">順番</param>
        /// <param name="status">ロングノーツのステータス</param>
        /// <param name="isIntermediate">中間点があるか</param>
        public void SetProperties(Reference.NoteType noteType, int index, Reference.LongNotePart status, bool isIntermediate)
        {
            SetProperties(noteType, index);
            this.part = status;
            this.isIntermediate = isIntermediate;
            if (isIntermediate)
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

        public void OnPointerEnter(PointerEventData eventData) => OnPressed(true);

        public void OnPointerExit(PointerEventData eventData) => OnPressed(false);

        #endregion
    }
}

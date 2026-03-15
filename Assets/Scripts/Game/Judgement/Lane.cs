using System;
using UnityEngine;
using UnityEngine.EventSystems;
using FRONTIER.Audio;

namespace FRONTIER.Game.Judgement
{
    [RequireComponent(typeof(Collider), typeof(MeshFilter), typeof(MeshRenderer))]
    public class Lane : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        #region フィールド

        /// <summary>
        /// レーンのインデックス。
        /// </summary>
        [SerializeField] private int index;

        /// <summary>
        /// レーンがタップされたときの時間。
        /// </summary>
        [SerializeField] private float tappedTime;

        /// <summary>
        /// レーンがタップされたか。
        /// </summary>
        [SerializeField] private bool isTapped;

        /// <summary>
        /// レーンのマテリアル。MeshRendererから取得する。
        /// </summary>
        private Material material;

        /// <summary>
        /// レーンの光る速さに応じて変化させる透明度。マテリアルに適用する。
        /// </summary>
        private float alfa;

        public event Action<int, float> TappedEvent;

        #endregion

        #region プロパティ
        
        /// <summary>
        /// レーンがタップされたときの時間。
        /// </summary>
        public float TappedTime
        {
            get => tappedTime;
            set => tappedTime = value;
        }

        /// <summary>
        /// レーンがタップされたか。
        /// </summary>
        public bool IsTapped
        {
            get => isTapped;
            set => isTapped = value;
        }

        /// <summary>
        /// レーンの光る速さ。
        /// </summary>
        /// <remarks>
        /// <see cref="LaneManager"/> 側から値がセットされる 
        /// </remarks>
        public float LightSpeed { get; set; }

        #endregion

        void Start()
        {
            material = GetComponent<MeshRenderer>().material;
        }

        void Update()
        {
            material.color = new(1f, 1f, 1f, alfa);

            // タップされていないとき、アルファ値を減少させる
            if (alfa > 0 && !isTapped)
            {
                alfa -= LightSpeed * Time.unscaledDeltaTime;
            }
            // 0未満にはならないように
            alfa = alfa < 0 ? 0 : alfa;
        }

        /// <summary>
        /// レーンがタップされたときの処理。
        /// </summary>
        private void OnTapped()
        {
            // 時間を記録
            tappedTime = Time.time;
            
            // フラグを立てる
            isTapped = true;
            
            // アルファを最大の値にする
            alfa = 0.2f;

            // イベントを発火させる
            TappedEvent?.Invoke(index, tappedTime);
        }

        public void OnPointerDown(PointerEventData eventData) => OnTapped();

        public void OnPointerUp(PointerEventData eventData) => isTapped = false;

        public void OnPointerEnter(PointerEventData eventData) => OnTapped();

        public void OnPointerExit(PointerEventData eventData) => isTapped = false;
    }
}

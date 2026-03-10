using UnityEngine;
using UnityEngine.EventSystems;
using FRONTIER.Audio;

namespace FRONTIER.Game.InputManagement
{
    [RequireComponent(typeof(Collider), typeof(MeshFilter), typeof(MeshRenderer))]
    public class Lane : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        /// <summary>
        /// 
        /// </summary>
        [SerializeField] private InputManager tapManager;

        /// <summary>
        /// レーンのインデックス。
        /// </summary>
        [SerializeField] private int laneIndex;

        /// <summary>
        /// レーンのマテリアル。MeshRendererから取得する。
        /// </summary>
        private Material material;

        /// <summary>
        /// レーンの光る速さに応じて変化させる透明度。マテリアルに適用する。
        /// </summary>
        private float alfa;
        
        /// <summary>
        /// レーンがタップされたときの時間。InputManagerに渡すためのプロパティ。
        /// </summary>
        private float TappedTime
        {
            get => tapManager.tappedTime[laneIndex];
            set => tapManager.tappedTime[laneIndex] = value;
        }

        /// <summary>
        /// レーンがタップされたか。InputManagerに渡すためのプロパティ。
        /// </summary>
        private bool IsTapped
        {
            get => tapManager.tappedLaneFlags[laneIndex];
            set => tapManager.tappedLaneFlags[laneIndex] = value;
        }
        

        void Start()
        {
            material = GetComponent<MeshRenderer>().material;
        }

        void Update()
        {
            material.color = new(1f, 1f, 1f, alfa);

            // タップされていないとき、アルファ値を減少させる
            if (alfa > 0 && !IsTapped)
            {
                alfa -= tapManager.LightSpeed * Time.unscaledDeltaTime;
            }

            // 0未満にはならないように
            alfa = alfa < 0 ? 0 : alfa;
        }

        /// <summary>
        /// レーンがタップされたときの処理。
        /// </summary>
        public void OnTapped()
        {
            // 時間を記録
            TappedTime = Time.time;
            
            // フラグを立てる
            IsTapped = true;
            
            // アルファを最大の値にする
            alfa = 0.2f;

            // イベントを発火させる
            tapManager.TappedEvent[laneIndex]?.Invoke(laneIndex, TappedTime);

            GameManager.Instance.audios.seManager.Play(SEManager.SE.TapedLane);
        }

        public void OnPointerDown(PointerEventData eventData) => OnTapped();

        public void OnPointerUp(PointerEventData eventData) => IsTapped = false;

        public void OnPointerEnter(PointerEventData eventData) => OnTapped();

        public void OnPointerExit(PointerEventData eventData) => IsTapped = false;
    }
}

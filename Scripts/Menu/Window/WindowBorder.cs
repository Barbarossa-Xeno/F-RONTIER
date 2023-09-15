using UnityEngine;
using FRONTIER.Utility;
using UnityEngine.UI;
using UnityEngine.Events;

namespace FRONTIER.Menu.Window
{
    /// <summary>
    /// ウィンドウの境界線を描くためのクラス。
    /// </summary>
    public class WindowBorder : MonoBehaviour
    {
        /// <summary>
        /// 曲線をかたどる点（ローカル座標）の集まり。
        /// </summary>
        /// <remarks>
        /// クラス変数
        /// </remarks>
        protected static Vector3[] curveLocalPositions;

        /// <summary>
        /// メニューウィンドウ全体の基準点。
        /// </summary>
        protected static Vector2 origin;

        /// <summary>
        /// 親の幅や高さを取得する。
        /// </summary>
        protected static ParentRect parentRect;

        /// <summary>
        /// 親（このクラス(<see cref = "WindowBorder"/>)からすると自分だが）の幅や高さ、トランスフォームを保持するクラス。
        /// </summary>
        protected class ParentRect
        {
            /// <summary>
            /// Canvasに対するメニューウィンドウの幅
            /// </summary>
            public float width;

            /// <summary>
            /// Canvasに対するメニューウィンドウの高さ
            /// </summary>
            public float height;

            /// <summary>
            /// Canvasに対するメニューウィンドウの幅の割合
            /// </summary>
            public float widthToParent;
        }

        /// <summary>
        /// キャンバスの大きさ。
        /// </summary>
        protected static Vector2 canvasSize;

        /// <summary>
        /// キャンバスの大きさを取得し続ける。
        /// </summary>
        private Vector2 CanvasSize => transform.root.GetComponent<RectTransform>().sizeDelta;

        /// <summary>
        /// 初期化メソッドをインスペクターから指定するためのイベント。
        /// </summary>
        [SerializeField] private UnityEvent InitializeEvents;

        /// <summary>
        /// Update時に実行するメソッドをインスペクターから指定するためのイベント。
        /// </summary>
        [SerializeField] private UnityEvent OnUpdateEvents;

        /* Monobehavior イベント */
        void Awake() => Initialize();

        void OnValidate() => Initialize();

        void Update()
        {
            canvasSize = CanvasSize;
            OnUpdateEvents?.Invoke();
        }

        /// <summary>
        /// 初期化する。
        /// </summary>
        public virtual void Initialize()
        {
            canvasSize = CanvasSize;
            parentRect = new()
            {
                width = transform.root.GetComponent<CanvasScaler>().referenceResolution.x * transform.GetComponent<RectTransform>().anchorMin.x,
                height = transform.root.GetComponent<CanvasScaler>().referenceResolution.y * transform.GetComponent<RectTransform>().anchorMax.y,
                widthToParent = GetComponent<RectTransform>().anchorMin.x
            };

            InitializeEvents?.Invoke();
        }

        /// <summary>
        /// 難易度の変更に応じてオブジェクトの色を変更する。
        /// </summary>
        /// <param name="difficulty">難易度</param>
        public virtual void SetColorTrigger(Reference.DifficultyEnum difficulty) { }

        /// <summary>
        /// オブジェクトのスケールを変更する。（画面の高さが変わった時に）
        /// </summary>
        public virtual void ReScaleObject() { }
    }
}
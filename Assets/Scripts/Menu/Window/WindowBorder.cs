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
        #region フィールド

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
        protected static WindowRectSize parentRect;

        /// <summary>
        /// キャンバスの大きさ。
        /// </summary>
        protected static Vector2 canvasSize;

        /// <summary>
        /// 初期化メソッドをインスペクターから指定するためのイベント。
        /// </summary>
        [SerializeField] protected UnityEvent InitializeEvents;

        /// <summary>
        /// Update時に実行するメソッドをインスペクターから指定するためのイベント。
        /// </summary>
        [SerializeField] protected UnityEvent OnUpdateEvents;

        #endregion

        #region プロパティ

        /// <summary>
        /// キャンバスの大きさを取得し続ける。
        /// </summary>
        private Vector2 CanvasSize => transform.root.GetComponent<RectTransform>().sizeDelta;

        #endregion

        #region クラス

        /// <summary>
        /// このクラス（コンポーネント）の幅や高さを保持するクラス。
        /// </summary>
        /// <remarks>
        /// <b>アタッチするコンポーネントの Transform の Anchor 構成に強く依存する。</b>
        /// そのため、常に親の Transform に対して相対的な値で扱えるようにする。 
        /// </remarks> 
        protected class WindowRectSize
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
            public float parentWidthRatio;
            
            public WindowRectSize(WindowBorder windowBorder)
            {
                CanvasScaler rootCanvasScaler = windowBorder.transform.root.GetComponent<CanvasScaler>();
                RectTransform rectTransform = windowBorder.transform.GetComponent<RectTransform>();

                width = rootCanvasScaler.referenceResolution.x * rectTransform.anchorMin.x;
                height = rootCanvasScaler.referenceResolution.y * rectTransform.anchorMax.y;
                parentWidthRatio = rectTransform.anchorMin.x;
            }
        }

        #endregion
        
        #region MonoBehaviourメソッド
        
        void Awake() => Initialize();

        void OnValidate() => Initialize();

        void Update()
        {
            canvasSize = CanvasSize;
            OnUpdateEvents?.Invoke();
        }

        #endregion

        #region 仮想メソッド

        /// <summary>
        /// 初期化する。
        /// </summary>
        public virtual void Initialize()
        {
            canvasSize = CanvasSize;
            parentRect = new WindowRectSize(this);

            InitializeEvents?.Invoke();
        }

        /// <summary>
        /// 難易度の変更に応じてオブジェクトの色を変更する。
        /// </summary>
        /// <param name="difficulty">難易度</param>
        public virtual void SetColorTrigger(Reference.DifficultyRank difficulty) { }

        /// <summary>
        /// オブジェクトのスケールを変更する。（画面の高さが変わった時に）
        /// </summary>
        public virtual void RescaleObject() { }

        #endregion
    }
}

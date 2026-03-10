using UnityEngine;
using System;
using System.Collections;
using TMPro;

namespace FRONTIER.Utility.Asset
{
    /// <summary>
    /// オーバーフローしたTextMeshProのテキストを左にスクロールさせる。
    /// </summary>
    public class OverflowTextScroll : MonoBehaviour
    {
        #region フィールド

        /// <summary>
        /// スクロールさせるTMPro。
        /// </summary>
        [SerializeField] private TextMeshProUGUI text;

        /// <summary>
        /// スクロールの速さ。
        /// </summary>
        [SerializeField] private float scrollSpeed;

        /// <summary>
        /// スクロールの待ち時間。
        /// </summary>
        [SerializeField] private WaitTime waitTime;

        /// <summary>
        /// テキストのパディング。
        /// </summary>
        [SerializeField] private Padding padding;

        /// <summary>
        /// タイムスケールを無視するか。
        /// </summary>
        [SerializeField] private bool ignoreTimeScale = false;

        /// <summary>
        /// テキスト領域を表示する。
        /// </summary>
        [SerializeField] private bool showTextArea = false;

        /// <summary>
        /// 溢れてしまったテキストの長さ。
        /// </summary>
        public float overflowPosition;

        /// <summary>
        /// 親のRectTransform。
        /// </summary>
        private RectTransform parentTransform;

        /// <summary>
        /// TMPのテキストのテンポラリー
        /// </summary>
        private string text_tmp = null;

        /// <summary>
        /// 使用中のルーチン
        /// </summary>
        private IEnumerator currentEnumerator;

        #endregion

        #region プロパティ

        /// <summary>
        /// TMPに設定するテキスト。
        /// </summary>
        public string Text
        {
            get { return text.text; }
            set { text.text = value; }
        }

        /// <summary>
        /// タイムスケールを無視するかどうかでデルタタイムの参照先を変える。
        /// </summary>
        private float DeltaTime
        {
            get
            {
                if (ignoreTimeScale) return Time.unscaledDeltaTime;
                else return Time.deltaTime;
            }
        }

        #endregion

        #region 構造体

        /// <summary>
        /// スクロールの開始待ち時間と終了待ち時間。
        /// </summary>
        [Serializable]
        private struct WaitTime
        {
            public float start;
            public float end;
        }

        [Serializable]
        private struct Padding
        {
            public float left;
            public float top;
            public float right;
            public float bottom;
        }

        #endregion

        #region MonoBehaviourメソッド

        void Awake() => Init();

        // コルーチンを登録
        void Start()
        {
            currentEnumerator = ScrollText(text.rectTransform, overflowPosition);
            StartCoroutine(currentEnumerator);
        }

        void OnValidate()
        {
            SetPadding();
            TextArea();
        }

        void Update()
        {
            if (Extension.IsValueChanged(ref text_tmp, text.text))
            {
                Init();
                StopCoroutine(currentEnumerator);
                currentEnumerator = ScrollText(text.rectTransform, overflowPosition);
                StartCoroutine(currentEnumerator);
            }
        }

        #endregion

        #region メソッド

        /// <summary>
        /// 要素を再取得する初期化。
        /// </summary>
        public void Init()
        {
            // いろいろ取得する
            parentTransform = GetComponent<RectTransform>();
            overflowPosition = parentTransform.sizeDelta.x - text.preferredWidth;
            if (overflowPosition < 0) { overflowPosition = -overflowPosition; }
            else { overflowPosition = 0; }
            // 位置合わせ
            text.rectTransform.offsetMin = new Vector2(0, 0);
            text.rectTransform.offsetMax = new Vector2(0, 0);
            text_tmp = text.text;
            SetPadding();
        }

        /// <summary>
        /// テキストをスクロールさせる。
        /// </summary>
        /// <param name="rect">TMPの位置。</param>
        /// <param name="overflow">溢れたテキストの幅。</param>
        /// <returns>コルーチン登録用</returns>
        private IEnumerator ScrollText(RectTransform rect, float overflow)
        {
            // オーバーフローしたテキストが無ければコルーチンを破棄
            if (overflow == 0) { yield break; }

            // ループさせてスクロールを繰り返す
            while (true)
            {
                // スクロールの開始を待つ
                yield return ignoreTimeScale ? new WaitForSecondsRealtime(waitTime.start) : new WaitForSeconds(waitTime.start);
                // テキストがすべて表示されるまで毎フレームごとにテキストを移動させる
                while (rect.offsetMin.x >= -overflow)
                {
                    float delta = scrollSpeed * DeltaTime;
                    // (left, top)
                    rect.offsetMin -= new Vector2(delta, rect.offsetMin.y);
                    // (right, bottom)
                    rect.offsetMax -= new Vector2(delta, rect.offsetMax.y);

                    yield return null;
                }
                // 最後までスクロールしたら一定時間待って初期位置に戻す
                yield return ignoreTimeScale ? new WaitForSecondsRealtime(waitTime.end) : new WaitForSeconds(waitTime.end);
                rect.offsetMin = new Vector2(0, rect.offsetMin.y);
                rect.offsetMax = new Vector2(0, rect.offsetMax.y);
            }
        }

        private void SetPadding()
        {
            text.rectTransform.offsetMin = new Vector2(padding.left, -padding.top);
            text.rectTransform.offsetMax = new Vector2(-padding.right, padding.bottom);
        }

        private void TextArea()
        {
            GetComponent<UnityEngine.UI.Image>().enabled = showTextArea;
        }
        
        #endregion
    }
}
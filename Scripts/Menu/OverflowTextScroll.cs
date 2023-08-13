using UnityEngine;
using System.Collections;
using TMPro;

/// <summary>
/// オーバーフローしたTextMeshProのテキストを左にスクロールさせる。
/// </summary>
public class OverflowTextScroll : MonoBehaviour
{
    /// <summary>
    /// スクロールさせるTMPro。
    /// </summary>
    [SerializeField] private TextMeshProUGUI tmp;
    
    /// <summary>
    /// スクロールの速さ。
    /// </summary>
    [SerializeField] private float scrollSpeed;
    
    /// <summary>
    /// スクロールの待ち時間。
    /// </summary>
    [SerializeField] private WaitTime waitTime;
    
    /// <summary>
    /// 溢れてしまったテキストの長さ。
    /// </summary>
    private float overflowPosition;
    
    /// <summary>
    /// 親のRectTransform。
    /// </summary>
    private RectTransform parentTransform;
    
    /// <summary>
    /// TMProのRectTransform。
    /// </summary>
    private RectTransform tmpTransform;
    
    /// <summary>
    /// スクロールの開始待ち時間と終了待ち時間。
    /// </summary>
    [System.Serializable]
    private struct WaitTime
    {
        public float start;
        public float end;
    }

    void Awake()
    {
        // いろいろ取得する
        parentTransform = GetComponent<RectTransform>();
        overflowPosition = tmp.preferredWidth;
        tmpTransform = tmp.GetComponent<RectTransform>();
    }

    // コルーチンを登録
    void Start() => StartCoroutine(ScrollText(tmpTransform, overflowPosition));

    /// <summary>
    /// テキストをスクロールさせる。
    /// </summary>
    /// <param name="rect">TMPの位置。</param>
    /// <param name="overflow">溢れたテキストの幅。</param>
    /// <returns>コルーチン登録用</returns>
    private IEnumerator ScrollText(RectTransform rect, float overflow)
    {
        // ループさせてスクロールを繰り返す
        while (true)
        {
            // スクロールの開始を待つ
            yield return new WaitForSeconds(waitTime.start);
            // テキストがすべて表示されるまで毎フレームごとにテキストを移動させる
            while (rect.offsetMin.x >= -overflow + parentTransform.sizeDelta.x)
            {
                float delta = scrollSpeed * Time.deltaTime;
                rect.offsetMin -= new Vector2(delta, rect.offsetMin.y);
                rect.offsetMax -= new Vector2(delta, rect.offsetMax.y);
                yield return null;
            }
            // 最後までスクロールしたら一定時間待って初期位置に戻す
            yield return new WaitForSeconds(waitTime.end);
            rect.offsetMin = new Vector2(0, rect.offsetMin.y);
            rect.offsetMax = new Vector2(0, rect.offsetMax.y);
        }


    }
}

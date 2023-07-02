using UnityEngine;
using System.Collections.Generic;

namespace CLSrollProject
{
    ///<summary>スクロールテキストを強制的に初期化します。</summary>
    public class CLScrollTextInitialize : MonoBehaviour
    {
        /* フィールド */
        ///<summary>CLScrollクラスを持つスクロールさせる対象のゲームオブジェクト（Prefab）。</summary>
        [SerializeField] private List<CLScroll> clScrollElements;
        ///<summary>初期化できる状態か、そうでないかを表す真偽値。</summary>
        bool isReset = false;
        /* メソッド */
        ///<summary>テキスト状態を強制的に初期化・更新します。</summary>
        public void UpdateTextCondition()
        {
            if (!isReset)
            {
                foreach (var elem in clScrollElements)
                {
                    elem.UpdateState();
                }
                isReset = true;
            }
        }
        ///<summary>フィールド<see cref = "isReset"/>をFalseに戻し、再び初期化できる状態に戻します。</summary>
        public void UpdateTextCondition(bool trigger = false) => isReset = trigger ? false : false;
    }
}

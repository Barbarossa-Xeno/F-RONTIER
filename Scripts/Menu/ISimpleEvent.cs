using UnityEngine;

namespace FRONTIER.Menu
{
    /// <summary>
    /// <see cref="SimpleEventTrigger"/>とともに実装するインターフェイス。
    /// </summary>
    public interface ISimpleEvent
    {
        /// <summary>
        /// イベントを登録するためのメソッド。
        /// </summary>
        void RegisterEvent();

        /// <summary>
        /// イベントとして実行させたい処理。
        /// </summary>
        void EventMethod();
    }
}
using System;
using UnityEngine;
using UnityEngine.Events;

namespace FRONTIER.Menu
{
    /// <summary>
    /// このクラスをいずれかのスクリプトと一緒に使うことで、スクリプト内のメソッドをこのクラス側で実行させる。<br/>
    /// 使い捨てのスクリプトを作るときに有効と思われる。
    /// </summary>
    public class SimpleEventTrigger : MonoBehaviour
    {
        /// <summary>
        /// インスペクターから実行したいメソッドを指定するときに使う。
        /// </summary>
        [SerializeField] private UnityEvent unityEvent = default;

        /// <summary>
        /// アクション型のデリゲートを登録するためのイベント。
        /// </summary>
        public event Action EventActions = default;

        /// <summary>
        /// 登録したイベントを発火するタイミング。
        /// </summary>
        public TriggerTiming triggerTiming = default;

        public bool onValidate = true;

        /// <summary>
        /// 登録したイベントを発火するタイミング。
        /// </summary>
        public enum TriggerTiming
        {
            Awake, OnEnable, Start, Update
        }

        /// <summary>
        /// 発火
        /// </summary>
        private void Trigger()
        {
            unityEvent?.Invoke();
            EventActions?.Invoke();
        }
        
        void Awake()
        {
            if (triggerTiming == TriggerTiming.Awake) { Trigger(); }
        }

        void OnEnable()
        {
            if (triggerTiming == TriggerTiming.OnEnable) { Trigger(); }
        }

        void Start()
        {
            if (triggerTiming == TriggerTiming.Start) { Trigger(); }
        }

        void Update()
        {
            if (triggerTiming == TriggerTiming.Update) { Trigger(); }
        }

        void OnValidate()
        {
            if (onValidate) { Trigger(); }
        }
    }
}

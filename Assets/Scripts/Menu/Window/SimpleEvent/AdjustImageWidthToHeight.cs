using UnityEngine;

namespace FRONTIER.Menu.Window
{
    public class AdjustImageWidthToHeight : MonoBehaviour, ISimpleEvent
    {
        [SerializeField] private SimpleEventTrigger simpleEventTrigger;

        void Awake() => RegisterEvent();

        public void RegisterEvent() => simpleEventTrigger.EventActions += () => EventMethod();

        public void EventMethod()
        {
            RectTransform rectTransform = transform.GetComponent<RectTransform>();
            float height = rectTransform.rect.height * 0.9f;
            rectTransform.sizeDelta = new(height, rectTransform.sizeDelta.y * 0.9f);
        }
    }
}

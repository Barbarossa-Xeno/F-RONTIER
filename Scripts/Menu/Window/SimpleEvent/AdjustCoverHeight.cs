using UnityEngine;

namespace Game.Menu.Window
{
    public class AdjustCoverHeight : MonoBehaviour, ISimpleEvent
    {
        [SerializeField] private SimpleEventTrigger simpleEventTrigger;

        void Awake() => RegisterEvent();

        public void RegisterEvent() => simpleEventTrigger.EventActions += () => EventMethod();

        public void EventMethod()
        {
            RectTransform rectTransform = transform.GetComponent<RectTransform>();
            float width = rectTransform.rect.width;
            float parentHeight = transform.parent.GetComponent<RectTransform>().rect.height;
            if (width > parentHeight)
            {
                transform.localScale = new(parentHeight / width, parentHeight / width, 1);
            }
            rectTransform.sizeDelta = new(rectTransform.sizeDelta.x, width);
        }
    }
}
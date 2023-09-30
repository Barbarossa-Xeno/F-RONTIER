using UnityEngine;
using UnityEngine.EventSystems;
using FRONTIER.Utility;

namespace FRONTIER.Game.InputManageMent
{
    public class Tap : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private InputManager tapManager;
        [SerializeField] private int laneIndex;
        private Material material;
        private float alfa;
        private AudioClip se;
        
        private float TapTime
        {
            get => tapManager.inputTime[laneIndex];
            set => tapManager.inputTime[laneIndex] = value;
        }
        private bool IsTapped
        {
            get => tapManager.inputFlag[laneIndex];
            set => tapManager.inputFlag[laneIndex] = value;
        }
        

        void Start()
        {
            tapManager = transform.parent.GetComponent<InputManager>();
            material = GetComponent<Renderer>().material;
            se = Resources.Load<AudioClip>(Reference.ResourcesPath.TAP_SE_PATH);
        }

        void Update()
        {
            material.color = new(1f, 1f, 1f, alfa);
            if (alfa > 0 && !IsTapped)
            {
                alfa -= tapManager.lightSpeed * Time.unscaledDeltaTime;
            }
            alfa = alfa < 0 ? 0 : alfa;
        }

        public void OnTap()
        {
            TapTime = Time.time;
            tapManager.onInput[laneIndex]?.Invoke(laneIndex, TapTime);
            IsTapped = true;
            alfa = 0.2f;
            GameManager.instance.audioManagers.seManager.Source.PlayOneShot(se);
        }

        public void OnPointerDown(PointerEventData eventData) => OnTap();

        public void OnPointerUp(PointerEventData eventData) => IsTapped = false;

        public void OnPointerEnter(PointerEventData eventData)
        {
            /*
            GetTap();
            IsTapped = true;
            GameManager.instance.seSource.PlayOneShot(se);
            */
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            /*
            IsTapped = false;
            */
        }
    }
}
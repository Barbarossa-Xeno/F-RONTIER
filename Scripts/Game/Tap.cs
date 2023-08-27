using UnityEngine;
using UnityEngine.EventSystems;
using Game.Utility;

public class Tap : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    public int laneIndex;
    TapManager tapManager;
    Material material;
    private float alfa;
    public float time { get { return tapManager.tapTime[laneIndex]; } set { tapManager.tapTime[laneIndex] = value; } }
    private bool isPressed { get { return tapManager.tapFlag[laneIndex]; } set { tapManager.tapFlag[laneIndex] = value; } }
    private AudioClip se;

    void Start()
    {
        tapManager = transform.parent.GetComponent<TapManager>();
        material = GetComponent<Renderer>().material;
        se = (AudioClip)Resources.Load(Reference.ResourcesPath.TAP_SE_PATH);
    }

    // Update is called once per frame
    void Update()
    {
        material.color = ColorApply(alfa);
        if (alfa > 0 && !isPressed)
        {
            alfa -= tapManager.lightSpeed * Time.unscaledDeltaTime;
        }
        alfa = alfa < 0 ? 0 : alfa;
    }

    public void GetTap()
    {        
        alfa = 0.2f;
        time = Time.time;
    }
    public Color ColorApply(float alfaValue) { return new Color(1f, 1f, 1f, alfaValue); }

    public void OnPointerDown(PointerEventData eventData)
    {
        GetTap();
        isPressed = true;
        GameManager.instance.seSource.PlayOneShot(se);
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        GetTap();
        //isPressed = true;
        GameManager.instance.seSource.PlayOneShot(se);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        isPressed = false;
    }
}

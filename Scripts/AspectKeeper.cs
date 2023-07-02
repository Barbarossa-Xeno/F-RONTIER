using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class AspectKeeper : MonoBehaviour
{
    [SerializeField]
    private Camera target;
    [SerializeField]
    private Vector2 aspect;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float screenAspect = Screen.width / (float)Screen.height;
        float targetAspect = aspect.x / aspect.y;
        float rate = targetAspect / screenAspect;
        Rect viewportRect = new Rect(0, 0, 1, 1);

        if(rate < 1){
            viewportRect.width = rate;
            viewportRect.x = 0.5f - viewportRect.width * 0.5f;
        }
        else{
            viewportRect.height = 1 / rate;
            viewportRect.y = 0.5f - viewportRect.height * 0.5f;
        }
        target.rect = viewportRect;
    }
}

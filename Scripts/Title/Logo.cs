using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Logo : MonoBehaviour
{
    private new Animation animation;
    private Material material;
    private float alfa = 0;
    [SerializeField, Range(0.001f, 3f)] private float FadeSpeed;
    [SerializeField, Range(0.1f, 2f)] private float Intensity;
    // Start is called before the first frame update
    void Start()
    {
        animation = this.GetComponent<Animation>();
        material = this.GetComponent<Image>().material;
    }

    // Update is called once per frame
    void Update()
    {
        material.color = new Color(material.color.r, material.color.g, material.color.b, alfa);

        if(animation.isPlaying){
            alfa += Time.deltaTime * FadeSpeed;
            if(alfa > 1f){
                alfa = 1f;
            }
        }
        if(alfa > 0.9f){
            material.EnableKeyword("_EMISSION");
            float factor = Mathf.Pow(2, Intensity);
            material.SetColor("_EmissionColor", new Color(material.color.r * factor, material.color.g * factor, material.color.b * factor));
        }
    }
}

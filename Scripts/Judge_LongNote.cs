using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Judge_LongNote : MonoBehaviour
{
    GameObject note;
    // Start is called before the first frame update
    void Start()
    {

    }

    void FixedUpdate()
    {
        if(Input.GetMouseButton(0)){
            GetTap();
        }
        /*
        if(this.transform.position.z < 0){
            this.gameObject.SetActive(this.gameObject.activeSelf? false: false);
        }*/
    }

    private void GetTap(){
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        int layerMask = 1 << 6;
        if(Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask)){
            Debug.Log(hit.collider.name);
            if(this.transform.Equals(hit.collider.transform)){
                Debug.Log("issho");
                //UnityEditor.EditorApplication.isPaused = true;
            }
        }
    }
}

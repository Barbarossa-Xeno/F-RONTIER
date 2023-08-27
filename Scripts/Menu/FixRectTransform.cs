using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Menu
{
    public class FixRectTransform : MonoBehaviour
    {
        private new RectTransform transform;
        private float positionY;
        // Start is called before the first frame update
        void Start()
        {
            positionY = -250;
            this.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, positionY, 0);
        }
    }
}

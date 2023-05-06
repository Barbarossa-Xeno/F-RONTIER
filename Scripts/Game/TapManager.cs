using UnityEngine;

public class TapManager : MonoBehaviour
{
    [SerializeField][Range(0.1f, 2f)] public float lightSpeed = 0.1f;
    public bool[] tapFlag = new bool[6];
    public float[] tapTime = new float[6];
}
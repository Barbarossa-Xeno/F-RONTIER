using UnityEngine;
using FadeTransition;

public class GameStart : MonoBehaviour
{
    [SerializeField] private float fadeSpeed;
    // Start is called before the first frame update

    public void ScreenTaped(){
        SceneNavigator.instance.SceneChange(sceneName: "Menu", _fadeTime: fadeSpeed);
    }
}

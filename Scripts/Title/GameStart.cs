using UnityEngine;
using FadeTransition;

namespace FRONTIER.Title
{
    public class GameStart : MonoBehaviour
    {
        [SerializeField] private float fadeSpeed;

        public void ScreenTaped()
        {
            GameManager.instance.scene.menu.Invoke();
        }
    }
}
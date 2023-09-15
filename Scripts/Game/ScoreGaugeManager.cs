using UnityEngine;
using UnityEngine.UI;
using FRONTIER.Utility;
using TMPro;

namespace FRONTIER.Game
{
    public class ScoreGaugeManager : MonoBehaviour
    {
        [SerializeField] private Slider slider;
        private TextMeshProUGUI[] scoreLank = new TextMeshProUGUI[4];

        void Awake()
        {
            slider.value = 0;
            for (int i = 0; i < 4; i++)
            {
                scoreLank[i] = GameObject.Find("Score").transform.GetChild(i).GetComponent<TextMeshProUGUI>();
            }
        }

        // Update is called once per frame
        void Update()
        {
            slider.value = (GameManager.instance.scoreManager.score / 1000000f);

            if (GameManager.instance.scoreManager.score >= (int)Reference.RankBorder.C)
            {
                scoreLank[0].color = new Color(152f / 255f, 94f / 255f, 39f / 255f, 255f / 255f);
                scoreLank[0].outlineColor = new Color(1f, 1f, 1f, 1f);
            }
            if (GameManager.instance.scoreManager.score >= (int)Reference.RankBorder.B)
            {
                scoreLank[1].color = new Color(135f / 255f, 135f / 255f, 135f / 255f, 255f / 255f);
                scoreLank[1].outlineColor = new Color(1f, 1f, 1f, 1f);
            }
            if (GameManager.instance.scoreManager.score >= (int)Reference.RankBorder.A)
            {
                scoreLank[2].color = new Color(173f / 255f, 146f / 255f, 34f / 255f, 255f / 255f);
                scoreLank[2].outlineColor = new Color(1f, 1f, 1f, 1f);
            }
            if (GameManager.instance.scoreManager.score >= (int)Reference.RankBorder.S)
            {
                scoreLank[3].color = new Color(150f / 255f, 206f / 255f, 199f / 255f, 255f / 255f);
                scoreLank[3].outlineColor = new Color(1f, 1f, 1f, 1f);
            }
        }
    }
}
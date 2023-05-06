using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;
using FadeTransition;

public class GameMenu : MonoBehaviour
{
    [SerializeField] Button pauseButton = default;
    [SerializeField] IntroTransitionElements introTransitionElements;
    [SerializeField] PauseMenuElements pauseMenuElements;
    [SerializeField] MVManager mvManager;
    private bool isPaused = false;
    private AudioSource audioSource;
    private VideoPlayer videoSource;
    public bool operationStart = false;
    [System.Serializable]
    class IntroTransitionElements
    {
        [SerializeField] public Animator BannerAnimator;
        [SerializeField] public Image BannerTop;
        [SerializeField] public Image BannerUnder;
    }
    [System.Serializable]
    class PauseMenuElements
    {
        [SerializeField] public GameObject pauseWindow = default;
        [SerializeField] public Button retireButton = default;
        [SerializeField] public Button continueButton = default;
        [SerializeField] public Button retryButton = default;
        [SerializeField] public TextMeshProUGUI countDownText = default;
    }
    private static class ImageColor
    {
        public static Color32 normal = new Color32(76, 199, 255, 255);
        public static Color32 hard = new Color32(255, 162, 76, 255);
        public static Color32 expert = new Color32(255, 76, 89, 255);
        public static Color32 master = new Color32(140, 76, 255, 255);
    }

    void Start()
    {
        switch (GameManager.instance.difficulty)
        {
            case "NORMAL":
                introTransitionElements.BannerTop.color = ImageColor.normal;
                introTransitionElements.BannerUnder.color = ImageColor.normal;
                introTransitionElements.BannerTop.color = new Color(introTransitionElements.BannerTop.color.r, introTransitionElements.BannerTop.color.g, introTransitionElements.BannerTop.color.b, 0.7f);
                introTransitionElements.BannerUnder.color = new Color(introTransitionElements.BannerTop.color.r, introTransitionElements.BannerTop.color.g, introTransitionElements.BannerTop.color.b, 0.85f);
                break;
            case "HARD":
                introTransitionElements.BannerTop.color = ImageColor.hard;
                introTransitionElements.BannerUnder.color = ImageColor.hard;
                introTransitionElements.BannerTop.color = new Color(introTransitionElements.BannerTop.color.r, introTransitionElements.BannerTop.color.g, introTransitionElements.BannerTop.color.b, 0.7f);
                introTransitionElements.BannerUnder.color = new Color(introTransitionElements.BannerTop.color.r, introTransitionElements.BannerTop.color.g, introTransitionElements.BannerTop.color.b, 0.85f);
                break;
            case "EXPERT":
                introTransitionElements.BannerTop.color = ImageColor.expert;
                introTransitionElements.BannerUnder.color = ImageColor.expert;
                introTransitionElements.BannerTop.color = new Color(introTransitionElements.BannerTop.color.r, introTransitionElements.BannerTop.color.g, introTransitionElements.BannerTop.color.b, 0.7f);
                introTransitionElements.BannerUnder.color = new Color(introTransitionElements.BannerTop.color.r, introTransitionElements.BannerTop.color.g, introTransitionElements.BannerTop.color.b, 0.85f);
                break;
            case "MASTER":
                introTransitionElements.BannerTop.color = ImageColor.master;
                introTransitionElements.BannerUnder.color = ImageColor.master;
                introTransitionElements.BannerTop.color = new Color(introTransitionElements.BannerTop.color.r, introTransitionElements.BannerTop.color.g, introTransitionElements.BannerTop.color.b, 0.7f);
                introTransitionElements.BannerUnder.color = new Color(introTransitionElements.BannerTop.color.r, introTransitionElements.BannerTop.color.g, introTransitionElements.BannerTop.color.b, 0.85f);
                break;
            default: return;
        }
        pauseMenuElements.pauseWindow.SetActive(pauseMenuElements.pauseWindow.activeSelf ? false : false);
        audioSource = GameManager.instance.musicSource;
        videoSource = mvManager.GetComponent<VideoPlayer>();
        Time.timeScale = 1;

        StartCoroutine(IntroAnimation());
    }

    private IEnumerator IntroAnimation()
    {
        introTransitionElements.BannerAnimator.SetTrigger("activate");
        yield return new WaitForSeconds(3f);
        GameManager.instance.gamePlayState = GameManager.GamePlayState.Starting;
    }

    ///<summary>ポーズボタンを押下した時の挙動です。</summary>
    ///<remarks><see cref = "pauseButton"/>のボタンコンポーネントメソッドに直接登録します。</remarks>
    public void Pause()
    {
        if (!isPaused)
        {  //ポーズしていないとき、押下されたら、
            Time.timeScale = 0;
            if (audioSource != null) audioSource.Pause();
            videoSource.Pause();
            isPaused = true;
            PauseMenu();
            GameManager.instance.gamePlayState = GameManager.GamePlayState.Pausing;
        }
    }
    private void PauseMenu()
    {
        if (isPaused)
        {
            pauseMenuElements.pauseWindow.SetActive(!pauseMenuElements.pauseWindow.activeSelf ? true : true);
        }
    }

    public void Retire()
    {
        SceneNavigator.instance.SceneChange("Menu", 1f, ignoreTimeScale: true);
        GameManager.instance.gamePlayState = GameManager.GamePlayState.Inactiving;
    }
    public void Continue()
    {
        pauseMenuElements.pauseWindow.SetActive(false);
        IEnumerator CountDown()
        {
            for (int i = 3; i >= 0; i--)
            {
                yield return new WaitForSecondsRealtime(1f);
                pauseMenuElements.countDownText.GetComponent<Animator>().SetTrigger("Start");
                pauseMenuElements.countDownText.text = i.ToString();
            }
            pauseMenuElements.countDownText.text = "";
            isPaused = false;
            Time.timeScale = 1;
            audioSource.Play();
            if (GameManager.instance.MV) { videoSource.Play(); }
            GameManager.instance.gamePlayState = GameManager.GamePlayState.Playing;
            yield break;
        }
        StartCoroutine(CountDown());
    }
    public void Retry()
    {
        Time.timeScale = 1;
        SceneNavigator.instance.SceneChange(SceneManager.GetActiveScene().name, 1.0f, ignoreTimeScale: true);
        GameManager.instance.gamePlayState = GameManager.GamePlayState.Starting;
        GameManager.instance.start = false;
    }

}

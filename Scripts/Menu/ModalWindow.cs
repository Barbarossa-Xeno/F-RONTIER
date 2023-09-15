using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using CLSrollProject;
using FadeTransition;
using FRONTIER.Save;

namespace FRONTIER.Menu
{
    public class ModalWindow : MonoBehaviour
    {
        [SerializeField] private Button settingOpenButtton;
        [SerializeField] private Button notificationOpenButton;
        [SerializeField] private Button settingCloseButtton;
        [SerializeField] private Button notificationCloseButton;
        [SerializeField] private GameObject settingWindow;
        [SerializeField] private SettingElement settingElement;
        [SerializeField] private GameObject notificationWindow;
        [SerializeField] private GameObject newsObject;
        [SerializeField] private GameObject newsObjectParent;
        [SerializeField] private CheckWindowElement checkWindowElement;
        [SerializeField] private FancyScrollView.FRONTIER.Cell cell;
        [SerializeField] private MenuManager menuManager;
        private const float MIN_NOTE_SPEED = 1.0f;
        private const float MAX_NOTE_SPEED = 14.9f;
        private const float MIN_JUDGING_TIMING = -2.0f;
        private const float MAX_JUDGING_TIMING = 2.0f;

        void OnEnable()
        {
            Init();
            NotificationWindowSetUp(newsObjectParent);
        }

        // Update is called once per frame
        void Update()
        {
            Reflect();
            Reflect(checkWindowElement);
        }

        ///<summary>各種表示テキストの初期化を行います。</summary>
        private void Init()
        {
            settingElement.noteSpeed.text = SettingData.Instance.setting.noteSpeed.ToString("f1");
            settingElement.timing.text = SettingData.Instance.setting.timing.ToString("f1");
            settingElement.mirror.isOn = SettingData.Instance.setting.mirror;
            settingElement.volumeMusic.value = SettingData.Instance.setting.musicVolume;
            settingElement.volumeSE.value = SettingData.Instance.setting.seVolume;
        }

        ///<summary>各種UIで調整した値を基底のデータに反映させる処理を行います。</summary>
        private void Reflect()
        {
            SettingData.Instance.setting.mirror = settingElement.mirror.isOn;
            SettingData.Instance.setting.musicVolume = (int)settingElement.volumeMusic.value;
            SettingData.Instance.setting.seVolume = (int)settingElement.volumeSE.value;
            Init();
        }
        ///<summary>チェックウィンドウ専用のオーバーロード。</summary>
        private void Reflect(CheckWindowElement check)
        {
            check.songTitle.text = MenuInfo.menuInfo.Name;
            check.difficulty.text = MenuInfo.menuInfo.DifficultyTo().Item1;
            check.level.text = MenuInfo.menuInfo.Level;
            check.backGround.color = MenuInfo.menuInfo.DifficultyColor;
            check.difficultyHighlight.color = new Color32(MenuInfo.menuInfo.DifficultyColor.r, MenuInfo.menuInfo.DifficultyColor.g, MenuInfo.menuInfo.DifficultyColor.b, 225);
            check.cover.sprite = MenuInfo.menuInfo.Cover;
            MenuInfo.menuInfo.autoPlay = check.autoPlay.isOn;
            MenuInfo.menuInfo.mv = check.mv.isOn;
        }

        ///<summary>ノーツスピードを調整するボタンを有効にするメソッドです。</summary>
        ///<param name = "buttonIndex">ボタンの順序(0 ~ 3)</param>
        public void SpeedButtonPressed(int buttonIndex)
        {
            void ChangeValue(int buttonIndex)
            {
                switch (buttonIndex)
                {
                    case 0:
                        SettingData.Instance.setting.noteSpeed -= 1.0f;
                        SettingData.Instance.setting.noteSpeed = SettingData.Instance.setting.noteSpeed < MIN_NOTE_SPEED ? MIN_NOTE_SPEED : SettingData.Instance.setting.noteSpeed;
                        SettingData.Instance.setting.noteSpeed = SettingData.Instance.setting.noteSpeed > MAX_NOTE_SPEED ? MAX_NOTE_SPEED : SettingData.Instance.setting.noteSpeed;
                        break;
                    case 1:
                        SettingData.Instance.setting.noteSpeed -= 0.1f;
                        SettingData.Instance.setting.noteSpeed = SettingData.Instance.setting.noteSpeed < MIN_NOTE_SPEED ? MIN_NOTE_SPEED : SettingData.Instance.setting.noteSpeed;
                        SettingData.Instance.setting.noteSpeed = SettingData.Instance.setting.noteSpeed > MAX_NOTE_SPEED ? MAX_NOTE_SPEED : SettingData.Instance.setting.noteSpeed;
                        break;
                    case 2:
                        SettingData.Instance.setting.noteSpeed += 0.1f;
                        SettingData.Instance.setting.noteSpeed = SettingData.Instance.setting.noteSpeed < MIN_NOTE_SPEED ? MIN_NOTE_SPEED : SettingData.Instance.setting.noteSpeed;
                        SettingData.Instance.setting.noteSpeed = SettingData.Instance.setting.noteSpeed > MAX_NOTE_SPEED ? MAX_NOTE_SPEED : SettingData.Instance.setting.noteSpeed;
                        break;
                    case 3:
                        SettingData.Instance.setting.noteSpeed += 1.0f;
                        SettingData.Instance.setting.noteSpeed = SettingData.Instance.setting.noteSpeed < MIN_NOTE_SPEED ? MIN_NOTE_SPEED : SettingData.Instance.setting.noteSpeed;
                        SettingData.Instance.setting.noteSpeed = SettingData.Instance.setting.noteSpeed > MAX_NOTE_SPEED ? MAX_NOTE_SPEED : SettingData.Instance.setting.noteSpeed;
                        break;
                }
            }
            ChangeValue(buttonIndex);
        }

        ///<summary>判定タイミングを調整するボタンを有効にするメソッドです。</summary>
        ///<param name = "buttonIndex">ボタンの順序(0 ~ 3)</param>
        public void TimingButtonPressed(int buttonIndex)
        {
            void ChangeValue(int buttonIndex)
            {
                switch (buttonIndex)
                {
                    case 0:
                        SettingData.Instance.setting.timing -= 1.0f;
                        SettingData.Instance.setting.timing = SettingData.Instance.setting.timing < MIN_JUDGING_TIMING ? MIN_JUDGING_TIMING : SettingData.Instance.setting.timing;
                        SettingData.Instance.setting.timing = SettingData.Instance.setting.timing > MAX_JUDGING_TIMING ? MAX_JUDGING_TIMING : SettingData.Instance.setting.timing;
                        break;
                    case 1:
                        SettingData.Instance.setting.timing -= 0.1f;
                        SettingData.Instance.setting.timing = SettingData.Instance.setting.timing < MIN_JUDGING_TIMING ? MIN_JUDGING_TIMING : SettingData.Instance.setting.timing;
                        SettingData.Instance.setting.timing = SettingData.Instance.setting.timing > MAX_JUDGING_TIMING ? MAX_JUDGING_TIMING : SettingData.Instance.setting.timing;
                        break;
                    case 2:
                        SettingData.Instance.setting.timing += 0.1f;
                        SettingData.Instance.setting.timing = SettingData.Instance.setting.timing < MIN_JUDGING_TIMING ? MIN_JUDGING_TIMING : SettingData.Instance.setting.timing;
                        SettingData.Instance.setting.timing = SettingData.Instance.setting.timing > MAX_JUDGING_TIMING ? MAX_JUDGING_TIMING : SettingData.Instance.setting.timing;
                        break;
                    case 3:
                        SettingData.Instance.setting.timing += 1.0f;
                        SettingData.Instance.setting.timing = SettingData.Instance.setting.timing < MIN_JUDGING_TIMING ? MIN_JUDGING_TIMING : SettingData.Instance.setting.timing;
                        SettingData.Instance.setting.timing = SettingData.Instance.setting.timing > MAX_JUDGING_TIMING ? MAX_JUDGING_TIMING : SettingData.Instance.setting.timing;
                        break;
                }
            }
            ChangeValue(buttonIndex);
        }

        ///<summary>
        ///モーダルウィンドウのオンオフを切り替えます。
        ///</summary>
        ///<param name = "window">オンオフを切り替えたいゲームオブジェクト。</param>
        ///<param name = "isOpen">対象のオブジェクトに反映させるアクティブ状態。<see cref="true"/>か<see cref="false"/>を指定</param>
        private void OpenWindow(GameObject window, bool isOpen)
        {
            window.SetActive(isOpen);
        }

        /**<summary>通知ウィンドウの初期化をします。</summary>
            <remarks>主にウィンドウ上に整列されるブロックを生成します。</remarks>
            <param name = "parent">テキストを書いた通知ブロックを配置させる親オブジェクト。</param>
        **/
        private void NotificationWindowSetUp(GameObject parent)
        {
            //Debug.Log($"{texts.Count}, {texts[0].name}, {texts[1].name}");
            for (int i = 0; i < NotificationData.Instance.notification.Length; i++)
            {
                GameObject info = Instantiate(newsObject);
                info.transform.SetParent(parent.transform);
                RectTransform infotrans = info.GetComponent<RectTransform>();
                info.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(infotrans.anchoredPosition3D.x, infotrans.anchoredPosition3D.y, 0);
                info.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
                List<Transform> texts = new List<Transform>(info.GetComponentsInChildren<Transform>());    //親と子を含めたオブジェクトを取得。これはTextMeshProを取得するため。
                texts.RemoveAt(0);  //親を除外。
                texts[0].GetComponent<TextMeshProUGUI>().text = NotificationData.Instance.notification[i].title;
                texts[1].GetComponent<TextMeshProUGUI>().text = NotificationData.Instance.notification[i].p;
            }
        }
        /* 以下、ボタンコンポーネントにアタッチして使うメソッドです。 */

        public void OpenSetting()
        {
            settingWindow.GetComponent<Animator>().SetBool("isOpen", true);
        }
        public void CloseSetting()
        {
            //menuManager.SaveSetting(Application.persistentDataPath);
            settingWindow.GetComponent<Animator>().SetBool("isOpen", false);
        }
        public void OpenNotification()
        {
            notificationWindow.GetComponent<Animator>().SetBool("isOpen", true);
        }
        public void CloseNotification()
        {
            notificationWindow.GetComponent<Animator>().SetBool("isOpen", false);
        }
        public void CheckWindowOpened()
        {
            checkWindowElement.songTitle.text = MenuInfo.menuInfo.Name;
            checkWindowElement.difficulty.text = MenuInfo.menuInfo.DifficultyTo().Item1;
            checkWindowElement.level.text = MenuInfo.menuInfo.Level;
            checkWindowElement.backGround.color = MenuInfo.menuInfo.DifficultyColor;
            checkWindowElement.difficultyHighlight.color = new Color32(MenuInfo.menuInfo.DifficultyColor.r, MenuInfo.menuInfo.DifficultyColor.g, MenuInfo.menuInfo.DifficultyColor.b, 225);
            checkWindowElement.cover.sprite = MenuInfo.menuInfo.Cover;
            checkWindowElement.checkWindow.GetComponent<Animator>().SetBool("isActive", true);
            checkWindowElement.checkWindow.GetComponent<CLScrollTextInitialize>().UpdateTextCondition();
        }
        public void CheckWindowClosed()
        {
            checkWindowElement.checkWindow.GetComponent<Animator>().SetBool("isActive", false);
            checkWindowElement.checkWindow.GetComponent<CLScrollTextInitialize>().UpdateTextCondition(false);
            cell.CellReturn();
        }

        public void PlayGameScene()
        {
            SceneNavigator.instance.FadeOutFinished += () => GameManager.instance.OnSceneLoaded(FRONTIER.Utility.Reference.Scene.GameScenes.Game);
            SceneNavigator.instance.ChangeScene("Game", 1.5f, ignoreTimeScale: true);
        }
    }

    [System.Serializable]
    internal class SettingElement
    {
        [SerializeField] public TextMeshProUGUI noteSpeed = default;
        //[SerializeField] public Button[] changeSpeedValue = new Button[4];
        [SerializeField] public TextMeshProUGUI timing = default;
        //[SerializeField] public Button[] changeTimingValue = new Button[4];
        [SerializeField] public Toggle mirror = default;
        [SerializeField] public Slider volumeMusic = default;
        [SerializeField] public Slider volumeSE = default;
    }

    [System.Serializable]
    internal class CheckWindowElement
    {
        [SerializeField] public GameObject checkWindow;
        [SerializeField] public Text songTitle;
        [SerializeField] public Image backGround;
        [SerializeField] public TextMeshProUGUI difficulty;
        [SerializeField] public Image difficultyHighlight;
        [SerializeField] public TextMeshProUGUI level;
        [SerializeField] public Image cover;
        [SerializeField] public Button settingOpen;
        [SerializeField] public Toggle autoPlay;
        [SerializeField] public Toggle mv;
    }
}
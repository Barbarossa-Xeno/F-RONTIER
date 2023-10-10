using UnityEngine;
using UnityEngine.UI;
using UniRx;
using FRONTIER.Utility;

namespace FRONTIER.Result
{
    /// <summary>
    /// リザルトシーンを管理する。
    /// </summary>
    public class ResultManager : GameUtility
    {
        [SerializeField] private ResultWindows resultWindows;
        [SerializeField] private Button screen;
    
        void Start()
        {
            resultWindows.Construct();
            screen.onClick.AddListener(Manager.scene.menu.Invoke);            
        }
    }
}



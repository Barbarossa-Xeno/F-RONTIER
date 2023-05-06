using System.Collections;
using UnityEngine;
using Game.Utility;

///<summary>ノーツの状態を管理します。</summary>
public class Notes : MonoBehaviour
{
    private float speed;
    ///<summary>種類。</summary>
    public SettingUtility.NoteType type;
    ///<summary>種類がロングノーツであったとき持つ情報。</summary>
    public LongNote longNote;
    [System.Serializable]
    public class LongNote
    {
        ///<summary>ロングノーツとしてのステータス。</summary>
        [SerializeField] public SettingUtility.LongNoteStatus status;
        ///<summary>譜面で何番目のロングノーツか。</summary>
        [SerializeField] public int index;
        ///<summary>中間点があるか。</summary>
        [SerializeField] public bool isInner;
        public LongNote()
        {
            status = SettingUtility.LongNoteStatus.None;
            index = -1;
        }
    }

    void Start()
    {
        if (type == SettingUtility.NoteType.Normal) { longNote = new LongNote(); }
        speed = GameManager.instance.NoteSpeed;
    }

    void Update()
    { if (GameManager.instance.gamePlayState == GameManager.GamePlayState.Playing) { this.transform.position -= new Vector3(0, 0, speed) * Time.deltaTime; } }
}

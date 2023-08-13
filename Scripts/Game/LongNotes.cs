using System;
using UnityEngine;
using UnityEngine.EventSystems;

///<summary>ロングノーツ</summary>
public class LongNotes : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    /* フィールド */
    ///<summary>ロングノーツが押下されているか。</summary>
    public bool isPressed = false;
    ///<summary>ロングノーツが押下されているときに発火するイベント。</summary>
    public event Action LongNoteOn = delegate { };
    ///<summary>ロングノーツが押下されていないときに発火するイベント。</summary>
    public event Action LongNoteOff = delegate { };
    ///<summary>Lノーツのメッシュのマテリアル。（シェーダーの設定を含む）</summary>
    private Material meshMaterial = default;
    ///<summary>このオブジェクトの子オブジェクト。</summary>
    public GameObject[] children;

    /* メソッド */
    void Start()
    {
        //このオブジェクトに関連付けされた子オブジェクト(Lノーツの断片)の数を長さとする配列を作成する。
        children = new GameObject[this.transform.childCount];
        //もし子オブジェクトがない ＝ 中間点がないLノーツの場合、自分のレンダラーからマテリアルを取得する。
        if (children.Length == 0) { meshMaterial = this.GetComponent<Renderer>().material; }
        //子オブジェクトがある ＝ 中間点があるLノーツの場合、子オブジェクトを取得する。
        for (int i = 0; i < children.Length; i++) { children[i] = this.transform.GetChild(i).gameObject; }
        //押下時イベントの登録。
        LongNoteOn += () =>
        {
            //中間点がある
            if (children.Length > 0)
            {
                for (int i = 0; i < children.Length; i++)
                {
                    children[i].GetComponent<MeshRenderer>().material.SetFloat("_isPressed", 1);
                    try
                    {
                        children[i].GetComponent<LineRenderer>().material.SetFloat("_isPressed", 1);
                    }
                    catch (System.NullReferenceException) { Game.Development.DevelopmentExtentionMethods.Log("ラインレンダラーが取得できませんでした"); }
                    catch (MissingComponentException) 
                    { 
                        Game.Development.DevelopmentExtentionMethods.Log("空のロングノーツが生成されました。"); 
                        Destroy(this.gameObject);
                    }
                }
            }
            //中間点がない
            else { meshMaterial.SetFloat("_isPressed", 1); }
        };
        //解放時イベントの登録。
        LongNoteOff += () =>
        {
            if (children.Length > 0)
            {
                for (int i = 0; i < children.Length; i++)
                {
                    children[i].GetComponent<MeshRenderer>().material.SetFloat("_isPressed", 0);
                    try
                    {
                        children[i].GetComponent<LineRenderer>().material.SetFloat("_isPressed", 0);
                    }
                    catch (System.NullReferenceException) { }
                }
            }
            else { meshMaterial.SetFloat("_isPressed", 0); }
        };
    }

    ///<summary>ロングノーツが押下されたときに行う処理。</summary>
    ///<param name = "flag">押下の有無のフラグ。</param>
    private void Pressing(bool flag)
    {
        if (!GameManager.instance.autoPlay)
        {
            isPressed = flag;
            if (flag) { LongNoteOn(); }
            else { LongNoteOff(); }
        }
    }

    public void OnPointerDown(PointerEventData pointerDownEvent) => Pressing(true);
    public void OnPointerEnter(PointerEventData pointerEnterEvent) => Pressing(true);
    public void OnPointerUp(PointerEventData pointerUpEvent) => Pressing(false);
    public void OnPointerExit(PointerEventData pointerExitEvent) => Pressing(false);

    void Update() { if(GameManager.instance.autoPlay) { LongNoteOn(); } }
}
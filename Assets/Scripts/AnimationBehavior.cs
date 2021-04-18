//動畫物件的行為

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AnimationBehavior : MonoBehaviour
{
    //-----------變數宣告------------------------------------------------
    //[HideInInspector]
    public float movingvariable; //移動變數(0 ~ 100)
    private bool movingSensor; //是否開啟移動狀態
    private Vector2 originPos; //原位
    private Vector2 targetPos; //目標位置

    //-----------內建方法------------------------------------------------
    void Update()
    {
        if (movingSensor) MovingByCurve();
    }

    //-----------自訂方法------------------------------------------------
    //顯示或隱藏
    public void SetActive()
    {
        this.gameObject.SetActive(!this.gameObject.activeSelf);
    }

    //改變Tag
    public void ChangeTag()
    {
        this.gameObject.tag = "Untagged";
    }

    //特效鎖開
    public void AnimationLock_TurnOn()
    {
        StaticScript.pause = true;
    }

    //特效鎖關
    public void AnimationLock_TurnOff()
    {
        StaticScript.pause = false;
    }

    //讀取關卡
    public void LoadScene(string name)
    {
        SceneManager.LoadScene(name);
    }

    //開啟Raycast Target
    public void RaacastTarget_TurnOn()
    {
        this.GetComponent<Image>().raycastTarget = true;
        if (this.GetComponentInChildren<Text>() != null) this.GetComponentInChildren<Text>().raycastTarget = true;
    }

    //關閉Raycast Target
    public void RaacastTarget_TurnOff()
    {
        this.GetComponent<Image>().raycastTarget = false;
        if (this.GetComponentInChildren<Text>() != null) this.GetComponentInChildren<Text>().raycastTarget = false;
    }

    //消除自身
    public void DestroySelf()
    {
        Destroy(this.gameObject);
    }

    //關閉Animator
    public void DisableAnimator()
    {
        this.GetComponent<Animator>().enabled = false;
        Collection.s_enlarging = false;
    }

    //移動路徑設定
    public void SetLocus(Vector2 o, Vector2 t)
    {
        originPos = o;
        targetPos = t;
        movingvariable = 0;
        movingSensor = true;
    }

    //透過變數移動位置
    public void MovingByCurve()
    {
        float x = originPos.x + ((targetPos.x - originPos.x) * (Mathf.Clamp(movingvariable, 0, 100) / 100.0f));
        float y = originPos.y + ((targetPos.y - originPos.y) * (Mathf.Clamp(movingvariable, 0, 100) / 100.0f));
        this.transform.localPosition = new Vector2(x, y);
        if (movingvariable >= 100) movingSensor = false;
    }
}

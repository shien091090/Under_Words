//主選單的收集品(碎片)行為與特效

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Collection : MonoBehaviour
{
    public static bool s_enlarging; //放大
    public Vector2 exitPos; //退幕位置
    public Vector2 originPos; //原始位置

    void Start()
    {
        originPos = this.transform.localPosition;
    }

    //鼠標停滯時
    public void Focused()
    {
        if (!UIBehaviour.Instance.refObj.collectionFrame.activeSelf && !s_enlarging) UIBehaviour.Instance.refObj.collectionFrame.SetActive(true);
        if (UIBehaviour.Instance.refObj.collectionFrame.transform.position != this.transform.position && !s_enlarging) UIBehaviour.Instance.refObj.collectionFrame.transform.position = this.transform.position;
    }

    //鼠標移出時
    public void MouseExit()
    {
        if (UIBehaviour.Instance.refObj.collectionFrame.activeSelf) UIBehaviour.Instance.refObj.collectionFrame.SetActive(false);
    }

    //點擊左鍵時
    public void Resizing()
    {
        Animator animator = this.GetComponent<Animator>();

        AudioManagerScript.Instance.PlayAudioClip("UI_BTN1");

        if (!s_enlarging) //點擊左鍵(放大)
        {
            s_enlarging = true; //放大狀態
            UIBehaviour.Instance.refObj.collectionFrame.SetActive(false); //消除選擇框光圈

            for (int i = 0; i < UIBehaviour.Instance.refObj.chipIcons.Length; i++) //其他碎片Icon移出
            {
                Animator a = UIBehaviour.Instance.refObj.chipIcons[i].GetComponent<Animator>();

                if (UIBehaviour.Instance.refObj.chipIcons[i] != this.gameObject)
                {
                    UIBehaviour.Instance.refObj.chipIcons[i].GetComponent<AnimationBehavior>().SetLocus(UIBehaviour.Instance.refObj.chipIcons[i].transform.localPosition, UIBehaviour.Instance.refObj.chipIcons[i].GetComponent<Collection>().exitPos);
                    a.enabled = true;
                    a.Play("ExitScreen", 0, 0);
                }
            }

            animator.enabled = true;
            this.GetComponent<AnimationBehavior>().SetLocus(this.transform.localPosition, new Vector2(0, 0));
            animator.Play("Enlarge", 0, 0); //點選到的Icon放大特效
            UIBehaviour.Instance.anim.collection_back.enabled = true;
            UIBehaviour.Instance.anim.collection_back.Play("ExitScreen", 0, 0); //返回鈕淡出特效
        }
        else //點擊左鍵(縮小)
        {
            for (int i = 0; i < UIBehaviour.Instance.refObj.chipIcons.Length; i++) //其他碎片Icon移入
            {
                Animator a = UIBehaviour.Instance.refObj.chipIcons[i].GetComponent<Animator>();

                if (UIBehaviour.Instance.refObj.chipIcons[i] != this.gameObject)
                {
                    UIBehaviour.Instance.refObj.chipIcons[i].GetComponent<AnimationBehavior>().SetLocus(UIBehaviour.Instance.refObj.chipIcons[i].transform.localPosition, UIBehaviour.Instance.refObj.chipIcons[i].GetComponent<Collection>().originPos);
                    a.enabled = true;
                    a.Play("EnterScreen", 0, 0);
                }
            }

            this.GetComponent<AnimationBehavior>().SetLocus(this.transform.localPosition, originPos);
            animator.Play("Lessen", 0, 0); //點選到的Icon縮小特效
            UIBehaviour.Instance.anim.collection_back.Play("EnterScreen", 0, 0); //返回鈕淡入特效
        }
    }

    //變更raycastTarget狀態
    public void ChangeRayCastTarget()
    {
        this.GetComponent<Image>().raycastTarget = !this.GetComponent<Image>().raycastTarget;
    }
}

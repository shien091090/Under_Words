//磚塊的控制腳本

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class BrickBehavior : MonoBehaviour
{
    //-----------變數宣告------------------------------------------------
    public static int s_positiveWords; //正面詞語總數
    public static int s_pCount; //目前正面詞語數
    public static int s_negativeWords; //負面詞語總數
    public static int s_nCount; //目前負面詞語數
    public static int s_colorAmount; //色彩總數
    public static int s_colorRemain; //剩餘色彩數
    public static bool sensor_color = false; //訊號：色彩出現
    public bool type; // true = 正面詞語 / false = 負面詞語
    public bool crash = false; //擊碎與否
    public bool buryingColor = false; //是否埋藏色彩
    public Text content; //內涵文字
    public Animator animator; //動畫機
    public GameObject colorPrefab; //顏色Prefab

    private RectTransform rectTransform;
    private BoxCollider2D boxCollider;
    //public static int[] colorBrickIndex = new int[0]; //藏有色彩的磚塊編號
    //private static string[] colorTypes = new string[0]; //色彩種類集合

    //-----------內建方法------------------------------------------------
    void Awake()
    {
        rectTransform = this.GetComponent<RectTransform>();
        boxCollider = this.GetComponent<BoxCollider2D>();
    }

    void Start()
    {
        if (type) //正面磚塊
        {
            int dice = UnityEngine.Random.Range(0, (int)Positive_tutorial.LengthNumber);
            content.text = Enum.GetName(typeof(Positive_tutorial), dice); //隨機內涵文字

        }
        else //負面磚塊
        {
            int dice = UnityEngine.Random.Range(0, (int)Negative_tutorial.LengthNumber);
            content.text = Enum.GetName(typeof(Negative_tutorial), dice); //隨機內涵文字

        }

        boxCollider.size = rectTransform.sizeDelta; //碰撞區域
    }

    //-----------自訂方法------------------------------------------------

    public void Smash() //擊毀磚塊
    {
        if (!crash)
        {
            crash = true;

            if (type)
            {

                s_pCount--; //更新目前剩餘磚塊數
                UIBehaviour.Instance.ProgressUpdate(true); //更新磚塊擊破進度
                UIBehaviour.Instance.anim.progressPanel.Play("UI_ProgressPanel_PFlash", 0, 0);
            }
            else
            {
                if (buryingColor)
                {
                    Transform colorHolder = GameObject.Find("Canvas_50_Main/Color Holder").GetComponent<Transform>(); //設定顏色預置體之父物件
                    Transform targetTransform = GameObject.Find("Canvas_50_Main/ColorTarget").GetComponent<Transform>(); //設定顏色預置體之位移目標位置

                    GameObject color = Instantiate(colorPrefab, this.transform.position, Quaternion.identity, colorHolder); //產生預置體
                    int rdm = UnityEngine.Random.Range(0, (int)ColorType.LengthNumber);
                    color.GetComponent<ColorEffect>().SetParameter(System.Enum.GetName(typeof(ColorType), rdm), targetTransform); //設定參數

                    AudioManagerScript.Instance.PlayAudioClip("SE_GETCOLOR");

                    sensor_color = true;
                }
                s_nCount--; //更新目前剩餘磚塊數
            }
        }

        animator.Play("Smash", 0, 0);
    }

    public static void ColorObjective(int amount) //埋藏顏色
    {
        s_colorRemain = amount;
        s_colorAmount = amount; //色彩總數

        //抓出有效編號
        List<int> validNumbers = new List<int>();
        for (int j = 0; j < GameController.Instance.negativeBricks.Count; j++)
        {
            if (GameController.Instance.negativeBricks[j] != null)
            {
                validNumbers.Add(j);
            }
        }

        //設定埋藏顏色的磚塊編號
        for (int i = 0; i < amount; i++)
        {
            int dice = UnityEngine.Random.Range(0, validNumbers.Count);
            //Debug.Log("[" + i + "] : " + validNumbers[dice]);
            GameController.Instance.negativeBricks[validNumbers[dice]].GetComponent<BrickBehavior>().buryingColor = true;
            validNumbers.Remove(validNumbers[dice]);
        }

    }
}

//色彩Prefab的移動特效

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorEffect : MonoBehaviour
{
    //-----------變數宣告------------------------------------------------
    public AnimationCurve movingCurve; //動作曲線
    //public Animator animator; //動畫機
    public Image image;
    public Sprite[] colorList; //色彩圖集

    private Vector3 origin; //原點
    private Vector3 target; //目標位置
    private float time; //計時器
    private Vector2 unit; //移動單位
    public bool enable; //可否移動

    //-----------內建方法------------------------------------------------

    void Start()
    {
        time = 0; //時間歸零
    }

    void Update()
    {
        if (enable)
        {
            this.transform.position = new Vector3(origin.x + (unit * movingCurve.Evaluate(time)).x, origin.y + (unit * movingCurve.Evaluate(time)).y, origin.z); //更新位置
            time += Time.deltaTime; //計時器行進
            if (time >= movingCurve.keys[movingCurve.length - 1].time)
            {
                enable = false;
                BrickBehavior.s_colorRemain--; //色彩剩餘數更新
                UIBehaviour.Instance.ProgressUpdate(false); //更新磚塊擊破進度
                UIBehaviour.Instance.anim.progressPanel.Play("UI_ProgressPanel_NFlash", 0, 0);
                Destroy(this.gameObject);
            }
        }

    }

    //-----------自訂方法------------------------------------------------

    public void SetParameter(string colorName, Transform targetTransform) //設定參數(顏色種類, 目標點)
    {
        origin = this.transform.position; //取原點
        target = targetTransform.position; //取目標點
        unit = new Vector2((target.x - origin.x) / movingCurve.keys[movingCurve.length - 1].value, (target.y - origin.y) / movingCurve.keys[movingCurve.length - 1].value); //取移動單位
        int index = (int)System.Enum.Parse(typeof(ColorType), colorName); //colorName轉Enum列舉值索引

        image.sprite = colorList[index]; //指定圖
        enable = true;
    }

}

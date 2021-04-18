//玩家橫條的控制腳本

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBarBehavior : MonoBehaviour
{
    //-----------變數宣告------------------------------------------------
    private static PlayerBarBehavior _instance; //單例物件
    public static PlayerBarBehavior Instance
    {
        get
        {
            return _instance;
        }
    }

    public float moveSpeed; //移動速度
    public float dragAcc; //拖曳加速度
    public float dragSpeed; //拖曳加成速度
    public List<GameObject> adhereBalls; //附著球列表

    private RectTransform rectTransform;
    private float stretchLength; //延伸長度
    private float bufferRange; //緩衝範圍(在更靠近牆時做精準判斷,遠離時則不做判斷,節省資源用)
    private float wallPos_right; //右牆位置
    private float wallPos_left; //左牆位置
    private bool rightLimit = false; //是否達右方盡頭
    private bool leftLimit = false; //是否達左方盡頭

    //-----------內建方法------------------------------------------------
    void Awake()
    {
        _instance = this; //單例模式
    }

    void Start()
    {
        rectTransform = this.GetComponent<RectTransform>();
        stretchLength = this.transform.parent.localScale.x * (rectTransform.sizeDelta.x / 2);
    }

    //-----------自訂方法------------------------------------------------

    //定位
    public void Locate(GameObject[] objs)
    {
        wallPos_right = objs[0].transform.position.x;
        wallPos_left = objs[1].transform.position.x;
        bufferRange = (objs[0].GetComponent<RectTransform>().sizeDelta.x / 2) + (moveSpeed / 10);

        //switch (name)
        //{
        //    case "wallPos_right":
        //        wallPos_right = (float)value;
        //        break;

        //    case "wallPos_left":
        //        wallPos_left = (float)value;
        //        break;

        //    case "bufferRange":
        //        bufferRange = (float)value;
        //        break;

        //    default:
        //        break;
        //}
    }

    //移動
    public void Move(string dir)
    {
        switch (dir)
        {
            case "右":
                if (!rightLimit) //若達右方盡頭則不再移動
                {
                    if (leftLimit) //解除左方盡頭狀態
                    {
                        leftLimit = false;
                    }

                    if (rectTransform.position.x + stretchLength <= wallPos_right - bufferRange) //在緩衝區外時簡單判定(效能取向)
                    {
                        this.transform.Translate(Vector3.right * Time.deltaTime * moveSpeed);
                        for (int i = 0; i < adhereBalls.Count; i++) //拖曳附著的球移動
                        {
                            adhereBalls[i].transform.position += Vector3.right * Time.deltaTime * moveSpeed;
                        }
                    }
                    else
                    {
                        float origin = rectTransform.position.x + stretchLength;
                        if (rectTransform.position.x + stretchLength + (Vector3.right * Time.deltaTime * moveSpeed).x >= wallPos_right) //在緩衝區內精確判定(效果取向)
                        {
                            this.transform.position = new Vector3(wallPos_right - stretchLength, this.transform.position.y, this.transform.position.z);
                            for (int i = 0; i < adhereBalls.Count; i++) //拖曳附著的球移動(精確計算)
                            {
                                adhereBalls[i].transform.position += Vector3.right * (wallPos_right - origin);
                            }
                            rightLimit = true;
                        }
                        else
                        {
                            this.transform.Translate(Vector3.right * Time.deltaTime * moveSpeed);
                            for (int i = 0; i < adhereBalls.Count; i++) //拖曳附著的球移動
                            {
                                adhereBalls[i].transform.position += Vector3.right * Time.deltaTime * moveSpeed;
                            }
                        }
                    }
                }

                break;

            case "左":
                if (!leftLimit) //若達左方盡頭則不再移動
                {
                    if (rightLimit) //解除右方盡頭狀態
                    {
                        rightLimit = false;
                    }

                    if (rectTransform.position.x - stretchLength >= wallPos_left + bufferRange) //在緩衝區外時簡單判定(效能取向)
                    {
                        this.transform.Translate(-Vector3.right * Time.deltaTime * moveSpeed);
                        for (int i = 0; i < adhereBalls.Count; i++) //拖曳附著的球移動
                        {
                            adhereBalls[i].transform.position += -Vector3.right * Time.deltaTime * moveSpeed;
                        }
                    }
                    else
                    {
                        float origin = rectTransform.position.x - stretchLength;
                        if (rectTransform.position.x - stretchLength - (Vector3.right * Time.deltaTime * moveSpeed).x <= wallPos_left) //在緩衝區內精確判定(效果取向)
                        {
                            this.transform.position = new Vector3(wallPos_left + stretchLength, this.transform.position.y, this.transform.position.z);
                            for (int i = 0; i < adhereBalls.Count; i++) //拖曳附著的球移動(精確計算)
                            {
                                adhereBalls[i].transform.position += Vector3.right * (wallPos_left - origin);
                            }
                            leftLimit = true;
                        }
                        else
                        {
                            this.transform.Translate(-Vector3.right * Time.deltaTime * moveSpeed);
                            for (int i = 0; i < adhereBalls.Count; i++) //拖曳附著的球移動
                            {
                                adhereBalls[i].transform.position += -Vector3.right * Time.deltaTime * moveSpeed;
                            }
                        }
                    }
                }

                break;
        }
    }

    //速度加成計算
    public void AccelerationEffect(float v)
    {
        if ((v > 0 && rightLimit) || (v < 0 && leftLimit))
        {
            dragSpeed = 0;
        }
        else
        {
            dragSpeed = dragAcc * v;
        }
    }

    //附著
    public void Adhere(ref GameObject b)
    {
        b.GetComponent<BallBehavior>().staying = true;
        adhereBalls.Add(b);
    }

    //擊發(多載1/2)
    public void Shoot()
    {
        adhereBalls[adhereBalls.Count - 1].SendMessage("InitialMove", new Vector2(dragSpeed == 0 ? 0 : (dragSpeed > 0 ? moveSpeed : -moveSpeed), 0));
        adhereBalls.RemoveAt(adhereBalls.Count - 1);
    }

    //擊發(多載2/2)
    public void Shoot(Vector2 v2)
    {
        adhereBalls[adhereBalls.Count - 1].SendMessage("InitialMove", v2);
        adhereBalls.RemoveAt(adhereBalls.Count - 1);
    }

    //全部擊發
    public void ShootAll(Vector2 v2)
    {
        for (int i = 0; i < adhereBalls.Count; i++)
        {
            adhereBalls[i].SendMessage("InitialMove", v2);
        }
        adhereBalls.RemoveRange(0, adhereBalls.Count);
    }
}

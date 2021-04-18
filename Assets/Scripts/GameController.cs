//遊戲邏輯控制腳本

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class GameController : MonoBehaviour
{
    //-----------變數宣告------------------------------------------------
    private static GameController _instance; //單例物件
    public static GameController Instance
    {
        get
        {
            return _instance;
        }
    }

    [Header("編輯器面板")]
    public bool debugTest = true; //是否開啟測試用按鍵
    public bool shootLock = true; //擊發鎖
    public Paragraph nowLibretto; //目前章節
    public Serif nowDialogue; //目前對話
    public List<string> listeningConditions = new List<string>(); //監聽中條件
    [Header("內部參數")]
    public List<GameObject> positiveBricks; //正面磚塊群
    public List<GameObject> negativeBricks; //負面磚塊群
    public byte fastForward; //快進倍率
    public float speechSpeed; //對話顯示速度
    public float speechOverDelay; //台詞結束後延遲
    public float chipColorChangeTime; //碎片顏色變換時間
    public short sensorBasicSpacing; //燈號基本間隔
    public short sensorSpacingUnit; //燈號間隔單位
    public int tut_fallCount = -1; //教程_掉落次數
    public string playBgmTag = ""; //BGM撥放標籤
    [Header("難易度參數")]
    public bool hpActive = false; //血條控球條件監聽
    public bool isReversing = false; //是否開啟條件倒轉
    public bool reversingState = false; //倒轉狀態
    public float listenFrequency; //控球條件監聽頻率
    public sbyte hpScales; //血條傾向值
    public float hpCoefficient; //HP變化速度係數
    public float recoverySpeed; //恢復速度
    public float damageSpeed; //損傷速度
    public float reverseFrequency; //倒轉頻率

    private List<GameObject> ballsPool = new List<GameObject>(); //球物件池
    private List<GameObject> spaceBricks = new List<GameObject>(); //空磚塊
    private bool FadeOutMark; //段落更換標記(對話框淡出)
    private float tut_moveCount = -1; //教程_移動量
    private int lineSite; //對話句數
    private object[] parArray; //控球條件參數組
    private SpeechBalloonInfo nearlyType; //對話框樣式(近)
    private SpeechBalloonInfo farType; //對話框樣式(遠)
    private SpeechBalloonInfo seletionType; //對話框樣式(選項)

    [System.Serializable]
    public struct Prefabs //預置體物件群
    {
        public Transform ballHolder; //父區域
        public GameObject[] balls; //各種球
        public Transform pBricksHolder; //正面磚塊父區域
        public Transform nBricksHolder; //負面磚塊父區域
        public GameObject brick_space; //空磚塊
    }
    [Header("預置體參考")]
    public Prefabs pref;

    //-----------內建方法------------------------------------------------
    void Awake()
    {
        _instance = this; //單例模式
        fastForward = fastForward <= 1 ? (byte)1 : fastForward; //快進倍率最低為1
    }

    void Start()
    {
        //StaticScript.stageNumber = 3;

        //隱藏物件
        UIBehaviour.Instance.refObj.wall_up.SetActive(false);
        UIBehaviour.Instance.refObj.wall_down.SetActive(false);
        UIBehaviour.Instance.refObj.wall_right.SetActive(false);
        UIBehaviour.Instance.refObj.wall_left.SetActive(false);
        UIBehaviour.Instance.refObj.playerBar.SetActive(false);
        UIBehaviour.Instance.refObj.PBricks.SetActive(false);
        UIBehaviour.Instance.refObj.NBricks.SetActive(false);
        UIBehaviour.Instance.refObj.botBG.SetActive(false);
        UIBehaviour.Instance.refObj.progressPanel.SetActive(false);
        UIBehaviour.Instance.refObj.btn_backToStartMenu.SetActive(false);
        UIBehaviour.Instance.refObj.title.SetActive(false);
        UIBehaviour.Instance.refObj.cdPanelMask.SetActive(true);
        UIBehaviour.Instance.refObj.PMask.SetActive(true);
        UIBehaviour.Instance.refObj.NMask.SetActive(true);
        UIBehaviour.Instance.cditem.ballSeveral_cond1.SetActive(false);

        //參數初始化
        debugTest = true;
        StaticScript.pause = true; //遊戲暫停
        UIBehaviour.Instance.comp.titleImage.sprite = UIBehaviour.Instance.comp.titleSprites[StaticScript.stageNumber]; //設定關卡標題
        UIBehaviour.Instance.comp.chipImage.sprite = UIBehaviour.Instance.comp.chipSprites[StaticScript.stageNumber]; //設定碎片圖案
        nearlyType = new SpeechBalloonInfo(new Vector2(0, 180), new Vector2(420, 110), 17, speechSpeed, 0.5f); //初始化對話框樣式
        farType = new SpeechBalloonInfo(new Vector2(0, 30), new Vector2(755, 180), 28, speechSpeed, 0.5f);
        seletionType = new SpeechBalloonInfo(new Vector2(0, 60), new Vector2(780, 200), 29, speechSpeed, 0.5f, 0.5f, new Vector2(0, -140), new Vector2(120, 60), 28, 200.0f);

        int[] pBrickShape;
        int[] nBrickShape;
        switch (StaticScript.stageNumber)
        {
            case 0:
                //磚塊塑形
                pBrickShape = new int[] { };
                nBrickShape = new int[] { };
                ShapeBricks(pBrickShape, nBrickShape);
                //埋藏顏色
                BrickBehavior.ColorObjective(7);

                StartCoroutine(ScenarioControl(Paragraph.全白畫面));
                break;

            case 1:
                //磚塊塑形
                pBrickShape = new int[] { 9, 14, 18, 21, 27, 28, 35, 42, 49 };
                nBrickShape = new int[] { 9, 13, 14, 17, 20, 22, 25, 28, 30, 33, 35, 38, 41, 43, 46, 49, 50, 54 };
                ShapeBricks(pBrickShape, nBrickShape);
                //埋藏顏色
                BrickBehavior.ColorObjective(7);
                //控球條件參數設定
                parArray = new object[] { 3, 0 };
                //血條參數
                recoverySpeed = 3;
                damageSpeed = 3;

                StartCoroutine(ScenarioControl(Paragraph.第二關開頭));
                break;

            case 2:
                //磚塊塑形
                pBrickShape = new int[] { 9, 14, 18, 21, 27, 28, 35, 36, 42, 45, 49, 54 };
                nBrickShape = new int[] { 10, 11, 12, 13, 17, 22, 25, 30, 33, 38, 41, 46, 50, 51, 52, 53 };
                ShapeBricks(pBrickShape, nBrickShape);
                //埋藏顏色
                BrickBehavior.ColorObjective(4);
                //控球條件參數設定
                parArray = new object[] { 3, 2 };
                //血條參數
                recoverySpeed = 2.5f;
                damageSpeed = 4;

                StartCoroutine(ScenarioControl(Paragraph.第三關開頭));
                break;

            case 3:
                //磚塊塑形
                pBrickShape = new int[] { 9, 14, 17, 22, 42, 45, 51, 52 };
                nBrickShape = new int[] { 10, 13, 19, 20, 41, 42, 45, 46, 50, 53 };
                ShapeBricks(pBrickShape, nBrickShape);
                //埋藏顏色
                BrickBehavior.ColorObjective(8);
                //控球條件參數設定
                parArray = new object[] { 3, 1 };
                reverseFrequency = 13;
                //血條參數
                recoverySpeed = 2.5f;
                damageSpeed = 4;

                StartCoroutine(ScenarioControl(Paragraph.第四關開頭));
                break;

            case 4:
                //磚塊塑形
                pBrickShape = new int[] { 9, 10, 13, 14, 16, 19, 20, 23, 24, 31, 33, 38, 42, 45, 51, 52 };
                nBrickShape = new int[] { 9, 10, 13, 14, 16, 19, 20, 23, 24, 31, 33, 38, 42, 45, 51, 52 };
                ShapeBricks(pBrickShape, nBrickShape);
                //埋藏顏色
                BrickBehavior.ColorObjective(10);
                //控球條件參數設定
                parArray = new object[] { 4, 1 };
                reverseFrequency = 7;
                //血條參數
                recoverySpeed = 2;
                damageSpeed = 4.5f;

                StartCoroutine(ScenarioControl(Paragraph.第五關開頭));
                break;
        }
    }

    void Update()
    {
        ButtonListen(); //按鍵監聽
        ScenarioListener(); //劇情條件監聽
    }

    //-----------自訂方法------------------------------------------------

    #region 同步更新方法

    //按鍵監聽
    private void ButtonListen()
    {
        if (Input.GetKeyDown(KeyCode.F1) && debugTest) //F1：測試用
        {
            Debug.Log("F1");
            Debug.Log("[關卡] 目前關卡數 : " + StaticScript.stageNumber);
            Debug.Log("[磚塊數] 正面詞語 : " + BrickBehavior.s_pCount + "/" + BrickBehavior.s_positiveWords + " || 負面詞語 : " + BrickBehavior.s_nCount + "/" + BrickBehavior.s_negativeWords + " || 色彩埋藏 : " + BrickBehavior.s_colorRemain + "/" + BrickBehavior.s_colorAmount);
            Debug.Log("[對話框] 尺寸 : " + UIBehaviour.Instance.balloonObj.speechBalloon.GetComponent<RectTransform>().sizeDelta + " || 在地位置 : " + UIBehaviour.Instance.balloonObj.speechBalloon.GetComponent<RectTransform>().localPosition);
            Debug.Log("[下牆] 世界位置 : " + UIBehaviour.Instance.refObj.wall_down.GetComponent<RectTransform>().position + " || 在地位置 : " + UIBehaviour.Instance.refObj.wall_down.GetComponent<RectTransform>().localPosition);
            Debug.Log("[色彩] 共" + BrickBehavior.s_colorAmount + "個");
        }

        if (Input.GetKeyDown(KeyCode.F2) && debugTest) //F2：測試用
        {
            Debug.Log("F2");

            GameObject ball = Instantiate(pref.balls[0], pref.ballHolder);
            ballsPool.Add(ball);
            ball.transform.position = new Vector3(UIBehaviour.Instance.refObj.playerBar.transform.position.x, ( ( UIBehaviour.Instance.refObj.playerBar.GetComponent<RectTransform>().sizeDelta.y / 2 ) * UIBehaviour.Instance.refObj.playerBar.transform.parent.localScale.y ) + ( ( ball.GetComponent<RectTransform>().sizeDelta.y / 2 ) * ball.transform.parent.parent.localScale.y ), ball.transform.position.z); //指定球位置
            ball.SendMessage("TestInstance");
        }

        if (Input.GetKeyDown(KeyCode.F3) && debugTest) //F3：測試用
        {
            Debug.Log("F3");
        }

        if (!StaticScript.pause) //非暫停狀態時
        {
            if (UIBehaviour.Instance.refObj.playerBar.activeSelf) //橫條不為隱藏時
            {
                if (Input.GetAxis("Horizontal") > 0) //持續按下D或右鍵：移動橫條往右
                {
                    PlayerBarBehavior.Instance.Move("右");
                    PlayerBarBehavior.Instance.AccelerationEffect(1.0f);
                    if (tut_moveCount != -1) //移動教程
                    {
                        tut_moveCount += Mathf.Abs(Input.GetAxis("Horizontal"));
                    }
                }

                if (Input.GetAxis("Horizontal") < 0) //A或左鍵：移動橫條往左
                {
                    PlayerBarBehavior.Instance.Move("左");
                    PlayerBarBehavior.Instance.AccelerationEffect(-1.0f);
                    if (tut_moveCount != -1) //移動教程
                    {
                        tut_moveCount += Mathf.Abs(Input.GetAxis("Horizontal"));
                    }
                }

                if (Input.GetAxis("Horizontal") == 0) //不移動時：橫桿碰撞加速度歸零
                {
                    PlayerBarBehavior.Instance.AccelerationEffect(0.0f);
                }
            }

            if (Input.GetKeyDown(KeyCode.Space) && PlayerBarBehavior.Instance.adhereBalls.Count > 0 && !shootLock) //空白鍵：擊發附著的球
            {
                PlayerBarBehavior.Instance.Shoot();
            }
        }
    }

    //劇情條件監聽
    private void ScenarioListener()
    {
        Condition condition;

        if (!StaticScript.pause) //非暫停狀態時
        {
            for (int i = 0; i < listeningConditions.Count; i++)
            {
                condition = (Condition)Enum.Parse(typeof(Condition), listeningConditions[i]);

                switch (condition)
                {
                    default:
                        break;

                    #region 共用條件
                    case Condition.達成通關條件時:
                        if (( BrickBehavior.s_pCount == 0 && BrickBehavior.s_colorRemain == 0 ) || ( debugTest && Input.GetKeyDown(KeyCode.F10) ))
                        {
                            listeningConditions.Remove(Enum.GetName(typeof(Condition), Condition.達成通關條件時)); //監聽條件消去
                            hpActive = false; //心門停止活動
                            isReversing = false; //條件倒轉停止

                            PlayerPrefs.SetInt("GAME_stage" + ( StaticScript.stageNumber + 1 ) + "_unlock", 1); //解鎖關卡
                            StartCoroutine(UIBehaviour.Instance.ShowDialogue(StaticScript.ActorLines(Serif.完成關卡), 1.0f, true, farType, Paragraph.獲得碎片));
                        }
                        break;

                    case Condition.心門關閉:
                        if (UIBehaviour.Instance.cditem.heartDoor[0].value == 0 && UIBehaviour.Instance.cditem.heartDoor[1].value == 0)
                        {
                            listeningConditions.Remove(Enum.GetName(typeof(Condition), Condition.達成通關條件時)); //監聽條件消去
                            listeningConditions.Remove(Enum.GetName(typeof(Condition), Condition.心門關閉));
                            hpActive = false; //心門停止活動
                            isReversing = false; //條件倒轉停止

                            StartCoroutine(UIBehaviour.Instance.ShowDialogue(StaticScript.ActorLines(Serif.關卡失敗), 0.0f, true, farType, Paragraph.關卡失敗));
                        }
                        break;
                    #endregion

                    case Condition.移動一定量時:
                        if (tut_moveCount >= 40)
                        {
                            listeningConditions.Remove(Enum.GetName(typeof(Condition), Condition.移動一定量時)); //監聽條件消去
                            tut_moveCount = -1;

                            StartCoroutine(UIBehaviour.Instance.ShowDialogue(StaticScript.ActorLines(Serif.嘗試擊球), 0.0f, true, nearlyType, Paragraph.玩家操作));

                            shootLock = false;
                            listeningConditions.Add(Enum.GetName(typeof(Condition), Condition.擊發並擊毀磚塊時)); //追加監聽條件
                        }
                        break;

                    case Condition.擊發並擊毀磚塊時:
                        if (BallBehavior.sensor_crash)
                        {
                            listeningConditions.Remove(Enum.GetName(typeof(Condition), Condition.擊發並擊毀磚塊時)); //監聽條件消去

                            BallBehavior.sensor_crash = false;
                            StartCoroutine(UIBehaviour.Instance.ShowDialogue(StaticScript.ActorLines(Serif.擊毀磚塊), 0.3f, true, nearlyType, Paragraph.玩家操作));

                            listeningConditions.Add(Enum.GetName(typeof(Condition), Condition.觸底失誤時)); //追加監聽條件
                            listeningConditions.Add(Enum.GetName(typeof(Condition), Condition.擊毀5個磚塊時)); //追加監聽條件
                        }
                        break;

                    case Condition.擊毀5個磚塊時:
                        if (BallBehavior.sensor_crash)
                        {
                            int count = debugTest ? 5 : 5; //(測試用)縮短測試時間
                            if (BrickBehavior.s_pCount <= ( BrickBehavior.s_positiveWords - count ))
                            {
                                listeningConditions.Remove(Enum.GetName(typeof(Condition), Condition.擊毀5個磚塊時)); //監聽條件消去

                                BallBehavior.sensor_crash = false;
                                StartCoroutine(UIBehaviour.Instance.ShowDialogue(StaticScript.ActorLines(Serif.真相一隅), 0.2f, true, nearlyType, Paragraph.玩家操作));

                                listeningConditions.Add(Enum.GetName(typeof(Condition), Condition.擊毀15個磚塊時)); //追加監聽條件
                            }
                            else
                            {
                                BallBehavior.sensor_crash = false;
                            }
                        }
                        break;

                    case Condition.擊毀15個磚塊時:
                        if (BallBehavior.sensor_crash)
                        {
                            int count = debugTest ? 15 : 15; //(測試用)縮短測試時間
                            if (BrickBehavior.s_pCount <= ( BrickBehavior.s_positiveWords - count ))
                            {
                                listeningConditions.Remove(Enum.GetName(typeof(Condition), Condition.擊毀15個磚塊時)); //監聽條件消去
                                listeningConditions.Remove(Enum.GetName(typeof(Condition), Condition.觸底失誤時)); //監聽條件消去

                                BallBehavior.sensor_crash = false;
                                StartCoroutine(UIBehaviour.Instance.ShowDialogue(StaticScript.ActorLines(Serif.言語失控), 0.2f, true, nearlyType, Paragraph.言語失控));

                                shootLock = true;
                                BallBehavior.sensor_touchBottom = false;
                                listeningConditions.Add(Enum.GetName(typeof(Condition), Condition.言語失控時觸底)); //追加監聽條件
                            }
                            else
                            {
                                BallBehavior.sensor_crash = false;
                            }
                        }
                        break;

                    case Condition.觸底失誤時:
                        if (BallBehavior.sensor_touchBottom)
                        {
                            StaticScript.pause = true;

                            Image ballImage = ballsPool[0].GetComponent<Image>();
                            GameObject ball = ballsPool[0];
                            //不使用Instantiate，而是重新指定球的位置
                            ball.GetComponent<CircleCollider2D>().enabled = false; //關閉碰撞器
                            ballImage.color = new Color(ballImage.color.r, ballImage.color.g, ballImage.color.b, 0); //透明化
                            ball.transform.position = new Vector3(UIBehaviour.Instance.refObj.playerBar.transform.position.x, ( ( UIBehaviour.Instance.refObj.playerBar.GetComponent<RectTransform>().sizeDelta.y / 2 ) * UIBehaviour.Instance.refObj.playerBar.transform.parent.localScale.y ) + ( ( ball.GetComponent<RectTransform>().sizeDelta.y / 2 ) * ball.transform.parent.parent.localScale.y ), ball.transform.position.z); //指定球位置
                            ball.GetComponent<Animator>().Play("FadeIn", 0, 0); //圖片淡入
                            PlayerBarBehavior.Instance.Adhere(ref ball); //橫條附著

                            BallBehavior.sensor_touchBottom = false;
                            tut_fallCount++;
                            StartCoroutine(UIBehaviour.Instance.ShowDialogue(StaticScript.ActorLines(Serif.掉落失誤), 0.0f, false, nearlyType, Paragraph.玩家操作));
                        }
                        break;

                    case Condition.言語失控時觸底:
                        if (BallBehavior.sensor_touchBottom) //觸底時觸發劇情
                        {
                            listeningConditions.Remove(Enum.GetName(typeof(Condition), Condition.言語失控時觸底)); //監聽條件消去

                            BallBehavior.sensor_touchBottom = false;
                            StartCoroutine(UIBehaviour.Instance.ShowDialogue(StaticScript.ActorLines(Serif.負面情緒暴露), 0.0f, true, nearlyType, Paragraph.視角擴大));


                            listeningConditions.Add(Enum.GetName(typeof(Condition), Condition.挖掘色彩時)); //追加監聽條件
                        }
                        break;

                    case Condition.挖掘色彩時:
                        if (BrickBehavior.sensor_color) //出現色彩時觸發劇情
                        {
                            listeningConditions.Remove(Enum.GetName(typeof(Condition), Condition.挖掘色彩時)); //監聽條件消去

                            BrickBehavior.sensor_color = false;
                            UIBehaviour.Instance.refObj.progressPanel.SetActive(true);
                            UIBehaviour.Instance.anim.progressPanel.Play("UI_ProgressPanel_FadeIn", 0, 0);
                            StartCoroutine(UIBehaviour.Instance.ShowDialogue(StaticScript.ActorLines(Serif.找到色彩), 0.0f, true, farType, Paragraph.玩家操作));

                            listeningConditions.Add(Enum.GetName(typeof(Condition), Condition.達成通關條件時)); //追加監聽條件
                        }
                        break;

                    case Condition.心門關上一定程度時:
                        if (UIBehaviour.Instance.cditem.heartDoor[0].value <= 0.75f && UIBehaviour.Instance.cditem.heartDoor[1].value <= 0.75f)
                        {
                            listeningConditions.Remove(Enum.GetName(typeof(Condition), Condition.心門關上一定程度時)); //監聽條件消去

                            StartCoroutine(UIBehaviour.Instance.ShowDialogue(StaticScript.ActorLines(Serif.嘗試扣血), 0.0f, true, farType, Paragraph.提示結束));
                        }
                        break;

                    case Condition.條件倒轉時:
                        if (reversingState)
                        {
                            listeningConditions.Remove(Enum.GetName(typeof(Condition), Condition.條件倒轉時)); //監聽條件消去

                            StartCoroutine(UIBehaviour.Instance.ShowDialogue(StaticScript.ActorLines(Serif.條件倒轉), 1.8f, true, farType, Paragraph.玩家操作));
                        }
                        break;

                }
            }

        }
    }
    #endregion
    //範圍檢測 type=true時, a >= b時回傳true, a < b時回傳false / type=false時, a <= b時回傳true, a > b時回傳false
    private bool RangeCheck(int a, int b, bool type)
    {
        return ( ( ( a > b ) ^ !type ) || ( a == b ) );
    }

    //磚塊群塑形(輸入消除磚塊編號)
    private void ShapeBricks(int[] pB, int[] nB)
    {
        BrickBehavior.s_positiveWords = 0;
        BrickBehavior.s_negativeWords = 0;

        for (int i = 0; i < pB.Length; i++) //正面磚塊群塑形
        {
            positiveBricks[pB[i]].SetActive(false);
            spaceBricks.Add(Instantiate(pref.brick_space, positiveBricks[pB[i]].transform.position, Quaternion.identity, pref.pBricksHolder)); //消除的方塊以空白方塊替代
            positiveBricks[pB[i]] = null;
        }

        for (int i = 0; i < nB.Length; i++) //負面磚塊群塑形
        {
            negativeBricks[nB[i]].SetActive(false);
            spaceBricks.Add(Instantiate(pref.brick_space, negativeBricks[nB[i]].transform.position, Quaternion.identity, pref.nBricksHolder)); //消除的方塊以空白方塊替代
            negativeBricks[nB[i]] = null;
        }

        for (int i = 0; i < positiveBricks.Count; i++) //計算正面磚塊數
        {
            if (positiveBricks[i] != null)
            {
                BrickBehavior.s_positiveWords++;
            }
        }
        BrickBehavior.s_pCount = BrickBehavior.s_positiveWords;

        for (int i = 0; i < negativeBricks.Count; i++) //計算負面磚塊數
        {
            if (negativeBricks[i] != null)
            {
                BrickBehavior.s_negativeWords++;
            }
        }

    }

    //-----------IEnumerator------------------------------------------------

    //劇本控制
    public IEnumerator ScenarioControl(Paragraph paragraph)
    {

        nowLibretto = paragraph;

        int[] ballsInfo = new int[0];
        switch (paragraph)
        {
            #region 共用章節
            case Paragraph.玩家操作:
                yield return new WaitForSeconds(speechOverDelay / fastForward);

                if (playBgmTag != "") //撥放指定BGM
                {
                    AudioManagerScript.Instance.PlayAudioClip(playBgmTag);
                    playBgmTag = "";
                }

                StaticScript.pause = false;
                break;

            case Paragraph.獲得碎片:
                AudioManagerScript.Instance.Stop(1); //BGM停止

                yield return new WaitForSeconds(0.8f / fastForward);

                //介面淡出
                UIBehaviour.Instance.anim.wall_up.Play("FadeOut", 0, 0);
                UIBehaviour.Instance.anim.wall_down.Play("FadeOut", 0, 0);
                UIBehaviour.Instance.anim.wall_left.Play("FadeOut", 0, 0);
                UIBehaviour.Instance.anim.wall_right.Play("FadeOut", 0, 0);
                UIBehaviour.Instance.anim.playerBar.Play("FadeOut", 0, 0);
                UIBehaviour.Instance.anim.botBG.Play("FadeOut", 0, 0);
                UIBehaviour.Instance.anim.progressPanel.Play("UI_ProgressPanel_FadeOut", 0, 0);
                UIBehaviour.Instance.anim.title.Play("FadeOut", 0, 0);
                UIBehaviour.Instance.anim.NbricksHolder.Play("Remove_toDown", 0, 0);
                UIBehaviour.Instance.anim.cdPanel.Play("Remove_toRight", 0, 0);
                UIBehaviour.Instance.anim.chip.Play("Chip_Enlarge", 0, 0);

                for (int i = 0; i < ballsPool.Count; i++) //球淡出
                {
                    ballsPool[i].GetComponent<Animator>().Play("FadeOut", 0, 0);
                }

                for (int i = 0; i < spaceBricks.Count; i++) //空白磚塊淡出
                {
                    spaceBricks[i].GetComponent<Animator>().Play("SpaceDisappear", 0, 0);
                }

                AudioManagerScript.Instance.PlayAudioClip("SE_REWARD");

                StartCoroutine(UIBehaviour.Instance.ShowDialogue(StaticScript.ActorLines(Serif.解鎖碎片), 2.0f, false, farType, Paragraph.返回主選單));

                break;

            case Paragraph.關卡失敗:
                AudioManagerScript.Instance.Stop(1); //BGM停止

                yield return new WaitForSeconds(0.8f / fastForward);

                //介面淡出
                UIBehaviour.Instance.anim.wall_up.Play("FadeOut", 0, 0);
                UIBehaviour.Instance.anim.wall_down.Play("FadeOut", 0, 0);
                UIBehaviour.Instance.anim.wall_left.Play("FadeOut", 0, 0);
                UIBehaviour.Instance.anim.wall_right.Play("FadeOut", 0, 0);
                UIBehaviour.Instance.anim.playerBar.Play("FadeOut", 0, 0);
                UIBehaviour.Instance.anim.botBG.Play("FadeOut", 0, 0);
                UIBehaviour.Instance.anim.chip.Play("FadeOut", 0, 0);

                yield return new WaitForSeconds(0.8f / fastForward);

                UIBehaviour.Instance.anim.title.Play("FadeOut", 0, 0);
                UIBehaviour.Instance.anim.progressPanel.Play("UI_ProgressPanel_FadeOut", 0, 0);
                UIBehaviour.Instance.anim.cdMask.Play("FadeIn", 0, 0);

                yield return new WaitForSeconds(0.8f / fastForward);

                UIBehaviour.Instance.anim.PMask.Play("FadeIn", 0, 0);
                UIBehaviour.Instance.anim.NMask.Play("FadeIn", 0, 0);

                for (int i = 0; i < ballsPool.Count; i++) //球淡出
                {
                    ballsPool[i].GetComponent<Animator>().Play("FadeOut", 0, 0);
                }

                for (int i = 0; i < spaceBricks.Count; i++) //空白磚塊淡出
                {
                    spaceBricks[i].GetComponent<Animator>().Play("SpaceDisappear", 0, 0);
                }

                yield return new WaitForSeconds(2.5f / fastForward);

                SceneManager.LoadScene("StartMenu");
                break;

            case Paragraph.返回主選單:
                yield return new WaitForSeconds(0.3f / fastForward);

                UIBehaviour.Instance.refObj.btn_backToStartMenu.SetActive(true);
                UIBehaviour.Instance.anim.btn_backToStartMenu.Play("ButtonFadeIn", 0, 0);
                break;
            #endregion
            #region 第一關章節

            case Paragraph.全白畫面:
                StartCoroutine(UIBehaviour.Instance.ShowDialogue(StaticScript.ActorLines(Serif.開頭導詞), 1.0f, true, nearlyType, Paragraph.生活之牆));
                break;

            case Paragraph.生活之牆:
                yield return new WaitForSeconds(1.0f / fastForward);

                UIBehaviour.Instance.refObj.wall_up.SetActive(true);
                UIBehaviour.Instance.refObj.wall_down.SetActive(true);
                UIBehaviour.Instance.refObj.wall_right.SetActive(true);
                UIBehaviour.Instance.refObj.wall_left.SetActive(true);
                UIBehaviour.Instance.refObj.botBG.SetActive(true);
                UIBehaviour.Instance.anim.wall_up.Play("FadeIn");
                UIBehaviour.Instance.anim.wall_down.Play("FadeIn");
                UIBehaviour.Instance.anim.wall_right.Play("FadeIn");
                UIBehaviour.Instance.anim.wall_left.Play("FadeIn");
                UIBehaviour.Instance.anim.botBG.Play("FadeIn");

                StartCoroutine(UIBehaviour.Instance.ShowDialogue(StaticScript.ActorLines(Serif.生活之牆), 2.0f, false, nearlyType, Paragraph.心之橫條));

                break;

            case Paragraph.心之橫條:
                yield return new WaitForSeconds(1.0f / fastForward);

                UIBehaviour.Instance.refObj.playerBar.SetActive(true);
                UIBehaviour.Instance.anim.playerBar.Play("FadeIn", 0, 0);
                PlayerBarBehavior.Instance.Locate(new GameObject[] { UIBehaviour.Instance.refObj.wall_right, UIBehaviour.Instance.refObj.wall_left }); //緩衝區定位

                StartCoroutine(UIBehaviour.Instance.ShowDialogue(StaticScript.ActorLines(Serif.心之橫條), 2.0f, false, nearlyType, Paragraph.言語之球));

                break;

            case Paragraph.言語之球:
                yield return new WaitForSeconds(1.0f / fastForward);

                ballsInfo = new int[] { 0, 1 }; // 0(普通球) : 1顆
                StartCoroutine(BallMaker(ballsInfo, 0, true));
                StartCoroutine(UIBehaviour.Instance.ShowDialogue(StaticScript.ActorLines(Serif.言語之球), 2.0f, false, nearlyType, Paragraph.情緒磚塊));

                break;

            case Paragraph.情緒磚塊:
                yield return new WaitForSeconds(1.0f / fastForward);

                UIBehaviour.Instance.refObj.PBricks.SetActive(true);
                UIBehaviour.Instance.anim.PMask.Play("FadeOut", 0, 0);

                tut_moveCount = 0;
                listeningConditions.Add(Enum.GetName(typeof(Condition), Condition.移動一定量時)); //追加監聽條件
                StartCoroutine(UIBehaviour.Instance.ShowDialogue(StaticScript.ActorLines(Serif.嘗試移動), 2.0f, false, nearlyType, Paragraph.玩家操作));
                break;

            case Paragraph.言語失控:
                yield return new WaitForSeconds(speechOverDelay / fastForward);

                StaticScript.pause = false;
                ballsInfo = new int[] { 0, 4 }; // 0(普通球) : 4顆
                StartCoroutine(BallMaker(ballsInfo, 0.2f, false));
                break;

            case Paragraph.視角擴大:
                yield return new WaitForSeconds(0.8f / fastForward);

                UIBehaviour.Instance.comp.downWallRect.localPosition = new Vector3(UIBehaviour.Instance.comp.downWallRect.localPosition.x, -356, UIBehaviour.Instance.comp.downWallRect.localPosition.z); //移動下牆位置
                UIBehaviour.Instance.anim.camera.Play("CameraMove", 0, 0); //移動攝影機(擴大視角)
                UIBehaviour.Instance.refObj.NBricks.SetActive(true);
                UIBehaviour.Instance.anim.NMask.Play("FadeOut", 0, 0);

                playBgmTag = "BGM1";

                StartCoroutine(UIBehaviour.Instance.ShowDialogue(StaticScript.ActorLines(Serif.尋找價值), 2.5f, false, farType, Paragraph.玩家操作));
                break;

            #endregion
            #region 第二關章節

            case Paragraph.第二關開頭:
                UIBehaviour.Instance.anim.camera.Play("CameraMove", 0, 0); //移動攝影機(擴大視角)
                yield return new WaitForSeconds(0.5f / fastForward);

                UIBehaviour.Instance.refObj.title.SetActive(true);
                UIBehaviour.Instance.anim.title.Play("FadeIn", 0, 0);

                AudioManagerScript.Instance.PlayAudioClip("SE_INTERLUDE");

                yield return new WaitForSeconds(0.5f / fastForward);

                UIBehaviour.Instance.comp.downWallRect.localPosition = new Vector3(UIBehaviour.Instance.comp.downWallRect.localPosition.x, -356, UIBehaviour.Instance.comp.downWallRect.localPosition.z); //移動下牆位置

                UIBehaviour.Instance.refObj.wall_up.SetActive(true);
                UIBehaviour.Instance.refObj.wall_down.SetActive(true);
                UIBehaviour.Instance.refObj.wall_right.SetActive(true);
                UIBehaviour.Instance.refObj.wall_left.SetActive(true);
                UIBehaviour.Instance.refObj.PBricks.SetActive(true);
                UIBehaviour.Instance.refObj.NBricks.SetActive(true);
                UIBehaviour.Instance.refObj.PMask.SetActive(true);
                UIBehaviour.Instance.refObj.NMask.SetActive(true);
                UIBehaviour.Instance.anim.wall_up.Play("FadeIn", 0, 0);
                UIBehaviour.Instance.anim.wall_down.Play("FadeIn", 0, 0);
                UIBehaviour.Instance.anim.wall_right.Play("FadeIn", 0, 0);
                UIBehaviour.Instance.anim.wall_left.Play("FadeIn", 0, 0);
                UIBehaviour.Instance.anim.PMask.Play("FadeOut", 0, 0);
                UIBehaviour.Instance.anim.NMask.Play("FadeOut", 0, 0);

                yield return new WaitForSeconds(0.5f / fastForward);

                UIBehaviour.Instance.refObj.playerBar.SetActive(true);
                UIBehaviour.Instance.refObj.botBG.SetActive(true);
                UIBehaviour.Instance.anim.playerBar.Play("FadeIn", 0, 0);
                UIBehaviour.Instance.anim.botBG.Play("FadeIn", 0, 0);

                yield return new WaitForSeconds(0.5f / fastForward);

                UIBehaviour.Instance.refObj.progressPanel.SetActive(true);
                UIBehaviour.Instance.anim.progressPanel.Play("UI_ProgressPanel_FadeIn", 0, 0);

                yield return new WaitForSeconds(1.2f / fastForward);

                PlayerBarBehavior.Instance.Locate(new GameObject[] { UIBehaviour.Instance.refObj.wall_right, UIBehaviour.Instance.refObj.wall_left }); //緩衝區定位
                StartCoroutine(UIBehaviour.Instance.ShowDialogue(StaticScript.ActorLines(Serif.第二關導詞), 1.0f, false, farType, Paragraph.血條控球));
                break;

            case Paragraph.血條控球:
                yield return new WaitForSeconds(1.0f / fastForward);

                UIBehaviour.Instance.anim.cdMask.Play("FadeOut", 0, 0);
                StartCoroutine(ShowConditionPanel(DamageMode.上或下層球在某數量以上, parArray)); //顯示控球條件燈號

                StartCoroutine(UIBehaviour.Instance.ShowDialogue(StaticScript.ActorLines(Serif.說話技巧), 1.6f, false, farType, Paragraph.說話技巧));
                break;

            case Paragraph.說話技巧:
                yield return new WaitForSeconds(speechOverDelay / fastForward);

                StaticScript.pause = false;
                ballsInfo = new int[] { 0, 4 }; // 0(普通球) : 4顆
                StartCoroutine(BallMaker(ballsInfo, 0.2f, false)); //生成球
                StartCoroutine(HpConditiondListen(DamageMode.上或下層球在某數量以上, parArray)); //設定控球監聽類型
                StartCoroutine(HpConditiondListen(DamageMode.上或下層球在某數量以上, parArray)); //心門開始活動

                listeningConditions.Add(Enum.GetName(typeof(Condition), Condition.心門關上一定程度時)); //追加監聽條件

                AudioManagerScript.Instance.PlayAudioClip("BGM1");

                yield return new WaitForSeconds(20);

                if (!StaticScript.pause && nowLibretto == Paragraph.說話技巧) StartCoroutine(UIBehaviour.Instance.ShowDialogue(StaticScript.ActorLines(Serif.技巧解說), 0.0f, true, farType, Paragraph.玩家操作));

                break;

            case Paragraph.提示結束:
                yield return new WaitForSeconds(0.5f / fastForward);

                StaticScript.pause = false;
                listeningConditions.Add(Enum.GetName(typeof(Condition), Condition.達成通關條件時)); //追加監聽條件
                listeningConditions.Add(Enum.GetName(typeof(Condition), Condition.心門關閉));
                break;
            #endregion
            #region 第三關章節

            case Paragraph.第三關開頭:

                UIBehaviour.Instance.anim.camera.Play("CameraMove", 0, 0); //移動攝影機(擴大視角)
                yield return new WaitForSeconds(0.5f / fastForward);

                UIBehaviour.Instance.refObj.title.SetActive(true);
                UIBehaviour.Instance.anim.title.Play("FadeIn", 0, 0);

                AudioManagerScript.Instance.PlayAudioClip("SE_INTERLUDE");

                yield return new WaitForSeconds(0.5f / fastForward);

                UIBehaviour.Instance.comp.downWallRect.localPosition = new Vector3(UIBehaviour.Instance.comp.downWallRect.localPosition.x, -356, UIBehaviour.Instance.comp.downWallRect.localPosition.z); //移動下牆位置

                UIBehaviour.Instance.refObj.wall_up.SetActive(true);
                UIBehaviour.Instance.refObj.wall_down.SetActive(true);
                UIBehaviour.Instance.refObj.wall_right.SetActive(true);
                UIBehaviour.Instance.refObj.wall_left.SetActive(true);
                UIBehaviour.Instance.refObj.PBricks.SetActive(true);
                UIBehaviour.Instance.refObj.NBricks.SetActive(true);
                UIBehaviour.Instance.refObj.PMask.SetActive(true);
                UIBehaviour.Instance.refObj.NMask.SetActive(true);
                UIBehaviour.Instance.anim.wall_up.Play("FadeIn", 0, 0);
                UIBehaviour.Instance.anim.wall_down.Play("FadeIn", 0, 0);
                UIBehaviour.Instance.anim.wall_right.Play("FadeIn", 0, 0);
                UIBehaviour.Instance.anim.wall_left.Play("FadeIn", 0, 0);
                UIBehaviour.Instance.anim.PMask.Play("FadeOut", 0, 0);
                UIBehaviour.Instance.anim.NMask.Play("FadeOut", 0, 0);

                yield return new WaitForSeconds(0.5f / fastForward);

                UIBehaviour.Instance.refObj.playerBar.SetActive(true);
                UIBehaviour.Instance.refObj.botBG.SetActive(true);
                UIBehaviour.Instance.anim.playerBar.Play("FadeIn", 0, 0);
                UIBehaviour.Instance.anim.botBG.Play("FadeIn", 0, 0);

                yield return new WaitForSeconds(0.5f / fastForward);

                UIBehaviour.Instance.refObj.progressPanel.SetActive(true);
                UIBehaviour.Instance.anim.progressPanel.Play("UI_ProgressPanel_FadeIn", 0, 0);

                yield return new WaitForSeconds(0.5f / fastForward);

                UIBehaviour.Instance.cditem.ballSeveral_cond1.SetActive(true);
                UIBehaviour.Instance.anim.cdMask.Play("FadeOut", 0, 0);

                yield return new WaitForSeconds(0.8f / fastForward);

                PlayerBarBehavior.Instance.Locate(new GameObject[] { UIBehaviour.Instance.refObj.wall_right, UIBehaviour.Instance.refObj.wall_left }); //緩衝區定位
                ballsInfo = new int[] { 0, 3, 1, 2 }; // 0(普通球) : 3顆 + 1(穿透球) : 2顆
                StartCoroutine(BallMaker(ballsInfo, 0, false)); //生成球

                yield return new WaitForSeconds(0.8f / fastForward);

                StartCoroutine(ShowConditionPanel(DamageMode.上或下層球在某數量以上, parArray)); //顯示控球條件燈號
                listeningConditions.Add(Enum.GetName(typeof(Condition), Condition.達成通關條件時)); //追加監聽條件
                listeningConditions.Add(Enum.GetName(typeof(Condition), Condition.心門關閉));

                StartCoroutine(HpConditiondListen(DamageMode.上或下層球在某數量以上, parArray)); //心門開始活動

                playBgmTag = "BGM2";

                StartCoroutine(UIBehaviour.Instance.ShowDialogue(StaticScript.ActorLines(Serif.第三關導詞), 1.0f, false, farType, Paragraph.玩家操作));
                break;
            #endregion
            #region 第四關章節

            case Paragraph.第四關開頭:

                UIBehaviour.Instance.anim.camera.Play("CameraMove", 0, 0); //移動攝影機(擴大視角)
                yield return new WaitForSeconds(0.5f / fastForward);

                UIBehaviour.Instance.refObj.title.SetActive(true);
                UIBehaviour.Instance.anim.title.Play("FadeIn", 0, 0);

                AudioManagerScript.Instance.PlayAudioClip("SE_INTERLUDE");

                yield return new WaitForSeconds(0.5f / fastForward);

                UIBehaviour.Instance.comp.downWallRect.localPosition = new Vector3(UIBehaviour.Instance.comp.downWallRect.localPosition.x, -356, UIBehaviour.Instance.comp.downWallRect.localPosition.z); //移動下牆位置

                UIBehaviour.Instance.refObj.wall_up.SetActive(true);
                UIBehaviour.Instance.refObj.wall_down.SetActive(true);
                UIBehaviour.Instance.refObj.wall_right.SetActive(true);
                UIBehaviour.Instance.refObj.wall_left.SetActive(true);
                UIBehaviour.Instance.refObj.PBricks.SetActive(true);
                UIBehaviour.Instance.refObj.NBricks.SetActive(true);
                UIBehaviour.Instance.refObj.PMask.SetActive(true);
                UIBehaviour.Instance.refObj.NMask.SetActive(true);
                UIBehaviour.Instance.anim.wall_up.Play("FadeIn", 0, 0);
                UIBehaviour.Instance.anim.wall_down.Play("FadeIn", 0, 0);
                UIBehaviour.Instance.anim.wall_right.Play("FadeIn", 0, 0);
                UIBehaviour.Instance.anim.wall_left.Play("FadeIn", 0, 0);
                UIBehaviour.Instance.anim.PMask.Play("FadeOut", 0, 0);
                UIBehaviour.Instance.anim.NMask.Play("FadeOut", 0, 0);

                yield return new WaitForSeconds(0.5f / fastForward);

                UIBehaviour.Instance.refObj.playerBar.SetActive(true);
                UIBehaviour.Instance.refObj.botBG.SetActive(true);
                UIBehaviour.Instance.anim.playerBar.Play("FadeIn", 0, 0);
                UIBehaviour.Instance.anim.botBG.Play("FadeIn", 0, 0);

                yield return new WaitForSeconds(0.5f / fastForward);

                UIBehaviour.Instance.refObj.progressPanel.SetActive(true);
                UIBehaviour.Instance.anim.progressPanel.Play("UI_ProgressPanel_FadeIn", 0, 0);

                yield return new WaitForSeconds(0.5f / fastForward);

                UIBehaviour.Instance.cditem.ballSeveral_cond1.SetActive(true);
                UIBehaviour.Instance.anim.cdMask.Play("FadeOut", 0, 0);

                yield return new WaitForSeconds(0.8f / fastForward);

                PlayerBarBehavior.Instance.Locate(new GameObject[] { UIBehaviour.Instance.refObj.wall_right, UIBehaviour.Instance.refObj.wall_left }); //緩衝區定位
                ballsInfo = new int[] { 0, 2, 1, 1, 2, 1 }; // 0(普通球) : 2顆 + 1(穿透球) : 1顆 + 2(快球) : 1顆
                StartCoroutine(BallMaker(ballsInfo, 0, false)); //生成球

                yield return new WaitForSeconds(0.8f / fastForward);

                StartCoroutine(ShowConditionPanel(DamageMode.上或下層球在某數量以上, parArray)); //顯示控球條件燈號
                listeningConditions.Add(Enum.GetName(typeof(Condition), Condition.達成通關條件時)); //追加監聽條件
                listeningConditions.Add(Enum.GetName(typeof(Condition), Condition.心門關閉));
                listeningConditions.Add(Enum.GetName(typeof(Condition), Condition.條件倒轉時));

                StartCoroutine(HpConditiondListen(DamageMode.上或下層球在某數量以上, parArray)); //心門開始活動
                StartCoroutine(ReverseCondition()); //條件倒轉

                playBgmTag = "BGM2";

                StartCoroutine(UIBehaviour.Instance.ShowDialogue(StaticScript.ActorLines(Serif.第四關導詞), 1.0f, false, farType, Paragraph.玩家操作));
                break;
            #endregion
            #region 第五關章節

            case Paragraph.第五關開頭:

                UIBehaviour.Instance.anim.camera.Play("CameraMove", 0, 0); //移動攝影機(擴大視角)
                yield return new WaitForSeconds(0.5f / fastForward);

                UIBehaviour.Instance.refObj.title.SetActive(true);
                UIBehaviour.Instance.anim.title.Play("FadeIn", 0, 0);

                AudioManagerScript.Instance.PlayAudioClip("SE_INTERLUDE");

                yield return new WaitForSeconds(0.5f / fastForward);

                UIBehaviour.Instance.comp.downWallRect.localPosition = new Vector3(UIBehaviour.Instance.comp.downWallRect.localPosition.x, -356, UIBehaviour.Instance.comp.downWallRect.localPosition.z); //移動下牆位置

                UIBehaviour.Instance.refObj.wall_up.SetActive(true);
                UIBehaviour.Instance.refObj.wall_down.SetActive(true);
                UIBehaviour.Instance.refObj.wall_right.SetActive(true);
                UIBehaviour.Instance.refObj.wall_left.SetActive(true);
                UIBehaviour.Instance.refObj.PBricks.SetActive(true);
                UIBehaviour.Instance.refObj.NBricks.SetActive(true);
                UIBehaviour.Instance.refObj.PMask.SetActive(true);
                UIBehaviour.Instance.refObj.NMask.SetActive(true);
                UIBehaviour.Instance.anim.wall_up.Play("FadeIn", 0, 0);
                UIBehaviour.Instance.anim.wall_down.Play("FadeIn", 0, 0);
                UIBehaviour.Instance.anim.wall_right.Play("FadeIn", 0, 0);
                UIBehaviour.Instance.anim.wall_left.Play("FadeIn", 0, 0);
                UIBehaviour.Instance.anim.PMask.Play("FadeOut", 0, 0);
                UIBehaviour.Instance.anim.NMask.Play("FadeOut", 0, 0);

                yield return new WaitForSeconds(0.5f / fastForward);

                UIBehaviour.Instance.refObj.playerBar.SetActive(true);
                UIBehaviour.Instance.refObj.botBG.SetActive(true);
                UIBehaviour.Instance.anim.playerBar.Play("FadeIn", 0, 0);
                UIBehaviour.Instance.anim.botBG.Play("FadeIn", 0, 0);

                yield return new WaitForSeconds(0.5f / fastForward);

                UIBehaviour.Instance.refObj.progressPanel.SetActive(true);
                UIBehaviour.Instance.anim.progressPanel.Play("UI_ProgressPanel_FadeIn", 0, 0);

                yield return new WaitForSeconds(0.5f / fastForward);

                UIBehaviour.Instance.cditem.ballSeveral_cond1.SetActive(true);
                UIBehaviour.Instance.anim.cdMask.Play("FadeOut", 0, 0);

                yield return new WaitForSeconds(0.8f / fastForward);

                PlayerBarBehavior.Instance.Locate(new GameObject[] { UIBehaviour.Instance.refObj.wall_right, UIBehaviour.Instance.refObj.wall_left }); //緩衝區定位
                ballsInfo = new int[] { 0, 1, 1, 2, 2, 2 }; // 0(普通球) : 1顆 + 1(穿透球) : 2顆 + 2(快球) : 2顆
                StartCoroutine(BallMaker(ballsInfo, 0, false)); //生成球

                yield return new WaitForSeconds(1.2f / fastForward);

                StartCoroutine(ShowConditionPanel(DamageMode.上或下層球在某數量以上, parArray)); //顯示控球條件燈號
                listeningConditions.Add(Enum.GetName(typeof(Condition), Condition.達成通關條件時)); //追加監聽條件
                listeningConditions.Add(Enum.GetName(typeof(Condition), Condition.心門關閉));

                StartCoroutine(HpConditiondListen(DamageMode.上或下層球在某數量以上, parArray)); //心門開始活動
                StartCoroutine(ReverseCondition()); //條件倒轉

                playBgmTag = "BGM3";

                StartCoroutine(UIBehaviour.Instance.ShowDialogue(StaticScript.ActorLines(Serif.第五關導詞), 1.0f, false, farType, Paragraph.玩家操作));
                break;
                #endregion
        }
    }

    //球物件生成
    private IEnumerator BallMaker(int[] type_and_count, float delay, bool adhere) //type_and_count = 種類和數量 , delay = 每發延遲(秒) , adhere = 附著與否
    {
        //計算總共生成球數量是否大於5, 是則報錯
        int[] eachCount = new int[type_and_count.Length / 2];
        for (int i = 0; i < eachCount.Length; i++)
        {
            eachCount[i] = type_and_count[2 * i + 1];
        }

        int times = new int();
        for (int i = 0; i < eachCount.Length; i++)
        {
            times += eachCount[i];
        }

        if (times > 5) Debug.Log("[BUG]總生成球數超過5顆");

        //依序生成球
        int line = 0;
        for (int i = 0; i < type_and_count.Length; i += 2)
        {
            float iV = pref.balls[type_and_count[i]].GetComponent<BallBehavior>().initialVelocity; //初速

            for (int j = 0; j < type_and_count[i + 1]; j++)
            {
                GameObject ball = Instantiate(pref.balls[type_and_count[i]], pref.ballHolder); //生成球物件
                float ballLength = ball.GetComponent<RectTransform>().sizeDelta.x * ball.transform.parent.parent.localScale.x; //球寬
                float coordinate = ( ( 0.5f - ( 0.5f * times ) ) * ballLength ) + ( line * ballLength ); //座標
                line++;

                ballsPool.Add(ball); //加進球池List方便控制
                ball.transform.position = new Vector3(UIBehaviour.Instance.refObj.playerBar.transform.position.x + coordinate, ( ( UIBehaviour.Instance.refObj.playerBar.GetComponent<RectTransform>().sizeDelta.y / 2 ) * UIBehaviour.Instance.refObj.playerBar.transform.parent.localScale.y ) + ( ( ball.GetComponent<RectTransform>().sizeDelta.y / 2 ) * ball.transform.parent.parent.localScale.y ), ball.transform.position.z); //指定球位置
                PlayerBarBehavior.Instance.Adhere(ref ball); //淡入特效
                ball.GetComponent<Animator>().Play("FadeIn", 0, 0); //橫桿附著

                yield return new WaitForSeconds(delay);
            }

            if (times > 1 && !adhere)
            {
                yield return new WaitForSeconds(( delay + 0.08f ) * times);
                PlayerBarBehavior.Instance.ShootAll(new Vector2(UnityEngine.Random.Range(iV / 2, iV), iV));
            }
        }
    }

    //顯示控球面板
    private IEnumerator ShowConditionPanel(DamageMode mode, object[] par)
    {
        switch (mode)
        {
            case DamageMode.上或下層球在某數量以上: //( [0] int 上層球數, [1] int 下層球數 )
                if (!UIBehaviour.Instance.cditem.ballSeveral_cond1.activeSelf) UIBehaviour.Instance.cditem.ballSeveral_cond1.SetActive(true);

                byte upCd = (byte)Mathf.Clamp((int)par[0], 0, 5); //上層條件數限制在0 ~ 5之間
                byte lowCd = (byte)Mathf.Clamp((int)par[1], 0, 5); //下層條件數限制在0 ~ 5之間

                //對齊
                for (int i = 0; i < 5; i++)
                {
                    if (i < upCd)
                    {
                        UIBehaviour.Instance.cditem.upperSensors[i].SetActive(true);
                        UIBehaviour.Instance.cditem.upperSensors[i].GetComponent<Image>().color = new Color(0, 0, 0, 0);
                    }
                    if (i < lowCd)
                    {
                        UIBehaviour.Instance.cditem.lowerSensors[i].SetActive(true);
                        UIBehaviour.Instance.cditem.lowerSensors[i].GetComponent<Image>().color = new Color(0, 0, 0, 0);
                    }
                    if (i > upCd && i > lowCd) break;

                }
                for (int i = 0; i < UIBehaviour.Instance.cditem.layoutComponents.Length; i++)
                {
                    UIBehaviour.Instance.cditem.layoutComponents[i].enabled = true;
                    byte v = 0;
                    if (i == 0) v = upCd;
                    if (i == 1) v = lowCd;
                    UIBehaviour.Instance.cditem.layoutComponents[i].spacing = sensorBasicSpacing + ( ( 5 - v ) * sensorSpacingUnit );
                    yield return null;
                    UIBehaviour.Instance.cditem.layoutComponents[i].enabled = false;
                }

                //燈號顏色變換
                byte upCount = 0;
                byte lowCount = 0;
                for (int i = 0; i < ballsPool.Count; i++) //統計上下層各有多少顆球
                {
                    if (ballsPool[i].transform.position.y > UIBehaviour.Instance.refObj.playerBar.transform.position.y)
                    {
                        upCount++;
                    }
                    else
                    {
                        lowCount++;
                    }
                }
                for (int i = 0; i < 5; i++)
                {
                    //啟動上層燈號
                    if (i < upCd)
                    {
                        if (upCount <= 0) //未激活燈號
                        {
                            UIBehaviour.Instance.cditem.upperSensors[i].GetComponent<Image>().color = UIBehaviour.Instance.cditem.deactivate;
                        }
                        else //激活燈號
                        {
                            UIBehaviour.Instance.cditem.upperSensors[i].GetComponent<Image>().color = UIBehaviour.Instance.cditem.upperActive;
                            upCount--;
                        }
                        UIBehaviour.Instance.cditem.upperSensors[i].GetComponent<Animator>().enabled = true;
                        UIBehaviour.Instance.cditem.upperSensors[i].GetComponent<Animator>().Play("FadeIn", 0, 0);
                        yield return new WaitForSeconds(0.2f);
                    }
                    else
                    {
                        UIBehaviour.Instance.cditem.upperSensors[i].SetActive(false);
                    }

                    //啟動下層燈號
                    if (i < lowCd)
                    {
                        if (lowCount <= 0) //未激活燈號
                        {
                            UIBehaviour.Instance.cditem.lowerSensors[i].GetComponent<Image>().color = UIBehaviour.Instance.cditem.deactivate;
                        }
                        else //激活燈號
                        {
                            UIBehaviour.Instance.cditem.lowerSensors[i].GetComponent<Image>().color = UIBehaviour.Instance.cditem.lowerActive;
                            lowCount--;
                        }
                        UIBehaviour.Instance.cditem.lowerSensors[i].GetComponent<Animator>().enabled = true;
                        UIBehaviour.Instance.cditem.lowerSensors[i].GetComponent<Animator>().Play("FadeIn", 0, 0);
                        yield return new WaitForSeconds(0.2f);
                    }
                    else
                    {
                        UIBehaviour.Instance.cditem.lowerSensors[i].SetActive(false);
                    }
                }

                break;
        }
    }

    //血條控球監聽
    private IEnumerator HpConditiondListen(DamageMode mode, object[] par)
    {
        hpActive = true;

        while (hpActive)
        {
            if (!StaticScript.pause)
            {
                hpScales = 1; //血條傾向初始化

                //燈號顏色變換
                byte upCount = 0;
                byte lowCount = 0;
                for (int i = 0; i < ballsPool.Count; i++) //統計上下層各有多少顆球
                {
                    if (ballsPool[i].transform.position.y > UIBehaviour.Instance.refObj.playerBar.transform.position.y)
                    {
                        upCount++;
                    }
                    else
                    {
                        lowCount++;
                    }
                }
                for (int i = 0; i < 5; i++)
                {
                    //上層燈號變換
                    if (UIBehaviour.Instance.cditem.upperSensors[i].activeSelf)
                    {
                        if (( reversingState ? lowCount : upCount ) <= 0) //未激活燈號
                        {
                            UIBehaviour.Instance.cditem.upperSensors[i].GetComponent<Image>().color = UIBehaviour.Instance.cditem.deactivate;
                            hpScales--;
                        }
                        else //激活燈號
                        {
                            UIBehaviour.Instance.cditem.upperSensors[i].GetComponent<Image>().color = UIBehaviour.Instance.cditem.upperActive;
                            if (reversingState)
                            {
                                lowCount--;
                            }
                            else
                            {
                                upCount--;
                            }
                        }
                    }

                    //下層燈號變換
                    if (UIBehaviour.Instance.cditem.lowerSensors[i].activeSelf)
                    {
                        if (( reversingState ? upCount : lowCount ) <= 0) //未激活燈號
                        {
                            UIBehaviour.Instance.cditem.lowerSensors[i].GetComponent<Image>().color = UIBehaviour.Instance.cditem.deactivate;
                            hpScales--;
                        }
                        else //激活燈號
                        {
                            UIBehaviour.Instance.cditem.lowerSensors[i].GetComponent<Image>().color = UIBehaviour.Instance.cditem.lowerActive;
                            if (reversingState)
                            {
                                upCount--;
                            }
                            else
                            {
                                lowCount--;
                            }
                        }
                    }
                }

                //HP恢復或損傷
                float q = hpScales >= 0 ? hpScales * recoverySpeed * listenFrequency : hpScales * damageSpeed * listenFrequency;
                q *= hpCoefficient;

                for (int i = 0; i < UIBehaviour.Instance.cditem.heartDoor.Length; i++) //左右心門等量位移
                {
                    UIBehaviour.Instance.cditem.heartDoor[i].value = Mathf.Clamp(UIBehaviour.Instance.cditem.heartDoor[i].value + q, 0, 1);
                }

                if (UIBehaviour.Instance.cditem.heartDoor[0].value != UIBehaviour.Instance.cditem.heartDoor[1].value) //檢測心門slider value是否相等
                {
                    Debug.Log("[BUG]左右心門Slider Value不等值");
                    UIBehaviour.Instance.cditem.heartDoor[0].value = UIBehaviour.Instance.cditem.heartDoor[1].value;
                }
            }

            yield return new WaitForSeconds(listenFrequency);
        }
    }

    //條件倒轉
    private IEnumerator ReverseCondition()
    {
        isReversing = true;
        float timer = 0; //計時器
        while (isReversing)
        {
            if (!StaticScript.pause) timer += listenFrequency;

            if (timer >= reverseFrequency)
            {
                if (!reversingState)
                {
                    UIBehaviour.Instance.cditem.sensorHolder.Play("RotateForward", 0, 0);
                }
                else
                {
                    UIBehaviour.Instance.cditem.sensorHolder.Play("RotateBack", 0, 0);
                }

                AudioManagerScript.Instance.PlayAudioClip("SE_ROTATE");

                ParticleEffectController.Instance.SetStaticEffect("PAR_ROTATE", true);

                yield return new WaitForSeconds(0.8f);

                ParticleEffectController.Instance.SetStaticEffect("PAR_ROTATE", false);

                reversingState = !reversingState;
                timer = 0;
            }

            yield return new WaitForSeconds(listenFrequency);
        }
    }
}

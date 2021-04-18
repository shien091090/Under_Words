//UI介面控鍵與圖示控制腳本

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class UIBehaviour : MonoBehaviour
{
    //-----------變數宣告------------------------------------------------
    private static UIBehaviour _instance; //單例模式
    public static UIBehaviour Instance
    {
        get
        {
            return _instance;
        }
    }

    public float fastForward = 1; //快進倍率
    public float funtionDelay; //延遲時間(選項選擇後延遲執行方法)
    public float afterSelectionDelay; //延遲時間(選項淡出後消除選項物件)

    [System.Serializable]
    public struct SpeechBalloonObjects //對話框物件
    {
        public GameObject speechBalloon; //對話框
        public GameObject speechCursor; //對話游標
        public GameObject selectionPrefab; //選項預置體
        public Animator BalloonAnimator; //對話框動畫機
        public Text dialogue; //對話Text
    }
    public SpeechBalloonObjects balloonObj;

    [System.Serializable]
    public struct ReferenceObjects //參考物件群
    {
        [Header("Stage")]
        public GameObject playerBar; //控制橫條
        public GameObject wall_up; //牆
        public GameObject wall_right;
        public GameObject wall_left;
        public GameObject wall_down;
        public GameObject botBG; //底部背景
        public GameObject PBricks; //正面磚塊群
        public GameObject NBricks; //負面磚塊群
        public GameObject PMask; //正面磚塊遮罩
        public GameObject NMask; //負面磚塊遮罩
        public GameObject progressPanel; //進度版
        public GameObject title; //關卡標題
        public GameObject cdPanelMask; //控球條件面板遮罩
        public GameObject btn_backToStartMenu; //返回主選單按鈕
        [Header("StartMenu")]
        public GameObject curtain_StartMenu; //主選單的遮蔽幕
        public GameObject canvas_MainMenu; //主要選單畫布
        public GameObject canvas_SubMenu; //副選單畫布
        public GameObject subMenu_StageSelect; //選單_關卡選擇
        public GameObject subMenu_Setting; //選單_設定
        public GameObject subMenu_Collections; //選單_收集品
        public GameObject subMenu_Exit; //選單_退出遊戲
        public GameObject collectionFrame; //收集品選擇框
        public GameObject[] chipIcons; //碎片物件群
        public GameObject collection_backIcon; //收集頁面的返回按鈕
        public GameObject collection_endingButton; //結束按鈕
        public GameObject collection_birthday; //賀圖
        public GameObject resetButton; //重置遊戲按鈕
    }
    public ReferenceObjects refObj;

    [System.Serializable]
    public struct Animators //動畫機物件
    {
        [Header("Stage")]
        public Animator wall_up; //牆動畫機
        public Animator wall_right;
        public Animator wall_left;
        public Animator wall_down;
        public Animator botBG; //底部背景動畫機
        public Animator playerBar; //控制橫條動畫機
        public Animator PMask; //正面磚塊群動畫機
        public Animator NMask; //負面磚塊群動畫機
        public Animator NbricksHolder; //負面磚塊父物件區域動畫機
        public Animator camera; //攝影機動畫機
        public Animator progressPanel; //進度版動畫機
        public Animator chip; //碎片動畫機
        public Animator title; //關卡標題動畫機
        public Animator cdMask; //控球條件面板遮罩動畫機
        public Animator cdPanel; //控球條件面板動畫機
        public Animator btn_backToStartMenu; //返回主選單按鈕動畫機
        [Header("StartMenu")]
        public Animator curtain_StartMenu; //主選單的遮蔽幕動畫機
        public AnimationScript menuChange; //主選單切換動畫機
        public Animator collection_back; //收集品選單的返回按鈕動畫機
    }
    public Animators anim;

    [System.Serializable]
    public struct Components //控件群
    {
        [Header("Stage")]
        public Text pProgressText; //正面進度Text
        public Text nProgressText; //負面進度Text
        public Image titleImage; //關卡標題Image
        public Image chipImage; //碎片Image
        public Sprite[] titleSprites; //關卡標題sprite集合
        public Sprite[] chipSprites; //碎片sprite集合
        public RectTransform downWallRect; //下牆的RectTransform
        [Header("StartMenu")]
        public Button[] stageButtons; //stage按鈕物件集合
        public Image[] stageImages; //stage圖片物件集合
        public CanvasGroup collection_cg; //收集品頁面的canvasGroup
    }
    public Components comp;

    [System.Serializable]
    public struct ConditionItems //控球條件物件群
    {
        [Header("Conditon 1 : 上下層球數條件")]
        public GameObject ballSeveral_cond1; // Conditon 1 : 上下層球數條件
        public GameObject[] upperSensors; //上層燈號群
        public GameObject[] lowerSensors; //下層燈號群
        public Animator sensorHolder; //燈號區域
        public HorizontalLayoutGroup[] layoutComponents; //管理燈號排列的LayoutGroup控件
        public Color upperActive; //上層激活狀態顏色
        public Color lowerActive; //下層激活狀態顏色
        public Color deactivate; //未激活狀態顏色
        [Header("心門參數")]
        public Color recoveryColor; //滿足條件時的顏色(回血)
        public Color damageColor; //不滿足條件時的顏色(扣血)
        public Slider[] heartDoor; //心門slider(左+右)
    }
    public ConditionItems cditem;

    private bool discolorChip = true; //碎片變色開關
    private float timer; //計時器
    private float colorChangeUnit; //顏色變換單位
    private Color registerColor; //註記顏色
    private byte line; //目前句子行數
    private delegate IEnumerator ScenarioMethod(Paragraph p); //對話框執行腳本委派
    private ScenarioMethod scenarioScript; //對話框委派方法
    public List<GameObject> selections; //選項物件對列
    public List<UnityAction> actions; //選項對應方法

    //-----------內建方法------------------------------------------------

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this; //單例模式
        }
    }

    void Start()
    {
        if (SceneManager.GetActiveScene().name == "Stage") scenarioScript = GameController.Instance.ScenarioControl; //設定對話框的執行方法
        if (SceneManager.GetActiveScene().name == "StartMenu") scenarioScript = StartMenuController.Instance.ScenarioControl;
    }

    void Update()
    {
        if (discolorChip && SceneManager.GetActiveScene().name == "Stage") ChangeChipColor(); //碎片顏色變換
    }

    //-----------自訂方法------------------------------------------------

    //返回主選單
    public void BTN_BackToStartMenu()
    {
        anim.chip.Play("ChangeScene", 0, 0);
        anim.btn_backToStartMenu.Play("ButtonFadeOut", 0, 0);
    }

    //磚塊進度更新
    public void ProgressUpdate(bool type)
    {
        AudioManagerScript.Instance.PlayAudioClip("SE_UNLOCK");

        if (type) //正面磚塊
        {
            comp.pProgressText.text = ((((float)BrickBehavior.s_positiveWords - (float)BrickBehavior.s_pCount) / (float)BrickBehavior.s_positiveWords) * 100.0f).ToString("0") + "%"; //更新正面磚塊進度
        }
        else //負面磚塊
        {
            discolorChip = true;
            comp.nProgressText.text = ((((float)BrickBehavior.s_colorAmount - (float)BrickBehavior.s_colorRemain) / (float)BrickBehavior.s_colorAmount) * 100.0f).ToString("0") + "%"; //更新負面磚塊進度
        }

    }

    //依照色彩收集進度返回碎片顏色
    private Color ChipColor(float colorUnit, float time)
    {
        float offset = (GameController.Instance.chipColorChangeTime - (timer <= 0 ? 0 : timer)) * colorUnit;
        timer -= Time.deltaTime;
        if (timer <= 0) discolorChip = false;
        return new Color(offset, offset, offset);
    }

    //變換碎片顏色
    private void ChangeChipColor()
    {
        if (timer <= 0)
        {
            float tarC = (((float)BrickBehavior.s_colorAmount - (float)BrickBehavior.s_colorRemain) / (float)BrickBehavior.s_colorAmount);
            float nowC = comp.chipImage.color.r;

            colorChangeUnit = (tarC - nowC) / GameController.Instance.chipColorChangeTime;
            timer = GameController.Instance.chipColorChangeTime;
            registerColor = comp.chipImage.color;
        }
        comp.chipImage.color = registerColor + ChipColor(colorChangeUnit, timer);
    }

    //對話呈現(接續章節)
    public IEnumerator ShowDialogue(string[] content, float delay, bool pauseMode, SpeechBalloonInfo info, Paragraph p)
    {
        line = 0; //句子行數歸零
        RectTransform r = balloonObj.speechBalloon.GetComponent<RectTransform>();
        if (r.sizeDelta != info.rectSize) r.sizeDelta = info.rectSize; //對話框尺寸
        if (r.localPosition != info.position) r.localPosition = info.position; //對話框位置
        if (balloonObj.dialogue.fontSize != info.fontSize) balloonObj.dialogue.fontSize = info.fontSize; //字體尺寸

        yield return new WaitForSeconds(delay / fastForward);

        StaticScript.pause = pauseMode ? pauseMode : StaticScript.pause; //改變暫停狀態

        if (!balloonObj.speechBalloon.activeSelf) //對話框淡入
        {
            balloonObj.speechBalloon.SetActive(true);
            balloonObj.BalloonAnimator.Play("FadeIn", 0, 0);
        }

        while (true)
        {
            if (!balloonObj.speechCursor.activeSelf) //對話逐字顯示
            {
                yield return new WaitForSeconds(info.insideDelay / fastForward);

                for (int i = 0; i < content[line].Length; i++)
                {
                    balloonObj.dialogue.text += content[line].Substring(i, 1);
                    yield return new WaitForSeconds(info.speechSpeed / fastForward);
                }
                line++; //句子行數增加
                balloonObj.speechCursor.SetActive(true);
            }
            else //在對話閃爍游標出現時按任意鍵繼續對話
            {
                if (Input.anyKeyDown)
                {
                    AudioManagerScript.Instance.PlayAudioClip("UI_DIALOGUEPASS");
                    balloonObj.speechCursor.SetActive(false);
                    balloonObj.dialogue.text = null;

                    if (line > content.Length - 1) break;
                }
            }

            yield return null;
        }

        balloonObj.BalloonAnimator.Play("FadeOut", 0, 0); //對話框淡出
        StartCoroutine(scenarioScript(p)); //進入指定章節
    }

    //對話呈現(選項)
    public IEnumerator ShowSelectionDialogue(string[] content, float delay, bool pauseMode, SpeechBalloonInfo info, List<string> nameList, List<UnityAction> actionList)
    {
        line = 0; //句子行數歸零
        RectTransform r = balloonObj.speechBalloon.GetComponent<RectTransform>();
        if (r.sizeDelta != info.rectSize) r.sizeDelta = info.rectSize; //對話框尺寸
        if (r.localPosition != info.position) r.localPosition = info.position; //對話框位置
        if (balloonObj.dialogue.fontSize != info.fontSize) balloonObj.dialogue.fontSize = info.fontSize; //字體尺寸

        actions = actionList;

        yield return new WaitForSeconds(delay / fastForward);

        StaticScript.pause = pauseMode ? pauseMode : StaticScript.pause; //改變暫停狀態

        if (!balloonObj.speechBalloon.activeSelf) //對話框淡入
        {
            balloonObj.speechBalloon.SetActive(true);
            balloonObj.BalloonAnimator.Play("FadeIn", 0, 0);
        }

        yield return new WaitForSeconds(info.insideDelay / fastForward);

        for (int i = 0; i < content[line].Length; i++)
        {
            balloonObj.dialogue.text += content[line].Substring(i, 1);
            yield return new WaitForSeconds(info.speechSpeed / fastForward);
        }

        yield return new WaitForSeconds(info.sel_delay / fastForward);

        //產生選項
        List<GameObject> sel_go = new List<GameObject>();
        selections = new List<GameObject>();
        Text sel_text;
        Animator sel_anim;

        for (int i = 0; i < nameList.Count; i++)
        {
            sel_go.Add(Instantiate(balloonObj.selectionPrefab, balloonObj.speechBalloon.GetComponent<Transform>())); //產生選項物件
            selections.Add(sel_go[i]); //加入對列

            r = sel_go[i].GetComponent<RectTransform>();
            sel_text = sel_go[i].GetComponentInChildren<Text>();
            sel_anim = sel_go[i].GetComponent<Animator>();

            sel_go[i].transform.localPosition = info.sel_localPosition; //設定位置
            r.sizeDelta = info.sel_size; //設定尺寸
            sel_text.fontSize = info.sel_fontSize; //設定字體尺寸
            sel_text.text = nameList[i]; //設定字串
            sel_go[i].GetComponentInChildren<Button>().onClick.AddListener(FinishSelection); //選項按下後執行特效
            sel_go[i].GetComponent<AnimationBehavior>().SetLocus(sel_go[i].transform.localPosition, new Vector2(info.sel_localPosition.x + info.sel_distance * (0.5f + i - (0.5f * nameList.Count)), info.sel_localPosition.y)); //設定選項位置移動軌跡
            sel_anim.Play("SelelctionFadeIn", 0, 0); //淡入特效
        }
    }

    //設定所選擇選項
    private void FinishSelection()
    {
        int index = 0;
        for (int i = 0; i < selections.Count; i++) //取得所選擇選項索引
        {
            if (EventSystem.current.currentSelectedGameObject.transform.parent.gameObject == selections[i])
            {
                index = i;
                break;
            }
        }
        StartCoroutine(SelectionAnimation(index));
    }

    //選項特效
    private IEnumerator SelectionAnimation(int selected)
    {
        for (int i = 0; i < selections.Count; i++) //按鈕失效
        {
            selections[i].GetComponentInChildren<Text>().raycastTarget = false;
        }

        balloonObj.dialogue.text = null; //清空對話框文字
        balloonObj.BalloonAnimator.Play("Hide", 0, 0);
        selections[selected].GetComponent<AnimationBehavior>().SetLocus(selections[selected].transform.localPosition, new Vector2(0, selections[selected].transform.localPosition.y)); //設定指定選項移動軌跡
        selections[selected].GetComponent<Animator>().Play("Selelcted", 0, 0); //所選擇選項特效
        for (int i = 0; i < selections.Count; i++) //其他未選擇選項特效
        {
            if (i != selected) selections[i].GetComponent<Animator>().Play("SelelctionFadeOut", 0, 0);
        }

        yield return new WaitForSeconds(funtionDelay);

        actions[selected].Invoke();

        yield return new WaitForSeconds(afterSelectionDelay);

        balloonObj.speechBalloon.SetActive(false);
        for (int i = 0; i < selections.Count; i++)
        {
            Destroy(selections[i].gameObject);
        }
        StaticScript.pause = false;
    }
}


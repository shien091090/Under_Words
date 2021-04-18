//開頭主選單腳本控制

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class StartMenuController : MonoBehaviour
{
    //-----------變數宣告------------------------------------------------
    private static StartMenuController _instance; //單例模式
    public static StartMenuController Instance
    {
        get
        {
            return _instance;
        }
    }

    public float speechSpeed; //對話顯示速度
    public float speechOverDelay; //台詞結束後延遲
    public sbyte menuIndexl = -1; //進入的選單索引
    public bool pause;

    [System.Serializable]
    public struct ButtonColors //按鈕顏色
    {
        public Color unlockedColor; //解鎖時按鈕顏色
        public Color hightlightedColor; //解鎖時按鈕焦點顏色
        public Color unlockingPressedColor; //解鎖時按鈕觸發顏色
        public Color lockedColor; //未解鎖時按鈕顏色
        public Color lockingPressdColor; //未解鎖時按鈕觸發顏色
    }
    public ButtonColors btnColor;

    private SpeechBalloonInfo menuType; //主選單對話框樣式
    private SpeechBalloonInfo menuType_sel; //主選單對話框(有選項)樣式
    private bool paramLoadingOver = false; //參數讀取結束

    //-----------內建方法------------------------------------------------
    void Awake()
    {
        if (_instance == null) _instance = this; //賦予單例
    }

    void Start()
    {
        AudioManagerScript.Instance.PlayAudioClip("GAME_OPENING");

        paramLoadingOver = false;
        if (PlayerPrefs.GetInt("GAME_stage5_unlock") == 1) //所有關卡通關時
        {
            UIBehaviour.Instance.comp.collection_cg.blocksRaycasts = false;
            UIBehaviour.Instance.refObj.collection_endingButton.SetActive(true);
            StaticScript.pause = false;
            MenuChange(2); //自動移進收集品頁面
            UIBehaviour.Instance.refObj.collection_backIcon.SetActive(false); //收集品頁面的返回按鈕暫時隱藏
            StartCoroutine(Ending());
        }
        UIBehaviour.Instance.refObj.curtain_StartMenu.SetActive(true);
        UIBehaviour.Instance.anim.curtain_StartMenu.Play("FadeIn", 0, 0); //畫面淡入
        for (int i = 0; i < UIBehaviour.Instance.comp.stageImages.Length; i++) //選擇關卡按鍵啟用
        {
            UIBehaviour.Instance.comp.stageImages[i].raycastTarget = true;
        }

        for (int i = 0; i <= 4; i++) //收集品碎片圖示顯示or隱藏
        {
            UIBehaviour.Instance.refObj.chipIcons[i].SetActive(PlayerPrefs.GetInt("GAME_stage" + ( i + 1 ) + "_unlock") == 1 ? true : false);
        }

        menuType = new SpeechBalloonInfo(new Vector2(0, 60), new Vector2(780, 200), 29, speechSpeed, 0.5f); //設定對話框樣式
        menuType_sel = new SpeechBalloonInfo(new Vector2(0, 60), new Vector2(780, 200), 29, speechSpeed, 0.5f, 0.5f, new Vector2(0, -140), new Vector2(120, 60), 28, 200.0f);

        LoadRecord(); //讀取紀錄參數
    }

    private void Update()
    {
        pause = StaticScript.pause;
        if (Input.GetKeyDown(KeyCode.F12))
        {
            BTN_ResetRecord();
            Debug.Log("重製紀錄");
        }
        if (Input.GetKeyDown(KeyCode.F11))
        {
            PlayerPrefs.DeleteKey("GAME_stage5_unlock");
            Debug.Log("重製第五關紀錄");
        }
        if (Input.GetKeyDown(KeyCode.F10))
        {
            for (int i = 1; i <= 5; i++)
            {
                PlayerPrefs.SetInt("GAME_stage" + i + "_unlock", 1);
            }
            Debug.Log("全部通關");
        }
    }

    //-----------自訂方法------------------------------------------------

    //主選單切換
    public void MenuChange(int index)
    {
        if (!StaticScript.pause)
        {
            switch (index)
            {
                case 0: //關卡選擇
                    AudioManagerScript.Instance.PlayAudioClip("UI_BTN1");

                    UIBehaviour.Instance.refObj.subMenu_StageSelect.SetActive(true);
                    UIBehaviour.Instance.refObj.subMenu_Setting.SetActive(false);
                    UIBehaviour.Instance.refObj.subMenu_Collections.SetActive(false);
                    UIBehaviour.Instance.anim.menuChange.AnimationEvent_Stay("MenuEffect_ChangeToSubMenu");
                    break;

                case 1: //設定
                    AudioManagerScript.Instance.PlayAudioClip("UI_BTN1");

                    UIBehaviour.Instance.refObj.subMenu_StageSelect.SetActive(false);
                    UIBehaviour.Instance.refObj.subMenu_Setting.SetActive(true);
                    UIBehaviour.Instance.refObj.subMenu_Collections.SetActive(false);
                    UIBehaviour.Instance.anim.menuChange.AnimationEvent_Stay("MenuEffect_ChangeToSubMenu");
                    break;

                case 2: //收集品
                    AudioManagerScript.Instance.PlayAudioClip("UI_BTN1");

                    UIBehaviour.Instance.refObj.subMenu_StageSelect.SetActive(false);
                    UIBehaviour.Instance.refObj.subMenu_Setting.SetActive(false);
                    UIBehaviour.Instance.refObj.subMenu_Collections.SetActive(true);
                    UIBehaviour.Instance.anim.menuChange.AnimationEvent_Stay("MenuEffect_ChangeToSubMenu");
                    break;

                case 3: //退出遊戲

                    break;

                case 4: //回主選單
                    AudioManagerScript.Instance.PlayAudioClip("UI_BTN1");

                    UIBehaviour.Instance.anim.menuChange.AnimationEvent_Stay("MenuEffect_ChangeToMainMenu");
                    break;

                case 5: //重置遊戲
                    StartCoroutine(ResetGame());
                    break;
            }

            menuIndexl = (sbyte)index;
        }
    }

    //讀取紀錄
    public void LoadRecord()
    {
        for (int i = 0; i < UIBehaviour.Instance.comp.stageButtons.Length; i++)
        {
            string savingName = "GAME_stage" + i + "_unlock";
            Button button = UIBehaviour.Instance.comp.stageButtons[i];
            Image image = UIBehaviour.Instance.comp.stageImages[i];
            Debug.Log("GAME_stage" + i + "_unlock : " + PlayerPrefs.GetInt(savingName, 0));
            if (PlayerPrefs.GetInt(savingName, 0) == 0 && i != 0)
            {
                ColorBlock colors = button.colors;
                colors.highlightedColor = Color.white;
                colors.pressedColor = btnColor.lockingPressdColor;

                image.color = btnColor.lockedColor; //重新指定按鈕行為的對應顏色
                button.colors = colors; //重新指定按鈕圖片的顏色
            }
            else
            {
                ColorBlock colors = button.colors;
                colors.highlightedColor = btnColor.hightlightedColor;
                colors.pressedColor = btnColor.unlockingPressedColor;

                image.color = btnColor.unlockedColor; //重新指定按鈕行為的對應顏色
                button.colors = colors; //重新指定按鈕圖片的顏色
            }
        }

        paramLoadingOver = true;
    }

    //選擇關卡
    public void StageEnter(int index)
    {
        if (!StaticScript.pause)
        {
            if (index > 1)
            {
                string savingName = "GAME_stage" + ( index - 1 ) + "_unlock";
                if (PlayerPrefs.GetInt(savingName) == 1)
                {
                    StaticScript.stageNumber = index - 1;
                    for (int i = 0; i < UIBehaviour.Instance.comp.stageImages.Length; i++)
                    {
                        UIBehaviour.Instance.comp.stageImages[i].raycastTarget = false;
                    }
                    UIBehaviour.Instance.refObj.curtain_StartMenu.SetActive(true);
                    AudioManagerScript.Instance.PlayAudioClip("UI_BTN2");
                    UIBehaviour.Instance.anim.curtain_StartMenu.Play("EnterStage", 0, 0);
                }
                else
                {
                    AudioManagerScript.Instance.PlayAudioClip("UI_CANCEL1");
                    Debug.Log("Stage " + index + " is Locked!");
                }
            }
            else
            {
                StaticScript.stageNumber = 0;
                for (int i = 0; i < UIBehaviour.Instance.comp.stageImages.Length; i++)
                {
                    UIBehaviour.Instance.comp.stageImages[i].raycastTarget = false;
                }
                UIBehaviour.Instance.refObj.curtain_StartMenu.SetActive(true);
                AudioManagerScript.Instance.PlayAudioClip("UI_BTN2");
                UIBehaviour.Instance.anim.curtain_StartMenu.Play("EnterStage", 0, 0);
            }
        }
    }

    public void BTN_Exit()
    {
        AudioManagerScript.Instance.PlayAudioClip("UI_BTN1");

        Application.Quit();
    }

    //測試用 : 重製關卡
    public void BTN_ResetRecord()
    {
        for (int i = 1; i <= 5; i++)
        {
            PlayerPrefs.DeleteKey("GAME_stage" + i + "_unlock");
        }

        for (int i = 0; i <= 4; i++) //收集品碎片圖示顯示or隱藏
        {
            UIBehaviour.Instance.refObj.chipIcons[i].SetActive(PlayerPrefs.GetInt("GAME_stage" + ( i + 1 ) + "_unlock") == 1 ? true : false);
        }

        LoadRecord();
    }

    //劇本控制
    public IEnumerator ScenarioControl(Paragraph paragraph)
    {
        switch (paragraph)
        {
            case Paragraph.主選單對話結束:
                yield return new WaitForSeconds(speechOverDelay);
                StaticScript.pause = false;
                break;

            default:
                break;
        }

        yield return null;
    }

    //結尾演出
    private IEnumerator Ending()
    {
        yield return new WaitUntil(() => paramLoadingOver); //讀取參數結束後

        yield return new WaitForSeconds(1);

        yield return StartCoroutine(UIBehaviour.Instance.ShowDialogue(new string[] { "你已經收集完所有碎片了。", "按下中間的按鈕，將碎片合而為一。" }, 0, true, menuType, Paragraph.結尾台詞));

        UIBehaviour.Instance.comp.collection_cg.blocksRaycasts = true;
    }

    //重置遊戲演出
    private IEnumerator ResetGame()
    {
        UIBehaviour.Instance.comp.collection_cg.blocksRaycasts = false;

        UIBehaviour.Instance.refObj.curtain_StartMenu.SetActive(true);
        UIBehaviour.Instance.anim.curtain_StartMenu.Play("FadeOut", 0, 0); //畫面淡出

        yield return new WaitUntil(() => UIBehaviour.Instance.anim.curtain_StartMenu.GetCurrentAnimatorStateInfo(0).normalizedTime > 1); //等待動畫撥放結束

        UIBehaviour.Instance.anim.menuChange.AnimationEvent_Stay("MenuEffect_ChangeToMainMenu");

        yield return new WaitForSeconds(1f);

        AudioManagerScript.Instance.Stop(1); //BGM停止

        BTN_ResetRecord();

        UIBehaviour.Instance.comp.collection_cg.blocksRaycasts = true;
        UIBehaviour.Instance.refObj.collection_backIcon.SetActive(true);
        UIBehaviour.Instance.refObj.collection_birthday.SetActive(false);
        UIBehaviour.Instance.refObj.resetButton.SetActive(false);

        UIBehaviour.Instance.anim.curtain_StartMenu.Play("FadeIn", 0, 0); //畫面淡入
    }
}

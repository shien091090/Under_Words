//跨場景的靜態變數

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class StaticScript
{
    public static bool pause = false; //暫停狀態
    public static int stageNumber; //目前關卡數

    //台詞
    public static string[] ActorLines(Serif site)
    {
        if(SceneManager.GetActiveScene().name == "Stage") GameController.Instance.nowDialogue = site; //寫入目前對話

        switch (site)
        {
            #region 共用關卡台詞

            case Serif.完成關卡:
                switch (StaticScript.stageNumber)
                {
                    case 0: //第一關

                        return new string[] { "「做得好，你找到了第一塊純粹碎片。」" };

                    case 1: //第二關
                        return new string[] { "「找到了第二塊純粹碎片。」" };

                    case 2: //第三關
                        return new string[] { "「找到了第三塊純粹碎片。」" };

                    case 3: //第四關
                        return new string[] { "「找到了第四塊純粹碎片。」" };

                    case 4: //第五關
                        return new string[] { "「找到了最後一塊純粹碎片。」" };

                }
                return null;

            case Serif.解鎖碎片:
                switch (StaticScript.stageNumber)
                {
                    case 0: //第一關
                        return new string[] {
                            "「它雖然有圖案和顏色，但並不完整。」",
                            "「繼續尋找吧，它的背後必定隱藏著某個重要的涵義。」"
                        };

                    case 1: //第二關
                        return new string[] {
                            "「已經收集兩塊碎片了，但距離真相還很遙遠。」",
                            "「繼續尋找下一個碎片吧。」"
                        };

                    case 2: //第三關
                        return new string[] {
                            "「你已漸入佳境。」",
                            "「繼續尋找下一個碎片吧。」"
                        };

                    case 3: //第四關
                        return new string[] {
                            "「這段旅程即將到達尾聲。」",
                            "「繼續尋找下一個碎片吧。」"
                        };

                    case 4: //第五關
                        return new string[] {
                            "「你費盡了千辛萬苦、克服了重重難關。」",
                            "「你沒有放棄去尋找隱藏在話語背後的事物。」",
                            "「來吧！用你的雙眼去見證碎片拼湊起來的真相。」"
                        };
                }
                return null;

            case Serif.關卡失敗:
                return new string[] {
                    "「心門關閉了。」",
                    "「但你不能輕言放棄，反省如何讓自己變得更好、如何修正自己的錯誤。」",
                    "「你必須再次敞開心門，去接納這個世界。」"
                };
            #endregion
            #region 第一關卡台詞

            case Serif.開頭導詞:
                return new string[] { "「人活著，倚靠陽光、空氣和水。」" };

            case Serif.生活之牆:
                return new string[] { "「你生活在一個範圍裡，它包含了你的工作、你所關心的事物、以及和你有所連結的所有人。」" };

            case Serif.心之橫條:
                return new string[] { "「它是你的『心』，代表自由的意志，是一切抉擇的核心。」" };

            case Serif.言語之球:
                return new string[] { "「在心上面的球，象徵『話語』。」" };

            case Serif.嘗試移動:
                return new string[] {
                    "「話語的目的，是為了讓生活變得更美好。」",
                    "「試著控制你的心。」(使用方向鍵)"
                };

            case Serif.嘗試擊球:
                return new string[] { "「很好，接下來試著說話。」(按下空白鍵)" };

            case Serif.擊毀磚塊:
                return new string[] { "「你說出了" + BallBehavior.s_PWords[BallBehavior.s_PWords.Count - 1] + "的話語，就照這樣繼續下去吧！」" };

            case Serif.掉落失誤:
                if (GameController.Instance.tut_fallCount == 0) { return new string[] { "「沒關係，不要氣餒，這是需要練習的。」" }; }
                else if (GameController.Instance.tut_fallCount == 1) { return new string[] { "「再試試看，你必須正視每一次說話的機會。」" }; }
                else if (GameController.Instance.tut_fallCount == 2) { return new string[] { "「寬容與耐心是美德，但也是有限度的，再試一次。」" }; }
                else if (GameController.Instance.tut_fallCount == 3) { return new string[] { "「你必須認真一點。」" }; }
                else { return new string[] { "「………。」" }; }

            case Serif.真相一隅:
                return new string[] {
                    "「漸漸的，你注意到話語的背後隱藏著什麼。」",
                    "「生活中充斥著混亂，各種事物、各種刺激，但你隱約感受到了一種純粹。」",
                    "「隱藏在話語背後的純粹。」"
                };

            case Serif.言語失控:
                return new string[] {
                    "「然而，現實並不是如此簡單。」",
                    "「話語比你想像中的更容易失控。」"
                };

            case Serif.負面情緒暴露:
                return new string[] { "「光的背後必定存在影子。」" };

            case Serif.尋找價值:
                return new string[] { "「然而在負面的情感中，也有值得尋找的事物。」" };

            case Serif.找到色彩:
                return new string[] {
                    "「在負面情感中，你找到了『色彩』。」",
                    "「它能讓回憶永保鮮豔，烙印在你的腦海裡。」"
                };
            #endregion
            #region 第二關卡台詞

            case Serif.第二關導詞:
                return new string[] { "「這次，你需要學習一些新事物。」" };

            case Serif.說話技巧:
                return new string[] {
                    "「右方的提示會教導你『說話的技巧』。」",
                    "「控制話語的移動，滿足右方提示。」",
                    "「如果沒有達成條件，右上的『心門』就會逐漸關上。」",
                    "「親自體驗看看吧！」"
                };

            case Serif.嘗試扣血:
                return new string[] {
                    "「盡量讓話語停留在上層，就能滿足條件」",
                    "「反之，當未滿足條件而亮紅燈時，心門就會漸漸關上。」",
                    "「我想你的技巧已經越來越純熟，接下來不會再給太多提示了。」",
                    "「記住，目標只有一個：找出全部的碎片，並保持心門敞開。」"
                };

            case Serif.技巧解說:
                return new string[] {
                    "「你應該注意到了，右方的燈號會隨著話語移動而有所改變。」",
                    "「現在試試刻意不滿足條件，使心門關閉。」",
                    "「簡單來說，想辦法讓右方的燈號變成紅色就可以了，試試看吧。」"
                };

            #endregion
            #region 第三關卡台詞

            case Serif.第三關導詞:
                //return new string[] { };
                return new string[] {
                    "「嘴巴長在人身上，但人卻總是管不住嘴巴。」",
                    "「現在出現了『失控的語言』，你的心控制不住它，但你依然得想辦法掌握狀況。」"
                };

            #endregion
            #region 第四關卡台詞

            case Serif.第四關導詞:
                return new string[] {
                    "「在某些時候，坦率是會造成傷害的。」",
                    "「想辦法控制住自己的『無心快語』。」"
                };

            case Serif.條件倒轉:
                return new string[] {
                    "「右方條件發生倒轉，意味著生活逐漸變得複雜。」",
                    "「面對困境，即使你不知道該如何是好，也要適應它、克服它。」"
                };

            #endregion
            #region 第五關卡台詞

            case Serif.第五關導詞:
                return new string[] {
                    "「這裡是最後、也是最難的一道關卡。」",
                    "「但終點也近在咫尺了。」"
                };

            #endregion
            #region 主選單台詞

            case Serif.主選單台詞:
                return new string[] { "主選單台詞測試。" };

            #endregion

            default:
                return null;
        }

    }
}

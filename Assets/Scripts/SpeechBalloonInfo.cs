//對話框控制腳本

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeechBalloonInfo
{
    public readonly Vector3 position; //位置
    public readonly Vector2 rectSize; //對話框尺寸
    public readonly int fontSize; //字體尺寸
    public readonly float speechSpeed; //對話速度
    public readonly float insideDelay; //內部延遲(彈出對話框到顯示文字之間的延遲)
    public readonly float sel_delay; //接續選項的間隔時間
    public readonly Vector3 sel_localPosition; //選項相對位置
    public readonly Vector2 sel_size; //選項尺寸
    public readonly int sel_fontSize; //選項字體尺寸
    public readonly float sel_distance; //選項平均分配的間隔

    //建構子(無選項)
    public SpeechBalloonInfo(Vector2 pos, Vector2 rSize, int fSize, float speed, float iDelay)
    {
        position = pos;
        rectSize = rSize;
        fontSize = fSize;
        speechSpeed = speed;
        insideDelay = iDelay;
    }

    //建構子(有選項)
    public SpeechBalloonInfo(Vector2 pos, Vector2 rSize, int fSize, float speed, float iDelay, float s_Delay, Vector3 s_Pos, Vector2 s_Size, int s_FSize, float s_dis)
    {
        position = pos;
        rectSize = rSize;
        fontSize = fSize;
        speechSpeed = speed;
        insideDelay = iDelay;

        sel_delay = s_Delay;
        sel_localPosition = s_Pos;
        sel_size = s_Size;
        sel_fontSize = s_FSize;
        sel_distance = s_dis;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ending : MonoBehaviour
{
    //撥放音訊
    public void PlaySound(string clipName)
    {
        AudioManagerScript.Instance.PlayAudioClip(clipName);
    }
}

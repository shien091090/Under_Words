//聲音控制腳本

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController : MonoBehaviour
{
    [System.Serializable]
    public struct EffectSoundClips
    {
        [Header("Stage")]
        public AudioClip test;
    }
    public EffectSoundClips esClip;

    public void PlayClip()
    {
        //AudioSource.PlayClipAtPoint();
    }
}

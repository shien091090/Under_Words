using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    public KeyCode[] testKey;
    public delegate void Del_CallFunc(params object[] inputParams);

    void Update()
    {
        for (int i = 0; i < testKey.Length; i++)
        {
            if (Input.GetKeyDown(testKey[i]))
            {
                this.SendMessage("Test" + ( i + 1 ).ToString());
            }
        }

    }

    private void Test1()
    {
        AudioManagerScript.Instance.PlayAudioClip("A");
    }

    private void Test2()
    {
        AudioManagerScript.Instance.Stop(0);
    }

    private void Test3()
    {
        AudioManagerScript.Instance.SetPause(0);
    }

    private void Test4()
    {

    }
}

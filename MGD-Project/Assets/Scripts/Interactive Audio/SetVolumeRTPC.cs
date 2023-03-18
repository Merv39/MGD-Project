using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetVolumeRTPC : MonoBehaviour
{
    public AK.Wwise.RTPC PlayerStateArousalRTPC;

    // Update is called once per frame
    void Update()
    {
        PlayerStateArousalRTPC.SetGlobalValue(PlayerPrefs.GetFloat("gameVolume", 0.3f));
    }
}

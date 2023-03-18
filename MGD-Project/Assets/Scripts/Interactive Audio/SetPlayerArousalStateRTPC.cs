using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetPlayerArousalStateRTPC : MonoBehaviour
{
    public AK.Wwise.RTPC PlayerStateArousalRTPC;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        PlayerStateArousalRTPC.SetGlobalValue(99f);
        //Debug.Log(PlayerStateArousalRTPC.GetGlobalValue());
    }
}

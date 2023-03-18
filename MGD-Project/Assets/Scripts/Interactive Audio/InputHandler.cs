using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    float pleasant;
    float activation;
    public AK.Wwise.RTPC PlayerStateArousalRTPC;

    // X Axis on MIDI CC1
    void OnPleasantAxis(InputValue value)
    {
        //print("Pleasant: "+value.Get<float>());
        pleasant = value.Get<float>();
    }

    // Y Axis on MIDI CC2
    void OnActivationAxis(InputValue value)
    {
        //print("Activation: " + value.Get<float>());
        activation = value.Get<float>();
        SetPlayerArousalState(activation);
    }

    void SetPlayerArousalState(float f)
    {
        PlayerStateArousalRTPC.SetGlobalValue(f);
    }

    private void Update()
    {
        print((pleasant, activation));
    }
}

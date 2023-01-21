using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    // Y Axis on MIDI CC1
    void OnActivationAxis(InputValue value)
    {
        print("Activation: "+value.Get<float>());
    }

    // X Axis on MIDI CC2
    void OnPleasantAxis(InputValue value)
    {
        print("Pleasant: "+value.Get<float>());
    }
}

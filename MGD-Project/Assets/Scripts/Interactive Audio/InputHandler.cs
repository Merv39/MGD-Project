using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    float pleasant;
    float activation;

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
    }

    private void Update()
    {
        print((pleasant, activation));
    }
}

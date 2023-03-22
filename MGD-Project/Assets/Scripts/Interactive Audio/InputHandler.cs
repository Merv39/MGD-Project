using UnityEngine;
using UnityEngine.InputSystem;

using System.Collections;
using System.IO.Ports;
public class InputHandler : MonoBehaviour
{
    float pleasant;
    float activation;
    public AK.Wwise.RTPC PlayerStateArousalRTPC;

    void SetPlayerArousalState(float f)
    {
        PlayerStateArousalRTPC.SetGlobalValue(f);
    }

    void compareEfficiency(float[] val, System.Func<float[], float> f1)
    {
        float T1 = Time.time;
        //function call here
        f1(val);

        float T2 = Time.time;
        print(T2 - T1);
    }

    private void Start()
    {
        float[] test = { 9, 10, 12, 13, 13, 13, 15, 15, 16, 16, 18, 22, 23, 24, 24, 25 };
        //compareEfficiency(test, standardDeviation);
        print(mean(test));
        print(standardDeviation(test));
        float[] zVal = zScore(test);
        for (int i = 0; i < zVal.Length; i++)
        {
            print(zVal[i]);
        }
        print(mean(zScore(test)));
    }
    private void Update()
    {
        //print((pleasant, activation));
        //float[] test = { 9, 10, 12, 13, 13, 13, 15, 15, 16, 16, 18, 22, 23, 24, 24, 25 };
        //print(mean(test));
        //print(standardDeviation(test));
    }

    //THIS SECTION IS FOR THE PROTOTYPE WITH PYTHON TO MIDI
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

    //THIS SECTION IS FOR DIRECT DATA INPUT FROM THE ARDUINO

    //SerialPort 

    float mean(float[] values) {
        float sum = 0;
        for (int i = 0; i < values.Length; i++) {
            sum += values[i];
        }
        sum = sum / values.Length;
        return sum;
    }

    float standardDeviation(float[] values) {
        //calculate variance
        float sum = 0;
        float meanVal = mean(values);
        for (int i = 0; i < values.Length; i++)
        {
            sum += Mathf.Pow(values[i] - meanVal, 2) ;
        }
        float variance = sum / values.Length;
        //sd = sqrt(variance)
        return Mathf.Sqrt(variance);
    }

    //Overloaded version for efficiency if meanVal already calculated
    public float standardDeviation(float[] values, float meanVal)
    {
        //calculate variance
        float sum = 0;
        for (int i = 0; i < values.Length; i++)
        {
            sum += Mathf.Pow(values[i] - meanVal, 2);
        }
        float variance = sum / values.Length;
        //sd = sqrt(variance)
        return Mathf.Sqrt(variance);
    }

    /// <summary>
    /// takes in a set of n values and calulates the Z Score for each
    /// </summary>
    /// <param name="values"></param>
    /// <returns>the list of Z scores</returns>
    float[] zScore(float[] values) {
        float meanVal = mean(values);
        float deviationVal = standardDeviation(values, meanVal);
        //subtract the mean from the values to center the distribution curve at 0
        float[] zVal = new float[values.Length];
        for (int i = 0; i < values.Length; i++)
        {
            zVal[i] = (values[i] - meanVal)/deviationVal;
        }
        return zVal;
    }
}

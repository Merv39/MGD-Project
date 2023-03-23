using UnityEngine;
using UnityEngine.InputSystem;

using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
public class InputHandler : MonoBehaviour
{
    float pleasant;
    float activation;
    public AK.Wwise.RTPC PlayerStateArousalRTPC;
    SerialPort arduino;

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
        arduino = new SerialPort();
        arduino.PortName = "COM3";
        arduino.BaudRate = 9600;
        arduino.Open();

/*        float[] test = { 9, 10, 12, 13, 13, 13, 15, 15, 16, 16, 18, 22, 23, 24, 24, 25 };
        //compareEfficiency(test, standardDeviation);
        print(mean(test));
        print(standardDeviation(test));
        float[] zVal = zScore(test);
        for (int i = 0; i < zVal.Length; i++)
        {
            print(zVal[i]);
        }
        print(mean(zScore(test)));*/
    }

    //A buffer is needed because Unity's Update() command runs every frame and causes timeout during ReadLine() 
    //string buffer = ""; //expects 3 character input
    static int samples = 5;
    List<int> values = new List<int>(); //set this to a history to calculate mean
    //the top of the array values[0] is always the latest value
    private void Update()
    {
        //read data from serial connection and store in values
        try
        {
            values.Insert(0, int.Parse(arduino.ReadLine()));
        }
        catch (System.Exception) {
        }

        //wait for first [samples] number of samples
        if (!(values.Count() < samples))
        {
            //need to set a suitable max min and mid so that the audio effects are triggered
            //float max = 676; //sensor's theoretical max is 1023 but real-world max is 676
            //float min;
            float mid = mean(values);       //need to determine a suitable value for neutral level of arousal

            print(values[0]);
            float arousal = sigmoid(values[0] - mid);
            PlayerStateArousalRTPC.SetGlobalValue(arousal);
        }



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
    float mean(List<float> values) {
        float sum = 0;
        for (int i = 0; i < values.Count; i++) {
            sum += values[i];
        }
        sum = sum / values.Count;
        return sum;
    }

    float standardDeviation(List<float> values) {
        //calculate variance
        float sum = 0;
        float meanVal = mean(values);
        for (int i = 0; i < values.Count; i++)
        {
            sum += Mathf.Pow(values[i] - meanVal, 2) ;
        }
        float variance = sum / values.Count;
        //sd = sqrt(variance)
        return Mathf.Sqrt(variance);
    }

    //Overloaded version for efficiency if meanVal already calculated
    public float standardDeviation(List<float> values, float meanVal)
    {
        //calculate variance
        float sum = 0;
        for (int i = 0; i < values.Count; i++)
        {
            sum += Mathf.Pow(values[i] - meanVal, 2);
        }
        float variance = sum / values.Count;
        //sd = sqrt(variance)
        return Mathf.Sqrt(variance);
    }

    /// <summary>
    /// takes in a set of n values and calulates the Z Score for each
    /// </summary>
    /// <param name="values"></param>
    /// <returns>the list of Z scores</returns>
    float[] zScore(List<float> values) {
        float meanVal = mean(values);
        float deviationVal = standardDeviation(values, meanVal);
        //subtract the mean from the values to center the distribution curve at 0
        float[] zVal = new float[values.Count];
        for (int i = 0; i < values.Count; i++)
        {
            zVal[i] = (values[i] - meanVal)/deviationVal;
        }
        return zVal;
    }

    /// <summary>
    /// Sigmoid expects signed number, where x = 0 returns 0.5
    /// </summary>
    float sigmoid(float x) {
        //different sigmoid functions give different slopes
        //Steepness: tanh > arctan > logistic
        //after testing logistic, it seems that logistic is still too high: change L and K of logistic?
        return logistic(x);
        //return tanh(x);  
        //return arctan(x);
    }

    /// <summary>
    ///default logistic function
    /// </summary>
    float logistic(float x) { return 1 / (1 + Mathf.Exp(-x)); } //range [0, 1]

    /// <summary>
    ///tanh range [-1, 1] so +1 and divide by 2 to map to range [0, 1]
    /// </summary>
    float tanh(float x) {
        float result = (Mathf.Exp(x) - Mathf.Exp(-x)) / (Mathf.Exp(x) + Mathf.Exp(-x));
        if (result + 1f == 0) { return 0; }
        else {
            return (result + 1f) / 2f;
        }
    }

    /// <summary>
    /// returns arctan mapped to range [0, 1]
    /// asymptote of arctan(x) is y=(+-)pi/2
    /// </summary>
    float arctan(float x)
    {
        float result = Mathf.Atan(x);
        if (result == 0) { return 0.5f; }
        else
        {
            return (result / Mathf.PI) + 0.5f;
        }
    }

    float map(float n, float[] oldRange, float[] newRange) {
        return (n - oldRange[0]) / (oldRange[1] - oldRange[0]) * (newRange[1] - newRange[0]) + newRange[0];
    }
}

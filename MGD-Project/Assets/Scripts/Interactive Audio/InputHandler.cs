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
    float currentValue;

    void SetPlayerArousalState(float f)
    {
        currentValue = f;
        PlayerStateArousalRTPC.SetGlobalValue(f);
    }

    void compareEfficiency(List<float> val, System.Func<List<float>, float> f1)
    {
        float T1 = Time.realtimeSinceStartup;
        //function call here
        f1(val);

        float T2 = Time.realtimeSinceStartup;
        print(T2 - T1);
    }
    private void Start()
    {
        arduino = new SerialPort();
        arduino.PortName = "COM3";
        arduino.BaudRate = 600; //baud rate any higher caused errors
        arduino.ReadTimeout = 1;    //creating a timeout to avoid taking all of CPU time
        arduino.Open();

        SetPlayerArousalState(0.5f);
    }

    static int samples = 20; //change the number of samples to change the variability of the signal
    List<float> values = new List<float>(); //set this to a history to calculate mean
    int heartRate;
    //the top of the array values[0] is always the latest value

    string buffer = ""; //expects 3 character input
    bool toggle = true; //true = bufferGSR, false = bufferPPG 
    private void Update()
    {
        //print avg frame rate:
        //print("FPS: "+Time.frameCount / Time.time);
        //GOAL: 60fps (game code is unoptimized so more FPS is needed for a smooth experience)

        //read data from serial connection and store in bufferGSR
        try
        {
            string c = ((char)arduino.ReadChar()).ToString();

            if (!int.TryParse(c, out int _)) //checks if the string is a digit
                return;
            buffer += c;
        }
        catch (System.Exception) {
        }

        //the whole sample takes 3 frames using a bufferGSR
        //this is to allow concurrency and prevent long frametimes compared to reading the whole line
        if (buffer.Length == 3) {
            //acknowledge and reset
            if (toggle) { values.Insert(0, int.Parse(buffer)); }
            else { heartRate = int.Parse(buffer); }
            toggle = !toggle;
            buffer = "";
        }

        //wait for first [samples] number of samples
        if (!(values.Count() < samples))
        {
            print(values[0]);
            float zVal = zScore(values)[0]; //the z score of the newest samepl
            float arousal = sigmoid(zVal);
            print("Arousal:"+arousal);
            print("BPM:" + heartRate);
            print("logistic " + logistic(heartRate, 70, 0.1f));
            if (heartRate > 40 && heartRate < 200) //threshold for extremeties
            {
                //Increase by 0.1 until the value
                float arousalValue = 0.4f * arousal + 0.6f * logistic(heartRate, 70, 0.1f); //weighted values
                if (currentValue < arousalValue)
                {
                    SetPlayerArousalState(currentValue + 0.001f);
                }
                else if (currentValue > arousalValue)
                {
                    SetPlayerArousalState(currentValue - 0.001f);
                }
                //SetPlayerArousalState(arousal * logistic(heartRate, 80, 0.1f));
            }
            else {
                //if values are invalid, slowly return to neutral
                if (currentValue < 0.5f)
                {
                    SetPlayerArousalState(currentValue + 0.001f);
                }
                else if (currentValue > 0.5f)
                {
                    SetPlayerArousalState(currentValue - 0.001f);
                }
            }
        }

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
    List<float> zScore(List<float> values) {
        float meanVal = mean(values);
        float deviationVal = standardDeviation(values, meanVal);
        //subtract the mean from the values to center the distribution curve at 0
        List<float> zVal = new List<float>(values.Count);
        for (int i = 0; i < values.Count; i++)
        {
            zVal.Add((values[i] - meanVal)/deviationVal);
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
    float logistic(float x, float midpoint, float steepness) {
        return 1 / (1 + Mathf.Exp(steepness * -(x-midpoint)));
    }

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

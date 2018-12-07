using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeviceText : MonoBehaviour
{
    public MicInput mic;

    // Update is called once per frame
    void Update()
    {
        GetComponent<Text>().text = mic.deviceText + "\n" + mic.currentAverage;
    }
}

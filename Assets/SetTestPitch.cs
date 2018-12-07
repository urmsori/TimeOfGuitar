using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetTestPitch : MonoBehaviour
{
    public PitchPatternMatcher matcher;
    public string[] pitches = new string[0];

    // Use this for initialization
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            matcher.pitchNames = pitches;
        });
    }

    // Update is called once per frame
    void Update()
    {

    }
}

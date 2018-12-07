using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowMatchedText : MonoBehaviour
{
    public PitchPatternMatcher matcher;

    // Use this for initialization
    void Update()
    {
        string text = "";
        foreach(var matched in matcher.matchedNames)
        {
            text += matched + "\n";
        }
        GetComponent<Text>().text = text;
    }
}

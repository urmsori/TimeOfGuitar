using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowTargetMelody : MonoBehaviour
{
    public PitchPatternMatcher matcher;

    void Update()
    {
        string text = "";
        foreach (var name in matcher.pitchNames)
        {
            text += name + "\n";
        }
        GetComponent<Text>().text = text;
    }
}

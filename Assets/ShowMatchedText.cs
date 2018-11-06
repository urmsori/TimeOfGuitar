using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowMatchedText : MonoBehaviour
{
    public PitchPatternMatcher matcher;

    // Use this for initialization
    void Start()
    {
        matcher.onMatched.AddListener(() =>
        {
            GetComponent<Text>().text += "\nMatched";
        });
    }
}

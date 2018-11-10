using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicControlTest : MonoBehaviour
{
    public PitchPatternMatcher matcher;

    private AudioSource audio;
    // Use this for initialization
    void Start()
    {
        audio = GetComponent<AudioSource>();
        matcher.onMatched.AddListener(() =>
        {
            if (audio.isPlaying)
                audio.Pause();
            else
                audio.UnPause();
        });
    }

    // Update is called once per frame
    void Update()
    {

    }
}

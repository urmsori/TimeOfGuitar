using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugImage : MonoBehaviour
{
    public RectTransform rectTransform;
    public PitchAnalyzer analyzer;

    private float saveX;
    private const float MULTIPLIER = 10;

    // Use this for initialization
    void Start()
    {
        saveX = rectTransform.anchoredPosition.x;
        rectTransform.anchoredPosition = new Vector2(saveX, 0);
    }

    // Update is called once per frame
    void Update()
    {
        var timeline = analyzer.NoteTimeline;
        if (timeline.Count > 0)
        {
            var notes = timeline[timeline.Count - 1].Notes;
            var note = notes[notes.Count - 1];


            rectTransform.anchoredPosition = new Vector2(saveX, note.Index * MULTIPLIER);
        }
        else
        {
            rectTransform.anchoredPosition = new Vector2(saveX, 0);
        }


        //var timeline = analyzer.NoteTimeline;

        //if (timeline.Count > 0)
        //{
        //    var note = timeline[timeline.Count - 1];
        //    rectTransform.anchoredPosition = new Vector2(saveX, note.Notes[0].Index*MULTIPLIER);
        //}
        //else
        //{
        //}
    }
}

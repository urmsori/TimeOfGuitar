using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugText : MonoBehaviour
{
    public PitchAnalyzer analyzer;

    private Text text;
    // Use this for initialization
    void Start()
    {
        text = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        string total = "";
        //var target = analyzer.DonimantNoteOrNull;
        //if(target != null)
        //{
        //    total += target.Name;
        //}

        foreach (var notes in analyzer.NoteTimeline)
        {
            total += string.Format("<{0}>\n", notes.Time);
            foreach (var note in notes.Notes)
            {
                total += note.Name + "\n";
            }
        }
        text.text=total;
    }
}

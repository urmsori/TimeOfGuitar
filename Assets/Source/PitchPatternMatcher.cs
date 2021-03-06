﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PitchPatternMatcher : MonoBehaviour
{
    public NoteDBReader noteDBReader;
    public PitchAnalyzer analyzer;
    public string[] pitchNames = new string[0];
    public List<string> matchedNames = new List<string>();

    [Serializable]
    public class MatchedEvent : UnityEvent { }
    [SerializeField]
    public MatchedEvent onMatched;

    private bool mIsMatchedPrevious = false;

    // Update is called once per frame
    void Update()
    {
        matchedNames.Clear();

        if (pitchNames.Length == 0)
            return;

        Queue<ANote> notes = new Queue<ANote>();
        foreach (var name in pitchNames)
        {
            var note = noteDBReader.NameToNoteOrNull(name);
            if (note != null)
            {
                notes.Enqueue(note);
            }
        }

        if (notes.Count == 0)
            return;

        foreach (var timeline in analyzer.NoteTimeline)
        {
            foreach (var note in timeline.Notes)
            {
                if (notes.Count > 0)
                {                    
                    if (note.EqualsOrDouble(notes.Peek()))
                    {
                        var deleted = notes.Dequeue();
                        matchedNames.Add(deleted.Name);
                    }
                }
            }
        }

        if (notes.Count == 0)
        {
            if (!mIsMatchedPrevious)
            {
                if (onMatched != null)
                    onMatched.Invoke();
            }

            mIsMatchedPrevious = true;
        }
        else
        {
            mIsMatchedPrevious = false;
        }
    }
}

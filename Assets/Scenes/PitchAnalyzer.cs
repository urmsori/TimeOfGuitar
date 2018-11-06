using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PitchAnalyzer : MonoBehaviour
{
    public MicInput input;
    
    public List<TimeAndNotes> NoteTimeline = new List<TimeAndNotes>();
    public ANote DonimantNoteOrNull = null;
    // Update is called once per frame
    void Update()
    {
        var buffer = input.NoteBuffer;

        if(buffer.Count == 0)
        {
            DonimantNoteOrNull = null;
        }
        else
        {
            Dictionary<ANote, int> indexNumMap = new Dictionary<ANote, int>();
            foreach (var noteAndTime in buffer)
            {
                if(indexNumMap.ContainsKey(noteAndTime.Note))
                {
                    indexNumMap[noteAndTime.Note]++;
                }
                else
                {
                    indexNumMap.Add(noteAndTime.Note, 1);
                }
            }

            ANote target = null;
            int most = 0;
            foreach (var pair in indexNumMap)
            {
                if(most < pair.Value)
                {
                    target = pair.Key;
                }
            }

            DonimantNoteOrNull = target;
        }

        Dictionary<float, List<ANote>> noteWholeTimeline = new Dictionary<float, List<ANote>>();
        float currentTime = 0;
        foreach (var noteAndTime in buffer)
        {
            if (currentTime == noteAndTime.Time)
            {
                noteWholeTimeline[noteAndTime.Time].Add(noteAndTime.Note);
            }
            else
            {
                noteWholeTimeline.Add(noteAndTime.Time, new List<ANote>() { noteAndTime.Note });
            }

            currentTime = noteAndTime.Time;
        }

        NoteTimeline.Clear();
        List<ANote> prevNotes = null;
        foreach (var notesPair in noteWholeTimeline)
        {
            var time = notesPair.Key;
            var notes = notesPair.Value;

            if (prevNotes != null)
            {
                foreach (var note in notes)
                {
                    if (prevNotes.Find((x) => { return x.Index == note.Index; }) == null)
                    {
                        // 새로운 음
                        var finded = NoteTimeline.Find((x) => { return x.Time == time; });
                        if (finded == null)
                        {
                            NoteTimeline.Add(new TimeAndNotes() { Time = time, Notes = new List<ANote>() { note } });
                        }
                        else
                        {
                            finded.Notes.Add(note);
                        }
                    }
                    else
                    {
                        // 이전것과 겹침
                    }
                }
            }
            else
            {
                NoteTimeline.Add(new TimeAndNotes() { Time = time, Notes = notes });
            }
            


            prevNotes = notes;
        }
    }
}

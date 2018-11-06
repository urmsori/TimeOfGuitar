using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class NoteDBReader : MonoBehaviour
{
    public const string FileName = "NoteDB.txt";

    public event Action<List<ANote>> ReadDone;
    public List<ANote> Values = new List<ANote>();

    public Dictionary<string, ANote> NoteFromName = new Dictionary<string, ANote>();

    // Use this for initialization
    void Start()
    {
        var lines = File.ReadAllLines(Path.Combine(Application.dataPath, FileName));

        int index = 0;
        foreach (var line in lines)
        {
            var split = line.Split('\t');

            ANote note = new ANote()
            {
                Frequency = float.Parse(split[1]),
                Index = index,
                Name = split[0]
            };

            Values.Add(note);

            var noteSplit = split[0].Split('/');
            foreach (var noteName in noteSplit)
            {
                NoteFromName.Add(noteName, note);
            }
            if(noteSplit.Length >= 2)
            {
                NoteFromName.Add(split[0], note);
            }
            index++;
        }
        if (ReadDone != null)
        {
            ReadDone.Invoke(Values);
        }
    }

    public ANote AssumeNoteFromFrequency(float frequency)
    {
        var assumeIndex = Mathf.Log(frequency / 15.434f) / 0.0578f;
        int center = (int)assumeIndex;

        if (center <= 0)
        {
            center = 1;
        }
        if (center >= Values.Count - 1)
        {
            center = Values.Count - 2;
        }

        var minDiffFrequency = float.MaxValue;
        var minIndex = center - 1;
        for (int i = -1; i <= 1; i++)
        {
            var diff = Mathf.Abs(Values[center + i].Frequency - frequency);
            if (minDiffFrequency > diff)
            {
                minDiffFrequency = diff;
                minIndex = center + i;
            }
        }

        return Values[minIndex];
    }
}

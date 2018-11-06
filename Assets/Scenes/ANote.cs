using System;
using System.Collections.Generic;

public class ANote
{
    public int Index { get; set; }
    public string Name { get; set; }
    public float Frequency { get; set; }

    public override bool Equals(object obj)
    {
        if (obj is ANote)
        {
            return Index == (obj as ANote).Index;
        }
        return false;
    }

    public override int GetHashCode()
    {
        var hashCode = -1493571310;
        hashCode = hashCode * -1521134295 + Index.GetHashCode();
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
        hashCode = hashCode * -1521134295 + Frequency.GetHashCode();
        return hashCode;
    }
}
public class TimeAndANote
{
    public ANote Note { get; set; }
    public float Time { get; set; }
}
public class TimeAndNotes
{
    public float Time { get; set; }
    public List<ANote> Notes { get; set; }
}

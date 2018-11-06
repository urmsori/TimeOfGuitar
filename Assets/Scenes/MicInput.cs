using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MicInput : MonoBehaviour
{
    public string deviceName = null;
    public AnimationCurve curve;
    public NoteDBReader noteDbReader;
    public float bufferTimeMax = 4.0f;

    private const int SAMPLE_SIZE = 4096;
    private const float LOUD_PERCENT_THRESHOLD = 200.0f;

    private const float FREQUENCY_MAX = 24000.0f;

    private AudioSource mAudio;

    private float[] mSamples = new float[SAMPLE_SIZE];

    private List<TimeAndANote> mNoteList = new List<TimeAndANote>();
    public List<TimeAndANote> NoteBuffer { get { return mNoteList; } }

    // Use this for initialization
    void Start()
    {
        mAudio = GetComponent<AudioSource>();

        var devs = Microphone.devices;
        if (devs.Length > 0)
        {
            deviceName = devs[0];
        }
        print(deviceName);

        mAudio.clip = Microphone.Start(deviceName, true, 1, 44100);
        mAudio.loop = true;

        mAudio.Play();
    }

    // Update is called once per frame
    void Update()
    {
        if (mAudio != null && Microphone.IsRecording(null))
        {
            mAudio.GetSpectrumData(mSamples, mAudio.clip.channels, FFTWindow.Blackman);

            // Filter
            float totalLoudness = 0;
            foreach (var sample in mSamples)
            {
                totalLoudness += sample;
            }
            float averageLoudness = totalLoudness / mSamples.Length;

            // Get Peaks
            List<KeyValuePair<int, float>> validPeaks = new List<KeyValuePair<int, float>>();
            for (int i = 1; i < mSamples.Length - 1; i++)
            {
                // For Debug
                Debug.DrawLine(new Vector3(i - 1, mSamples[i] + 10, 0), new Vector3(i, mSamples[i + 1] + 10, 0), Color.red);
                Debug.DrawLine(new Vector3(i - 1, Mathf.Log(mSamples[i - 1]) + 10, 2), new Vector3(i, Mathf.Log(mSamples[i]) + 10, 2), Color.cyan);
                Debug.DrawLine(new Vector3(Mathf.Log(i - 1), mSamples[i - 1] - 10, 1), new Vector3(Mathf.Log(i), mSamples[i] - 10, 1), Color.green);
                Debug.DrawLine(new Vector3(Mathf.Log(i - 1), Mathf.Log(mSamples[i - 1]), 3), new Vector3(Mathf.Log(i), Mathf.Log(mSamples[i]), 3), Color.blue);

                // Average 몇 배 이상인 Sample만 챙김
                if (mSamples[i] / averageLoudness > LOUD_PERCENT_THRESHOLD)
                {
                    if (i > 0 && i < mSamples.Length - 1)
                    {
                        // Is Peak?
                        if (mSamples[i - 1] < mSamples[i] && mSamples[i] > mSamples[i + 1])
                        {
                            validPeaks.Add(new KeyValuePair<int, float>(i, mSamples[i]));
                        }
                    }
                }
            }

            // Top 2개만 챙김
            validPeaks.Sort((x, y) => { return y.Value.CompareTo(x.Value); }); // Y축 Sort

            List<KeyValuePair<int, float>> targetPeaks = new List<KeyValuePair<int, float>>();
            int peakNum = 0;
            foreach (var peak in validPeaks)
            {
                if (!(peakNum < 2))
                    break;
                targetPeaks.Add(peak);
                peakNum++;
            }

            // 가장 낮은 Frequency가 앞에 오도록
            targetPeaks.Sort((x, y) => { return x.Key.CompareTo(y.Key); }); // X축 Sort

            // Add Note
            foreach (var peak in targetPeaks)
            {
                var freq = peak.Key / (float)mSamples.Length * FREQUENCY_MAX;
                var note = noteDbReader.AssumeNoteFromFrequency(freq);

                mNoteList.Add(new TimeAndANote() { Note = note, Time = Time.time });
            }

            // For Debug
            if (targetPeaks.Count > 0)
            {
                //print("----------");
                var peak = targetPeaks[0];
                //print(peak.Key);

                var freq = peak.Key / (float)mSamples.Length * FREQUENCY_MAX;
                //print(freq);
                var note = noteDbReader.AssumeNoteFromFrequency(freq);
                //print(note);

                Debug.DrawLine(new Vector3(Mathf.Log(peak.Key - 1), Mathf.Log(peak.Value), 3), new Vector3(Mathf.Log(peak.Key + 1), Mathf.Log(peak.Value), 3), Color.red);
            }

        }

        // Clear Old Note
        if (mNoteList.Count > 0)
        {
            var currentTime = Time.time;
            while (currentTime - mNoteList[0].Time > bufferTimeMax)
            {
                mNoteList.RemoveAt(0);
                if (mNoteList.Count == 0)
                    break;
            }

            // Debug
            //string total = "";
            //foreach (var note in mNoteList)
            //{
            //    total += note.Time + ": " + note.Note.Name + "\n";
            //}
            //print(total);
        }
    }

    void OnDisable()
    {
        mAudio.Stop();
    }
}

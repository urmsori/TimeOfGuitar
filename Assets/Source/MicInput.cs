#define DEBUG_LINE

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MicInput : MonoBehaviour
{
    public string deviceName = null;
    public AnimationCurve curve;
    public NoteDBReader noteDbReader;
    public float bufferTimeMax = 2.0f;

    private const int SAMPLE_SIZE = 4096;
    private float LOUD_PERCENT_THRESHOLD = 2.0f;

    private AudioSource mAudio;

    private float[] mSamplesRaw = new float[SAMPLE_SIZE];
    private float[] mSamples = new float[SAMPLE_SIZE];

    private List<TimeAndANote> mNoteList = new List<TimeAndANote>();
    public List<TimeAndANote> NoteBuffer { get { return mNoteList; } }

    float mFrequencyMax = 24000.0f; //AudioSettings.outputSampleRate / 2

    public string deviceText = "";
    public float currentAverage = 0;
    
    public void SetThreshold(float value)
    {
        LOUD_PERCENT_THRESHOLD = value;
    }

    // Use this for initialization
    void Start()
    {
        mFrequencyMax = AudioSettings.outputSampleRate / 2;
        mAudio = GetComponent<AudioSource>();

        var devs = Microphone.devices;
        if (devs.Length > 0)
        {
            deviceName = devs[0];
        }

        int min = 0;
        int max = 0;
        Microphone.GetDeviceCaps(deviceName, out min, out max);
        deviceText = deviceName + ", " + max;
        print(deviceText);

        mAudio.clip = Microphone.Start(deviceName, true, 1, max);
        while (!(Microphone.GetPosition(deviceName) > 1))
        {
            // Wait until the recording has started
        }

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
            int upperLength = 0;
            foreach (var sample in mSamples)
            {
                if (averageLoudness < sample)
                {
                    totalLoudness += sample;
                    upperLength++;
                }
            }
            averageLoudness = totalLoudness / upperLength;
            currentAverage = Mathf.Log(averageLoudness);

            // Get Peaks
            List<KeyValuePair<int, float>> validPeaks = new List<KeyValuePair<int, float>>();
            for (int i = 1; i < mSamples.Length - 1; i++)
            {
#if DEBUG_LINE
                // For Debug
                Debug.DrawLine(new Vector3(i - 1, mSamples[i] + 10, 0), new Vector3(i, mSamples[i + 1] + 10, 0), Color.red);
                Debug.DrawLine(new Vector3(i - 1, Mathf.Log(mSamples[i - 1]) + 10, 2), new Vector3(i, Mathf.Log(mSamples[i]) + 10, 2), Color.cyan);
                //Debug.DrawLine(new Vector3(Mathf.Log(i - 1), mSamples[i - 1] - 10, 1), new Vector3(Mathf.Log(i), mSamples[i] - 10, 1), Color.green);
                var avgline = Mathf.Log(LOUD_PERCENT_THRESHOLD * averageLoudness);
                Debug.DrawLine(new Vector3(Mathf.Log(i - 1), avgline, 1), new Vector3(Mathf.Log(i), avgline, 1), Color.green);
                Debug.DrawLine(new Vector3(Mathf.Log(i - 1), Mathf.Log(mSamples[i - 1]), 3), new Vector3(Mathf.Log(i), Mathf.Log(mSamples[i]), 3), Color.blue);
#endif

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

            // N개의 Peak만 골라냄
            List<KeyValuePair<int, float>> nPeaks = new List<KeyValuePair<int, float>>();
            int maxPeakNum = 40;
            foreach (var peak in validPeaks)
            {
                if (nPeaks.Count == 0)
                {
                    nPeaks.Add(peak);
                    continue;
                }
                for (int i = 0; i < nPeaks.Count; i++)
                {
                    if (nPeaks[i].Value < peak.Value)
                    {
                        nPeaks.Insert(i, peak);
                        if (nPeaks.Count > maxPeakNum)
                        {
                            nPeaks.RemoveAt(maxPeakNum);
                        }
                        break;
                    }
                }
            }

            // Harmonics 필터링
            List<KeyValuePair<int, float>> targetPeaks = new List<KeyValuePair<int, float>>();
            for (int cur = 0; cur < nPeaks.Count; cur++)
            {
                for (int next = 0; next < nPeaks.Count; next++)
                {
                    if (cur == next)
                        continue;

                    if (Mathf.Abs((nPeaks[cur].Key * 2) - nPeaks[next].Key) <= 1)
                    {
#if DEBUG_LINE
                        Debug.DrawLine(new Vector3(Mathf.Log(nPeaks[cur].Key - 1), Mathf.Log(nPeaks[cur].Value), 3), new Vector3(Mathf.Log(nPeaks[cur].Key + 1), Mathf.Log(nPeaks[cur].Value), 3), Color.red);
#endif
                        targetPeaks.Add(nPeaks[cur]);
                        break;
                    }
                }
            }

            // 가장 낮은 Frequency가 앞에 오도록
            targetPeaks.Sort((x, y) => { return x.Key.CompareTo(y.Key); }); // X축 Sort

            // Add Note
            var curTime = Time.time;
            foreach (var peak in targetPeaks)
            {
                var freq = peak.Key / (float)mSamples.Length * mFrequencyMax;
                var note = noteDbReader.AssumeNoteFromFrequency(freq);

                mNoteList.Add(new TimeAndANote() { Note = note, Time = curTime });
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
        }

    }

    void OnDisable()
    {
        mAudio.Stop();
    }
}

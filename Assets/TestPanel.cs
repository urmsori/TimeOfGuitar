using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class TestPanel : MonoBehaviour
{
    public Text textTestId;
    public Button startButton;
    public Button stopButton;
    public Button endTestButton;
    public AudioSource startAudio;
    public AudioSource stopAudio;
    public AudioSource[] audios;

    private int curAudioIndex = 0;
    private AudioSource curAudioSource = null;
    private bool isPlaying = false;

    private List<float> testLogs = new List<float>();

    // Use this for initialization
    void Start()
    {
        gameObject.SetActive(false);

        startButton.onClick.AddListener(() => { StartOrStop(); });
        stopButton.onClick.AddListener(() => { StartOrStop(); });
        endTestButton.onClick.AddListener(() =>
        {
            TestEnd();
            gameObject.SetActive(false);
        });
    }

    public void TestStart()
    {
        testLogs.Clear();
        curAudioIndex = -1;
        curAudioSource = null;
        isPlaying = false;
        StartOrStopForce();
    }
    public void TestEnd()
    {
        if (isPlaying)
            MusicStop();
        curAudioIndex = -1;
        curAudioSource = null;

        System.DateTime now = System.DateTime.Now;
        string filePath = Path.Combine(Application.persistentDataPath, textTestId.text +"-"+ now.ToString("HHmmSS") + ".txt");
        string totalLog = "";
        foreach (var log in testLogs)
        {
            totalLog += log + "\n";
        }
        File.WriteAllText(filePath, totalLog);
        testLogs.Clear();
    }

    public void StartOrStop()
    {
        if (!isTimeStarted)
            return;

        StartOrStopForce();
    }
    public void StartOrStopForce()
    {
        if (gameObject.activeInHierarchy)
        {
            if (isPlaying)
            {
                MusicStop();
            }
            else
            {
                MusicStart();
            }
        }
    }

    void MusicStart()
    {
        startAudio.Stop();
        TimeStop();

        curAudioIndex++;
        curAudioIndex = curAudioIndex % audios.Length;

        if (audios.Length > curAudioIndex)
        {
            curAudioSource = audios[curAudioIndex];
        }
        else
        {
            curAudioSource = null;
        }

        if (curAudioSource != null)
        {
            curAudioSource.time = 10.0f;
            curAudioSource.Play();

            isPlaying = true;
            startButton.gameObject.SetActive(false);
            stopButton.gameObject.SetActive(true);
        }
        else
        {
            // Error
            isPlaying = false;
            startButton.gameObject.SetActive(true);
            stopButton.gameObject.SetActive(false);
        }

        StartCoroutine(SoundStop(Random.value * 5 + 7));
    }

    void MusicStop()
    {
        stopAudio.Stop();
        TimeStop();

        if (curAudioSource != null)
        {
            curAudioSource.Stop();
        }
        isPlaying = false;
        startButton.gameObject.SetActive(true);
        stopButton.gameObject.SetActive(false);

        StartCoroutine(SoundStart(Random.value * 5 + 7));
    }

    IEnumerator SoundStop(float delay)
    {
        yield return new WaitForSeconds(delay);
        stopAudio.Play();
        TimeStart();
    }

    IEnumerator SoundStart(float delay)
    {
        yield return new WaitForSeconds(delay);
        startAudio.Play();
        TimeStart();
    }

    bool isTimeStarted = false;
    float startedTime = 0;
    void TimeStart()
    {
        isTimeStarted = true;
        startedTime = Time.time;
    }
    void TimeStop()
    {
        if (!isTimeStarted)
            return;

        isTimeStarted = false;
        var diff = Time.time - startedTime;
        testLogs.Add(diff);
        print(diff);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowDebugLog : MonoBehaviour
{
    Queue myLogQueue = new Queue();

    // Use this for initialization
    void Start()
    {
        Application.logMessageReceived += Application_logMessageReceived;
    }

    private void Application_logMessageReceived(string condition, string stackTrace, LogType type)
    {
        string newString = "\n [" + type + "] : " + condition;
        myLogQueue.Enqueue(newString);
        if (type == LogType.Exception)
        {
            newString = "\n" + stackTrace;
            myLogQueue.Enqueue(newString);
        }
    }

    // Update is called once per frame
    void Update()
    {
        while(myLogQueue.Count > 20)
        {
            myLogQueue.Dequeue();
        }
        string total = "";
        foreach (var log in myLogQueue)
        {
            total += log;
        }
        GetComponent<Text>().text = total;
    }
}

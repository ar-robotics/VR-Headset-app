using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// This script is used to display debug logs on the a VR screen for testing purposes.
/// It is used to display the logs in a more readable format inside the VR environment.
/// </summary>
public class DebugDisplayPro : MonoBehaviour
{

    /// <summary>
    /// A dictionary to store the debug logs.
    /// </summary>
    Dictionary<string, string> debugLogs = new Dictionary<string, string>();

    /// <summary>
    /// The TextMeshPro component used to display the debug logs.
    /// </summary>
    public TMP_Text debugText;

    // // Start is called before the first frame update
    // void Start()
    // {

    // }

    // Update is called once per frame
    private void Update()
    {
        // Debug.Log("time:" + Time.time);
        // Debug.Log(gameObject.name);
    }

    /// <summary>
    /// This method is called when the script is enabled and is used to subscribe to the log message event.
    /// </summary>
    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    /// <summary>
    /// This method is called when the script is disabled and is used to unsubscribe from the log message event.
    /// </summary>
    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    /// <summary>
    /// This method is used to handle the log messages and store them in the debugLogs dictionary.
    /// It also updates the debugText component with the latest logs.
    /// </summary>
    /// <param name="logString"></param>
    /// <param name="stackTrace"></param>
    /// <param name="type"></param>
    void HandleLog(string logString, string stackTrace, LogType type)
    {

        if (debugLogs.Count > 30)
        {
            debugLogs.Clear();
        }
        if (type == LogType.Log)
        {
            string[] splitString = logString.Split(char.Parse(":"));
            string debugKey = splitString[0];
            string debugValue = splitString.Length > 1 ? splitString[1] : "";

            // if (debugKey.ToLower().Contains("voice") || debugKey.ToLower().Contains("log"))
            // {
            //     return;
            // }
            if (debugLogs.ContainsKey(debugKey))
            {
                debugLogs[debugKey] = debugValue;
            }
            else
            {
                debugLogs.Add(debugKey, debugValue);
            }
        }

        string debugTextString = "";
        foreach (KeyValuePair<string, string> log in debugLogs)
        {
            if (log.Value == "")
            {
                debugTextString += log.Key + "\n";
            }
            else
            {
                debugTextString += log.Key + ": " + log.Value + "\n";
            }
        }
        debugText.text = debugTextString;
    }
}

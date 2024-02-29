using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This script is used to display debug logs on the a VR screen for testing purposes.
/// This is a simplified version of the DebugDisplayPro script, it is the first version of the script.
/// For documentation on the DebugDisplayPro script, see the <see cref="DebugDisplayPro"/> class.
/// </summary>
public class DebugDisplay : MonoBehaviour
{

    Dictionary<string, string> debugLogs = new Dictionary<string, string>();
    public Text debugText;

    private void Update()
    {
        Debug.Log("time:" + Time.time);
        // Debug.Log(gameObject.name);
    }


    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        if (type == LogType.Log)
        {
            string[] splitString = logString.Split(char.Parse(":"));
            string debugKey = splitString[0];
            string debugValue = splitString.Length > 1 ? splitString[1] : "";

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

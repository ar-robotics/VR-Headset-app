using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Meta.WitAi;
using Meta.WitAi.Json;
using System;
using Oculus.Voice;

public class Responsehandler : MonoBehaviour
{
    public AppVoiceExperience voiceExperience;
    // Start is called before the first frame update
    void Start()
    {
        // voiceExperience.OnVoiceCommandReceived += HandleResponseTest;
        // voiceExperience.OnStartListening += OnStartListening;
        // voiceExperience.OnEmergencyVoiceCommandActivated += OnEmergencyVoiceCommandActivated;
        // voiceExperience.OnEmergency += OnEmergency;

    }

    private int index = 0;

    public event Action<int> OnVoiceCommandReceived;

    public void HandleResponseTest(WitResponseNode response)
    {
        // string value = response["entities"]["intent"][0]["value"];

        string value = response["entities"];
        Debug.Log($"ResponseKromium: {response}");
        Debug.Log($"Value Response: {value}");
        Debug.Log($"Got Response from voice command: {true}");
        string command = "Voice command" + index++;
        // OnVoiceCommandReceived?.Invoke(command);
    }

    public void OnStartListening()
    {
        Debug.Log($"Vocie controll Listening: {index++}");
    }

    public void OnEmergencyVoiceCommandActivated(string[] em)
    {
        Debug.Log($"Emerg index: {index++}");
        Debug.Log($"Vocie controll Listening Emergency STRING NAME: {em[0]}");

        int mode = 0;
        switch (em[0].ToLower())
        {
            case "idle":
                Debug.Log($"Vocie controll Listening Emergency: {em[0]}");
                mode = 0;
                break;
            case "drive":
                mode = 1;
                Debug.Log($"Vocie controll Listening Emergency: {em[0]}");
                break;
            case "arm":
                mode = 2;
                break;
            case "emergency":
                mode = 3;
                Debug.Log($"Vocie controll Listening Emergency: {em[0]}");
                break;
        }

        OnVoiceCommandReceived?.Invoke(mode);
    }

    public void OnEmergency(string emergency)
    {
        Debug.Log($"Vocie controll Listening Emergency: {emergency}  +{index++}");
    }
    // public void HandleError(WitError error)
    // {
    //     Debug.LogError($"Error: {error}");
    // }
    // Update is called once per frame
    void Update()
    {
        if (!voiceExperience.Active)
        {
            voiceExperience.Activate();
        }
    }
}

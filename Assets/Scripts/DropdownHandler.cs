using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// This script is used to handle the dropdown in the UI. When the dropdown value is changed, it sends the new value to the server.
/// The Dropdown is used to controll the mode of the robot. 
/// </summary>
/// <remarks>
/// - Supported modes at the moment:
///   * Idle
///   * Drive
///   * Arm
///   * Emergency Stop
/// </remarks>
public class DropdownHandler : MonoBehaviour
{

    /// <summary>
    /// The ModeValues interface used to send the mode value to the robot.
    /// </summary>
    /// <remarks> This interface has to match with the ROS2 interface from the robot </remarks>
    class ModeValues
    {
        public int mode;
    }

    public TMP_Dropdown dropdown;
    private NetworkManager networkManager;
    private ModeValues modeValues = new ModeValues();

    public Responsehandler responsehandler;
    public event Action<int> OnDropdownValueChanged;

    /// <summary>
    /// Initialization of the script by finding the NetworkManager component in the scene and subscribing to the onValueChanged event of the dropdown.
    /// </summary>
    void Start()
    {

        // Ensure the Dropdown is assigned
        if (dropdown != null)
        {
            // Subscribe to the onValueChanged event
            dropdown.onValueChanged.AddListener(delegate { DropdownValueChanged(dropdown); });
        }
        networkManager = NetworkManager.Instance;
        responsehandler.OnVoiceCommandReceived += HandleVoiceCommand;
    }


    void HandleVoiceCommand(int mode)
    {
        Debug.Log($"Voice Command Received: {mode}");
        dropdown.value = mode;
    }

    /// <summary>
    /// This method is called when the dropdown value is changed. It sends the new value to the server.
    /// </summary>
    void DropdownValueChanged(TMP_Dropdown change)
    {
        Debug.Log($"New Dropdown Value Selected: {change.value}");
        // Perform your action here
        // For example, if (change.value == 0) { // Do something }
        modeValues.mode = change.value;
        SendDataToServer(JsonUtility.ToJson(modeValues));
        OnDropdownValueChanged?.Invoke(change.value);
    }

    /// <summary>
    /// This method is used to send the mode value to the server.
    /// </summary>
    /// <param name="data"></param>
    void SendDataToServer(string data)
    {
        if (networkManager != null)
        {
            networkManager.SendData(data);
        }
        else
        {
            Debug.Log("NetworkManager component not found.");
        }
    }

    /// <summary>
    /// Get method for the dropdown value.
    /// </summary>
    /// <returns>Dropdown.value</returns>
    public int GetDropdownValue()
    {
        return dropdown.value;
    }
}

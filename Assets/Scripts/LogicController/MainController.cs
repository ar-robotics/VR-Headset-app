using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;
using TMPro;

/// <summary>
/// This class is the main controller for handling scene changes and mode changes.
/// It enables and disables scenes based on the selected mode dynamically.
/// </summary>
public class MainController : MonoBehaviour
{
    public DropdownHandler dropdownHandler;
    public GameObject driveScene; // Assign in the Unity Editor
    public GameObject armScene; // Assign in the Unity Editor

    public TMP_Dropdown dropdown; // Assign in the Unity Editor

    public ModeAudioPlay modeAudioPlay; // Assign in the Unity Editor
    public XRPokeFollowAffordance EmergencyStopButton;

    /// <summary>
    /// Start is called before the first frame update, it is used to initialize the controller, subscribe to relevant events, and deactivate scenes.
    /// </summary>
    void Start()
    {
        if (dropdownHandler != null)
        {
            dropdownHandler.OnDropdownValueChanged += HandleDropdownChange;
            EmergencyStopButton.OnEmergency += HandleEmergencyStop;
        }

        // Deactivate both scenes at the start
        if (driveScene != null)
        {
            driveScene.SetActive(false);
        }

        if (armScene != null)
        {
            armScene.SetActive(false);
        }

    }

    /// <summary>
    /// This method is used to handle the emergency stop event.
    /// </summary>
    /// <param name="emergency"></param>
    private void HandleEmergencyStop(bool emergency)
    {
        Debug.Log($"Emergency Stop: {emergency}");
        // Modify the switch statement to activate/deactivate scenes
        if (emergency)
        {
            driveScene.SetActive(false);
            armScene.SetActive(false);
            dropdown.value = 3;
        }
    }

    void OnDestroy()
    {
        if (dropdownHandler != null)
        {
            dropdownHandler.OnDropdownValueChanged -= HandleDropdownChange;
        }
    }

    /// <summary>
    /// This method is used to enabling/disabling scenes based on the selected mode.
    /// </summary>
    /// <param name="newValue"></param>
    private void HandleDropdownChange(int newValue)
    {
        Debug.Log($"Dropdown value changed to: {newValue}");
        // Modify the switch statement to activate/deactivate scenes
        switch (newValue) // Changed to use newValue directly
        {
            case 0:
                // Idle mode
                driveScene.SetActive(false);
                armScene.SetActive(false);
                modeAudioPlay.PlayIdle();
                break;
            case 1:
                // Drive mode
                driveScene.SetActive(true);
                armScene.SetActive(false);
                modeAudioPlay.PlayDrive();
                break;
            case 2:
                // Arm mode
                driveScene.SetActive(false);
                armScene.SetActive(true);
                modeAudioPlay.PlayArm();
                break;
            case 3:
                // Emergency Stop mode
                driveScene.SetActive(false);
                armScene.SetActive(false);
                modeAudioPlay.PlayEmergency();
                break;
            default:
                // Default case
                break;
        }
    }
}

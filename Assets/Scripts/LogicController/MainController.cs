using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;
using TMPro;
public class MainController : MonoBehaviour
{
    public DropdownHandler dropdownHandler;
    public GameObject driveScene; // Assign in the Unity Editor
    public GameObject armScene; // Assign in the Unity Editor

    public TMP_Dropdown dropdown; // Assign in the Unity Editor

    public ModeAudioPlay modeAudioPlay; // Assign in the Unity Editor
    public XRPokeFollowAffordance EmergencyStopButton;
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

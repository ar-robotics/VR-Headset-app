using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;

/// <summary>
/// This class is used to control the drive mode of the robot.
/// It allows the user to change the drive mode of the robot.
/// </summary>
public class DriveModeController : MonoBehaviour
{
    private NetworkManager networkManager;

    public event Action<string> DriveModeChanged;

    public Button precisionButton;
    public Button normalButton;
    public Button reverseButton;

    public Color activeColor;
    public Color defaultColor;
    class DriveMode
    {
        public string drive_mode;
    }

    void Start()
    {
        networkManager = NetworkManager.Instance;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void precisionMode()
    {
        updateDriveModeData("precision");
        updateButtonColor(activeColor, defaultColor, defaultColor);
    }

    public void normalMode()
    {
        updateDriveModeData("normal");
        updateButtonColor(defaultColor, activeColor, defaultColor);
    }

    public void reverseMode()
    {
        updateDriveModeData("reverse");
        updateButtonColor(defaultColor, defaultColor, activeColor);
    }

    /// <summary>
    /// This method is used to update the drive mode data.
    /// </summary>
    void updateDriveModeData(string mode)
    {
        DriveMode driveMode = new DriveMode();
        driveMode.drive_mode = mode;
        SendDataToServer(JsonUtility.ToJson(driveMode));
        DriveModeChanged?.Invoke(mode);
    }

    /// <summary>
    /// This method is used to update the color of the buttons.
    /// </summary>
    /// <param name="precisionColor"></param>
    /// <param name="normalColor"></param>
    /// <param name="reverseColor"></param>
    /// <summary>
    void updateButtonColor(Color precisionColor, Color normalColor, Color reverseColor)
    {
        // precisionButton.GetComponent<Image>().color = precisionColor;
        // normalButton.GetComponent<Image>().color = normalColor;
        // reverseButton.GetComponent<Image>().color = reverseColor;

        precisionButton.image.color = precisionColor;
        normalButton.image.color = normalColor;
        reverseButton.image.color = reverseColor;

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
}

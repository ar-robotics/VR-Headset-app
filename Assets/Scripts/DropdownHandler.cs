using UnityEditor;
using UnityEngine;
using UnityEngine.UI; // Import this namespace to work with UI components
using TMPro; // 
public class DropdownHandler : MonoBehaviour
{

    // ModeValues 
    class ModeValues
    {
        public int mode;
    }

    public TMP_Dropdown dropdown;

    private NetworkManager networkManager;
    private ModeValues modeValues = new ModeValues();


    void Start()
    {
        // Ensure the Dropdown is assigned
        if (dropdown != null)
        {
            // Subscribe to the onValueChanged event
            dropdown.onValueChanged.AddListener(delegate { DropdownValueChanged(dropdown); });
        }
        networkManager = NetworkManager.Instance;
    }

    // This method will be called whenever the Dropdown's value changes
    void DropdownValueChanged(TMP_Dropdown change)
    {
        Debug.Log($"New Dropdown Value Selected: {change.value}");
        // Perform your action here
        // For example, if (change.value == 0) { // Do something }
        modeValues.mode = change.value;
        SendDataToServer(JsonUtility.ToJson(modeValues));
    }


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

    public int GetDropdownValue()
    {
        return dropdown.value;
    }
}

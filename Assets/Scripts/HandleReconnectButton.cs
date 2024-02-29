using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script is used to handle the reconnect button in the UI. When the button is clicked, it calls the Reconnect method of the NetworkManager 
/// to disconnect and reconnect to the server. It is used for testing purposes.
/// <remarks> This script is used for testing purposes. </remarks>
/// </summary>
public class HandleReconnectButton : MonoBehaviour
{
    // Start is called before the first frame update

    /// <summary>
    /// The GameObject used to display the reconnect button in the UI. A rectangular button is used for this purpose.
    /// </summary>
    public GameObject reconnectButton;

    /// <summary>
    /// The NetworkManager component used to handle the network connection.
    /// </summary>
    private NetworkManager networkManager;

    /// <summary>
    /// Initializes the script by finding the NetworkManager component in the scene.
    /// </summary>
    void Start()
    {
        networkManager = NetworkManager.Instance;
    }

    /// <summary>
    /// This method is called when the reconnect button is clicked. It calls the Reconnect method of the NetworkManager to disconnect and reconnect to the server.
    /// </summary>
    public void OnReconnectButtonClicked()
    {
        if (networkManager != null)
        {
            networkManager.Reconnect();
        }
        else
        {
            Debug.Log("NetworkManager component not found.");
        }
    }


    // Update is called once per frame
    void Update()
    {
    }
}

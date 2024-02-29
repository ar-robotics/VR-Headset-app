using UnityEngine;

/// <summary>
/// This class is used to send data to the server by pressing the space key.
/// It is used only for testing the communication with the server.
/// <remarks> This class is used for testing purposes. </remarks>
/// </summary>
public class ObjectController : MonoBehaviour
{

    /// <summary>
    /// The NetworkManager component used to send data to the server.
    /// </summary>
    private NetworkManager networkManager;

    /// <summary>
    /// Initialization of the script by finding the NetworkManager component in the scene.
    /// </summary>
    void Start()
    {
        // Get the singelton instance of the NetworkManager
        networkManager = NetworkManager.Instance;
    }

    /// <summary>
    /// Update is called once per frame and is used to send data to the server when the space key is pressed.
    /// </summary>
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SendDataToServer("Space key pressed!");
        }
        // SendDataToServer("hello world!");
    }

    /// <summary>
    /// This method is used to send data to the server.
    /// </summary>
    void SendDataToServer(string data)
    {
        if (networkManager != null)
        {
            networkManager.SendData(data);
        }
        else
        {
            Debug.LogError("NetworkManager component not found.");
        }


    }
}

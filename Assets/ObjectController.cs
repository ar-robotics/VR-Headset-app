using UnityEngine;
public class ObjectController : MonoBehaviour
{
    private NetworkManager networkManager;
    void Start()
    {
        // Get the singelton instance of the NetworkManager
        networkManager = NetworkManager.Instance;
    }

    void Update()
    {
        // Example: Send data when the space key is pressed
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SendDataToServer("Space key pressed!");
        }
        // SendDataToServer("hello world!");
    }

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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandleReconnectButton : MonoBehaviour
{
    // Start is called before the first frame update

    public GameObject reconnectButton;
    private NetworkManager networkManager;

    void Start()
    {
        networkManager = NetworkManager.Instance;
    }

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

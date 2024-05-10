using UnityEngine;
using System.Collections;
using Unity.WebRTC;
using WebSocketSharp;
using UnityEngine.UI;

public class WebRTCReceiver : MonoBehaviour
{
    private WebSocket _webSocket;
    NetworkManager networkManager;
    public RawImage rawImage; // Reference to the RawImage component
    private Texture2D texture;
    void Start()
    {
        networkManager = NetworkManager.Instance;
        texture = new Texture2D(27, 15, TextureFormat.RGB24, false); // Adjust format as necessary
        networkManager.OnConnected += () =>
        {
            _webSocket = new WebSocket("ws://192.168.1.6:5000");
            _webSocket.OnMessage += OnMessage;
            _webSocket.OnError += OnError;
            _webSocket.Connect();
        };

    }

    void Update()
    {

    }

    private int index = 0;
    // void OnMessage(object sender, MessageEventArgs e)
    // {
    //     Debug.Log($"Camera: Received data {index++}");
    //     byte[] bytes = System.Convert.FromBase64String(e.Data);

    //     UnityMainThreadDispatcher.Instance.Enqueue(() =>
    //         {
    //             Texture2D texture = new Texture2D(27, 15);
    //             if (texture.LoadImage(bytes))
    //             {
    //                 texture.Apply();
    //                 rawImage.texture = texture;
    //                 Debug.Log($"Camera: Texture applied successfully");
    //             }
    //             else
    //             {
    //                 Debug.LogError($"Camera: Failed to load texture from received data");
    //             }
    //         });
    // }

    void OnMessage(object sender, MessageEventArgs e)
    {
        Debug.Log($"Camera: Received data {index++}");
        byte[] bytes = System.Convert.FromBase64String(e.Data);

        UnityMainThreadDispatcher.Instance.Enqueue(() =>
        {
            if (texture.LoadImage(bytes))
            {
                texture.Apply();
                rawImage.texture = texture;
                Debug.Log("Camera: Texture applied successfully");
            }
            else
            {
                Debug.LogError("Camera: Failed to load texture from received data");
            }
        });
    }
    void OnError(object sender, ErrorEventArgs e)
    {
        Debug.LogError("Camera: WebSocket Error" + e.Message);
    }

    void OnDestroy()
    {
        if (_webSocket != null)
        {
            _webSocket.OnMessage -= OnMessage;
            _webSocket.OnError -= OnError;
            _webSocket.Close();
            _webSocket = null;
        }
    }
}

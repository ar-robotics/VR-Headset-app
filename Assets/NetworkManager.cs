using UnityEngine;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance; // Singleton instance
    private ConcurrentQueue<string> receivedDataQueue = new ConcurrentQueue<string>();
    private TcpClient client;
    private NetworkStream stream;
    private string serverIP = "192.168.1.6";
    // private string serverIP = "192.168.0.187";

    private int port = 8080;

    private Thread receiveThread;
    private bool isListening = false;
    public event Action<string> OnDataReceived;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep the instance alive across scenes
        }
        else if (Instance != this)
        {
            Destroy(gameObject); // Ensures only one instance exists
        }
    }

    void Start()
    {
        ConnectToServer();
    }

    private void ConnectToServer()
    {
        try
        {
            client = new TcpClient(serverIP, port);
            if (client.Connected)
            {
                stream = client.GetStream();
                isListening = true;

                // Start the receiving thread
                receiveThread = new Thread(new ThreadStart(ReceiveData));
                receiveThread.IsBackground = true;
                receiveThread.Start();

                Debug.Log("Connected to server.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error connecting to server: " + e.Message);
        }
    }


    public bool SendData(string data)
    {
        if (stream == null)
        {
            Debug.LogError("Network stream is not available.");
            return false;
        }

        try
        {
            byte[] bytes = Encoding.ASCII.GetBytes(data);
            stream.Write(bytes, 0, bytes.Length);
            Debug.Log($"Sent: {data}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error sending data: {e.Message}");
            return false;
        }
    }

    private void ReceiveData()
    {
        byte[] buffer = new byte[1024];
        int byteLength;

        while (isListening && client != null && client.Connected)
        {
            try
            {
                if (stream.DataAvailable)
                {
                    byteLength = stream.Read(buffer, 0, buffer.Length);
                    string dataReceived = Encoding.ASCII.GetString(buffer, 0, byteLength);

                    // Enqueue the received data
                    receivedDataQueue.Enqueue(dataReceived);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error receiving data: {e.Message}");
                isListening = false;
            }
        }
    }

    void OnApplicationQuit()
    {
        isListening = false;
        if (receiveThread != null && receiveThread.IsAlive)
        {
            receiveThread.Join();
        }

        if (client != null)
        {
            client.Close();
        }
    }

    void Update()
    {
        // Process all pending messages
        while (receivedDataQueue.TryDequeue(out string receivedData))
        {
            // Here you can directly process the data or invoke an event
            // Debug.Log($"Processing received data on the main thread: {receivedData}");
            // For example, invoke a custom event with the received data
            OnDataReceived?.Invoke(receivedData);
        }
    }


    void OnDestroy()
    {
        if (client != null)
            client.Close();
    }
    public void Reconnect()
    {
        // First, disconnect existing connection if any
        Disconnect();

        // Then, attempt to reconnect
        ConnectToServer();
    }

    private void Disconnect()
    {
        isListening = false;

        // Wait for the receive thread to finish, if it's running
        if (receiveThread != null && receiveThread.IsAlive)
        {
            receiveThread.Join();
        }

        // Close the client connection and the stream
        if (client != null)
        {
            if (stream != null)
            {
                stream.Close();
                stream = null;
            }
            client.Close();
            client = null;
        }

        Debug.Log("Disconnected from server.");
    }

}

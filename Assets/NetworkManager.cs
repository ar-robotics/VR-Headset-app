using UnityEngine;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;
using OVRSimpleJSON;
using UnityEditor;
using System.Collections.Generic;


/// <summary>
/// Class <c> JsonRobotInfo </c>
/// Represents the JSON data structure for the robot information.
/// </summary>
[Serializable]
public class JsonRobotInfo
{

    public List<float> accelerometer;
    public List<float> gyroscope;
    public List<float> magnetometer;
    public List<float> motion;
    public float speed = 88;
    public float voltage;
    public int battery_precentage;
    public string mode;
}

/// <summary>
/// Class <c> NetworkManager </c>
/// Manages network communications for the application, implementing a singleton pattern to ensure only one instance exists.
/// This class handles the creation of a TCP client, manages connections, and processes incoming data asynchronously.
/// </summary>
public class NetworkManager : MonoBehaviour
{
    /// <summary>
    /// Singleton instance of the NetworkManager class.
    /// <code> NetworkManager = NetworkManager.Instance; </code>
    /// </summary>
    public static NetworkManager Instance;
    /// <summary>
    /// Queue to store received data.
    /// </summary>

    /// <summary>  
    /// The received data queue.
    /// </summary>
    private ConcurrentQueue<string> receivedDataQueue = new ConcurrentQueue<string>();

    /// <summary>
    /// The TCP client for the network connection.
    /// </summary>
    private TcpClient client;

    /// <summary>
    /// The network stream for reading and writing data to the server.
    /// </summary>
    private NetworkStream stream;


    /// <summary>
    /// The server IP address.
    /// </summary>
    public string serverIP = "192.168.1.6";
    // private string serverIP = "192.168.0.187";

    /// <summary>   
    /// The port number for the server.
    /// </summary>
    private int port = 8080;

    /// <summary>
    /// The receive thread for processing incoming data.
    /// </summary>
    private Thread receiveThread;

    /// <summary>
    /// Flag to indicate if the client is listening for incoming data.
    /// </summary>
    /// <value> true </value> if the client is listening; otherwise, <value> false </value>
    private bool isListening = false;

    /// <summary>
    ///  Event to handle received data.
    ///  <code> NetworkManager.OnDataReceived += OnDataReceivedHandler; </code>
    /// </summary>
    public event Action<string> OnDataReceived;

    public event Action<JsonRobotInfo> OnRobotInfoDataReceived;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
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

    /// <summary>
    /// Start is called before the first frame update.
    /// Connects to the server when the application starts.
    /// </summary>
    void Start()
    {
        ConnectToServer();
    }

    /// <summary>
    /// Connects to the server.
    /// Starts also the receiving thread to process incoming data.
    /// </summary>
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


    /// <summary>
    /// Sends data to the server.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Receives data from the server. This method runs on a separate thread.
    /// </summary>
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

    /// <summary>
    /// Called when the application quits. Closes the client connection and the receive thread.
    /// </summary>
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

    /// <summary>
    /// Update is called once per frame, and processes all pending messages.
    /// </summary>
    void Update()
    {
        // Process all pending messages
        while (receivedDataQueue.TryDequeue(out string receivedData))
        {

            try
            {
                var jsonData = OVRSimpleJSON.JSON.Parse(receivedData);

                if (jsonData == null)
                {
                    Debug.LogError("Error parsing JSON: " + receivedData);
                    return;
                }

                if (jsonData.HasKey("speed"))
                {
                    JsonRobotInfo info = ParseJsonRobotInfo(jsonData);
                    OnRobotInfoDataReceived?.Invoke(info);
                    Debug.Log($"Received robot info: {receivedData}");
                    Debug.Log($"Speed: {info}");
                    Debug.Log($"Speed: {jsonData}");
                }

            }
            catch (Exception e)
            {
                Debug.LogError($"Error parsing JSON: {e.Message}");
            }

            // Here you can directly process the data or invoke an event
            // Debug.Log($"Processing received data on the main thread: {receivedData}");
            // For example, invoke a custom event with the received data
            OnDataReceived?.Invoke(receivedData);
        }
    }

    JsonRobotInfo ParseJsonRobotInfo(OVRSimpleJSON.JSONNode jsonNode)
    {
        return new JsonRobotInfo
        {
            accelerometer = ParseFloatList(jsonNode["accelerometer"]),
            gyroscope = ParseFloatList(jsonNode["gyroscope"]),
            magnetometer = ParseFloatList(jsonNode["magnetometer"]),
            motion = ParseFloatList(jsonNode["motion"]),
            speed = jsonNode["speed"].AsFloat,
            voltage = jsonNode["voltage"].AsFloat,
            battery_precentage = jsonNode["battery"],
            mode = jsonNode["mode"]
        };
    }

    // helper method to parse a list of floats from a JSON node
    List<float> ParseFloatList(OVRSimpleJSON.JSONNode node)
    {
        List<float> list = new List<float>();
        if (node.IsArray)
        {
            foreach (OVRSimpleJSON.JSONNode n in node.AsArray)
            {
                list.Add(n.AsFloat);
            }
        }
        else
        {
            Debug.LogError("Node is not an array");
        }
        return list;
    }

    /// <summary>
    /// Called when the object is destroyed. Closes the client connection as well.
    /// </summary>
    void OnDestroy()
    {
        if (client != null)
            client.Close();
    }

    /// <summary>
    /// Reconnects to the server, closing the existing connection if any.
    /// </summary>
    public void Reconnect()
    {
        // First, disconnect existing connection if any
        Disconnect();

        // Then, attempt to reconnect
        ConnectToServer();
    }

    /// <summary>
    /// Disconnects from the server.
    /// </summary>
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

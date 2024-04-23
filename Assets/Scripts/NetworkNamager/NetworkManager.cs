using UnityEngine;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;
using OVRSimpleJSON;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.Rendering;
using Oculus.Platform;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections;
using System.Data.Common;
using UnityEngine.XR.ARFoundation.VisualScripting;
using System.IO;
using System.IO.Compression;
using Unity.VisualScripting;
using System.Linq;
using UnityEngine.XR.Interaction.Toolkit;


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
/// Vector data to store tdepth data (x, y, z)
/// </summary>
[Serializable]
public struct Vector3Data
{
    public float x;
    public float y;
    public float z;

    public Vector3Data(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }
}

/// <summary>
/// depth camera interface datatype 
/// The data is recieved from the robot as this type
/// </summary>
[Serializable]
public class CameraDepthData
{

    public int time;
    public List<Vector3Data> depthData = new List<Vector3Data>();
}

/// <summary>
/// Depth data interface Datatype 
/// </summary>
public class DepthDataPoint
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }

    public DepthDataPoint(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }
}

[Serializable]
public class DepthData
{
    public int time;
    private List<DepthDataPoint> depthDataPoints = new List<DepthDataPoint>();

    public void AddDepthDataPoint(float x, float y, float z)
    {
        depthDataPoints.Add(new DepthDataPoint(x, y, z));
    }

    public IEnumerable<DepthDataPoint> Points => depthDataPoints;

    public int Count => depthDataPoints.Count;
    public void ClearPoints()
    {
        depthDataPoints.Clear();
    }
}

/// <summary>
/// Class <c> NetworkManager </c>
/// Manages network communications for the application, implementing a singleton pattern to ensure only one instance exists.
/// This class handles the creation of a TCP client, manages connections, and processes incoming data asynchronously.
/// </summary>
public class NetworkManager : MonoBehaviour
{

    public TextAsset brotliTestFile;


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
    /// UDP port
    /// </summary>
    private int udpPort = 12000;

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

    public event Action<DepthData> onDepthDataReceived;

    /// <summary>
    /// The camera depth data.
    /// </summary>
    CameraDepthData cameraDepthData = new CameraDepthData();

    /// <summary>
    /// Camera Depth data 
    /// </summary>
    DepthData depthData = new DepthData();


    /// <summary>
    /// Size before updating camera depth data
    /// </summary>
    private int sizeBeforeUpdate = 10000;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        if (depthData == null)
        {
            depthData = new DepthData();
        }
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
        // ConnectToServer();
        Debug.Log($"Network manager is a live: {true}");
        // if (depthData == null)
        // {
        //     depthData = new DepthData();
        // }
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
                // receiveThread = new Thread(new ThreadStart(ReceiveDataAsync));
                receiveThread.IsBackground = true;
                receiveThread.Start();

                // await ReceiveDataAsync();
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
    /// 
    byte[] broHeader = new byte[] { (byte)'b', (byte)'r', (byte)'o' };
    private void ReceiveData()
    {
        byte[] buffer = new byte[4096];
        int byteLength;
        string dataReceived = string.Empty;

        while (isListening && client != null && client.Connected)
        {
            try
            {
                if (stream.DataAvailable)
                {
                    byteLength = stream.Read(buffer, 0, buffer.Length);
                    if (buffer.Take(3).SequenceEqual(broHeader))
                    {
                        byte[] compressedData = DecompressBrotli(buffer.Skip(3).ToArray());  // Skip the first 3 bytes which are the header
                        if (compressedData != null)
                        {
                            dataReceived = Encoding.UTF8.GetString(compressedData);  // Decode the entire decompressed data
                        }
                    }
                    else
                    {
                        dataReceived = Encoding.ASCII.GetString(buffer, 0, byteLength);
                    }
                    receivedDataQueue.Enqueue(dataReceived);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error receiving data: A{e.Message}");
                isListening = false;
            }
        }
    }

    private async Task ReceiveDataAsync()
    {
        byte[] buffer = new byte[8192]; // Increased buffer size

        try
        {
            while (isListening && client != null && client.Connected)
            {
                if (stream.DataAvailable)
                {

                    int byteLength = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (byteLength > 0)
                    {
                        string dataReceived = Encoding.ASCII.GetString(buffer, 0, byteLength);
                        lock (receivedDataQueue)
                        {
                            receivedDataQueue.Enqueue(dataReceived);
                        }
                        // Debug.Log($"Received data: {dataReceived}");
                    }
                    else
                    {
                    }
                }
                await Task.Delay(10); // Yield to maintain responsiveness, adjust timing as necessary
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error receiving data asynchronously: {e.Message}");
            isListening = false;
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

    // / <summary>
    // / Update is called once per frame, and processes all pending messages.
    // / </summary>
    void Update()
    {
        // Process all pending messages
        while (receivedDataQueue.TryDequeue(out string receivedData))
        {
            try
            {
                var jsonData = OVRSimpleJSON.JSON.Parse(receivedData);
                if (jsonData != null)
                {
                    processRecievedData(jsonData);
                }
                else
                {
                    Debug.Log($"Error parsing JSON: {receivedData}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error parsing JSON: {e.Message}");
            }
        }
    }


    void processRecievedData(OVRSimpleJSON.JSONNode jsonData)
    {
        if (!jsonData.HasKey("type"))
        {
            Debug.Log($"No type key found in the JSON data: a{jsonData}");
            return;
        }
        // switch (jsonData["type"].ToString())
        // {
        //     case "log":
        //         // TODO process log data
        //         break;
        //     case "robot_data":
        //         JsonRobotInfo info = ParseJsonRobotInfo(jsonData);
        //         OnRobotInfoDataReceived?.Invoke(info);
        //         break;
        //     case "depth":
        //         ProcessDepthData(jsonData);
        //         onDepthDataReceived?.Invoke(depthData);
        //         break;
        //     default:
        //         Debug.Log($"Unknown data type: {jsonData["type"].ToString()}");
        //         break;
        // }

        if (jsonData["type"] == "log")
        {

        }
        else if (jsonData["type"] == "robot_data")
        {
            JsonRobotInfo info = ParseJsonRobotInfo(jsonData);
            OnRobotInfoDataReceived?.Invoke(info);
        }
        else if (jsonData["type"] == "depth")
        {
            ProcessDepthData(jsonData);
            Debug.Log($"Pricess res Depth data: {jsonData}");
            onDepthDataReceived?.Invoke(depthData);
        }
        else if (jsonData["type"] == "arm_angles")
        {
            Debug.Log($"Arm angels data: {jsonData}");
        }
    }

    /// <summary>
    /// Checks if the data is compressed using Brotli.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    private bool IsBrotli(string data)
    {
        return data.Contains("brotli");
    }

    /// <summary>
    /// Decodes a base64 string to a byte array.
    /// </summary>
    /// <param name="compressedData"></param>
    /// <returns></returns>
    public byte[] DecompressBrotli(byte[] compressedData)
    {
        using (var inputStream = new MemoryStream(compressedData))
        using (var outputStream = new MemoryStream())
        using (var brotliStream = new BrotliStream(inputStream, CompressionMode.Decompress))
        {
            brotliStream.CopyToAsync(outputStream);
            return outputStream.ToArray();
        }

    }

    /// <summary>
    /// Decodes a base64 string to a byte array.
    /// </summary>
    /// <param name="base64EncodedData"></param>
    /// <returns></returns>
    public byte[] DecodeBase64(string base64EncodedData)
    {
        try
        {
            byte[] data = Convert.FromBase64String(base64EncodedData);
            return data;
        }
        catch (FormatException ex)
        {
            Debug.Log($"Base64 string is not in a valid format: {ex.Message}");
            return null;
        }
    }
    /// <summary>
    /// For testing
    /// </summary>

    // void Update()
    // {
    //     List<string> batchedData = new List<string>();
    //     int batchSize = 1000; // You can adjust the batch size based on performance tests

    //     while (receivedDataQueue.TryDequeue(out string receivedData))
    //     {
    //         batchedData.Add(receivedData);
    //         if (batchedData.Count >= batchSize)
    //         {
    //             StartCoroutine(ProcessDataCoroutine(batchedData));
    //             batchedData = new List<string>(); // Reset for next batch
    //         }
    //     }

    //     if (batchedData.Count > 0)
    //     {
    //         StartCoroutine(ProcessDataCoroutine(batchedData)); // Process the remaining items
    //     }
    // }


    IEnumerator ProcessDataCoroutine(List<string> batchData)
    {
        foreach (string recievedData in batchData)
        {
            try
            {
                var jsonData = OVRSimpleJSON.JSON.Parse(recievedData);
                if (jsonData != null)
                {
                    if (jsonData.HasKey("type"))
                    {

                        if (jsonData["type"] == "log")
                        {

                        }
                        else if (jsonData["type"] == "depth")
                        {
                            // CameraDepthData depthData = ParseCameraDepthData(jsonData);
                            // ParseCameraDepthData(jsonData);

                            // if (cameraDepthData.depthData.Count > sizeBeforeUpdate)
                            // {
                            //     // onDepthDataReceived?.Invoke(cameraDepthData);
                            //     cameraDepthData.depthData.Clear();
                            // }
                        }
                        else if (jsonData["type"] == "robot_data")
                        {
                            JsonRobotInfo info = ParseJsonRobotInfo(jsonData);
                            OnRobotInfoDataReceived?.Invoke(info);
                        }
                    }
                }
                else
                {
                    Debug.LogError("Error parsing JSON: " + recievedData);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error parsing JSON: {e.Message}");
            }
        }
        yield return null;

    }

    /// <summary>
    /// Parse the depth data from the JSON node
    /// </summary>
    /// <param name="jsonNode"></param>
    /// <returns></returns>
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
    /// Parse the depth data from the JSON node
    /// </summary>
    /// <param name="jsonNode"></param>
    private void ProcessDepthData(OVRSimpleJSON.JSONNode jsonData)
    {
        depthData.ClearPoints();
        depthData.time = jsonData["time"].AsInt;
        Debug.Log($"ProcessDepthData res time: {depthData.time}");

        for (var i = 0; i < jsonData["data"][0].Count; i++)
        {
            depthData.AddDepthDataPoint(jsonData["data"][0][i].AsInt, jsonData["data"][1][i].AsInt, jsonData["data"][2][i].AsInt);
        }
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

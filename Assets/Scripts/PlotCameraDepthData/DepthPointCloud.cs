using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.IO.Compression;
using System.Text;
using System;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class DepthPointCloud : MonoBehaviour
{

    NetworkManager networkManager;
    public ParticleSystem particleSystem;
    public Gradient depthColorGradient; // Assign this gradient in the Unity editor
    private List<ParticleSystem.Particle> particles = new List<ParticleSystem.Particle>();

    // Optional: Define min and max depth for color normalization

    DepthData depthData = new DepthData();
    public TextAsset brotlifile;
    public MeshCreator meshCreator;
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

    public byte[] DecodeBase64(string base64EncodedData)
    {
        try
        {
            byte[] data = Convert.FromBase64String(base64EncodedData);
            return data;
        }
        catch (FormatException ex)
        {
            Debug.LogError("Base64 string is not in a valid format: " + ex.Message);
            return null;
        }
    }

    private void ProcessDepthData(OVRSimpleJSON.JSONNode jsonData)
    {
        depthData.ClearPoints();
        depthData.time = jsonData["time"].AsInt;

        for (var i = 0; i < jsonData["data"][0].Count; i++)
        {
            depthData.AddDepthDataPoint(jsonData["data"][0][i].AsInt, jsonData["data"][1][i].AsInt, jsonData["data"][2][i].AsInt);
        }
    }
    void Start()
    {
        networkManager = NetworkManager.Instance;
        networkManager.onDepthDataReceived += OnDepthDataReceived;
    }

    void Update()
    {
        // if (Time.frameCount % 100 == 0)
        // {
        //     string receivedData = brotlifile.text;
        //     byte[] compressedData = DecodeBase64(receivedData);
        //     byte[] decompressedData = DecompressBrotli(compressedData);
        //     if (compressedData != null && decompressedData != null)
        //     {
        //         receivedData = Encoding.UTF8.GetString(decompressedData);
        //     }
        //     var jsonData = OVRSimpleJSON.JSON.Parse(receivedData);
        //     ProcessDepthData(jsonData);
        //     OnDepthDataReceived(depthData);
        // }
    }

    void OnDepthDataReceived(DepthData depthData)
    {
        meshCreator.CreateCubeGridTest(depthData);
        // Processing depth data
        foreach (DepthDataPoint point in depthData.Points)
        {
            Debug.Log($"X, Y, Z: {point.X}, {point.Y}, {point.Z}");
            Debug.Log($"Time for depthData: {depthData.time}");
        }
    }


    /// <summary>
    /// Interface for requesting depth camera from the robot
    /// </summary>
    class RequestDepthDataMsg
    {
        public bool get_depth;
    }

    /// <summary>
    /// This method is used to request depth data from the server.
    /// When it is sent, the robot will respond with the depth data only once.
    /// </summary>
    public void RequestDepthData()
    {
        RequestDepthDataMsg msg = new RequestDepthDataMsg();
        msg.get_depth = true;
        networkManager.SendData(JsonUtility.ToJson(msg));
    }

    /// <summary>
    /// This method is used 
    /// </summary>
    void OnDestroy()
    {
        networkManager.onDepthDataReceived -= OnDepthDataReceived;
    }
}


// Test code
// byte[] decompressed;
// try
// {
//     decompressed = DecompressBrotli(DecodeBase64(brotlifile.text));
// }
// catch (Exception ex)
// {
//     Debug.LogError($"Decompression error: {ex.Message}");
//     return;
// }

// string jsonString;
// try
// {
//     jsonString = System.Text.Encoding.UTF8.GetString(decompressed);
// }
// catch (Exception ex)
// {
//     Debug.LogError($"Error converting bytes to string: {ex.Message}");
//     return;
// }

// Debug.Log($"JSON STRING: {jsonString}");

// var jsonData = OVRSimpleJSON.JSON.Parse(jsonString);

// List<string> values = new List<string>() { "X", "Y", "Z" };

// if (jsonData.IsNull)
// {
//     Debug.Log($"Data is null: {jsonData}");
//     return;
// }
// else
// {
//     Debug.Log($"Data is null: {jsonData}");
// }

// for (var i = 0; i < jsonData["data"][0].Count; i++)
// {
//     depthDataFromFile.AddDepthDataPoint(jsonData["data"][0][i].AsInt, jsonData["data"][1][i].AsInt, jsonData["data"][2][i].AsInt);
// }

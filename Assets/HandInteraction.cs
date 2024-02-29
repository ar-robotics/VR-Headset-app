using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OculusSampleFramework;


/// <summary>
/// This script is used to send hand tracking data to the server at regular intervals.
/// It is used for testing purposes. 
/// </summary>
public class HandInteraction : MonoBehaviour
{

    /// <summary>
    /// The OVRHand component used to track hand gestures.
    /// </summary>
    private OVRHand hand;

    /// <summary>
    /// The time of the last data send. 
    /// </summary>
    private float lastSendTime;

    /// <summary>
    /// The interval at which to send data to the server.
    /// </summary>
    public float sendInterval = 0.1f;

    /// <summary>
    /// The NetworkManager component used to send data to the server.
    /// </summary>
    private NetworkManager networkManager;

    /// <summary>
    /// Flag to indicate if the script is running in testing mode.
    /// </summary>
    public bool isTesting = false;

    /// <summary>
    /// Start is called before the first frame update and is used to initialize the script.
    /// Returns is isTesting is false.
    /// </summary>
    void Start()
    {
        if (!isTesting) return;

        hand = GetComponent<OVRHand>();
        Debug.Log("Hand Interaction Script is running!");
        // Find the NetworkManager component on this GameObject
        networkManager = NetworkManager.Instance;
        NetworkManager.Instance.OnDataReceived += HandleReceivedData;
    }

    /// <summary>
    /// Update is called once per frame and is used to send hand tracking data to the server at regular intervals.
    /// Returns if isTesting is false.
    /// </summary>
    void Update()
    {
        if (!isTesting) return;

        bool isIndexFingerPinching = hand.GetFingerIsPinching(OVRHand.HandFinger.Index);
        float indexFingerPinchStrength = hand.GetFingerPinchStrength(OVRHand.HandFinger.Index);
        OVRHand.TrackingConfidence confidence = hand.GetFingerConfidence(OVRHand.HandFinger.Index);
        Debug.Log($"Index finger pinching: {isIndexFingerPinching}");
        Debug.Log($"Strength: {indexFingerPinchStrength}");
        Debug.Log($"Confidence: {confidence}");
        ///
        /// Hand Tracking
        /// 
        Vector3 handPosition = hand.transform.position;
        Debug.Log($"Hand Position: {handPosition}");
        // Send data at intervals
        if (Time.time - lastSendTime > sendInterval)
        {
            string data = $"{handPosition}";
            SendDataToServer(data);
            SendDataToServer($"{handPosition.y}".ToString());
            lastSendTime = Time.time;
        }

    }

    /// <summary>
    /// Handles the received data from the server.
    /// <param name="data">The received data.</param>
    /// </summary>
    private void HandleReceivedData(string data)
    {
        // Process the received data
        Debug.Log($"ObjectController received data: {data}");
    }

    /// <summary>
    /// Sends the data to the server.
    /// <param name="data">The data to send.</param>
    /// </summary>
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

    void OnDestroy()
    {
        // It's important to unsubscribe when the GameObject is destroyed
        NetworkManager.Instance.OnDataReceived -= HandleReceivedData;
    }
}

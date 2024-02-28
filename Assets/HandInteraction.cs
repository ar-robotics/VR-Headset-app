using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OculusSampleFramework;

public class HandInteraction : MonoBehaviour
{

    private OVRHand hand;
    private float lastSendTime;
    public float sendInterval = 0.1f;
    private NetworkManager networkManager;
    void Start()
    {
        hand = GetComponent<OVRHand>();
        Debug.Log("Hand Interaction Script is running!");
        // Find the NetworkManager component on this GameObject
        networkManager = NetworkManager.Instance;
        NetworkManager.Instance.OnDataReceived += HandleReceivedData;
    }

    // Update is called once per frame
    void Update()
    {
        ///
        /// Hand Pinching
        /// Methods to get the track hand pinching
        ///
        // bool isIndexFingerPinching = hand.GetFingerIsPinching(OVRHand.HandFinger.Index);
        // float indexFingerPinchStrength = hand.GetFingerPinchStrength(OVRHand.HandFinger.Index);
        // OVRHand.TrackingConfidence confidence = hand.GetFingerConfidence(OVRHand.HandFinger.Index);

        // Debug.Log($"Index finger pinching: {isIndexFingerPinching}");
        // Debug.Log($"Strength: {indexFingerPinchStrength}");
        // Debug.Log($"Confidence: {confidence}"); ;

        // ///
        // /// Hand Tracking
        // /// 
        // Vector3 handPosition = hand.transform.position;
        // Debug.Log($"Hand Position: {handPosition}");
        // // Send data at intervals
        // if (Time.time - lastSendTime > sendInterval)
        // {
        //     string data = $"{handPosition}";
        //     // SendDataToServer(data);
        //     // SendDataToServer($"{handPosition.y}".ToString());
        //     lastSendTime = Time.time;

        // }

    }
    private void HandleReceivedData(string data)
    {
        // Process the received data
        Debug.Log($"ObjectController received data: {data}");
    }

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

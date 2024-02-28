using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
// i want to use time
using System.Timers;
using System.Linq;
using System;

public class HandDetectionCube : MonoBehaviour
{
    class RobotControlValues
    {
        public float x;
        public float y;  // for the robot, z is y
        public int speed;
    }

    public class RobotControlX
    {
        public float x;
        public float y;
        public int pinch;
        public float strength;
    }

    public DropdownHandler dropdownHandler;

    public OVRSkeleton handSkeleton;
    public OVRHand rightHand;
    private RobotControlValues controlValues = new RobotControlValues();
    private RobotControlX controlX = new RobotControlX();
    private NetworkManager networkManager;
    private bool isHandDetected = false;
    public float distanceCalculationInterval = 0.5f;   // Interval in seconds to calculate distances
    float lastSendTime;
    public float sendInterval = 0.5f;
    // The visualIndicator 
    private Transform visualIndicatorTransform;

    private Material cubeMaterial;
    public Color insideColor = new Color(1, 0, 0, 0.1f); // Red with low opacity
    private Color originalColor;

    void Start()
    {
        // Working but we have to install some packages to get access to the logfile in VR headset
        // It should be easly accessible in the VR headset
        FindObjectOfType<Logger>().Log("Test log file.");
        // Finding the visualIndicator child of the cube
        visualIndicatorTransform = transform.Find("visualIndicator");
        // Debug.Log($"Indicator position: {visualIndicatorTransform.localPosition.y}");
        if (visualIndicatorTransform == null)
        {
            Debug.LogError("VisualIndicator child not found!");
        }

        // Get the material and the color of the visualIndocatorCube
        cubeMaterial = GetComponent<Renderer>().material;
        originalColor = cubeMaterial.color; // Save the original color
        networkManager = NetworkManager.Instance;
    }

    void Update()
    {
        int Hand_WristRoot = (int)OVRPlugin.BoneId.Hand_WristRoot;

        // Loging to the VR LOG screen in the VR headset
        OVRBone WristBone = handSkeleton.Bones[Hand_WristRoot];
        Debug.Log($"Hand_WristRoot bone ID: {Hand_WristRoot}");
        Debug.Log($"Hand_WristRoot bone position: {WristBone.Transform.position}");

        Debug.Log($"CurrentNumBones: {handSkeleton.GetCurrentNumBones()}");
        Debug.Log($"Right Hand start: {handSkeleton.GetCurrentStartBoneId()}");
        Debug.Log($"Right Hanh end: {handSkeleton.GetCurrentEndBoneId()}");
    }


    /// <summary>
    /// Happens when colliders enter the cube
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        // Check if TipBoneEnd is in the cube
        if (other.CompareTag("TipboneSphere"))
        {
            isHandDetected = true;
            Debug.Log($"Visualindicator entered cube area.{other.bounds.size}");
            Debug.Log($"Other size: {other.bounds.size}");

            // Change the color of the cube when the hand enters the cube
            cubeMaterial.color = insideColor;

            // Start coroutine to calculate distances repeatedly
            int Hand_WristRoot = (int)OVRPlugin.BoneId.Hand_MiddleTip;
            OVRBone WristBone = handSkeleton.Bones[Hand_WristRoot];
            StartCoroutine(RepeatedlyDistanceCalculation(WristBone.Transform));
        }


    }

    /// <summary>
    /// Stop the coroutine when the hand leaves the cube
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("TipboneSphere"))
        {
            isHandDetected = false;
            Debug.Log("Hand exited cube area.");
            // Reset the color of the cube when the hand leaves the cube
            cubeMaterial.color = originalColor;

            // Stop the coroutine when the hand leaves the Cude
            StopAllCoroutines();

            // Reset the control values to stop the robot
            ResetControlValues();

            // Stop the robot when the had leaves the control area(Cube)
            string json = JsonUtility.ToJson(controlValues);
            SendDataToServer(json);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // not sure if this is necessary
    }

    /// <summary>
    /// Reset the control values to stop the robot, this makes the robot stop when the hand leaves the cube
    /// </summary>
    private void ResetControlValues()
    {
        controlValues.x = 0;
        controlValues.y = 0;
        controlValues.speed = 0;
    }

    private IEnumerator RepeatedlyDistanceCalculation(Transform handTransform)
    {

        while (isHandDetected)
        {
            CalculateNormalizedControlValues(handTransform.position);
            yield return new WaitForSeconds(distanceCalculationInterval);
        }
    }
    private void CalculateDistances(Vector3 handPosition)
    {
        // Assuming the cube's local scale is uniform and using its position to find edges
        float halfScaleX = transform.localScale.x / 2;
        float halfScaleZ = transform.localScale.z / 2;

        // Calculate distances from the hand to each edge
        float distanceToLeftEdge = handPosition.x - (transform.position.x - halfScaleX);
        float distanceToRightEdge = (transform.position.x + halfScaleX) - handPosition.x;
        float distanceToFrontEdge = (transform.position.z + halfScaleZ) - handPosition.z;
        float distanceToBackEdge = handPosition.z - (transform.position.z - halfScaleZ);

        // Log distances to the console
        Debug.Log($"Distance to Left Edge: {distanceToLeftEdge}");
        Debug.Log($"Distance to Right Edge: {distanceToRightEdge}");
        Debug.Log($"Distance to Front Edge: {distanceToFrontEdge}");
        Debug.Log($"Distance to Back Edge: {distanceToBackEdge}");


    }

    /// <summary>
    /// Calculate the normalized control values based on the hand position within the cube
    /// </summary>
    /// <param name="handPosition"></param>
    private void CalculateNormalizedControlValues(Vector3 handPosition)
    {
        // Convert hand position to the cube's local space
        Vector3 handLocalPosition = transform.InverseTransformPoint(handPosition);

        // Calculate normalized X within -1 to 1 range
        // This directly uses the local position within the cube, assuming cube's center at local origin
        float normalizedX = Mathf.Clamp(handLocalPosition.x / (transform.localScale.x / 2), -1, 1);

        // Calculate normalized Z within 0 to 1 range
        // Adjust the calculation to correctly map the full depth of the cube from back (0) to front (1)
        float normalizedZ = Mathf.Clamp((handLocalPosition.z / (transform.localScale.z / 2) + 1) / 2, 0, 1);


        float scaleY = transform.localScale.y;

        float normalizedY = (handLocalPosition.y + scaleY / 2) / scaleY;
        normalizedY = Mathf.Clamp(normalizedY, 0, 1);

        // Log normalized values to the console (or use these values for robot control, etc.)
        float mappedY = (normalizedY - 0.3f) * 3.0f;
        Debug.Log($"Normalized X: {normalizedX}");
        Debug.Log($"Normalized Z: {normalizedZ}");
        Debug.Log($"Normalized Y: {mappedY}");


        // Calculate speed, send control values to the robot, and update the visual indicator
        int speed = CalculateSpeed(normalizedX, normalizedZ);
        SendControlValues(normalizedX, normalizedZ, mappedY, speed); // Modify to calculate tresholds for X and Z?
        UpdateVisualIndicator(normalizedX, normalizedZ);
    }

    private void UpdateVisualIndicator(float normalizedX, float normalizedZ)
    {

        float unchangedY = visualIndicatorTransform.localPosition.y;

        float scaleX = transform.localScale.x / 2; // Half size of the detection cube in X
        float scaleZ = transform.localScale.z / 2; // Half size of the detection cube in Z

        // Map the normalized control values back to the world position within the detection cube
        float worldX = normalizedX * scaleX;
        float worldZ = normalizedZ * scaleZ;

        // Update the position of the visual indicator within the detection cube
        // Assuming the visual indicator should move at a constant height above the detection cube
        Debug.Log($"New Position - X: {worldX}");
        Debug.Log($"New Position -Y: {unchangedY}");
        Debug.Log($"New Position -Z: {worldZ}");

        Vector3 newPosition = new Vector3(worldX, unchangedY, worldZ);
        visualIndicatorTransform.localPosition = newPosition;
    }

    /// <summary>
    ///  Sending the information to the robot through socket communication.
    ///  Speed values is between -100 and 100, where 0 is stop, 100 is full speed forward, and -100 is full speed backward
    /// </summary>
    /// <param name="normalizedX"></param>
    /// <param name="normalizedZ"></param>
    /// <param name==speed></param>" 
    private void SendControlValues(float normalizedX, float normalizedZ, float normalizedY, int speed = 50)
    {

        int dropdownValue = dropdownHandler.GetDropdownValue();
        string data = "";

        switch (dropdownValue)
        {
            case 0:
                // Idle mode
                break;
            case 1:
                // Drive mode
                controlValues.x = normalizedX;
                controlValues.y = normalizedZ;
                controlValues.speed = speed;
                data = JsonUtility.ToJson(controlValues);
                Debug.Log($"Drive values: {data}");
                Debug.Log($"Speed: {speed}");
                break;
            case 2:
                // Arm mode
                controlX.x = normalizedX;
                controlX.y = normalizedY;
                controlX.pinch = rightHand.GetFingerIsPinching(OVRHand.HandFinger.Index) ? 1 : 0;
                controlX.strength = rightHand.GetFingerPinchStrength(OVRHand.HandFinger.Index);
                data = JsonUtility.ToJson(controlX);
                Debug.Log($"Control ARM values: {data}");

                break;
            case 3:
                // Emergency stop

                break;
        }

        if (!dropdownValue.Equals(0) && (lastSendTime == 0 || Time.time - lastSendTime > sendInterval))
        {
            SendDataToServer(data);
            lastSendTime = Time.time;
            // Convert the control values to a string
        }
        else
        {
            Debug.Log("No data sent to the robot.");
        }

    }

    /// <summary>
    /// Calculate the speed based on the normalized X and Z values
    /// </summary>
    /// <param name="normalizedX"></param>
    /// <param name="normalizedZ"></param>
    /// <returns></returns>
    private int CalculateSpeed(float normalizedX, float normalizedZ)
    {
        float calculatedSpeed = 0;
        if (normalizedX < 0.3 && normalizedX > -0.3 && normalizedZ < 0.3)
        {
            calculatedSpeed = 0;
        }
        else if (normalizedX > 0.3 || normalizedX < -0.3)
        {
            calculatedSpeed = Math.Abs(100 * normalizedX);
        }
        else if (normalizedZ > 0.4)
        {
            calculatedSpeed = Math.Abs(100 * normalizedZ);
        }
        return (int)calculatedSpeed;
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
}
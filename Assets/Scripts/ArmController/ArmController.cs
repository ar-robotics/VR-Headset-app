using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
// i want to use time
using System.Timers;
using System.Linq;
using System;

/// <summary>
/// This script is used to detect the hand position within a cube and send control values to the robot based on the hand position.
/// This class is used for both controlling the arm.
/// This class will be refactored to be more modular and to use interfaces which implemented as state machines.
/// </summary>
public class ArmController : MonoBehaviour
{
    /// <summary>
    /// The control values used to control the robot for controlling the robot (car).
    /// The is the interface between the VR and the Robot. 
    /// <remarks> This interface has to match with the ROS2 interface from the robot </remarks>
    /// </summary>
    class RobotControlValues
    {
        public float x;
        public float y;  // for the robot, z is y
        public int speed;
    }

    /// <summary>
    /// The control values used to control the robot for controlling the arm.
    /// The is the interface between the VR and the Robot. 
    /// <remarks> This interface has to match with the ROS2 interface from the robot </remarks>
    /// </summary>
    public class RobotControlX
    {
        public float x;
        public float y;
        public float z;
        public int pinch;
        public float strength;
    }

    /// <summary>
    /// The DropdownHandler component used to get the mode values from the UI(VR).
    /// Modes are: Idle, Drive, Arm, and Emergency stop
    /// </summary>
    /// <remarks> The mode values (0 , 1, 2, 3) has to match with the ROS2 interface from the robot </remarks>
    public DropdownHandler dropdownHandler;

    /// <summary>
    /// The OVRSkeleton component used to track hand gestures.
    /// </summary>
    public OVRSkeleton handSkeleton;

    /// <summary>
    /// The OVRHand component used to track hand gestures.
    /// </summary>
    public OVRHand rightHand;
    private RobotControlValues controlValues = new RobotControlValues();
    private RobotControlX controlX = new RobotControlX();

    /// <summary>
    /// The NetworkManager component used to send data to the server.
    /// </summary>
    private NetworkManager networkManager;

    /// <summary> 
    /// The flag to indicate if the hand is detected within the cube.
    /// </summary>
    private bool isHandDetected = false;

    /// <summary>  
    /// The interval at which to calculate the distances.
    /// </summary>
    public float distanceCalculationInterval = 0.5f;   // Interval in seconds to calculate distances
    float lastSendTime;

    /// <summary>
    /// The interval at which to send data to the server.
    /// </summary>
    public float sendInterval = 0.5f;

    /// <summary>
    /// The transform of the visual indicator. That is a dot which indicates the x and z position of the TipBoneEnd of the users hand.
    /// </summary>
    private Transform visualIndicatorTransform;

    private Material cubeMaterial;
    public Color insideColor = new Color(1, 0, 0, 0.1f); // Red with low opacity
    private Color originalColor;

    /// <summary>
    /// The script is called before the first frame update and is used to initialize the necessary variables.
    /// </summary>
    void Start()
    {
        // Working but we have to install some packages to get access to the logfile in VR headset
        // It should be easly accessible in the VR headset
        FindObjectOfType<Logger>().Log("Test log file.");
        // Finding the visualIndicator child of the cube
        visualIndicatorTransform = transform.Find("VisualIndicatorHand");
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


    /// <summary>
    /// Update is called once per frame and is used to log the position of the Hand_WristRoot bone in the VR headset.
    /// This is used for testing purposes.
    /// </summary>
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
    /// This method is called when the hand enters the cube area. It changes the color of the cube and starts the coroutine to calculate the distances repeatedly.
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

    /// <summary>
    /// Asynchronous method to calculate the distances repeatedly when the coroutine is started.
    /// </summary>
    /// <param name="handTransform"></param>
    private IEnumerator RepeatedlyDistanceCalculation(Transform handTransform)
    {

        while (isHandDetected)
        {
            CalculateNormalizedControlValues(handTransform.position);
            yield return new WaitForSeconds(distanceCalculationInterval);
        }
    }

    /// <summary>
    /// Calculate the distances from the hand to the edges of the cube
    /// This is used for debugging and understanding the hand position within the cube
    /// </summary>
    /// <param name="handPosition"></param>
    private void CalculateDistances(Vector3 handPosition)
    {
        float halfScaleX = transform.localScale.x / 2;
        float halfScaleZ = transform.localScale.z / 2;

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
        float normalizedX = Mathf.Clamp(handLocalPosition.x / (transform.localScale.x / 2), -1, 1);

        // Calculate normalized Z within 0 to 1 range
        float normalizedZ = Mathf.Clamp((handLocalPosition.z / (transform.localScale.z / 2) + 1) / 2, 0, 1);

        float normalizedY = handLocalPosition.y + 0.50000f;
        float mappedY;

        if (normalizedY < 0.3)
        {
            mappedY = normalizedY - 0.3f;
        }
        else
        {
            // Range which comes inside here is 0,3 between and 1 is mapped
            mappedY = (normalizedY - 0.3f) / (1 - 0.3f);
        }

        normalizedY = mappedY;
        Debug.Log($"HandLocalPosition: {handLocalPosition.y}");
        Debug.Log($"Transform localScale: {transform.localScale.y}");
        Debug.Log($"Normalized X: {normalizedX}");
        Debug.Log($"Normalized Z: {normalizedZ}");
        Debug.Log($"Normalized Y: {normalizedY}");

        SendControlValues(normalizedX, normalizedY, normalizedZ);
        // UpdateVisualIndicator(normalizedX, normalizedZ, normalizedY);
    }

    /// <summary>
    /// Update the position of the visual indicator within the detection cube according to the normalized X and Z values.
    /// This is to give a feedback to the user about the hand position within the cube which indicates the direction and the speed of the robot.
    /// </summary>
    /// <param name="normalizedX"></param>
    /// <param name="normalizedZ"></param>
    private void UpdateVisualIndicator(float normalizedX, float normalizedZ, float normalizedY)
    {

        float unchangedY = visualIndicatorTransform.localPosition.y;

        float scaleX = transform.localScale.x / 2; // Half size of the detection cube in X
        float scaleZ = transform.localScale.z / 2; // Half size of the detection cube in Z
        float scaleY = transform.localScale.y + ((1.1f - (-0.3f) - 1) / 2);

        // Map the normalized control values back to the world position within the detection cube
        float worldX = normalizedX * scaleX;
        float worldZ = normalizedZ * scaleZ * 2 - scaleZ;
        float worldY = normalizedY * scaleY;

        worldZ = Mathf.Clamp(worldZ, -scaleZ, scaleZ);

        Debug.Log($"New Position - X: {worldX}");
        Debug.Log($"New Position -Y: {worldY}");
        Debug.Log($"New Position -Z: {worldZ}");

        Vector3 newPosition = new Vector3(worldX, worldY, worldZ);
        visualIndicatorTransform.localPosition = newPosition;
    }

    /// <summary>
    ///  Sending the information to the robot through socket communication.
    ///  The directions is calculated based on the normalized X and Z values. X goes from -1 (drive left) to 1 (drive right) and Z goes from 0 (stop) to 1 (drive forward).
    ///  Speed is calculated based in how far the hand is from the edges, the closer the hand is to the edge the faster the robot moves.
    ///  The max speed is when the hand reaches the edge of the cube.
    /// </summary>
    /// <param name="normalizedX"></param>
    /// <param name="normalizedZ"></param>
    /// <param name="normalizedY"></param>
    /// <param name==speed></param>" 
    private void SendControlValues(float normalizedX, float normalizedY, float normalizedZ)
    {

        int dropdownValue = dropdownHandler.GetDropdownValue();
        string data = "";

        switch (dropdownValue)
        {
            case 0:
                // Idle mode
                break;
            // case 1:
            //     // Drive mode
            //     controlValues.x = normalizedX;
            //     controlValues.y = normalizedZ;
            //     controlValues.speed = speed;
            //     data = JsonUtility.ToJson(controlValues);
            //     Debug.Log($"Drive values: {data}");
            //     Debug.Log($"Speed: {speed}");
            //     break;
            case 2:
                // Arm mode
                controlX.x = normalizedX;
                controlX.z = normalizedY;
                controlX.y = normalizedZ;
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

    /// <summary>
    /// Handles the recieved data from the server.
    /// It is not in use at the moment.
    /// </summary>
    private void HandleReceivedData(string data)
    {
        // Process the received data
        Debug.Log($"ObjectController received data: {data}");
    }

    /// <summary>
    /// Sends the data to the server.
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
}
using UnityEngine;

/// <summary>
/// This class is used to get the hand gesture and rotation data and send it to the server.
/// It is used for testing purposes to control the robot arm with hand gestures. 
/// <remarks> This class does not work as expected and needs to be fixed </remarks> 
/// </summary>
public class HandGestureAndRotation : MonoBehaviour
{
    /// <summary>
    /// The OVRSkeleton component used to track hand gestures.
    /// </summary>
    public OVRSkeleton handSkeleton;

    /// <summary>
    /// The OVRHand component used to track hand gestures.
    /// </summary>
    public OVRHand hand;

    /// <summary>
    /// The NetworkManager component used to send data to the server.
    /// </summary>
    private NetworkManager networkManager;
    float lastSendTime;

    /// <summary>
    /// The interval at which to send data to the server.
    /// </summary>
    public float sendInterval = 0.3f;

    /// <summary>
    /// Flag to indicate if the script is running in testing mode.
    /// </summary>
    public bool isTesting = false;

    /// <summary>  
    /// The HandData interface used to send the data to the robot.
    /// <remarks> This interface has to match with the ROS2 interface from the robot </remarks>
    class HandData
    {
        public int pinch;
        public int wrist;
    }

    HandData handData = new HandData();

    /// <summary>
    /// Initilization of the script by finding the NetworkManager component in the scene.
    /// </summary>
    void Start()
    {
        if (!isTesting) return;
        networkManager = NetworkManager.Instance;

    }

    /// <summary>
    /// Update is called once per frame and is used to get the hand gesture and rotation data and send it to the server.
    /// Returns if isTesting is false.
    /// </summary>
    private void Update()
    {
        if (!isTesting) return;
        if (handSkeleton == null || hand == null)
        {
            Debug.LogWarning("HandSkeleton or OVRHand reference is missing.");
            return;
        }

        // Ensure the skeleton and hand are fully tracked
        if (!handSkeleton.IsDataValid || !handSkeleton.IsDataHighConfidence || !hand.IsTracked)
        {
            return;
        }

        // Accessing the MiddleTip bone
        int handMiddleTipIndex = (int)OVRPlugin.BoneId.Hand_MiddleTip;
        if (handMiddleTipIndex < 0 || handMiddleTipIndex >= handSkeleton.Bones.Count)
        {
            Debug.LogWarning("Hand_MiddleTip index is out of range.");
            return;
        }
        OVRBone middleTipBone = handSkeleton.Bones[handMiddleTipIndex];

        // Check for pinch
        bool isPinching = hand.GetFingerIsPinching(OVRHand.HandFinger.Index);

        // Output the pinch status and rotation of the MiddleTip bone
        Debug.Log($"Is Pinching: {isPinching}");
        Debug.Log($"MiddleTip Bone Rotation: {middleTipBone.Transform.rotation}");

        handData.pinch = isPinching ? 1 : 0;

        Quaternion handRootRotation = handSkeleton.Bones[(int)OVRPlugin.BoneId.Hand_WristRoot].Transform.rotation;

        // To display or use this rotation:
        // Convert to Euler angles for easier understanding or display
        Vector3 handRootEuler = handRootRotation.eulerAngles;

        // Optionally convert to radians
        Vector3 handRootRadians = new Vector3(handRootEuler.x * Mathf.Deg2Rad, handRootEuler.y * Mathf.Deg2Rad, handRootEuler.z * Mathf.Deg2Rad);

        Debug.Log($"Hand Root Rotation (Euler): {handRootEuler}");
        Debug.Log($"Hand Root Rotation (Radians): {handRootRadians}");

        // Convert the handData object to a JSON string
        string data = JsonUtility.ToJson(handData);

        // Send the data to the server
        if (Time.time - lastSendTime > 0.1f)
        {
            SendDataToServer(data);
            lastSendTime = Time.time;
        }

        // Optionally perform actions based on pinch status and bone rotation
        // For example, you could trigger an event or control an object in your scene
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
}

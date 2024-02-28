using UnityEngine;
public class HandGestureAndRotation : MonoBehaviour
{
    public OVRSkeleton handSkeleton;
    public OVRHand hand;

    private NetworkManager networkManager;
    float lastSendTime;
    public float sendInterval = 0.3f;

    class HandData
    {
        public int pinch;
        public int wrist;
    }

    HandData handData = new HandData();
    void Start()
    {
        networkManager = NetworkManager.Instance;

    }
    private void Update()
    {
        // NOT in use for now!
        return;
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

using UnityEngine;

/// <summary>
/// This class is used to display a green dot that is attached to the Tip of the Middle Finger of the Hand.
/// It displays the green dot in the VR environment with 2 cm offset from the wrist bone.
/// </summary>
public class WristMarker : MonoBehaviour
{
    /// <summary>
    /// The OVRSkeleton component used to get the bone data.
    /// </summary>
    public OVRSkeleton skeleton;

    /// <summary>
    /// The GameObject used to display the green dot. A sphere is used to display the green dot.
    /// </summary>
    public GameObject greenDot;

    /// <summary>
    /// The index of the Hand_MiddleTip bone in the OVRSkeleton.Bones array.
    /// </summary>
    int Hand_MiddleTip = (int)OVRPlugin.BoneId.Hand_MiddleTip;
    void Start()
    {
    }

    /// <summary>
    /// Update is called once per frame and is used to update the position of the green dot to the wrist bone's position.
    /// </summary>
    void Update()
    {
        if (skeleton == null || greenDot == null)
            return;

        // Debug.Log("Time.captureFramerate: " + Time.captureFramerate);
        OVRBone WristBone = skeleton.Bones[Hand_MiddleTip];
        if (WristBone != null)
        {
            // Update the green dot's position to the wrist bone's position
            // Assuming an offset of 0.02 meters (2 cm) above the wrist bone
            greenDot.transform.position = WristBone.Transform.position + Vector3.up * 0.01f;

            Debug.Log($"Green dot position: {greenDot.transform.position}");
        }

    }

}

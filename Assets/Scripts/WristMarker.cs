using UnityEngine;

/// <summary>
/// This script is used to update the position of the green dot to the wrist bone's position
/// </summary>
public class WristMarker : MonoBehaviour
{
    public OVRSkeleton skeleton;
    public GameObject greenDot;
    int Hand_WristRoot = (int)OVRPlugin.BoneId.Hand_MiddleTip;
    void Start()
    {

    }
    void Update()
    {
        if (skeleton == null || greenDot == null)
            return;
        OVRBone WristBone = skeleton.Bones[Hand_WristRoot];
        if (WristBone != null)
        {
            // Update the green dot's position to the wrist bone's position
            // Assuming an offset of 0.02 meters (2 cm) above the wrist bone
            greenDot.transform.position = WristBone.Transform.position + Vector3.up * 0.02f;

            Debug.Log($"Green dot position: {greenDot.transform.position}");
        }
    }
}

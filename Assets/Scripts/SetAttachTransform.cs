using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// This script is used to set the attach transform of an XR poke interactor to a specific bone in the hand skeleton.
/// The middle finger tip bone is used as the attach transform and it is used for poking interactions, such as poking buttons.
/// This is used for interaction with the UI Menu in the VR environment.
/// </summary>  
public class SetAttachTransform : MonoBehaviour
{
    /// <summary>
    /// The XRBaseInteractor component used to interact with the UI Menu.
    /// </summary>
    public XRBaseInteractor pokeInteractor;

    /// <summary>
    /// The OVRSkeleton component used to get the bone data.
    /// </summary>
    public OVRSkeleton handSkeleton;

    /// <summary>
    /// The index of the finger tip bone in the OVRSkeleton.Bones array.
    /// </summary>
    int poke_finger_tip_id = 20;

    /// <summary>
    /// Start is called before the first frame update and is used to set the attach transform of the poke interactor to the middle finger tip bone.
    /// </summary>
    void Start()
    {
        if (handSkeleton != null && pokeInteractor != null)
        {
            // Attempt to find the desired bone in the skeleton
            OVRBone bone = handSkeleton.Bones[poke_finger_tip_id];
            if (bone != null)
            {
                // If the bone is found, set the interactor's attach transform to the bone's transform
                pokeInteractor.attachTransform = bone.Transform;
            }
            else
            {
                Debug.LogError("Desired bone not found in the skeleton.");
            }
        }
        else
        {
            Debug.LogError("HandSkeleton or Interactor is not assigned.");
        }
    }
}

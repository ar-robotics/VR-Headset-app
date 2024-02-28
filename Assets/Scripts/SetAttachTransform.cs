using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SetAttachTransform : MonoBehaviour
{
    public XRBaseInteractor pokeInteractor;
    public OVRSkeleton handSkeleton;

    int poke_finger_tip_id = 20;
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

using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class LimitGrabRotationYOnly : MonoBehaviour
{
    XRGrabInteractable grab;
    public bool onlyWhileGrabbed = true; 

    void Awake() => grab = GetComponent<XRGrabInteractable>();

    void LateUpdate()
    {
        if (onlyWhileGrabbed && (grab == null || !grab.isSelected)) return;
        Vector3 fwd = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
        if (fwd.sqrMagnitude < 1e-6f) return; 
        transform.rotation = Quaternion.LookRotation(fwd.normalized, Vector3.up);
    }
}

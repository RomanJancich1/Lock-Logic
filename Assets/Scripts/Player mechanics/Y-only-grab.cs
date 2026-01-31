using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class YawOnlyGrab : MonoBehaviour
{
    XRGrabInteractable grab;
    Vector3 fixedWorldPos;

    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        fixedWorldPos = transform.position;   
    }

    void LateUpdate()
    {
        transform.position = fixedWorldPos;

        var yaw = transform.eulerAngles.y;
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
    }
}

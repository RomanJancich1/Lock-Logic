using UnityEngine;

public class HudAttachToMainCamera : MonoBehaviour
{
    public Vector3 localPos = new Vector3(0f, 0.25f, 0.8f);

    void Start()
    {
        if (Camera.main == null) return;
        transform.SetParent(Camera.main.transform, false);
        transform.localPosition = localPos;
        transform.localRotation = Quaternion.identity;
    }
}

using UnityEngine;
public class LockYHeight : MonoBehaviour
{
    public float lockedY = 0f;   
    void LateUpdate()
    {
        var p = transform.position;
        transform.position = new Vector3(p.x, lockedY, p.z);
    }
}

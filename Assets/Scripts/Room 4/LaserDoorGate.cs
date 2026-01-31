using UnityEngine;

public class LaserDoorGate : MonoBehaviour
{
    public LaserReceiver receiver;
    public MyDoorController door;

    void Start()
    {
        if (door) door.Lock();
        if (receiver) receiver.OnSolved.AddListener(() => { if (door) door.Unlock(); });
    }
}

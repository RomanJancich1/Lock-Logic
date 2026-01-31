using UnityEngine;

public class DoorTrigger_room3 : MonoBehaviour
{
    public MultiStagePadController pad;
    public DoorController door;

    void OnEnable()
    {
        if (door) door.Lock();
        if (pad) pad.OnSolvedAll.AddListener(HandleSolved);
    }

    void OnDisable()
    {
        if (pad) pad.OnSolvedAll.RemoveListener(HandleSolved);
    }

    void HandleSolved()
    {
        if (door) door.Unlock();
    }
}

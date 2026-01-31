using UnityEngine;

public class DoorController : MonoBehaviour
{
    public Animator animator;  
    public bool isLocked = true;

    void Awake() { isLocked = true; }
    public void Lock() { isLocked = true; }
    public void Unlock() { isLocked = false; if (animator) animator.SetTrigger("Open"); }
}

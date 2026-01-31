using UnityEngine;

public class EndRoomWinTrigger : MonoBehaviour
{
    [Tooltip("Tag hráča / XR Origin objektu")]
    public string playerTag = "Player";

    bool triggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!string.IsNullOrEmpty(playerTag) && !other.CompareTag(playerTag))
            return;

        triggered = true;

        if (TimerManager.Instance != null)
            TimerManager.Instance.NotifyPlayerFinished();
    }
}

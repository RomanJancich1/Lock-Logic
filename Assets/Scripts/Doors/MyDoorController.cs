using UnityEngine;

public class MyDoorController : MonoBehaviour
{
    [Header("Motion")]
    public float openAngle = 90f;      
    public float openTime = 0.40f;

    [Header("SFX")]
    public AudioSource audioSrc;       
    public AudioClip clipOpen;         
    public AudioClip clipClose;        
    public AudioClip clipLocked;       
    public AudioClip clipUnlocked;     

    Quaternion rotClosed, rotOpen;
    bool isOpen, isMoving;

    [SerializeField] bool isLocked = true;  

    void Awake()
    {
        rotClosed = transform.localRotation;
        rotOpen = rotClosed * Quaternion.Euler(0f, openAngle, 0f);
    }

    
    public void Lock() => isLocked = true;

    public void Unlock()
    {
        if (isLocked && clipUnlocked) audioSrc?.PlayOneShot(clipUnlocked);
        isLocked = false;
    }

    
    public void TryToggle()
    {
        if (isMoving) return;

        if (isLocked)
        {
            if (clipLocked) audioSrc?.PlayOneShot(clipLocked);
            return;
        }

        bool goingToOpen = !isOpen;
        StopAllCoroutines();
        StartCoroutine(RotateTo(goingToOpen ? rotOpen : rotClosed));

        if (audioSrc)
        {
            if (goingToOpen && clipOpen) audioSrc.PlayOneShot(clipOpen);
            if (!goingToOpen && clipClose) audioSrc.PlayOneShot(clipClose);
        }

        isOpen = goingToOpen;
    }

    System.Collections.IEnumerator RotateTo(Quaternion target)
    {
        isMoving = true;
        float t = 0f;
        Quaternion start = transform.localRotation;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.01f, openTime);
            float s = t * t * (3f - 2f * t); 
            transform.localRotation = Quaternion.Slerp(start, target, s);
            yield return null;
        }
        transform.localRotation = target;
        isMoving = false;
    }
}

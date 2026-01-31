using UnityEngine;
using TMPro;

public class MonitorMenu : MonoBehaviour
{
    [Header("UI Roots")]
    public GameObject introUI;
    public TMP_Text titleText;
    public TMP_Text bodyText;

    [Header("Startup")]
    public bool showOnStart = true;

    [Header("Door to unlock on Continue")]
    public MyDoorController doorToUnlock;
    public bool unlockOnlyOnce = true;

    bool alreadyUnlocked = false;

    void Awake()
    {
        if (introUI) introUI.SetActive(false);
    }

    void Start()
    {
        if (showOnStart)
            ShowMenu();
    }

    public void ShowMenu()
    {
        if (introUI) introUI.SetActive(true);
    }

    public void ContinueGame()
    {
        if (introUI) introUI.SetActive(false);

        if (doorToUnlock && (!alreadyUnlocked || !unlockOnlyOnce))
        {
            doorToUnlock.Unlock();
            alreadyUnlocked = true;
        }

        if (TimerManager.Instance != null)
            TimerManager.Instance.StartTimer();
    }

    public void EscapeMenu()
    {
        if (introUI) introUI.SetActive(false);
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void SetTexts(string title, string body)
    {
        if (titleText) titleText.text = title;
        if (bodyText) bodyText.text = body;
    }
}

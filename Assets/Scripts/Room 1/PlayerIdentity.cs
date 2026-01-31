using UnityEngine;

public class PlayerIdentity : MonoBehaviour
{
    public static PlayerIdentity Instance { get; private set; }

    public string CurrentPlayerName { get; private set; } = "";
    public bool CurrentIsAnonymous { get; private set; } = true;
    public bool HasChosenName { get; private set; } = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetAnonymous()
    {
        CurrentPlayerName = LeaderboardManager.Instance.GetNextAnonymousName();
        CurrentIsAnonymous = true;
        HasChosenName = true;
    }

    public bool TrySetNamed(string typedName)
    {
        typedName = (typedName ?? "").Trim();

        if (string.IsNullOrEmpty(typedName))
        {
            SetAnonymous();
            return true;
        }

        if (LeaderboardManager.Instance.IsReservedAnonymousPattern(typedName))
        {
            return false; 
        }

        CurrentPlayerName = typedName;
        CurrentIsAnonymous = false;
        HasChosenName = true;
        return true;
    }
}

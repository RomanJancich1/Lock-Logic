using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;
using System.Collections.Generic;

public class GameEndUI : MonoBehaviour
{
    [Header("Root panel")]
    public GameObject root;

    [Header("Texts")]
    public TMP_Text titleText;
    public TMP_Text bodyText;

    [Header("Groups")]
    public GameObject mainWinGroup;  

    [Header("Buttons")]
    public Button primaryButton;       
    public TMP_Text primaryLabel;

    public Button secondaryButton;     
    public TMP_Text secondaryLabel;

    [Header("Statistics Button (3rd)")]
    public Button statisticsButton;   

    [Header("Statistics UI")]
    public GameObject statsPanel;
    public Button closeStatsButton;
    public Transform rowsParent;
    public StatsRow rowPrefab;
    public int maxRows = 20;

    [Header("Fixed time limit (seconds)")]
    public int fixedLimitSeconds = 600;

    [Header("Movement to freeze")]
    [Tooltip("Drag ONLY locomotion providers here (Teleportation Provider, Continuous Move Provider, Snap/Continuous Turn Provider).")]
    public LocomotionProvider[] movementProviders;

    void Awake()
    {
        if (root) root.SetActive(false);
        if (statsPanel) statsPanel.SetActive(false);

        if (closeStatsButton)
        {
            closeStatsButton.onClick.RemoveAllListeners();
            closeStatsButton.onClick.AddListener(CloseStats);
        }
    }

    void SetMovementEnabled(bool enabled)
    {
        if (movementProviders == null) return;
        foreach (var p in movementProviders)
            if (p) p.enabled = enabled;
    }

    static string FormatMMSS(float seconds)
    {
        if (seconds < 0) seconds = 0;
        int s = Mathf.FloorToInt(seconds + 0.0001f);
        int mm = s / 60;
        int ss = s % 60;
        return $"{mm:0}:{ss:00}";
    }

    void ShowPanel()
    {
        if (root) root.SetActive(true);
    }

    void HidePanel()
    {
        if (root) root.SetActive(false);
    }

    public void ShowLose()
    {
        ShowPanel();

        if (titleText) titleText.text = "Time's up!";
        if (bodyText)
        {
            string lim = FormatMMSS(fixedLimitSeconds);
            bodyText.text =
                $"You didn't make it in {lim}.\n\n" +
                "You can restart from the beginning\n" +
                "or continue to look around.";
        }

        if (primaryLabel) primaryLabel.text = "Restart";
        if (secondaryLabel) secondaryLabel.text = "Continue";

        if (statisticsButton) statisticsButton.gameObject.SetActive(false);

        if (primaryButton) primaryButton.onClick.RemoveAllListeners();
        if (secondaryButton) secondaryButton.onClick.RemoveAllListeners();

        if (primaryButton) primaryButton.onClick.AddListener(RestartGame);
        if (secondaryButton) secondaryButton.onClick.AddListener(LoseContinueFreezeMovement);
    }

    void LoseContinueFreezeMovement()
    {
        HidePanel();
        SetMovementEnabled(false);
    }

    public void ShowWin(float elapsedSeconds)
    {
        ShowPanel();

        string playerName = GetSafePlayerName();
        bool isAnonymous = LeaderboardManager.Instance.IsReservedPlayerName(playerName);

        LeaderboardManager.Instance.AddOrUpdate(playerName, isAnonymous, elapsedSeconds);

        if (titleText) titleText.text = "You escaped!";
        if (bodyText)
        {
            bodyText.text =
                $"Name: {playerName}\n" +
                $"Time: {FormatMMSS(elapsedSeconds)}\n\n" +
                "You can leave the game now, or continue to collect\n" +
                "your final prize.";
        }

        if (primaryLabel) primaryLabel.text = "Exit game";
        if (secondaryLabel) secondaryLabel.text = "Continue";

        if (statisticsButton)
        {
            statisticsButton.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("statisticsButton is not assigned in GameEndUI.");
        }

        if (primaryButton) primaryButton.onClick.RemoveAllListeners();
        if (secondaryButton) secondaryButton.onClick.RemoveAllListeners();
        if (statisticsButton) statisticsButton.onClick.RemoveAllListeners();

        if (primaryButton) primaryButton.onClick.AddListener(ExitGame);
        if (secondaryButton) secondaryButton.onClick.AddListener(ClosePanelOnly);
        if (statisticsButton) statisticsButton.onClick.AddListener(OpenStats);
    }

    string GetSafePlayerName()
    {
        string name = null;

        if (PlayerIdentity.Instance != null)
            name = PlayerIdentity.Instance.CurrentPlayerName;

        name = (name ?? "").Trim();

        if (string.IsNullOrWhiteSpace(name))
            name = LeaderboardManager.Instance.GetNextAnonymousName();

        return name;
    }

    void OpenStats()
    {
        if (!statsPanel)
        {
            Debug.LogWarning("StatsPanel is not assigned in GameEndUI.");
            return;
        }

        if (mainWinGroup) mainWinGroup.SetActive(false);

        statsPanel.SetActive(true);
        RefreshStats();
    }

    void CloseStats()
    {
        if (statsPanel) statsPanel.SetActive(false);

        if (mainWinGroup) mainWinGroup.SetActive(true);
    }


    void RefreshStats()
    {
        if (!rowsParent || !rowPrefab)
        {
            Debug.LogWarning("RowsParent or RowPrefab not assigned in GameEndUI.");
            return;
        }

        for (int i = rowsParent.childCount - 1; i >= 0; i--)
            Destroy(rowsParent.GetChild(i).gameObject);

        List<LeaderboardManager.Entry> list = LeaderboardManager.Instance.GetSorted();
        int count = Mathf.Min(maxRows, list.Count);

        for (int i = 0; i < count; i++)
        {
            var e = list[i];
            var row = Instantiate(rowPrefab, rowsParent);
            row.Set(i + 1, e.name, LeaderboardManager.FormatTime(e.timeSeconds));
        }
    }

    void ClosePanelOnly()
    {
        HidePanel();
        SetMovementEnabled(true);
    }

    void RestartGame()
    {
        SetMovementEnabled(true);
        Scene active = SceneManager.GetActiveScene();
        SceneManager.LoadScene(active.buildIndex);
    }

    void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

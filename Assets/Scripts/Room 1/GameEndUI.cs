using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;

public class GameEndUI : MonoBehaviour
{
    [Header("Root panel")]
    public GameObject root;

    [Header("Texts")]
    public TMP_Text titleText;
    public TMP_Text bodyText;

    [Header("Buttons")]
    public Button primaryButton;
    public TMP_Text primaryLabel;

    public Button secondaryButton;
    public TMP_Text secondaryLabel;

    [Header("Fixed time limit (seconds)")]
    public int fixedLimitSeconds = 600; // 10 minutes

    [Header("Movement to freeze (assign explicitly!)")]
    [Tooltip("Drag ONLY locomotion providers here (Teleportation Provider, Continuous Move Provider, Snap/Continuous Turn Provider). Do NOT add sockets/interactors/interaction manager.")]
    public LocomotionProvider[] movementProviders;

    bool _movementFrozen = false;

    void Awake()
    {
        if (root) root.SetActive(false);
        // IMPORTANT: no auto-find here (prevents grabbing unwanted XR components)
    }

    void SetMovementEnabled(bool enabled)
    {
        if (movementProviders == null) return;

        for (int i = 0; i < movementProviders.Length; i++)
        {
            var p = movementProviders[i];
            if (p) p.enabled = enabled;
        }
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

    // ---------------- LOSE ----------------

    public void ShowLose()
    {
        if (!root) return;

        ShowPanel();

        if (titleText) titleText.text = "Time's up!";
        if (bodyText)
        {
            string lim = FormatMMSS(fixedLimitSeconds);
            bodyText.text =
                $"You didn't make it in {lim}.\n\n" +
                "You can restart from the very beginning,\n" +
                "or continue to look around.";
        }

        if (primaryLabel) primaryLabel.text = "Restart";
        if (secondaryLabel) secondaryLabel.text = "Continue";

        if (primaryButton) primaryButton.onClick.RemoveAllListeners();
        if (secondaryButton) secondaryButton.onClick.RemoveAllListeners();

        if (primaryButton) primaryButton.onClick.AddListener(RestartGame);
        if (secondaryButton) secondaryButton.onClick.AddListener(LoseContinueFreezeMovement);
    }

    void LoseContinueFreezeMovement()
    {
        HidePanel();

        // From now on, player cannot move (until restart / scene reload)
        _movementFrozen = true;
        SetMovementEnabled(false);
    }

    // ---------------- WIN ----------------

    public void ShowWin(float elapsedSeconds)
    {
        if (!root) return;

        ShowPanel();

        if (titleText) titleText.text = "You escaped!";
        if (bodyText)
        {
            string t = FormatMMSS(elapsedSeconds);
            bodyText.text =
                $"You finished it in {t}.\n\n" +
                "You can leave the game now, or continue to collect\n" +
                "your final prize.";
        }

        if (primaryLabel) primaryLabel.text = "Exit game";
        if (secondaryLabel) secondaryLabel.text = "Continue to prize";

        if (primaryButton) primaryButton.onClick.RemoveAllListeners();
        if (secondaryButton) secondaryButton.onClick.RemoveAllListeners();

        if (primaryButton) primaryButton.onClick.AddListener(ExitGame);
        if (secondaryButton) secondaryButton.onClick.AddListener(ClosePanelOnly);
    }

    public void ShowWin() => ShowWin(0f);

    // ---------------- COMMON ----------------

    void ClosePanelOnly()
    {
        HidePanel();

        // Restore movement only if not frozen by Lose->Continue
        if (!_movementFrozen)
            SetMovementEnabled(true);
    }

    void RestartGame()
    {
        // On restart, always restore movement
        _movementFrozen = false;
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

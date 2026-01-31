using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VRNameKeyboard : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject rootPanel;

    [Header("Display")]
    [SerializeField] private TMP_Text inputText;
    [SerializeField] private TMP_Text errorText;

    [Header("Key generation")]
    [SerializeField] private Transform keysGrid;
    [SerializeField] private Button keyButtonPrefab; 

    [Header("Bottom row buttons (drag from Hierarchy)")]
    [SerializeField] private Button btnBackspace;
    [SerializeField] private Button btnClear;
    [SerializeField] private Button btnOk;
    [SerializeField] private Button btnCancel;

    [Header("Limits")]
    [SerializeField] private int maxLen = 16;

    private string current = "";
    private bool generated = false;
    private bool wired = false;

    private void Awake()
    {
        if (rootPanel != null) rootPanel.SetActive(false);
        if (errorText != null) errorText.text = "";
        Repaint();
        WireBottomButtonsOnce();
    }

    private void OnEnable()
    {
        GenerateKeysOnce();
        WireBottomButtonsOnce();
        Repaint();
    }

    public void Show()
    {
        if (rootPanel != null) rootPanel.SetActive(true);
        if (errorText != null) errorText.text = "";
        current = "";
        GenerateKeysOnce();
        WireBottomButtonsOnce();
        Repaint();
    }

    public void Hide()
    {
        if (rootPanel != null) rootPanel.SetActive(false);
    }

    public void Backspace()
    {
        if (current.Length == 0) return;
        current = current.Substring(0, current.Length - 1);
        Repaint();
    }

    public void Clear()
    {
        current = "";
        if (errorText != null) errorText.text = "";
        Repaint();
    }

    public void Confirm()
    {
        string typed = (current ?? "").Trim();

        bool ok = PlayerIdentity.Instance.TrySetNamed(typed);
        if (!ok)
        {
            if (errorText != null) errorText.text = "Name cannot be PlayerN (reserved).";
            return;
        }

        Hide();
    }

    public void Cancel()
    {
        PlayerIdentity.Instance.SetAnonymous();
        Hide();
    }

    private void AddChar(string c)
    {
        if (current.Length >= maxLen) return;
        current += c;
        Repaint();
    }

    private void Repaint()
    {
        if (inputText != null) inputText.text = current;
    }

    private void WireBottomButtonsOnce()
    {
        if (wired) return;

        if (btnBackspace == null || btnClear == null || btnOk == null || btnCancel == null) return;

        wired = true;

        btnBackspace.onClick.RemoveAllListeners();
        btnClear.onClick.RemoveAllListeners();
        btnOk.onClick.RemoveAllListeners();
        btnCancel.onClick.RemoveAllListeners();

        btnBackspace.onClick.AddListener(Backspace);
        btnClear.onClick.AddListener(Clear);
        btnOk.onClick.AddListener(Confirm);
        btnCancel.onClick.AddListener(Cancel);
    }

    private void GenerateKeysOnce()
    {
        if (generated) return;
        if (keysGrid == null || keyButtonPrefab == null) return;

        for (int i = keysGrid.childCount - 1; i >= 0; i--)
            Destroy(keysGrid.GetChild(i).gameObject);

        generated = true;

        string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        string digits = "0123456789";

        foreach (char ch in letters)
            CreateKey(ch.ToString(), () => AddChar(ch.ToString()));

        foreach (char ch in digits)
            CreateKey(ch.ToString(), () => AddChar(ch.ToString()));

        CreateKey("SPACE", () => AddChar(" "));
    }


    private void CreateKey(string label, Action onClick)
    {
        var btn = Instantiate(keyButtonPrefab, keysGrid);
        btn.gameObject.SetActive(true);

        var tmp = btn.GetComponentInChildren<TMP_Text>(true);
        if (tmp != null) tmp.text = label;

        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => onClick.Invoke());
    }

}

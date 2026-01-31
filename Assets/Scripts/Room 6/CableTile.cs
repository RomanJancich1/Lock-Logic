using UnityEngine;
using TMPro;

[RequireComponent(typeof(Collider))]
public class CableTile : MonoBehaviour
{
    public int BaseMask = 0;
    public bool IsSource = false;
    public bool IsTarget = false;
    public bool LockRotation = false;

    public Renderer targetRenderer;

    public Color baseColor = new Color(0.55f, 0.55f, 0.55f, 1f);
    public float baseEmission = 0.08f;

    public Color blinkGreenColor = new Color(0.2f, 1f, 0.2f, 1f);
    public float blinkEmission = 2.2f;

    public GameObject connN;
    public GameObject connE;
    public GameObject connS;
    public GameObject connW;

    public TMP_Text hintTMP;
    public Vector3 hintLocalOffset = new Vector3(0f, 0f, -0.6f);
    public float hintFontSize = 6f;
    public Color hintColor = Color.white;
    public bool hintOutline = false;
    public float hintOutlineWidth = 0.25f;
    public Color hintOutlineColor = Color.black;

    [HideInInspector] public int gx;
    [HideInInspector] public int gy;

    int rotationSteps = 0;
    MaterialPropertyBlock mpb;
    Quaternion hintWorldRotation;

    static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");
    static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");
    static readonly int ColorID = Shader.PropertyToID("_Color");

    public int CurrentMask
    {
        get
        {
            int m = BaseMask & 0xF;
            for (int i = 0; i < rotationSteps; i++)
                m = RotateMask90CW(m);
            return m;
        }
    }

    public void Init(int x, int y)
    {
        gx = x;
        gy = y;

        if (!targetRenderer)
            targetRenderer = GetComponentInChildren<Renderer>(true);

        if (mpb == null)
            mpb = new MaterialPropertyBlock();

        EnsureHintTMP();
        if (hintTMP != null)
            hintWorldRotation = hintTMP.transform.rotation;

        UpdateConnectors();
        ShowBase();
        RefreshHintFromCurrentMask();
    }

    void LateUpdate()
    {
        if (hintTMP != null)
            hintTMP.transform.rotation = hintWorldRotation;
    }

    public void Use()
    {
        if (LockRotation) return;
        Rotate90();
        CableGridManager.Instance?.OnTileRotated(this);
    }

    void Rotate90()
    {
        rotationSteps = (rotationSteps + 1) & 3;
        transform.Rotate(0f, 0f, 90f, Space.Self);
        UpdateConnectors();
        RefreshHintFromCurrentMask();
    }

    public void ResetRotationState()
    {
        rotationSteps = 0;
        Vector3 e = transform.localEulerAngles;
        e.z = 0f;
        transform.localEulerAngles = e;
        UpdateConnectors();
        RefreshHintFromCurrentMask();
    }

    public void ForceSetRotationSteps(int steps)
    {
        steps &= 3;
        rotationSteps = steps;

        Vector3 e = transform.localEulerAngles;
        e.z = steps * 90f;
        transform.localEulerAngles = e;

        UpdateConnectors();
        RefreshHintFromCurrentMask();
    }

    int RotateMask90CW(int m)
    {
        bool n = (m & 1) != 0;
        bool e = (m & 2) != 0;
        bool s = (m & 4) != 0;
        bool w = (m & 8) != 0;

        int r = 0;
        if (w) r |= 1;
        if (n) r |= 2;
        if (e) r |= 4;
        if (s) r |= 8;
        return r;
    }

    void UpdateConnectors()
    {
        int m = CurrentMask;
        if (connN) connN.SetActive((m & 1) != 0);
        if (connE) connE.SetActive((m & 2) != 0);
        if (connS) connS.SetActive((m & 4) != 0);
        if (connW) connW.SetActive((m & 8) != 0);
    }

    void EnsureHintTMP()
    {
        if (hintTMP != null) return;

        hintTMP = GetComponentInChildren<TMP_Text>(true);
        if (hintTMP == null)
        {
            var go = new GameObject("HintTMP");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = hintLocalOffset;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            hintTMP = go.AddComponent<TextMeshPro>();
        }

        hintTMP.alignment = TextAlignmentOptions.Center;
        hintTMP.fontSize = hintFontSize;
        hintTMP.color = hintColor;
        hintTMP.textWrappingMode = TextWrappingModes.NoWrap;
        hintTMP.richText = false;

        var mr = hintTMP.GetComponent<MeshRenderer>();
        if (mr != null) mr.sortingOrder = 1000;

        if (hintTMP.font == null)
        {
            hintTMP.enabled = false;
            return;
        }

        hintTMP.outlineWidth = hintOutline ? hintOutlineWidth : 0f;
        hintTMP.outlineColor = hintOutlineColor;
        hintTMP.enabled = true;
    }

    public void RefreshHintFromCurrentMask()
    {
        EnsureHintTMP();
        if (hintTMP == null || !hintTMP.enabled) return;

        string t = MaskToArrows(CurrentMask);
        hintTMP.text = string.IsNullOrEmpty(t) ? "•" : t;
    }

    string MaskToArrows(int mask)
    {
        mask &= 0xF;

        bool n = (mask & 1) != 0;
        bool e = (mask & 2) != 0;
        bool s = (mask & 4) != 0;
        bool w = (mask & 8) != 0;

        int cnt = (n ? 1 : 0) + (e ? 1 : 0) + (s ? 1 : 0) + (w ? 1 : 0);
        if (cnt == 0) return "";

        if (cnt == 1)
        {
            if (n) return "↑";
            if (e) return "→";
            if (s) return "↓";
            return "←";
        }

        if (cnt == 2)
        {
            if (n && s) return "↑↓";
            if (e && w) return "←→";
            if (n && e) return "↑→";
            if (n && w) return "↑←";
            if (s && e) return "↓→";
            if (s && w) return "↓←";
        }

        string t = "";
        if (n) t += "↑";
        if (e) t += "→";
        if (s) t += "↓";
        if (w) t += "←";
        return t;
    }

    public void ShowBase()
    {
        SetColor(baseColor, baseEmission);
    }

    public void BlinkGreen()
    {
        SetColor(blinkGreenColor, blinkEmission);
    }

    void SetColor(Color c, float emissionPower)
    {
        if (!targetRenderer) return;

        targetRenderer.GetPropertyBlock(mpb);

        Color em = c * emissionPower;
        if (targetRenderer.sharedMaterial != null && targetRenderer.sharedMaterial.HasProperty(EmissionColorID))
            mpb.SetColor(EmissionColorID, em);

        if (targetRenderer.sharedMaterial != null && targetRenderer.sharedMaterial.HasProperty(BaseColorID))
            mpb.SetColor(BaseColorID, c);
        else
            mpb.SetColor(ColorID, c);

        targetRenderer.SetPropertyBlock(mpb);
    }
}

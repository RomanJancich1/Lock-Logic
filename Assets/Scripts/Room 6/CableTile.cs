using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CableTile : MonoBehaviour
{

    [Header("Tile Definition")]
    [Tooltip("Base connection mask at rotation = 0° (N=1,E=2,S=4,W=8).")]
    public int BaseMask = 0;

    public bool IsSource = false;
    public bool IsTarget = false;

    [Tooltip("If true, this tile cannot be rotated.")]
    public bool LockRotation = false;

    [Header("Visuals")]
    public Renderer targetRenderer;

    [Tooltip("Color when tile is correctly powered")]
    public Color poweredColor = Color.green;

    [Tooltip("Color when tile is wrong")]
    public Color wrongColor = Color.red;

    [Tooltip("Base color when tile is unpowered (dim but visible)")]
    public Color unpoweredBaseColor = new Color(0.18f, 0.18f, 0.18f, 1f);

    [Tooltip("Emission intensity for powered/wrong")]
    public float emissionIntensity = 2f;

    [Tooltip("Emission intensity for unpowered (very weak)")]
    public float unpoweredEmissionIntensity = 0.2f;

    [Header("Connectors (optional)")]
    public GameObject connN;
    public GameObject connE;
    public GameObject connS;
    public GameObject connW;

    [HideInInspector] public int gx;
    [HideInInspector] public int gy;

    int rotationSteps = 0;

    static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");
    static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");
    static readonly int ColorID = Shader.PropertyToID("_Color");

    MaterialPropertyBlock mpb;

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

        mpb = new MaterialPropertyBlock();

        UpdateConnectors();
        ShowUnpowered();
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
    }

    public void ResetRotationState()
    {
        rotationSteps = 0;
        UpdateConnectors();
    }

    int RotateMask90CW(int m)
    {
        bool n = (m & 1) != 0;
        bool e = (m & 2) != 0;
        bool s = (m & 4) != 0;
        bool w = (m & 8) != 0;

        int r = 0;
        if (w) r |= 1; // W -> N
        if (n) r |= 2; // N -> E
        if (e) r |= 4; // E -> S
        if (s) r |= 8; // S -> W
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


    public void ShowPowered()
    {
        SetColor(poweredColor, emissionIntensity);
    }

    public void ShowWrong()
    {
        SetColor(wrongColor, emissionIntensity);
    }

    public void ShowUnpowered()
    {
        SetColor(unpoweredBaseColor, unpoweredEmissionIntensity);
    }

    void SetColor(Color baseColor, float emissionPower)
    {
        if (!targetRenderer) return;
        if (mpb == null) mpb = new MaterialPropertyBlock();

        targetRenderer.GetPropertyBlock(mpb);

        Color em = baseColor * Mathf.Max(0f, emissionPower);
        if (targetRenderer.sharedMaterial != null &&
            targetRenderer.sharedMaterial.HasProperty(EmissionColorID))
        {
            mpb.SetColor(EmissionColorID, em);
        }

        if (targetRenderer.sharedMaterial != null &&
            targetRenderer.sharedMaterial.HasProperty(BaseColorID))
        {
            mpb.SetColor(BaseColorID, baseColor);
        }
        else
        {
            mpb.SetColor(ColorID, baseColor);
        }

        targetRenderer.SetPropertyBlock(mpb);
    }
}

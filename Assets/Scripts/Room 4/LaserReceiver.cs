using UnityEngine;
using UnityEngine.Events;

public class LaserReceiver : MonoBehaviour
{
    public Renderer indicator;          
    public Color idle = Color.gray;
    public Color active = Color.green;
    public float requireHitForSeconds = 0.05f; 

    public UnityEvent OnSolved;         

    bool hitThisFrame;
    bool solved;
    float hitAccum;

    static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");
    static readonly int ColorID = Shader.PropertyToID("_Color");

    void LateUpdate()
    {
        if (solved) return;

        if (hitThisFrame) hitAccum += Time.deltaTime;
        else hitAccum = 0f;

        SetColor(hitThisFrame ? active : idle);

        if (hitAccum >= requireHitForSeconds)
        {
            solved = true;
            OnSolved.Invoke();
        }

        hitThisFrame = false;
    }

    public void RegisterHit() => hitThisFrame = true;

    void SetColor(Color c)
    {
        if (!indicator) return;
        var mat = indicator.material;
        if (mat.HasProperty(BaseColorID)) mat.SetColor(BaseColorID, c);
        else if (mat.HasProperty(ColorID)) mat.SetColor(ColorID, c);
    }
}

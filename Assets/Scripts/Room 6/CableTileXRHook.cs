using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(CableTile))]
[RequireComponent(typeof(XRSimpleInteractable))]
public class CableTileXRClick : MonoBehaviour
{
    private CableTile tile;
    private XRSimpleInteractable xri;

    void Awake()
    {
        tile = GetComponent<CableTile>();
        xri = GetComponent<XRSimpleInteractable>();

        xri.selectEntered.AddListener(_ => tile.Use());
    }
}

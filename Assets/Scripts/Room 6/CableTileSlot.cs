using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor))]
public class CableTileSlot : MonoBehaviour
{
    [Header("Grid coords for this slot")]
    public int gx;
    public int gy;

    [Header("Optional: only accept the missing piece")]
    public string requiredPieceId = "MissingPiece";

    [Header("Inserted tile base mask")]
    [Tooltip("Pick one of {3,6,12,9} to match (0,2) required mask=3 by rotation.")]
    public int insertedBaseMask = 6;

    UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor socket;

    void Awake()
    {
        socket = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor>();
        socket.selectEntered.AddListener(OnInserted);
    }

    void OnDestroy()
    {
        if (socket)
            socket.selectEntered.RemoveListener(OnInserted);
    }

    void OnInserted(SelectEnterEventArgs args)
    {
        var tile = args.interactableObject.transform.GetComponentInParent<CableTile>();
        if (!tile) return;

        if (!string.IsNullOrEmpty(requiredPieceId))
        {
            var tag = args.interactableObject.transform.GetComponentInParent<CablePieceTag>();
            if (!tag || tag.pieceId != requiredPieceId)
            {
                socket.interactionManager.SelectExit(socket, args.interactableObject);
                return;
            }
        }

        tile.transform.position = transform.position;
        tile.transform.rotation = transform.rotation;

        tile.gx = gx;
        tile.gy = gy;

        tile.BaseMask = insertedBaseMask;

        tile.ResetRotationState();

        var grab = tile.GetComponent<XRGrabInteractable>();
        if (grab) grab.enabled = false;

        var rb = tile.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        var simple = tile.GetComponent<XRSimpleInteractable>();
        if (simple) simple.enabled = true;

        var click = tile.GetComponent<CableTileXRClick>();
        if (click) click.enabled = true;

        socket.enabled = false;

        CableGridManager.Instance?.RegisterInsertedTile(tile, gx, gy);
    }
}

using UnityEngine;

[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
public class NumberTile : MonoBehaviour
{
    [Tooltip("Hodnota kocky")]
    public int Value = 1;

    public int GetValue() => Value;
}

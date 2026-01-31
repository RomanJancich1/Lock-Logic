using UnityEngine;
using UnityEngine.EventSystems;

public class OpenKeyboardOnPointerDown : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private VRNameKeyboard keyboard;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (keyboard != null) keyboard.Show();
    }
}

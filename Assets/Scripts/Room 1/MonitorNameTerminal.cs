using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class MonitorNameTerminal : MonoBehaviour
{
    [SerializeField] private VRNameKeyboard keyboard;

    public void OnSelected(SelectEnterEventArgs args)
    {
        if (keyboard != null) keyboard.Show();
    }
}

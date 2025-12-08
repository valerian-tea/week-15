using UnityEngine;

public class ToggleUIElement : MonoBehaviour
{
    public void ToggleActive(bool isActive)
    {
        this.gameObject.SetActive(isActive);
        Debug.Log("Toggled UI Element to " + isActive);
    }
}

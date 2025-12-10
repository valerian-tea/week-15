using UnityEngine;

public class ToggleUIElement : MonoBehaviour
{
    public void ToggleActive(bool isActive)
    {
        this.gameObject.SetActive(isActive);
    }
}

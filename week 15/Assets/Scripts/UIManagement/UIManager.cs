using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject[] panels;
    public static bool isPaused { get; private set; }

    private GameObject GetPanelByName(string panelName)
    {
        foreach (var panel in panels)
        {
            if (panel != null && panel.name == panelName)
                return panel;
        }
        return null;
    }

    public void ToggleInventory()
    {
        var inventoryPanel = GetPanelByName("InventoryPanel");
        if (inventoryPanel != null)
        {
            CanvasGroup cg = inventoryPanel.GetComponent<CanvasGroup>();
            bool isInventoryVisible = cg.alpha > 0.5f;

            if (isInventoryVisible) // Hide inventory
            {
                cg.alpha = 0f;
                cg.interactable = false;
                cg.blocksRaycasts = false;
                isPaused = false;
            }
            else // Show inventory
            {
                cg.alpha = 1f;
                cg.interactable = true;
                cg.blocksRaycasts = true;
                isPaused = true;
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            ToggleInventory();
        }
    }
}

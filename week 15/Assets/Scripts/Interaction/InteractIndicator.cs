using InventorySystem;
using UnityEngine;

public class InteractIndicator : MonoBehaviour
{
    public GameObject interactText;
    public AddItem addItemScript; // Assign in Inspector

    public ItemInitializer itemToAdd; // Assign in Inspector
    public InventoryInitializer inventory; // Assign in Inspector

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        interactText.SetActive(false);
        // ItemOnPlayer.SetActive(false);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            interactText.SetActive(true);

            if (Input.GetKey(KeyCode.X))
            {
                gameObject.SetActive(false);
                interactText.SetActive(false);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            interactText.SetActive(false);
        }
    }
}

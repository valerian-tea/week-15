using UnityEngine;
using InventorySystem;

public class PickUpItem : MonoBehaviour
{
    // public GameObject ItemOnPlayer;
    public GameObject pickupText;
    public AddItem addItemScript; // Assign in Inspector

    public ItemInitializer itemToAdd; // Assign in Inspector
    public InventoryInitializer inventory; // Assign in Inspector

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pickupText.SetActive(false);
        // ItemOnPlayer.SetActive(false);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            pickupText.SetActive(true);

            if (Input.GetKey(KeyCode.E))
            {
                // Set up the AddItem script with the correct item/inventory
                addItemScript.SetItem(itemToAdd);
                addItemScript.SetInventory(inventory);

                // Trigger the add logic
                addItemScript.gameObject.SetActive(true);

                gameObject.SetActive(false);
                pickupText.SetActive(false);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            pickupText.SetActive(false);
        }
    }
}

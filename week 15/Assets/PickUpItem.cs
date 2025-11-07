using UnityEngine;

public class PickUpItem : MonoBehaviour
{
    // public GameObject ItemOnPlayer;
    public GameObject pickupText;
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
                gameObject.SetActive(false);
                // ItemOnPlayer.SetActive(true);
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

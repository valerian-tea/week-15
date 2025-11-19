using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTrigger : MonoBehaviour
{
    [SerializeField]
    string sceneToLoad;

    public GameObject sceneChangeText;

    void Start()
    {
        sceneChangeText.SetActive(false);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            sceneChangeText.SetActive(true);

            if (Input.GetKey(KeyCode.Space))
            {
                Debug.Log("Player reached destination, loading scene: " + sceneToLoad);
                SceneManager.LoadScene(sceneToLoad);
                sceneChangeText.SetActive(false);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            sceneChangeText.SetActive(false);
        }
    }
}

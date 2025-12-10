using UnityEngine;

public class SceneTrigger : MonoBehaviour
{
    [SerializeField]
    string sceneToLoad;

    public GameObject sceneChangeText;

    void Start()
    {
        if (sceneChangeText != null)
        {
            sceneChangeText.SetActive(false);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        Debug.Log("ONTRIGGERSTAY CALLED in" + sceneToLoad);
        if (SceneLoader.Instance != null && SceneLoader.Instance.isChangingScene)
        {
            Debug.Log("scene loader is changing scene, aborting" + sceneToLoad);
            return;
        }
        Debug.Log("ONTRIGGERSTAY here1 " + sceneToLoad);

        if (other.CompareTag("Player"))
        {
            Debug.Log("player entered trigger");

            if (sceneChangeText != null)
            {
                sceneChangeText.SetActive(true);
                Debug.Log("text set active: " + sceneChangeText);
            }

            if (Input.GetKey(KeyCode.Space))
            {
                SceneLoader.Instance.LoadScene(sceneToLoad);
                Debug.Log(
                    "found scene loader: "
                        + SceneLoader.Instance
                        + ", loading scene: "
                        + sceneToLoad
                );
                if (sceneChangeText != null)
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

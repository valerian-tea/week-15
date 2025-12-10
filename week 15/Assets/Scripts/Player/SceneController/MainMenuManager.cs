using UnityEngine;
using UnityEngine.SceneManagement; // Required for scene management

public class MainMenuManager : MonoBehaviour
{
    [SerializeField]
    string sceneToLoad;

    public void StartGame()
    {
        SceneManager.LoadScene(sceneToLoad);
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }
}

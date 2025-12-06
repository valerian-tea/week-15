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
        Application.Quit(); // Exits the application
    }

    // Add other functions for options, etc. as needed
}
